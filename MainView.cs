﻿// Copyright (c) 2020, Dijji, and released under Ms-PL.  This, with other relevant licenses, can be found in the root of this distribution.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using FolderSelect;

namespace CustomWindowsProperties
{
    class MainView : INotifyPropertyChanged
    {
        private State state;

        // Dictionaries of the parents in the trees (does not include the terminals)
        private Dictionary<string, TreeItem> dictSavedTree = null;
        private Dictionary<string, TreeItem> dictInstalledTree = null;

        #region Public properties
        public ObservableCollection<TreeItem> InstalledPropertyTree { get; } = new ObservableCollection<TreeItem>();
        public ObservableCollection<TreeItem> SavedPropertyTree { get; } = new ObservableCollection<TreeItem>();

        public PropertyConfig SelectedSavedProperty
        {
            get { return selectedSavedProperty; }
            private set { selectedSavedProperty = value; }
        }
        private PropertyConfig selectedSavedProperty;

        public PropertyConfig SelectedInstalledProperty
        {
            get { return selectedInstalledProperty; }
            private set
            {
                selectedInstalledProperty = value;
                OnPropertyChanged(nameof(IsInstalledPropertyVisible));
                OnPropertyChanged(nameof(CanCopy));
            }
        }
        private PropertyConfig selectedInstalledProperty;

        public TreeItem SelectedTreeItem
        {
            get { return selectedTreeItem; }
            private set { selectedTreeItem = value; OnPropertyChanged(); }
        }
        private TreeItem selectedTreeItem;

        public PropertyConfig EditorConfig { get; set; }

        public PropertyConfig EditorBaseline { get; set; }

        public string EditorInstalledText
        {
            get
            {
                // Name has to be valid structurally and not part of the parent tree of another property
                var canonicalName = EditorConfig.CanonicalName;
                var error = ValidateName(canonicalName);
                if (error != null)
                    return error;
                else if (state.DictInstalledProperties.ContainsKey(canonicalName))
                    return "True";
                else
                    return "False";
            }
        }

        public bool CanDiscard { get { return IsEditorDirty; } }

        public bool CanDelete { get { return CanBeDeleted(EditorConfig); } }

        public bool CanInstall { get { return CanBeInstalled(EditorConfig); } }

        public bool CanCopy { get { return SelectedInstalledProperty != null; } }

        public bool CompareSaved
        {
            get { return compareSaved; }
            set
            {
                if (compareSaved != value)
                {
                    compareSaved = value;
                    OnPropertyChanged();
                    CompareEditorToBaseline();
                }
            }
        }
        private bool compareSaved;

        public bool CompareInstalled
        {
            get { return compareInstalled; }
            set
            {
                if (compareInstalled != value)
                {
                    compareInstalled = value;
                    OnPropertyChanged();
                    CompareEditorToBaseline();
                }
            }
        }
        private bool compareInstalled;

        public bool IsInstalledPropertyVisible { get { return SelectedInstalledProperty != null; } }

        public bool IsEditorDirty { get { return isEditorDirty; } private set { isEditorDirty = value; OnPropertyChanged(nameof(CanDiscard)); } }
        private bool isEditorDirty;

        public string DifferencesText { get { return differencesText; } set { differencesText = value; OnPropertyChanged(); } }
        private string differencesText;

        public string HelpText { get { return helpText; } set { helpText = value; OnPropertyChanged(); } }
        private string helpText;

        public bool HasDataFolder { get { return state.DataFolder != null; } }
        #endregion

        private BaselineType EditorBaselineType { get; set; }

        private bool IsBulkUpdating { get; set; }


        public MainView()
        {
            EditorConfig = new PropertyConfig();
            EditorConfig.PropertyChanged += Editor_PropertyChanged;
        }

