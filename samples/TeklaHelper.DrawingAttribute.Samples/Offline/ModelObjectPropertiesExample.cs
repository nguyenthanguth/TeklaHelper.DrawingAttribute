using System;
using TeklaHelper.DrawingAttribute.DrawingObjects;
using TeklaHelper.DrawingAttribute.Pages;

namespace TeklaHelper.DrawingAttribute.Samples.Offline
{
    /// <summary>
    /// The property files of the model objects in a drawing (double-click a part, a bolt, a weld ... in the drawing
    /// editor): how one object is drawn, read by the Open API with <c>new Part.PartAttributes(name)</c> and the like.
    /// </summary>
    internal sealed class ModelObjectPropertiesExample : Example
    {
        public override string Title => "Model objects in a drawing: part, bolt, weld, rebar, mesh, surface, pour, grid";

        public override string Description => ".dprt .sc .welo .drbr .dsrf .po .pbr .dgr .grl .dcha .drmp — the files the Drawing API attribute classes read.";

        public override void Run(ExampleContext context)
        {
            string output = context.OutputDirectory;

            context.Step("Part (.dprt): exact representation, hidden lines, center line, ANSI31 section hatch");
            PartAttributeFile part = PartAttributeFile.CreateDefault();
            part.Properties.Content.Representation.Representation = Representation.Exact;
            part.Properties.Content.Lines.HiddenLines = true;
            part.Properties.Content.Lines.CenterLine = true;
            part.Properties.Content.AdditionalMarks.OrientationMark = true;
            part.Properties.Appearance.HiddenLines.Type = DrawingLineType.Dashed;
            part.Properties.Appearance.HiddenLines.Color = DrawingColor.Gray50;
            part.Properties.Fill.Sections.Type = "ANSI31";
            part.Properties.Fill.Sections.Scale = HatchScale.Automatic;
            context.ShowFile(part.SaveTo(output, "sample_exact_part"), 8);

            context.Step("Bolt (.sc): symbol with axis and hole, blue");
            BoltAttributeFile bolt = BoltAttributeFile.CreateDefault();
            bolt.Properties.Content.BoltRepresentation.SolidSymbol = BoltDrawAs.Symbol;
            bolt.Properties.Content.BoltRepresentation.Axis = true;
            bolt.Properties.Content.BoltRepresentation.Hole = true;
            bolt.Properties.Appearance.Color = DrawingColor.Blue;
            bolt.SaveTo(output, "sample_bolt_symbol");

            context.Step("Weld (.welo): site and workshop welds as outlines");
            WeldAttributeFile weld = WeldAttributeFile.CreateDefault();
            weld.Properties.Content.Visibility.Welds = WeldVisibility.BothVisible;
            weld.Properties.Content.Representation.Representation = WeldRepresentation.Outline;
            weld.Properties.Appearance.VisibleLines.Color = DrawingColor.Red;
            weld.SaveTo(output, "sample_weld_outline");

            context.Step("Reinforcement (.drbr, also meshes): double line, first and last bar of a group, hooks");
            ReinforcementAttributeFile rebar = ReinforcementAttributeFile.CreateDefault();
            rebar.Properties.Content.ReinforcingBarRepresentation.ReinforcingBarRepresentation = ReinforcingBarRepresentation.DoubleLine;
            rebar.Properties.Content.ReinforcingBarRepresentation.VisibilityOfReinforcingBarsInGroup = VisibilityOfReinforcingBarsInGroup.FirstAndLast;
            rebar.Properties.Content.Symbols.SymbolAtHookedEnd = SymbolAtHookedEnd.FullHook;
            rebar.Properties.Content.Lines.HideLinesBehindParts = true;
            rebar.SaveTo(output, "sample_rebar_double_line");

            context.Step("Reinforcement mesh (.drmsh, the dialog's file): outline");
            ReinforcementMeshAttributeFile mesh = ReinforcementMeshAttributeFile.CreateDefault();
            mesh.Properties.Content.ReinforcingBarRepresentation.ReinforcingBarRepresentation = MeshRepresentation.OutLine;
            mesh.SaveTo(output, "sample_mesh_outline");

            context.Step("Surface treatment (.dsrf), pour (.po), pour break (.pbr)");
            SurfaceTreatmentAttributeFile surface = SurfaceTreatmentAttributeFile.CreateDefault();
            surface.Properties.Content.Representation = SurfaceTreatmentRepresentation.Exact;
            surface.Properties.Content.ShowPattern = true;
            surface.SaveTo(output, "sample_surface_exact");

            PourAttributeFile pour = PourAttributeFile.CreateDefault();
            pour.Properties.Content.Representation.Representation = Representation.Outline;
            pour.Properties.Fill.PourFaces.Type = "ANSI37";
            pour.Properties.Fill.PourFaces.Color = DrawingColor.Gray50;
            pour.SaveTo(output, "sample_pour_hatched");

            PourBreakAttributeFile pourBreak = PourBreakAttributeFile.CreateDefault();
            pourBreak.Properties.Content.Visibility = true;
            pourBreak.Properties.Appearance.VisibleLines.Type = DrawingLineType.DashDoubleDot;
            pourBreak.SaveTo(output, "sample_pour_break");

            context.Step("Grid (.dgr) and one grid line (.grl): labels at the top and left, red dash-dot lines");
            GridAttributeFile grid = GridAttributeFile.CreateDefault();
            grid.Properties.Grid = Grid.Visible;
            grid.Properties.TextPlacing.TextPlacingTop = true;
            grid.Properties.TextPlacing.TextPlacingLeft = true;
            grid.Properties.TextPlacing.TextPlacingBottom = false;
            grid.Properties.GridLine.Color = DrawingColor.Red;
            grid.Properties.GridLine.Type = DrawingLineType.DashDot;
            grid.Properties.Text.Height = 5;
            grid.Properties.Text.Frame = TextFrame.Round;
            grid.SaveTo(output, "sample_grid");

            GridLineAttributeFile gridLine = GridLineAttributeFile.CreateDefault();
            gridLine.Properties.CopyFrom(grid.Properties);                       // the same page, so the same keys
            gridLine.Properties.GridLine.Color = DrawingColor.Blue;
            gridLine.SaveTo(output, "sample_grid_line");

            context.Step("Edge chamfer (.dcha) and reference object (.drmp)");
            EdgeChamferAttributeFile chamfer = EdgeChamferAttributeFile.CreateDefault();
            chamfer.Properties.VisibleLines.Color = DrawingColor.Green;
            chamfer.SaveTo(output, "sample_chamfer");

            ReferenceObjectAttributeFile reference = ReferenceObjectAttributeFile.CreateDefault();
            reference.Properties.Appearance.Representation.Representation = ReferenceRepresentation.Outline;
            reference.Properties.Appearance.VisibleLines.Color = DrawingColor.Gray50;
            reference.SaveTo(output, "sample_reference_outline");

            Console.WriteLine();
            Console.WriteLine("In Tekla: drawingPart.Attributes = new Part.PartAttributes(\"sample_exact_part\"); drawingPart.Modify();");
            Console.WriteLine("          likewise Bolt.BoltAttributes, Weld.WeldAttributes, ReinforcementBase.ReinforcementSingleAttributes ...,");
            Console.WriteLine("          Surfacing.SurfacingAttributes, PourObject.PourAttributes, Grid.GridAttributes, GridLine.GridLineAttributes.");
        }
    }
}
