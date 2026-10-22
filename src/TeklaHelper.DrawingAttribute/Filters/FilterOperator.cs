using System;

namespace TeklaHelper.DrawingAttribute.Filters
{
    /// <summary>
    /// The condition column of a filter row: a symbol Tekla evaluates (<c>==</c>, <c>*CONTAINS*</c>, <c>&lt;=</c>, ...) and
    /// the label key the dialog shows for it (<c>albl_Equals</c>, ...). The static instances are the ones
    /// <c>Tekla.Structures.Filtering</c> writes; rows read from files may carry other labels (some shipped files hold a
    /// translated text instead of the key), and only the symbol decides how Tekla filters.
    /// </summary>
    public sealed class FilterOperator : IEquatable<FilterOperator>
    {
        /// <summary>Equals (<c>==</c>), for every value type.</summary>
        public static readonly FilterOperator Equal = new FilterOperator("==", "albl_Equals");

        /// <summary>Does not equal (<c>!=</c>), for every value type.</summary>
        public static readonly FilterOperator NotEqual = new FilterOperator("!=", "albl_DoesNotEqual");

        /// <summary>Text contains (<c>*CONTAINS*</c>).</summary>
        public static readonly FilterOperator Contains = new FilterOperator("*CONTAINS*", "albl_Contains");

        /// <summary>Text does not contain (<c>*NOTCONTAINS*</c>).</summary>
        public static readonly FilterOperator NotContains = new FilterOperator("*NOTCONTAINS*", "albl_DoesNotContain");

        /// <summary>Text begins with (<c>*BEGINS*</c>).</summary>
        public static readonly FilterOperator BeginsWith = new FilterOperator("*BEGINS*", "albl_BeginsWith");

        /// <summary>Text does not begin with (<c>*NOTBEGINS*</c>).</summary>
        public static readonly FilterOperator NotBeginsWith = new FilterOperator("*NOTBEGINS*", "albl_DoesNotBeginW");

        /// <summary>Text ends with (<c>*ENDS*</c>).</summary>
        public static readonly FilterOperator EndsWith = new FilterOperator("*ENDS*", "albl_EndsWith");

        /// <summary>Text does not end with (<c>*NOTENDS*</c>).</summary>
        public static readonly FilterOperator NotEndsWith = new FilterOperator("*NOTENDS*", "albl_DoesNotEnd");

        /// <summary>Number is less than (<c>&lt;</c>).</summary>
        public static readonly FilterOperator LessThan = new FilterOperator("<", "albl_LessThan");

        /// <summary>Number is less than or equal (<c>&lt;=</c>).</summary>
        public static readonly FilterOperator LessOrEqual = new FilterOperator("<=", "albl_LessOrEqual");

        /// <summary>Number is greater than (<c>&gt;</c>).</summary>
        public static readonly FilterOperator GreaterThan = new FilterOperator(">", "albl_GreaterThan");

        /// <summary>Number is greater than or equal (<c>&gt;=</c>).</summary>
        public static readonly FilterOperator GreaterOrEqual = new FilterOperator(">=", "albl_GreaterOrEqual");

        /// <summary>Date is earlier than (<c>&lt;</c>).</summary>
        public static readonly FilterOperator EarlierThan = new FilterOperator("<", "albl_EarlierThan");

        /// <summary>Date is earlier than or equal (<c>&lt;=</c>).</summary>
        public static readonly FilterOperator EarlierOrEqual = new FilterOperator("<=", "albl_EarlierOrEqual");

        /// <summary>Date is later than (<c>&gt;</c>).</summary>
        public static readonly FilterOperator LaterThan = new FilterOperator(">", "albl_LaterThan");

        /// <summary>Date is later than or equal (<c>&gt;=</c>).</summary>
        public static readonly FilterOperator LaterOrEqual = new FilterOperator(">=", "albl_LaterOrEqual");

        /// <summary>
        /// Creates an operator.
        /// </summary>
        /// <param name="symbol">Symbol Tekla evaluates, e.g. <c>==</c>.</param>
        /// <param name="label">Label key shown in the dialog, e.g. <c>albl_Equals</c>.</param>
        public FilterOperator(string symbol, string label)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                throw new ArgumentException("Symbol must not be empty.", nameof(symbol));
            }

            Symbol = FilterToken.Check(symbol, nameof(symbol));
            Label = FilterToken.Check(label, nameof(label)) ?? string.Empty;
        }

        /// <summary>Symbol Tekla evaluates, e.g. <c>==</c> or <c>*CONTAINS*</c>.</summary>
        public string Symbol { get; }

        /// <summary>Label key shown in the dialog, e.g. <c>albl_Equals</c>.</summary>
        public string Label { get; }

        /// <inheritdoc />
        public bool Equals(FilterOperator other) =>
            other != null && string.Equals(Symbol, other.Symbol, StringComparison.Ordinal) && string.Equals(Label, other.Label, StringComparison.Ordinal);

        /// <inheritdoc />
        public override bool Equals(object obj) => Equals(obj as FilterOperator);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (StringComparer.Ordinal.GetHashCode(Symbol) * 397) ^ StringComparer.Ordinal.GetHashCode(Label);
            }
        }

        /// <inheritdoc />
        public override string ToString() => Symbol;
    }
}
