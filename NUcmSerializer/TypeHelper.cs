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

namespace NUmcSerializer
{
    public static class TypeHelper
    {
        public static bool IsSimpleType(this Type type)
        {
            return type.IsPrimitive || type.IsValueType || type == typeof(string);
        }

        public static bool IsSimpleArrayType(this Type type)
        {
            return type.IsArray && IsSimpleType(type.GetElementType());
        }

        public static bool IsSimpleTupleType(this Type type)
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

        public static bool IsVendorArrayType(this Type type)
        {
            return type.IsArray && !IsSimpleType(type.GetElementType());
        }

        public static TokenType GetTypeTokenType(this Type type)
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

        public static object GetGenericObjectPropertyValue(object obj, uint index)
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

        public static T GetGenericObjectPropertyValue<T>(object obj, uint index)
        {
            return (T)GetGenericObjectPropertyValue(obj, index);
        }
    }
}
