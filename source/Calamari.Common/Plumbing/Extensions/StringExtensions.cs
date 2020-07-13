﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Calamari.Common.Plumbing.Extensions
{
    public static class StringExtensions
    {
        public static bool ContainsIgnoreCase(this string originalString, string value)
        {
            return originalString.IndexOf(value, StringComparison.OrdinalIgnoreCase) != -1;
        }

        public static string EscapeSingleQuotedString(this string str) =>
            str.Replace("'", "''");

        public static byte[] EncodeInUtf8Bom(this string source)
        {
            return Encoding.UTF8.GetPreamble().Concat(source.EncodeInUtf8NoBom()).ToArray();
        }

        public static byte[] EncodeInUtf8NoBom(this string source)
        {
            return Encoding.UTF8.GetBytes(source);
        }

        public static string AsRelativePathFrom(this string source, string baseDirectory)
        {
            // Adapted from https://stackoverflow.com/a/340454
            var uri = new Uri(source);
            if (!baseDirectory.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                baseDirectory += Path.DirectorySeparatorChar.ToString();
            }
            var baseUri = new Uri(baseDirectory);

            var relativeUri = baseUri.MakeRelativeUri(uri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath;
        }
        
        public static bool IsValidUrl(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return Uri.TryCreate(value, UriKind.Absolute, out var _);
        }

        public static string Join(this IEnumerable<string> values, string separator)
            => string.Join(separator, values);
    }
}