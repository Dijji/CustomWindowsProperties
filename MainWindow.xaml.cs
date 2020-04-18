// Copyright (c) 2020, Dijji, and released under Ms-PL.  This, with other relevant licenses, can be found in the root of this distribution.

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace CustomWindowsProperties
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly State state = new State();
        private readonly MainView view = new MainView();

        public MainWindow()
        {
            state.Populate();
            view.Populate(state);
            InitializeComponent();
            DataContext = view;
        }

        private void CustomTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (((TreeView)sender).SelectedItem is TreeItem item)
                view.SelectedTreeItem = item;
        }

        private void SystemTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (((TreeView)sender).SelectedItem is TreeItem item)
                view.SelectedTreeItem = item;
        }

        private void Export_Clicked(object sender, RoutedEventArgs e)
        {
            TreeItem treeItem = view.SelectedTreeItem;
            if (treeItem != null)
            {
                XmlDocument doc;
                string configName;
                IEnumerable<TreeItem> items;
                if (treeItem.Children.Count == 0)
                {
                    items = new TreeItem[] { treeItem };
                    configName = treeItem.Item as string;
                }
                else
                {
                    items = treeItem.Children;
                    configName = treeItem.Path;
                }

                doc = PropertyConfig.GetPropertyViewsAsXml(
                            items.
                            Select(t => t.Item).Cast<string>().Where(s => s != null).
                            Select(s => state.InstalledProperties[s]));
                var fileName = $"{FixFileName(configName)}.propdesc";
                doc.Save(state.DataFolder + $@"\{fileName}");
                DisplayStatus($"Exported successfully to {fileName}");
            }
        }

        private void ChooseDataFolder_Clicked(object sender, RoutedEventArgs e)
        {
            if (view.ChooseDataFolder())
                DisplayStatus($"Data folder is now {state.DataFolder}");
        }
        private void DisplayStatus(string text)
        {
            StatusBar.Text = text;
        }

        // To do Need a better home for this
        private static string FixFileName(string fileName)
        {
            return Regex.Replace(fileName, @"[\/?:*""><|]+", "_", RegexOptions.Compiled);
        }
    }
}
