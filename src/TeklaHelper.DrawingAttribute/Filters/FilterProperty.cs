using System;
using System.Globalization;

namespace TeklaHelper.DrawingAttribute.Filters
{
    /// <summary>
    /// A property that filter rows compare, with the tokens Tekla writes for it: category <c>co_part</c>, property
    /// <c>proNAME</c>, label <c>albl_Name</c>. <see cref="FilterProperties"/> lists the properties of
    /// <c>Tekla.Structures.Filtering.Categories</c>; the typed subclasses create conditions with the operators that suit
    /// the value type.
    /// </summary>
    public abstract class FilterProperty
    {
        private protected FilterProperty(string category, string name, string label, FilterValueType valueType)
        {
            if (string.IsNullOrEmpty(category))
            {
                throw new ArgumentException("Category must not be empty.", nameof(category));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name must not be empty.", nameof(name));
            }

            Category = FilterToken.Check(category, nameof(category));
            Name = FilterToken.Check(name, nameof(name));
            Label = FilterToken.Check(label, nameof(label)) ?? name;
            ValueType = valueType;
        }

        /// <summary>Category token, e.g. <c>co_part</c>.</summary>
        public string Category { get; }

        /// <summary>Property token, e.g. <c>proNAME</c>, or the name of a user-defined attribute.</summary>
        public string Name { get; }

        /// <summary>Label key shown in the dialog, e.g. <c>albl_Name</c>; the attribute name for user-defined attributes.</summary>
        public string Label { get; }

        /// <summary>Type of the values the property is compared with.</summary>
        public FilterValueType ValueType { get; }

        /// <summary>
        /// Creates a condition with any operator and a value already in file form.
        /// </summary>
        public FilterCondition Is(FilterOperator @operator, string value) => new FilterCondition(this, @operator, value);

        /// <inheritdoc />
        public override string ToString() => $"{Category}.{Name}";

        // Tekla writes an empty text as the word Empty (Tekla.Structures.Filtering does the same)
        private protected static string FormatString(string value) => string.IsNullOrEmpty(value) ? "Empty" : value;

        // Invariant, like the files the Tekla dialogs write; Tekla.Structures.Filtering would use the current culture
        private protected static string FormatNumber(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Filter values cannot be NaN or infinity.");
            }

            // Negative zero would be "-0" on .NET Core 3.0+ and "0" on .NET Framework
            return (value == 0 ? 0d : value).ToString("G15", CultureInfo.InvariantCulture);
        }

        private protected static string FormatDate(DateTime value) => value.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

