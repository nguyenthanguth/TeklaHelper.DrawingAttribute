using System;
using System.Globalization;

namespace TeklaHelper.DrawingAttribute.Filters
{
    internal static class FilterToken
    {
        public static string Check(string value, string parameterName)
        {
            if (value != null && value.IndexOfAny(new[] { '\r', '\n' }) >= 0)
            {
                throw new ArgumentException("Filter tokens and values cannot contain line breaks.", parameterName);
            }

            return value;
        }

        /// <summary>
        /// Checks a value against <see cref="FilterRow.MaxValueLength"/> in characters. The limit is in bytes of the file's
        /// encoding, which <see cref="FilterFile.ToBytes"/> checks again; a value longer in characters can never fit.
        /// </summary>
        public static string CheckValue(string value, string parameterName)
        {
            Check(value, parameterName);
            if (value != null && value.Length > FilterRow.MaxValueLength)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture, "A filter value can hold at most {0} characters (Tekla's filter dialog keeps it in a 1024-byte buffer); this one has {1}.", FilterRow.MaxValueLength, value.Length),
                    parameterName);
            }

            return value;
        }
    }
}
