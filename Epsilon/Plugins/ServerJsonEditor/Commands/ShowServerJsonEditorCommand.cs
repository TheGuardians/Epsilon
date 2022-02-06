using EpsilonLib.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerJsonEditor
{
    [ExportCommand]
    class ShowServerJsonEditorCommand : CommandDefinition
    {
        public override string Name => "ServerJsonEditor.Show";

        public override string DisplayText => "Edit Server Voting";
    }
}
