// Copyright (c) 2020, Dijji, and released under Ms-PL.  This, with other relevant licenses, can be found in the root of this distribution.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CustomWindowsProperties
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly State state = new State();
        private readonly MainView view = new MainView();

        private string EditedPropertyName { get { return view.EditorConfig.CanonicalName; } }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = view;
            try
            {
                state.Populate();
                view.Populate(state);
            }
            catch (Exception ex)
            {
                DisplayStatus($"Error {ex.Message} occurred during initialisation");
            }
            RefreshPropertyEditor();
            //view.Test();
        }

        private void EditorTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (((TreeView)sender).SelectedItem is TreeItem item)
            {
                var pc = view.SetSelectedTreeItem(item, false);
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
                var config = view.SetSelectedTreeItem(item, true);
                if (config != null)
                {
                    PropertyDisplay.DataContext = config;
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

        private void Discard_Clicked(object sender, RoutedEventArgs e)
        {
            view.DiscardEditorChanges();
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
                switch (view.InstallEditedProperty())
                {
                    case 0:
                        DisplayStatus($"Property {EditedPropertyName} installed");
                        break;
                    case 1:
                        DisplayStatus($"Property {EditedPropertyName} installed with warnings of possible incompleteness");
                        break;
                    default:
                        break;
                }
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

        private void EditedInstall_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var config = (treeViewEditor.SelectedItem as TreeItem)?.Item as PropertyConfig;
            e.CanExecute = view.CanBeInstalled(config);
        }

        private void EditedInstall_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void EditedExport_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var treeItem = treeViewEditor.SelectedItem as TreeItem;
            e.CanExecute = view.CanBeExported(treeItem);
        }

        private void EditedExport_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var treeItem = treeViewEditor.SelectedItem as TreeItem;
            try
            {
                var outputFile = view.ExportPropDesc(treeItem);
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

        private void EditedDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var config = (treeViewEditor.SelectedItem as TreeItem)?.Item as PropertyConfig;
            e.CanExecute = view.CanBeDeleted(config);
        }


        private void EditedDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void InstalledUninstall_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

        }

        private void InstalledUninstall_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void RefreshPropertyEditor()
        {
            PropertyEditor.DataContext = null;
            PropertyEditor.DataContext = view.EditorConfig;
            view.RefreshEditedStatus();
        }

        private void DisplayStatus(string text)
        {
            StatusBar.Text = text;
        }
    }
}
