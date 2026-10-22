using System;

namespace TeklaHelper.DrawingAttribute.Filters
{
    /// <summary>
    /// Checks the text of filter tokens: each token is one line of the file, so it cannot hold a line break.
    /// </summary>
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
    }
}
