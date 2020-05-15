// Copyright (c) 2020, Dijji, and released under Ms-PL.  This, with other relevant licenses, can be found in the root of this distribution.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Serialization;

namespace CustomWindowsProperties
{
    [Serializable]
    public class PropertyConfig : INotifyPropertyChanged
    {
        // BASICS

        /// <summary>
        /// The case-sensitive name of a property as it is known to the system, 
        /// regardless of its localized name.
        /// </summary>
        public string CanonicalName
        {
            get { return canonicalName; }
            set { canonicalName = value; OnPropertyChanged(); }
        }
        private string canonicalName;

        /// <summary>
        /// A unique GUID for the property
        /// </summary>
        public Guid FormatId { get; set; }

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
        public bool InInvertedIndex { get { return inInvertedIndex; } set { inInvertedIndex = value; OnPropertyChanged(); } }
        private bool inInvertedIndex;

        /// <summary>
        /// Indicates whether the property should also be stored in the Windows search database as a column,
        /// so that independent software vendors (ISVs) can create predicate-based queries
        /// (for example, "Select * Where "System.Title"='qqq'").
        /// Set to "true" to enable end users (or developers) to create predicate based queries on the property
        /// The default is "false".
        /// </summary>
        public bool IsColumn { get { return isColumn; } set { isColumn = value; OnPropertyChanged(); } }
        private bool isColumn;

        /// <summary>
        /// The default is "true". If the property is multi-valued, this attribute is always "true".
        /// </summary>
        public bool IsColumnSparse { get { return isColumnSparse; } set { isColumnSparse = value; OnPropertyChanged(); } }
        private bool isColumnSparse;

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
        public ColumnIndexType ColumnIndexType { get { return columnIndexType; } set { columnIndexType = value; OnPropertyChanged(); } }
        private ColumnIndexType columnIndexType;

        /// <summary>
        /// The maximum size, in bytes, allowed for a certain property that is stored in the Windows
        /// search database. The default is: 512 bytes
        /// Note that this maximum size is measured in bytes, not characters.
        /// The maximum number of characters depends on their encoding.
        /// </summary>
        public uint MaxSize { get { return maxSize; } set { maxSize = value; OnPropertyChanged(); } }
        private uint maxSize;

        /// <summary>
        /// A list of mnemonic values that can be used to refer to the property in search queries.
        /// The list is delimited with the '|' character.
        /// </summary>
        public string Mnemonics { get { return mnemonics; } set { mnemonics = value; OnPropertyChanged(); } }
        private string mnemonics;

        /// <summary>
        /// 
        /// </summary>
        public bool AlwaysInclude { get { return alwaysInclude; } set { alwaysInclude = value; OnPropertyChanged(); } }
        private bool alwaysInclude;

        /// <summary>
        /// 
        /// </summary>
        public bool UseForTypeAhead { get { return useForTypeAhead; } set { useForTypeAhead = value; OnPropertyChanged(); } }
        private bool useForTypeAhead;


        // LABEL

        /// <summary>
        /// The display name of the property as it is shown in any user interface (UI).
        /// </summary>
        public string DisplayName { get { return displayName; } set { displayName = value; OnPropertyChanged(); } }
        private string displayName;

        /// <summary>
        /// The current sort description flags for the property, 
        /// which indicate the particular wordings of sort offerings.
        /// </summary>
        /// <remarks>The settings retrieved by this method are set 
        /// through the <c>sortDescription</c> attribute of the <c>labelInfo</c> 
        /// element in the property's .propdesc file.</remarks>
        public PropertySortDescription SortDescription { get { return sortDescription; } set { sortDescription = value; OnPropertyChanged(); } }
        private PropertySortDescription sortDescription;

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

        /// <summary>
        /// The text used in edit controls hosted in various dialog boxes.
        /// </summary>
        public string EditInvitation { get { return editInvitation; } set { editInvitation = value; OnPropertyChanged(); } }
        private string editInvitation;

        public bool HideLabel { get { return hideLabel; } set { hideLabel = value; OnPropertyChanged(); } }
        private bool hideLabel;

        // TYPE

        public PropertyTypes Type { get { return type; } set { type = value; OnPropertyChanged(); } }
        private PropertyTypes type;

