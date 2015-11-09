using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salsa20.Stream.Console.Commands
{
    internal class Operation
    {
       internal string OperationType { get; set; }
       internal bool Overwrite { get; set; }
       internal string SourceFile { get; set; }
       internal string TargetFile { get; set; }
       internal byte[] Key { get; set; }
       internal byte[] IV { get; set; }
       internal int Rounds { get; set; }
    }
}
