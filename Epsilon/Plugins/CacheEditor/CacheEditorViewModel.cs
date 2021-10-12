using CacheEditor.Components.TagExplorer.Commands;
using CacheEditor.Components.TagTree;
using CacheEditor.TagEditing.Messages;
using CacheEditor.ViewModels;
using EpsilonLib.Commands;
using EpsilonLib.Shell.TreeModels;
using Microsoft.Win32;
using Shared;
using Stylet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TagTool.Cache;

namespace CacheEditor
{
    class CacheEditorViewModel
        : Conductor<IScreen>.Collection.OneActive, 
        ICacheEditor,
        ICommandHandler<OpenTagCommand>,
        ICommandHandler<RenameTagCommand>,
        ICommandHandler<DuplicateTagCommand>,
        ICommandHandler<ExtractTagCommand>,
        ICommandHandler<ImportTagCommand>,
        ICommandHandler<DeleteTagCommand>
    {
        private readonly ICacheEditingService _cacheEditingService;
        private readonly IShell _shell;
        private readonly ICacheFile _cacheFile;
        public static int _counter;

        public int Counter => _counter;

        public CacheEditorViewModel(IShell shell, ICacheEditingService cacheEditingService, ICacheFile cacheFile)
        {
            _counter++;
            _shell = shell;
            CacheFile = cacheFile;
            _cacheEditingService = cacheEditingService;
            _cacheFile = cacheFile;
            cacheFile.Reloaded += CacheFile_Reloaded;

            DisplayName = _cacheFile.File.Name;
            TagTree = new TagTreeViewModel(_cacheEditingService, _cacheFile, new TagEditing.TagExtract(_shell));
            TagTree.ContextMenu = Components.TagExplorer.MenuDefinitions.ContextMenu;
            TagTree.NodeDoubleClicked += TagTree_ItemDoubleClicked;
            CloseCommand = new DelegateCommand(Close);
        }

        public ICacheFile CacheFile { get; }
        public IDictionary<string, object> PluginStorage { get; } = new Dictionary<string, object>();
        public IObservableCollection<IScreen> Documents => Items;
        public IObservableCollection<ICacheEditorTool> Tools { get; } = new BindableCollection<ICacheEditorTool>();
        public ICommand CloseCommand { get; }
        public TagTreeViewModel TagTree { get; }
        public event EventHandler CurrentTagChanged;
        public CachedTag CurrentTag => (ActiveItem as TagEditorViewModel)?.Tag;

        public object _activeLayoutItem;

        public object ActiveLayoutItem
        {
            get => _activeLayoutItem;
            set
            {
                if (ReferenceEquals(_activeLayoutItem, value))
                    return;

                _activeLayoutItem = value;

                if (!(value is ICacheEditorTool))
                    ActivateItem((IScreen)value);

                if(value is TagEditorViewModel)
                    CurrentTagChanged?.Invoke(this, EventArgs.Empty);

                NotifyOfPropertyChange();
            }
        }

        public override void ActivateItem(IScreen item)
        {
            if (item != null && item.ScreenState == ScreenState.Closed)
                return;

            ActiveLayoutItem = item;

            base.ActivateItem(item);
        }

        private void TagTree_ItemDoubleClicked(object sender, TreeNodeEventArgs e)
        {
            if (e.Node.Tag is CachedTag instance)
                OpenTag(instance);
        }

        private void OpenTag(CachedTag instance)
        {
            using (var progress = _shell.CreateProgressScope())
            {
                progress.Report($"Deserializing Tag '{instance}'...");
                var futureDefinitionData = Task.Run(() =>
                {
                    using (var stream = CacheFile.Cache.OpenCacheRead())
                        return CacheFile.Cache.Deserialize(stream, instance);
                });

                var context = new TagEditorContext()
                {
                    CacheEditor = this,
                    DefinitionData = futureDefinitionData,
                    Instance = instance
                };

                ActiveItem = new TagEditorViewModel(_cacheEditingService, context);
            }
        }

        protected override void OnInitialActivate()
        {
            base.OnInitialActivate();
            CreateAndShowTools();
        }

        private void CreateAndShowTools()
        {
            foreach (var toolFactory in _cacheEditingService.Tools)
            {
                if (!toolFactory.ValidForEditor(this))
                    continue;

                var tool = toolFactory.CreateTool(this);
                Tools.Add(tool);

                if (tool.Name == "CacheEditor.TagExplorer")
                    ShowTool(tool, true);
                else if (tool.IsVisible)
                    ShowTool(tool, false);
            }
        }

        private void CacheFile_Reloaded(object sender, System.EventArgs e)
        {
            Documents.Clear();
            TagTree.Refresh();
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            _cacheEditingService.ActiveCacheEditor = this;
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            _cacheEditingService.ActiveCacheEditor = null;
        }

        protected override void OnClose()
        {
            base.OnClose();
            TagTree.NodeDoubleClicked -= TagTree_ItemDoubleClicked;
            CloseAllPanes();
        }

        private void CloseAllPanes()
        {
            foreach (var tool in Tools)
                tool.Close();
            Tools.Clear();

            foreach (var document in Documents)
                document.Close();
            Documents.Clear();
        }

        public void Close()
        {
            RequestClose();
        }

        private void ShowTool(ICacheEditorTool tool, bool activate = false)
        {
            if (!Tools.Contains(tool))
                Tools.Add(tool);

            tool.Show(true, activate);
        }

        public ICacheEditorTool GetTool(string name)
        {
            return Tools.FirstOrDefault(x => x.Name == name);
        }

        #region ICacheEditor Members

        ITagTree ICacheEditor.TagTree => TagTree;

        CachedTag ICacheEditor.RunBrowseTagDialog()
        {
            var vm = new BrowseTagDialogViewModel(_cacheEditingService, _cacheFile);
            if (_shell.ShowDialog(vm) == true)
            {
                return vm.TagTree.SelectedNode.Tag as CachedTag;
            }

            return null;
        }

        void ICacheEditor.Reload()
        {
            TagTree.Refresh();
            Documents.Clear();
        }


        #endregion

        #region Command Handlers

        void ICommandHandler<OpenTagCommand>.UpdateCommand(Command command)
        {
            command.IsVisible = IsActive && TagTree?.SelectedNode?.Tag is CachedTag;
        }

        void ICommandHandler<OpenTagCommand>.ExecuteCommand(Command command)
        {
            if (TagTree.SelectedNode?.Tag is CachedTag instance)
                OpenTag(instance);
        }

        void ICacheEditor.OpenTag(CachedTag tag)
        {
            OpenTag(tag);
        }


        void ICommandHandler<RenameTagCommand>.UpdateCommand(Command command)
        {
            command.IsVisible = _cacheFile.CanRenameTag && TagTree.SelectedNode?.Tag is CachedTag;
        }

        void ICommandHandler<RenameTagCommand>.ExecuteCommand(Command command)
        {
            if (TagTree.SelectedNode?.Tag is CachedTag tag)
            {
                var vm = new RenameTagDialogViewModel();
                vm.DisplayName = "Rename Tag";
                vm.Message = "Enter the new name for the tag";
                vm.Name = tag.Name;

                if (_shell.ShowDialog(vm) == true)
                {
                    _cacheFile.RenameTag(tag, vm.Name);
                    TagTree.Refresh();
                }
            }
        }

        void ICommandHandler<ExtractTagCommand>.UpdateCommand(Command command)
        {
            command.IsVisible = _cacheFile.CanExtractTag && TagTree.SelectedNode?.Tag is CachedTag;
        }

        void ICommandHandler<ExtractTagCommand>.ExecuteCommand(Command command)
        {
            if (TagTree.SelectedNode?.Tag is CachedTag tag)
            {
                var ofd = new SaveFileDialog();
                ofd.FileName = $"{Path.GetFileName(tag.Name)}.{tag.Group}";
                if(ofd.ShowDialog() == true)
                {
                    _cacheFile.ExtractTag(tag, ofd.FileName);
                    MessageBox.Show("Tag extracted successfully", "Tag Extracted", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        void ICommandHandler<ImportTagCommand>.UpdateCommand(Command command)
        {
            command.IsVisible = _cacheFile.CanImportTag && TagTree.SelectedNode?.Tag is CachedTag;
        }

        void ICommandHandler<ImportTagCommand>.ExecuteCommand(Command command)
        {
            if (TagTree.SelectedNode?.Tag is CachedTag tag)
            {
                var ofd = new OpenFileDialog();
                ofd.FileName = $"{Path.GetFileName(tag.Name)}.{tag.Group}";
                if (ofd.ShowDialog() == true)
                {
                    _cacheFile.ImportTag(tag, ofd.FileName);
                    MessageBox.Show("Tag imported successfully", "Tag Imported", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        void ICommandHandler<DuplicateTagCommand>.UpdateCommand(Command command)
        {
            command.IsVisible = _cacheFile.CanDuplicateTag && TagTree.SelectedNode?.Tag is CachedTag;
        }

        void ICommandHandler<DuplicateTagCommand>.ExecuteCommand(Command command)
        {
            if (TagTree.SelectedNode?.Tag is CachedTag tag)
            {
                var vm = new RenameTagDialogViewModel();
                vm.DisplayName = "Duplicate Tag";
                vm.Message = "Enter the name for the new tag";
                vm.Name = tag.Name;

                if (_shell.ShowDialog(vm) == true)
                {
                    _cacheFile.DuplicateTag(tag, vm.Name);
                    TagTree.Refresh();
                }
            }
        }

        void ICommandHandler<DeleteTagCommand>.UpdateCommand(Command command)
        {
            command.IsVisible = _cacheFile.CanDeleteTag && TagTree.SelectedNode?.Tag is CachedTag;
        }

        void ICommandHandler<DeleteTagCommand>.ExecuteCommand(Command command)
        {
            if (TagTree.SelectedNode?.Tag is CachedTag tag)
            {
                var result = MessageBox.Show(
                    $"Deleting tag '{Path.GetFileName(tag.Name)}.{tag.Group}'.\n\nClick OK to continue.",
                    "Warning", MessageBoxButton.OKCancel,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.OK)
                {
                    _cacheFile.DeleteTag(tag);
                    TagTree.Refresh();
                }
            }
        }

        public void ReloadCurrentTag()
        {
            if(ActiveItem is ITagEditorPluginClient tagEditor)
            {
                using (var stream = CacheFile.Cache.OpenCacheRead())
                {
                    var data = CacheFile.Cache.Deserialize(stream, CurrentTag);
                    tagEditor.PostMessage(this, new DefinitionDataChangedEvent(data));
                }
            }
        }

        #endregion
    }
}
