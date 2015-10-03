namespace Salsa20.Stream.Console
{
    class Program
    {
        static void Main(string[] args)
        {   
            var server = new StreamServer();
            while (true)
            {
                server.Start();
            }
            
        }
    }
}
