namespace CustomWindowsProperties
{
    static class Help
    {
        public static string HtmlText(string tag)
        {
            var body = Text(tag);
            if (!body.StartsWith("<"))
                body = $"<p>{body}</p>";

            return "<!doctype html><html lang=en><head><meta charset=utf-8><style>body{font-size:11px; font-family:Arial,Helvetica,sans-serif;}td{vertical-align:top;}</style></head>" +
                    $"<body><p><b>{tag}</b></p>{body}</body></html>";
        }

        public static string Text(string tag)
        {
            switch (tag)
            {
                case null:
                    return null;
                case nameof(PropertyConfig.CanonicalName):
                    return LocalizedHelp.CanonicalName;
                case nameof(PropertyConfig.InInvertedIndex):
                    return LocalizedHelp.InInvertedIndex;
                case nameof(PropertyConfig.IsColumn):
                    return LocalizedHelp.IsColumn;
                case nameof(PropertyConfig.IsColumnSparse):
                    return LocalizedHelp.IsColumnSparse;
                case nameof(PropertyConfig.ColumnIndexType):
                    return LocalizedHelp.ColumnIndexType;
                case nameof(PropertyConfig.MaxSize):
                    return LocalizedHelp.MaxSize;
                case nameof(PropertyConfig.Mnemonics):
                    return LocalizedHelp.Mnemonics;
                case nameof(PropertyConfig.DisplayName):
                    return LocalizedHelp.DisplayName;
                case nameof(PropertyConfig.SortDescription):
                    return LocalizedHelp.SortDescription;
                case nameof(PropertyConfig.EditInvitation):
                    return LocalizedHelp.EditInvitation;
                case nameof(PropertyConfig.HideLabel):
                    return LocalizedHelp.HideLabel;
                case nameof(PropertyConfig.Type):
                    return LocalizedHelp.Type;
                case nameof(PropertyConfig.GroupingRange):
                    return LocalizedHelp.GroupingRange;
                case nameof(PropertyConfig.IsInnate):
                    return LocalizedHelp.IsInnate;
                case nameof(PropertyConfig.CanBePurged):
                    return LocalizedHelp.CanBePurged;
                case nameof(PropertyConfig.MultipleValues):
                    return LocalizedHelp.MultipleValues;
                case nameof(PropertyConfig.IsGroup):
                    return LocalizedHelp.IsGroup;
                case nameof(PropertyConfig.AggregationType):
                    return LocalizedHelp.AggregationType;
                case nameof(PropertyConfig.IsTreeProperty):
                    return LocalizedHelp.IsTreeProperty;
                case nameof(PropertyConfig.IsViewable):
                    return LocalizedHelp.IsViewable;
                case nameof(PropertyConfig.SearchRawValue):
                    return LocalizedHelp.SearchRawValue;
                case nameof(PropertyConfig.ConditionType):
                    return LocalizedHelp.ConditionType;
                case nameof(PropertyConfig.ConditionOperation):
                    return LocalizedHelp.ConditionOperation;
                case nameof(PropertyConfig.EditControl):
                    return LocalizedHelp.EditControl;
                case nameof(PropertyConfig.DisplayType):
                    return LocalizedHelp.DisplayType;
                case nameof(PropertyConfig.StringFormat):
                    return LocalizedHelp.StringFormat;
                case nameof(PropertyConfig.BooleanFormat):
                    return LocalizedHelp.BooleanFormat;
                case nameof(PropertyConfig.NumberFormat):
                    return LocalizedHelp.NumberFormat;
                case nameof(PropertyConfig.DateTimeFormat):
                    return LocalizedHelp.DateTimeFormat;
                case nameof(PropertyConfig.Alignment):
                    return LocalizedHelp.Alignment;
                case nameof(PropertyConfig.DefaultSortDirection):
                    return LocalizedHelp.DefaultSortDirection;
                default:
                    return $"Help for {tag} is not yet available";
            }
        }
    }
}
