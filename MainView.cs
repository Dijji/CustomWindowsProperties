// Copyright (c) 2020, Dijji, and released under Ms-PL.  This, with other relevant licenses, can be found in the root of this distribution.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Xml;
using FolderSelect;
using KellermanSoftware.CompareNetObjects;

namespace CustomWindowsProperties
{
    class MainView : INotifyPropertyChanged
    {
        private State state;

        // Dictionaries of the parents in the trees (does not include the terminals)
        private Dictionary<string, TreeItem> dictEditorTree = null;
        private Dictionary<string, TreeItem> dictInstalledTree = null;

        #region Public properties
        public ObservableCollection<TreeItem> InstalledPropertyTree { get; } = new ObservableCollection<TreeItem>();
        public ObservableCollection<TreeItem> EditorPropertyTree { get; } = new ObservableCollection<TreeItem>();

        public PropertyConfig SelectedEditorProperty
        {
            get { return selectedEditorProperty; }
            private set { selectedEditorProperty = value; }
        }
        private PropertyConfig selectedEditorProperty;

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
                else if (state.InstalledProperties.ContainsKey(canonicalName))
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
            EditorConfig.PropertyChanged += PropertyBeingEdited_PropertyChanged;
        }

        private void PropertyBeingEdited_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PropertyConfig.CanonicalName))
            {
                RefreshEditedStatus();
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
            dictEditorTree = PropertyTree.PopulatePropertyTree(state.EditorProperties,
                EditorPropertyTree, false);
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
                RefreshEditedStatus();
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
                SelectedEditorProperty = selectedProperty;
                if (SelectedEditorProperty != null)
                {
                    LoadEditorConfig(SelectedEditorProperty, BaselineType.Edited);
                }
            }

            CompareEditorToBaseline(); // in case there is an override 
            return selectedProperty;
        }

        public void RefreshEditedStatus()
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
                !state.InstalledProperties.ContainsKey(config.CanonicalName);
        }

        public bool CanBeDeleted(PropertyConfig config)
        {
            return CanBeInstalled(config) &&
                state.EditedProperties.ContainsKey(config.CanonicalName);
        }

        public bool CanBeExported(TreeItem treeItem)
        {
            // Must exist and be a leaf, or the immediate parent of leaves
            return HasDataFolder && treeItem != null &&
                (treeItem.Children.Count == 0 || treeItem.Children[0].Children.Count == 0);
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

        public void DeleteEditedProperty()
        {
            var canonicalName = EditorConfig.CanonicalName;

            if (SelectedEditorProperty != null &&
                SelectedEditorProperty.CanonicalName != canonicalName)
                MessageBox.Show("Save or discard changes first", "Cannot delete property");

            // Property is in the editor tree, but not installed
            state.DeletePropertyConfig(canonicalName);
            state.RemoveEditorProperty(canonicalName);
            PropertyTree.RemoveTreeItem(dictEditorTree, EditorPropertyTree, canonicalName);
            RefreshEditedStatus();
        }

        public void DiscardEditorChanges()
        {
            LoadEditorConfig(EditorBaseline, EditorBaselineType, true);
            CheckIfEditorDirty();
        }

        public PropertyConfig SaveEditedProperty()
        {
            if (state.EditedProperties.TryGetValue(EditorConfig.CanonicalName, out PropertyConfig config))
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

                if (newConfig.FormatId == Guid.Empty)
                {
                    // To do reuse format ID from sibling, if available
                    EditorConfig.FormatId = newConfig.FormatId = Guid.NewGuid();
                    EditorConfig.PropertyId = newConfig.PropertyId = 1;
                }

                state.SavePropertyConfig(EditorConfig);
                state.AddEditorProperty(newConfig);
                PropertyTree.AddTreeItem(dictEditorTree, EditorPropertyTree, newConfig);
                IsEditorDirty = false;
                RefreshEditedStatus();
                return newConfig;
            }
        }

        public int InstallEditedProperty()
        {
            // Save as XML and update state and tree as necessary
            var config = SaveEditedProperty();

            return InstallProperty(config);
        }

        public int InstallProperty(PropertyConfig config)
        {
            // Save as propdesc
            var doc = PropertyConfig.GetPropDesc(new PropertyConfig[] { config });
            var fullFileName = $"{state.DataFolder}{Path.DirectorySeparatorChar}{config.CanonicalName}.propdesc";
            doc.Save(fullFileName);

            // Attempt installation
            var result = state.RegisterCustomProperty(fullFileName, config);

            if (result >= 0)
            {
                // Property is new, need to clone it and add it in
                PropertyConfig newConfig = new PropertyConfig();
                newConfig.CopyFrom(config, false);
                state.AddInstalledProperty(newConfig);
                PropertyTree.AddTreeItem(dictInstalledTree, InstalledPropertyTree, newConfig);
                RefreshEditedStatus();
            }

            return result;
        }

        public bool UninstallEditedProperty()
        {
            var canonicalName = SelectedInstalledProperty.CanonicalName;

            // Attempt uninstall
            bool succeeded = state.UnregisterCustomProperty(canonicalName);

            if (succeeded)
            {
                state.RemoveInstalledProperty(canonicalName);
                PropertyTree.RemoveTreeItem(dictInstalledTree, InstalledPropertyTree, canonicalName);
                RefreshEditedStatus();
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
            else if (PropertyTree.NameContainsExistingName(name, EditorPropertyTree, true))
                return "Name clashes with edited property name";
            else if (PropertyTree.NameContainsExistingName(name, InstalledPropertyTree, false) &&
                     !PropertyTree.NameContainsExistingName(name, EditorPropertyTree, false))
                // All installed property names clash unless we installed it ourselves
                return "Name clashes with installed property name";
            else
                return null;
        }

        private enum BaselineType
        {
            Standalone,
            Edited,
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
            RefreshEditedStatus();

            switch (EditorBaselineType)
            {
                case BaselineType.Standalone:
                case BaselineType.Edited:
                    CompareSaved = true;
                    break;
                case BaselineType.Installed:
                    CompareInstalled = true;
                    break;
            }

            return true;
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
                baseline = SelectedEditorProperty;
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

            var compare = new CompareLogic();
            compare.Config.MaxDifferences = int.MaxValue;
            compare.Config.CompareChildren = false;
            var result = compare.Compare(baseline, EditorConfig);
            StringBuilder sb = null;
            bool different = false;

            if (!result.AreEqual)
            {
                sb = new StringBuilder();
                //sb.AppendLine("Differences are:");
                foreach (var d in result.Differences)
                {
                    if (!isInstalled ||
                        !PropertyConfig.InstalledExclusions.Contains(d.PropertyName))
                    {
                        sb.AppendLine($"{d.PropertyName} changed from '{d.Object1Value}' to '{d.Object2Value}'");
                        different = true;
                    }
                }
            }

            if (different)
                DifferencesText = sb.ToString();
            else
                DifferencesText = "No differences";

            return different;
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
