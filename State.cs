// Copyright (c) 2013, 2020 Dijji, and released under Ms-PL.  This, with other relevant licenses, can be found in the root of this distribution.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Xml.Serialization;

namespace CustomWindowsProperties
{
    internal class State
    {
        private Options options = null;
      
        public List<PropertyConfig> SystemProperties { get; } = new List<PropertyConfig>();
        public List<PropertyConfig> CustomProperties { get; } = new List<PropertyConfig>();
        public List<PropertyConfig> SavedProperties { get; } = new List<PropertyConfig>();


        public Dictionary<string, PropertyConfig> DictInstalledProperties { get; } = new Dictionary<string, PropertyConfig>();

        public Dictionary<string, PropertyConfig> DictSavedProperties { get; } = new Dictionary<string, PropertyConfig>();


        public string DataFolder
        {
            get { return Options.DataFolder; }
            set { Options.DataFolder = value; SaveOptions(); }
        }


        private Options Options
        {
            get
            {
                if (options == null)
                    options = new Options();
                return options;
            }
        }

        private string OptionsFileName
        {
            get
            {
                return ApplicationDataFolder + Path.DirectorySeparatorChar + "Options.xml";
            }
        }

        private string ApplicationDataFolder
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "CustomWindowsProperties");
            }
        }

        public void LoadOptions()
        {
            if (File.Exists(OptionsFileName))
            {
                try
                {
                    XmlSerializer x = new XmlSerializer(typeof(Options));
                    using (TextReader reader = new StreamReader(OptionsFileName))
                    {
                        options = (Options)x.Deserialize(reader);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error reading saved options");
                }
            }
        }

        public void SaveOptions()
        {
            try
            {
                if (!Directory.Exists(ApplicationDataFolder))
                    Directory.CreateDirectory(ApplicationDataFolder);

                XmlSerializer x = new XmlSerializer(typeof(Options));
                using (TextWriter writer = new StreamWriter(OptionsFileName))
                {
                    x.Serialize(writer, Options);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error saving chosen options");
            }
        }

        public PropertyConfig LoadPropertyConfig(string fullFileName)
        {
            if (File.Exists(fullFileName))
            {
                try
                {
                    XmlSerializer x = new XmlSerializer(typeof(PropertyConfig));
                    using (TextReader reader = new StreamReader(fullFileName))
                    {
                        return (PropertyConfig)x.Deserialize(reader);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, $"Error reading saved property {fullFileName}");
                }
            }

            return null;
        }

        public void SavePropertyConfig(PropertyConfig config)
        {
            try
            {
                string fileName = DataFolder + Path.DirectorySeparatorChar + config.CanonicalName + ".xml";

                XmlSerializer x = new XmlSerializer(typeof(PropertyConfig));
                using (TextWriter writer = new StreamWriter(fileName))
                {
                    x.Serialize(writer, config);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"Error saving property configuration {config.CanonicalName}");
            }
        }

        public void DeletePropertyConfig(string canonicalName)
        {
            try
            {
                string fileName = DataFolder + Path.DirectorySeparatorChar + canonicalName + ".xml";
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, $"Error deleting property configuration {canonicalName}");
            }
        }



        public void Populate()
        {
            LoadOptions();
            PopulatePropertyList(SystemProperties, PropertySystemNativeMethods.PropDescEnumFilter.PDEF_SYSTEM);
            PopulatePropertyList(CustomProperties, PropertySystemNativeMethods.PropDescEnumFilter.PDEF_NONSYSTEM);
            LoadSavedProperties();
        }

        public void AddSavedProperty(PropertyConfig config)
        {
            SavedProperties.Add(config);
            DictSavedProperties[config.CanonicalName] = config;
        }

        public void RemoveSavedProperty(string canonicalName)
        {
            var index = SavedProperties.FindIndex(p => p.CanonicalName == canonicalName);
            if (index != -1)
                SavedProperties.RemoveAt(index);
            DictSavedProperties.Remove(canonicalName);
        }

        public void AddInstalledProperty(PropertyConfig config)
        {
            CustomProperties.Add(config);
            DictInstalledProperties[config.CanonicalName] = config;
        }

        public void RemoveInstalledProperty(string canonicalName)
        {
            var index = CustomProperties.FindIndex(p => p.CanonicalName == canonicalName);
            if (index != -1)
                CustomProperties.RemoveAt(index);
            DictInstalledProperties.Remove(canonicalName);
        }

        // Returns negative numbers for failure, positive for success
        public int RegisterCustomProperty(string fullFileName, PropertyConfig pc, out PropertyConfig installedConfig)
        {
            installedConfig = null;
            FileInfo fi = new FileInfo(fullFileName);
            if (!fi.Exists)
                throw new Exception($"Installed property configuration file {fullFileName} is missing");

            // Copy the file into a more protected common area away from the editor
            var targetFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "CustomWindowsProperties");
            if (!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder);
            var targetFileName = $"{targetFolder}{Path.DirectorySeparatorChar}{fi.Name}";
            File.Copy(fullFileName, targetFileName, true);

            var result = PropertySystemNativeMethods.PSRegisterPropertySchema(targetFileName);

            if (result == 0 || result == 0x000401A0) // INPLACE_S_TRUNCATED
            {
                // Read back what was actually installed
                installedConfig = GetInstalledProperty(pc);
                if (installedConfig != null)
                {
                    if (result == 0)
                        return 0;
                    else
                    {
                        //MessageBox.Show("Property configuration was installed by Windows, but not all sections could be used. " + 
                        //    "There may be more information in the Application event log.",
                        // "Partial installation");
                        return 1;
                    }
                }
                else
                    MessageBox.Show("Property configuration was rejected by Windows. There may be more information in the Application event log.",
                     "Error installing property");
            }
            else
                MessageBox.Show($"Property registration failed with error code 0x{result:x}", "Error installing property");

            return -1;
        }

        private PropertyConfig GetInstalledProperty(PropertyConfig pc, bool skipBasics = false)
        {
            try
            {
                PropertyConfig installed = null;
                var key = new PropertyKey(pc.FormatId, (int)pc.PropertyId);
                var guidDescription = new Guid(ShellIIDGuid.IPropertyDescription);

                if (!skipBasics)
                {
                    var hr = PropertySystemNativeMethods.PSGetPropertyDescription(
                                ref key, ref guidDescription, out IPropertyDescription propertyDescription);

                    if (hr >= 0)
                    {
                        var shellProperty = new ShellPropertyDescription(propertyDescription);
                        installed = new PropertyConfig(shellProperty);
                        shellProperty.Dispose(); // Releases propertyDescription
                    }
                }
                else
                    installed = pc; // Continue populating existing config

                // All attempts to read search properties using IPropertyDescriptionSearchInfo
                // have failed  to date, so leave that code commented out
                /*
                if (installed != null)
                {
                    var guidSearch = new Guid(ShellIIDGuid.IPropertyDescriptionSearchInfo);

                    var hr = PropertySystemNativeMethods.PSGetPropertyDescription(
                                ref key, ref guidSearch, out IPropertyDescriptionSearchInfo propSearchInfo);

                    if (hr >= 0)
                    {
                        hr = propSearchInfo.GetSearchInfoFlags(out PropertySearchInfoFlags searchOptions);
                        if (hr >= 0)
                        {
                            installed.InInvertedIndex = searchOptions.HasFlag(PropertySearchInfoFlags.InInvertedIndex);
                            installed.IsColumn = searchOptions.HasFlag(PropertySearchInfoFlags.IsColumn);
                            installed.IsColumnSparse = searchOptions.HasFlag(PropertySearchInfoFlags.IsColumnSparse);
                            installed.AlwaysInclude = searchOptions.HasFlag(PropertySearchInfoFlags.AlwaysInclude);
                            installed.UseForTypeAhead = searchOptions.HasFlag(PropertySearchInfoFlags.UseForTypeAhead);
                        }
                        hr = propSearchInfo.GetColumnIndexType(out ColumnIndexType ppType);
                        if (hr >= 0) installed.ColumnIndexType = ppType;
                        // Just the canonical name again
                        hr = propSearchInfo.GetProjectionString(out IntPtr namePtr); 
                        if (CoreErrorHelper.Succeeded(hr) && namePtr != IntPtr.Zero)
                        {
                            string displayName = Marshal.PtrToStringUni(namePtr);
                            Marshal.FreeCoTaskMem(namePtr);
                        }
                        hr = propSearchInfo.GetMaxSize(out uint maxSize);
                        if (hr >= 0) installed.MaxSize = maxSize;

                        Marshal.ReleaseComObject(propSearchInfo);
                    }

                    var guidAlias = new Guid(ShellIIDGuid.IPropertyDescriptionAliasInfo);

                    hr = PropertySystemNativeMethods.PSGetPropertyDescription(
                                ref key, ref guidAlias, out IPropertyDescriptionAliasInfo propAliasInfo);

                    if (hr >= 0)
                    {
                        hr = propAliasInfo.GetSortByAlias(guidDescription, out IPropertyDescription alias);
                        if (hr >= 0 && alias != null)
                        {
                            // To do use the information, if we ever get it

                            Marshal.ReleaseComObject(alias);
                        }

                        Marshal.ReleaseComObject(propAliasInfo);
                    }
                }
                */

                return installed;
            }
#pragma warning disable CS0168 // Variable is declared but never used
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            catch (Exception ex)
#pragma warning restore IDE0059 // Unnecessary assignment of a value
#pragma warning restore CS0168 // Variable is declared but never used
            {
                return null;
            }
        }
      
        public bool UnregisterCustomProperty(string canonicalName)
        {
            // The file is held in a more protected common area away from the editor
            var targetFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "CustomWindowsProperties");
            var targetFileName = $"{targetFolder}{Path.DirectorySeparatorChar}{canonicalName}.propdesc";
            if (!File.Exists(targetFileName))
                throw new Exception($"Installed property configuration file {targetFileName} is missing");

            var result = PropertySystemNativeMethods.PSUnregisterPropertySchema(targetFileName);

            if (result == 0)
            {
                File.Delete(targetFileName);
                return true;
            }

            MessageBox.Show($"Property unregistration failed with error code 0x{result:x}", "Error uninstalling property");

            return false;
        }


        private void PopulatePropertyList(List<PropertyConfig> propertyList,
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
                    propertyDescriptionList.GetCount(out uint count);
                    guid = new Guid(ShellIIDGuid.IPropertyDescription);

                    for (uint i = 0; i < count; i++)
                    {
                        propertyDescriptionList.GetAt(i, ref guid, out propertyDescription);

                        if (propertyDescription != null)
                        {
                            var shellProperty = new ShellPropertyDescription(propertyDescription);
                            var pc = new PropertyConfig(shellProperty);
                            shellProperty.Dispose(); // Releases propertyDescription
                            propertyDescription = null;
                            GetInstalledProperty(pc, true); // Add search and alias info
                            propertyList.Add(pc);
                            DictInstalledProperties.Add(pc.CanonicalName, pc);
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

        private void LoadSavedProperties()
        {
            if (DataFolder == null)
                return;

            var di = new DirectoryInfo(DataFolder);
            foreach (var fi in di.GetFiles().Where(f => f.Extension == ".xml"))
            {
                var pc = LoadPropertyConfig(fi.FullName);
                if (pc != null) // Occurs when property cannot be loaded
                {
                    SavedProperties.Add(pc);
                    DictSavedProperties.Add(pc.CanonicalName, pc);
                }
            }
        }
    }
}