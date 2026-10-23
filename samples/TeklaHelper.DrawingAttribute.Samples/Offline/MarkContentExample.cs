using TeklaHelper.DrawingAttribute.DrawingObjects;
using TeklaHelper.DrawingAttribute.Marks;
using TeklaHelper.DrawingAttribute.Pages;
using TeklaHelper.DrawingAttribute.Views;

namespace TeklaHelper.DrawingAttribute.Samples.Offline
{
    /// <summary>
    /// Builds the "Elements in mark" of a view label and of a part mark from typed elements instead of writing the XML of
    /// the <c>ContentString</c> keys by hand.
    /// </summary>
    internal sealed class MarkContentExample : Example
    {
        public override string Title => "Mark and label content (ContentString) from typed elements";

        public override string Description => "View name, scale and a user attribute in a view label; position and a framed profile in a part mark.";

        public override void Run(ExampleContext context)
        {
            context.Step("View label: name in green Tahoma 5, the scale, and USER_FIELD_1 on the next line");
            ViewProperties view = ViewProperties.CreateDefault();
            view.Attributes.LabelMarkA1.Elements = new MarkContent()
                .Add(MarkElementTypes.ViewLabel.ViewName, new MarkFormat { Font = "Tahoma", Color = DrawingColor.Green, Height = 5 })
                .AddText(" ")
                .Add(MarkElementTypes.ViewLabel.Scale)
                .AddLineFeed()
                .AddUserAttribute("USER_FIELD_1");
            string path = view.SaveTo(context.OutputDirectory, "sample_label");
            System.Console.WriteLine("    ViewLabelMarkA1.ContentString holds " + view.GetString("ViewLabelMarkA1.ContentString").Length + " characters of XML");

            context.Step("Part mark: the position, then the profile in a red rectangle");
            PartMarkAttributeFile mark = PartMarkAttributeFile.CreateDefault();
            mark.Properties.SetMarkContent("ContentString1", new MarkContent()
                .Add(MarkElementTypes.PartMark.PartPosition)
                .AddFrame(FrameAroundElementsType.Rectangular, DrawingColor.Red, new MarkContent().Add(MarkElementTypes.PartMark.Profile)));
            mark.SaveTo(context.OutputDirectory, "sample_framed_mark");

            context.Step("Read it back: the elements of the label row");
            foreach (MarkElement element in AttributeFile.FromFile<ViewProperties>(path).Attributes.LabelMarkA1.Elements)
            {
                System.Console.WriteLine("    " + element);
            }
        }
    }
}
