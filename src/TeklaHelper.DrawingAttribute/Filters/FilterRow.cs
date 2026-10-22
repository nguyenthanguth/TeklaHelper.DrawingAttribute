using System;
using System.Globalization;

namespace TeklaHelper.DrawingAttribute.Filters
{
    /// <summary>
    /// One row of a filter file (a <c>SECTION_OBJECT_GROUP</c>), i.e. one line of the Tekla filter dialog. A row is ten
    /// tokens: opening parentheses, enabled, category, property, property label, condition, condition label, value,
    /// closing parentheses and the And/Or operator to the next row. The text properties cannot contain line breaks.
    /// </summary>
    public sealed class FilterRow
    {
        internal const int TokenCount = 10;

        private string _category = string.Empty;
        private string _property = string.Empty;
        private string _propertyLabel = string.Empty;
        private string _condition = string.Empty;
        private string _conditionLabel = string.Empty;
        private string _value = string.Empty;
        private int _openParentheses;
        private int _closeParentheses;

        // The token lines as read from a file and the tokens they held, so an unchanged token is written back unchanged
        private byte[][] _sourceLines;
        private string[] _sourceTokens;
        private int _sourceCodePage;

        /// <summary>
        /// Creates an empty row.
        /// </summary>
        public FilterRow()
        {
        }

        /// <summary>
        /// Creates a row for <paramref name="condition"/>.
        /// </summary>
        public FilterRow(FilterCondition condition, FilterLogic logic = FilterLogic.None)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            Enabled = condition.Enabled;
            Category = condition.Category;
            Property = condition.Property;
            PropertyLabel = condition.PropertyLabel;
            Condition = condition.Operator.Symbol;
            ConditionLabel = condition.Operator.Label;
            Value = condition.Value;
            Logic = logic;
        }

        /// <summary>Number of parentheses opened before this row.</summary>
        public int OpenParentheses
        {
            get => _openParentheses;
            set => _openParentheses = value >= 0 ? value : throw new ArgumentOutOfRangeException(nameof(value));
        }

        /// <summary>Whether the row is used (the check box in front of the row).</summary>
        public bool Enabled { get; set; } = true;

        /// <summary>Category token, e.g. <c>co_part</c>.</summary>
        public string Category
        {
            get => _category;
            set => _category = FilterToken.Check(value, nameof(value)) ?? string.Empty;
        }

        /// <summary>Property token, e.g. <c>proNAME</c>, or the name of a user-defined attribute.</summary>
        public string Property
        {
            get => _property;
            set => _property = FilterToken.Check(value, nameof(value)) ?? string.Empty;
        }

        /// <summary>Label key of the property, e.g. <c>albl_Name</c>.</summary>
        public string PropertyLabel
        {
            get => _propertyLabel;
            set => _propertyLabel = FilterToken.Check(value, nameof(value)) ?? string.Empty;
        }

        /// <summary>Condition symbol, e.g. <c>==</c> or <c>*CONTAINS*</c>.</summary>
        public string Condition
        {
            get => _condition;
            set => _condition = FilterToken.Check(value, nameof(value)) ?? string.Empty;
        }

        /// <summary>Label key of the condition, e.g. <c>albl_Equals</c>.</summary>
        public string ConditionLabel
        {
            get => _conditionLabel;
            set => _conditionLabel = FilterToken.Check(value, nameof(value)) ?? string.Empty;
        }

        /// <summary>
        /// Value as written in the file, with spaces where the file has its space marker (<c>\u0002</c>).
        /// See <see cref="FilterCondition.Value"/> for the formats.
        /// </summary>
        public string Value
        {
            get => _value;
            set => _value = FilterToken.Check(value, nameof(value)) ?? string.Empty;
        }

        /// <summary>Number of parentheses closed after this row.</summary>
        public int CloseParentheses
        {
            get => _closeParentheses;
            set => _closeParentheses = value >= 0 ? value : throw new ArgumentOutOfRangeException(nameof(value));
        }

        /// <summary>Operator that joins this row to the next one.</summary>
        public FilterLogic Logic { get; set; }

        /// <summary>
        /// The condition of the row: symbol and label.
        /// </summary>
        public FilterOperator Operator
        {
            get => new FilterOperator(Condition, ConditionLabel);
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                Condition = value.Symbol;
                ConditionLabel = value.Label;
            }
        }

        /// <summary>
        /// Returns the condition of the row, without parentheses and operator.
        /// </summary>
        public FilterCondition ToCondition() =>
            new FilterCondition(Category, Property, PropertyLabel, Operator, Value) { Enabled = Enabled };

        /// <inheritdoc />
        public override string ToString() =>
            new string('(', OpenParentheses) + (Enabled ? string.Empty : "[off] ") + $"{Category}.{Property} {Condition} {Value}"
            + new string(')', CloseParentheses) + " " + FormatLogic(Logic);

        internal static string FormatLogic(FilterLogic logic)
        {
            switch (logic)
            {
                case FilterLogic.And:
                    return "&&";
                case FilterLogic.Or:
                    return "||";
                default:
                    return "Empty";
            }
        }

        internal static FilterRow FromTokens(string[] tokens, byte[][] lines, int codePage)
        {
            var row = new FilterRow
            {
                OpenParentheses = ParseCount(tokens[0], "opening parentheses"),
                Enabled = tokens[1] != "0",
                Category = tokens[2],
                Property = tokens[3],
                PropertyLabel = tokens[4],
                Condition = tokens[5],
                ConditionLabel = tokens[6],
                Value = tokens[7],
                CloseParentheses = ParseCount(tokens[8], "closing parentheses"),
                Logic = ParseLogic(tokens[9]),
            };

            row._sourceLines = lines;
            row._sourceTokens = tokens;
            row._sourceCodePage = codePage;
            return row;
        }

        internal string[] GetTokens() => new[]
        {
            OpenParentheses.ToString(CultureInfo.InvariantCulture),
            Enabled ? "1" : "0",
            Category,
            Property,
            PropertyLabel,
            Condition,
            ConditionLabel,
            Value,
            CloseParentheses.ToString(CultureInfo.InvariantCulture),
            FormatLogic(Logic),
        };

        /// <summary>
        /// Returns the line a token was read from, if the token has not changed since and the file keeps its code page.
        /// </summary>
        internal byte[] GetSourceLine(int index, string token, int codePage)
        {
            if (_sourceLines == null || codePage != _sourceCodePage || !string.Equals(_sourceTokens[index], token, StringComparison.Ordinal))
            {
                return null;
            }

            return _sourceLines[index];
        }

        private static int ParseCount(string token, string what)
        {
            if (!int.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out int count))
            {
                throw new FormatException($"Filter row has '{token}' where the number of {what} belongs.");
            }

            return count;
        }

        private static FilterLogic ParseLogic(string token)
        {
            switch (token)
            {
                case "&&":
                    return FilterLogic.And;
                case "||":
                    return FilterLogic.Or;
                case "Empty":
                    return FilterLogic.None;
                default:
                    throw new FormatException($"Filter row has '{token}' where &&, || or Empty belongs.");
            }
        }
    }
}
