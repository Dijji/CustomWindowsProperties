using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CustomWindowsProperties
{
    class MainView
    {
        public List<TreeItem> SystemPropertyTree { get; } = new List<TreeItem>();
        public List<TreeItem> CustomPropertyTree { get; } = new List<TreeItem>();


        public void PopulatePropertyTrees(State state)
        {
            PopulatePropertyTree(state.SystemProperties, SystemPropertyTree, true);
            PopulatePropertyTree(state.CustomProperties, CustomPropertyTree, false);
        }

        private void PopulatePropertyTree(List<PropertyView> properties, List<TreeItem> treeItems, bool isSystem)
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
                if (isSystem && root.Name == "System")
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
                }
                else
                    treeItems.Add(root);
            }
        }

        // Top level entry point for the algorithm that builds the property name tree from an unordered sequence
        // of property names
        private TreeItem AddTreeItem(Dictionary<string, TreeItem> dict, List<TreeItem> roots, PropertyView pv)
        {
            Debug.Assert(pv.CanonicalName.Contains('.')); // Because the algorithm assumes that this is the case
            TreeItem ti = AddTreeItemInner(dict, roots, pv.CanonicalName, pv.DisplayName);
            ti.Item = pv;

            return ti;
        }

        // Recurse backwards through each term in the property name, adding tree items as we go,
        // until we join onto an existing part of the tree
        private TreeItem AddTreeItemInner(Dictionary<string, TreeItem> dict, List<TreeItem> roots,
            string name, string displayName = null)
        {
            TreeItem ti, parent;
            string parentName = FirstPartsOf(name);

            if (parentName != null)
            {
                if (!dict.TryGetValue(parentName, out parent))
                {
                    parent = AddTreeItemInner(dict, roots, parentName);
                    dict.Add(parentName, parent);
                }

                if (displayName != null)
                    ti = new TreeItem(String.Format("{0} ({1})", LastPartOf(name), displayName));
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
    }
}
