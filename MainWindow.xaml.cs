// Copyright (c) 2020, Dijji, and released under Ms-PL.  This, with other relevant licenses, can be found in the root of this distribution.

using System;
using System.Windows;
using System.Windows.Controls;

namespace CustomWindowsProperties
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly State state = new State();
        private readonly MainView view = new MainView();

        private string EditedPropertyName { get { return view.PropertyBeingEdited.CanonicalName; } }

        public MainWindow()
        {
            state.Populate();
            view.Populate(state);
            InitializeComponent();
            DataContext = view;
            RefreshPropertyEditor();
            //view.Test();
        }

        private void EditorTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (((TreeView)sender).SelectedItem is TreeItem item)
            {
                var pc = view.SetSelectedItem(item, false);
                if (pc != null)
                {
                    RefreshPropertyEditor();
                }
            }
        }

        private void InstalledTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (((TreeView)sender).SelectedItem is TreeItem item)
            {
                var pc = view.SetSelectedItem(item, true);
                if (pc != null)
                {
                    PropertyDisplay.DataContext = pc;
                }
            }
        }

        private void Editor_GotFocus(object sender, RoutedEventArgs e)
        {
            view.EditorFocusChanged(((FrameworkElement)e.OriginalSource).Tag as string);
        }

        private void Editor_LostFocus(object sender, RoutedEventArgs e)
        {
            view.EditorFocusChanged(null);
        }

        private void ChooseDataFolder_Clicked(object sender, RoutedEventArgs e)
        {
            if (view.ChooseDataFolder())
                DisplayStatus($"Data folder is now {state.DataFolder}");
        }

        private void Export_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                var outputFile = view.ExportPropDesc();
                if (outputFile != null)
                {
                    DisplayStatus($"Exported successfully to {outputFile}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error exporting propdesc");
            }
        }

        private void Save_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                view.SaveEditedProperty();
                DisplayStatus($"Property {EditedPropertyName} saved");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error saving property");
            }
        }

        private void Delete_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                view.DeleteEditedProperty();
                DisplayStatus($"Property {EditedPropertyName} deleted");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error deleting property");
            }
        }

        private void Install_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (view.InstallEditedProperty())
                    DisplayStatus($"Property {EditedPropertyName} installed");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error installing property");
            }
        }

        private void Uninstall_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                var name = view.SelectedInstalledProperty.CanonicalName;
                if (view.UninstallEditedProperty())
                    DisplayStatus($"Property {name} uninstalled");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error uninstalling property");
            }
        }

        private void Copy_Clicked(object sender, RoutedEventArgs e)
        {
            view.CopyInstalledPropertyToEditor();
            RefreshPropertyEditor();
        }

        private void RefreshPropertyEditor()
        {
            PropertyEditor.DataContext = null;
            PropertyEditor.DataContext = view.PropertyBeingEdited;
            view.RefreshEditedStatus();
        }

        private void DisplayStatus(string text)
        {
            StatusBar.Text = text;
        }

    }
}
