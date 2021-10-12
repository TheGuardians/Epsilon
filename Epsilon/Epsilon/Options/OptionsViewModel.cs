using EpsilonLib.Options;
using EpsilonLib.Settings;
using EpsilonLib.Shell.TreeModels;
using Stylet;
using System;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Epsilon.Options
{
    class OptionsViewModel : Conductor<IOptionsPage>.Collection.OneActive
    {
        public OptionsPageTreeViewModel CategoryTree { get; }

        private IOptionsService _optionsService;

        public OptionsViewModel(IOptionsService optionsService)
        {
            _optionsService = optionsService;
            DisplayName = "Options";
            CategoryTree = new OptionsPageTreeViewModel(optionsService.OptionPages);
            CategoryTree.NodeSelected += CategoryTree_NodeSelected;
        }

        private void CategoryTree_NodeSelected(object sender, TreeNodeEventArgs e)
        {
            if(e.Node is OptionsTreeNode node)
            {
                ActiveItem = node.Page;
                if(node.Children.Count > 0)
                    node.IsExpanded = true;
            }
        }

        public void Apply()
        {
            foreach(var page in _optionsService.OptionPages)
            {
                if(page.IsDirty)
                    page.Apply();

                page.IsDirty = false;
            }

            RequestClose(true);
        }

        public void Cancel()
        {
            GeneralOptionsViewModel general = (GeneralOptionsViewModel)_optionsService.OptionPages.First();

            string originalAccent = general._settings.Get("Accent", Accent.Cobalt).ToString();
            string originalTheme = general._settings.Get("Theme", Theme.Solid).ToString();
            bool originalAlwaysOnTop = general._settings.Get("AlwaysOnTop", false);
            Application.Current.Resources["AccentColor"] = (Color)Application.Current.Resources[originalAccent];

            var mergedDictionary = Application.Current.Resources.MergedDictionaries;
            mergedDictionary.RemoveAt(mergedDictionary.Count - 1);

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("/Epsilon;component/Themes/" + originalTheme + ".xaml", UriKind.Relative)
            });

            Application.Current.Resources["AlwaysOnTop"] = originalAlwaysOnTop;

            RequestClose(false);
        }
    }
}
