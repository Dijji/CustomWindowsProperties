// Copyright (c) 2020, Dijji, and released under Ms-PL.  This, with other relevant licenses, can be found in the root of this distribution.

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

        public TreeItem SelectedSavedTreeItem
        {
            get { return selectedSavedTreeItem; }
            private set { selectedSavedTreeItem = value; OnPropertyChanged(); }
        }
        private TreeItem selectedSavedTreeItem;

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
            var newBaseline = new PropertyConfig();
            newBaseline.SetDefaultValues();
            LoadEditorConfig(newBaseline, BaselineType.Standalone, alwaysOverwrite: true);
            dictInstalledTree = PropertyTree.PopulatePropertyTree(state.SystemProperties.Concat(state.CustomProperties),
                InstalledPropertyTree, true);
            dictSavedTree = PropertyTree.PopulatePropertyTree(state.SavedProperties,
                SavedPropertyTree, false, ShowIfSavedIsInstalled);
        }

        public PropertyConfig SetSelectedTreeItem(TreeItem treeItem, bool isInstalled)
        {
            var selectedProperty = (treeItem)?.Item as PropertyConfig;

            if (isInstalled)
            {
                SelectedInstalledProperty = selectedProperty;
            }
            else
            {
                SelectedSavedTreeItem = treeItem;
                SelectedSavedProperty = selectedProperty;
                if (SelectedSavedProperty != null)
                {
                    LoadEditorConfig(SelectedSavedProperty, BaselineType.Saved);
                }
                else
                {
                    NewEditorProperty(treeItem.Tag + ".");
                }
            }

            CompareEditorToBaseline(); // in case there is an override 
            return selectedProperty;
        }

        #region Editor support
        public void RefreshEditorStatus()
        {
            OnPropertyChanged(nameof(EditorInstalledText));
            OnPropertyChanged(nameof(CanDelete));
            OnPropertyChanged(nameof(CanInstall));
        }

        public void EditorFocusChanged(string tag, WebBrowser browser)
        {
            if (tag != null)
                browser.NavigateToString(Help.HtmlText(tag));
        }

        public void NewEditorProperty(string canonicalName = null)
        {
            if (SelectedSavedTreeItem != null)
                SelectedSavedTreeItem.IsSelected = false;
            var newBaseline = new PropertyConfig();
            newBaseline.SetDefaultValues();
            if (canonicalName != null)
                newBaseline.CanonicalName = canonicalName;
            LoadEditorConfig(newBaseline, BaselineType.Standalone);
        }

        public void DiscardEditorChanges()
        {
            LoadEditorConfig(EditorBaseline, EditorBaselineType, alwaysOverwrite: true);
        }

        public PropertyConfig SaveEditorProperty(TreeView treeViewSaved)
        {
            if (state.DictSavedProperties.TryGetValue(EditorConfig.CanonicalName, out PropertyConfig config))
            {
                // Property is already known about, just update its values
                config.CopyFrom(EditorConfig, false);
                if (config.PropertyId <= 1) // Nust be at least 2
                    AssignPropertyKey(config);
                state.SavePropertyConfig(EditorConfig);
                CheckIfEditorDirty();
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
                // Just select rather than bring into view for the moment
                // to avoid having the tree collapsed
                treeItem.IsSelected = true; 
                //SelectTreeItemAfterDelay(treeViewSaved, treeItem);
                RefreshEditorStatus();
                return newConfig;
            }
        }

        public void CopyInstalledPropertyToEditor()
        {
            LoadEditorConfig(SelectedInstalledProperty, BaselineType.Installed);
        }
        #endregion

        #region Command support

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

        public bool CanBeExported(TreeItem treeItem)
        {
            // Must exist and that's about it
            return HasDataFolder && treeItem != null;
        }

        public bool CanBeDeleted(TreeItem treeItem)
        {
            return GetTreeTargets(treeItem, out bool isSubTree)
                .Where(p => CanBeDeleted(p) && !IsDirtySubTreeConfig(p, isSubTree))
                .FirstOrDefault() != null;
        }

        public bool CanBeDeleted(PropertyConfig config)
        {
            return CanBeInstalled(config) &&
                state.DictSavedProperties.ContainsKey(config.CanonicalName);
        }

        public bool CanBeInstalled(TreeItem treeItem)
        {
            return GetTreeTargets(treeItem, out bool isSubTree)
                .Where(p => CanBeInstalled(p) && !IsDirtySubTreeConfig(p, isSubTree))
                .FirstOrDefault() != null;
        }

        public bool CanBeInstalled(PropertyConfig config)
        {
            return HasDataFolder && config != null &&
                ValidateName(config.CanonicalName) == null &&
                !state.DictInstalledProperties.ContainsKey(config.CanonicalName);
        }

        public bool CanBeUninstalled(TreeItem treeItem)
        {
            return GetTreeTargets(treeItem, out bool isSubTree)
                .Where(p => CanBeUninstalled(p))
                .FirstOrDefault() != null;
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

            var items = GetTreeTargets(treeItem, out bool isSubTree);

            if (!isSubTree)
                configName = items.First().CanonicalName;
            else
                configName = treeItem.Path;

            doc = PropertyConfig.GetPropDesc(items);

            var fileName = $"{Extensions.FixFileName(configName)}.propdesc";
            doc.Save(state.DataFolder + $@"\{fileName}");

            return fileName;
        }

        public int DeleteProperties(TreeItem treeItem)
        {
            var configs = GetTreeTargets(treeItem, out bool isSubTree)
                .Where(p => CanBeDeleted(p) && !IsDirtySubTreeConfig(p, isSubTree)).ToList();

            if (isSubTree)
            {
                if (MessageBox.Show($"This will delete {PropertyNameListQ(configs)}",
                    "Delete Properties", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return 0;
            }

            foreach (var config in configs)
                DeleteProperty(config);

            return configs.Count();
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

        // Returns a tuple of success and failure counts (property matching not in target Windows .Net)
        public Tuple<int, int> InstallProperties(TreeItem treeItem, TreeView treeViewInstalled)
        {
            var configs = GetTreeTargets(treeItem, out bool isSubTree)
                .Where(p => CanBeInstalled(p) && !IsDirtySubTreeConfig(p, isSubTree)).ToList();

            if (isSubTree)
            {
                if (MessageBox.Show($"This will install {PropertyNameListQ(configs)}",
                    "Install Properties", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return new Tuple<int, int>(0, 0);
            }

            int good = 0;
            foreach (var config in configs)
            {
                if (InstallProperty(config, isSubTree ? null : treeViewInstalled) > 0)
                    good++;
            }

            return new Tuple<int, int>(good, configs.Count() - good);
        }
        
        public int InstallEditorProperty(TreeView treeViewSaved, TreeView treeViewInstalled)
        {
            // Save as XML and update state and tree as necessary
            var config = SaveEditorProperty(treeViewSaved);

            return InstallProperty(config, treeViewInstalled);
        }

        // Returns negative numbers for failure, positive for success
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
                if (treeViewInstalled != null)
                    SelectTreeItemAfterDelay(treeViewInstalled, treeItem);
                ShowIfSavedIsInstalled(installedConfig.CanonicalName);
                RefreshEditorStatus();
            }

            return result;
        }

        public int UninstallProperties(TreeItem treeItem)
        {
            var configs = GetTreeTargets(treeItem, out bool isSubTree)
                .Where(p => CanBeUninstalled(p)).ToList();

            if (isSubTree)
            {
                if (MessageBox.Show($"This will uninstall {PropertyNameListQ(configs)}",
                    "Uninstall Properties", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return 0;
            }

            foreach (var config in configs)
                UninstallProperty(config);

            return configs.Count();
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
        #endregion

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
                if (CompareEditorToBaseline(allowOverride: false))
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

        private string PropertyNameListQ(List<PropertyConfig> configs)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < configs.Count; i++)
            {
                sb.Append(configs[i].CanonicalName);
                if (i < configs.Count - 2)
                    sb.Append(", ");
                else if (i == configs.Count - 2)
                    sb.Append(" and ");
            }
            sb.AppendLine("");
            sb.AppendLine("Do you wish to continue?");
            return sb.ToString();
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

        private bool IsDirtySubTreeConfig(PropertyConfig config, bool isSubTree)
        {
            // Check if this is a subtree property which is open and changed in the editor 
            return isSubTree && EditorConfig.CanonicalName == config.CanonicalName && IsEditorDirty;
        }

        private void CheckIfEditorDirty()
        {
            if (!IsBulkUpdating)
                IsEditorDirty = CompareEditorToBaseline();
        }

        private bool CompareEditorToBaseline(bool allowOverride = true)
        {
            var baseline = EditorBaseline;
            var isInstalled = (EditorBaselineType == BaselineType.Installed);

            // Check for comparison override
            if (allowOverride)
            {
                if (CompareSaved && EditorBaselineType == BaselineType.Installed)
                    baseline = SelectedSavedProperty;
                else if (CompareInstalled && EditorBaselineType != BaselineType.Installed)
                {
                    baseline = SelectedInstalledProperty;
                    isInstalled = true;
                }
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
                var siblingConfigs = parent.Children.Select(c => c.Item).OfType<PropertyConfig>()
                                           .Where(p => p != null).ToList();
                if (siblingConfigs.Count > 0)
                {
                    config.FormatId = siblingConfigs[0].FormatId;
                    config.PropertyId = siblingConfigs.Select(p => p?.PropertyId).Max() + 1;
                    return;
                }
            }

            // No usable parent, give it a new name  
            // Start numbering at 2. 0 and 1 are reserved by the system
            config.FormatId = Guid.NewGuid();
            config.PropertyId = 2;
        }

        private IEnumerable<PropertyConfig> GetTreeTargets(TreeItem treeItem, out bool isSubTree)
        {
            IEnumerable<TreeItem> items;

            if (treeItem.Children.Count == 0)
            {
                items = new TreeItem[] { treeItem };
                isSubTree = false;
            }
            else
            {
                items = treeItem.Children.Flatten<TreeItem>(t => t.Children);
                isSubTree = true;
            }

            return items.Select(t => t.Item).Cast<PropertyConfig>().Where(s => s != null);
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