        private protected static string FormatBoolean(bool value) => value ? "True" : "False";
    }

    /// <summary>
    /// A text property, e.g. part name or profile.
    /// </summary>
    public sealed class StringFilterProperty : FilterProperty
    {
        /// <summary>
        /// Creates a text property.
        /// </summary>
        /// <param name="category">Category token, e.g. <c>co_part</c>.</param>
        /// <param name="name">Property token, e.g. <c>proNAME</c>, or a user-defined attribute name.</param>
        /// <param name="label">Label key, e.g. <c>albl_Name</c>; defaults to <paramref name="name"/>.</param>
        public StringFilterProperty(string category, string name, string label = null)
            : base(category, name, label, FilterValueType.String)
        {
        }

        /// <summary>Equals <paramref name="value"/>; several values can be given separated by spaces.</summary>
        public FilterCondition IsEqual(string value) => Is(FilterOperator.Equal, FormatString(value));

        /// <summary>Does not equal <paramref name="value"/>.</summary>
        public FilterCondition IsNotEqual(string value) => Is(FilterOperator.NotEqual, FormatString(value));

        /// <summary>Contains <paramref name="value"/>.</summary>
        public FilterCondition Contains(string value) => Is(FilterOperator.Contains, FormatString(value));

        /// <summary>Does not contain <paramref name="value"/>.</summary>
        public FilterCondition NotContains(string value) => Is(FilterOperator.NotContains, FormatString(value));

        /// <summary>Begins with <paramref name="value"/>.</summary>
        public FilterCondition BeginsWith(string value) => Is(FilterOperator.BeginsWith, FormatString(value));

        /// <summary>Does not begin with <paramref name="value"/>.</summary>
        public FilterCondition NotBeginsWith(string value) => Is(FilterOperator.NotBeginsWith, FormatString(value));

        /// <summary>Ends with <paramref name="value"/>.</summary>
        public FilterCondition EndsWith(string value) => Is(FilterOperator.EndsWith, FormatString(value));

        /// <summary>Does not end with <paramref name="value"/>.</summary>
        public FilterCondition NotEndsWith(string value) => Is(FilterOperator.NotEndsWith, FormatString(value));
    }

    /// <summary>
    /// A numeric property, e.g. phase or class.
    /// </summary>
    public sealed class NumericFilterProperty : FilterProperty
    {
        /// <summary>
        /// Creates a numeric property.
        /// </summary>
        /// <param name="category">Category token, e.g. <c>co_part</c>.</param>
        /// <param name="name">Property token, e.g. <c>proPHASE</c>, or a user-defined attribute name.</param>
        /// <param name="label">Label key, e.g. <c>albl_Phase</c>; defaults to <paramref name="name"/>.</param>
        public NumericFilterProperty(string category, string name, string label = null)
            : base(category, name, label, FilterValueType.Number)
        {
        }

        /// <summary>Equals <paramref name="value"/>.</summary>
        public FilterCondition IsEqual(double value) => Is(FilterOperator.Equal, FormatNumber(value));

        /// <summary>Does not equal <paramref name="value"/>.</summary>
        public FilterCondition IsNotEqual(double value) => Is(FilterOperator.NotEqual, FormatNumber(value));

        /// <summary>Is less than <paramref name="value"/>.</summary>
        public FilterCondition LessThan(double value) => Is(FilterOperator.LessThan, FormatNumber(value));

        /// <summary>Is less than or equal to <paramref name="value"/>.</summary>
        public FilterCondition LessOrEqual(double value) => Is(FilterOperator.LessOrEqual, FormatNumber(value));

        /// <summary>Is greater than <paramref name="value"/>.</summary>
        public FilterCondition GreaterThan(double value) => Is(FilterOperator.GreaterThan, FormatNumber(value));

        /// <summary>Is greater than or equal to <paramref name="value"/>.</summary>
        public FilterCondition GreaterOrEqual(double value) => Is(FilterOperator.GreaterOrEqual, FormatNumber(value));
    }

    /// <summary>
    /// A date property, e.g. the planned start of a task. Only the date part is written.
    /// </summary>
    public sealed class DateFilterProperty : FilterProperty
    {
        /// <summary>
        /// Creates a date property.
        /// </summary>
        /// <param name="category">Category token, e.g. <c>co_task</c>.</param>
        /// <param name="name">Property token, e.g. <c>proPlannedStartDate</c>, or a user-defined attribute name.</param>
        /// <param name="label">Label key, e.g. <c>albl_Planned_start_date</c>; defaults to <paramref name="name"/>.</param>
        public DateFilterProperty(string category, string name, string label = null)
            : base(category, name, label, FilterValueType.Date)
        {
        }

        /// <summary>Equals <paramref name="value"/>.</summary>
        public FilterCondition IsEqual(DateTime value) => Is(FilterOperator.Equal, FormatDate(value));

        /// <summary>Does not equal <paramref name="value"/>.</summary>
        public FilterCondition IsNotEqual(DateTime value) => Is(FilterOperator.NotEqual, FormatDate(value));

        /// <summary>Is earlier than <paramref name="value"/>.</summary>
        public FilterCondition EarlierThan(DateTime value) => Is(FilterOperator.EarlierThan, FormatDate(value));

        /// <summary>Is earlier than or equal to <paramref name="value"/>.</summary>
        public FilterCondition EarlierOrEqual(DateTime value) => Is(FilterOperator.EarlierOrEqual, FormatDate(value));

        /// <summary>Is later than <paramref name="value"/>.</summary>
        public FilterCondition LaterThan(DateTime value) => Is(FilterOperator.LaterThan, FormatDate(value));

        /// <summary>Is later than or equal to <paramref name="value"/>.</summary>
        public FilterCondition LaterOrEqual(DateTime value) => Is(FilterOperator.LaterOrEqual, FormatDate(value));
    }

    /// <summary>
    /// A true / false property; Tekla only has these for user-defined attributes.
    /// </summary>
    public sealed class BooleanFilterProperty : FilterProperty
    {
        /// <summary>
        /// Creates a true / false property.
        /// </summary>
        /// <param name="category">Category token, e.g. <c>co_part</c>.</param>
        /// <param name="name">User-defined attribute name.</param>
        /// <param name="label">Label key; defaults to <paramref name="name"/>.</param>
        public BooleanFilterProperty(string category, string name, string label = null)
            : base(category, name, label, FilterValueType.Boolean)
        {
        }

        /// <summary>Equals <paramref name="value"/>.</summary>
        public FilterCondition IsEqual(bool value) => Is(FilterOperator.Equal, FormatBoolean(value));

        /// <summary>Does not equal <paramref name="value"/>.</summary>
        public FilterCondition IsNotEqual(bool value) => Is(FilterOperator.NotEqual, FormatBoolean(value));
    }
}
