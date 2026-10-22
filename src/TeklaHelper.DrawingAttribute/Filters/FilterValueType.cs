namespace TeklaHelper.DrawingAttribute.Filters
{
    /// <summary>
    /// Type of the values a filter property is compared with; decides which operators apply.
    /// </summary>
    public enum FilterValueType
    {
        /// <summary>Text: equals, contains, begins with, ends with and their negations.</summary>
        String,

        /// <summary>Number: equals, not equal, less / greater (or equal).</summary>
        Number,

        /// <summary>Date: equals, not equal, earlier / later (or equal).</summary>
        Date,

        /// <summary>True / false: equals, not equal.</summary>
        Boolean,
    }
}
