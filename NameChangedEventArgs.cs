using System;

namespace CustomWindowsProperties
{
    public class NameChangedEventArgs : EventArgs
    {
        private readonly string newName;

        public NameChangedEventArgs(string newName)
        {
            this.newName = newName;
        }

        public virtual string NewName { get { return newName; } }
    }

    public delegate void NameChangedEventHandler(object sender, NameChangedEventArgs e);
}
