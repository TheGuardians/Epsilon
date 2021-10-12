using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
//using TagStructEditor;

namespace DefinitionEditor.Options
{
    /// <summary>
    /// Interaction logic for OptionsPageView.xaml
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class OptionsPageView : UserControl
    {
        public OptionsPageView()
        {
            InitializeComponent();

            DisplayTypesCheckbox.Click += new RoutedEventHandler(DisplayTypesCheckbox_Click);
            DisplayOffsetsCheckbox.Click += new RoutedEventHandler(DisplayOffsetsCheckbox_Click);
            CollapseBlocksCheckbox.Click += new RoutedEventHandler(CollapseBlocksCheckbox_Click);

        }

        private void DisplayTypesCheckbox_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DisplayOffsetsCheckbox_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CollapseBlocksCheckbox_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
