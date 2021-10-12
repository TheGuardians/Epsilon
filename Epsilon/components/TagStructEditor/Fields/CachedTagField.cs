using EpsilonLib.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TagStructEditor.Common;
using TagTool.Cache;

namespace TagStructEditor.Fields
{
    public class CachedTagField : ValueField
    {
        private readonly Func<CachedTag> _browseTagCallback;

        public CachedTagField(
            ValueFieldInfo info, 
            TagList tagList, 
            Action<CachedTag> openTagCallback, 
            Func<CachedTag> browseTagCallback) : base(info)
        {
            _browseTagCallback = browseTagCallback;

            Groups = tagList.Groups;

            GotoCommand = new DelegateCommand(() => openTagCallback(SelectedInstance.Instance), () => SelectedInstance != null);
            NullCommand = new DelegateCommand(Null, () => SelectedGroup != null);
            BrowseCommand = new DelegateCommand(BrowseTag);
            CopyTagNameCommand = new DelegateCommand(CopyTagName, () => SelectedInstance != null);
            CopyTagIndexCommand = new DelegateCommand(CopyTagIndex, () => SelectedInstance != null);
            PasteTagNameCommand = new DelegateCommand(PasteTagName);
        }

        public IEnumerable<TagGroupItem> Groups { get; set; }
        public TagGroupItem SelectedGroup { get; set; }
        public TagInstanceItem SelectedInstance { get; set; }

        public string SelectedInstanceName { get; set; }

        public string SelectedGroupName { get; set; }

        public bool SelectedGroupValid => SelectedGroup != null;
        public bool SelectedInstanceValid => SelectedInstance != null;

        public DelegateCommand GotoCommand { get; }
        public DelegateCommand NullCommand { get; }
        public DelegateCommand BrowseCommand { get; }
        public DelegateCommand CopyTagNameCommand { get; }
        public DelegateCommand CopyTagIndexCommand { get; }
        public DelegateCommand PasteTagNameCommand { get; }

        public override void Accept(IFieldVisitor visitor) => visitor.Visit(this);

        protected override void OnPopulate(object value)
        {
            var instance = (CachedTag)value;
            if (instance != null)
            {
                SelectCachedTag(instance);
            }
            else
            {
                SelectedGroup = null;
                SelectedInstance = null;
            }

            InvalidateCommands();
        }

        private void SelectCachedTag(CachedTag instance)
        {
            SelectedGroup = Groups.FirstOrDefault(item => item.Group == instance.Group);
            SelectedInstance = SelectedGroup?.Instances.FirstOrDefault(item => $"{item.Instance}" == $"{instance}");
            SelectedInstanceName = $"{SelectedInstance.Instance}";
            SelectedGroupName = $"{SelectedGroup.Group}";
        }

        public void OnSelectedInstanceChanged()
        {
            if (SelectedInstance != null)
            {
                SetActualValue(SelectedInstance?.Instance);
                SelectedInstanceName = $"{SelectedInstance.Instance}";
            }
            else
                SelectedInstanceName = null;

            InvalidateCommands();
        }

        public void OnSelectedGroupChanged()
        {
            if (SelectedGroup != null)
            {
                SelectedGroupName = $"{SelectedGroup.Group}";
            }
            else
                SelectedGroupName = null;

            InvalidateCommands();
        }

        private void InvalidateCommands()
        {
            GotoCommand.RaiseCanExecuteChanged();
            NullCommand.RaiseCanExecuteChanged();
            BrowseCommand.RaiseCanExecuteChanged();
            CopyTagIndexCommand.RaiseCanExecuteChanged();
            CopyTagNameCommand.RaiseCanExecuteChanged();
            PasteTagNameCommand.RaiseCanExecuteChanged();
        }

        public void Null()
        {
            SelectedGroup = null;
        }

        private void BrowseTag()
        {
            var instance = _browseTagCallback();
            if (instance == null)
                return;

            SelectCachedTag(instance);
        }

        private void CopyTagName()
        {
            ClipboardEx.SetTextSafe($"{SelectedInstance.Instance}");
        }

        private void CopyTagIndex()
        {
            ClipboardEx.SetTextSafe($"0x{SelectedInstance.Instance.Index:X08}");
        }

        private void PasteTagName()
        {
            string[] split = Clipboard.GetText().Split('.');
            if (split.Count() != 2)
                return;
            else
            {
                SelectedGroup = Groups.FirstOrDefault(item => ((TagTool.Cache.Gen3.TagGroupGen3)item.Group).Name == split.Last()) ?? SelectedGroup;
                SelectedInstance = SelectedGroup.Instances.First(item => item.Name == split.First()) ?? SelectedInstance;
                SelectedInstanceName = $"{SelectedInstance.Instance}";
                SelectedGroupName = $"{SelectedGroup.Group}";
            }
        }
    }
}
