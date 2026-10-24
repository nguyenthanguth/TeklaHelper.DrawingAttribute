using System;
using System.Collections.Generic;
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
    /// The previous properties of each object are kept, so that undo gives each its own back.
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
            TSD.View view = drawing == null ? null : TeklaSession.SelectedOrFirstView(handler, drawing);
            if (view == null)
            {
                Console.WriteLine("    Open a drawing with a view first (and select a view to use that one).");
                return;
            }

            Console.WriteLine($"    View '{view.Name}': {TeklaSession.Count(view, typeof(TSD.Part))} parts, {TeklaSession.Count(view, typeof(TSD.Bolt))} bolts, " +
                              $"{TeklaSession.Count(view, typeof(TSD.Grid))} grids");

            context.Step("The property files (written after you confirm)");
            PartAttributeFile part = PartAttributeFile.CreateDefault();
            part.Properties.Content.Representation.Representation = Representation.Exact;
            part.Properties.Content.Lines.HiddenLines = true;
            part.Properties.Appearance.VisibleLines.Color = DrawingColor.Gray50;
            part.Properties.Appearance.HiddenLines.Type = DrawingLineType.Dashed;

            BoltAttributeFile bolt = BoltAttributeFile.CreateDefault();
            bolt.Properties.Content.BoltRepresentation.SolidSymbol = BoltDrawAs.Symbol;
            bolt.Properties.Appearance.Color = DrawingColor.Blue;

            GridAttributeFile grid = GridAttributeFile.CreateDefault();
            grid.Properties.Grid = TeklaHelper.DrawingAttribute.Pages.Grid.Visible;
            grid.Properties.TextPlacing.TextPlacingTop = true;
            grid.Properties.Text.Color = DrawingColor.Red;
            grid.Properties.GridLine.Color = DrawingColor.Red;

            if (!context.Confirm($"Write three property files and restyle the objects of view '{view.Name}'?"))
            {
                Console.WriteLine("    Cancelled: nothing was written.");
                return;
            }

            string attributes = TeklaSession.AttributesDirectory(model);
            string name = TeklaSession.UniqueName("tda_restyle");
            part.SaveTo(attributes, name);
            bolt.SaveTo(attributes, name);
            grid.SaveTo(attributes, name);

            // Each object with the properties it had, for the undo
            var undo = new List<Action>();
            int parts = 0, bolts = 0, grids = 0;
            DrawingObjectEnumerator objects = view.GetAllObjects();
            while (objects.MoveNext())
            {
                switch (objects.Current)
                {
                    case TSD.Part drawingPart:
                        TSD.Part.PartAttributes partBefore = drawingPart.Attributes;
                        drawingPart.Attributes = new TSD.Part.PartAttributes(name);
                        if (drawingPart.Modify())
                        {
                            parts++;
                            undo.Add(() => { drawingPart.Attributes = partBefore; drawingPart.Modify(); });
                        }

                        break;
                    case TSD.Bolt drawingBolt:
                        TSD.Bolt.BoltAttributes boltBefore = drawingBolt.Attributes;
                        drawingBolt.Attributes = new TSD.Bolt.BoltAttributes(name);
                        if (drawingBolt.Modify())
                        {
                            bolts++;
                            undo.Add(() => { drawingBolt.Attributes = boltBefore; drawingBolt.Modify(); });
                        }

                        break;
                    case TSD.Grid drawingGrid:
                        TSD.Grid.GridAttributes gridBefore = drawingGrid.Attributes;
                        drawingGrid.Attributes = new TSD.Grid.GridAttributes(name);
                        if (drawingGrid.Modify())
                        {
                            grids++;
                            undo.Add(() => { drawingGrid.Attributes = gridBefore; drawingGrid.Modify(); });
                        }

                        break;
                }
            }

            drawing.CommitChanges();
            Console.WriteLine($"    Modified {parts} parts, {bolts} bolts, {grids} grids");

            // Only what the example changed goes back: the rest of the drawing, saved or not, stays as it was
            if (context.Confirm("Undo (give every object its previous properties, delete the property files)?"))
            {
                undo.ForEach(action => action());
                drawing.CommitChanges();
                TeklaSession.DeleteFiles(attributes, name);
                Console.WriteLine($"    Restored {undo.Count} objects.");
            }
        }
    }
}
