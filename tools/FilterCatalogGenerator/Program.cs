using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using TeklaHelper.DrawingAttribute.Filters;

namespace FilterCatalogGenerator
{
    /// <summary>
    /// Writes FilterProperties.cs: one nested class per Tekla.Structures.Filtering.Categories class, with the tokens Tekla
    /// writes for each property. The tokens are not in the Tekla API, so every property is written to a filter file with
    /// Filter.CreateFile and the file is read back.
    /// </summary>
    internal static class Program
    {
        private static readonly Dictionary<string, string> Descriptions = new Dictionary<string, string>
        {
            ["Assembly"] = "assemblies",
            ["Bolt"] = "bolts",
            ["Component"] = "components",
            ["ConstructionObject"] = "construction objects",
            ["Load"] = "loads",
            ["LogicalArea"] = "locations (logical building areas)",
            ["Object"] = "any model object",
            ["ObjectTypes"] = "object types",
            ["Part"] = "parts",
            ["PourBreak"] = "pour breaks",
            ["PourObject"] = "pour objects",
            ["PourUnit"] = "pour units",
            ["ReferenceObject"] = "reference objects",
            ["ReinforcingBar"] = "reinforcement",
            ["Surface"] = "surface treatments",
            ["Task"] = "tasks",
            ["Template"] = "template attributes",
            ["Weld"] = "welds",
        };

        private static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine("Usage: FilterCatalogGenerator <output FilterProperties.cs> <Tekla version, e.g. 2025>");
                return 2;
            }

