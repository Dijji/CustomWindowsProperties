// Copyright (c) 2020, Dijji, and released under Ms-PL.  This, with other relevant licenses, can be found in the root of this distribution.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml;
using FolderSelect;

namespace CustomWindowsProperties
{
    class MainView : INotifyPropertyChanged
    {
        private State state;

        // Dictionaries of the parents in the trees (does not include the terminals)
        private Dictionary<string, TreeItem> dictEditorTree = null;
        private Dictionary<string, TreeItem> dictInstalledTree = null;

        public void Test ()
        {
            var t = dictEditorTree["A"];
            t = t.Children[0];
            SetSelectedItem(t, false);
            InstallEditedProperty();
        }

        public ObservableCollection<TreeItem> InstalledPropertyTree { get; } = new ObservableCollection<TreeItem>();
        public ObservableCollection<TreeItem> EditorPropertyTree { get; } = new ObservableCollection<TreeItem>();

        public event PropertyChangedEventHandler PropertyChanged;

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
                OnPropertyChanged(nameof(CanUninstall));
            }
        }
        private PropertyConfig selectedInstalledProperty;

        public TreeItem SelectedTreeItem
        {
            get { return selectedTreeItem; }
            private set { selectedTreeItem = value; OnPropertyChanged(nameof(CanExport)); }
        }
        private TreeItem selectedTreeItem;

        public PropertyConfig PropertyBeingEdited { get; set; }

        public MainView()
        {
            PropertyBeingEdited = new PropertyConfig();
            PropertyBeingEdited.PropertyChanged += PropertyBeingEdited_PropertyChanged;
        }

        private void PropertyBeingEdited_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PropertyConfig.CanonicalName))
            {
                RefreshEditedStatus();
            }
        }


        public string IsEditedInstalled
        {
            get
            {
                // Name has to be valid structurally and not part of the parent tree of another property
                if (!Extensions.IsValidPropertyName(PropertyBeingEdited.CanonicalName) ||
                    dictEditorTree.ContainsKey(PropertyBeingEdited.CanonicalName))
                    return "Invalid property name";
                else if (state.InstalledProperties.ContainsKey(PropertyBeingEdited.CanonicalName))
                    return "True";
                else
                    return "False";
            }
        }

        public bool CanExport { get { return HasDataFolder && SelectedTreeItem != null; } }

        public bool CanDelete
        {
            get
            {
                return HasDataFolder && IsEditedInstalled == "False" &&
                    state.EditedProperties.ContainsKey(PropertyBeingEdited.CanonicalName);
            }
        }

        public bool CanInstall { get { return HasDataFolder && IsEditedInstalled == "False"; } }

        public bool CanUninstall
        {
            get
            {
                return HasDataFolder && SelectedInstalledProperty != null &&
                    !SelectedInstalledProperty.IsSystemProperty;
            }
        }

        public bool CanCopy { get { return SelectedInstalledProperty != null; } }

        public bool IsInstalledPropertyVisible
        { get { return SelectedInstalledProperty != null && HelpText == null; } }


        public bool IsHelpVisible { get { return HelpText != null; } }

        public string HelpText
        {
            get { return helpText; }
            set
            {
                helpText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsInstalledPropertyVisible));
                OnPropertyChanged(nameof(IsHelpVisible));
            }
        }
        private string helpText;

        private bool HasDataFolder { get { return state.DataFolder != null; } }

        public PropertyConfig SetSelectedItem(TreeItem treeItem, bool isInstalled)
        {
            SelectedTreeItem = treeItem;
            OnPropertyChanged(nameof(CanExport));

            PropertyConfig selectedProperty;
            if (SelectedTreeItem != null && SelectedTreeItem.Item != null)
                selectedProperty = SelectedTreeItem.Item as PropertyConfig;
            else
                selectedProperty = null;

            if (isInstalled)
            {
                SelectedInstalledProperty = selectedProperty;
            }
            else
            {
                SelectedEditorProperty = selectedProperty;
                if (SelectedEditorProperty != null)
                {
                    // to do checking of the edited property is dirty here
                    // and if so, let the user back out
                    PropertyBeingEdited.CopyFrom(selectedProperty, false);
                    RefreshEditedStatus();
                }
            }
            return selectedProperty;
        }

        public void RefreshEditedStatus()
        {
            OnPropertyChanged(nameof(IsEditedInstalled));
            OnPropertyChanged(nameof(CanDelete));
            OnPropertyChanged(nameof(CanInstall));
            OnPropertyChanged(nameof(CanUninstall));
        }

        public void EditorFocusChanged(string tag)
        {
            if (tag == null)
                HelpText = null;
            else
                HelpText = $"Help for {tag}";
        }

        public void Populate(State state)
        {
            this.state = state;
            PropertyBeingEdited.SetDefaultValues();
            dictInstalledTree = PopulatePropertyTree(state.SystemProperties.Concat(state.CustomProperties),
                InstalledPropertyTree, true);
            dictEditorTree = PopulatePropertyTree(state.EditorProperties,
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
                OnPropertyChanged(nameof(CanExport));
            }
            return false;
        }

        public string ExportPropDesc()
        {
            TreeItem treeItem = SelectedTreeItem;
            if (treeItem != null)
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
            return null;
        }

        public void DeleteEditedProperty()
        {
            var canonicalName = PropertyBeingEdited.CanonicalName;

            if (SelectedEditorProperty != null &&
                SelectedEditorProperty.CanonicalName != canonicalName)
                MessageBox.Show("Save or discard changes first", "Cannot delete property");

            // Property is in the editor tree, but not installed
            state.DeletePropertyConfig(canonicalName);
            state.RemoveEditorProperty(canonicalName);
            RemoveTreeItem(dictEditorTree, EditorPropertyTree, canonicalName);
            RefreshEditedStatus();
        }


        public PropertyConfig SaveEditedProperty()
        {
            if (state.EditedProperties.TryGetValue(PropertyBeingEdited.CanonicalName, out PropertyConfig config))
            {
                // Property is already known about, just update its values
                config.CopyFrom(PropertyBeingEdited, false);
                state.SavePropertyConfig(PropertyBeingEdited);
                return config;
            }
            else
            {
                // Property is new, need to clone it and add it in
                PropertyConfig newConfig = new PropertyConfig();
                newConfig.CopyFrom(PropertyBeingEdited, false);

                if (newConfig.FormatId == Guid.Empty)
                {
                    // To do reuse format ID from sibling, if available
                    PropertyBeingEdited.FormatId = newConfig.FormatId = Guid.NewGuid();
                    PropertyBeingEdited.PropertyId = newConfig.PropertyId = 1;
                }

                state.SavePropertyConfig(PropertyBeingEdited);
                state.AddEditorProperty(newConfig);
                AddTreeItem(dictEditorTree, EditorPropertyTree, newConfig);
                RefreshEditedStatus();
                return newConfig;
            }
        }

        public int InstallEditedProperty()
        {
            // Save as XML and update state and tree as necessary
            var config = SaveEditedProperty();

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
                AddTreeItem(dictInstalledTree, InstalledPropertyTree, newConfig);
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
                RemoveTreeItem(dictInstalledTree, InstalledPropertyTree, canonicalName);
                RefreshEditedStatus();
            }

            return succeeded;
        }

        public void CopyInstalledPropertyToEditor()
        {
            PropertyBeingEdited.CopyFrom(SelectedInstalledProperty, true);
        }

        private Dictionary<string, TreeItem> PopulatePropertyTree(
            IEnumerable<PropertyConfig> properties, ObservableCollection<TreeItem> treeItems, bool isInstalled)
        {
            Dictionary<string, TreeItem> dict = new Dictionary<string, TreeItem>();

            // Build tree based on property names
            foreach (var p in properties)
            {
                AddTreeItem(dict, treeItems, p);
            }

            // Populating installed tree - do some surgery on the system properties
            if (isInstalled)
            {
                List<TreeItem> roots = new List<TreeItem>(treeItems);
                treeItems.Clear();

                // Wire trees to tree controls, tweaking the structure as we go
                TreeItem propGroup = null;
                foreach (TreeItem root in roots)
                {
                    if (isInstalled && root.Name == "System")
                    {
                        treeItems.Insert(0, root);

                        // Move property groups from root to their own list
                        propGroup = root.Children.Where(x => x.Name == "PropGroup").FirstOrDefault();
                        if (propGroup != null)
                        {
                            //    foreach (TreeItem ti in propGroup.Children)
                            //      GroupProperties.Add(ti.Name);
                            root.RemoveChild(propGroup);
                        }

                        // Move properties with names of the form System.* to their own subtree
                        var systemProps = new TreeItem("System.*");
                        treeItems.Insert(0, systemProps);

                        foreach (var ti in root.Children.Where(x => x.Children.Count == 0).ToList())
                        {
                            root.RemoveChild(ti);
                            systemProps.AddChild(ti);
                        }

                    }
                    else
                        treeItems.Add(root);
                }
            }

            return dict;
        }

        // Top level entry point for the algorithm that builds the property name tree from an unordered sequence
        // of property names
        private TreeItem AddTreeItem(Dictionary<string, TreeItem> dict, ObservableCollection<TreeItem> roots, PropertyConfig pc)
        {
            Debug.Assert(pc.CanonicalName.Contains('.')); // Because the algorithm assumes that this is the case
            TreeItem ti = AddTreeItemInner(dict, roots, pc.CanonicalName, pc.DisplayName);
            ti.Item = pc;

            return ti;
        }

        // Recurse backwards through each term in the property name, adding tree items as we go,
        // until we join onto an existing part of the tree
        private TreeItem AddTreeItemInner(Dictionary<string, TreeItem> dict, ObservableCollection<TreeItem> roots,
            string name, string displayName = null)
        {
            TreeItem ti;
            string parentName = FirstPartsOf(name);

            if (parentName != null)
            {
                if (!dict.TryGetValue(parentName, out TreeItem parent))
                {
                    parent = AddTreeItemInner(dict, roots, parentName);
                    dict.Add(parentName, parent);
                }

                if (displayName != null)
                    ti = new TreeItem($"{LastPartOf(name)} ({displayName})");
                else
                    ti = new TreeItem(LastPartOf(name));

                ti.Tag = name;
                parent.AddChild(ti);
            }
            else
            {
                if (!dict.TryGetValue(name, out ti))
                {
                    ti = new TreeItem(name);
                    ti.Tag = name;
                    roots.Add(ti);
                }
            }

            return ti;
        }

        private void RemoveTreeItem(Dictionary<string, TreeItem> dict, ObservableCollection<TreeItem> roots, string canonicalName)
        {
            // Takes care of the dictionary entries and the tree items
            RemoveTreeItemInner(dict, roots, canonicalName);
        }

        // Returns true if the sought after tree item has been found and removed
        private bool RemoveTreeItemInner(Dictionary<string, TreeItem> dict, ICollection<TreeItem> treeItems, string canonicalName)
        {
            TreeItem toRemove = null;
            foreach (var treeItem in treeItems)
            {
                if (treeItem.Children.Count == 0)
                {
                    if (treeItem.Item is PropertyConfig config && config.CanonicalName == canonicalName)
                    {
                        toRemove = treeItem;
                        break;
                    }
                }
                else if (RemoveTreeItemInner(dict, treeItem.Children, canonicalName))
                {
                    // If that parent is now empty, remove it too
                    if (treeItem.Children.Count == 0)
                    {
                        // Remove it from the dictionary of parents
                        dict.Remove(treeItem.Tag);
                        toRemove = treeItem;
                    }
                    else
                        return true;
                }
            }
            if (toRemove != null)
            {
                treeItems.Remove(toRemove);
                return true;
            }
            else
                return false;
        }


        private string FirstPartsOf(string name)
        {
            int index = name.LastIndexOf('.');
            return index >= 0 ? name.Substring(0, index) : null;
        }

        private string LastPartOf(string name)
        {
            int index = name.LastIndexOf('.');
            return index >= 0 ? name.Substring(index + 1) : name;
        }

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
