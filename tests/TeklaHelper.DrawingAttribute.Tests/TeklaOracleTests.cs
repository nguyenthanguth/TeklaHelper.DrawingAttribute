#if TEKLA_ORACLE
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TeklaHelper.DrawingAttribute.Filters;
using Xunit;
using T = Tekla.Structures.Filtering;
using TC = Tekla.Structures.Filtering.Categories;

namespace TeklaHelper.DrawingAttribute.Tests
{
    /// <summary>
    /// Compares the filter files and the filter catalog with what Tekla.Structures.Filtering itself writes. Compiled only
    /// on net48 when Tekla Structures 2025 is installed (see the test project file).
    /// </summary>
    public sealed class TeklaOracleTests : IDisposable
    {
        private readonly string _directory = Path.Combine(Path.GetTempPath(), "TeklaHelper.DrawingAttribute.Tests", Guid.NewGuid().ToString("N"));
        private readonly CultureInfo _previousCulture = CultureInfo.CurrentCulture;

        static TeklaOracleTests()
        {
            // Tekla.Structures.dll is not copied next to the tests; it and its dependencies load from the Tekla bin folder
            string bin = typeof(TeklaOracleTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                .First(a => a.Key == "TeklaBinDirectory").Value;
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                string path = Path.Combine(bin, new AssemblyName(e.Name).Name + ".dll");
                return File.Exists(path) ? Assembly.LoadFrom(path) : null;
            };
        }

        public TeklaOracleTests()
        {
            Directory.CreateDirectory(_directory);

            // Tekla.Structures.Filtering formats numbers and dates with the current culture; the library always uses the
            // invariant one, like the files the Tekla dialogs write
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _previousCulture;
            if (Directory.Exists(_directory))
            {
                Directory.Delete(_directory, recursive: true);
            }
        }

        [Fact]
        public void Catalog_MatchesTeklaStructuresFiltering() => CheckCatalog();

        [Fact]
        public void Writer_MatchesTeklaStructuresFiltering() => CheckWriter();

