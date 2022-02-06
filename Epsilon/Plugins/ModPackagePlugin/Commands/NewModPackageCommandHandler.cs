using CacheEditor;
using EpsilonLib.Commands;
using Shared;
using Stylet;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using TagTool.Cache;
using TagTool.Cache.Gen3;
using TagTool.Cache.HaloOnline;
using TagTool.IO;

namespace ModPackagePlugin.Commands
{
    [ExportCommandHandler]
    class NewModPackageCommandHandler : ICommandHandler<NewModPackageCommand>
    {
        private readonly Lazy<IShell> _shell;
        private readonly ICacheEditingService _editingService;
        private readonly IFileHistoryService _fileHistory;

        [ImportingConstructor]
        public NewModPackageCommandHandler(Lazy<IShell> shell, ICacheEditingService editingService, IFileHistoryService fileHistory)
        {
            _shell = shell;
            _editingService = editingService;
            _fileHistory = fileHistory;
        }

        public async void ExecuteCommand(Command command)
        {
            var shell = _shell.Value;

            if (!FileDialogs.RunBrowseCacheDialog(out FileInfo baseCacheFile))
                return;

            var directory = Path.GetFullPath($"{baseCacheFile.Directory.FullName}\\..\\mods\\downloads");
            if (!FileDialogs.RunSaveDialog(out FileInfo modpackageFile, initialDirectory: directory))
                return;

            using (var progress = shell.CreateProgressScope())
            {
                progress.Report("Loading base cache file...");

                var baseCache = (GameCacheHaloOnline)GameCache.Open(baseCacheFile);
                var cache = await CreateModPackageCacheAsync(baseCache, modpackageFile, progress);

                shell.ActiveDocument = (IScreen)_editingService.CreateEditor(new ModPackageCacheFile(modpackageFile, cache));
            }
        }

        private async Task<GameCacheModPackage> CreateModPackageCacheAsync(GameCacheHaloOnline baseCache, FileInfo file, IProgressReporter progress)
        {
            GameCacheModPackage modCache = await Task.Run(() => CreateAndInitializePackage(baseCache, progress));

            progress.Report("Creating package file...");
            await Task.Run(() => modCache.SaveModPackage(file));

            await _fileHistory.RecordFileOpened(ModPackageEditorProvider.EditorId, file.FullName);

            return modCache;
        }

        private static GameCacheModPackage CreateAndInitializePackage(GameCacheHaloOnline baseCache, IProgressReporter progress)
        {
            var modCache = new GameCacheModPackage(baseCache);

            modCache.BaseModPackage.CacheNames = new List<string>();
            modCache.BaseModPackage.TagCachesStreams = new List<ExtantStream>();
            modCache.BaseModPackage.TagCacheNames = new List<Dictionary<int, string>>();

            var referenceStream = new MemoryStream(); // will be reused by all base caches
            var modTagCache = new TagCacheHaloOnline(baseCache.Version, referenceStream, modCache.BaseModPackage.StringTable);

            var tagCount = baseCache.TagCache.Count;
            for (var tagIndex = 0; tagIndex < tagCount; tagIndex++)
            {
                var srcTag = baseCache.TagCache.GetTag(tagIndex);

                if (srcTag == null)
                {
                    modTagCache.AllocateTag(new TagGroupGen3());
                    continue;
                }

                progress.Report($"Allocating tag ({tagIndex}/{tagCount}). '{srcTag}'...", false, (tagIndex + 1) / (float)tagCount);

                var emptyTag = modTagCache.AllocateTag(srcTag.Group, srcTag.Name);
                var cachedTagData = new CachedTagData
                {
                    Data = new byte[0],
                    Group = (TagGroupGen3)emptyTag.Group
                };

                // TODO: defer this process until when they actually save through File -> Save

                modTagCache.SetTagData(referenceStream, (CachedTagHaloOnline)emptyTag, cachedTagData);
                if (!((CachedTagHaloOnline)emptyTag).IsEmpty())
                {
                    throw new InvalidOperationException("A tag in the base cache was empty");
                }
            }

            referenceStream.Position = 0;

            Dictionary<int, string> tagNames = new Dictionary<int, string>();

            foreach (var tag in baseCache.TagCache.NonNull())
                tagNames[tag.Index] = tag.Name;

            modCache.BaseModPackage.TagCachesStreams.Add(new ExtantStream(referenceStream));
            modCache.BaseModPackage.CacheNames.Add("default");
            modCache.BaseModPackage.TagCacheNames.Add(tagNames);

            modCache.SetActiveTagCache(0);

            return modCache;
        }

        public void UpdateCommand(Command command)
        {

        }
    }
}
