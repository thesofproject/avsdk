//
// Copyright (c) 2018, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// This program is free software; you can redistribute it and/or modify it
// under the terms and conditions of the MIT License.
//
// This program is distributed in the hope it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NUmcSerializer
{
    internal static class TypeHelper
    {
        internal static bool IsSimpleType(this Type type)
        {
            return type.IsPrimitive || type.IsValueType || type == typeof(string);
        }

        internal static bool IsSimpleArrayType(this Type type)
        {
            return type.IsArray && IsSimpleType(type.GetElementType());
        }

        internal static bool IsSimpleTupleType(this Type type)
        {
            if (type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(Tuple<,>))
            {
                var args = type.GenericTypeArguments;
                foreach (var arg in args)
                    if (!IsSimpleType(arg))
                        return false;
                return true;
            }

            return false;
        }

        internal static bool IsVendorArrayType(this Type type)
        {
            return type.IsArray && !IsSimpleType(type.GetElementType());
        }

        internal static TokenType GetTypeTokenType(this Type type)
        {
            if (type.IsSimpleType())
                return TokenType.Element;
            else if (type.IsSimpleArrayType())
                return TokenType.Array;
            else if (type.IsSimpleTupleType())
                return TokenType.Tuple;
            else if (type.IsVendorArrayType())
                return TokenType.VendorArray;
            else if (type.IsSubclassOf(typeof(Section)))
                return TokenType.Section;

            return TokenType.None;
        }

        internal static object GetObjectGenericPropertyValue(object obj, uint index)
        {
            Type type = obj.GetType();
            if (type.IsGenericType)
            {
                var infos = type.GetProperties();
                if (index < infos.Length)
                    return infos[index].GetValue(obj);
            }

            return null;
        }

        internal static T GetObjectGenericPropertyValue<T>(object obj, uint index)
        {
            return (T)GetObjectGenericPropertyValue(obj, index);
        }
    }

    internal static class ExtensionMethods
    {
        internal static byte[] ToBytes(this string value)
        {
            var result = new List<byte>();
            var substrs = value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim());

            foreach (var substr in substrs)
            {
                if (substr.StartsWith("0x") &&
                    byte.TryParse(substr.Substring(2), NumberStyles.HexNumber,
                                        CultureInfo.InvariantCulture, out byte val))
                    result.Add(val);
                else if (byte.TryParse(substr, out val))
                    result.Add(val);
            }

            return result.ToArray();
        }

        internal static ushort[] ToUInts16(this string value)
        {
            var result = new List<ushort>();
            var substrs = value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim());

            foreach (var substr in substrs)
            {
                if (substr.StartsWith("0x", StringComparison.CurrentCulture) &&
                    ushort.TryParse(substr.Substring(2), NumberStyles.HexNumber,
                                        CultureInfo.CurrentCulture, out ushort val))
                    result.Add(val);
                else if (ushort.TryParse(substr, out val))
                    result.Add(val);
            }

            return result.ToArray();
        }

        internal static uint[] ToUInts32(this string value)
        {
            var result = new List<uint>();
            var substrs = value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim());

            foreach (var substr in substrs)
            {
                if (substr.StartsWith("0x", StringComparison.CurrentCulture) &&
                    uint.TryParse(substr.Substring(2), NumberStyles.HexNumber,
                                        CultureInfo.CurrentCulture, out uint val))
                    result.Add(val);
                else if (uint.TryParse(substr, out val))
                    result.Add(val);
            }

            return result.ToArray();
        }

        internal static T GetAttributeOfType<T>(this Enum value)
            where T : Attribute
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name == null)
                return null;
            // lets be reliable for different locales
            var memInfo = type.GetMember(name);
            if (memInfo.Length == 0)
                return null;

            return (T)Attribute.GetCustomAttribute(memInfo[0], typeof(T), false);
        }
    }
}
