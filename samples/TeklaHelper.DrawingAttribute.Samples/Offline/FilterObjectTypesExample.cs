using System;
using TeklaHelper.DrawingAttribute.Drawings;
using TeklaHelper.DrawingAttribute.Filters;
using TeklaHelper.DrawingAttribute.Views;

namespace TeklaHelper.DrawingAttribute.Samples.Offline
{
    /// <summary>
    /// Filters on the object type (the "Object / Object type" row of the filter dialog), and filters for model selection,
    /// view filters of the model, and every drawing type.
    /// </summary>
    internal sealed class FilterObjectTypesExample : Example
    {
        public override string Title => "Filters on the object type; selection filters; filters of every drawing type";

        public override string Description => "FilterObjectType (parts, bolts, rebars, ...), .SObjGrp / .VObjGrp, and the filters of .gd / .ad / .wd / .cud.";

        public override void Run(ExampleContext context)
        {
            context.Step("Object type rows: the dialog writes label keys (albl_Part ...), several separated by spaces");
            FilterCondition noRebar = FilterProperties.Object.Type.IsNotEqual(FilterObjectType.ReinforcingBar);
            FilterCondition partsOrBolts = FilterProperties.Object.Type.IsEqual(FilterObjectType.Part, FilterObjectType.BoltGroup);
            Console.WriteLine("    " + new FilterRow(noRebar));
            Console.WriteLine("    " + new FilterRow(partsOrBolts));
            Console.WriteLine("    Every object type: " + string.Join(", ", FilterObjectTypes.All));

            context.Step("A model selection filter: parts (not bolts, welds or rebars) of a concrete material in phase 2");
            FilterExpression concrete = new FilterGroup()
                .Add(FilterProperties.Object.Type.IsEqual(FilterObjectType.Part), FilterLogic.And)
                .Add(FilterProperties.Part.Material.BeginsWith("C"), FilterLogic.And)
                .Add(FilterProperties.Part.Phase.IsEqual(2));
            string selection = FilterFile.FromExpression(FilterFileExtensions.ModelSelectionFilter, concrete).SaveTo(context.OutputDirectory, "sample_concrete_phase2");
            context.ShowFile(selection, 40);

            context.Step("The same expression as a view filter of the model (.VObjGrp)");
            FilterFile.FromExpression(FilterFileExtensions.ModelViewFilter, concrete).SaveTo(context.OutputDirectory, "sample_concrete_phase2");

            context.Step("A drawing view without reinforcement, and drawings that show parts and bolts only");
            var view = ViewProperties.CreateDefault();
            view.Filter.SetExpression(noRebar);
            view.SaveTo(context.OutputDirectory, "sample_no_rebar");

            GADrawingAttributeFile ga = GADrawingAttributeFile.CreateDefault();
            ga.Filter.SetExpression(partsOrBolts);
            ga.SaveTo(context.OutputDirectory, "sample_parts_bolts");

            AssemblyDrawingAttributeFile assembly = AssemblyDrawingAttributeFile.CreateDefault();
            assembly.Filter.SetExpression(partsOrBolts);
            assembly.SaveTo(context.OutputDirectory, "sample_parts_bolts");

            context.Step("Stand-alone filter files of the Filter page of each drawing dialog");
            foreach (string extension in new[]
            {
                FilterFileExtensions.GADrawingFilter, FilterFileExtensions.AssemblyDrawingFilter,
                FilterFileExtensions.SinglePartDrawingFilter, FilterFileExtensions.CastUnitDrawingFilter,
            })
            {
                Console.WriteLine("    " + FilterFile.FromExpression(extension, noRebar).SaveTo(context.OutputDirectory, "sample_no_rebar"));
            }

            Console.WriteLine();
            Console.WriteLine("In Tekla: new ModelObjectSelector().GetObjectsByFilterName(\"sample_concrete_phase2\") (reads .SObjGrp only),");
            Console.WriteLine("          or pick the filter in the selection filter list of the model.");
        }
    }
}
