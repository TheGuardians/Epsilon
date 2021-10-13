using CacheEditor;
using EpsilonLib.Editors;
using Microsoft.Win32;
using Shared;
using Stylet;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TagTool.Cache;

namespace ModPackagePlugin
{
    [Export(typeof(IEditorProvider))]
    class ModPackageEditorProvider : IEditorProvider
    {
        private readonly ICacheEditingService _editingService;

        public string DisplayName => "Mod Package";

        public static Guid EditorId = new Guid("{BECDF82F-CF23-4DBD-BE7A-A44DBF943BFB}");

        public Guid Id => EditorId;

        public IReadOnlyList<string> FileExtensions => new[] { ".pak" };

        [ImportingConstructor]
        public ModPackageEditorProvider(ICacheEditingService editingService)
        {
            _editingService = editingService;
        }

        public async Task OpenFileAsync(IShell shell, string fileName)
        {
            FileInfo file = new FileInfo(fileName);
            FileInfo baseCacheFile = new FileInfo(Path.Combine(file.Directory.FullName, "..\\..\\maps\\tags.dat"));
            if (!baseCacheFile.Exists)
            {
                if (!FileDialogs.RunBrowseCacheDialog(out baseCacheFile))
                    return;
            }

            if (file.Exists)
            {
                var cache = await Task.Run(() => new GameCacheModPackage((GameCacheHaloOnlineBase)GameCache.Open(baseCacheFile), new FileInfo(fileName)));
                shell.ActiveDocument = (IScreen)_editingService.CreateEditor(new ModPackageCacheFile(file, cache));
            }
            else
                MessageBox.Show($"Mod Package no longer exists at this location: \n\"{fileName}\"", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
