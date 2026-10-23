using TeklaHelper.DrawingAttribute.Marks;

namespace TeklaHelper.DrawingAttribute.Pages
{
    // The content tables of the part mark: the "Mark content of" menu of the Content tab picks the table the elements list
    // edits. Checked in Tekla 2025: a view loaded with a different text in each table, its marks read through the Open API.
    // Each get reads the key afresh and returns a copy, to be assigned back after a change; null empties the table.
    public sealed partial class PartMark
    {
        /// <summary>
        /// What the marks of main parts show (<c>ContentString1</c>, "Main part"; <see cref="MarkElementTypes.PartMark"/>).
        /// </summary>
        public MarkContent MainPartElements
        {
            get => GetMarkContentOrNull("ContentString1");
            set => SetMarkContent("ContentString1", value);
        }

        /// <summary>What the marks of secondary parts show (<c>ContentString2</c>, "Secondary part").</summary>
        public MarkContent SecondaryPartElements
        {
            get => GetMarkContentOrNull("ContentString2");
            set => SetMarkContent("ContentString2", value);
        }

        /// <summary>What the marks of the main parts of sub-assemblies show (<c>ContentString3</c>, "Sub-assembly main part").</summary>
        public MarkContent SubAssemblyMainPartElements
        {
            get => GetMarkContentOrNull("ContentString3");
            set => SetMarkContent("ContentString3", value);
        }

        /// <summary>
        /// What the marks of the secondary parts of sub-assemblies show (<c>ContentString4</c>, "Sub-assembly secondary
        /// part").
        /// </summary>
        public MarkContent SubAssemblySecondaryPartElements
        {
            get => GetMarkContentOrNull("ContentString4");
            set => SetMarkContent("ContentString4", value);
        }
    }
}
