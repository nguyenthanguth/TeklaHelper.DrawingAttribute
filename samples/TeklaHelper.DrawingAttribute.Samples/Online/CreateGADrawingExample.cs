using System;
using System.IO;
using System.Linq;
using Tekla.Structures.Drawing;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using TeklaHelper.DrawingAttribute.Drawings;
using TeklaHelper.DrawingAttribute.Filters;
using TeklaHelper.DrawingAttribute.Pages;
using TeklaHelper.DrawingAttribute.Views;
using Grid = TeklaHelper.DrawingAttribute.Pages.Grid;
using TSD = Tekla.Structures.Drawing;

namespace TeklaHelper.DrawingAttribute.Samples.Online
{
    /// <summary>
    /// The reason for this library: the Drawing API only sees the objects a view shows, so the view has to be created
    /// with the right object settings and filter. This example writes a .gd and two .vi files into the model's attributes
    /// folder, creates a GA drawing with them and two plan views (all parts, and only the parts with a given name), and
    /// counts the parts the API finds in each view.
    /// </summary>
    internal sealed class CreateGADrawingExample : Example
    {
        public override string Title => "Create a GA drawing with filtered views in the open model";

        public override string Description => "Writes .gd/.vi files to model\\attributes, inserts a GA drawing and two plan views, counts the parts the API sees.";

        public override bool NeedsTekla => true;

        public override void Run(ExampleContext context)
        {
            context.Step("Connect to Tekla Structures");
            Model model = TeklaSession.Connect();
            if (model == null)
            {
                return;
            }

            var handler = new DrawingHandler();
            if (handler.GetActiveDrawing() != null)
            {
                Console.WriteLine("    Close the open drawing first.");
                return;
            }

            AABB box = TeklaSession.PartsBox(model);
            if (box == null)
            {
                Console.WriteLine("    The model has no parts.");
                return;
            }

            Console.WriteLine("    Part names in the model: " + string.Join(", ", TeklaSession.PartNames(model).Select(pair => $"{pair.Key} ({pair.Value})")));
            string partName = context.Ask("Name of the parts the filtered view shows", "BEAM");
            string attributes = TeklaSession.AttributesDirectory(model);
            if (!context.Confirm($"Write tda_sample_* files to {attributes} and create a GA drawing?"))
            {
                Console.WriteLine("    Cancelled.");
                return;
            }

            context.Step("Write the view properties: every part, outline, grid visible");
            var allParts = new ViewProperties();
            allParts.Attributes.Attributes1.View.Scale = 200;
            allParts.Objects.Part.Content.Representation.Representation = Representation.Outline;
            allParts.Objects.Part.Content.HiddenLines.HiddenLines = false;
            allParts.Objects.Grid.Grid = Grid.Visible;
            allParts.Marks.PartMark.General.VisibilityOfMark.VisibilityInView = VisibilityInView.None;
            string allName = TeklaSession.UniqueName("tda_sample_all");
            allParts.SaveTo(attributes, allName);

            context.Step($"Write the view properties: only the parts named {partName}, in red, with hidden lines");
            var filtered = AttributeFile.FromFile<ViewProperties>(Path.Combine(attributes, allName + ViewProperties.FileExtension));
            filtered.Objects.Part.Content.HiddenLines.HiddenLines = true;
            filtered.Objects.Part.Appearance.VisibleLines.Color = DrawingColor.Red;
            filtered.Marks.PartMark.General.VisibilityOfMark.VisibilityInView = VisibilityInView.Always;
            filtered.Filter.SetExpression(FilterProperties.Part.Name.IsEqual(partName));
            string filteredName = TeklaSession.UniqueName("tda_sample_" + partName.ToLowerInvariant());
            filtered.SaveTo(attributes, filteredName);
            context.ShowFile(Path.Combine(attributes, filteredName + FilterFileExtensions.ViewAttributeFilter), 20);

            context.Step("Write the GA drawing properties, with a user-defined attribute");
            var drawingProperties = new GADrawingAttributeFile { Title1 = "TeklaHelper.DrawingAttribute", Title2 = "sample" };
            drawingProperties.UserAttributes.DrawnBy = Environment.UserName;
            string drawingName = TeklaSession.UniqueName("tda_sample");
            drawingProperties.SaveTo(attributes, drawingName);

            context.Step("Create the drawing and open it");
            var drawing = new GADrawing("TDA SAMPLE", drawingName);
            if (!drawing.Insert() || !handler.SetActiveDrawing(drawing, true))
            {
                Console.WriteLine("    Tekla could not create or open the drawing.");
                return;
            }

            context.Step("Insert two plan views with the two view properties");

            // Top view of the whole model: the view looks down the global Z axis
            var restriction = new AABB(
                new Point(box.MinPoint.X - 1000, box.MinPoint.Y - 1000, box.MinPoint.Z - 1000),
                new Point(box.MaxPoint.X + 1000, box.MaxPoint.Y + 1000, box.MaxPoint.Z + 1000));
            TSD.View allView = InsertView(drawing, restriction, allName);
            TSD.View filteredView = InsertView(drawing, restriction, filteredName);
            if (allView == null || filteredView == null)
            {
                Console.WriteLine("    Tekla could not insert the views.");
                return;
            }

            // Side by side on the sheet
            filteredView.Origin = new Point(allView.Origin.X + allView.Width + 50, allView.Origin.Y);
            filteredView.Modify();
            drawing.CommitChanges();

            context.Step("What the Drawing API sees in each view");
            Console.WriteLine($"    {allName}: {TeklaSession.Count(allView, typeof(TSD.Part))} parts");
            Console.WriteLine($"    {filteredName}: {TeklaSession.Count(filteredView, typeof(TSD.Part))} parts (only '{partName}')");
            Console.WriteLine("    The parts a view does not show cannot be found through it: create the view with the right settings.");

            if (context.Confirm("Save and close the drawing?"))
            {
                handler.CloseActiveDrawing(true);
            }
        }

        private static TSD.View InsertView(Drawing drawing, AABB restriction, string attributeFile)
        {
            var view = new TSD.View(drawing.GetSheet(), new CoordinateSystem(), new CoordinateSystem(), restriction, attributeFile);
            return view.Insert() ? view : null;
        }
    }
}
