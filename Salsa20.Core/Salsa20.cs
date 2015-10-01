/*
 * This implementation of Salsa20 is ported from the reference implementation
 * by D. J. Bernstein, which can be found at:
 *   http://cr.yp.to/snuffle/salsa20/ref/salsa20.c
 *
 * This work is hereby released into the Public Domain. To view a copy of the public domain dedication,
 * visit http://creativecommons.org/licenses/publicdomain/ or send a letter to
 * Creative Commons, 171 Second Street, Suite 300, San Francisco, California, 94105, USA.
 */

using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace Salsa20.Core
{
    /// <summary>
    /// Implements the Salsa20 stream encryption cipher, as defined at http://cr.yp.to/snuffle.html.
    /// </summary>
    /// <remarks>See <a href="http://code.logos.com/blog/2008/06/salsa20_implementation_in_c_1.html">Salsa20 Implementation in C#</a>.</remarks>
    public sealed class Salsa20 : SymmetricAlgorithm
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Salsa20"/> class.
        /// </summary>
        /// <exception cref="CryptographicException">The implementation of the class derived from the symmetric algorithm is not valid.</exception>
        public Salsa20()
        {
            // set legal values
            LegalBlockSizesValue = new[] {new KeySizes(512, 512, 0)};
            LegalKeySizesValue = new[] {new KeySizes(128, 256, 128)};

            // set default values
            BlockSizeValue = 512;
            KeySizeValue = 256;
            m_rounds = 20;
        }

        /// <summary>
        /// Creates a symmetric decryptor object with the specified <see cref="SymmetricAlgorithm.Key"/> property
        /// and initialization vector (<see cref="SymmetricAlgorithm.IV"/>).
        /// </summary>
        /// <param name="rgbKey">The secret key to use for the symmetric algorithm.</param>
        /// <param name="rgbIV">The initialization vector to use for the symmetric algorithm.</param>
        /// <returns>A symmetric decryptor object.</returns>
        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
        {
            // decryption and encryption are symmetrical
            return CreateEncryptor(rgbKey, rgbIV);
        }

        /// <summary>
        /// Creates a symmetric encryptor object with the specified <see cref="SymmetricAlgorithm.Key"/> property
        /// and initialization vector (<see cref="SymmetricAlgorithm.IV"/>).
        /// </summary>
        /// <param name="rgbKey">The secret key to use for the symmetric algorithm.</param>
        /// <param name="rgbIV">The initialization vector to use for the symmetric algorithm.</param>
        /// <returns>A symmetric encryptor object.</returns>
        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
        {
            if (rgbKey == null)
                throw new ArgumentNullException("rgbKey");
            if (!ValidKeySize(rgbKey.Length*8))
                throw new CryptographicException("Invalid key size; it must be 128 or 256 bits.");
            CheckValidIV(rgbIV, "rgbIV");

            return new Salsa20CryptoTransform(rgbKey, rgbIV, m_rounds);
        }

        /// <summary>
        /// Generates a random initialization vector (<see cref="SymmetricAlgorithm.IV"/>) to use for the algorithm.
        /// </summary>
        public override void GenerateIV()
        {
            // generate a random 8-byte IV
            IVValue = GetRandomBytes(8);
        }

        /// <summary>
        /// Generates a random key (<see cref="SymmetricAlgorithm.Key"/>) to use for the algorithm.
        /// </summary>
        public override void GenerateKey()
        {
            // generate a random key
            KeyValue = GetRandomBytes(KeySize/8);
        }

        /// <summary>
        /// Gets or sets the initialization vector (<see cref="SymmetricAlgorithm.IV"/>) for the symmetric algorithm.
        /// </summary>
        /// <value>The initialization vector.</value>
        /// <exception cref="ArgumentNullException">An attempt was made to set the initialization vector to null. </exception>
        /// <exception cref="CryptographicException">An attempt was made to set the initialization vector to an invalid size. </exception>
        public override byte[] IV
        {
            get { return base.IV; }
            set
            {
                CheckValidIV(value, "value");
                IVValue = (byte[]) value.Clone();
            }
        }

        /// <summary>
        /// Gets or sets the number of rounds used by the Salsa20 algorithm.
        /// </summary>
        /// <value>The number of rounds.</value>
        public int Rounds
        {
            get { return m_rounds; }
            set
            {
                if (value != 8 && value != 12 && value != 20)
                    throw new ArgumentOutOfRangeException("value", "The number of rounds must be 8, 12, or 20.");
                m_rounds = value;
            }
        }

        // Verifies that iv is a legal value for a Salsa20 IV.
        private static void CheckValidIV(byte[] iv, string paramName)
        {
            if (iv == null)
                throw new ArgumentNullException(paramName);
            if (iv.Length != 8)
                throw new CryptographicException("Invalid IV size; it must be 8 bytes.");
        }

        // Returns a new byte array containing the specified number of random bytes.
        private static byte[] GetRandomBytes(int byteCount)
        {
            byte[] bytes = new byte[byteCount];
            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
                rng.GetBytes(bytes);
            return bytes;
        }

        private int m_rounds;
    }
}