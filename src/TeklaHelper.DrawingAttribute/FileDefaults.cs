using System.Collections.Generic;

namespace TeklaHelper.DrawingAttribute
{
    internal static partial class FileDefaults
    {
        /// <summary>
        /// Sets every key of <paramref name="table"/> in <paramref name="values"/>, in the order of the table. A key of a page
        /// (null value) gets an empty placeholder that keeps its place: the <c>SetDefaults()</c> of its page writes it.
        /// </summary>
        public static void Apply(AttributeCollection values, IEnumerable<(string Key, string Value)> table)
        {
            foreach ((string key, string value) in table)
            {
                values.Set(key, value == null ? AttributeValue.FromString(string.Empty) : AttributeValue.Parse(value));
            }
        }
    }
}
