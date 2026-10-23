using TeklaHelper.DrawingAttribute.Marks;

namespace TeklaHelper.DrawingAttribute.Pages
{
    // The content tables of the bolt mark ("Mark content of" Site / Shop). Checked in Tekla 2025: a view loaded with a
    // different text in each table, its marks read through the Open API. ContentString3 and ContentString4 are in the files
    // too; no bolt mark showed them. Each get returns a copy, to be assigned back after a change; null empties the table.
    public sealed partial class BoltMark
    {
        /// <summary>
        /// What the marks of site bolts show (<c>ContentString1</c>, "Site"; <see cref="MarkElementTypes.BoltMark"/>).
        /// </summary>
        public MarkContent SiteBoltElements
        {
            get => GetMarkContentOrNull("ContentString1");
            set => SetMarkContent("ContentString1", value);
        }

        /// <summary>What the marks of workshop bolts show (<c>ContentString2</c>, "Shop").</summary>
        public MarkContent WorkshopBoltElements
        {
            get => GetMarkContentOrNull("ContentString2");
            set => SetMarkContent("ContentString2", value);
        }
    }
}
