// Copyright (c) 2020, Dijji, and released under Ms-PL.  This, with other relevant licenses, can be found in the root of this distribution.

using System;
using System.Runtime.InteropServices;

namespace CustomWindowsProperties
{

    public class PropertyView
    {
        public string FullName { get; set; }
        public string DisplayName { get; set; }

        internal PropertyView(ShellPropertyDescription propertyDescription)
        {
            FullName = propertyDescription.CanonicalName;
            DisplayName = propertyDescription.DisplayName;
        }
    }
}