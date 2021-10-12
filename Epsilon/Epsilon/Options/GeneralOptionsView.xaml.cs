using Epsilon.Pages;
using StartBar.Interop;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Epsilon.Options
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class GeneralOptionsView : UserControl
    {
        public GeneralOptionsView()
        {
            InitializeComponent();
        }

        private void AppearanceSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mergedDictionary = Application.Current.Resources.MergedDictionaries;
            int lastIndex = mergedDictionary.Count - 1;
            Uri currentThemeURI = mergedDictionary[lastIndex].Source;

            if (((ComboBox)sender).Name == "AccentComboBox")
            {
                Application.Current.Resources["AccentColor"] = (Color)Application.Current.Resources[(sender as ComboBox).SelectedItem.ToString()];

                mergedDictionary.RemoveAt(lastIndex);
                mergedDictionary.Add(new ResourceDictionary
                {
                    Source = currentThemeURI
                });
            }
            else if (((ComboBox)sender).Name == "ThemeComboBox")
            {
                string newThemeName = (sender as ComboBox).SelectedItem.ToString();

                mergedDictionary.RemoveAt(lastIndex);
                Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri("/Epsilon;component/Themes/" + newThemeName + ".xaml", UriKind.Relative)
                });

                Application.Current.Resources["BlurEnabledByTheme"] = Application.Current.Resources["BlurBehind"] ?? false;
                
                var mainwindow = Application.Current.MainWindow as ShellView;
                WindowBlur.SetIsEnabled(mainwindow, (bool)Application.Current.Resources["BlurEnabledByTheme"]);
            }
        }

        private void AlwaysOnTopCheckbox_Unchecked(object sender, RoutedEventArgs e) => Application.Current.Resources["AlwaysOnTop"] = false;
        private void AlwaysOnTopCheckbox_Checked(object sender, RoutedEventArgs e) => Application.Current.Resources["AlwaysOnTop"] = true;
    }
}
