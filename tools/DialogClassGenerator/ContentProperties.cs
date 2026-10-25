using System.Collections.Generic;
using System.Linq;

namespace DialogClassGenerator
{
    /// <summary>
    /// The <c>MarkContent</c> properties of the pages: one per mark content key (<c>ContentString1</c>, <c>PourContentString</c>,
    /// ...) whose meaning is known. The tables of the part, bolt and reinforcement marks and of the associative note were
    /// read back in Tekla 2025 (see CLAUDE.md); the keys every mark page has are named after the key. The tables of the
    /// other pages are not named: they were not probed. <c>SetDefaults()</c> assigns a mark content default through these
    /// properties.
    /// </summary>
    internal static class ContentProperties
    {
        private const string Types = "global::TeklaHelper.DrawingAttribute.Marks.MarkElementTypes.";
        private const string Copy = " Each get reads the key and returns a copy, to be assigned back after a change; null empties the key.";
        private const string ViewProbe = " Checked in Tekla 2025: a view loaded with a different text in each table, its marks read through the Open API.";

        private static readonly (string Page, string Key, string Property, Location Where, string Summary)[] Specific =
        {
            ("PartMark", "ContentString1", "MainPartElements", Location.Table, $"What the marks of main parts show (<c>ContentString1</c>, \"Main part\"; <see cref=\"{Types}PartMark\"/>).{ViewProbe}"),
            ("PartMark", "ContentString2", "SecondaryPartElements", Location.Table, $"What the marks of secondary parts show (<c>ContentString2</c>, \"Secondary part\").{ViewProbe}"),
            ("PartMark", "ContentString3", "SubAssemblyMainPartElements", Location.Table, $"What the marks of the main parts of sub-assemblies show (<c>ContentString3</c>, \"Sub-assembly main part\").{ViewProbe}"),
            ("PartMark", "ContentString4", "SubAssemblySecondaryPartElements", Location.Table, $"What the marks of the secondary parts of sub-assemblies show (<c>ContentString4</c>, \"Sub-assembly secondary part\").{ViewProbe}"),
            ("BoltMark", "ContentString1", "SiteBoltElements", Location.Table, $"What the marks of site bolts show (<c>ContentString1</c>, \"Site\"; <see cref=\"{Types}BoltMark\"/>).{ViewProbe}"),
            ("BoltMark", "ContentString2", "WorkshopBoltElements", Location.Table, $"What the marks of workshop bolts show (<c>ContentString2</c>, \"Shop\").{ViewProbe}"),
            ("ReinforcementMark", "ContentString1", "SingleElements", Location.Table, $"What the marks of single bars show (<c>ContentString1</c>, \"Single\"; <see cref=\"{Types}ReinforcementMark\"/>).{ViewProbe}"),
            ("ReinforcementMark", "ContentString2", "GroupElements", Location.Table, $"What the marks of reinforcement groups show (<c>ContentString2</c>, \"Group\"); marks merged by <c>MergeMarks</c> and the leader line options show it too.{ViewProbe}"),
            ("ReinforcementMark", "ContentString3", "MergedMarkElements", Location.Merge, "The merged mark (<c>ContentString3</c>): its block prefix, single mark content, distances between groups and separator, which the files store as <c>&lt;MergeElement subType=\"53\"&gt;</c> ... <c>\"56\"</c> (the Open API's <c>MergedMarkPropertyElementTypes</c>) and the library keeps as <see cref=\"global::TeklaHelper.DrawingAttribute.Marks.MarkXmlElement\"/>. The files hold nothing else in this table; Tekla 2025 was not seen to read it (the Open API cannot read the content of a merged mark set)."),
            ("ReinforcementMark", "ContentString4", "MeshElements", Location.Table, $"What the marks of reinforcement meshes show (<c>ContentString4</c>, \"Mesh\"; <see cref=\"{Types}ReinforcementMeshMark\"/>).{ViewProbe}"),
            ("AssociativeNoteProperties", "ContentString1", "PartElements", Location.Table, "What a note on a part shows (<c>ContentString1</c>, \"Part\"; every part, also of sub-assemblies). Checked in Tekla 2025 with <c>Mark.MarkAttributes(object, name)</c>."),
            ("AssociativeNoteProperties", "ContentString2", "BoltElements", Location.Table, "What a note on a bolt shows (<c>ContentString2</c>, \"Bolt\"; site and workshop bolts). Checked in Tekla 2025 with <c>Mark.MarkAttributes(object, name)</c>."),
            ("AssociativeNoteProperties", "ContentString3", "SurfaceTreatmentElements", Location.Table, "What a note on a surface treatment shows (<c>ContentString3</c>, \"Surface treatment\"). Not checked in Tekla (the model had no surface treatment): the shipped files hold the name and surfacing name elements here."),
            ("AssociativeNoteProperties", "ContentString4", "ReinforcementElements", Location.Table, "What a note on reinforcement shows (<c>ContentString4</c>, \"Rebar\"; single bars, groups and meshes). Checked in Tekla 2025 with <c>Mark.MarkAttributes(object, name)</c>."),
            ("AssociativeNoteProperties", "ChamferContentString", "EdgeChamferElements", Location.Table, "What a note on an edge chamfer shows (<c>ChamferContentString</c>, \"Edge chamfer\" of the dialog's menu). Not checked in Tekla (the model had no edge chamfer): the shipped files hold the chamfer's x and y here."),
            ("AssociativeNoteProperties", "ReferenceObjectContentString", "ReferenceObjectElements", Location.Table, "What a note on a reference model shows (<c>ReferenceObjectContentString</c>, \"Reference object\"). Checked in Tekla 2025 with <c>Mark.MarkAttributes(object, name)</c>."),

            ("NeighborPartMark", "ContentString1", "MainPartElements", Location.Table, $"What the marks of neighbouring main parts show (<c>ContentString1</c>, \"Main part\"; <see cref=\"{Types}PartMark\"/>).{ViewProbe}"),
            ("NeighborPartMark", "ContentString2", "SecondaryPartElements", Location.Table, "What the marks of neighbouring secondary parts show (<c>ContentString2</c>, \"Secondary part\" of the dialog's menu, as on the part mark page). Not seen read: Tekla 2025 made no mark for a neighbouring secondary part in the probes, also with <c>for_sec</c> on."),
            ("NeighborReinforcementMark", "ContentString1", "SingleElements", Location.Table, $"What the marks of neighbouring single bars show (<c>ContentString1</c>, \"Single\"; <see cref=\"{Types}ReinforcementMark\"/>).{ViewProbe}"),
            ("NeighborReinforcementMark", "ContentString2", "GroupElements", Location.Table, $"What the marks of neighbouring reinforcement groups show (<c>ContentString2</c>, \"Group\").{ViewProbe}"),
            ("NeighborReinforcementMark", "ContentString3", "MergedMarkElements", Location.Merge, "The merged mark (<c>ContentString3</c>), as on the reinforcement mark page: the files hold only <c>&lt;MergeElement subType=\"53\"&gt;</c> ... <c>\"56\"</c> here (kept as <see cref=\"global::TeklaHelper.DrawingAttribute.Marks.MarkXmlElement\"/>); not seen read."),
            ("NeighborReinforcementMark", "ContentString4", "MeshElements", Location.Table, $"What the marks of neighbouring reinforcement meshes show (<c>ContentString4</c>, \"Mesh\"; <see cref=\"{Types}ReinforcementMeshMark\"/>).{ViewProbe}"),
            ("PourMark", "PourContentString", "PourElements", Location.Table, "What pour marks show (<c>PourContentString</c>; the page has one table): the table the pour mark dialog of the drawing editor edits (checked in Tekla 2025)."),
            ("ConnectionMark", "ContentString1", "Elements", Location.Table, $"What connection marks show (<c>ContentString1</c>; the page has one table; <see cref=\"{Types}ConnectionMark\"/>).{ViewProbe}"),
            ("SurfaceTreatmentMark", "ContentString1", "Elements", Location.Table, $"What surface treatment marks show (<c>ContentString1</c>; the page has one table; <see cref=\"{Types}SurfaceTreatmentMark\"/>).{ViewProbe}"),
            ("DimensionMark", "ContentString1", "PartElements", Location.Table, "What the mark shows for parts (<c>ContentString1</c>, \"Part\" of the dialog's menu). From the menu and the shipped files, which hold part elements here, like the associative note; not probed in Tekla."),
            ("DimensionMark", "ContentString2", "BoltElements", Location.Table, "What the mark shows for bolts (<c>ContentString2</c>, \"Bolt\"). From the menu and the shipped files (bolt elements), like the associative note; not probed in Tekla."),
            ("DimensionMark", "ContentString3", "SurfaceTreatmentElements", Location.Table, "What the mark shows for surface treatments (<c>ContentString3</c>, \"Surface treatment\"). From the menu and the shipped files, like the associative note; not probed in Tekla."),
            ("DimensionMark", "ContentString4", "ReinforcementElements", Location.Table, "What the mark shows for reinforcement (<c>ContentString4</c>, \"Rebar\"). From the menu and the shipped files (reinforcement elements), like the associative note; not probed in Tekla."),

            ("ViewLabelMark", "ContentString", "Elements", Location.Table, "The elements of the label row (<c>ContentString</c>): what the row shows, e.g. the view name and the scale (<see cref=\"" + Types + "ViewLabel\"/>, <see cref=\"" + Types + "SectionMark\"/>, <see cref=\"" + Types + "DetailMark\"/>), texts, symbols and frames; null when the row has none, or text that is no mark content (<see cref=\"global::TeklaHelper.DrawingAttribute.AttributeSection.GetMarkContent\"/> says why). Checked in Tekla 2025: the Open API read the elements of a view label, a section mark and a detail mark the library wrote. To change a row: <c>MarkContent row = view.Attributes.LabelMarkA1.Content.Elements; row.AddText(\" (preliminary)\"); view.Attributes.LabelMarkA1.Content.Elements = row;</c>"),
        };