        /// <summary>
        /// Gets the method used when a view is grouped by this property.
        /// </summary>
        /// <remarks>The information retrieved by this method comes from 
        /// the <c>groupingRange</c> attribute of the <c>typeInfo</c> element in the 
        /// property's .propdesc file.</remarks>
        public PropertyGroupingRange GroupingRange { get { return groupingRange; } set { groupingRange = value; OnPropertyChanged(); } }
        private PropertyGroupingRange groupingRange;

        /// <summary>
        /// This property cannot be written to. 
        /// </summary>
        /// <remarks>
        /// This value is set by the isInnate attribute of the typeInfo element in the property's .propdesc file.
        /// </remarks>
        public bool IsInnate { get { return isInnate; } set { isInnate = value; OnPropertyChanged(); } }
        private bool isInnate;

        /// <summary>
        /// Used with an innate property (that is, a value calculated from other property values) to indicate that it can be deleted.  
        /// </summary>
        /// <remarks>
        /// Windows Vista with Service Pack 1 (SP1) and later.
        /// This value is used by the Remove Properties user interface (UI) to determine whether to display a check box next to an property that allows that property to be selected for removal.
        /// Note that a property that is not innate can always be purged regardless of the presence or absence of this flag.
        /// </remarks>
        public bool CanBePurged { get { return canBePurged; } set { canBePurged = value; OnPropertyChanged(); } }
        private bool canBePurged;

        /// <summary>
        /// The property can have multiple values.   
        /// </summary>
        /// <remarks>
        /// These values are stored as a VT_VECTOR in the PROPVARIANT structure.
        /// This value is set by the multipleValues attribute of the typeInfo element in the property's .propdesc file.
        /// </remarks>
        public bool MultipleValues { get { return multipleValues; } set { multipleValues = value; OnPropertyChanged(); } }
        private bool multipleValues;

        /// <summary>
        /// The property is a group heading. 
        /// </summary>
        /// <remarks>
        /// This value is set by the isGroup attribute of the typeInfo element in the property's .propdesc file.
        /// </remarks>
        public bool IsGroup { get { return isGroup; } set { isGroup = value; OnPropertyChanged(); } }
        private bool isGroup;

        /// <summary>
        /// Gets a value that describes how the property values are displayed when 
        /// multiple items are selected in the user interface (UI).
        /// </summary>
        public PropertyAggregationType AggregationType { get { return aggregationType; } set { aggregationType = value; OnPropertyChanged(); } }
        private PropertyAggregationType aggregationType;

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
        public bool IsTreeProperty { get { return isTreeProperty; } set { isTreeProperty = value; OnPropertyChanged(); } }
        private bool isTreeProperty;

        /// <summary>
        /// This property is meant to be viewed by the user.  
        /// </summary>
        /// <remarks>   
        /// This influences whether the property shows up in the "Choose Columns" dialog, for example.
        /// This value is set by the isViewable attribute of the typeInfo element in the property's .propdesc file.
        /// </remarks>
        public bool IsViewable { get { return isViewable; } set { isViewable = value; OnPropertyChanged(); } }
        private bool isViewable;

        // To do source of value
        public bool SearchRawValue { get { return searchRawValue; } set { searchRawValue = value; OnPropertyChanged(); } }
        private bool searchRawValue;

        /// <summary>
        /// Gets the condition type to use when displaying the property in 
        /// the query builder user interface (UI). This influences the list 
        /// of predicate conditions (for example, equals, less than, and 
        /// contains) that are shown for this property.
        /// </summary>
        /// <remarks>For more information, see the <c>conditionType</c> attribute 
        /// of the <c>typeInfo</c> element in the property's .propdesc file.</remarks>
        public PropertyConditionType ConditionType { get { return conditionType; } set { conditionType = value; OnPropertyChanged(); } }
        private PropertyConditionType conditionType;

        /// <summary>
        /// Gets the default condition operation to use 
        /// when displaying the property in the query builder user 
        /// interface (UI). This influences the list of predicate conditions 
        /// (for example, equals, less than, and contains) that are shown 
        /// for this property.
        /// Note that the installed enumeration differs from the configurable enumeration,
        /// hence the need for two versions of the property
        /// </summary>
        /// <remarks>For more information, see the <c>conditionType</c> attribute of the 
        /// <c>typeInfo</c> element in the property's .propdesc file.</remarks>
        public ConditionOperationConfigured ConditionOperation { get { return conditionOperation; } set { conditionOperation = value; OnPropertyChanged(); } }
        private ConditionOperationConfigured conditionOperation;

