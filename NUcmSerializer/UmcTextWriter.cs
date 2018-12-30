using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NUmcSerializer
{
    public enum TokenType
    {
        None,
        Element,
        Array,
        Tuple,
        VendorArray, // array with non-basic elements
        Section
    }

    public class UmcTextWriter
    {
        TextWriter writer;
        Encoding encoding;
        char indentChar;
        int indentSize;

        TokenInfo[] stack;
        int top;

        public struct TokenInfo
        {
            public string Name;
            public TokenType Type;
            public string Identifier;

            public void Init(object token, PropertyInfo tokenInfo)
            {
                Type type = token.GetType();
                object[] attrs;

                Type = type.GetTypeTokenType();

                if (tokenInfo == null)
                    attrs = type.GetCustomAttributes(false);
                else
                    attrs = tokenInfo.GetCustomAttributes(false);

                var attr = (UmcNamedTagAttribute)attrs.SingleOrDefault(
                    a => a.GetType().IsSubclassOf(typeof(UmcNamedTagAttribute))
                );

                // obtain token name
                if (attr != null && attr.Name != null)
                    Name = attr.Name;
                else if (type.IsSimpleTupleType())
                    Name = TypeHelper.GetGenericObjectPropertyValue<string>(token, 0);
                else if (tokenInfo != null)
                    Name = tokenInfo.Name;
                else
                    Name = type.Name;

                PropertyInfo propInfo = type.GetProperties().SingleOrDefault(
                    p => Attribute.IsDefined(p, typeof(UmcIdentifierAttribute))
                );

                if (propInfo != null)
                    Identifier = (string)propInfo.GetValue(token);
            }
        }

        public UmcTextWriter(Stream stream, Encoding encoding)
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
        }

        // helper method, retrieves token properties to enumerate over
        static PropertyInfo[] GetTypeTokenProperties(object token)
        {
            Type type = token.GetType();
            PropertyInfo[] props = type.GetProperties();

            List<PropertyInfo> result = props.Where(
                p => !Attribute.IsDefined(p, typeof(UmcIdentifierAttribute)) &&
                     !Attribute.IsDefined(p, typeof(UmcExclusiveAttribute)) &&
                     !Attribute.IsDefined(p, typeof(UmcIgnoreAttribute)) &&
                     p.GetValue(token) != null
            ).ToList();

            var exclusives = props.Where(
                p => Attribute.IsDefined(p, typeof(UmcExclusiveAttribute))
            );

            var groups = exclusives.GroupBy(
                p => p.GetCustomAttribute<UmcExclusiveAttribute>().Namespace
            );

            foreach (var group in groups)
            {
                var chosen = group.FirstOrDefault(p => p.GetValue(token) != null);
                if (chosen != null)
                    result.Add(chosen);
            }

            return result.ToArray();
        }

        internal void WriteValue(object token)
        {
            writer.Write(token.ToString().ToLower());
        }

        internal void WriteArray(object token)
        {
            Array array = (Array)token;
            StringBuilder str = new StringBuilder();

            foreach (var elem in array)
            {
                str.Clear();
                str.Append(indentChar, indentSize * top);
                str.Append("\"");
                str.Append(elem);
                str.Append("\"");
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

            WriteTagStart(current);

            switch (current.Type)
            {
                case TokenType.Element:
                    WriteValue(token);
                    break;

                case TokenType.Array:
                    WriteArray(token);
                    break;

                case TokenType.Tuple:
                    WriteValue(TypeHelper.GetGenericObjectPropertyValue(token, 1));
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

            writer.Close();
        }
    }
}
