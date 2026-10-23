using System;
using TeklaHelper.DrawingAttribute.Drawings;
using TeklaHelper.DrawingAttribute.Pages;

namespace TeklaHelper.DrawingAttribute.Samples.Offline
{
    /// <summary>
    /// The pages of the GA drawing properties besides the object and mark pages: layout, view, section view, detail view,
    /// dimension, automatic dimensioning and protection.
    /// </summary>
    internal sealed class GADrawingPagesExample : Example
    {
        public override string Title => "GA drawing pages: layout, section and detail views, dimensions, protection";

        public override string Description => "Layout (dl.), Views (dview.), SectionView (dc.), DetailView, Dimension (dd.), Dimensioning, Protection of a .gd.";

        public override void Run(ExampleContext context)
        {
            GADrawingAttributeFile drawing = GADrawingAttributeFile.CreateDefault();
            drawing.Title1 = "PLAN LEVEL +3.600";

            context.Step("Layout: the layout of the environment, automatic scale from a list of scales");
            drawing.Layout.LayoutName = "standard";
            drawing.Layout.AutoScale = true;
            drawing.Layout.ViewScales = "50 100 200";
            drawing.Layout.AlignSectionViewsWithMainView = true;

            context.Step("View page: scale and shortening of the views the drawing creates");
            drawing.Views.Scale = 100;
            drawing.Views.CreateSectionViews = ViewCreation.Off;
            drawing.Views.ShorteningOn = true;
            drawing.Views.ShorteningCutLength = 2000;

            context.Step("Section view page: size, depth, direction, the cutting line, the mark and the label");
            SectionViewPage section = drawing.SectionView;
            section.Attributes.Scale.Size = SectionViewSize.SectionDepth;
            section.Attributes.Scale.SectionDepth = 500;
            section.Attributes.Scale.CutScale = 20;
            section.Attributes.Direction.MiddleSection = SectionDirection.Left;
            section.CuttingLine.Properties.Color = DrawingColor.Blue;
            section.CuttingLine.Line.LineArrowLength = 10;
            section.SectionMark.Symbol.Color = DrawingColor.Blue;
            section.SectionMark.StartNumberOrLetterOfSectionViewAndSymbolLabel = "A";
            section.ViewLabel.ViewLabel.LabelPosition.Vertical = LabelPositionVertical.Below;
            section.ViewLabel.Label.ViewDirectionMarks.SectionViews = ShowMarks.SymbolAndLabel;

            context.Step("Detail view page: the boundary and the mark of details");
            DetailViewPage detail = drawing.DetailView;
            detail.DetailBoundary.Shape.ShapeOfTheBoundary = DetailBoundaryShape.Rectangle;
            detail.DetailBoundary.BoundingLine.Color = DrawingColor.Red;
            detail.DetailBoundary.BoundingLine.LineType = DrawingLineType.DashDot;
            detail.DetailMark.Symbol.Color = DrawingColor.Red;
            detail.ViewLabel.LabelPosition.Vertical = LabelPositionVertical.Below;

            context.Step("Dimension page: the dimensions Tekla creates in the drawing");
            DimensionPage dimension = drawing.Dimension;
            dimension.General.DimensionTypes.Straight = DimensionType.RelativeAndAbsolute;
            dimension.General.DimensionTypes.Precision.Straight = DimensionPrecision.OnePerEight;
            dimension.General.Placing.Placing.DimensionLinesSpacing = 10;
            dimension.Appearance.Text.Height = 3;
            dimension.Appearance.Text.Color = DrawingColor.Black;
            dimension.Appearance.LineArrow.Color = DrawingColor.Gray50;

            context.Step("Automatic dimensioning of the GA drawing: grid line dimensions and part positions");
            drawing.Dimensioning.Grid.DimensionCreation.GridLineDimensions = true;
            drawing.Dimensioning.Grid.DimensionCreation.OverallDimension = true;
            drawing.Dimensioning.Grid.DimensionPositioning.Horizontal = GridDimensionHorizontalPosition.Both;
            drawing.Dimensioning.Grid.DimensionPositioning.Vertical = GridDimensionVerticalPosition.Below;
            drawing.Dimensioning.Parts.MaximumNumberOfOutsideDimensions = 3;

            context.Step("Protection: texts must not overlap parts, dimension lines or other texts");
            drawing.Protection.TextPart = true;
            drawing.Protection.TextDiml = true;
            drawing.Protection.TextText = true;

            string path = drawing.SaveTo(context.OutputDirectory, "sample_plan");
            context.ShowFile(path, 12);
            Console.WriteLine();
            Console.WriteLine("In Tekla: new GADrawing(\"PLAN\", \"sample_plan\").Insert(), or Load in the drawing properties dialog.");
        }
    }
}
