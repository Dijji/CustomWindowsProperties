// Copyright (c) 2020, Dijji, and released under Ms-PL.  This, with other relevant licenses, can be found in the root of this distribution.

using System.Linq;
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
        private State state = new State();
        private MainView view = new MainView();

        public MainWindow()
        {
            state.PopulateProperties();
            view.PopulatePropertyTrees(state);
            InitializeComponent();
            DataContext = view;
        }

        private void treeCustom_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = ((TreeView)sender).SelectedItem as TreeItem;
            if (item != null)
                treeItem = item;
        }
    
        private void treeSystem_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = ((TreeView)sender).SelectedItem as TreeItem;
            if (item != null)
                treeItem = item;
        }

        TreeItem treeItem = null;

        private void Export_Clicked(object sender, RoutedEventArgs e)
        {
            if (treeItem != null)
            {
                XmlDocument doc;
                var pview = treeItem.Item as PropertyView;
                if (pview != null)
                    doc = view.GetPropertyViewsAsXml(new PropertyView[] { pview });
                else
                    doc = view.GetPropertyViewsAsXml(treeItem.Children.Select(t => t.Item).Cast<PropertyView>());
                doc.Save(@"d:\documents\schema.xml");
            }
        }
    }

    public class MyVirtualizingStackPanel : VirtualizingStackPanel
    {
        /// <summary>
        /// Publically expose BringIndexIntoView.
        /// </summary>
        public void BringIntoView(int index)
        {
            this.BringIndexIntoView(index);
        }
    }

}
