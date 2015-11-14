using System.IO;
using System.Linq;
using Salsa20.Stream.Console.Commands;

namespace Salsa20.Stream.Console.ImageFormat
{
    internal class BmpImageFormat : IImageFormat
    {
        private const int HeaderSize = 54;
        public bool CanProcess(Operation operation)
        {
            var returnValue = new FileInfo(operation.SourceFile).Extension.ToLowerInvariant() == ".bmp";
            return returnValue;
        }

        public void Encrypt(Operation operation)
        {
            var encryptor = operation.SymmetricAlgorithm;

            var sourceFile = File.ReadAllBytes(operation.SourceFile);

            var header = sourceFile.Take(HeaderSize);

            var body = sourceFile.Skip(HeaderSize).Take(sourceFile.Length - HeaderSize).ToArray();

            var cryptoTransform = encryptor.CreateEncryptor(encryptor.Key, encryptor.IV);
            
            //transform the specified region of bytes array to resultArray
            var resultArray = cryptoTransform.TransformFinalBlock(body, 0, body.Length);
            
            File.WriteAllBytes(operation.TargetFile, header.Concat(resultArray).ToArray());
        }

        public void Decrypt(Operation operation)
        {
            var encryptor = operation.SymmetricAlgorithm;
            
            var sourceFile = File.ReadAllBytes(operation.SourceFile);

            var header = sourceFile.Take(HeaderSize);

            var body = sourceFile.Skip(HeaderSize).Take(sourceFile.Length - HeaderSize).ToArray();

            var cryptoTransform = encryptor.CreateDecryptor(encryptor.Key, encryptor.IV);

            var resultArray = cryptoTransform.TransformFinalBlock(body, 0, body.Length);

            File.WriteAllBytes(operation.TargetFile, header.Concat(resultArray).ToArray());
        }
    }
}
