using EpsilonLib.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapVariantFixer
{
    [ExportCommand]
    class ShowMapVariantFixerCommand : CommandDefinition
    {
        public override string Name => "MapVariantFixer.Show";

        public override string DisplayText => "Update Map Variants";
    }
}
