//
// Copyright (c) 2019, Intel Corporation. All rights reserved.
//
// Author: Cezary Rojewski <cezary.rojewski@intel.com>
//
// SPDX-License-Identifier: Apache-2.0 OR MIT
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace NUcmSerializer
{
    public class UcmReader : IDisposable
    {
        const string NAME_GROUP = "Name";
        const string VALUE_GROUP = "Value";
        const string IDENTIFIER_GROUP = "Identifier";

        static readonly RegexOptions regexOptions = (RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        static readonly Regex sectionRegex = new Regex($"^(?<{NAME_GROUP}>[^.\"\\s]+)(\\.\"(?<{IDENTIFIER_GROUP}>[^\"]+)\")?\\s*{{", regexOptions);
        static readonly Regex tupleRegex = new Regex($"^(?<{NAME_GROUP}>[^\"\\s]+)\\s*\"(?<{VALUE_GROUP}>[^\"]+)\"", regexOptions);
        static readonly Regex elementRegex = new Regex($"^\"?(?<{VALUE_GROUP}>[^\"]+)\"?", regexOptions);
        static readonly Regex arrayRegex = new Regex($"^(?<{NAME_GROUP}>[^\"\\s]+)\\s*\\[", regexOptions);

        readonly StreamReader reader;
        readonly Encoding encoding;

        bool disposed;

        public UcmReader(Stream stream, Encoding encoding)
        {
            this.encoding = encoding;
            if (encoding != null)
                reader = new StreamReader(stream, encoding);
            else
                reader = new StreamReader(stream);
        }

        public UcmReader(Stream stream)
            : this(stream, null)
        {
        }

        internal class TokenInfo
        {
            public TokenType Type;
            public string Name;
            public string Value;
            public string Identifier;

            internal TokenInfo(TokenType type, Match match)
            {
                Type = type;
                if (match == null)
                    return;

                Group name = match.Groups[NAME_GROUP];
                if (name != null && name.Success)
                    Name = name.Value;
                Group value = match.Groups[VALUE_GROUP];
                if (value != null && value.Success)
                    Value = value.Value;
                Group identifier = match.Groups[IDENTIFIER_GROUP];
                if (identifier != null && identifier.Success)
                    Identifier = identifier.Value;

                if (Type == TokenType.Section && "tuples".Equals(Name))
                    Type = TokenType.VendorArray;
            }
        }

        internal TokenInfo GetNextToken()
        {
            string line, str = string.Empty;

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Equals(string.Empty))
                    continue;
                str = string.Concat(str, line);
                if (str.Last() == ',')
                    continue;
                break;
            }

            if (line == null)
                return null;
            char last = str.Last();
            TokenType type = TokenType.None;
            Match match = null;

            switch (last)
            {
                case '{':
                    match = sectionRegex.Match(str);
                    if (match.Success)
                        type = TokenType.Section;
                    break;

                case '}':
                    type = TokenType.SectionEnd;
                    break;

                case '[':
                    match = arrayRegex.Match(str);
                    if (match.Success)
                        type = TokenType.Array;
                    break;

                case ']':
                    type = TokenType.ArrayEnd;
                    break;

                default:
                    match = tupleRegex.Match(str);
                    if (match.Success)
                    {
                        type = TokenType.Tuple;
                        break;
                    }

                    match = elementRegex.Match(str);
                    if (match.Success)
                        type = TokenType.Element;
                    break;
            }

            return new TokenInfo(type, match);
        }

        internal static bool IsPropertyInlinedArray(PropertyInfo prop)
        {
            var attr = prop.GetCustomAttribute<UcmArrayAttribute>();
            return (attr != null) && attr.Inline;
        }

        internal static PropertyInfo GetSectionProperty(Section section, TokenInfo token)
        {
            PropertyInfo[] props = section.GetType().GetProperties();

            foreach (var prop in props)
            {
                if (prop.Name.Equals(token.Name))
                    return prop;

                var attr = prop.GetCustomAttribute<UcmNamedTagAttribute>();
                if (attr == null || attr.Name == null)
                    continue;
                if (attr.Name.Equals(token.Name)) // accounts also for section-inlined arrays
                    return prop;
            }

            return null;
        }

        internal Section CreateSection(TokenInfo token, PropertyInfo prop)
        {
            Type type = prop?.PropertyType;

            if (type == null)
            {
                string ns = typeof(Section).Namespace;
                type = Type.GetType($"{ns}.{token.Name}");
            }
            else if (type.IsArray)
            {
                type = prop.PropertyType.GetElementType();
            }

            Section section = (Section)Activator.CreateInstance(type);
            section.Identifier = token.Identifier;

            return section;
        }

        internal Array ReadSimpleArray(Type valueType)
        {
            var result = new List<object>();
            TokenInfo token;

            while ((token = GetNextToken()) != null)
            {
                if (token.Type == TokenType.ArrayEnd)
                {
                    var res = Array.CreateInstance(valueType, result.Count);
                    for (int i = 0; i < res.Length; i++)
                        res.SetValue(result[i], i);
                    return res;
                }

                if (token.Type != TokenType.Element)
                    throw new InvalidDataException("expected element, got: " + token.Type);

                object value = valueType.ConvertFromString(token.Value);
                if (value == null)
                    throw new InvalidDataException($"cannot convert {token.Value} to {valueType}");
                result.Add(value);
            }

            throw new InvalidDataException("expected array end tag");
        }

        internal Array ReadTupleArray(Type valueType)
        {
            Type tupleType = typeof(Tuple<,>).MakeGenericType(typeof(string), valueType);
            var tuples = new List<object>();
            TokenInfo token;

            while ((token = GetNextToken()) != null)
            {
                if (token.Type == TokenType.SectionEnd)
                {
                    var res = Array.CreateInstance(tupleType, tuples.Count);
                    for (int i = 0; i < res.Length; i++)
                        res.SetValue(tuples[i], i);
                    return res;
                }

                if (token.Type != TokenType.Tuple)
                    throw new InvalidDataException("expected tuple, got: " + token.Type);

                object value = valueType.ConvertFromString(token.Value);
                if (value == null)
                    throw new InvalidDataException($"cannot convert {token.Value} to {valueType}");

                object tuple = Activator.CreateInstance(tupleType, token.Name, value);
                tuples.Add(tuple);
            }

            throw new InvalidDataException("expected section end tag");
        }

        internal VendorTuples ReadVendorTuples(TokenInfo token)
        {
            Type valueType = null;
            foreach (var pair in VendorTuples.TupleTypes)
            {
                if (!token.Identifier.StartsWith(pair.Value, StringComparison.Ordinal))
                    continue;
                valueType = pair.Key;
                // strip away '<type>.' from tuples identifier
                int count = pair.Value.Length;
                if (token.Identifier.Length > count)
                    count++;
                token.Identifier = token.Identifier.Remove(0, count);
                break;
            }

            if (valueType == null)
                throw new InvalidOperationException($"unknown tuples type: {token.Identifier}");

            Type vendorTupleType = typeof(VendorTuples<>).MakeGenericType(valueType);
            VendorTuples result = (VendorTuples)Activator.CreateInstance(vendorTupleType);
            result.Identifier = token.Identifier;

            object tuples = ReadTupleArray(valueType);

            var prop = vendorTupleType.GetProperty(nameof(VendorTuples<uint>.Tuples));
            prop.SetValue(result, tuples);
            return result;
        }

        internal Section ReadSectionTokens(Section section)
        {
            var sectionTokens = (SectionVendorTokens)section;

            object tokens = ReadTupleArray(typeof(uint));
            sectionTokens.Tokens = (Tuple<string, uint>[])tokens;

            return sectionTokens;
        }

        internal Section ReadSection(TokenInfo source, PropertyInfo info)
        {
            Section section = CreateSection(source, info);

            if (section is SectionVendorTokens)
                return ReadSectionTokens(section);

            TokenInfo token;
            while ((token = GetNextToken()) != null)
            {
                if (token.Type == TokenType.SectionEnd)
                    return section;

                PropertyInfo prop = GetSectionProperty(section, token);
                if (prop == null) // be permissive about unknown properties
                    continue;

                object value;
                switch (token.Type)
                {
                    case TokenType.Section:
                        value = ReadSection(token, prop);
                        break;

                    case TokenType.VendorArray:
                        value = ReadVendorTuples(token);
                        break;

                    case TokenType.Array:
                        Type valueType = prop.PropertyType.GetElementType();
                        value = ReadSimpleArray(valueType);
                        break;

                    case TokenType.Tuple:
                        value = prop.PropertyType.ConvertFromString(token.Value);
                        break;

                    default:
                        throw new InvalidOperationException();
                }

                if (IsPropertyInlinedArray(prop))
                {
                    Array currArr = (Array)prop.GetValue(section);
                    Type elemType = prop.PropertyType.GetElementType();
                    int length = (currArr != null) ? currArr.Length : 0;

                    // enlarge container and append new value
                    Array newArr = Array.CreateInstance(elemType, length + 1);
                    if (currArr != null)
                        currArr.CopyTo(newArr, 0);
                    newArr.SetValue(value, length);
                    value = newArr;
                }

                prop.SetValue(section, value);
            }

            throw new InvalidDataException("expected section end tag");
        }

        public Section ReadToken()
        {
            TokenInfo token = GetNextToken();
            if (token == null)
                return null;

            if (token.Type != TokenType.Section)
                throw new InvalidDataException("expected section, got: " + token);

            return ReadSection(token, null);
        }

        public void Close()
        {
            reader.Close();
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
                reader.Dispose();
            }

            disposed = true;
        }
    }
}
