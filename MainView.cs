// Copyright (c) 2020, Dijji, and released under Ms-PL.  This, with other relevant licenses, can be found in the root of this distribution.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using FolderSelect;

namespace CustomWindowsProperties
{
    class MainView : INotifyPropertyChanged
    {
        private State state;

        public List<TreeItem> InstalledPropertyTree { get; } = new List<TreeItem>();
        public List<TreeItem> EditorPropertyTree { get; } = new List<TreeItem>();

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
                OnPropertyChanged(nameof(IsEditedInstalled));
                OnPropertyChanged(nameof(CanInstall));
                OnPropertyChanged(nameof(CanUninstall));
            }
        }

        public string IsEditedInstalled
        {
            get
            {
                if (!Extensions.IsValidPropertyName(PropertyBeingEdited.CanonicalName))
                    return "Invalid property name";
                else if (state.InstalledProperties.ContainsKey(PropertyBeingEdited.CanonicalName))
                    return "True";
                else
                    return "False";
            }
        }

        public bool CanExport { get { return state.DataFolder != null && SelectedTreeItem != null; } }

        public bool CanInstall { get { return IsEditedInstalled == "False"; } }

        public bool CanUninstall { get { return IsEditedInstalled == "True" &&
                        !state.InstalledProperties[PropertyBeingEdited.CanonicalName].IsSystemProperty; } }

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
                if (!isInstalled && SelectedEditorProperty != null)
                {
                    // to do checking of the edited property is dirty here
                    // and if so, let the user back out
                    PropertyBeingEdited.CopyFrom(selectedProperty, true);
                    OnPropertyChanged(nameof(IsEditedInstalled));
                }
            }
            return selectedProperty;
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
            PopulatePropertyTree(state.SystemProperties.Concat(state.CustomProperties),
                InstalledPropertyTree, true);
            PopulatePropertyTree(state.EditorProperties, EditorPropertyTree, false);
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

        private void PopulatePropertyTree(IEnumerable<PropertyConfig> properties, List<TreeItem> treeItems, bool isInstalled)
        {
            Dictionary<string, TreeItem> dict = new Dictionary<string, TreeItem>();
            List<TreeItem> roots = new List<TreeItem>();

            // Build tree based on property names
            foreach (var p in properties)
            {
                AddTreeItem(dict, roots, p);
            }

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

        // Top level entry point for the algorithm that builds the property name tree from an unordered sequence
        // of property names
        private TreeItem AddTreeItem(Dictionary<string, TreeItem> dict, List<TreeItem> roots, PropertyConfig pc)
        {
            Debug.Assert(pc.CanonicalName.Contains('.')); // Because the algorithm assumes that this is the case
            TreeItem ti = AddTreeItemInner(dict, roots, pc.CanonicalName, pc.DisplayName);
            ti.Item = pc;

            return ti;
        }

        // Recurse backwards through each term in the property name, adding tree items as we go,
        // until we join onto an existing part of the tree
        private TreeItem AddTreeItemInner(Dictionary<string, TreeItem> dict, List<TreeItem> roots,
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

                parent.AddChild(ti);
            }
            else
            {
                if (!dict.TryGetValue(name, out ti))
                {
                    ti = new TreeItem(name);
                    roots.Add(ti);
                }
            }

            return ti;
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
