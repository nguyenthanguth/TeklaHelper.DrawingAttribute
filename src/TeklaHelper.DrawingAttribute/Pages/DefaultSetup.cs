using TeklaHelper.DrawingAttribute.Drawings;
using TeklaHelper.DrawingAttribute.Views;

namespace TeklaHelper.DrawingAttribute.Pages
{
    /// <summary>
    /// The settings <c>CreateDefault()</c> chooses on top of the page defaults: a view that shows its objects to the Open
    /// API (which finds only what a view shows) and draws them plainly. Every value here is a choice of this library, not
    /// a Tekla default, and every choice of the library is here.
    /// </summary>
    internal static class DefaultSetup
    {
        /// <summary>
        /// Content of the part mark: the assembly position, in the form the View properties dialog saves
        /// (<c>vpm.ContentString1</c>), without its line breaks and in black. Without a content a visible part mark shows
        /// nothing.
        /// </summary>
        internal const string PartMarkContent =
            "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?><!DOCTYPE Mark><Mark><MarkPart groupid=\"0\">" +
            "<Element name=\"ASSEMBLY_POS\" value=\"\" subType=\"22\"><Format font=\"Arial\" color=\"153\" height=\"2.500\" hidden=\"0\"/></Element>" +
            "</MarkPart></Mark>";

        /// <summary>View properties: 1:20, sized to the parts, not shortened, black view label; objects and marks below.</summary>
        public static void Apply(ViewProperties view)
        {
            view.Attributes.Attributes1.View.Scale = 20;
            view.Attributes.Attributes1.View.Size = ViewSize.FitByParts;
            view.Attributes.Attributes2.CutParts.Shortening.CutParts = CutParts.No;
            view.Attributes.Label.ViewLabel.Symbol.Color = DrawingColor.Black;
            Apply(view.Objects);
            Apply(view.Marks);
        }

        /// <summary>
        /// Drawing properties: black view labels, and no view properties file in the View creation table (the shipped tables
        /// name files that only some environments have), so that the views use the settings below (a 3D view then uses the
        /// environment's standard view properties); objects and marks below.
        /// </summary>
        public static void Apply(DrawingAttributeFile drawing)
        {
            drawing.Views.LabelColor = DrawingColor.Black;
            if (drawing.Views.GetViewTable().Count > 0)
            {
                drawing.Views.SetViewAttributeFile(string.Empty);
            }

            Apply(drawing.Objects);
            Apply(drawing.Marks);
        }

        /// <summary>
        /// Parts exact with hidden lines and dash-dot center lines of main beams, part, bolt and grid lines black; bolts, welds,
        /// surface treatments, the grid and connected neighbor parts (outline) shown.
        /// </summary>
        private static void Apply(ObjectPages objects)
        {
            PartObject part = objects.Part;
            part.Content.Representation.Representation = Representation.Exact;
            part.Content.HiddenLines.HiddenLines = true;
            part.Content.HiddenLines.OwnHiddenLines = false;
            part.Content.CenterLine.MainPartBeam = true;
            part.Appearance.VisibleLines.Color = DrawingColor.Black;
            part.Appearance.VisibleLines.Type = DrawingLineType.Solid;
            part.Appearance.HiddenLines.Color = DrawingColor.Black;
            part.Appearance.HiddenLines.Type = DrawingLineType.Dashed;
            part.Appearance.CenterLine.Type = DrawingLineType.DashDot;

            BoltObject bolt = objects.Bolt;
            bolt.Content.BoltRepresentation.SolidSymbol = BoltDrawAs.Symbol;
            bolt.Content.BoltRepresentation.VisibilityOfBolts.InMainParts = true;
            bolt.Content.BoltRepresentation.VisibilityOfBolts.InSecondaryParts = true;
            bolt.Content.BoltRepresentation.VisibilityOfBolts.InSubAssemblies = true;
            bolt.Appearance.Color = DrawingColor.Black;

            objects.Weld.Content.Visibility.Welds = WeldVisibility.BothVisible;
            objects.SurfaceTreatment.Content.Visibility = true;

            objects.Grid.Grid = Grid.Visible;
            objects.Grid.GridLine.Color = DrawingColor.Black;

            objects.NeighborPart.Content.Representation.Representation = NeighborPartRepresentation.Outline;
            objects.NeighborPart.Visibility.NeighborParts = NpCollectBy.ConnectedParts;
        }

        /// <summary>Part marks shown with the assembly position; bolt marks hidden.</summary>
        private static void Apply(MarkPages marks)
        {
            PartMark partMark = marks.PartMark;
            partMark.General.VisibilityOfMark.VisibilityInView = VisibilityInView.Always;
            partMark.Advanced.Create = 1;
            partMark.Advanced.ContentString1 = PartMarkContent;

            marks.BoltMark.General.VisibilityOfBoltMarks.VisibilityInView = VisibilityInView.None;
        }
    }
}
