using System.IO;
using TeklaHelper.DrawingAttribute.Filters;
using TeklaHelper.DrawingAttribute.Pages;
using TeklaHelper.DrawingAttribute.Views;

namespace TeklaHelper.DrawingAttribute.Samples.Offline
{
    /// <summary>
    /// Builds view properties from scratch, page by page like the Tekla View properties dialog, and saves the files the
    /// Open API reads: name.vi and name.vi.filter.
    /// </summary>
    internal sealed class ViewPropertiesExample : Example
    {
        public override string Title => "Create view properties (.vi + .vi.filter) page by page";

        public override string Description => "Attributes, Objects (part, bolt, grid, ...), Marks and Filter, as in the View properties dialog.";

        public override void Run(ExampleContext context)
        {
            var view = new ViewProperties();

            context.Step("Attributes page: Attributes 1, Attributes 2 and Label tabs");

            // The tab "Attributes 1", group box "View"
            ViewAttributes.Attributes1Tab.ViewGroup general = view.Attributes.Attributes1.View;
            general.Scale = 20;
            general.Size = ViewSize.FitByParts;
            general.DepthUp = 300;
            general.DepthDown = 300;

            // The tab "Attributes 2": shortening of long parts
            ViewAttributes.Attributes2Tab.CutPartsGroup.ShorteningGroup shortening = view.Attributes.Attributes2.CutParts.Shortening;
            shortening.CutParts = CutParts.X_Direction;
            shortening.MinimumCutPartLength = 1500;
            shortening.SpaceBetweenCutParts = 40;

            // The tab "Label": what the view label shows and where
            view.Attributes.Label.ViewLabel.LabelPosition.Vertical = LabelPositionVertical.Below;
            view.Attributes.Label.ViewLabel.ViewDirectionMarks.ShowMarks = ShowMarks.SymbolAndLabel;
            view.Attributes.LabelMarkA1.Content.Font.Height = 5;

            context.Step("Objects pages: what the view shows, and how");

            // Objects > Part: tabs Content, Appearance and Fill, with their group boxes
            PartObject part = view.Objects.Part;
            part.Content.Representation.Representation = Representation.Exact;
            part.Content.HiddenLines.HiddenLines = true;
            part.Content.HiddenLines.OwnHiddenLines = false;
            part.Content.CenterLine.MainPartBeam = true;
            part.Content.AdditionalMarks.OrientationMarks = true;
            part.Appearance.VisibleLines.Color = DrawingColor.Black;
            part.Appearance.HiddenLines.Color = DrawingColor.Gray50;
            part.Appearance.HiddenLines.Type = DrawingLineType.Dashed;
            part.Fill.Sections.Type = "ANSI31";
            part.Fill.Sections.Scale = HatchScale.Automatic;

            // Objects > Bolt
            view.Objects.Bolt.Content.BoltRepresentation.SolidSymbol = BoltDrawAs.Symbol;
            view.Objects.Bolt.Content.BoltRepresentation.Axis = true;
            view.Objects.Bolt.Appearance.Color = DrawingColor.Blue;

            // Objects > Neighbor part: drawn as outline, connected parts only
            view.Objects.NeighborPart.Content.Representation.Representation = NeighborPartRepresentation.Outline;
            view.Objects.NeighborPart.Visibility.NeighborParts = NpCollectBy.ConnectedParts;

            // Objects > Grid, Weld
            view.Objects.Grid.Grid = Grid.Visible;
            view.Objects.Grid.Text.Height = 3.5;
            view.Objects.Grid.GridLine.Color = DrawingColor.Red;
            view.Objects.Grid.GridLine.Type = DrawingLineType.DashDot;
            view.Objects.Weld.Content.Visibility.Welds = WeldVisibility.NotVisible;

            context.Step("Marks pages");

            // Marks > Part mark: tab Content (font, frame of the elements) and tab General (visibility, frame, leader line)
            PartMark partMark = view.Marks.PartMark;
            partMark.Content.Font.Font = "Arial";
            partMark.Content.Font.Height = 3;
            partMark.Content.Font.Color = DrawingColor.Blue;
            partMark.General.VisibilityOfMark.VisibilityInView = VisibilityInView.Always;
            partMark.General.FrameAroundMark.Type = FrameAroundMarkType.Rectangular;
            partMark.General.LeaderLine.Arrow = LeaderLineArrow.SolidArrow;
            partMark.General.Placing.Place.Quarter.Quarter1 = true;
            partMark.General.Placing.Place.Quarter.Quarter2 = true;

            // Marks > Bolt mark: not shown in this view
            view.Marks.BoltMark.General.VisibilityOfBoltMarks.VisibilityInView = VisibilityInView.None;

            context.Step("Filter: the view shows only beams and columns of phase 1 or later");

            // The rows of the filter page, saved as name.vi.filter. Without rows no filter file is written (an empty one
            // would make Tekla show no objects at all).
            view.Filter.SetExpression(new FilterGroup()
                .Add(new FilterGroup()
                    .Add(FilterProperties.Part.Name.IsEqual("BEAM"), FilterLogic.Or)
                    .Add(FilterProperties.Part.Name.IsEqual("COLUMN")), FilterLogic.And)
                .Add(FilterProperties.Part.Phase.GreaterOrEqual(1)));

            context.Step("Save: name.vi and name.vi.filter, the files View.ViewAttributes(name) loads");
            string path = view.SaveTo(context.OutputDirectory, "sample_detail_20");
            context.ShowFile(path, 12);
            context.ShowFile(Path.ChangeExtension(path, ".vi.filter"), 40);

            System.Console.WriteLine();
            System.Console.WriteLine("In Tekla: new View(sheet, viewCoordinateSystem, displayCoordinateSystem, restrictionBox, \"sample_detail_20\")");
            System.Console.WriteLine("          or view.Attributes.LoadAttributes(\"sample_detail_20\"); view.Modify();");
        }
    }
}
