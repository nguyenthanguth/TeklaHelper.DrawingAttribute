using System;

namespace TeklaHelper.DrawingAttribute.Filters
{
    /// <summary>
    /// One condition of a filter: a property of a category compared with a value, e.g. part name equals <c>BEAM</c>.
    /// Usually created from <see cref="FilterProperties"/>: <c>FilterProperties.Part.Name.IsEqual("BEAM")</c>.
    /// </summary>
    public sealed class FilterCondition : FilterExpression
    {
        /// <summary>
        /// Creates a condition on <paramref name="property"/>.
        /// </summary>
        /// <param name="property">Property to compare.</param>
        /// <param name="operator">Condition.</param>
        /// <param name="value">Value as written in the file (see <see cref="Value"/>).</param>
        public FilterCondition(FilterProperty property, FilterOperator @operator, string value)
            : this(
                (property ?? throw new ArgumentNullException(nameof(property))).Category,
                property.Name,
                property.Label,
                @operator,
                value)
        {
        }

        /// <summary>
        /// Creates a condition from the tokens of a filter row, for categories or properties that
        /// <see cref="FilterProperties"/> does not list.
        /// </summary>
        /// <param name="category">Category token, e.g. <c>co_part</c>.</param>
        /// <param name="property">Property token, e.g. <c>proNAME</c>, or the name of a user-defined attribute.</param>
        /// <param name="propertyLabel">Label key of the property, e.g. <c>albl_Name</c>.</param>
        /// <param name="operator">Condition.</param>
        /// <param name="value">Value as written in the file (see <see cref="Value"/>).</param>
        public FilterCondition(string category, string property, string propertyLabel, FilterOperator @operator, string value)
        {
            Category = RequireToken(category, nameof(category));
            Property = RequireToken(property, nameof(property));
            PropertyLabel = FilterToken.Check(propertyLabel, nameof(propertyLabel)) ?? string.Empty;
            Operator = @operator ?? throw new ArgumentNullException(nameof(@operator));
            Value = FilterToken.Check(value, nameof(value)) ?? string.Empty;
        }

        /// <summary>Category token, e.g. <c>co_part</c>.</summary>
        public string Category { get; }

        /// <summary>Property token, e.g. <c>proNAME</c>, or the name of a user-defined attribute.</summary>
        public string Property { get; }

        /// <summary>Label key of the property shown in the dialog, e.g. <c>albl_Name</c>.</summary>
        public string PropertyLabel { get; }

        /// <summary>Condition.</summary>
        public FilterOperator Operator { get; }

        /// <summary>
        /// Value as written in the file. Numbers are invariant (<c>2.5</c>), dates <c>MM/dd/yyyy</c>, booleans
        /// <c>True</c> / <c>False</c>; Tekla writes an empty text as <c>Empty</c>. Spaces are allowed: several values in
        /// one row are separated by spaces, as in the Tekla dialog. Line breaks are not allowed.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Whether the row is used (the check box in front of the row in the Tekla dialog).
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <inheritdoc />
        public override string ToString() => $"{Category}.{Property} {Operator.Symbol} {Value}";

        private static string RequireToken(string token, string parameterName)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Token must not be empty.", parameterName);
            }

            return FilterToken.Check(token, parameterName);
        }
    }
}
