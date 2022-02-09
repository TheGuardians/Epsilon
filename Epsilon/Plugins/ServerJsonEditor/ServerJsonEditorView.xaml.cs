using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using static ServerJsonEditor.ServerJsonEditorViewModel;

namespace ServerJsonEditor
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class ServerJsonEditorView : UserControl
    {
        int gametypeIndex = -1;
        ModEntry selectedModEntry = null;
        TypeEntry selectedTypeEntry = null;
        public string modsComboBoxSelection = null;
        public string typesComboBoxSelection = null;
        private ObservableCollection<TypeEntry> currentModGametypes = new ObservableCollection<TypeEntry>();

        public ServerJsonEditorView()
        {
            InitializeComponent();
            TypeDataGrid.Visibility = Visibility.Collapsed;
        }

            // General Handlers

        private void mainGrid_Loaded(object sender, RoutedEventArgs e)
        {
            //ModsListBox.ItemsSource = ((ServerJsonEditorViewModel)DataContext).gametypeMapping.Keys;
        }

            // ListBox Selection Handlers

        private void ModsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selectedModEntry != null && gametypeIndex > -1) // && currentTypes.Count() > 0
                ReplaceTypeEntryOnSwitch();

            TypeDataGrid.Visibility = Visibility.Collapsed;

            selectedModEntry = (ModEntry)((ListBox)sender).SelectedValue;
            if (selectedModEntry != null)
			{
                ((ServerJsonEditorViewModel)DataContext).CurrentGametypeList = ((ServerJsonEditorViewModel)DataContext).modGametypeMapping[selectedModEntry];
                TypesListBox.ItemsSource = ((ServerJsonEditorViewModel)DataContext).CurrentGametypeList;
            }
        }
        private void TypesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ReplaceTypeEntryOnSwitch();

            if (selectedTypeEntry == null)
                TypeDataGrid.Visibility = Visibility.Visible;

            selectedTypeEntry = (TypeEntry)((ListBox)sender).SelectedValue;
            gametypeIndex = ((ServerJsonEditorViewModel)DataContext).modGametypeMapping[selectedModEntry].IndexOf(selectedTypeEntry);

            TypeDataGrid.DataContext = selectedTypeEntry;

            if (TypeDataGrid.DataContext != null)
			{
                //CommandListBox.ItemsSource = ((TypeEntry)TypeDataGrid.DataContext).Commands;
                CharacterListBox.ItemsSource = ((TypeEntry)TypeDataGrid.DataContext).CharacterOverrides;
                //MapsListBox.ItemsSource = ((TypeEntry)TypeDataGrid.DataContext).SpecificMaps;
                
                MapsComboBox.ItemsSource = ((ServerJsonEditorViewModel)DataContext).LocalMapList;

                //((ServerJsonEditorViewModel)DataContext).CurrentSpecificMaps = selectedTypeEntry.SpecificMaps;
            }
        }

            // ComboBox Selection Handlers

        private void ModsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
        private void TypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

            // Addition/Removal Handlers

        private void ClickedAddMod(object sender, RoutedEventArgs e)
        {
            string modNameToAdd = (string)(ModsCombobox.SelectedItem);

            if (!string.IsNullOrEmpty(modNameToAdd))
			{
                ((ServerJsonEditorViewModel)DataContext).AddMod(modNameToAdd);
                ModsCombobox.SelectedIndex = -1;
                ModsListBox.Items.SortDescriptions.Add(new SortDescription("FileName", ListSortDirection.Ascending));
            }
        }
        private void ClickedRemoveMod(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Are you sure you want to remove this mod entry?"
                + "\nIts associated gametype entries will also be removed.",
                "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                ModEntry modToRemove = (ModEntry)ModsListBox.SelectedItem;

                TypeDataGrid.Visibility = Visibility.Collapsed;
                selectedTypeEntry = null;
                TypesListBox.ItemsSource = null;
                ModsListBox.SelectedIndex = -1;

                ((ServerJsonEditorViewModel)DataContext).RemoveMod(modToRemove);

                ModsListBox.Items.SortDescriptions.Remove(new SortDescription("FileName", ListSortDirection.Ascending));
            }
        }

        private void ClickedAddGametype(object sender, RoutedEventArgs e)
        {
            string gametypeNameToAdd = (string)(TypesCombobox.SelectedItem);
            ModEntry modSelection = (ModEntry)ModsListBox.SelectedItem;

            if (!string.IsNullOrEmpty(gametypeNameToAdd))
			{
                ((ServerJsonEditorViewModel)DataContext).AddGametype(gametypeNameToAdd, modSelection);
                TypesCombobox.SelectedIndex = -1;
                ModsListBox.Items.SortDescriptions.Add(new SortDescription("TypeName", ListSortDirection.Ascending));
            }
        }
        private void ClickedRemoveGametype(object sender, RoutedEventArgs e)
        {
            TypeEntry gametypeToRemove = (TypeEntry)(TypesListBox.SelectedItem);
            ((ServerJsonEditorViewModel)DataContext).RemoveGametype(gametypeToRemove);
            TypesListBox.SelectedIndex = -1;

            ModsListBox.Items.SortDescriptions.Remove(new SortDescription("TypeName", ListSortDirection.Ascending));
        }

        private void ClickedAddMap(object sender, RoutedEventArgs e)
        {
            string mapNameToAdd = (string)(MapsComboBox.SelectedItem);

            if (mapNameToAdd != null)
            {
                MapEntry newEntry = new MapEntry()
                {
                    MapFileName = mapNameToAdd,
                    DisplayName = mapNameToAdd
                };

                ((TypeEntry)TypeDataGrid.DataContext).SpecificMaps.Add(newEntry);
                MapsComboBox.SelectedIndex = -1;
                MapsListBox.Items.SortDescriptions.Add(new SortDescription("TypeName", ListSortDirection.Ascending));
            }
        }
        private void ClickedRemoveMap(object sender, RoutedEventArgs e)
        {
            MapEntry mapToRemove = (MapEntry)MapsListBox.SelectedItem;

            if (mapToRemove != null)
			{
                ((TypeEntry)TypeDataGrid.DataContext).SpecificMaps.Remove(mapToRemove);
                MapsListBox.SelectedItem = -1;
                MapsListBox.Items.SortDescriptions.Remove(new SortDescription(mapToRemove.MapFileName, ListSortDirection.Ascending));
            }
        }

        private void ClickedAddCharOverride(object sender, RoutedEventArgs e)
        {
            string count = ((TypeEntry)TypeDataGrid.DataContext).CharacterOverrides.Count.ToString();

            CharacterOverride charToAdd = new CharacterOverride()
            {
                Team = "team" + count,
                Character = ""
            };

            ((TypeEntry)TypeDataGrid.DataContext).CharacterOverrides.Add(charToAdd);
            CharacterListBox.SelectedItem = -1;
            CharacterListBox.Items.SortDescriptions.Add(new SortDescription(charToAdd.Team, ListSortDirection.Ascending));
        }
        private void ClickedRemoveCharOverride(object sender, RoutedEventArgs e)
        {
            CharacterOverride charToRemove = (CharacterOverride)CharacterListBox.SelectedItem;

            if (charToRemove != null)
			{
                ((TypeEntry)TypeDataGrid.DataContext).CharacterOverrides.Remove(charToRemove);
                CharacterListBox.SelectedItem = -1;
                CharacterListBox.Items.SortDescriptions.Remove(new SortDescription(charToRemove.Team, ListSortDirection.Ascending));
            }
        }

            // Other Methods

        private void ReplaceTypeEntryOnSwitch()
        {
            if (selectedTypeEntry != null)
			{
                DupeSlider.Value = selectedTypeEntry.DuplicateAmount;
                ((ServerJsonEditorViewModel)DataContext).modGametypeMapping[selectedModEntry][gametypeIndex] = (TypeEntry)TypeDataGrid.DataContext;
            }
        }

		private void DupeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
            DuplicateAmountTextBox.Text = e.NewValue.ToString();
        }

		private void SaveButtonClicked(object sender, RoutedEventArgs e)
		{
            ReplaceTypeEntryOnSwitch();
            ((ServerJsonEditorViewModel)DataContext).Save();
            TypeDataGrid.Visibility = Visibility.Collapsed;
            TypesListBox.SelectedItem = -1;
            ModsListBox.SelectedItem = -1;
        }

		private void ReloadAllClicked(object sender, RoutedEventArgs e)
		{
            if(MessageBox.Show($"Are you sure you want to reload your Server JSONs?" + $"\nAny changes you've made will be lost.",
                "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
			{
                selectedTypeEntry = null;
                gametypeIndex = -1;
                selectedModEntry = null;
                modsComboBoxSelection = null;
                typesComboBoxSelection = null;

                ((ServerJsonEditorViewModel)DataContext).ReloadAll();
                TypeDataGrid.Visibility = Visibility.Collapsed;
                TypesListBox.SelectedItem = -1;
                ModsListBox.SelectedItem = -1;
            }
        }
	}
}
