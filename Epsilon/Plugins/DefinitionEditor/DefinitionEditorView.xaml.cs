using System;
using System.ComponentModel.Composition;
using System.Windows;
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

            EventManager.RegisterClassHandler(typeof(Window), Window.PreviewKeyUpEvent, new KeyEventHandler(TagDefWindowKeyUp));
        }

        private void TagDefWindowKeyUp(object sender, KeyEventArgs e)
        {
            // ctrl-F to focus Definition Search

            if ((e.Key == Key.F && e.KeyboardDevice.IsKeyDown(Key.LeftCtrl)) || (e.Key == Key.LeftCtrl && e.KeyboardDevice.IsKeyDown(Key.F)))
            {
                DefinitionEditorViewModel definitionViewModel = (DefinitionEditorViewModel)DataContext;
                if (definitionViewModel != null && IsVisible)
                {
                    SearchBox.Focus();
                    Keyboard.Focus(SearchBox);
                    e.Handled = true;
                }
            }

            // ctrl-S to save

            else if ((e.Key == Key.S && e.KeyboardDevice.IsKeyDown(Key.LeftCtrl)) || (e.Key == Key.LeftCtrl && e.KeyboardDevice.IsKeyDown(Key.S)))
            {
                DefinitionEditorViewModel definitionViewModel = (DefinitionEditorViewModel)DataContext;
                if (definitionViewModel != null && IsVisible)
                {
                    definitionViewModel.SaveCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }

        private void DefinitionContent_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)    // && e.OriginalSource is TextBox box
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
