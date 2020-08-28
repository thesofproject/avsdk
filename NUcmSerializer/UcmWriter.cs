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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NUcmSerializer
{
    public enum TokenType
    {
        None,
        Element,
        Array,
        Tuple,
        VendorArray, // array with non-basic elements
        Section,
        // reader-only
        SectionEnd,
        ArrayEnd,
    }

    public class UcmWriter : IDisposable
    {
        TextWriter writer;
        Encoding encoding;
        char indentChar;
        int indentSize;

        TokenInfo[] stack;
        int top;

        bool disposed;

        public struct TokenInfo
        {
            public string Name;
            public TokenType Type;
            public string Identifier;
            public bool Inline;
            public bool TagElements;

            public void Init(object token, PropertyInfo tokenInfo)
            {
                Type type = token.GetType();
                object[] attrs;

                Type = type.GetTypeTokenType();

                if (tokenInfo == null)
                    attrs = type.GetCustomAttributes(false);
                else
                    attrs = tokenInfo.GetCustomAttributes(false);

                var attr = (UcmNamedTagAttribute)attrs.SingleOrDefault(
                    a => a.GetType().IsSubclassOf(typeof(UcmNamedTagAttribute)));
                // obtain token name
                if (attr != null && attr.Name != null)
                    Name = attr.Name;
                else if (type.IsSimpleTupleType())
                    Name = TypeHelper.GetObjectGenericPropertyValue<string>(token, 0);
                else if (tokenInfo != null)
                    Name = tokenInfo.Name;
                else
                    Name = type.Name;

                // obtain token misc params
                if (type.IsArray)
                {
                    var arrAttr = attr as UcmArrayAttribute;
                    Inline = (arrAttr != null) ? arrAttr.Inline : false;
                    TagElements = (arrAttr != null) ? arrAttr.TagElements : true;
                }

                PropertyInfo propInfo = type.GetProperties().SingleOrDefault(
                    p => Attribute.IsDefined(p, typeof(UcmIdentifierAttribute)));
                if (propInfo != null)
                    Identifier = (string)propInfo.GetValue(token);
            }
        }

        public UcmWriter(Stream stream, Encoding encoding)
        {
            this.encoding = encoding;
            if (encoding != null)
                writer = new StreamWriter(stream, encoding);
            else
                writer = new StreamWriter(stream);
            stack = new TokenInfo[8];
            top = 0;
            indentChar = ' ';
            indentSize = 4;
            disposed = false;
        }

        public UcmWriter(Stream stream)
            : this(stream, null)
        {
        }

        // helper method, retrieves token properties to enumerate over
        static PropertyInfo[] GetTypeTokenProperties(object token)
        {
            Type type = token.GetType();
            PropertyInfo[] props = type.GetProperties();

            List<PropertyInfo> result = props.Where(
                p => p.GetValue(token) != null &&
                     !Attribute.IsDefined(p, typeof(UcmIdentifierAttribute)) &&
                     !Attribute.IsDefined(p, typeof(UcmIgnoreAttribute))
            ).ToList();

            var cache = new List<string>();
            for (int i = 0; i < result.Count; i++)
            {
                PropertyInfo propInfo = result[i];
                var attr = propInfo.GetCustomAttribute<UcmExclusiveAttribute>();
                if (attr == null)
                    continue;

                if (cache.Contains(attr.Namespace))
                    result.RemoveAt(i--);
                else
                    cache.Add(attr.Namespace);
            }

            return result.ToArray();
        }

        internal static string GetTokenStringValue(object token)
        {
            // non-null tuples may still have Item2 (value) set to null
            if (token == null)
                return string.Empty;

            Type type = token.GetType();
            string value;

            if (type == typeof(Guid))
            {
                value = string.Join(", ", ((Guid)token).ToByteArray());
            }
            else if (type.IsSubclassOf(typeof(Enum)))
            {
                var attr = ((Enum)token).GetAttributeOfType<UcmEnumAttribute>();
                if (attr != null)
                    value = attr.Name;
                else
                    value = ((Enum)token).ToString("D"); // default not acceptable
            }
            else if (type == typeof(string))
            {
                value = token.ToString();
            }
            else
            {
                value = token.ToString().ToLower();
            }

            return value;
        }

        internal void WriteValue(object token)
        {
            writer.Write(GetTokenStringValue(token));
        }

        internal void WriteArray(object token, string elemTag)
        {
            Array array = (Array)token;
            StringBuilder str = new StringBuilder();

            foreach (var elem in array)
            {
                str.Clear();
                str.Append(indentChar, indentSize * top);
                str.Append(elemTag);
                str.Append(GetTokenStringValue(elem));
                str.Append(elemTag);
                writer.WriteLine(str);
            }
        }

        internal void WriteVendorArray(object token)
        {
            Array array = (Array)token;

            foreach (var elem in array)
                WriteToken(elem, null);
        }

        public void WriteToken(object token, PropertyInfo tokenInfo)
        {
            if (token == null)
                throw new ArgumentNullException("token");

            TokenInfo current = new TokenInfo();
            current.Init(token, tokenInfo);

            if (!current.Inline)
                WriteTagStart(current);

            switch (current.Type)
            {
                case TokenType.Element:
                    WriteValue(token);
                    break;

                case TokenType.Array:
                    WriteArray(token, current.TagElements ? "\"" : string.Empty);
                    break;

                case TokenType.Tuple:
                    WriteValue(TypeHelper.GetObjectGenericPropertyValue(token, 1));
                    break;

                case TokenType.VendorArray:
                    WriteVendorArray(token);
                    break;

                case TokenType.Section:
                    PropertyInfo[] infos = GetTypeTokenProperties(token);

                    foreach (var info in infos)
                        WriteToken(info.GetValue(token), info);
                    break;

                default:
                    throw new NotSupportedException("current");
            }

            if (!current.Inline)
                WriteTagEnd();
        }

        public void WriteTagStart(TokenInfo tokenInfo)
        {
            if (tokenInfo.Type == TokenType.None)
                throw new InvalidOperationException("tokenInfo");

            StringBuilder str = new StringBuilder();
            str.Append(indentChar, indentSize * top);
            str.Append(tokenInfo.Name);
            if (tokenInfo.Identifier != null)
            {
                str.Append(".\"");
                str.Append(tokenInfo.Identifier);
                str.Append("\"");
            }

            switch (tokenInfo.Type)
            {
                case TokenType.Element:
                case TokenType.Tuple:
                    str.Append(" \"");
                    writer.Write(str);
                    break;

                case TokenType.Array:
                case TokenType.VendorArray:
                    writer.WriteLine();
                    str.Append(" [");
                    writer.WriteLine(str);
                    break;

                case TokenType.Section:
                    writer.WriteLine();
                    str.Append(" {");
                    writer.WriteLine(str);
                    break;

                default:
                    throw new NotSupportedException("tokenInfo");
            }

            PushStack(tokenInfo);
        }

        public void WriteTagEnd()
        {
            if (top <= 0)
                throw new InvalidOperationException("top");
            TokenInfo current = stack[top];
            top--; // pop stack

            switch (current.Type)
            {
                case TokenType.Element:
                case TokenType.Tuple:
                    writer.Write("\"");
                    break;

                case TokenType.Array:
                case TokenType.VendorArray:
                case TokenType.Section:
                    StringBuilder str = new StringBuilder();
                    str.Append(indentChar, indentSize * top);
                    writer.Write(str);

                    if (current.Type != TokenType.Section)
                        writer.Write("]");
                    else
                        writer.Write("}");
                    break;

                default:
                    throw new NotSupportedException("current");
            }

            writer.WriteLine();
        }

        void PushStack(TokenInfo tokenInfo)
        {
            if (top == stack.Length - 1)
            {
                TokenInfo[] na = new TokenInfo[stack.Length * 2];
                if (top > 0) Array.Copy(stack, na, top + 1);
                stack = na;
            }

            top++; // Move up stack
            stack[top] = tokenInfo;
        }

        // Flushes whatever is in the buffer to the underlying stream/TextWriter and flushes the underlying stream/TextWriter.
        public void Flush()
        {
            writer.Flush();
        }

        public void AutoCompleteAll()
        {
            while (top > 0)
                WriteTagEnd();
        }

        public void Close()
        {
            try
            {
                AutoCompleteAll();
            }
            catch
            {
            }
            finally
            {
                writer.Close();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                Close();
                writer.Dispose();
            }

            disposed = true;
        }
    }
}
