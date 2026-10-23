using TeklaHelper.DrawingAttribute.DrawingObjects;
using TeklaHelper.DrawingAttribute.Pages;

namespace TeklaHelper.DrawingAttribute.Samples.Offline
{
    /// <summary>
    /// The graphic objects of the drawing editor: line, arc, polyline, circle, rectangle and polygon (also clouds). The
    /// Open API reads them with <c>new Line.LineAttributes(name)</c>, <c>Arc.ArcAttributes</c>, ...
    /// </summary>
    internal sealed class GraphicObjectsExample : Example
    {
        public override string Title => "Graphic objects: line, arc, polyline, circle, rectangle, polygon";

        public override string Description => ".gln .gar .gpl .gci .grt .gpg: line type, colour, arrows, fill (hatch) and its scale.";

        public override void Run(ExampleContext context)
        {
            string output = context.OutputDirectory;

            context.Step("Line (.gln): a red dashed line with a filled arrow at the end");
            LineAttributeFile line = LineAttributeFile.CreateDefault();
            line.Properties.Line.Type = DrawingLineType.Dashed;
            line.Properties.Line.Color = DrawingColor.Red;
            line.Properties.Arrow.Position = ArrowPosition.End;
            line.Properties.Arrow.Type = LineArrowType.SolidArrow;
            line.Properties.Arrow.Height = 2;
            line.Properties.Arrow.Width = 4;
            context.ShowFile(line.SaveTo(output, "sample_red_arrow"), 12);

            context.Step("Arc (.gar) with open arrows at both ends; polyline (.gpl) with a bulge");
            ArcAttributeFile arc = ArcAttributeFile.CreateDefault();
            arc.Properties.Line.Radius = 500;
            arc.Properties.Arrow.Position = ArrowPosition.Both;
            arc.Properties.Arrow.Type = LineArrowType.OpenArrow;
            arc.SaveTo(output, "sample_arc");

            PolylineAttributeFile polyline = PolylineAttributeFile.CreateDefault();
            polyline.Properties.Line.Type = DrawingLineType.DashDot;
            polyline.Properties.Line.BulgeForAllLines = 0.2;
            polyline.SaveTo(output, "sample_polyline");

            context.Step("Circle (.gci) filled with a hatch behind the model objects");
            CircleAttributeFile circle = CircleAttributeFile.CreateDefault();
            circle.Properties.BehindModelObjects = true;
            circle.Properties.Line.Color = DrawingColor.Blue;
            circle.Properties.Fill.Type = "ANSI31";
            circle.Properties.Fill.Color = DrawingColor.Blue;
            circle.Properties.ScaleAndRotationOfFill.ScalingInDirectionX = 5;
            circle.Properties.ScaleAndRotationOfFill.ScalingInDirectionY = 5;
            circle.Properties.ScaleAndRotationOfFill.Angle = 45;
            circle.SaveTo(output, "sample_hatched_circle");

            context.Step("Rectangle (.grt) and polygon / cloud (.gpg) with a solid background colour");
            RectangleAttributeFile rectangle = RectangleAttributeFile.CreateDefault();
            rectangle.Properties.Line.Type = DrawingLineType.Slashed;
            rectangle.Properties.Fill.Background = DrawingColor.Yellow;
            rectangle.SaveTo(output, "sample_rectangle");

            PolygonAttributeFile polygon = PolygonAttributeFile.CreateDefault();
            polygon.Properties.Line.Color = DrawingColor.Magenta;
            polygon.Properties.Line.BulgeForAllLines = 0.5;                      // a cloud
            polygon.Properties.Fill.Type = "ANSI37";
            polygon.Properties.ScaleAndRotationOfFill.KeepRatioOfXAndY = true;
            polygon.SaveTo(output, "sample_cloud");

            System.Console.WriteLine();
            System.Console.WriteLine("In Tekla: new Line(view, start, end, new Line.LineAttributes(\"sample_red_arrow\")).Insert();");
            System.Console.WriteLine("          new Circle(view, center, radius, new Circle.CircleAttributes(\"sample_hatched_circle\")).Insert();");
            System.Console.WriteLine("          new Cloud(view, points, new Polygon.PolygonAttributes(\"sample_cloud\")).Insert().");
        }
    }
}