        private void Editor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PropertyConfig.CanonicalName))
            {
                RefreshEditorStatus();
            }
            CheckIfEditorDirty();
        }

        #region Public methods

        public void Populate(State state)
        {
            this.state = state;
            EditorBaseline = new PropertyConfig();
            EditorBaseline.SetDefaultValues();
            LoadEditorConfig(EditorBaseline, BaselineType.Standalone);
            dictInstalledTree = PropertyTree.PopulatePropertyTree(state.SystemProperties.Concat(state.CustomProperties),
                InstalledPropertyTree, true);
            dictSavedTree = PropertyTree.PopulatePropertyTree(state.SavedProperties,
                SavedPropertyTree, false, ShowIfSavedIsInstalled);
        }

        public bool ChooseDataFolder()
        {
            var fsd = new FolderSelectDialog
            {
                Title = "Choose folder for storing data files",
                InitialDirectory = state.DataFolder ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            if (fsd.ShowDialog(IntPtr.Zero))
            {
                state.DataFolder = fsd.FileName;
                RefreshEditorStatus();
            }
            return false;
        }

        public PropertyConfig SetSelectedTreeItem(TreeItem treeItem, bool isInstalled)
        {
            SelectedTreeItem = treeItem;
            var selectedProperty = (treeItem)?.Item as PropertyConfig;

            if (isInstalled)
            {
                SelectedInstalledProperty = selectedProperty;
            }
            else
            {
                SelectedSavedProperty = selectedProperty;
                if (SelectedSavedProperty != null)
                {
                    LoadEditorConfig(SelectedSavedProperty, BaselineType.Saved);
                }
            }

            CompareEditorToBaseline(); // in case there is an override 
            return selectedProperty;
        }

        public void RefreshEditorStatus()
        {
            OnPropertyChanged(nameof(EditorInstalledText));
            OnPropertyChanged(nameof(CanDelete));
            OnPropertyChanged(nameof(CanInstall));
        }

        public void EditorFocusChanged(string tag)
        {
            //CheckIfEditorDirty();
            if (tag != null)
                HelpText = Help.Text(tag);
        }

        public bool CanBeInstalled(PropertyConfig config)
        {
            return HasDataFolder && config != null &&
                ValidateName(config.CanonicalName) == null &&
                !state.DictInstalledProperties.ContainsKey(config.CanonicalName);
        }

        public bool CanBeDeleted(PropertyConfig config)
        {
            return CanBeInstalled(config) &&
                state.DictSavedProperties.ContainsKey(config.CanonicalName);
        }

        public bool CanBeExported(TreeItem treeItem)
        {
            // Must exist and be a leaf, or the immediate parent of leaves
            return HasDataFolder && treeItem != null &&
                (treeItem.Children.Count == 0 || treeItem.Children[0].Children.Count == 0);
        }

        public bool CanBeUninstalled(PropertyConfig config)
        {
            return HasDataFolder && config != null &&
                state.DictInstalledProperties.ContainsKey(config.CanonicalName) &&
                state.DictSavedProperties.ContainsKey(config.CanonicalName);
        }

        public string ExportPropDesc(TreeItem treeItem)
        {
            XmlDocument doc;
            string configName;
            IEnumerable<TreeItem> items;

            if (treeItem.Children.Count == 0)
            {
                items = new TreeItem[] { treeItem };
                configName = ((PropertyConfig)treeItem.Item).CanonicalName;
            }
            else
            {
                items = treeItem.Children;
                configName = treeItem.Path;
            }

            doc = PropertyConfig.GetPropDesc(
                    items.Select(t => t.Item).Cast<PropertyConfig>().Where(s => s != null));

            var fileName = $"{Extensions.FixFileName(configName)}.propdesc";
            doc.Save(state.DataFolder + $@"\{fileName}");

            // Testing
            //if (treeItem.Item != null)
            //{
            //    var pc = state.InstalledProperties[treeItem.Item as string];
            //    state.SavePropertyConfig(pc);
            //    var pc2 = state.LoadPropertyConfig($@"{state.DataFolder}\{pc.CanonicalName}.xml");
            //    pc2.CanonicalName = pc2.CanonicalName + "2";
            //    state.SavePropertyConfig(pc2); // Round-trip for comparison at leisure
            //}

            return fileName;
        }

        public void DeleteProperty(PropertyConfig config)
        {
            var canonicalName = config.CanonicalName;

            // Caller must ensure that property is in the editor tree, but not installed
            state.DeletePropertyConfig(canonicalName);
            state.RemoveSavedProperty(canonicalName);
            PropertyTree.RemoveTreeItem(dictSavedTree, SavedPropertyTree, canonicalName);
            RefreshEditorStatus();
        }

        public void DiscardEditorChanges()
        {
            LoadEditorConfig(EditorBaseline, EditorBaselineType, true);
            CheckIfEditorDirty();
        }

        public PropertyConfig SaveEditorProperty(TreeView treeViewSaved)
        {
            if (state.DictSavedProperties.TryGetValue(EditorConfig.CanonicalName, out PropertyConfig config))
            {
                // Property is already known about, just update its values
                config.CopyFrom(EditorConfig, false);
                state.SavePropertyConfig(EditorConfig);
                IsEditorDirty = false;
                return config;
            }
            else
            {
                // Property is new, need to clone it and add it in
                PropertyConfig newConfig = new PropertyConfig();
                newConfig.CopyFrom(EditorConfig, false);
                AssignPropertyKey(newConfig);
                EditorConfig.FormatId = newConfig.FormatId;
                EditorConfig.PropertyId = newConfig.PropertyId;

                state.SavePropertyConfig(newConfig);
                state.AddSavedProperty(newConfig);
                var treeItem = PropertyTree.AddTreeItem(dictSavedTree, SavedPropertyTree, newConfig);
                EditorBaseline = newConfig;
                EditorBaselineType = BaselineType.Saved;
                CheckIfEditorDirty();
                SelectTreeItemAfterDelay(treeViewSaved, treeItem);
                RefreshEditorStatus();
                return newConfig;
            }
        }
     
        public int InstallEditorProperty(TreeView treeViewSaved, TreeView treeViewInstalled)
        {
            // Save as XML and update state and tree as necessary
            var config = SaveEditorProperty(treeViewSaved);

            return InstallProperty(config, treeViewInstalled);
        }

        public int InstallProperty(PropertyConfig config, TreeView treeViewInstalled)
        {
            // Save as propdesc
            var doc = PropertyConfig.GetPropDesc(new PropertyConfig[] { config });
            var fullFileName = $"{state.DataFolder}{Path.DirectorySeparatorChar}{config.CanonicalName}.propdesc";
            doc.Save(fullFileName);

            // Attempt installation
            var result = state.RegisterCustomProperty(fullFileName, config, out PropertyConfig installedConfig);

            if (result >= 0)
            {
                // Property is new, need to add it in
                state.AddInstalledProperty(installedConfig);
                var treeItem = PropertyTree.AddTreeItem(dictInstalledTree, InstalledPropertyTree,
                                installedConfig, addRootTop: true);
                SelectTreeItemAfterDelay(treeViewInstalled, treeItem);
                ShowIfSavedIsInstalled(installedConfig.CanonicalName);
                RefreshEditorStatus();
            }

            return result;
        }

        public bool UninstallProperty(PropertyConfig config)
        {
            var canonicalName = config.CanonicalName;

            // Attempt uninstall
            bool succeeded = state.UnregisterCustomProperty(canonicalName);

            if (succeeded)
            {
                state.RemoveInstalledProperty(canonicalName);
                ShowIfSavedIsInstalled(canonicalName);
                PropertyTree.RemoveTreeItem(dictInstalledTree, InstalledPropertyTree, canonicalName);
                RefreshEditorStatus();
            }

            return succeeded;
        }


        public void CopyInstalledPropertyToEditor()
        {
            LoadEditorConfig(SelectedInstalledProperty, BaselineType.Installed);
        }
#endregion
        #region Private methods
        private string ValidateName(string name)
        {
            // Name has to be valid structurally and not part of the parent tree of another property
            if (!Extensions.IsValidPropertyName(name))
                return "Invalid property name";
            else if (PropertyTree.NameContainsExistingName(name, SavedPropertyTree, true))
                return "Name clashes with saved property name";
            else if (PropertyTree.NameContainsExistingName(name, InstalledPropertyTree, false) &&
                     !PropertyTree.NameContainsExistingName(name, SavedPropertyTree, false))
                // All installed property names clash unless we installed it ourselves
                return "Name clashes with installed property name";
            else
                return null;
        }

        private enum BaselineType
        {
            Standalone,
            Saved,
            Installed,
        }

        private bool LoadEditorConfig(PropertyConfig baseline, BaselineType type, bool alwaysOverwrite = false)
        {
            if (!alwaysOverwrite)
            {
                string question = null;
                if (IsEditorDirty)
                    question = "Do you want to discard the changes you have made?";
                else if (EditorBaselineType == BaselineType.Installed &&
                         type != BaselineType.Installed)
                    question = "Do you want to discard the installed property values?";

                if (question != null)
                {
                    var result = MessageBox.Show(question,
                        "All data in the editor will be overwritten", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.No)
                        return false;
                }
            }

            IsBulkUpdating = true;
            EditorConfig.CopyFrom(baseline, type == BaselineType.Installed);
            IsBulkUpdating = false;
            EditorBaseline = baseline;
            EditorBaselineType = type;
            IsEditorDirty = false;
            CheckIfEditorDirty();
            RefreshEditorStatus();

            switch (EditorBaselineType)
            {
                case BaselineType.Standalone:
                case BaselineType.Saved:
                    CompareSaved = true;
                    break;
                case BaselineType.Installed:
                    CompareInstalled = true;
                    break;
            }

            return true;
        }

        private void ShowIfSavedIsInstalled(string canonicalName)
        {
            var treeItem = PropertyTree.FindTreeItem(canonicalName, dictSavedTree);
            if (treeItem != null)
                ShowIfSavedIsInstalled(treeItem, treeItem.Item as PropertyConfig);
        }

        private void ShowIfSavedIsInstalled(TreeItem treeItem, PropertyConfig config)
        {
            if (CanBeUninstalled(config))
                treeItem.Background = Brushes.LightGreen;
            else
                treeItem.Background = null;
        }

        private void CheckIfEditorDirty()
        {
            if (!IsBulkUpdating)
                IsEditorDirty = CompareEditorToBaseline();
        }
        
        private bool CompareEditorToBaseline()
        {
            var baseline = EditorBaseline;
            var isInstalled = (EditorBaselineType == BaselineType.Installed);

            // Check for comparison override
            if (CompareSaved && EditorBaselineType == BaselineType.Installed)
                baseline = SelectedSavedProperty;
            else if (CompareInstalled && EditorBaselineType != BaselineType.Installed)
            {
                baseline = SelectedInstalledProperty;
                isInstalled = true;
            }

            if (baseline == null)
            {
                DifferencesText = "No baseline property selected";
                return false;
            }

            var differences = EditorConfig.CompareTo(baseline, isInstalled); 

            if (differences.Count > 0)
            {
                var sb = new StringBuilder();

                foreach (var d in differences)
                {
                    sb.AppendLine($"{d.Name} changed from '{d.Previous}' to '{d.Current}'");
                }

                DifferencesText = sb?.ToString();
                return true;
            }
            else
            {
                DifferencesText = "No differences";
                return false;
            }

            //var compare = new CompareLogic();
            //compare.Config.MaxDifferences = int.MaxValue;
            //compare.Config.CompareChildren = false;
            //var result = compare.Compare(baseline, EditorConfig);
            //StringBuilder sb = null;
            //bool different = false;

            //if (!result.AreEqual)
            //{
            //    sb = new StringBuilder();
            //    //sb.AppendLine("Differences are:");
            //    foreach (var d in result.Differences)
            //    {
            //        if (!isInstalled ||
            //            !PropertyConfig.InstalledExclusions.Contains(d.PropertyName))
            //        {
            //            sb.AppendLine($"{d.PropertyName} changed from '{d.Object1Value}' to '{d.Object2Value}'");
            //            different = true;
            //        }
            //    }
            //}

            //if (different)
            //    DifferencesText = sb.ToString();
            //else
            //    DifferencesText = "No differences";

            //return different;
        }

        private void AssignPropertyKey(PropertyConfig config)
        {
            // Look for parent property
            var parentName = PropertyTree.FirstPartsOf(config.CanonicalName);
            if (parentName != null && dictSavedTree.TryGetValue(parentName, out TreeItem parent))
            {
                if (parent.Children.Count != 0)
                {
                    config.FormatId = (parent.Children[0].Item as PropertyConfig).FormatId;
                    config.PropertyId = parent.Children.Select(t => t.Item).Cast<PropertyConfig>()
                        .Select(p => p?.PropertyId).Max() + 1;
                    return;
                }
            }

            // No usable parent, give it a new name  
            config.FormatId = Guid.NewGuid();
            config.PropertyId = 1;
        }

        private void SelectTreeItemAfterDelay(TreeView treeView, TreeItem treeItem)
        {
            // Need to give the tree view a little time before selecting a new item
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(200);
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    TreeViewHelper.SelectTreeProperty(treeView, treeItem);
                }));
            });
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}
