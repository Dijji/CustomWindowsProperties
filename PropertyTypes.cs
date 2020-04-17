// Copyright (c) 2020, Dijji, and released under Ms-PL.  This, with other relevant licenses, can be found in the root of this distribution.

using System.Runtime.InteropServices;

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

    public enum EditControl
    {
        Default,
        Calendar,
        CheckboxDropList,
        DropList,
        MultiLineText,
        MultiValueText,
        Rating,
        Text,
        IconList,
    }
    

    internal class PropertyUtils
    {
        /// <summary>
        /// Converts VarEnum to its associated propdesc Type.
        /// </summary>
        /// <param name="VarEnumType">VarEnum value</param>
        /// <returns>Associated .net equivelent.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public static PropertyTypes VarEnumToPropertyType(VarEnum VarEnumType)
        {
            switch (VarEnumType)
            {
                case (VarEnum.VT_EMPTY):
                case (VarEnum.VT_NULL):
                    return PropertyTypes.Null;
                case (VarEnum.VT_UI1):
                    return PropertyTypes.Byte;
                case (VarEnum.VT_I2):
                    return PropertyTypes.Int16;
                case (VarEnum.VT_UI2):
                    return PropertyTypes.UInt16;
                case (VarEnum.VT_I4):
                    return PropertyTypes.Int32;
                case (VarEnum.VT_UI4):
                    return PropertyTypes.UInt32;
                case (VarEnum.VT_I8):
                    return PropertyTypes.Int64;
                case (VarEnum.VT_UI8):
                    return PropertyTypes.UInt64;
                case (VarEnum.VT_R8):
                    return PropertyTypes.Double;
                case (VarEnum.VT_BOOL):
                    return PropertyTypes.Boolean;
                case (VarEnum.VT_FILETIME):
                    return PropertyTypes.DateTime;
                case (VarEnum.VT_CLSID):
                    return PropertyTypes.Guid;
                case (VarEnum.VT_CF):
                    return PropertyTypes.Clipboard;
                case (VarEnum.VT_BLOB):
                    return PropertyTypes.Blob;
                case (VarEnum.VT_LPWSTR):
                    return PropertyTypes.String;
                case (VarEnum.VT_UNKNOWN):
                    return PropertyTypes.Any;
                case (VarEnum.VT_STREAM):
                    return PropertyTypes.Stream;
                case (VarEnum.VT_VECTOR | VarEnum.VT_UI1):
                    return PropertyTypes.Byte;
                case (VarEnum.VT_VECTOR | VarEnum.VT_I2):
                    return PropertyTypes.Int16;
                case (VarEnum.VT_VECTOR | VarEnum.VT_UI2):
                    return PropertyTypes.UInt16;
                case (VarEnum.VT_VECTOR | VarEnum.VT_I4):
                    return PropertyTypes.Int32;
                case (VarEnum.VT_VECTOR | VarEnum.VT_UI4):
                    return PropertyTypes.UInt32;
                case (VarEnum.VT_VECTOR | VarEnum.VT_I8):
                    return PropertyTypes.Int64;
                case (VarEnum.VT_VECTOR | VarEnum.VT_UI8):
                    return PropertyTypes.UInt64;
                case (VarEnum.VT_VECTOR | VarEnum.VT_R8):
                    return PropertyTypes.Double;
                case (VarEnum.VT_VECTOR | VarEnum.VT_BOOL):
                    return PropertyTypes.Boolean;
                case (VarEnum.VT_VECTOR | VarEnum.VT_FILETIME):
                    return PropertyTypes.DateTime;
                case (VarEnum.VT_VECTOR | VarEnum.VT_CLSID):
                    return PropertyTypes.Guid;
                case (VarEnum.VT_VECTOR | VarEnum.VT_CF):
                    return PropertyTypes.Clipboard;
                case (VarEnum.VT_VECTOR | VarEnum.VT_LPWSTR):
                    return PropertyTypes.String;
                default:
                    return PropertyTypes.Object;
            }
        }
    }
}