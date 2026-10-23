using TeklaHelper.DrawingAttribute.Marks;

namespace TeklaHelper.DrawingAttribute.Pages
{
    // The content tables of the reinforcement mark ("Mark content of" Single / Group / Mesh; the mesh table is the fourth,
    // the third holds the merged mark). Checked in Tekla 2025: a view loaded with a different text in each table, its marks
    // of a single bar, of the reinforcement groups and of a mesh read through the Open API. Each get returns a copy, to be
    // assigned back after a change; null empties the table.
    public sealed partial class ReinforcementMark
    {
        /// <summary>
        /// What the marks of single bars show (<c>ContentString1</c>, "Single"; <see cref="MarkElementTypes.ReinforcementMark"/>).
        /// </summary>
        public MarkContent SingleElements
        {
            get => GetMarkContentOrNull("ContentString1");
            set => SetMarkContent("ContentString1", value);
        }

        /// <summary>
        /// What the marks of reinforcement groups show (<c>ContentString2</c>, "Group"); marks merged by
        /// <c>MergeMarks</c> and the leader line options show it too.
        /// </summary>
        public MarkContent GroupElements
        {
            get => GetMarkContentOrNull("ContentString2");
            set => SetMarkContent("ContentString2", value);
        }

        /// <summary>
        /// The merged mark (<c>ContentString3</c>): its block prefix, single mark content, distances between groups and
        /// separator, which the files store as <c>&lt;MergeElement subType="53"&gt;</c> ... <c>"56"</c> (the Open API's
        /// <c>MergedMarkPropertyElementTypes</c>) and the library keeps as <see cref="MarkXmlElement"/>. The files hold
        /// nothing else in this table; Tekla 2025 was not seen to read it (the Open API cannot read the content of a merged
        /// mark set).
        /// </summary>
        public MarkContent MergedMarkElements
        {
            get => GetMarkContentOrNull("ContentString3");
            set => SetMarkContent("ContentString3", value);
        }

        /// <summary>
        /// What the marks of reinforcement meshes show (<c>ContentString4</c>, "Mesh";
        /// <see cref="MarkElementTypes.ReinforcementMeshMark"/>).
        /// </summary>
        public MarkContent MeshElements
        {
            get => GetMarkContentOrNull("ContentString4");
            set => SetMarkContent("ContentString4", value);
        }
    }
}
