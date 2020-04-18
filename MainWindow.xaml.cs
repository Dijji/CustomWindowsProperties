﻿// Copyright (c) 2020, Dijji, and released under Ms-PL.  This, with other relevant licenses, can be found in the root of this distribution.

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
            var outputFile = view.ExportPropDesc();
            if (outputFile != null)
            {
                DisplayStatus($"Exported successfully to {outputFile}");
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
    }
}
