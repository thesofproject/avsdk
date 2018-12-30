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

        public static TokenType GetTypeTokenType(this Type type)
        {
            if (type.IsSimpleType())
                return TokenType.Element;
            else if (type.IsSimpleArrayType())
                return TokenType.Array;
            else if (type.IsSubclassOf(typeof(Section)))
                return TokenType.Section;

            return TokenType.None;
        }
    }
}
