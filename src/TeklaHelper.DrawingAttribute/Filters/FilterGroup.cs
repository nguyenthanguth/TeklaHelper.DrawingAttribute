using System;
using System.Collections;
using System.Collections.Generic;

namespace TeklaHelper.DrawingAttribute.Filters
{
    /// <summary>
    /// Expressions joined by And / Or, written to a file inside a pair of parentheses. Each item carries the operator that
    /// joins it to the next item, like a row of the Tekla filter dialog:
    /// <code>
    /// // Name = BEAM AND (Profile begins with HEA OR Profile begins with IPE)
    /// var filter = new FilterGroup()
    ///     .Add(FilterProperties.Part.Name.IsEqual("BEAM"), FilterLogic.And)
    ///     .Add(new FilterGroup()
    ///         .Add(FilterProperties.Part.Profile.BeginsWith("HEA"), FilterLogic.Or)
    ///         .Add(FilterProperties.Part.Profile.BeginsWith("IPE")));
    /// </code>
    /// </summary>
    public sealed class FilterGroup : FilterExpression, IEnumerable<FilterGroupItem>
    {
        /// <summary>
        /// Items in order.
        /// </summary>
        public IList<FilterGroupItem> Items { get; } = new List<FilterGroupItem>();

        /// <summary>
        /// Adds an expression and returns this group, for chaining.
        /// </summary>
        /// <param name="expression">Condition or nested group.</param>
        /// <param name="logic">Operator that joins this item to the next one; <see cref="FilterLogic.None"/> for the last.</param>
        public FilterGroup Add(FilterExpression expression, FilterLogic logic = FilterLogic.None)
        {
            Items.Add(new FilterGroupItem(expression, logic));
            return this;
        }

        /// <inheritdoc />
        public IEnumerator<FilterGroupItem> GetEnumerator() => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// An expression in a <see cref="FilterGroup"/> and the operator that joins it to the next item.
    /// </summary>
    public sealed class FilterGroupItem
    {
        /// <summary>
        /// Creates an item.
        /// </summary>
        public FilterGroupItem(FilterExpression expression, FilterLogic logic = FilterLogic.None)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Logic = logic;
        }

        /// <summary>Condition or nested group.</summary>
        public FilterExpression Expression { get; }

        /// <summary>Operator that joins this item to the next one.</summary>
        public FilterLogic Logic { get; set; }
    }
}
