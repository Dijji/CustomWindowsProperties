// Copyright (c) 2020, Dijji, and released under Ms-PL.  This, with other relevant licenses, can be found in the root of this distribution.

using System;
using System.Collections.Generic;
using System.Xml;

namespace CustomWindowsProperties
{
    [Serializable]
    public class PropertyConfig
    {
        // BASICS

        /// <summary>
        /// The case-sensitive name of a property as it is known to the system, 
        /// regardless of its localized name.
        /// </summary>
        public string CanonicalName { get; set; }

        /// <summary>
        /// A unique GUID for the property
        /// </summary>
        public Guid? FormatId { get; set; }

        /// <summary>
        ///  Property identifier (PID)
        /// </summary>
        public Int32? PropertyId { get; set; }

        // SEARCH

        /// <summary>
        /// Indicates whether the property value should be stored in the inverted index. 
        /// This lets end users perform full-text queries over the values of this property.
        /// The default is "false".
        /// </summary>
        public bool InInvertedIndex { get; set; }

        /// <summary>
        /// Indicates whether the property should also be stored in the Windows search database as a column,
        /// so that independent software vendors (ISVs) can create predicate-based queries
        /// (for example, "Select * Where "System.Title"='qqq'").
        /// Set to "true" to enable end users (or developers) to create predicate based queries on the property
        /// The default is "false".
        /// </summary>
        public bool IsColumn { get; set; }

        /// <summary>
        /// The default is "true". If the property is multi-valued, this attribute is always "true".
        /// </summary>
        public bool IsColumnSparse { get; set; }

        /// <summary>
        /// To optimize sorting and grouping, the Windows search engine can create secondary indexes
        /// for properties that have isColumn="true" and is only useful in such cases. 
        /// If the property tends to be sorted frequently by users, this attribute should be specified. 
        /// The default value is "OnDemand". The following values are valid.
        /// NotIndexed: Never build a value index.
        /// OnDisk: Build a value index by default for this property.
        /// OnDiskAll: Build a value index by default for this property, and if it is a vector property,
        /// also a value index for all concatenated vector values.
        /// OnDiskVector: Build a value index by default for the concatenated vector values.
        /// OnDemand: Only build value indices by demand, that is, only first time they are used for a query.
        /// </summary>
        public ColumnIndexType ColumnIndexType { get; set; }

        /// <summary>
        /// The maximum size, in bytes, allowed for a certain property that is stored in the Windows
        /// search database. The default is: 512 bytes
        /// Note that this maximum size is measured in bytes, not characters.
        /// The maximum number of characters depends on their encoding.
        /// </summary>
        public uint MaxSize { get; set; }

        /// <summary>
        /// A list of mnemonic values that can be used to refer to the property in search queries.
        /// The list is delimited with the '|' character.
        /// </summary>
        public string Mnemonics { get; set; }

        // LABEL

        /// <summary>
        /// Gets the display name of the property as it is shown in any user interface (UI).
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets the current sort description flags for the property, 
        /// which indicate the particular wordings of sort offerings.
        /// </summary>
        /// <remarks>The settings retrieved by this method are set 
        /// through the <c>sortDescription</c> attribute of the <c>labelInfo</c> 
        /// element in the property's .propdesc file.</remarks>
        public PropertySortDescription SortDescription { get; set; }

        /// <summary>
        /// Gets the localized display string that describes the current sort order.
        /// </summary>
        /// <param name="descending">Indicates the sort order should 
        /// reference the string "Z on top"; otherwise, the sort order should reference the string "A on top".</param>
        /// <returns>The sort description for this property.</returns>
        /// <remarks>The string retrieved by this method is determined by flags set in the 
        /// <c>sortDescription</c> attribute of the <c>labelInfo</c> element in the property's .propdesc file.</remarks>
#pragma warning disable IDE0060 // Remove unused parameter
        public string GetSortDescriptionLabel(bool descending)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            return null;
        }

        // To do HideLabel

        /// <summary>
        /// Gets the text used in edit controls hosted in various dialog boxes.
        /// </summary>
        public string EditInvitation { get; set; }


