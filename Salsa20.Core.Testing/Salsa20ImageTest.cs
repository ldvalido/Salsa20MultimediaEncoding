using System.IO;
using System.Text;
using NUnit.Framework;

namespace Salsa20.Core.Testing
{
    [TestFixture]
    public class Salsa20ImageTest
    {
        private byte[] key;
        private byte[] iv;
        private int rounds;
        private string sourceFile;
        private string destinyFile;
        private bool overwrite;

        [SetUp]
        public void SetupTest()
        {
            const string phrase = "12345678";
            var phraseToKey = string.Empty;
            for (var r = 0; r < 4; r++)
            {
                phraseToKey = string.Concat(phraseToKey,phrase);
            }
            key = Encoding.UTF8.GetBytes(phraseToKey);

            iv = new byte[8];
            for (var i = 0; i < iv.Length; i++)
            {
                iv[i] = byte.Parse(i.ToString());
            }

            rounds = 12;

        }

        [Test]
        public void TestEncryptFile()
        {
            sourceFile = @"C:\projects\Salsa20MultimediaEncoding\Salsa20.Stream.Console\Image\Example1.png";
            destinyFile = @"C:\projects\Salsa20MultimediaEncoding\Salsa20.Stream.Console\Image\Example1Encrypted.png";
            var salsa20Cryptor = new Salsa20
            {
                Key = key,
                IV = iv,
                Rounds = rounds
            };
            var file = File.ReadAllBytes(sourceFile);
            var finalEncryption = salsa20Cryptor.CreateEncryptor().TransformFinalBlock(file, 0, file.Length);
            File.WriteAllBytes(destinyFile, finalEncryption);

        }

        [Test]
        public void TestDecryptFile()
        {
            destinyFile = @"C:\projects\Salsa20MultimediaEncoding\Salsa20.Stream.Console\Image\Example1Decrypted.png";
            sourceFile = @"C:\projects\Salsa20MultimediaEncoding\Salsa20.Stream.Console\Image\Example1Encrypted.png";

            var salsa20Cryptor = new Salsa20
            {
                Key = key,
                IV = iv,
                Rounds = rounds
            };
            var file = File.ReadAllBytes(sourceFile);
            var finalEncryption = salsa20Cryptor.CreateDecryptor().TransformFinalBlock(file, 0, file.Length);
            File.WriteAllBytes(destinyFile, finalEncryption);
        }
    }
}
