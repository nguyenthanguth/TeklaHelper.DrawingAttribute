using System;
using Tekla.Structures.Drawing;
using Tekla.Structures.Model;
using TeklaHelper.DrawingAttribute.DrawingObjects;
using TeklaHelper.DrawingAttribute.Pages;
using TSD = Tekla.Structures.Drawing;

namespace TeklaHelper.DrawingAttribute.Samples.Online
{
    /// <summary>
    /// Restyles the objects of a view of the open drawing with property files of the drawing editor: every part gets a
    /// .dprt (exact, hidden lines, gray), every bolt a .sc (symbol, blue) and the grid a .dgr (red labels at the top).
    /// </summary>
    internal sealed class RestyleDrawingObjectsExample : Example
    {
        public override string Title => "Restyle the parts, bolts and grid of a view with .dprt / .sc / .dgr files";

        public override string Description => "drawingObject.Attributes = new Part.PartAttributes(name) (Bolt, Grid ...) + Modify for every object of a view.";

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
            TSD.View view = null;
            if (drawing != null)
            {
                DrawingObjectEnumerator selected = handler.GetDrawingObjectSelector().GetSelected();
                while (view == null && selected.MoveNext())
                {
                    view = selected.Current as TSD.View;
                }

                DrawingObjectEnumerator views = drawing.GetSheet().GetViews();
                while (view == null && views.MoveNext())
                {
                    view = views.Current as TSD.View;
                }
            }

            if (view == null)
            {
                Console.WriteLine("    Open a drawing with a view first (and select a view to use that one).");
                return;
            }

            Console.WriteLine($"    View '{view.Name}': {TeklaSession.Count(view, typeof(TSD.Part))} parts, {TeklaSession.Count(view, typeof(TSD.Bolt))} bolts, " +
                              $"{TeklaSession.Count(view, typeof(TSD.Grid))} grids");

            context.Step("Write the property files");
            string attributes = TeklaSession.AttributesDirectory(model);

            PartAttributeFile part = PartAttributeFile.CreateDefault();
            part.Properties.Content.Representation.Representation = Representation.Exact;
            part.Properties.Content.Lines.HiddenLines = true;
            part.Properties.Appearance.VisibleLines.Color = DrawingColor.Gray50;
            part.Properties.Appearance.HiddenLines.Type = DrawingLineType.Dashed;
            string partName = TeklaSession.UniqueName("tda_part");
            part.SaveTo(attributes, partName);

            BoltAttributeFile bolt = BoltAttributeFile.CreateDefault();
            bolt.Properties.Content.BoltRepresentation.SolidSymbol = BoltDrawAs.Symbol;
            bolt.Properties.Appearance.Color = DrawingColor.Blue;
            string boltName = TeklaSession.UniqueName("tda_bolt");
            bolt.SaveTo(attributes, boltName);

            GridAttributeFile grid = GridAttributeFile.CreateDefault();
            grid.Properties.Grid = TeklaHelper.DrawingAttribute.Pages.Grid.Visible;
            grid.Properties.TextPlacing.TextPlacingTop = true;
            grid.Properties.Text.Color = DrawingColor.Red;
            grid.Properties.GridLine.Color = DrawingColor.Red;
            string gridName = TeklaSession.UniqueName("tda_grid");
            grid.SaveTo(attributes, gridName);

            if (!context.Confirm($"Restyle the objects of view '{view.Name}'?"))
            {
                Console.WriteLine("    Cancelled.");
                return;
            }

            int parts = 0, bolts = 0, grids = 0;
            DrawingObjectEnumerator objects = view.GetAllObjects();
            while (objects.MoveNext())
            {
                switch (objects.Current)
                {
                    case TSD.Part drawingPart:
                        drawingPart.Attributes = new TSD.Part.PartAttributes(partName);
                        parts += drawingPart.Modify() ? 1 : 0;
                        break;
                    case TSD.Bolt drawingBolt:
                        drawingBolt.Attributes = new TSD.Bolt.BoltAttributes(boltName);
                        bolts += drawingBolt.Modify() ? 1 : 0;
                        break;
                    case TSD.Grid drawingGrid:
                        drawingGrid.Attributes = new TSD.Grid.GridAttributes(gridName);
                        grids += drawingGrid.Modify() ? 1 : 0;
                        break;
                }
            }

            drawing.CommitChanges();
            Console.WriteLine($"    Modified {parts} parts, {bolts} bolts, {grids} grids");

            if (context.Confirm("Undo (close the drawing without saving)?"))
            {
                handler.CloseActiveDrawing(false);
                Console.WriteLine("    Closed without saving.");
            }
        }
    }
}
