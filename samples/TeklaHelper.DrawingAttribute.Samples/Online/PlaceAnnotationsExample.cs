using System;
using Tekla.Structures.Drawing;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using TeklaHelper.DrawingAttribute.DrawingObjects;
using TeklaHelper.DrawingAttribute.Pages;
using TSD = Tekla.Structures.Drawing;

namespace TeklaHelper.DrawingAttribute.Samples.Online
{
    /// <summary>
    /// Inserts drawing objects into a view of the open drawing with property files the library wrote: a text (.drtxt), a
    /// line (.gln), a level mark (.lev) and a dimension (.dim); then reads their properties back through the Drawing API.
    /// </summary>
    internal sealed class PlaceAnnotationsExample : Example
    {
        public override string Title => "Insert a text, a line, a level mark and a dimension from library files";

        public override string Description => "new Text / Line / LevelMark (… Attributes(name)) and CreateDimensionSet with a .dim; reads the properties back.";

        public override bool NeedsTekla => true;

        public override void Run(ExampleContext context)
        {
            context.Step("Connect to Tekla Structures and pick a view of the open drawing");
            Model model = TeklaSession.Connect();
            if (model == null)
            {
                return;
            }

            var handler = new DrawingHandler();
            Drawing drawing = handler.GetActiveDrawing();
            if (drawing == null)
            {
                Console.WriteLine("    Open a drawing first (and select a view to use that one).");
                return;
            }

            TSD.View view = TeklaSession.SelectedOrFirstView(handler, drawing);
            if (view == null)
            {
                Console.WriteLine("    The drawing has no view.");
                return;
            }

            Console.WriteLine($"    View '{view.Name}' of drawing '{drawing.Name}', scale 1:{view.Attributes.Scale}");

            context.Step("The property files (written to the model's attributes folder after you confirm; a new name per run: Tekla caches files)");
            TextAttributeFile text = TextAttributeFile.CreateDefault();
            text.Properties.Appearance.Text.Height = 5;
            text.Properties.Appearance.Text.Color = DrawingColor.Blue;
            text.Properties.Appearance.Frame.Type = FrameAroundMarkType.Rectangular;

            LineAttributeFile line = LineAttributeFile.CreateDefault();
            line.Properties.Line.Color = DrawingColor.Red;
            line.Properties.Line.Type = DrawingLineType.Dashed;
            line.Properties.Arrow.Position = ArrowPosition.End;

            LevelMarkAttributeFile level = LevelMarkAttributeFile.CreateDefault();
            level.Properties.General.LevelMarkContent.PrefixForPositiveLevel = PositiveLevelPrefix.Plus;
            level.Properties.Appearance.Text.Color = DrawingColor.Green;

            DimensionAttributeFile dimension = DimensionAttributeFile.CreateDefault();
            dimension.Properties.General.DimensionTypes.Straight = StraightDimensionType.Absolute;
            dimension.Properties.Appearance.Text.Height = 4;
            dimension.Properties.Appearance.Text.Color = DrawingColor.Red;

            if (!context.Confirm($"Write four property files and insert four objects into view '{view.Name}'?"))
            {
                Console.WriteLine("    Cancelled: nothing was written.");
                return;
            }

            string attributes = TeklaSession.AttributesDirectory(model);
            string name = TeklaSession.UniqueName("tda_annotations");
            text.SaveTo(attributes, name);
            line.SaveTo(attributes, name);
            level.SaveTo(attributes, name);
            dimension.SaveTo(attributes, name);

            context.Step("Insert them near the lower left corner of the view");
            AABB box = view.RestrictionBox;
            double step = 20 * view.Attributes.Scale;
            var origin = new Point(box.MinPoint.X, box.MinPoint.Y - step, 0);

            var newText = new Text(view, new Point(origin.X, origin.Y, 0), "TeklaHelper.DrawingAttribute", new Text.TextAttributes(name));
            var newLine = new TSD.Line(view, new Point(origin.X, origin.Y - step, 0), new Point(origin.X + 4 * step, origin.Y - step, 0), new TSD.Line.LineAttributes(name));
            var newLevel = new LevelMark(view, new Point(origin.X + 6 * step, origin.Y, 0), new Point(origin.X + 6 * step, origin.Y - step, 0), new LevelMark.LevelMarkAttributes(name));
            Console.WriteLine($"    Text: {newText.Insert()}, line: {newLine.Insert()}, level mark: {newLevel.Insert()}");

            var points = new PointList { new Point(origin.X, origin.Y - 2 * step, 0), new Point(origin.X + 4 * step, origin.Y - 2 * step, 0) };

            // Tekla 2025 marks the constructor that takes only the file name obsolete in favour of one that also takes the
            // model object the dimension belongs to; a free dimension has none, and this one reads the .dim as before
#pragma warning disable CS0618
            var dimensionAttributes = new StraightDimensionSet.StraightDimensionSetAttributes(name);
#pragma warning restore CS0618
            StraightDimensionSet set = new StraightDimensionSetHandler().CreateDimensionSet(view, points, new Vector(0, -1, 0), step, dimensionAttributes);
            Console.WriteLine($"    Dimension set: {set != null}");
            drawing.CommitChanges();

            context.Step("Read the properties back through the Drawing API");
            newText.Select();
            Console.WriteLine($"    Text: height {newText.Attributes.Font.Height}, colour {newText.Attributes.Font.Color}, frame {newText.Attributes.Frame.Type}");
            newLine.Select();
            Console.WriteLine($"    Line: colour {newLine.Attributes.Line.Color}, type {newLine.Attributes.Line.Type}, arrow at {newLine.Attributes.Arrowhead.ArrowPosition}");
            newLevel.Select();
            Console.WriteLine($"    Level mark: colour {newLevel.Attributes.Font.Color}");
            if (set != null && set.Select() && set.Attributes is StraightDimensionSet.StraightDimensionSetAttributes read)
            {
                Console.WriteLine($"    Dimension: type {read.DimensionType}, text height {read.Text.Font.Height}, colour {read.Text.Font.Color}");
            }

            // Only what the example added goes: the rest of the drawing, saved or not, stays as it was
            if (context.Confirm("Undo (delete the four objects and the property files)?"))
            {
                bool deleted = newText.Delete() & newLine.Delete() & newLevel.Delete() & (set == null || set.Delete());
                drawing.CommitChanges();
                TeklaSession.DeleteFiles(attributes, name);
                Console.WriteLine("    Deleted: " + deleted);
            }
        }
    }
}
