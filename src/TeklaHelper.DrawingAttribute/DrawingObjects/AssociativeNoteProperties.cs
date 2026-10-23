using TeklaHelper.DrawingAttribute.Marks;

namespace TeklaHelper.DrawingAttribute.DrawingObjects
{
    // The content tables of the associative note, one per kind of object ("Mark content of" Part / Bolt / Rebar / Surface
    // treatment / Edge chamfer / ... / Reference object). Checked in Tekla 2025 with Mark.MarkAttributes(object, name) on a
    // file with a different text in each table: main and secondary parts, also of sub-assemblies, read ContentString1; site
    // and workshop bolts ContentString2; single bars, reinforcement groups and meshes ContentString4; a reference model
    // ReferenceObjectContentString. Each get returns a copy, to be assigned back after a change; null empties the table.
    public sealed partial class AssociativeNoteProperties
    {
        /// <summary>What a note on a part shows (<c>ContentString1</c>, "Part").</summary>
        public MarkContent PartElements
        {
            get => GetMarkContentOrNull("ContentString1");
            set => SetMarkContent("ContentString1", value);
        }

        /// <summary>What a note on a bolt shows (<c>ContentString2</c>, "Bolt").</summary>
        public MarkContent BoltElements
        {
            get => GetMarkContentOrNull("ContentString2");
            set => SetMarkContent("ContentString2", value);
        }

        /// <summary>
        /// What a note on a surface treatment shows (<c>ContentString3</c>, "Surface treatment"). Not checked in Tekla (the
        /// model had no surface treatment): the shipped files hold the name and surfacing name elements here.
        /// </summary>
        public MarkContent SurfaceTreatmentElements
        {
            get => GetMarkContentOrNull("ContentString3");
            set => SetMarkContent("ContentString3", value);
        }

        /// <summary>What a note on reinforcement (single bar, group or mesh) shows (<c>ContentString4</c>, "Rebar").</summary>
        public MarkContent ReinforcementElements
        {
            get => GetMarkContentOrNull("ContentString4");
            set => SetMarkContent("ContentString4", value);
        }

        /// <summary>What a note on a reference model shows (<c>ReferenceObjectContentString</c>, "Reference object").</summary>
        public MarkContent ReferenceObjectElements
        {
            get => GetMarkContentOrNull("ReferenceObjectContentString");
            set => SetMarkContent("ReferenceObjectContentString", value);
        }
    }
}
