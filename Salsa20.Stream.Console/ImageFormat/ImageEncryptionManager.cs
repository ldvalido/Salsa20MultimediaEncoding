using System.Collections.Generic;
using System.Linq;
using Salsa20.Stream.Console.Commands;

namespace Salsa20.Stream.Console.ImageFormat
{
    internal class ImageEncryptionManager : IImageFormat
    {
        #region Properties

        private readonly List<IImageFormat> _encryptors; 
        #endregion
        #region public C...tor
        public ImageEncryptionManager()
        {
            _encryptors = new List<IImageFormat>()
            {
                new BmpImageFormat()
            };
        }
        #endregion
        #region IImageFormat
        public bool CanProcess(Operation operation)
        {
            return _encryptors.Any(enc => enc.CanProcess(operation));
        }

        public void Encrypt(Operation operation)
        {
            var encHandler = _encryptors.Find(enc => enc.CanProcess(operation));
            encHandler.Encrypt(operation);
        }

        public void Decrypt(Operation operation)
        {
            var encHandler = _encryptors.Find(enc => enc.CanProcess(operation));
            encHandler.Decrypt(operation);
        }
        #endregion
    }
}
