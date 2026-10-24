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

        private static readonly (string Page, string Key, string Property, string Summary)[] Specific =
        {
            ("PartMark", "ContentString1", "MainPartElements", $"What the marks of main parts show (<c>ContentString1</c>, \"Main part\"; <see cref=\"{Types}PartMark\"/>).{ViewProbe}"),
            ("PartMark", "ContentString2", "SecondaryPartElements", $"What the marks of secondary parts show (<c>ContentString2</c>, \"Secondary part\").{ViewProbe}"),
            ("PartMark", "ContentString3", "SubAssemblyMainPartElements", $"What the marks of the main parts of sub-assemblies show (<c>ContentString3</c>, \"Sub-assembly main part\").{ViewProbe}"),
            ("PartMark", "ContentString4", "SubAssemblySecondaryPartElements", $"What the marks of the secondary parts of sub-assemblies show (<c>ContentString4</c>, \"Sub-assembly secondary part\").{ViewProbe}"),
            ("BoltMark", "ContentString1", "SiteBoltElements", $"What the marks of site bolts show (<c>ContentString1</c>, \"Site\"; <see cref=\"{Types}BoltMark\"/>).{ViewProbe}"),
            ("BoltMark", "ContentString2", "WorkshopBoltElements", $"What the marks of workshop bolts show (<c>ContentString2</c>, \"Shop\").{ViewProbe}"),
            ("ReinforcementMark", "ContentString1", "SingleElements", $"What the marks of single bars show (<c>ContentString1</c>, \"Single\"; <see cref=\"{Types}ReinforcementMark\"/>).{ViewProbe}"),
            ("ReinforcementMark", "ContentString2", "GroupElements", $"What the marks of reinforcement groups show (<c>ContentString2</c>, \"Group\"); marks merged by <c>MergeMarks</c> and the leader line options show it too.{ViewProbe}"),
            ("ReinforcementMark", "ContentString3", "MergedMarkElements", "The merged mark (<c>ContentString3</c>): its block prefix, single mark content, distances between groups and separator, which the files store as <c>&lt;MergeElement subType=\"53\"&gt;</c> ... <c>\"56\"</c> (the Open API's <c>MergedMarkPropertyElementTypes</c>) and the library keeps as <see cref=\"global::TeklaHelper.DrawingAttribute.Marks.MarkXmlElement\"/>. The files hold nothing else in this table; Tekla 2025 was not seen to read it (the Open API cannot read the content of a merged mark set)."),
            ("ReinforcementMark", "ContentString4", "MeshElements", $"What the marks of reinforcement meshes show (<c>ContentString4</c>, \"Mesh\"; <see cref=\"{Types}ReinforcementMeshMark\"/>).{ViewProbe}"),
            ("AssociativeNoteProperties", "ContentString1", "PartElements", "What a note on a part shows (<c>ContentString1</c>, \"Part\"; every part, also of sub-assemblies). Checked in Tekla 2025 with <c>Mark.MarkAttributes(object, name)</c>."),
            ("AssociativeNoteProperties", "ContentString2", "BoltElements", "What a note on a bolt shows (<c>ContentString2</c>, \"Bolt\"; site and workshop bolts). Checked in Tekla 2025 with <c>Mark.MarkAttributes(object, name)</c>."),
            ("AssociativeNoteProperties", "ContentString3", "SurfaceTreatmentElements", "What a note on a surface treatment shows (<c>ContentString3</c>, \"Surface treatment\"). Not checked in Tekla (the model had no surface treatment): the shipped files hold the name and surfacing name elements here."),
            ("AssociativeNoteProperties", "ContentString4", "ReinforcementElements", "What a note on reinforcement shows (<c>ContentString4</c>, \"Rebar\"; single bars, groups and meshes). Checked in Tekla 2025 with <c>Mark.MarkAttributes(object, name)</c>."),
            ("AssociativeNoteProperties", "ReferenceObjectContentString", "ReferenceObjectElements", "What a note on a reference model shows (<c>ReferenceObjectContentString</c>, \"Reference object\"). Checked in Tekla 2025 with <c>Mark.MarkAttributes(object, name)</c>."),

            // Hand-written (Pages/ViewLabelMark.cs, with an example in its comment): no summary, the generator only knows the name
            ("ViewLabelMark", "ContentString", "Elements", null),
        };

        private static readonly (string Key, string Property, string Summary)[] Generic =
        {
            ("PourContentString", "PourElements", "Mark content of pour objects (<c>PourContentString</c>). The pour mark dialog of the drawing editor edits this table (checked in Tekla 2025); on the other mark pages it is named after the key, not probed."),
            ("PourBreakContentString", "PourBreakElements", "Mark content of pour breaks (<c>PourBreakContentString</c>), named after the key; not probed."),
            ("ChamferContentString", "EdgeChamferElements", "Mark content of edge chamfers (<c>ChamferContentString</c>), named after the key; not probed."),
            ("ReferenceObjectContentString", "ReferenceObjectElements", "Mark content of reference objects (<c>ReferenceObjectContentString</c>), named after the key; not probed on this page (the associative note shows it for reference models)."),
        };

        /// <summary>
        /// The content properties the generator writes for a page that has the given keys, in the order of
        /// <paramref name="keys"/> (not the hand-written ones).
        /// </summary>
        public static IEnumerable<(string Key, string Property, string Summary)> For(string pageClass, IEnumerable<string> keys)
        {
            foreach (string key in keys)
            {
                (string Key, string Property, string Summary) entry = Find(pageClass, key);
                if (entry.Property != null && entry.Summary != null)
                {
                    yield return (key, entry.Property, entry.Summary + Copy);
                }
            }
        }

        /// <summary>The content property of a key of a page; null when it has none.</summary>
        public static string PropertyFor(string pageClass, string key) => Find(pageClass, key).Property;

        private static (string Key, string Property, string Summary) Find(string pageClass, string key)
        {
            var specific = Specific.FirstOrDefault(e => e.Page == pageClass && e.Key == key);
            if (specific.Property != null)
            {
                return (key, specific.Property, specific.Summary);
            }

            return Generic.FirstOrDefault(e => e.Key == key);
        }
    }
}
