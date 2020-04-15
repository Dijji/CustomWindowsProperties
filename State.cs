// Copyright (c) 2013, 2020 Dijji, and released under Ms-PL.  This, with other relevant licenses, can be found in the root of this distribution.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace CustomWindowsProperties
{
    public class State
    {
        private List<TreeItem> allProperties = new List<TreeItem>();
        private List<string> groupProperties = new List<string>();
        //private SavedState savedState = new SavedState();

        public List<PropertyView> SystemProperties { get; } = new List<PropertyView>();
        public List<PropertyView> CustomProperties { get; } = new List<PropertyView>();
        public List<PropertyView> GroupProperties { get; } = new List<PropertyView>();


        public void PopulateProperties()
        {
            PopulatePropertyList(SystemProperties, PropertySystemNativeMethods.PropDescEnumFilter.PDEF_SYSTEM);
            PopulatePropertyList(CustomProperties, PropertySystemNativeMethods.PropDescEnumFilter.PDEF_NONSYSTEM);
        }

        private void PopulatePropertyList(List<PropertyView> propertyList,
                PropertySystemNativeMethods.PropDescEnumFilter filter)
        {
            propertyList.Clear();
            IPropertyDescriptionList propertyDescriptionList = null;
            IPropertyDescription propertyDescription = null;
            Guid guid = new Guid(ShellIIDGuid.IPropertyDescriptionList);

            try
            {
                var hr = PropertySystemNativeMethods.PSEnumeratePropertyDescriptions(
                            filter, ref guid, out propertyDescriptionList);
                if (hr >= 0)
                {
                    uint count;
                    propertyDescriptionList.GetCount(out count);
                    guid = new Guid(ShellIIDGuid.IPropertyDescription);

                    for (uint i = 0; i < count; i++)
                    {
                        propertyDescriptionList.GetAt(i, ref guid, out propertyDescription);

                        if (propertyDescription != null)
                        {
                            var shellProperty = new ShellPropertyDescription(propertyDescription);
                            propertyList.Add(new PropertyView(shellProperty));
                            shellProperty.Dispose(); // Releases propertyDescription
                            propertyDescription = null;
                        }
                    }
                }
            }
            finally
            {
                if (propertyDescriptionList != null)
                {
                    Marshal.ReleaseComObject(propertyDescriptionList);
                }
                if (propertyDescription != null)
                {
                    Marshal.ReleaseComObject(propertyDescription);
                }
            }
        }

        /*
        public void LoadSavedState(string savedStateFile)
        {
            var fiDefault = GetDefaultSavedStateInfo();

            // If a state file has been specified, use it 
            if (savedStateFile != null)
            {
                var fi = new FileInfo(savedStateFile);
                if (!fi.Exists)
                    throw new AssocMgrException
                    {
                        Description = String.Format(LocalizedMessages.MissingDefinitionsFile, savedStateFile),
                        Exception = null,
                        ErrorCode = WindowsErrorCodes.ERROR_FILE_NOT_FOUND
                    };

                savedState = LoadSavedState(fi); 

                // If it's not just the default one, remember that we used a non-default file
                if (String.Compare(savedStateFile, fiDefault.FullName, true) != 0)
                    nonDefaultStateFileLoaded = true;
            }
            else if (fiDefault.Exists)
            {
                savedState = LoadSavedState(fiDefault); 
            }
        }

        public void StoreSavedState()
        {
            StoreSavedStateAsDefault(savedState);
        }

        private FileInfo GetDefaultSavedStateInfo()
        {
            DirectoryInfo di = ObtainDataDirectory();
            return new FileInfo(di.FullName + @"\SavedState.xml");
        }

        private SavedState LoadSavedState(FileInfo fi)
        {
            try
            {
                XmlSerializer x = new XmlSerializer(typeof(SavedState));
                using (TextReader reader = new StreamReader(fi.FullName))
                {
                    return (SavedState)x.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                if (ex is AssocMgrException)
                    throw ex;
                else
                    throw new AssocMgrException { Description = LocalizedMessages.XmlParseError, Exception = ex, ErrorCode = WindowsErrorCodes.ERROR_XML_PARSE_ERROR };
            }
        }

        private void StoreSavedStateAsDefault(SavedState state)
        {
            var fi = GetDefaultSavedStateInfo();
            try
            {
                XmlSerializer x = new XmlSerializer(typeof(SavedState));
                using (TextWriter writer = new StreamWriter(fi.FullName))
                {
                    x.Serialize(writer, state);
                }
            }
            catch (Exception ex)
            {
                throw new AssocMgrException { Description = LocalizedMessages.XmlWriteError, Exception = ex, ErrorCode = WindowsErrorCodes.ERROR_XML_PARSE_ERROR };
            }
       }
       */

    }
}