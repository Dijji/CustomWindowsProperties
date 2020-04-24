using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomWindowsProperties
{
    static class Help
    {
        public static string Text (string tag)
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
                default:
                    return $"Help for {tag} is not yet available";
            }
        }
    }
}
