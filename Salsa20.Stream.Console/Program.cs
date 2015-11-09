using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
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
                var operation = new Operation();

                if (CommandLine.Parser.Default.ParseArguments(args, options))
                {
                    DoValidation(options);
                    

                    if (string.IsNullOrEmpty(options.Key)|| options.Key.Length != 32)
                    {
                        System.Console.WriteLine("A Random key will be used");
                    }
                    else
                    {
                        operation.Key = Encoding.UTF8.GetBytes(options.Key);
                    }

                    if (string.IsNullOrEmpty(options.IV) || options.IV.Length != 8)
                    {
                        System.Console.WriteLine("A random initialization vector will be used");
                    }
                    else
                    {
                        operation.IV = Encoding.UTF8.GetBytes(options.IV);
                    }
                    
                    operation.SourceFile = options.SourceFile;
                    operation.TargetFile = options.TargetFile;
                    operation.Rounds = options.Rounds;
                    operation.Overwrite = options.OverWrite;
                    operation.OperationType = options.Operation;

                    System.Console.WriteLine($"Source File: {operation.SourceFile}");
                    System.Console.WriteLine($"Target File: {operation.TargetFile}");
                    System.Console.WriteLine($"Number of Round: {operation.Rounds}");
                    System.Console.WriteLine($"Overwritting: {operation.Overwrite}");
                    System.Console.WriteLine($"Operation Type: {operation.OperationType}");
                    System.Console.WriteLine($"Key Used: {Encoding.UTF8.GetString(operation.Key)}");
                    System.Console.WriteLine($"Vector Used: {Encoding.UTF8.GetString(operation.IV)}");
                    DoOperation(operation);
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        static void DoOperation(Operation operation)
        {
            if (operation.Overwrite)
            {
                File.Delete(operation.TargetFile);
            }

            var encryptor = new Core.Salsa20();
            if (operation.IV == null)
            {
                encryptor.GenerateIV();
            }
            else
            {
                encryptor.IV = operation.IV;
            }

            if (operation.Key == null)
            {
                encryptor.GenerateKey();
            }
            else
            {
                encryptor.Key = operation.Key;
            }

            ICryptoTransform cryptoTransform = null;

            var dictOperations = new Dictionary<string, Action>
                    {
                        {"encrypt", () => cryptoTransform = encryptor.CreateEncryptor(encryptor.Key,encryptor.IV)},
                        {"decrypt", () => cryptoTransform = encryptor.CreateDecryptor(encryptor.Key,encryptor.IV)},
                    };
            
            dictOperations[operation.OperationType.ToLowerInvariant()].Invoke();

            var sourceFile = File.ReadAllBytes(operation.SourceFile);

            //transform the specified region of bytes array to resultArray
            var resultArray = cryptoTransform.TransformFinalBlock(sourceFile, 0, sourceFile.Length);

            //Release resources held by TripleDes Encryptor
            File.WriteAllBytes(operation.TargetFile, resultArray);

            System.Console.WriteLine("Operation Completed Succesfully");
            
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
