using Salsa20.Stream.Console.Commands;

namespace Salsa20.Stream.Console.ImageFormat
{
    internal interface IImageFormat
    {
        bool CanProcess(Operation operation);
        void Encrypt(Operation operation);
        void Decrypt(Operation operation);
    }
}
