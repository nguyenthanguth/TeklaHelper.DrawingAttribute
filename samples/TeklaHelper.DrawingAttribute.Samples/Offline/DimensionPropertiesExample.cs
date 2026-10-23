using System;
using TeklaHelper.DrawingAttribute.DrawingObjects;
using TeklaHelper.DrawingAttribute.Pages;

namespace TeklaHelper.DrawingAttribute.Samples.Offline
{
    /// <summary>
    /// Dimension properties (.dim), the file of the Dimension properties dialog of the drawing editor, which the Open API
    /// reads with <c>new StraightDimensionSet.StraightDimensionSetAttributes(name)</c>: dimension type, format, text,
    /// arrows, exaggeration, tags, and the marks around the value.
    /// </summary>
    internal sealed class DimensionPropertiesExample : Example
    {
        public override string Title => "Dimension properties (.dim): type, format, text, arrows, tags";

        public override string Description => "DimensionAttributeFile with its General / Appearance / Marks / Tags tabs and the nine dimension marks.";

        public override void Run(ExampleContext context)
        {
            context.Step("Absolute dimensions, 1/8 precision, two decimals, short dimensions inside");
            DimensionAttributeFile dimension = DimensionAttributeFile.CreateDefault();
            DimensionProperties properties = dimension.Properties;
            properties.General.DimensionTypes.Straight = StraightDimensionType.Absolute;
            properties.General.DimensionFormat.Units = DimensionUnits.Millimeters;
            properties.General.DimensionFormat.Precision = DimensionPrecision.OnePerEight;
            properties.General.DimensionFormat.Format = DimensionFormat.TwoDecimals;
            properties.General.Placing.DimensionLinesSpacing = 8;
            properties.General.Placing.ShortDimensions = ShortDimensionPlacing.Inside;

            context.Step("Appearance: red text 3.5 high in a rounded frame, filled arrows");
            properties.Appearance.Text.Font = "Arial";
            properties.Appearance.Text.Height = 3.5;
            properties.Appearance.Text.Color = DrawingColor.Red;
            properties.Appearance.Text.Frame = DimensionTextFrame.RoundedRectangle;
            properties.Appearance.LineArrow.Color = DrawingColor.Red;
            properties.Appearance.LineArrow.Height = 1.5;
            properties.Appearance.LineArrow.Width = 3;

            context.Step("Marks tab: a prefix and a postfix around the value, exaggeration of short dimensions");
            properties.Marks.DimensionMarkContents.APrefix = "~";
            properties.Marks.DimensionMarkContents.Postfix = " mm";
            properties.Marks.Exaggeration.Exaggeration = Exaggeration.Specified;
            properties.Marks.Exaggeration.Width = 7;

            context.Step("Tags: texts at the corners of the dimension line, and the font of the prefix mark");
            properties.Tags.Tags.ALeftUpperTag = "A";
            properties.Tags.Tags.ARightUpperTag = "B";
            dimension.PrefixMark.Font.Font = "Arial Narrow";
            dimension.PrefixMark.Font.Height = 2.5;
            dimension.PrefixMark.FrameAroundElements.Type = FrameAroundElementsType.Line;   // underline

            string path = dimension.SaveTo(context.OutputDirectory, "sample_dimension");
            context.ShowFile(path, 16);

            context.Step("Read it back, and change one value of a file that already exists");
            var loaded = AttributeFile.FromFile<DimensionAttributeFile>(path);
            Console.WriteLine($"    Type {loaded.Properties.General.DimensionTypes.Straight}, precision {loaded.Properties.General.DimensionFormat.Precision}, " +
                              $"text {loaded.Properties.Appearance.Text.Height} {loaded.Properties.Appearance.Text.Color}, frame {loaded.Properties.Appearance.Text.Frame}");
            loaded.Properties.General.DimensionTypes.Straight = StraightDimensionType.RelativeAndAbsolute;
            loaded.SaveTo(context.OutputDirectory, "sample_dimension_relative_absolute");

            Console.WriteLine();
            Console.WriteLine("In Tekla: new StraightDimensionSetHandler().CreateDimensionSet(view, points, direction, distance,");
            Console.WriteLine("              new StraightDimensionSet.StraightDimensionSetAttributes(\"sample_dimension\"))");
            Console.WriteLine("          or Load \"sample_dimension\" in the Dimension properties dialog.");
        }
    }
}
