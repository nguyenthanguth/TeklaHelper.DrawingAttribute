using TeklaHelper.DrawingAttribute.DrawingObjects;
using TeklaHelper.DrawingAttribute.Pages;

namespace TeklaHelper.DrawingAttribute.Samples.Offline
{
    /// <summary>
    /// Embedded and linked objects of the drawing editor: DWG/DXF, images, hyperlinks, links to other drawings and text
    /// files.
    /// </summary>
    internal sealed class EmbeddedObjectsExample : Example
    {
        public override string Title => "Embedded and linked objects: DWG, image, hyperlink, linked drawing, text file";

        public override string Description => ".fdg .idg .fhl .ldr .fas: scaling, frame, file name and link text.";

        public override void Run(ExampleContext context)
        {
            string output = context.OutputDirectory;

            context.Step("DWG / DXF (.fdg): scaled to fit, a gray frame");
            DwgAttributeFile dwg = DwgAttributeFile.CreateDefault();
            dwg.Properties.Scaling.Type = EmbeddedObjectScaling.ScaleToFit;
            dwg.Properties.Frame.Color = DrawingColor.Gray50;
            dwg.Properties.Frame.LineType = DrawingLineType.Solid;
            context.ShowFile(dwg.SaveTo(output, "sample_dwg_fit"), 10);

            context.Step("Image (.idg): scaled in X and Y");
            ImageAttributeFile image = ImageAttributeFile.CreateDefault();
            image.Properties.Scaling.Type = EmbeddedObjectScaling.ScaleXY;
            image.Properties.Scaling.ScaleInX = 0.5;
            image.Properties.Scaling.ScaleInY = 0.5;
            image.SaveTo(output, "sample_image_half");

            context.Step("Hyperlink (.fhl), link to another drawing (.ldr), text file (.fas)");
            HyperlinkAttributeFile hyperlink = HyperlinkAttributeFile.CreateDefault();
            hyperlink.Properties.Link.FileOrURL = "https://example.com/specification.pdf";
            hyperlink.Properties.Link.Text = "Specification";
            hyperlink.Properties.TextStyle.Effect = TextEffect.Underlined;
            hyperlink.Properties.TextStyle.Color = DrawingColor.Blue;
            hyperlink.SaveTo(output, "sample_specification_link");

            LinkedDrawingAttributeFile linked = LinkedDrawingAttributeFile.CreateDefault();
            linked.Properties.Link.Text = "See general arrangement";
            linked.Properties.TextStyle.Height = 3;
            linked.SaveTo(output, "sample_drawing_link");

            TextFileAttributeFile textFile = TextFileAttributeFile.CreateDefault();
            textFile.Properties.TextStyle.Font = "Courier New";
            textFile.Properties.TextStyle.Height = 2.5;
            textFile.Properties.Frame.LineType = DrawingLineType.Dashed;
            textFile.SaveTo(output, "sample_notes_file");

            System.Console.WriteLine();
            System.Console.WriteLine("In Tekla: new DwgObject(view, insertion, \"plan.dwg\", new EmbeddedObjectAttributes(\"sample_dwg_fit\")).Insert();");
            System.Console.WriteLine("          (EmbeddedObjectAttributes reads .fdg for DWG and images; the other files are for the dialogs.)");
            System.Console.WriteLine("          new Link(view, point, \"https://...\", new LinkAttributes(\"sample_specification_link\")).");
        }
    }
}
