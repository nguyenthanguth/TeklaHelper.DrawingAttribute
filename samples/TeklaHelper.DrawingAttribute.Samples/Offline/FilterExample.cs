using System;
using System.IO;
using TeklaHelper.DrawingAttribute.Filters;
using TeklaHelper.DrawingAttribute.Views;

namespace TeklaHelper.DrawingAttribute.Samples.Offline
{
    /// <summary>
    /// Filters: expressions with groups, every value type, user-defined attributes, the neighbor part filter, reading a
    /// filter back as an expression, editing rows, and stand-alone filter files.
    /// </summary>
    internal sealed class FilterExample : Example
    {
        public override string Title => "Filters: expressions, rows, neighbor part filter, filter files";

        public override string Description => "Build view filters with groups, numbers, dates, UDAs; read and edit them; write .vf / .SObjGrp files.";

        public override void Run(ExampleContext context)
        {
            context.Step("An expression: (name is BEAM or COLUMN) and profile begins with HEA and phase >= 2 and not a UDA value");

            // FilterProperties is the catalog of Tekla.Structures.Filtering.Categories: one class per category, one
            // field per property, typed (string, number, date, boolean). Custom* are user-defined attributes.
            FilterExpression expression = new FilterGroup()
                .Add(new FilterGroup()
                    .Add(FilterProperties.Part.Name.IsEqual("BEAM"), FilterLogic.Or)
                    .Add(FilterProperties.Part.Name.IsEqual("COLUMN")), FilterLogic.And)
                .Add(FilterProperties.Part.Profile.BeginsWith("HEA"), FilterLogic.And)
                .Add(FilterProperties.Part.Phase.GreaterOrEqual(2), FilterLogic.And)
                .Add(FilterProperties.Assembly.CustomString("USER_FIELD_1").NotContains("TEMP"), FilterLogic.And)
                .Add(FilterProperties.Part.CustomDateTime("FABRICATION_DATE").LaterOrEqual(new DateTime(2026, 1, 1)));

            var view = new ViewProperties();
            view.Attributes.Attributes1.View.Scale = 50;
            view.Filter.SetExpression(expression);

            context.Step("Neighbor part filter: only plates are drawn as neighbor parts");
            view.NeighborPartFilter.SetExpression(FilterProperties.Part.Profile.BeginsWith("PL"));

            string path = view.SaveTo(context.OutputDirectory, "sample_filtered_50");
            context.ShowFile(path + ".filter", 30);
            context.ShowFile(path + ".npfilter", 30);

            context.Step("Read the filter back: rows, and the expression tree");
            var loaded = AttributeFile.FromFile<ViewProperties>(path);
            foreach (FilterRow row in loaded.Filter.Rows)
            {
                Console.WriteLine("    " + row);
            }

            Console.WriteLine("    Expression: " + Describe(loaded.Filter.ToExpression()));

            context.Step("Edit a row in place (the other rows keep their bytes) and save under another name");
            loaded.Filter.Rows[0].Value = "GIRDER";
            loaded.Filter.Rows.Add(new FilterRow(FilterProperties.Part.Material.IsNotEqual("S235")));
            loaded.SaveTo(context.OutputDirectory, "sample_filtered_girders");
            Console.WriteLine("    " + string.Join(Environment.NewLine + "    ", loaded.Filter.Rows));

            context.Step("Stand-alone filter files: a view filter (.vf, the Filter page of the dialog) and a selection filter");
            FilterFile.FromExpression(FilterFileExtensions.ViewFilter, expression).SaveTo(context.OutputDirectory, "sample_beams");
            FilterFile.FromExpression(FilterFileExtensions.ModelSelectionFilter, FilterProperties.Part.Name.IsEqual("BEAM"))
                .SaveTo(context.OutputDirectory, "sample_beams");
            context.ShowFile(Path.Combine(context.OutputDirectory, "sample_beams" + FilterFileExtensions.ModelSelectionFilter), 30);

            context.Step("Load any filter file and turn it into an expression");
            FilterFile file = FilterFile.FromFile(Path.Combine(context.OutputDirectory, "sample_beams.vf"));
            Console.WriteLine($"    {file.Rows.Count} rows, version {file.Version}: {Describe(file.ToExpression())}");
        }

        private static string Describe(FilterExpression expression)
        {
            switch (expression)
            {
                case FilterCondition condition:
                    return condition.ToString();
                case FilterGroup group:
                    var text = new System.Text.StringBuilder("(");
                    foreach (FilterGroupItem item in group)
                    {
                        text.Append(Describe(item.Expression));
                        if (item.Logic != FilterLogic.None)
                        {
                            text.Append(item.Logic == FilterLogic.And ? " AND " : " OR ");
                        }
                    }

                    return text.Append(')').ToString();
                default:
                    return "(no filter)";
            }
        }
    }
}