        // TYPE

        public PropertyTypes Type { get; set; }

        /// <summary>
        /// Gets the method used when a view is grouped by this property.
        /// </summary>
        /// <remarks>The information retrieved by this method comes from 
        /// the <c>groupingRange</c> attribute of the <c>typeInfo</c> element in the 
        /// property's .propdesc file.</remarks>
        public PropertyGroupingRange GroupingRange { get; set; }

        /// <summary>
        /// This property cannot be written to. 
        /// </summary>
        /// <remarks>
        /// This value is set by the isInnate attribute of the typeInfo element in the property's .propdesc file.
        /// </remarks>
        public bool IsInnate { get; set; }

        /// <summary>
        /// Used with an innate property (that is, a value calculated from other property values) to indicate that it can be deleted.  
        /// </summary>
        /// <remarks>
        /// Windows Vista with Service Pack 1 (SP1) and later.
        /// This value is used by the Remove Properties user interface (UI) to determine whether to display a check box next to an property that allows that property to be selected for removal.
        /// Note that a property that is not innate can always be purged regardless of the presence or absence of this flag.
        /// </remarks>
        public bool CanBePurged { get; set; }

        /// <summary>
        /// The property can have multiple values.   
        /// </summary>
        /// <remarks>
        /// These values are stored as a VT_VECTOR in the PROPVARIANT structure.
        /// This value is set by the multipleValues attribute of the typeInfo element in the property's .propdesc file.
        /// </remarks>
        public bool MultipleValues { get; set; }

        /// <summary>
        /// The property is a group heading. 
        /// </summary>
        /// <remarks>
        /// This value is set by the isGroup attribute of the typeInfo element in the property's .propdesc file.
        /// </remarks>
        public bool IsGroup { get; set; }

        /// <summary>
        /// Gets a value that describes how the property values are displayed when 
        /// multiple items are selected in the user interface (UI).
        /// </summary>
        public PropertyAggregationType AggregationType { get; set; }

        /// <summary>
        /// The user can group by this property. 
        /// </summary>
        /// <remarks>
        /// This value is set by the canGroupBy attribute of the typeInfo element in the property's .propdesc file.
        /// </remarks>
      //  public bool CanGroupBy { get; set; }

        /// <summary>
        /// The user can stack by this property. 
        /// </summary>
        /// <remarks>
        /// This value is set by the canStackBy attribute of the typeInfo element in the property's .propdesc file.
        /// </remarks>
        //public bool CanStackBy { get; set; }

        /// <summary>
        /// This property contains a hierarchy. 
        /// </summary>
        /// <remarks>
        /// This value is set by the isTreeProperty attribute of the typeInfo element in the property's .propdesc file.
        /// </remarks>
        public bool IsTreeProperty { get; set; }

        /// <summary>
        /// This property is meant to be viewed by the user.  
        /// </summary>
        /// <remarks>
        /// This influences whether the property shows up in the "Choose Columns" dialog, for example.
        /// This value is set by the isViewable attribute of the typeInfo element in the property's .propdesc file.
        /// </remarks>
        public bool IsViewable { get; set; }

        // To do source of value
        public bool SearchRawValue { get; set; }

        /// <summary>
        /// Gets the condition type to use when displaying the property in 
        /// the query builder user interface (UI). This influences the list 
        /// of predicate conditions (for example, equals, less than, and 
        /// contains) that are shown for this property.
        /// </summary>
        /// <remarks>For more information, see the <c>conditionType</c> attribute 
        /// of the <c>typeInfo</c> element in the property's .propdesc file.</remarks>
        public PropertyConditionType ConditionType { get; set; }

        /// <summary>
        /// Gets the default condition operation to use 
        /// when displaying the property in the query builder user 
        /// interface (UI). This influences the list of predicate conditions 
        /// (for example, equals, less than, and contains) that are shown 
        /// for this property.
        /// </summary>
        /// <remarks>For more information, see the <c>conditionType</c> attribute of the 
        /// <c>typeInfo</c> element in the property's .propdesc file.</remarks>
        public PropertyConditionOperation ConditionOperation { get; set; }

