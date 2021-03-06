﻿// Copyright (c) 2020, Dijji, and released under Ms-PL.  This, with other relevant licenses, can be found in the root of this distribution.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

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
            AddRectanglesToPropertyEditor();
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
        }

        private void SavedTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
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
            view.EditorFocusChanged(((FrameworkElement)e.OriginalSource).Tag as string, wbHelp);
        }

        private void AddRectanglesToPropertyEditor()
        {
            List<int> rows = new List<int>();
            foreach (var child in PropertyEditor.Children.Cast<UIElement>())
            {
                if (Grid.GetColumn(child) == 2)
                    rows.Add(Grid.GetRow(child));
            }

            foreach (var row in rows)
            {
                var rect = new Rectangle { Fill = Brushes.Transparent };
                rect.MouseLeftButtonUp += Rectangle_MouseLeftButtonUp;
                Grid.SetRow(rect, row);
                Grid.SetColumn(rect, 0);
                Grid.SetColumnSpan(rect, 2);
                PropertyEditor.Children.Add(rect);
            }
        }

        private void Rectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Clicking on the invisible rectangle in columns 0 and 1 
            // puts the focus on any control in column 2 of the same row
            var rect = (Rectangle)sender;
            var row = Grid.GetRow(rect);
            var target = PropertyEditor.Children.Cast<FrameworkElement>()
                    .FirstOrDefault(el => Grid.GetRow(el) == row && Grid.GetColumn(el) == 2 && 
                                    el.Tag as string != null );
            if (target != null)
                target.Focus();
        }

        private void Editor_LostFocus(object sender, RoutedEventArgs e)
        {
            view.EditorFocusChanged(null, wbHelp);
        }

        private void ChooseDataFolder_Clicked(object sender, RoutedEventArgs e)
        {
            if (view.ChooseDataFolder())
                DisplayStatus($"Data folder is now {state.DataFolder}");
        }

        private void New_Clicked(object sender, RoutedEventArgs e)
        {
            view.NewEditorProperty();
        }

        private void Save_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                view.SaveEditorProperty(treeViewSaved);
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

        private void Install_Clicked(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait; // Tree search can be slow
            try
            {
                switch (view.InstallEditorProperty(treeViewSaved, treeViewInstalled))
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
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void Copy_Clicked(object sender, RoutedEventArgs e)
        {
            view.CopyInstalledPropertyToEditor();
            RefreshPropertyEditor();
        }

        private void SavedExport_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = view.CanBeExported(treeViewSaved.SelectedItem as TreeItem);
        }

        private void SavedExport_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var outputFile = view.ExportPropDesc(treeViewSaved.SelectedItem as TreeItem);
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

        private void SavedDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = view.CanBeDeleted(treeViewSaved.SelectedItem as TreeItem);
        }

        private void SavedDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var count = view.DeleteProperties(treeViewSaved.SelectedItem as TreeItem);
                DisplayStatus($"Deleted {count} {(count == 1 ? "property" : "properties")}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error deleting properties");
            }
        }

        private void SavedInstall_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = view.CanBeInstalled(treeViewSaved.SelectedItem as TreeItem);
        }

        private void SavedInstall_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait; // Tree search can be slow
            try
            {
                var result = view.InstallProperties(treeViewSaved.SelectedItem as TreeItem, treeViewInstalled);
                DisplayStatus($"There were {result.Item1} successful installations and {result.Item2} failures");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error installing properties");
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void InstalledUninstall_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = view.CanBeUninstalled(treeViewInstalled.SelectedItem as TreeItem);
        }

        private void InstalledUninstall_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var count = view.UninstallProperties(treeViewInstalled.SelectedItem as TreeItem);
                DisplayStatus($"Uninstalled {count} {(count == 1 ? "property" : "properties")}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error uninstalling properties");
            }
        }

        private void RefreshPropertyEditor()
        {
            PropertyEditor.DataContext = null;
            PropertyEditor.DataContext = view.EditorConfig;
            view.RefreshEditorStatus();
        }

        private void DisplayStatus(string text)
        {
            StatusBar.Text = text;
        }

    }
}