        [Fact]
        public void TeklaReadsTheFilesTheLibraryWrites() => CheckTeklaReads();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void CheckCatalog()
        {
            Assembly tekla = typeof(T.Filter).Assembly;
            var failures = new List<string>();
            int checkedCount = 0;

            foreach (Type outer in tekla.GetTypes().Where(t => t.IsPublic && t.Namespace == "Tekla.Structures.Filtering.Categories"))
            {
                string categoryName = outer.Name.Substring(0, outer.Name.Length - "FilterExpressions".Length);
                Type ours = typeof(FilterProperties).GetNestedType(categoryName);
                if (ours == null)
                {
                    failures.Add($"FilterProperties.{categoryName} is missing");
                    continue;
                }

                foreach (Type nested in outer.GetNestedTypes().Where(t => t.IsNestedPublic && typeof(T.DataFilterExpression).IsAssignableFrom(t)))
                {
                    bool custom = nested.GetConstructor(Type.EmptyTypes) == null;
                    var teklaExpression = (T.DataFilterExpression)(custom ? Activator.CreateInstance(nested, "MY_UDA") : Activator.CreateInstance(nested));
                    FilterRow expected = WriteWithTekla(Condition(teklaExpression)).Rows.Single();

                    FilterProperty property;
                    if (custom)
                    {
                        string method = nested.Name == "CustomDateTime" ? "CustomDateTime" : nested.Name;
                        property = (FilterProperty)ours.GetMethod(method)?.Invoke(null, new object[] { "MY_UDA" });
                    }
                    else
                    {
                        property = (FilterProperty)ours.GetField(nested.Name)?.GetValue(null);
                    }

                    checkedCount++;
                    if (property == null)
                    {
                        failures.Add($"FilterProperties.{categoryName}.{nested.Name} is missing");
                    }
                    else if (property.Category != expected.Category || property.Name != expected.Property || property.Label != expected.PropertyLabel
                        || property.ValueType != ValueTypeOf(teklaExpression))
                    {
                        failures.Add($"{categoryName}.{nested.Name}: Tekla {expected.Category}/{expected.Property}/{expected.PropertyLabel}/{ValueTypeOf(teklaExpression)}, "
                            + $"library {property.Category}/{property.Name}/{property.Label}/{property.ValueType}");
                    }
                }
            }

            int ourCount = typeof(FilterProperties).GetNestedTypes()
                .Sum(t => t.GetFields(BindingFlags.Public | BindingFlags.Static).Count(f => typeof(FilterProperty).IsAssignableFrom(f.FieldType))
                    + t.GetMethods(BindingFlags.Public | BindingFlags.Static).Count(m => m.Name.StartsWith("Custom", StringComparison.Ordinal)));

            Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
            Assert.Equal(checkedCount, ourCount);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void CheckWriter()
        {
            var cases = new List<(string Name, T.FilterExpression Tekla, FilterExpression Ours)>
            {
                ("single condition", S("A"), N("A")),
                ("one item", C(I(S("A"))), new FilterGroup().Add(N("A"))),
                ("one item with And", C(I(S("A"), T.BinaryFilterOperatorType.BOOLEAN_AND)), new FilterGroup().Add(N("A"), FilterLogic.And)),
                ("group in the middle",
                    C(I(S("A"), And), I(C(I(S("B"), Or), I(S("C"))), Or), I(S("D"))),
                    new FilterGroup().Add(N("A"), FilterLogic.And).Add(new FilterGroup().Add(N("B"), FilterLogic.Or).Add(N("C")), FilterLogic.Or).Add(N("D"))),
                ("nested groups",
                    C(I(S("A"), And), I(C(I(S("B"), Or), I(C(I(S("C"), Or), I(S("D"), And)), And)), Or), I(S("E"))),
                    new FilterGroup().Add(N("A"), FilterLogic.And)
                        .Add(new FilterGroup().Add(N("B"), FilterLogic.Or).Add(new FilterGroup().Add(N("C"), FilterLogic.Or).Add(N("D"), FilterLogic.And), FilterLogic.And), FilterLogic.Or)
                        .Add(N("E"))),
                ("groups first and last",
                    C(I(C(I(S("A"), Or), I(S("B"))), And), I(C(I(S("C"), Or), I(S("D"))))),
                    new FilterGroup().Add(new FilterGroup().Add(N("A"), FilterLogic.Or).Add(N("B")), FilterLogic.And).Add(new FilterGroup().Add(N("C"), FilterLogic.Or).Add(N("D")))),
                ("disabled row", C(I(S("A"), And), I(Disabled(S("B")))), new FilterGroup().Add(N("A"), FilterLogic.And).Add(DisabledN("B"))),
                ("special text", C(I(S("  a  b\t\"q\" Äö 図面 x  "))), new FilterGroup().Add(N("  a  b\t\"q\" Äö 図面 x  "))),
                ("empty text", C(I(S(string.Empty))), new FilterGroup().Add(N(string.Empty))),
                ("numbers",
                    C(I(Num(new TC.PartFilterExpressions.Phase(), T.NumericOperatorType.GREATER_OR_EQUAL, 2.5), And), I(Num(new TC.WeldFilterExpressions.SizeAboveLine(), T.NumericOperatorType.SMALLER_THAN, 1234567.891))),
                    new FilterGroup().Add(FilterProperties.Part.Phase.GreaterOrEqual(2.5), FilterLogic.And).Add(FilterProperties.Weld.SizeAboveLine.LessThan(1234567.891))),
                ("dates",
                    C(I(new T.BinaryFilterExpression(new TC.TaskFilterExpressions.PlannedStartDate(), T.DateTimeOperatorType.LATER_OR_EQUEL, new T.DateTimeConstantFilterExpression(new DateTime(2026, 10, 21, 13, 45, 30))))),
                    new FilterGroup().Add(FilterProperties.Task.PlannedStartDate.LaterOrEqual(new DateTime(2026, 10, 21, 13, 45, 30)))),
                ("booleans",
                    C(I(new T.BinaryFilterExpression(new TC.AssemblyFilterExpressions.CustomBoolean("MY_BOOL"), T.BooleanOperatorType.IS_NOT_EQUAL, new T.BooleanConstantFilterExpression(false)))),
                    new FilterGroup().Add(FilterProperties.Assembly.CustomBoolean("MY_BOOL").IsNotEqual(false))),
            };

            var stringOperators = new (T.StringOperatorType Tekla, Func<StringFilterProperty, string, FilterCondition> Ours)[]
            {
                (T.StringOperatorType.IS_EQUAL, (p, v) => p.IsEqual(v)),
                (T.StringOperatorType.IS_NOT_EQUAL, (p, v) => p.IsNotEqual(v)),
                (T.StringOperatorType.CONTAINS, (p, v) => p.Contains(v)),
                (T.StringOperatorType.NOT_CONTAINS, (p, v) => p.NotContains(v)),
                (T.StringOperatorType.STARTS_WITH, (p, v) => p.BeginsWith(v)),
                (T.StringOperatorType.NOT_STARTS_WITH, (p, v) => p.NotBeginsWith(v)),
                (T.StringOperatorType.ENDS_WITH, (p, v) => p.EndsWith(v)),
                (T.StringOperatorType.NOT_ENDS_WITH, (p, v) => p.NotEndsWith(v)),
            };
            foreach (var op in stringOperators)
            {
                cases.Add(("string " + op.Tekla, C(I(S("X", op.Tekla))), new FilterGroup().Add(op.Ours(FilterProperties.Part.Name, "X"))));
            }

            var numericOperators = new (T.NumericOperatorType Tekla, Func<NumericFilterProperty, double, FilterCondition> Ours)[]
            {
                (T.NumericOperatorType.IS_EQUAL, (p, v) => p.IsEqual(v)),
                (T.NumericOperatorType.IS_NOT_EQUAL, (p, v) => p.IsNotEqual(v)),
                (T.NumericOperatorType.SMALLER_THAN, (p, v) => p.LessThan(v)),
                (T.NumericOperatorType.SMALLER_OR_EQUAL, (p, v) => p.LessOrEqual(v)),
                (T.NumericOperatorType.GREATER_THAN, (p, v) => p.GreaterThan(v)),
                (T.NumericOperatorType.GREATER_OR_EQUAL, (p, v) => p.GreaterOrEqual(v)),
            };
            foreach (var op in numericOperators)
            {
                cases.Add(("numeric " + op.Tekla, C(I(Num(new TC.PartFilterExpressions.Phase(), op.Tekla, 3))), new FilterGroup().Add(op.Ours(FilterProperties.Part.Phase, 3))));
            }

            var dateOperators = new (T.DateTimeOperatorType Tekla, Func<DateFilterProperty, DateTime, FilterCondition> Ours)[]
            {
                (T.DateTimeOperatorType.IS_EQUAL, (p, v) => p.IsEqual(v)),
                (T.DateTimeOperatorType.IS_NOT_EQUAL, (p, v) => p.IsNotEqual(v)),
                (T.DateTimeOperatorType.EARLIER_THAN, (p, v) => p.EarlierThan(v)),
                (T.DateTimeOperatorType.EARLIER_OR_EQUAL, (p, v) => p.EarlierOrEqual(v)),
                (T.DateTimeOperatorType.LATER_THAN, (p, v) => p.LaterThan(v)),
                (T.DateTimeOperatorType.LATER_OR_EQUEL, (p, v) => p.LaterOrEqual(v)),
            };
            foreach (var op in dateOperators)
            {
                var date = new DateTime(2001, 2, 3);
                cases.Add(("date " + op.Tekla,
                    C(I(new T.BinaryFilterExpression(new TC.TaskFilterExpressions.ActualEndDate(), op.Tekla, new T.DateTimeConstantFilterExpression(date)))),
                    new FilterGroup().Add(op.Ours(FilterProperties.Task.ActualEndDate, date))));
            }

            var failures = new List<string>();
            foreach (var testCase in cases)
            {
                byte[] expected = File.ReadAllBytes(CreateWithTekla(testCase.Tekla));
                byte[] actual = FilterFile.FromExpression(FilterFileExtensions.GADrawingFilter, testCase.Ours).ToBytes();
                if (!expected.SequenceEqual(actual))
                {
                    failures.Add(testCase.Name + Environment.NewLine + "Tekla:" + Environment.NewLine + System.Text.Encoding.UTF8.GetString(expected)
                        + Environment.NewLine + "Library:" + Environment.NewLine + System.Text.Encoding.UTF8.GetString(actual));
                }
            }

            Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void CheckTeklaReads()
        {
            // No user-defined attributes: Tekla's own reader throws "operand is not supported" for them, also in files
            // Tekla.Structures.Filtering wrote itself
            var expression = new FilterGroup()
                .Add(FilterProperties.Part.Name.IsEqual("BEAM COLUMN"), FilterLogic.And)
                .Add(new FilterGroup()
                    .Add(FilterProperties.Part.Phase.GreaterOrEqual(2), FilterLogic.Or)
                    .Add(FilterProperties.Assembly.Prefix.Contains("A")), FilterLogic.And)
                .Add(FilterProperties.Task.PlannedEndDate.EarlierThan(new DateTime(2026, 12, 31)));
            FilterFile ours = FilterFile.FromExpression(FilterFileExtensions.GADrawingFilter, expression);
            string path = ours.SaveTo(_directory, "library");

            var read = new T.Filter(path, CultureInfo.InvariantCulture);
            byte[] rewritten = File.ReadAllBytes(read.CreateFile(T.FilterExpressionFileType.DRAWING_GENERAL, Path.Combine(_directory, "rewritten")));

            Assert.Equal(ours.ToBytes(), rewritten);
        }

        private string CreateWithTekla(T.FilterExpression expression) =>
            new T.Filter(expression).CreateFile(T.FilterExpressionFileType.DRAWING_GENERAL, Path.Combine(_directory, Guid.NewGuid().ToString("N")));

        private FilterFile WriteWithTekla(T.FilterExpression expression) => FilterFile.FromFile(CreateWithTekla(expression));

        private static FilterValueType ValueTypeOf(T.DataFilterExpression expression)
        {
            switch (expression)
            {
                case T.StringFilterExpression _:
                    return FilterValueType.String;
                case T.NumericFilterExpression _:
                    return FilterValueType.Number;
                case T.DateTimeFilterExpression _:
                    return FilterValueType.Date;
                default:
                    return FilterValueType.Boolean;
            }
        }

        private static T.FilterExpression Condition(T.DataFilterExpression left)
        {
            switch (left)
            {
                case T.StringFilterExpression s:
                    return new T.BinaryFilterExpression(s, T.StringOperatorType.IS_EQUAL, new T.StringConstantFilterExpression("V"));
                case T.NumericFilterExpression n:
                    return new T.BinaryFilterExpression(n, T.NumericOperatorType.IS_EQUAL, new T.NumericConstantFilterExpression(1));
                case T.DateTimeFilterExpression d:
                    return new T.BinaryFilterExpression(d, T.DateTimeOperatorType.IS_EQUAL, new T.DateTimeConstantFilterExpression(new DateTime(2001, 2, 3)));
                default:
                    return new T.BinaryFilterExpression((T.BooleanFilterExpression)left, T.BooleanOperatorType.IS_EQUAL, new T.BooleanConstantFilterExpression(true));
            }
        }

        private static T.BinaryFilterOperatorType And => T.BinaryFilterOperatorType.BOOLEAN_AND;

        private static T.BinaryFilterOperatorType Or => T.BinaryFilterOperatorType.BOOLEAN_OR;

        private static T.BinaryFilterExpression S(string value, T.StringOperatorType op = T.StringOperatorType.IS_EQUAL) =>
            new T.BinaryFilterExpression(new TC.PartFilterExpressions.Name(), op, new T.StringConstantFilterExpression(value));

        private static T.BinaryFilterExpression Num(T.NumericFilterExpression left, T.NumericOperatorType op, double value) =>
            new T.BinaryFilterExpression(left, op, new T.NumericConstantFilterExpression(value));

        private static T.BinaryFilterExpression Disabled(T.BinaryFilterExpression expression)
        {
            expression.IsEnable = false;
            return expression;
        }

        private static T.BinaryFilterExpressionItem I(T.FilterExpression expression, T.BinaryFilterOperatorType? op = null) =>
            op == null ? new T.BinaryFilterExpressionItem(expression) : new T.BinaryFilterExpressionItem(expression, op.Value);

        private static T.BinaryFilterExpressionCollection C(params T.BinaryFilterExpressionItem[] items)
        {
            var collection = new T.BinaryFilterExpressionCollection();
            foreach (T.BinaryFilterExpressionItem item in items)
            {
                collection.Add(item);
            }

            return collection;
        }

        private static FilterCondition N(string value) => FilterProperties.Part.Name.IsEqual(value);

        private static FilterCondition DisabledN(string value)
        {
            FilterCondition condition = N(value);
            condition.Enabled = false;
            return condition;
        }
    }
}
#endif
