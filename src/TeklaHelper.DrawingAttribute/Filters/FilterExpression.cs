namespace TeklaHelper.DrawingAttribute.Filters
{
    /// <summary>
    /// A filter as a tree: a single <see cref="FilterCondition"/> or a <see cref="FilterGroup"/> of expressions joined by
    /// And / Or. Written to a file, a group becomes a pair of parentheses around its rows.
    /// </summary>
    public abstract class FilterExpression
    {
        private protected FilterExpression()
        {
        }
    }
}
