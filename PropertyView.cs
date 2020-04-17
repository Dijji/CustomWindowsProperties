// Copyright (c) 2020, Dijji, and released under Ms-PL.  This, with other relevant licenses, can be found in the root of this distribution.

using System;
using System.Runtime.InteropServices;
using System.Xml;

namespace CustomWindowsProperties
{
    public enum PropertyTypes
    {
        Any,
        Null,
        String,
        Boolean,
        Byte,
        Buffer,
        Int16,
        UInt16,
        Int32,
        UInt32,
        Int64,
        UInt64,
        Double,
        DateTime,
        Guid,
        Blob,
        Stream,
        Clipboard,
        Object,
    }

   

    public class PropertyView
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
        public Guid FormatId { get; set; }

        /// <summary>
        ///  Property identifier (PID)
        /// </summary>
        public Int32 PropertyId { get; set; }

        // SEARCH

        /// <summary>
        /// inInvertedIndex. 
        /// Indicates whether the property value should be stored in the inverted index. 
        /// This lets end users perform full-text queries over the values of this property.
        /// The default is "false".
        /// </summary>
        public bool EnableFullTextSearch { get; set; }

        /// <summary>
        /// isColumn
        /// Indicates whether the property should also be stored in the Windows search database as a column,
        /// so that independent software vendors (ISVs) can create predicate-based queries
        /// (for example, "Select * Where "System.Title"='qqq'").
        /// Set to "true" to enable end users (or developers) to create predicate based queries on the property
        /// The default is "false".
        /// </summary>
        public bool EnableSearchQueries { get; set; }

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
        public string GetSortDescriptionLabel(bool descending)
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

        // OTHER

        /// <summary>
        /// This property is owned by the system.
        /// </summary>
        public bool IsSystemProperty { get; set; }

        /// <summary>
        /// Gets the .NET system type for a value of this property, or
        /// null if the value is empty.
        /// </summary>
        public Type ValueType { get; set; }

        /// <summary>
        /// Gets the column state flag, which describes how the property 
        /// should be treated by interfaces or APIs that use this flag.
        /// </summary>
        //public PropertyColumnStateOptions ColumnState { get; set; }


        /// <summary>
        /// Gets the current set of flags governing the property's view.
        /// </summary>
        public PropertyViewOptions ViewFlags { get; set; }


        internal PropertyView(ShellPropertyDescription propertyDescription)
        {
            // Basics
            CanonicalName = propertyDescription.CanonicalName;
            FormatId = propertyDescription.PropertyKey.FormatId;
            PropertyId = propertyDescription.PropertyKey.PropertyId;

            // Search information
            // Not held in property description, so set the defaults 
            EnableFullTextSearch = false;
            EnableSearchQueries = false;
       
            // Label information
            DisplayName = propertyDescription.DisplayName;
            EditInvitation = propertyDescription.EditInvitation;
            SortDescription = propertyDescription.SortDescription;

            // Type information
            ValueType = propertyDescription.ValueType;
            // to do set Type from Value Type
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

            // To do. See if these are still relevant
            ViewFlags = propertyDescription.ViewFlags;

            // GetRelativeDescription: is it in the shell property?

        }

        internal void SetDefaultValues ()
        {
            EnableFullTextSearch = false;
            EnableSearchQueries = false;

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

        /*
         <propertyDescription name="Microsoft.SDKSample.DirectoryLevel" formatID="{581CF603-2925-4acf-BB5A-3D3EB39EACD3}" propID="3">
            <description>Number of directory levels to this item.</description>
            <searchInfo inInvertedIndex="false" isColumn="false,
            <typeInfo canStackBy="false" type="Int32"/>
            <labelInfo label="Directory Level"/>
        </propertyDescription>
         */
        internal XmlElement GetXmlPropertyDescription (XmlDocument doc)
        {
            var desc = doc.CreateElement("propertyDescription");
            desc.SetAttribute("name", CanonicalName);
            desc.SetAttribute("formatID", FormatId.ToString("B").ToUpper());
            desc.SetAttribute("propID", PropertyId.ToString());

            var search = doc.CreateElement("searchInfo");
            search.SetAttribute("inInvertedIndex", EnableFullTextSearch.ToString());
            search.SetAttribute("isColumn", EnableSearchQueries.ToString());
            // To do and can't index type and maybe is column sparse
            desc.AppendChild(search);

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



            // Pre Windows 7?
            // canStackBy=\"{CanStackBy}\" 
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
            return desc;
        }

        internal void CopyFrom(PropertyView from, bool isSystem)
        {
            CanonicalName = from.CanonicalName;
            DisplayName = from.DisplayName;
            FormatId = from.FormatId;
            PropertyId = from.PropertyId;
            EditInvitation = from.EditInvitation;
            ValueType = from.ValueType;
            DisplayType = from.DisplayType;
            DefaultColumWidth = from.DefaultColumWidth;
            AggregationType = from.AggregationType;
            
            GroupingRange = from.GroupingRange;
            SortDescription = from.SortDescription;
            // To do type flags
            ViewFlags = from.ViewFlags;
            
        }
    
    }
}