        // DISPLAY

        //To do lots of things about how the value is set

        /// <summary>
        /// Gets the current data type used to display the property.
        /// </summary>
        public PropertyDisplayType DisplayType { get; set; }

        /// <summary>
        /// Gets the default user interface (UI) column width for this property.
        /// </summary>
        public uint DefaultColumWidth { get; set; }


        /// <summary>
        /// The control used to edit the property.
        /// </summary>
        public EditControl EditControl { get; set; }

        // OTHER

        /// <summary>
        /// This property is owned by the system.
        /// </summary>
        public bool IsSystemProperty { get; set; }

        /// <summary>
        /// Gets the column state flag, which describes how the property 
        /// should be treated by interfaces or APIs that use this flag.
        /// </summary>
        //public PropertyColumnStateOptions ColumnState { get; set; }


        /// <summary>
        /// Gets the current set of flags governing the property's view.
        /// </summary>
        public PropertyViewOptions ViewFlags { get; set; }

        // For serialisation
        public PropertyConfig()
        {
        }

        internal PropertyConfig(ShellPropertyDescription propertyDescription)
        {
            // Basics
            CanonicalName = propertyDescription.CanonicalName;
            FormatId = propertyDescription.PropertyKey.FormatId;
            PropertyId = propertyDescription.PropertyKey.PropertyId;

            // Search information
            // Not held in property description, so set the defaults 
            InInvertedIndex = false;
            IsColumn = false;
            IsColumnSparse = true;
            ColumnIndexType = ColumnIndexType.OnDemand;
            MaxSize = 512;
            Mnemonics = null;

            // Label information
            DisplayName = propertyDescription.DisplayName;
            EditInvitation = propertyDescription.EditInvitation;
            SortDescription = propertyDescription.SortDescription;

            // Type information
            Type = PropertyUtils.VarEnumToPropertyType(propertyDescription.VarEnumType);
            GroupingRange = propertyDescription.GroupingRange;
            MultipleValues = propertyDescription.TypeFlags.HasFlag(PropertyTypeOptions.MultipleValues);
            IsInnate = propertyDescription.TypeFlags.HasFlag(PropertyTypeOptions.IsInnate);
            CanBePurged = propertyDescription.TypeFlags.HasFlag(PropertyTypeOptions.CanBePurged);
            IsGroup = propertyDescription.TypeFlags.HasFlag(PropertyTypeOptions.IsGroup);
            AggregationType = propertyDescription.AggregationTypes;
            //CanGroupBy = propertyDescription.TypeFlags.HasFlag(PropertyTypeOptions.CanGroupBy);
            //CanStackBy = propertyDescription.TypeFlags.HasFlag(PropertyTypeOptions.CanStackBy);
            IsTreeProperty = propertyDescription.TypeFlags.HasFlag(PropertyTypeOptions.IsTreeProperty);
            IsViewable = propertyDescription.TypeFlags.HasFlag(PropertyTypeOptions.IsViewable);
            IsSystemProperty = propertyDescription.TypeFlags.HasFlag(PropertyTypeOptions.IsSystemProperty);
            ConditionType = propertyDescription.ConditionType;
            ConditionOperation = propertyDescription.ConditionOperation;

            // Display information
            DisplayType = propertyDescription.DisplayType;
            DefaultColumWidth = propertyDescription.DefaultColumWidth;
            // To do are these just pre-Windows 7?
            //ColumnState = propertyDescription.ColumnState;

            // To do elements controlling editing the property

            // To do this GetRelativeDescription:  shell property Does not read it  from the interface

            // To do. See if these are still relevant
            ViewFlags = propertyDescription.ViewFlags;

        }

