namespace TeklaHelper.DrawingAttribute.Filters
{
    /// <summary>
    /// How a filter row is joined to the next one (the And/Or column of the Tekla filter dialog).
    /// </summary>
    public enum FilterLogic
    {
        /// <summary>No operator, written <c>Empty</c>; used on the last row.</summary>
        None,

        /// <summary>And, written <c>&amp;&amp;</c>.</summary>
        And,

        /// <summary>Or, written <c>||</c>.</summary>
        Or,
    }
}
