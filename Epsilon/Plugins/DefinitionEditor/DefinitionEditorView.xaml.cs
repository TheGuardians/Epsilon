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
                    SearchBox.Select(0, SearchBox.Text.Length);
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
            // enter to poke

            if (e.Key == Key.Return)    // && e.OriginalSource is TextBox box
            {
                DefinitionEditorViewModel definitionViewModel = (DefinitionEditorViewModel)DataContext;

                if (definitionViewModel != null && IsVisible)
                {
                    switch (e.OriginalSource)
                    {
                        case TextBox box:
                            box.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                            break;
                    }

                    definitionViewModel.PokeCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }
    }
}