            string bin = typeof(Program).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().First(a => a.Key == "TeklaBinDirectory").Value;
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                string path = Path.Combine(bin, new AssemblyName(e.Name).Name + ".dll");
                return File.Exists(path) ? Assembly.LoadFrom(path) : null;
            };

            Generate(args[0], args[1]);
            return 0;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Generate(string outputPath, string teklaVersion)
        {
            // Tekla.Structures.Filtering formats values with the current culture; the tokens do not depend on it
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            string work = Path.Combine(Path.GetTempPath(), "FilterCatalogGenerator", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(work);
            try
            {
                Assembly tekla = typeof(Tekla.Structures.Filtering.Filter).Assembly;
                var lines = new List<string>();
                void W(string line) => lines.Add(line);

                W($"// Generated from Tekla.Structures.Filtering.Categories of Tekla Structures {teklaVersion} (Tekla.Structures.dll {teklaVersion}.0) by");
                W("// writing one filter per property with Filter.CreateFile and reading the tokens back (tools/FilterCatalogGenerator).");
                W("// TeklaOracleTests checks it against Tekla.Structures.dll when Tekla Structures is installed.");
                W(string.Empty);
                W("namespace TeklaHelper.DrawingAttribute.Filters");
                W("{");
                W("    /// <summary>");
                W("    /// The filter properties of <c>Tekla.Structures.Filtering.Categories</c>, with the tokens Tekla writes for them. Each");
                W("    /// nested class is one category (<c>PartFilterExpressions</c> is <see cref=\"Part\"/>), and has CustomString,");
                W("    /// CustomNumber, CustomDateTime and CustomBoolean for user-defined attributes:");
                W("    /// <code>");
                W("    /// FilterProperties.Part.Name.IsEqual(\"BEAM\")");
                W("    /// FilterProperties.Part.Phase.GreaterOrEqual(2)");
                W("    /// FilterProperties.Assembly.CustomString(\"USER_FIELD_1\").Contains(\"A\")");
                W("    /// </code>");
                W("    /// The value types are those of the Tekla API, which is why for example <see cref=\"Assembly.Phase\"/> is text.");
                W("    /// </summary>");
                W("    public static class FilterProperties");
                W("    {");

                bool first = true;
                foreach (Type outer in tekla.GetTypes().Where(t => t.IsPublic && t.Namespace == "Tekla.Structures.Filtering.Categories").OrderBy(t => t.Name, StringComparer.Ordinal))
                {
                    string name = outer.Name.Substring(0, outer.Name.Length - "FilterExpressions".Length);
                    var properties = new List<(string Name, string Kind, FilterRow Row)>();
                    string category = null;
                    foreach (Type nested in outer.GetNestedTypes().Where(t => t.IsNestedPublic && typeof(Tekla.Structures.Filtering.DataFilterExpression).IsAssignableFrom(t)).OrderBy(t => t.Name, StringComparer.Ordinal))
                    {
                        bool custom = nested.GetConstructor(Type.EmptyTypes) == null;
                        var expression = (Tekla.Structures.Filtering.DataFilterExpression)(custom ? Activator.CreateInstance(nested, "MY_UDA") : Activator.CreateInstance(nested));
                        string file = new Tekla.Structures.Filtering.Filter(Condition(expression))
                            .CreateFile(Tekla.Structures.Filtering.FilterExpressionFileType.DRAWING_GENERAL, Path.Combine(work, outer.Name + "." + nested.Name));
                        FilterRow row = FilterFile.FromFile(file).Rows.Single();
                        if (custom)
                        {
                            category = row.Category;
                        }
                        else
                        {
                            properties.Add((nested.Name, PropertyType(expression), row));
                        }
                    }

                    category = category ?? properties.First().Row.Category;
                    if (!first)
                    {
                        W(string.Empty);
                    }

                    first = false;
                    W($"        /// <summary>Properties of {Descriptions[name]} (<c>{category}</c>), like <c>{outer.Name}</c>.</summary>");
                    W($"        public static class {name}");
                    W("        {");
                    W("            /// <summary>Category token.</summary>");
                    W($"            public const string Category = \"{category}\";");
                    foreach (var property in properties)
                    {
                        if (property.Row.Category != category)
                        {
                            throw new InvalidOperationException($"{name}.{property.Name} has category {property.Row.Category}, not {category}.");
                        }

                        W(string.Empty);
                        W($"            /// <summary>{Words(property.Name)} (<c>{property.Row.Property}</c>).</summary>");
                        W($"            public static readonly {property.Kind} {property.Name} = new {property.Kind}(Category, \"{property.Row.Property}\", \"{property.Row.PropertyLabel}\");");
                    }

                    foreach (var (method, kind, what) in new[]
                    {
                        ("CustomString", "StringFilterProperty", "text"),
                        ("CustomNumber", "NumericFilterProperty", "numeric"),
                        ("CustomDateTime", "DateFilterProperty", "date"),
                        ("CustomBoolean", "BooleanFilterProperty", "true / false"),
                    })
                    {
                        W(string.Empty);
                        W($"            /// <summary>A {what} user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>");
                        W($"            public static {kind} {method}(string name) => new {kind}(Category, name);");
                    }

                    W("        }");
                }

                W("    }");
                W("}");
                File.WriteAllText(outputPath, string.Join("\n", lines) + "\n", new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
                Console.WriteLine("Written " + outputPath);
            }
            finally
            {
                Directory.Delete(work, recursive: true);
            }
        }

        private static Tekla.Structures.Filtering.FilterExpression Condition(Tekla.Structures.Filtering.DataFilterExpression left)
        {
            switch (left)
            {
                case Tekla.Structures.Filtering.StringFilterExpression s:
                    return new Tekla.Structures.Filtering.BinaryFilterExpression(s, Tekla.Structures.Filtering.StringOperatorType.IS_EQUAL, new Tekla.Structures.Filtering.StringConstantFilterExpression("V"));
                case Tekla.Structures.Filtering.NumericFilterExpression n:
                    return new Tekla.Structures.Filtering.BinaryFilterExpression(n, Tekla.Structures.Filtering.NumericOperatorType.IS_EQUAL, new Tekla.Structures.Filtering.NumericConstantFilterExpression(1));
                case Tekla.Structures.Filtering.DateTimeFilterExpression d:
                    return new Tekla.Structures.Filtering.BinaryFilterExpression(d, Tekla.Structures.Filtering.DateTimeOperatorType.IS_EQUAL, new Tekla.Structures.Filtering.DateTimeConstantFilterExpression(new DateTime(2001, 2, 3)));
                default:
                    return new Tekla.Structures.Filtering.BinaryFilterExpression((Tekla.Structures.Filtering.BooleanFilterExpression)left, Tekla.Structures.Filtering.BooleanOperatorType.IS_EQUAL, new Tekla.Structures.Filtering.BooleanConstantFilterExpression(true));
            }
        }

        private static string PropertyType(Tekla.Structures.Filtering.DataFilterExpression expression)
        {
            switch (expression)
            {
                case Tekla.Structures.Filtering.StringFilterExpression _:
                    return "StringFilterProperty";
                case Tekla.Structures.Filtering.NumericFilterExpression _:
                    return "NumericFilterProperty";
                case Tekla.Structures.Filtering.DateTimeFilterExpression _:
                    return "DateFilterProperty";
                default:
                    return "BooleanFilterProperty";
            }
        }

        private static string Words(string name)
        {
            string spaced = Regex.Replace(name, "(?<=[a-z])(?=[A-Z])", " ");
            return spaced.Substring(0, 1) + spaced.Substring(1).ToLowerInvariant();
        }
    }
}