        public PropertyConditionOperation ConditionOperationInstalled { get { return conditionOperationInstalled; } set { conditionOperationInstalled = value; OnPropertyChanged(); } }
        private PropertyConditionOperation conditionOperationInstalled;

        // ALIAS

        /// <summary>
        /// 
        /// </summary>
        public string SortByAlias { get { return sortByAlias; } set { sortByAlias = value; OnPropertyChanged(); } }
        private string sortByAlias;

        // To do Consider adding additional aliases


        // DISPLAY

        /// <summary>
        /// The control used to edit the property.
        /// </summary>
        public EditControl EditControl { get { return editControl; } set { editControl = value; OnPropertyChanged(); } }
        private EditControl editControl;


        //To do Add support for missing pieces: more controls; enumerations; duration

        /// <summary>
        /// Gets the current data type used to display the property.
        /// </summary>
        public PropertyDisplayType DisplayType
        {
            get { return displayType; }
            set
            {
                displayType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsStringFormat));
                OnPropertyChanged(nameof(IsBooleanFormat));
                OnPropertyChanged(nameof(IsNumberFormat));
                OnPropertyChanged(nameof(IsDateTimeFormat));
            }
        }
        private PropertyDisplayType displayType;
        public bool IsStringFormat { get { return DisplayType == PropertyDisplayType.String; } }
        public bool IsBooleanFormat { get { return DisplayType == PropertyDisplayType.Boolean; } }
        public bool IsNumberFormat { get { return DisplayType == PropertyDisplayType.Number; } }
        public bool IsDateTimeFormat { get { return DisplayType == PropertyDisplayType.DateTime; } }

        public StringFormat StringFormat { get { return stringFormat; } set { stringFormat = value; OnPropertyChanged(); } }
        private StringFormat stringFormat;

        public BooleanFormat BooleanFormat { get { return booleanFormat; } set { booleanFormat = value; OnPropertyChanged(); } }
        private BooleanFormat booleanFormat;

        public NumberFormat NumberFormat { get { return numberFormat; } set { numberFormat = value; OnPropertyChanged(); } }
        private NumberFormat numberFormat;

        public DateTimeFormat DateTimeFormat { get { return dateTimeFormat; } set { dateTimeFormat = value; OnPropertyChanged(); } }
        private DateTimeFormat dateTimeFormat;

        /// <summary>
        /// Gets the default user interface (UI) column width for this property.
        /// </summary>
        public uint DefaultColumnWidth { get { return defaultColumnWidth; } set { defaultColumnWidth = value; OnPropertyChanged(); } }
        private uint defaultColumnWidth;

        /// <summary>
        /// 
        /// </summary>
        public PropertyAlignmentType Alignment { get { return alignment; } set { alignment = value; OnPropertyChanged(); } }
        private PropertyAlignmentType alignment;


        /// <summary>
        /// 
        /// </summary>
        public RelativeDescriptionType RelativeDescriptionType { get { return relativeDescriptionType; } set { relativeDescriptionType = value; OnPropertyChanged(); } }
        private RelativeDescriptionType relativeDescriptionType;


        /// <summary>
        /// 
        /// </summary>
        public SortDirection DefaultSortDirection { get { return defaultSortDirection; } set { defaultSortDirection = value; OnPropertyChanged(); } }
        private SortDirection defaultSortDirection;



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

        [XmlIgnore]
        public string BoundedName
        {
            get
            {
                return CanonicalName.Length < 20 ? CanonicalName :
                    $"{CanonicalName.Substring(0, 10)}...{CanonicalName.Substring(CanonicalName.Length - 10)}";
            }
        }

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
            HideLabel = propertyDescription.ViewFlags.HasFlag(PropertyViewOptions.HideLabel);

            // Type information
            Type = PropertyUtils.VarEnumToPropertyType(propertyDescription.VarEnumType);
            GroupingRange = propertyDescription.GroupingRange;
            MultipleValues = propertyDescription.TypeFlags.HasFlag(PropertyTypeOptions.MultipleValues);
            IsInnate = propertyDescription.TypeFlags.HasFlag(PropertyTypeOptions.IsInnate);
            CanBePurged = propertyDescription.TypeFlags.HasFlag(PropertyTypeOptions.CanBePurged);
            IsGroup = propertyDescription.TypeFlags.HasFlag(PropertyTypeOptions.IsGroup);
            AggregationType = propertyDescription.AggregationTypes;
            // Pre-Windows 7
            //CanGroupBy = propertyDescription.TypeFlags.HasFlag(PropertyTypeOptions.CanGroupBy);
            //CanStackBy = propertyDescription.TypeFlags.HasFlag(PropertyTypeOptions.CanStackBy);
            IsTreeProperty = propertyDescription.TypeFlags.HasFlag(PropertyTypeOptions.IsTreeProperty);
            IsViewable = propertyDescription.TypeFlags.HasFlag(PropertyTypeOptions.IsViewable);
            IsSystemProperty = propertyDescription.TypeFlags.HasFlag(PropertyTypeOptions.IsSystemProperty);
            ConditionType = propertyDescription.ConditionType;
            ConditionOperationInstalled = propertyDescription.ConditionOperation;

            // Display information
            DisplayType = propertyDescription.DisplayType;
            DefaultColumnWidth = propertyDescription.DefaultColumnWidth;
            if (propertyDescription.ViewFlags.HasFlag(PropertyViewOptions.CenterAlign))
                Alignment = PropertyAlignmentType.Center;
            else if (propertyDescription.ViewFlags.HasFlag(PropertyViewOptions.RightAlign))
                Alignment = PropertyAlignmentType.Right;
            else
                Alignment = PropertyAlignmentType.Left;
            RelativeDescriptionType = propertyDescription.RelativeDescriptionType;
            if (propertyDescription.ViewFlags.HasFlag(PropertyViewOptions.SortDescending))
                DefaultSortDirection = SortDirection.Descending;
            else
                DefaultSortDirection = SortDirection.Ascending;

            // Pre-Windows 7 displayInfo
            //ColumnState = propertyDescription.ColumnState;

            // To do elements controlling editing the property
            // They do not seem to be readable: we have EditControl so far

            // To do. See if these have any more juice in them
            ViewFlags = propertyDescription.ViewFlags;

        }

        internal void SetDefaultValues()
        {
            FormatId = Guid.Empty;
            PropertyId = 0;
            CanonicalName = string.Empty;

            InInvertedIndex = false;
            IsColumn = false;
            IsColumnSparse = true;
            ColumnIndexType = ColumnIndexType.OnDemand;
            MaxSize = 512;
            Mnemonics = null;

            DisplayName = null;
            SortDescription = PropertySortDescription.General;
            EditInvitation = null;
            HideLabel = false;

            Type = PropertyTypes.Any;
            GroupingRange = PropertyGroupingRange.Discrete;
            IsInnate = false;
            CanBePurged = false;
            MultipleValues = false;
            IsGroup = false;
            AggregationType = PropertyAggregationType.Default;
            IsTreeProperty = false;
            IsViewable = false;
            SearchRawValue = false;
            ConditionType = PropertyConditionType.None;
            ConditionOperation = ConditionOperationConfigured.Equal;

            DisplayType = PropertyDisplayType.String;
            StringFormat = StringFormat.General;
            BooleanFormat = BooleanFormat.YesNo;
            NumberFormat = NumberFormat.General;
            DateTimeFormat = DateTimeFormat.General;
            DefaultColumnWidth = 20;
            Alignment = PropertyAlignmentType.Left;
            RelativeDescriptionType = RelativeDescriptionType.General;
            DefaultSortDirection = SortDirection.Ascending;
            EditControl = EditControl.Default;
        }

        public static string Publisher { get; set; } = "Publisher";
        public static string Product { get; set; } = "Product";

        internal static XmlDocument GetPropDesc(IEnumerable<PropertyConfig> properties)
        {
            var doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", null, null));
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
            desc.SetAttribute("formatID", FormatId.ToString("B").ToUpper());
            desc.SetAttribute("propID", PropertyId.ToString());

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

            var label = doc.CreateElement("labelInfo");
            label.SetAttribute("label", DisplayName);
            label.SetAttribute("sortDescription", SortDescription.ToString());
            if (EditInvitation != null && EditInvitation.Length > 0)
                label.SetAttribute("invitationText", EditInvitation);
            if (HideLabel)
                label.SetAttribute("hideLabel", HideLabel.ToString());
            desc.AppendChild(label);

            var type = doc.CreateElement("typeInfo");
            type.SetAttribute("type", Type.ToString());
            if (GroupingRange != PropertyGroupingRange.Discrete)
                type.SetAttribute("groupingRange", GroupingRange.ToString());
            if (IsInnate)
                type.SetAttribute("isInnate", IsInnate.ToString());
            if (IsInnate && CanBePurged)
                type.SetAttribute("canBePurged", CanBePurged.ToString());
            //if (MultipleValues)
                type.SetAttribute("multipleValues", MultipleValues.ToString());
            if (IsGroup)
                type.SetAttribute("isGroup", IsGroup.ToString());
            if (AggregationType != PropertyAggregationType.Default)
                type.SetAttribute("aggregationType", AggregationType.ToString());
            if (IsTreeProperty)
                type.SetAttribute("isTreeProperty", IsTreeProperty.ToString());
            if (IsViewable)
                type.SetAttribute("isViewable", IsViewable.ToString());
            if (SearchRawValue)
                type.SetAttribute("searchRawValue", SearchRawValue.ToString());
            if (ConditionType != PropertyConditionType.String)
                type.SetAttribute("conditionType", ConditionType.ToString());
            if (ConditionOperation != ConditionOperationConfigured.Equal)
                type.SetAttribute("defaultOperation", ConditionOperation.ToString());
            desc.AppendChild(type);

            var display = doc.CreateElement("displayInfo");
            display.SetAttribute("displayType", DisplayType.ToString());
            if (DisplayType == PropertyDisplayType.String && StringFormat != StringFormat.General)
            {
                var format = doc.CreateElement("stringFormat");
                format.SetAttribute("formatAs", StringFormat.ToString());
                display.AppendChild(format);
            }
            if (DisplayType == PropertyDisplayType.Boolean && BooleanFormat != BooleanFormat.YesNo)
            {
                var format = doc.CreateElement("booleanFormat");
                format.SetAttribute("formatAs", BooleanFormat.ToString());
                display.AppendChild(format);
            }
            if (DisplayType == PropertyDisplayType.Number && NumberFormat != NumberFormat.General)
            {
                var format = doc.CreateElement("numberFormat");
                format.SetAttribute("formatAs", NumberFormat.ToString());
                display.AppendChild(format);
            }
            if (DisplayType == PropertyDisplayType.DateTime && DateTimeFormat != DateTimeFormat.General)
            {
                var format = doc.CreateElement("dateTimeFormat");
                format.SetAttribute("formatAs", DateTimeFormat.ToString());
                display.AppendChild(format);
            }



            if (DefaultColumnWidth != 20)
                display.SetAttribute("defaultColumnWidth", DefaultColumnWidth.ToString());
            if (Alignment != PropertyAlignmentType.Left)
                display.SetAttribute("alignment", Alignment.ToString());
            //if (RelativeDescriptionType != RelativeDescriptionType.General)
            //  display.SetAttribute("relativeDescriptionType", RelativeDescriptionType.ToString());
            if (DefaultSortDirection != SortDirection.Ascending)
                display.SetAttribute("defaultSortDirection", DefaultSortDirection.ToString());
            if (EditControl != EditControl.Default)
            {
                var edit = doc.CreateElement("editControl");
                edit.SetAttribute("control", EditControl.ToString());
                display.AppendChild(edit);
            }
            desc.AppendChild(display);

            return desc;
        }

        internal static HashSet<string> InstalledExclusions = new HashSet<string>
            { nameof(CanonicalName), nameof(FormatId), nameof(PropertyId),
              nameof(InInvertedIndex), nameof(IsColumn), nameof(IsColumnSparse),
              nameof(ColumnIndexType), nameof(MaxSize), nameof(Mnemonics),
              nameof(BoundedName), nameof(IsSystemProperty),
            };

        internal void CopyFrom(PropertyConfig from, bool isInstalled)
        {
            // If we don't copy a value, set it to the default
            SetDefaultValues();

            // Basics
            if (!isInstalled)
            {
                CanonicalName = from.CanonicalName;
                FormatId = from.FormatId;
                PropertyId = from.PropertyId;
            }

            // Search
            InInvertedIndex = from.InInvertedIndex;
            IsColumn = from.IsColumn;
            IsColumnSparse = from.IsColumnSparse;
            ColumnIndexType = from.ColumnIndexType;
            MaxSize = from.MaxSize;
            if (!isInstalled)
                Mnemonics = from.Mnemonics;

            // Label
            DisplayName = from.DisplayName;
            SortDescription = from.SortDescription;
            EditInvitation = from.EditInvitation;
            HideLabel = from.HideLabel;

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
            if (!isInstalled)
                SearchRawValue = from.SearchRawValue;
            ConditionType = from.ConditionType;
            if (!isInstalled)
                ConditionOperation = from.ConditionOperation;
            else
                ConditionOperation = ConfiguredOperationFromInstalled(from.ConditionOperationInstalled);

            // Display
            DisplayType = from.DisplayType;
            if (!isInstalled)
            {
                StringFormat = from.StringFormat;
                BooleanFormat = from.BooleanFormat;
                NumberFormat = from.NumberFormat;
                DateTimeFormat = from.DateTimeFormat;
            }
            DefaultColumnWidth = from.DefaultColumnWidth;
            Alignment = from.Alignment;
            RelativeDescriptionType = from.RelativeDescriptionType;
            DefaultSortDirection = from.DefaultSortDirection;
            if (!isInstalled)
                EditControl = from.EditControl;
        }

        ConditionOperationConfigured ConfiguredOperationFromInstalled (PropertyConditionOperation operation)
        {
            switch(operation)
            {
                case PropertyConditionOperation.Equal:
                case PropertyConditionOperation.WordEqual:
                default:
                    return ConditionOperationConfigured.Equal;

                case PropertyConditionOperation.NotEqual:
                case PropertyConditionOperation.ValueNotContains:
                    return ConditionOperationConfigured.NotEqual;

                case PropertyConditionOperation.LessThan:
                case PropertyConditionOperation.LessThanOrEqual:
                    return ConditionOperationConfigured.LessThan;

                case PropertyConditionOperation.GreaterThan:
                case PropertyConditionOperation.GreaterThanOrEqual:
                    return ConditionOperationConfigured.GreaterThan;

                case PropertyConditionOperation.ValueContains:
                case PropertyConditionOperation.ValueEndsWith:
                case PropertyConditionOperation.ValueStartsWith:
                case PropertyConditionOperation.WordStartsWith:
                    return ConditionOperationConfigured.Contains;
            }
        }

        internal struct Difference
        {
            public string Name;
            public string Current;
            public string Previous;
            public Difference(string name, object current, object previous)
            {
                Name = name;
                Current = current?.ToString();
                Previous = previous?.ToString();
            }
        }

        internal List<Difference> CompareTo(PropertyConfig baseline, bool isInstalled)
        {
            var result = new List<Difference>();

            // Basics
            if (!isInstalled)
            {
                if (CanonicalName != baseline.CanonicalName) result.Add(new Difference(nameof(CanonicalName), CanonicalName, baseline.CanonicalName));
                if (FormatId != baseline.FormatId) result.Add(new Difference(nameof(FormatId), FormatId, baseline.FormatId));
                if (PropertyId != baseline.PropertyId) result.Add(new Difference(nameof(PropertyId), PropertyId, baseline.PropertyId));
            }

            // Search
            if (InInvertedIndex != baseline.InInvertedIndex) result.Add(new Difference(nameof(InInvertedIndex), InInvertedIndex, baseline.InInvertedIndex));
            if (IsColumn != baseline.IsColumn) result.Add(new Difference(nameof(IsColumn), IsColumn, baseline.IsColumn));
            if (IsColumnSparse != baseline.IsColumnSparse) result.Add(new Difference(nameof(IsColumnSparse), IsColumnSparse, baseline.IsColumnSparse));
            if (ColumnIndexType != baseline.ColumnIndexType) result.Add(new Difference(nameof(ColumnIndexType), ColumnIndexType, baseline.ColumnIndexType));
            if (MaxSize != baseline.MaxSize) result.Add(new Difference(nameof(MaxSize), MaxSize, baseline.MaxSize));
            if (!isInstalled)
                if (Mnemonics != baseline.Mnemonics) result.Add(new Difference(nameof(Mnemonics), Mnemonics, baseline.Mnemonics));


            // Label
            if (DisplayName != baseline.DisplayName) result.Add(new Difference(nameof(DisplayName), DisplayName, baseline.DisplayName));
            if (SortDescription != baseline.SortDescription) result.Add(new Difference(nameof(SortDescription), SortDescription, baseline.SortDescription));
            if (EditInvitation != baseline.EditInvitation) result.Add(new Difference(nameof(EditInvitation), EditInvitation, baseline.EditInvitation));
            if (HideLabel != baseline.HideLabel) result.Add(new Difference(nameof(HideLabel), HideLabel, baseline.HideLabel));

            // Type
            if (Type != baseline.Type) result.Add(new Difference(nameof(Type), Type, baseline.Type));
            if (GroupingRange != baseline.GroupingRange) result.Add(new Difference(nameof(GroupingRange), GroupingRange, baseline.GroupingRange));
            if (IsInnate != baseline.IsInnate) result.Add(new Difference(nameof(IsInnate), IsInnate, baseline.IsInnate));
            if (CanBePurged != baseline.CanBePurged) result.Add(new Difference(nameof(CanBePurged), CanBePurged, baseline.CanBePurged));
            if (MultipleValues != baseline.MultipleValues) result.Add(new Difference(nameof(MultipleValues), MultipleValues, baseline.MultipleValues));
            if (IsGroup != baseline.IsGroup) result.Add(new Difference(nameof(IsGroup), IsGroup, baseline.IsGroup));
            if (AggregationType != baseline.AggregationType) result.Add(new Difference(nameof(AggregationType), AggregationType, baseline.AggregationType));
            if (IsTreeProperty != baseline.IsTreeProperty) result.Add(new Difference(nameof(IsTreeProperty), IsTreeProperty, baseline.IsTreeProperty));
            if (IsViewable != baseline.IsViewable) result.Add(new Difference(nameof(IsViewable), IsViewable, baseline.IsViewable));
            if (!isInstalled)
                if (SearchRawValue != baseline.SearchRawValue) result.Add(new Difference(nameof(SearchRawValue), SearchRawValue, baseline.SearchRawValue));
            if (ConditionType != baseline.ConditionType) result.Add(new Difference(nameof(ConditionType), ConditionType, baseline.ConditionType));
            if (!isInstalled)
            {
                if (ConditionOperation != baseline.ConditionOperation) result.Add(new Difference(nameof(ConditionOperation), ConditionOperation, baseline.ConditionOperation));
            }
            else
            {
                if (baseline.ConditionOperationInstalled > PropertyConditionOperation.GreaterThan || 
                    (int)baseline.ConditionOperationInstalled != (int)ConditionOperation)
                    result.Add(new Difference(nameof(ConditionOperation), ConditionOperation, baseline.ConditionOperationInstalled));
            }

            // Display
            if (DisplayType != baseline.DisplayType) result.Add(new Difference(nameof(DisplayType), DisplayType, baseline.DisplayType));
            if (!isInstalled)
            {
                if (StringFormat != baseline.StringFormat) result.Add(new Difference(nameof(StringFormat), StringFormat, baseline.StringFormat));
                if (BooleanFormat != baseline.BooleanFormat) result.Add(new Difference(nameof(BooleanFormat), BooleanFormat, baseline.BooleanFormat));
                if (NumberFormat != baseline.NumberFormat) result.Add(new Difference(nameof(NumberFormat), NumberFormat, baseline.NumberFormat));
                if (DateTimeFormat != baseline.DateTimeFormat) result.Add(new Difference(nameof(DateTimeFormat), DateTimeFormat, baseline.DateTimeFormat));
            }
            if (DefaultColumnWidth != baseline.DefaultColumnWidth) result.Add(new Difference(nameof(DefaultColumnWidth), DefaultColumnWidth, baseline.DefaultColumnWidth));
            if (Alignment != baseline.Alignment) result.Add(new Difference(nameof(Alignment), Alignment, baseline.Alignment));
            //if (RelativeDescriptionType != baseline.RelativeDescriptionType) result.Add(new Difference(nameof(RelativeDescriptionType), RelativeDescriptionType, baseline.RelativeDescriptionType));
            if (DefaultSortDirection != baseline.DefaultSortDirection) result.Add(new Difference(nameof(DefaultSortDirection), DefaultSortDirection, baseline.DefaultSortDirection));
            if (!isInstalled)
                if (EditControl != baseline.EditControl) result.Add(new Difference(nameof(EditControl), EditControl, baseline.EditControl));
            return result;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}