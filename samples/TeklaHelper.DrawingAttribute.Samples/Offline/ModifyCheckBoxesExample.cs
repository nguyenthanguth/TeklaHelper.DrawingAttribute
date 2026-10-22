using System;
using System.Linq;
using TeklaHelper.DrawingAttribute.Views;

namespace TeklaHelper.DrawingAttribute.Samples.Offline
{
    /// <summary>
    /// The check boxes in front of the controls of the dialogs (keys <c>*_en</c>). They decide what the dialog's Modify
    /// button applies to selected objects. <c>view.Attributes.LoadAttributes(name)</c> in the Open API ignores them and
    /// applies every value (checked in Tekla Structures 2025), so they only matter for files people load in the dialog.
    /// </summary>
    internal sealed class ModifyCheckBoxesExample : Example
    {
        public override string Title => "Check boxes of the dialog (*_en) for Load + Modify by hand";

        public override string Description => "SetAllEnabled per page, SetEnabled per key; the Open API ignores them.";

        public override void Run(ExampleContext context)
        {
            var view = new ViewProperties();
            view.Objects.Part.Appearance.VisibleLines.Color = DrawingColor.Red;

            context.Step("Tick every check box of the Part page, then untick the visible line colour");
            view.Objects.Part.SetAllEnabled(true);
            view.Objects.Part.SetEnabled("visible_colour", false);
            Console.WriteLine("    " + string.Join(", ", view.Values.Where(p => p.Key.EndsWith("_en", StringComparison.Ordinal)).Take(8).Select(p => p.Key + "=" + p.Value.ToFileText())) + ", ...");
            Console.WriteLine("    vp.visible_colour enabled: " + view.Objects.Part.IsEnabled("visible_colour"));

            context.Step("Untick every check box of every object page: Modify in the dialog then changes nothing there");
            view.Objects.Part.SetAllEnabled(false);
            view.Objects.Bolt.SetAllEnabled(false);
            view.Objects.NeighborPart.SetAllEnabled(false);
            view.Objects.Grid.SetAllEnabled(false);
            view.Objects.Weld.SetAllEnabled(false);

            context.ShowFile(view.SaveTo(context.OutputDirectory, "sample_check_boxes"), 10);
        }
    }
}
