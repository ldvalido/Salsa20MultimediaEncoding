using CommandLine;
using CommandLine.Text;

namespace Salsa20.Stream.Console.Commands
{
    public class CommandOptions
    {
        [Option('t',"Type",Required = true,HelpText = "Type of Operation")]
        public string Operation { get; set; }

        [Option('o', "OriginFile", Required= true, HelpText = "Select source File")]
        public string SourceFile { get; set; }

        [Option('d',"TargetFile", Required = true, HelpText = "Select target File")]
        public string TargetFile { get; set; }

        [Option('y',"Overwrite",
            Required = false,
            DefaultValue = false,
            HelpText = "Indicate if a file must be rewrriten when is encrypted")]
        public bool OverWrite { get; set; }

        [Option('r',"Rounds",Required=true,HelpText = "Indicate number of rounds to encrypt the password")]
        public int Rounds { get; set; }

        [Option('k', "Key", Required = false,
            HelpText =
                "Indicates the key to encrypt to process the file. Otherwise the key loaded in the configuration file will be used"
            )]
        public string Key { get; set; }

        [Option('k',"Key",Required=false,HelpText = "Indicates the vector to encrypt to process the file. Otherwise the key loaded in the configuration file will be used")]
        public string IV { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
