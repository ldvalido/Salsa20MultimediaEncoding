using System;
using System.IO;
using System.Text;
using Salsa20.Stream.Console.Commands;

namespace Salsa20.Stream.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var options = new CommandOptions();
                
                if (CommandLine.Parser.Default.ParseArguments(args, options))
                {
                    DoValidation(options);
                    var operation = new Operation();

                    if (string.IsNullOrWhiteSpace(options.Key))
                    {
                        operation.Key = Encoding.UTF8.GetBytes(options.Key);
                    }

                    if (!string.IsNullOrWhiteSpace(options.IV))
                    {
                        operation.IV = Encoding.UTF8.GetBytes(options.IV); 
                    }

                    operation.SourceFile = options.SourceFile;
                    operation.TargetFile = options.TargetFile;
                    operation.Rounds = options.Rounds;
                    operation.Overwrite = operation.Overwrite;

                    Encrypt(operation);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
            System.Console.ReadLine();
        }

        static void Encrypt(Operation operation)
        {
            if (operation.Overwrite)
            {
                File.Delete(operation.TargetFile);
            }

            var encryptor = new Core.Salsa20();
            if (operation.IV.Length == 0)
            {
                encryptor.GenerateIV();
            }
            else
            {
                encryptor.IV = operation.IV;
            }

            if (operation.Key.Length == 0)
            {
                encryptor.GenerateKey();
            }
            else
            {
                encryptor.Key = operation.Key;
            }

            var cryptoTransform = encryptor.CreateDecryptor(encryptor.Key, encryptor.IV);

            var sourceFile = File.ReadAllBytes(operation.SourceFile);

            //transform the specified region of bytes array to resultArray
            var resultArray = cryptoTransform.TransformFinalBlock(sourceFile, 0, sourceFile.Length);

            //Release resources held by TripleDes Encryptor
            File.WriteAllBytes(operation.TargetFile, resultArray);
        }

        private static void DoValidation(CommandOptions options)
        {
            if (!File.Exists(options.SourceFile))
            {
                throw new ArgumentException(
                    $"{options.SourceFile} does not exists. Please verify the filename and try again");
            }

            if (File.Exists(options.TargetFile) && !options.OverWrite)
            {
                throw new ArgumentException(
                    $"{options.TargetFile} exists. Please verify the filename or set the overwrite setting when the command is executed");
            }

            if (string.Compare(options.Operation, "encrypt", StringComparison.OrdinalIgnoreCase) != 0 &&
                string.Compare(options.Operation, "decrypt", StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new ArgumentException($"Operations value {options.Operation} is not allowed. It should be 'encrypt' or 'decrypt'");
            }

            if (options.Rounds < 0)
            {
                throw new ArgumentException($"{options.Rounds} should be greather than 0");
            }
        }
    }
}
