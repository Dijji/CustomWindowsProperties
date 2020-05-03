// Copyr itight (c) 2013, Dijji, and released under Ms-PL.  This, with other relevant licenses, can be found in the root of this distribution.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace CustomWindowsProperties
{
    public class TreeItem : INotifyPropertyChanged
    {
        string name = null;
        bool isSelected = false;
        TreeItem parent = null;
        readonly ObservableCollection<TreeItem> children = new ObservableCollection<TreeItem>();

        public event NameChangedEventHandler NameChanged;

        public TreeItem(string name, object item = null)
        {
            this.name = name;
            this.Item = item;
        }

        public string Name { get { return name; } }
        public object Item { get; set; }
        public string Tag { get; set; }
        public TreeItem Parent { get { return parent; } }
        public ObservableCollection<TreeItem> Children { get { return children; } }

        public Brush Background { get { return background; } set { background = value; OnPropertyChanged(); } }
        private Brush background = null;


        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Path
        {
            get
            {
                if (Parent == null)
                    return Name;
                else
                    return Parent.Path + "." + Name;
            }
        }

        public string EditableName
        {
            get
            {
                return Name;
            }
            set
            {
                NameChanged?.Invoke(this, new NameChangedEventArgs(value));
            }
        }

        public void AddChild(TreeItem child)
        {
            child.parent = this;
            Children.Add(child);
        }

        public void InsertChild(int index, TreeItem child)
        {
            child.parent = this;
            Children.Insert(index, child);
        }

        public void RemoveChild(TreeItem child)
        {
            child.parent = null;
            Children.Remove(child);
        }

        public void AbandonNameChange()
        {
            OnPropertyChanged(nameof(EditableName));
        }

        public void ChangeName(string newName)
        {
            name = newName;
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(EditableName));
        }

        public TreeItem Clone()
        {
            TreeItem clone = new TreeItem(this.Name, this.Item);
            foreach (var ti in this.Children)
                clone.AddChild(ti.Clone());
            return clone;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}
