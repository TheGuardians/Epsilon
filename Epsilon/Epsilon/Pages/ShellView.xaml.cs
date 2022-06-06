using EpsilonLib.Editors;
using StartBar.Interop;
using System.ComponentModel.Composition;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;
using WpfApp20;

namespace Epsilon.Pages
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    [Export]

    public partial class ShellView : ChromeWindow
    {
        public ShellView()
        {
            InitializeComponent();
            bool blurEnabled = (bool)Application.Current.Resources["BlurEnabledByTheme"];
            WindowBlur.SetIsEnabled(this, blurEnabled);
        }

        private void StatusText_TargetUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            string newText = ((TextBlock)sender).Text;
            //StatusBar bar = ((StatusBarItem)((TextBlock)sender).Parent).Parent as StatusBar;
            //
            //
            //LinearGradientBrush flash = new LinearGradientBrush();
            //SolidColorBrush flashtest = new SolidColorBrush();
            //flashtest.Color = Colors.White;

            ColorAnimation animation = new ColorAnimation();
            animation.From = (Color)Application.Current.Resources["StatusUpdate"];
            animation.To = (Color)Application.Current.Resources["AccentColor"];
            animation.Duration = new Duration(TimeSpan.FromSeconds(0.35));

            if (!newText.Contains("Loading") && !newText.Contains("Deserializing") && !newText.Contains("Serializing") && newText != "Ready")
            {
                //EpsilonStatusBar.Background = new SolidColorBrush(Colors.Orange);
                EpsilonStatusBar.Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);
            }
            e.Handled = true;
        }

        private void Shell_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 0)
                return;

            if (DataContext is ShellViewModel vm)
            {
                vm.OnDroppedFiles(files);
            }
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string path = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
                string extension = path.Split('.').Last();

                IEditorService editors = ((ShellViewModel)DataContext)._editorService.Value;
                var editorlist = editors.EditorProviders.ToList();

                switch(extension)
                {
                    case "map":
                    case "dat":
                        editors.OpenFileWithEditorAsync(path, editorlist[0].Id);
                        break;
                    case "pak":
                        editors.OpenFileWithEditorAsync(path, editorlist[1].Id);
                        break;
                    default:
                        break;
                }
                e.Handled = true;
            }
        }

        //private void Grid_DragOver(object sender, DragEventArgs e)
        //{
        //    e.AcceptedOperation = DataPackageOperation.Copy;
        //}
    }
}