        internal void SetDefaultValues()
        {
            InInvertedIndex = false;
            IsColumn = false;
            IsColumnSparse = true;
            ColumnIndexType = ColumnIndexType.OnDemand;
            MaxSize = 512;
            Mnemonics = null;

            DisplayName = null;
            EditInvitation = null;
            SortDescription = PropertySortDescription.General;

            Type = PropertyTypes.Any;
            GroupingRange = PropertyGroupingRange.Discrete;
            IsInnate = false;
            CanBePurged = false;
            MultipleValues = false;
            IsGroup = false;
            AggregationType = PropertyAggregationType.Default;
            IsTreeProperty = false;
            IsViewable = false;
            ConditionType = PropertyConditionType.None;
            ConditionOperation = PropertyConditionOperation.Equal;
            //SearchRawValue = false;

            DisplayType = PropertyDisplayType.String;
            DefaultColumWidth = 20;
        }

        public static string Publisher { get; set; } = "Publisher";
        public static string Product { get; set; } = "Product";

        public static XmlDocument GetPropDesc(IEnumerable<PropertyConfig> properties)
        {
            var doc = new XmlDocument();
            var root = doc.CreateElement("schema");
            root.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            root.SetAttribute("xmlns", "http://schemas.microsoft.com/windows/2006/propertydescription");
            root.SetAttribute("schemaVersion", "1.0");
            doc.AppendChild(root);

            var list = doc.CreateElement("propertyDescriptionList");
            list.SetAttribute("publisher", Publisher);
            list.SetAttribute("product", Product);
            root.AppendChild(list);

            foreach (var property in properties)
                list.AppendChild(property.GetXmlPropertyDescription(doc));

            return doc;
        }