        private static readonly (string Key, string Property, string Summary)[] Generic =
        {
            ("PourContentString", "PourElements", "Mark content of pour objects (<c>PourContentString</c>). The pour mark dialog of the drawing editor edits this table (checked in Tekla 2025); on the other mark pages it is named after the key, not probed."),
            ("PourBreakContentString", "PourBreakElements", "Mark content of pour breaks (<c>PourBreakContentString</c>), named after the key; not probed."),
            ("ChamferContentString", "EdgeChamferElements", "Mark content of edge chamfers (<c>ChamferContentString</c>), named after the key; not probed."),
            ("ReferenceObjectContentString", "ReferenceObjectElements", "Mark content of reference objects (<c>ReferenceObjectContentString</c>), named after the key; not probed on this page (the associative note shows it for reference models)."),
        };

        /// <summary>
        /// Where a content property goes: the container of the dialog's elements table (the tables of the "Mark content of"
        /// menu and single tables), of the merged mark table (Merging tab), or of the key's control (keys the dialog does not
        /// show: Advanced).
        /// </summary>
        public enum Location
        {
            Table,
            Merge,
            Control,
        }

        /// <summary>The content properties of a page that has the given keys, in the order of <paramref name="keys"/>.</summary>
        public static IEnumerable<(string Key, string Property, string Summary, Location Where)> For(string pageClass, IEnumerable<string> keys)
        {
            foreach (string key in keys)
            {
                var specific = Specific.FirstOrDefault(e => e.Page == pageClass && e.Key == key);
                if (specific.Property != null)
                {
                    yield return (key, specific.Property, specific.Summary + Copy, specific.Where);
                    continue;
                }

                var generic = Generic.FirstOrDefault(e => e.Key == key);
                if (generic.Property != null)
                {
                    yield return (key, generic.Property, generic.Summary + Copy, Location.Control);
                }
            }
        }
    }
}
