using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Input;

namespace DefinitionEditor
{
    /// <summary>
    /// Interaction logic for TagEditorView.xaml
    /// </summary>

    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class DefinitionEditorView : UserControl
    {
        public DefinitionEditorView()
        {
            InitializeComponent();
        }

        private void DefinitionContent_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && e.OriginalSource is TextBox box)
            {
                //box.IsSelectionActive = false;
                var poker = ((DefinitionEditorViewModel)DataContext).PokeCommand;

                if (poker.CanExecute(null))
                {
                    poker.Execute(null);
                }
                e.Handled = true;
            }
        }
    }
}