        /*
         <propertyDescription name="Microsoft.SDKSample.DirectoryLevel" formatID="{581CF603-2925-4acf-BB5A-3D3EB39EACD3}" propID="3">
            <description>Number of directory levels to this item.</description>
            <searchInfo inInvertedIndex="false" isColumn="false,
            <typeInfo canStackBy="false" type="Int32"/>
            <labelInfo label="Directory Level"/>
        </propertyDescription>
         */
        internal XmlElement GetXmlPropertyDescription(XmlDocument doc)
        {
            var desc = doc.CreateElement("propertyDescription");
            desc.SetAttribute("name", CanonicalName);
            desc.SetAttribute("formatID", FormatId?.ToString("B").ToUpper());
            desc.SetAttribute("propID", PropertyId.ToString());

            // Unfortunately, we have no search information for system properties
            if (!IsSystemProperty)
            {
                var search = doc.CreateElement("searchInfo");
                search.SetAttribute("inInvertedIndex", InInvertedIndex.ToString());
                if (IsColumn)
                {
                    search.SetAttribute("isColumn", IsColumn.ToString());
                    search.SetAttribute("isColumnSparse", IsColumnSparse.ToString());
                    search.SetAttribute("columnIndexType", ColumnIndexType.ToString());
                }
                if (MaxSize != 512)
                    search.SetAttribute("maxSize", MaxSize.ToString());
                if (Mnemonics != null && Mnemonics.Length > 0)
                    search.SetAttribute("mnemonics", Mnemonics);
                desc.AppendChild(search);
            }

            var label = doc.CreateElement("labelInfo");
            label.SetAttribute("label", DisplayName);
            label.SetAttribute("sortDescription", SortDescription.ToString());
            if (EditInvitation != null && EditInvitation.Length > 0)
                label.SetAttribute("invitationText", EditInvitation);
            //label.SetAttribute("hideLabel", HideLabel);
            desc.AppendChild(label);

            var type = doc.CreateElement("typeInfo");
            type.SetAttribute("type", Type.ToString());
            if (GroupingRange != PropertyGroupingRange.Discrete)
                type.SetAttribute("groupingRange", GroupingRange.ToString());
            if (IsInnate)
                type.SetAttribute("isInnate", IsInnate.ToString());
            if (IsInnate && CanBePurged)
                type.SetAttribute("canBePurged", CanBePurged.ToString());
            if (MultipleValues)
                type.SetAttribute("multipleValues", MultipleValues.ToString());
            if (IsGroup)
                type.SetAttribute("isGroup", IsGroup.ToString());
            if (AggregationType != PropertyAggregationType.Default)
                type.SetAttribute("aggregationType", AggregationType.ToString());
            if (IsTreeProperty)
                type.SetAttribute("isTreeProperty", IsTreeProperty.ToString());
            if (IsViewable)
                type.SetAttribute("isViewable", IsViewable.ToString());
            //if (SearchRawValue)
            //    type.SetAttribute("searchRawValue", SearchRawValue.ToString());
            if (ConditionType != PropertyConditionType.String)
                type.SetAttribute("conditionType", ConditionType.ToString());
            if (ConditionOperation != PropertyConditionOperation.Equal)
                type.SetAttribute("defaultOperation", ConditionOperation.ToString());
            desc.AppendChild(type);

            var display = doc.CreateElement("displayInfo");
            display.SetAttribute("displayType", DisplayType.ToString());
            if (DefaultColumWidth != 20)
                display.SetAttribute("defaultColumnWidth", DefaultColumWidth.ToString());
            if (EditControl != EditControl.Default)
            {
                var edit = doc.CreateElement("editControl");
                edit.SetAttribute("control", DisplayType.ToString());
                display.AppendChild(edit);
            }
            desc.AppendChild(display);

            return desc;

            //string S = $"<propertyDescription name=\"{CanonicalName}\" formatID=\"{FormatId.ToString("B").ToUpper()}\" propID=\"{PropertyId}\">" +
            //       $"<searchInfo inInvertedIndex=\"{EnableFullTextSearch}\" isColumn=\"{EnableSearchQueries}\"/>" +
            //       $"<typeInfo type=\"{Type}\" groupingRange=\"{GroupingRange}\" isInnate=\"{IsInnate}\"" +
            //        $" canBePurged=\"{CanBePurged}\" multipleValues=\"{MultipleValues}\"" + 
            //        $" isGroup=\"{IsGroup}\" aggregationType=\"{AggregationType}\"" + 
            //        $" isTreeProperty=\"{IsTreeProperty}\" isViewable=\"{IsViewable}\"" + 
            //        $" searchRawValue=\"false\" conditionType=\"{ConditionType}\" defaultOperation=\"{ConditionOperation}\"" + 
            //       $"/>" +
            //       $"<labelInfo label=\"{DisplayName}\" sortDescription=\"{SortDescription}\"" + 
            //        $" invitationText=\"{EditInvitation}\" hideLabel=\"false\"/>" +
            //       $"<displayInfo defaultColumnWidth=\"{DefaultColumWidth}\" displayType=\"{DisplayType}\"" +
            //        $" alignment=\"Left\" relativeDescriptionType=\"General\" defaultSortDirection=\"Ascending\"/>" +
            //       //$"<aliasInfo sortByAlias=\"{DisplayName}\" additionalSortByAliases=\"{SortDescription}\"" +
            //       "</ propertyDescription >";
        }

        internal void CopyFrom(PropertyConfig from, bool isSystem)
        {
            // Basics
            if (!isSystem)
                CanonicalName = from.CanonicalName;
            FormatId = null;
            PropertyId = null;

            // Search
            if (!isSystem)
            {
                InInvertedIndex = from.InInvertedIndex;
                IsColumn = from.IsColumn;
                IsColumnSparse = from.IsColumnSparse;
                ColumnIndexType = from.ColumnIndexType;
                MaxSize = from.MaxSize;
                Mnemonics = from.Mnemonics;
            }

            // Label
            DisplayName = from.DisplayName;
            SortDescription = from.SortDescription;
            EditInvitation = from.EditInvitation;

            // Type
            Type = from.Type;
            GroupingRange = from.GroupingRange;
            IsInnate = from.IsInnate;
            CanBePurged = from.CanBePurged;
            MultipleValues = from.MultipleValues;
            IsGroup = from.IsGroup;
            AggregationType = from.AggregationType;
            IsTreeProperty = from.IsTreeProperty;
            IsViewable = from.IsViewable;
            ConditionType = from.ConditionType;
            ConditionOperation = from.ConditionOperation;

            // Display
            DisplayType = from.DisplayType;
            DefaultColumWidth = from.DefaultColumWidth;
            EditControl = from.EditControl;
        }
    }
}