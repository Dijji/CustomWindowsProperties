// Copyright (c) 2020, Dijji, and released under Ms-PL.  This, with other relevant licenses, can be found in the root of this distribution.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace CustomWindowsProperties
{
    static class PropertyTree
    {
        public static Dictionary<string, TreeItem> PopulatePropertyTree(
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
                List<TreeItem> lastRoots = new List<TreeItem>();

                // Wire trees to tree controls, tweaking the structure as we go
                TreeItem propGroup = null;
                foreach (TreeItem root in 
                    roots)
                {
                    if (root.Name == "System")
                    {
                        lastRoots.Insert(0, root);

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
                        lastRoots.Insert(0, systemProps);

                        foreach (var ti in root.Children.Where(x => x.Children.Count == 0).ToList())
                        {
                            root.RemoveChild(ti);
                            systemProps.AddChild(ti);
                        }
                    }
                    else if (root.Name == "Microsoft")
                        lastRoots.Insert(0, root);
                    else
                        treeItems.Add(root);
                }

                foreach (var ti in lastRoots)
                    treeItems.Add(ti);
            }

            return dict;
        }

        public static bool NameContainsExistingName(string name, ObservableCollection<TreeItem> roots, bool isEditor)
        {
            return NameContainsExistingNameInner(name, roots, isEditor);
        }

        private static bool NameContainsExistingNameInner(string name, ICollection<TreeItem> treeItems, bool isEditor)
        {
            var part = FirstPartOf(name, out string remainder);
            var item = treeItems.Where(t => t.Name == part).FirstOrDefault();
            if (item != null)
            {
                if (isEditor)
                {
                    if ((item.Children.Count != 0 && remainder.Length == 0) ||  // Making a parent into a leaf
                        (item.Children.Count == 0 && remainder.Length > 0))     // Making a leap into a parent
                        return true;
                }
                else
                {
                    // Everything is a clash
                    return true;
                }

                if (item.Children.Count > 0)
                    return NameContainsExistingNameInner(remainder, item.Children, isEditor);
            }

            return false;
        }

        // Top level entry point for the algorithm that builds the property name tree from an unordered sequence
        // of property names
        public static TreeItem AddTreeItem(Dictionary<string, TreeItem> dict,
            ObservableCollection<TreeItem> roots, PropertyConfig pc, bool addRootTop = false)
        {
            Debug.Assert(pc.CanonicalName.Contains('.')); // Because the algorithm assumes that this is the case
            TreeItem ti = AddTreeItemInner(dict, roots, pc.CanonicalName, addRootTop, pc.DisplayName);
            ti.Item = pc;

            return ti;
        }

        // Recurse backwards through each term in the property name, adding tree items as we go,
        // until we join onto an existing part of the tree
        private static TreeItem AddTreeItemInner(Dictionary<string, TreeItem> dict, 
            ObservableCollection<TreeItem> roots,
            string name, bool addRootTop, string displayName = null)
        {
            TreeItem ti;
            string parentName = FirstPartsOf(name);

            if (parentName != null)
            {
                if (!dict.TryGetValue(parentName, out TreeItem parent))
                {
                    parent = AddTreeItemInner(dict, roots, parentName, addRootTop);
                    dict.Add(parentName, parent);
                }

                if (displayName != null && displayName.Length > 0)
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
                    ti = new TreeItem(name)
                    {
                        Tag = name
                    };
                    if (addRootTop)
                        roots.Insert(0, ti);
                    else
                        roots.Add(ti);
                }
            }

            return ti;
        }

        public static void RemoveTreeItem(Dictionary<string, TreeItem> dict, ObservableCollection<TreeItem> roots, string canonicalName)
        {
            // Takes care of the dictionary entries and the tree items
            RemoveTreeItemInner(dict, roots, canonicalName);
        }

        // Returns true if the sought after tree item has been found and removed
        private static bool RemoveTreeItemInner(Dictionary<string, TreeItem> dict, ICollection<TreeItem> treeItems, string canonicalName)
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

        private static string FirstPartOf(string name, out string remainder)
        {
            int index = name.IndexOf('.');
            remainder = index > 0 ? name.Substring(index + 1) : string.Empty;
            return index >= 0 ? name.Substring(0, index) : name;
        }

        public static string FirstPartsOf(string name)
        {
            int index = name.LastIndexOf('.');
            return index >= 0 ? name.Substring(0, index) : null;
        }

        private static string LastPartOf(string name)
        {
            int index = name.LastIndexOf('.');
            return index >= 0 ? name.Substring(index + 1) : name;
        }
    }
}
