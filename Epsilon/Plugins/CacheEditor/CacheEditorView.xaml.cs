using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Xceed.Wpf.AvalonDock;

namespace CacheEditor
{
    /// <summary>
    /// Interaction logic for CacheEditorView.xaml
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class CacheEditorView : UserControl
    {
        public CacheEditorView()
        {
            InitializeComponent();
        }

        private void DockingManager_ActiveContentChanged(object sender, EventArgs e)
        {
            var dockingManager = (DockingManager)sender;
            Debug.WriteLine($"Active Content {dockingManager.ActiveContent}");
        }
    }
}
