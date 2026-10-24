namespace DialogClassGenerator
{
    /// <summary>
    /// An attribute file of a drawing object (the file a property dialog of the drawing editor saves) and the pages it
    /// holds. The generator writes its class (unless it is written by hand) and its table in <c>FileDefaults</c>.
    /// </summary>
    internal sealed class ObjectFileSpec
    {
        public string Class { get; set; }

        public string Extension { get; set; }

        /// <summary>Name of its class in <c>FileDefaults</c>.</summary>
        public string Table { get; set; }

        /// <summary>What the file holds, for the summary of its table ("text properties (.drtxt)").</summary>
        public string Title { get; set; }

        /// <summary>The first sentences of the class summary.</summary>
        public string Summary { get; set; }

        /// <summary>
        /// How the Open API reads the file, verified in Tekla 2025 (a probe loaded each file type with each attribute class of
        /// Tekla.Structures.Drawing that takes a file name, and with <c>Mark.MarkAttributes</c>); null when none reads it.
        /// </summary>
        public string OpenApi { get; set; }

        /// <summary>Dialog name in front of every key of legacy files (<c>text_dial.</c>).</summary>
        public string LegacyKeyPrefix { get; set; }

        /// <summary>The file saved from the dialog, relative to the folder of the sample .vi: the keys and their order.</summary>
        public string DialogSampleFile { get; set; }

        /// <summary>The class is written by hand (released before the generator wrote file classes): only its table is generated.</summary>
        public bool HandWritten { get; set; }

        /// <summary>(property, page class, prefix of its keys, summary of the property), in the order of the file.</summary>
        public (string Property, string Page, string Prefix, string Summary)[] Pages { get; set; }

        /// <summary>Summary of a <c>Marks</c> property listing the <c>DimensionMark</c> pages; null for none.</summary>
        public string MarksSummary { get; set; }

        /// <summary>
        /// The one content table of the file's dialog in the drawing editor: the key it reads, the keys Save as writes it to,
        /// and what else was seen (checked in Tekla 2025, see CLAUDE.md); null for files whose dialog has none or several.
        /// </summary>
        public (string Read, string[] Writes, string Dialog, string Note)? DialogContent { get; set; }
    }

    internal static class ObjectFileSpecs
    {
        private const string DimensionMarks = "The nine marks: prefix, postfix and the tags A to G.";

        private static readonly (string Property, string Page, string Prefix, string Summary)[] DimensionPages =
        {
            ("Properties", "DimensionProperties", "", "The Dimension properties dialog: the tabs General, Appearance, Marks and Tags (the top-level keys)."),
            ("PrefixMark", "DimensionMark", "PrefixMark.", "The mark before the dimension value (<c>PrefixMark.*</c>; \"...\" next to Prefix on the Marks tab)."),
            ("PostfixMark", "DimensionMark", "PostfixMark.", "The mark after the dimension value (<c>PostfixMark.*</c>; \"...\" next to Postfix on the Marks tab)."),
            ("LeftUpperTagMark", "DimensionMark", "LeftUpperTagMark.", "Tag A, left upper (<c>LeftUpperTagMark.*</c>)."),
            ("LeftMiddleTagMark", "DimensionMark", "LeftMiddleTagMark.", "Tag B, left middle (<c>LeftMiddleTagMark.*</c>)."),
            ("LeftLowerTagMark", "DimensionMark", "LeftLowerTagMark.", "Tag C, left lower (<c>LeftLowerTagMark.*</c>)."),
            ("MiddleLowerTagMark", "DimensionMark", "MiddleLowerTagMark.", "Tag D, middle lower (<c>MiddleLowerTagMark.*</c>)."),
            ("RightUpperTagMark", "DimensionMark", "RightUpperTagMark.", "Tag E, right upper (<c>RightUpperTagMark.*</c>)."),
            ("RightMiddleTagMark", "DimensionMark", "RightMiddleTagMark.", "Tag F, right middle (<c>RightMiddleTagMark.*</c>)."),
            ("RightLowerTagMark", "DimensionMark", "RightLowerTagMark.", "Tag G, right lower (<c>RightLowerTagMark.*</c>)."),
        };

        public static readonly ObjectFileSpec[] All =
        {
            new ObjectFileSpec
            {
                Class = "DimensionAttributeFile", Extension = ".dim", Table = "Dimension", Title = "dimension properties (.dim)", HandWritten = true,
                Pages = DimensionPages,
            },
            new ObjectFileSpec
            {
                Class = "ReinforcementDimensionAttributeFile", Extension = ".rdim", Table = "ReinforcementDimension", Title = "reinforcement dimension properties (.rdim)",
                Summary = "Reinforcement dimension properties (.rdim): the file the dimension properties of reinforcement (dialog <c>gr_rebar_dim_attrib</c>) save. It holds the keys of a .dim file: <see cref=\"Properties\"/> the dialog, each mark of the dimension a <see cref=\"DimensionMark\"/> under its own prefix.",
                LegacyKeyPrefix = "rebar_dim_dial.", DialogSampleFile = "OBJECT-TEST.rdim", Pages = DimensionPages, MarksSummary = DimensionMarks,
            },
            new ObjectFileSpec
            {
                Class = "TextAttributeFile", Extension = ".drtxt", Table = "Text", Title = "text properties (.drtxt)",
                Summary = "Text properties (.drtxt): the file the Text properties dialog of the drawing editor saves, for texts and texts with a leader line.",
                OpenApi = "<c>new Text.TextAttributes(name)</c>", LegacyKeyPrefix = "text_dial.", DialogSampleFile = "OBJECT-TEST.drtxt",
                Pages = new[] { ("Properties", "TextProperties", "", "The Text properties dialog (the top-level keys and the placing, <c>txpl.*</c>).") },
            },
            new ObjectFileSpec
            {
                Class = "SymbolAttributeFile", Extension = ".sbl", Table = "Symbol", Title = "symbol properties (.sbl)",
                Summary = "Symbol properties (.sbl): the file the Symbol properties dialog of the drawing editor saves.",
                OpenApi = "<c>new Symbol.SymbolAttributes(name)</c>", LegacyKeyPrefix = "symbol_dial.", DialogSampleFile = "OBJECT-TEST.sbl",
                Pages = new[] { ("Properties", "SymbolProperties", "", "The Symbol properties dialog (the top-level keys and the placing, <c>txpl.*</c>).") },
            },
            new ObjectFileSpec
            {
                Class = "LevelMarkAttributeFile", Extension = ".lev", Table = "LevelMark", Title = "level mark properties (.lev)",
                Summary = "Level mark properties (.lev): the file the Level mark properties dialog of the drawing editor saves.",
                OpenApi = "<c>new LevelMark.LevelMarkAttributes(name)</c>", LegacyKeyPrefix = "level_dial.", DialogSampleFile = "OBJECT-TEST.lev",
                Pages = new[] { ("Properties", "LevelMarkProperties", "", "The Level mark properties dialog (the top-level keys and the placing, <c>txpl.*</c>).") },
            },
            new ObjectFileSpec
            {
                Class = "RevisionMarkAttributeFile", Extension = ".rev", Table = "RevisionMark", Title = "revision mark properties (.rev)",
                Summary = "Revision mark properties (.rev): the file the Revision mark properties dialog of the drawing editor saves.",
                LegacyKeyPrefix = "revision_dial.", DialogSampleFile = "OBJECT-TEST.rev",
                Pages = new[] { ("Properties", "RevisionMarkProperties", "", "The Revision mark properties dialog (the top-level keys and the placing, <c>txpl.*</c>).") },
            },
            new ObjectFileSpec
            {
                Class = "AssociativeNoteAttributeFile", Extension = ".note", Table = "AssociativeNote", Title = "associative note properties (.note)",
                Summary = "Associative note properties (.note): the file the Associative note properties dialog of the drawing editor saves. It holds the keys of every mark (content, frame, font, units, leader line, placing).",
                OpenApi = "<c>new Mark.MarkAttributes(modelObject, name)</c>, the attributes of the marks the Open API creates (the part, bolt and other mark files are not read by name)", LegacyKeyPrefix = "note_dial.", DialogSampleFile = "OBJECT-TEST.note",
                Pages = new[] { ("Properties", "AssociativeNoteProperties", "", "The Associative note properties dialog (the top-level keys and the placing, <c>txpl.*</c>).") },
            },
            new ObjectFileSpec
            {
                Class = "PartMarkAttributeFile",
                DialogContent = ("ContentString1", new[] { "ContentString1", "ContentString2", "ContentString3", "ContentString4" }, "Part mark properties", " Modify gave every selected part mark this content, main, secondary and sub-assembly parts alike."), Extension = ".pm", Table = "PartMark", Title = "part mark properties (.pm)",
                Summary = "Part mark properties (.pm): the file the Part mark properties dialog of the drawing editor saves. It holds the keys of the Part mark page of the view properties, so <see cref=\"Properties\"/> is that page's class.",
                LegacyKeyPrefix = "pmark_dial.", DialogSampleFile = "OBJECT-TEST.pm",
                Pages = new[] { ("Properties", "PartMark", "", "The part mark settings (the top-level keys).") },
            },
            new ObjectFileSpec
            {
                Class = "BoltMarkAttributeFile",
                DialogContent = ("ContentString1", new[] { "ContentString1", "ContentString2", "ContentString3", "ContentString4" }, "Bolt mark properties", string.Empty), Extension = ".sm", Table = "BoltMark", Title = "bolt mark properties (.sm)",
                Summary = "Bolt mark properties (.sm): the file the Bolt mark properties dialog of the drawing editor saves. It holds the keys of the Bolt mark page of the view properties, so <see cref=\"Properties\"/> is that page's class.",
                LegacyKeyPrefix = "smark_dial.", DialogSampleFile = "OBJECT-TEST.sm",
                Pages = new[] { ("Properties", "BoltMark", "", "The bolt mark settings (the top-level keys).") },
            },
            new ObjectFileSpec
            {
                Class = "ConnectionMarkAttributeFile",
                DialogContent = ("ContentString1", new[] { "ContentString1", "ContentString2", "ContentString4" }, "Connection mark properties", " <c>ContentString3</c> is left as it is."), Extension = ".jm", Table = "ConnectionMark", Title = "connection mark properties (.jm)",
                Summary = "Connection mark properties (.jm): the file the Connection mark properties dialog of the drawing editor saves. It holds the keys of the Connection mark page of the view properties, so <see cref=\"Properties\"/> is that page's class.",
                LegacyKeyPrefix = "jmark_dial.", DialogSampleFile = "OBJECT-TEST.jm",
                Pages = new[] { ("Properties", "ConnectionMark", "", "The connection mark settings (the top-level keys).") },
            },
            new ObjectFileSpec
            {
                Class = "ReinforcementMarkAttributeFile",
                DialogContent = ("ContentString1", new[] { "ContentString1", "ContentString2", "ContentString4" }, "Reinforcement mark properties", " <c>ContentString3</c>, the merged mark, is left as it is."), Extension = ".rm", Table = "ReinforcementMark", Title = "reinforcement mark properties (.rm)",
                Summary = "Reinforcement mark properties (.rm): the file the Reinforcement mark properties dialog of the drawing editor saves. It holds the keys of the Reinforcement mark page of the view properties, so <see cref=\"Properties\"/> is that page's class.",
                LegacyKeyPrefix = "rebar_mark_dial.", DialogSampleFile = "OBJECT-TEST.rm",
                Pages = new[] { ("Properties", "ReinforcementMark", "", "The reinforcement mark settings (the top-level keys).") },
            },
            new ObjectFileSpec
            {
                Class = "SurfaceTreatmentMarkAttributeFile",
                DialogContent = ("ContentString1", new[] { "ContentString1", "ContentString2", "ContentString4" }, "Surface treatment mark properties", " <c>ContentString3</c> is left as it is."), Extension = ".surfm", Table = "SurfaceTreatmentMark", Title = "surface treatment mark properties (.surfm)",
                Summary = "Surface treatment mark properties (.surfm): the file the Surface treatment mark properties dialog of the drawing editor saves. It holds the keys of the Surface treatment mark page of the view properties, so <see cref=\"Properties\"/> is that page's class.",
                LegacyKeyPrefix = "surf_mark_dial.", DialogSampleFile = "OBJECT-TEST.surfm",
                Pages = new[] { ("Properties", "SurfaceTreatmentMark", "", "The surface treatment mark settings (the top-level keys).") },
            },
            new ObjectFileSpec
            {
                Class = "PourMarkAttributeFile",
                DialogContent = ("PourContentString", new[] { "PourContentString", "ContentString1" }, "Pour mark properties", " <c>ContentString2</c> to <c>ContentString4</c> are left as they are."), Extension = ".pom", Table = "PourMark", Title = "pour mark properties (.pom)",
                Summary = "Pour mark properties (.pom): the file the Pour mark properties dialog of the drawing editor saves. It holds the keys of the Pour mark page of the view properties, so <see cref=\"Properties\"/> is that page's class.",
                LegacyKeyPrefix = "pour_mark_dial.", DialogSampleFile = "OBJECT-TEST.pom",
                Pages = new[] { ("Properties", "PourMark", "", "The pour mark settings (the top-level keys).") },
            },
            new ObjectFileSpec
            {
                Class = "WeldMarkAttributeFile", Extension = ".wel", Table = "WeldMark", Title = "weld mark properties (.wel)",
                Summary = "Weld mark properties (.wel): the file the properties dialog of the weld marks of model welds saves (the weld marks drawn in the drawing editor use .wls files, <see cref=\"DrawingWeldMarkAttributeFile\"/>). It holds the keys of the Weld mark page of the view properties, so <see cref=\"Properties\"/> is that page's class.",
                OpenApi = "<c>new WeldMark.WeldMarkAttributes(name)</c>", LegacyKeyPrefix = "weld_dial.", DialogSampleFile = "OBJECT-TEST.wel",
                Pages = new[] { ("Properties", "WeldMark", "", "The weld mark settings (the top-level keys and the placing, <c>weldpl.*</c>).") },
            },
            new ObjectFileSpec
            {
                Class = "DrawingWeldMarkAttributeFile", Extension = ".wls", Table = "DrawingWeldMark", Title = "weld mark properties of drawing weld marks (.wls)",
                Summary = "Weld mark properties (.wls): the file the Weld mark properties dialog saves for the weld marks drawn in the drawing editor (the weld marks of model welds use .wel files, <see cref=\"WeldMarkAttributeFile\"/>).",
                LegacyKeyPrefix = "wld_dial.", DialogSampleFile = "OBJECT-TEST.wls",
                Pages = new[] { ("Properties", "DrawingWeldMarkProperties", "", "The Weld mark properties dialog (the top-level keys and the placing, <c>wldpl.*</c>).") },
            },
            new ObjectFileSpec
            {
                Class = "LineAttributeFile", Extension = ".gln", Table = "Line", Title = "line properties (.gln)",
                Summary = "Line properties (.gln): the file the Line properties dialog of the drawing editor saves.",
                OpenApi = "<c>new Line.LineAttributes(name)</c>", LegacyKeyPrefix = "go_line_dial.", DialogSampleFile = "OBJECT-TEST.gln",
                Pages = new[] { ("Properties", "LineProperties", "", "The Line properties dialog (the top-level keys and <c>fo_attr.*</c>).") },
            },
            new ObjectFileSpec
            {
                Class = "ArcAttributeFile", Extension = ".gar", Table = "Arc", Title = "arc properties (.gar)",
                Summary = "Arc properties (.gar): the file the Arc properties dialog of the drawing editor saves.",
                OpenApi = "<c>new Arc.ArcAttributes(name)</c>", LegacyKeyPrefix = "go_arc_dial.", DialogSampleFile = "OBJECT-TEST.gar",
                Pages = new[] { ("Properties", "ArcProperties", "", "The Arc properties dialog (the top-level keys and <c>fo_attr.*</c>).") },
            },
            new ObjectFileSpec
            {
                Class = "PolylineAttributeFile", Extension = ".gpl", Table = "Polyline", Title = "polyline properties (.gpl)",
                Summary = "Polyline properties (.gpl): the file the Polyline properties dialog of the drawing editor saves.",
                OpenApi = "<c>new Polyline.PolylineAttributes(name)</c>", LegacyKeyPrefix = "go_polyline_dial.", DialogSampleFile = "OBJECT-TEST.gpl",
                Pages = new[] { ("Properties", "PolylineProperties", "", "The Polyline properties dialog (the top-level keys and <c>fo_attr.*</c>).") },
            },
            new ObjectFileSpec
            {
                Class = "CircleAttributeFile", Extension = ".gci", Table = "Circle", Title = "circle properties (.gci)",
                Summary = "Circle properties (.gci): the file the Circle properties dialog of the drawing editor saves.",
                OpenApi = "<c>new Circle.CircleAttributes(name)</c>", LegacyKeyPrefix = "go_circle_dial.", DialogSampleFile = "OBJECT-TEST.gci",
                Pages = new[] { ("Properties", "CircleProperties", "", "The Circle properties dialog (the top-level keys and <c>fo_attr.*</c>).") },
            },
            new ObjectFileSpec
            {
                Class = "RectangleAttributeFile", Extension = ".grt", Table = "Rectangle", Title = "rectangle properties (.grt)",
                Summary = "Rectangle properties (.grt): the file the Rectangle properties dialog of the drawing editor saves.",
                OpenApi = "<c>new Rectangle.RectangleAttributes(name)</c>", LegacyKeyPrefix = "go_rectangle_dial.", DialogSampleFile = "OBJECT-TEST.grt",
                Pages = new[] { ("Properties", "RectangleProperties", "", "The Rectangle properties dialog (the top-level keys and <c>fo_attr.*</c>).") },
            },
            new ObjectFileSpec
            {
                Class = "PolygonAttributeFile", Extension = ".gpg", Table = "Polygon", Title = "polygon properties (.gpg)",
                Summary = "Polygon properties (.gpg): the file the Polygon properties dialog of the drawing editor saves; clouds read it too.",
                OpenApi = "<c>new Polygon.PolygonAttributes(name)</c> and <c>new Cloud.CloudAttributes(name)</c>", LegacyKeyPrefix = "go_polygon_dial.", DialogSampleFile = "OBJECT-TEST.gpg",
                Pages = new[] { ("Properties", "PolygonProperties", "", "The Polygon properties dialog (the top-level keys and <c>fo_attr.*</c>).") },
            },
            new ObjectFileSpec
            {
                Class = "TextFileAttributeFile", Extension = ".fas", Table = "TextFile", Title = "text file properties (.fas)",
                Summary = "Text file properties (.fas): the file the properties dialog of text files (ASCII files) in drawings saves.",
                LegacyKeyPrefix = "fo_asc_dial.", DialogSampleFile = "OBJECT-TEST.fas",
                Pages = new[] { ("Properties", "TextFileProperties", "", "The Text file properties dialog (the top-level keys).") },
            },
            new ObjectFileSpec
            {
                Class = "DwgAttributeFile", Extension = ".fdg", Table = "Dwg", Title = "DWG/DXF properties (.fdg)",
                Summary = "DWG/DXF properties (.fdg): the file the properties dialog of DWG and DXF files in drawings saves.",
                OpenApi = "<c>new EmbeddedObjectAttributes(name)</c> (for DWG/DXF objects and images alike)", LegacyKeyPrefix = "fo_dwg_dial.", DialogSampleFile = "OBJECT-TEST.fdg",
                Pages = new[] { ("Properties", "DwgProperties", "", "The DWG/DXF properties dialog (the top-level keys).") },
            },
            new ObjectFileSpec
            {
                Class = "ImageAttributeFile", Extension = ".idg", Table = "Image", Title = "image properties (.idg)",
                Summary = "Image properties (.idg): the file the Image properties dialog of the drawing editor saves (the images of the Open API take their settings from .fdg files, <see cref=\"DwgAttributeFile\"/>).",
                LegacyKeyPrefix = "fo_image_dial.", DialogSampleFile = "OBJECT-TEST.idg",
                Pages = new[] { ("Properties", "ImageProperties", "", "The Image properties dialog (the top-level keys).") },
            },
            new ObjectFileSpec
            {
                Class = "HyperlinkAttributeFile", Extension = ".fhl", Table = "Hyperlink", Title = "hyperlink properties (.fhl)",
                Summary = "Hyperlink properties (.fhl): the file the Hyperlink properties dialog of the drawing editor saves.",
                OpenApi = "<c>new LinkAttributes(name)</c>", LegacyKeyPrefix = "fo_link_dial.", DialogSampleFile = "OBJECT-TEST.fhl",
                Pages = new[] { ("Properties", "HyperlinkProperties", "", "The Hyperlink properties dialog (the top-level keys).") },
            },
            new ObjectFileSpec
            {
                Class = "LinkedDrawingAttributeFile", Extension = ".ldr", Table = "LinkedDrawing", Title = "linked drawing properties (.ldr)",
                Summary = "Linked drawing properties (.ldr): the file the properties dialog of drawing links in drawings saves.",
                LegacyKeyPrefix = "link_drawing_dial.", DialogSampleFile = "OBJECT-TEST.ldr",
                Pages = new[] { ("Properties", "LinkedDrawingProperties", "", "The Linked drawing properties dialog (the top-level keys).") },
            },
            new ObjectFileSpec
            {
                Class = "PartAttributeFile", Extension = ".dprt", Table = "Part", Title = "part properties in drawings (.dprt)",
                Summary = "Part properties in drawings (.dprt): the file the Part properties dialog of the drawing editor saves, for the parts selected in a drawing.",
                OpenApi = "<c>new Part.PartAttributes(name)</c>", LegacyKeyPrefix = "part_dial.", DialogSampleFile = "OBJECT-TEST.dprt",
                Pages = new[] { ("Properties", "PartProperties", "", "The Part properties dialog (the top-level keys and the hatches, <c>PartHatch.*</c> and <c>SectionHatch.*</c>).") },
            },
            new ObjectFileSpec
            {
                Class = "SurfaceTreatmentAttributeFile", Extension = ".dsrf", Table = "SurfaceTreatment", Title = "surface treatment properties in drawings (.dsrf)",
                Summary = "Surface treatment properties in drawings (.dsrf): the file the Surface treatment properties dialog of the drawing editor saves.",
                OpenApi = "<c>new Surfacing.SurfacingAttributes(name)</c>", LegacyKeyPrefix = "surfacing_dial.", DialogSampleFile = "OBJECT-TEST.dsrf",
                Pages = new[] { ("Properties", "SurfaceTreatmentProperties", "", "The Surface treatment properties dialog (the top-level keys and the hatches).") },
            },
            new ObjectFileSpec
            {
                Class = "PourAttributeFile", Extension = ".po", Table = "Pour", Title = "pour object properties in drawings (.po)",
                Summary = "Pour object properties in drawings (.po): the file the Pour object properties dialog of the drawing editor saves.",
                OpenApi = "<c>new PourObject.PourAttributes(name)</c>", LegacyKeyPrefix = "pour_dial.", DialogSampleFile = "OBJECT-TEST.po",
                Pages = new[] { ("Properties", "PourProperties", "", "The Pour object properties dialog (the top-level keys and the hatches).") },
            },
            new ObjectFileSpec
            {
                Class = "PourBreakAttributeFile", Extension = ".pbr", Table = "PourBreak", Title = "pour break properties in drawings (.pbr)",
                Summary = "Pour break properties in drawings (.pbr): the file the Pour break properties dialog of the drawing editor saves. It holds the keys of the Pour break page of the view properties, so <see cref=\"Properties\"/> is that page's class.",
                OpenApi = "<c>new PourBreak.PourBreakAttributes(name)</c>", LegacyKeyPrefix = "pour_break_dial.", DialogSampleFile = "OBJECT-TEST.pbr",
                Pages = new[] { ("Properties", "PourBreakObject", "", "The pour break settings (the top-level keys).") },
            },
            new ObjectFileSpec
            {
                Class = "BoltAttributeFile", Extension = ".sc", Table = "Bolt", Title = "bolt properties in drawings (.sc)",
                Summary = "Bolt properties in drawings (.sc): the file the Bolt properties dialog of the drawing editor saves. It holds the keys of the Bolt page of the view properties, so <see cref=\"Properties\"/> is that page's class.",
                OpenApi = "<c>new Bolt.BoltAttributes(name)</c>", LegacyKeyPrefix = "screw_dial.", DialogSampleFile = "OBJECT-TEST.sc",
                Pages = new[] { ("Properties", "BoltObject", "", "The bolt settings (the top-level keys).") },
            },
            new ObjectFileSpec
            {
                Class = "WeldAttributeFile", Extension = ".welo", Table = "Weld", Title = "weld properties in drawings (.welo)",
                Summary = "Weld properties in drawings (.welo): the file the Weld properties dialog of the drawing editor saves. It holds the keys of the Weld page of the view properties, so <see cref=\"Properties\"/> is that page's class.",
                OpenApi = "<c>new Weld.WeldAttributes(name)</c>", LegacyKeyPrefix = "weldobject_dial.", DialogSampleFile = "OBJECT-TEST.welo",
                Pages = new[] { ("Properties", "WeldObject", "", "The weld settings (the top-level keys).") },
            },
            new ObjectFileSpec
            {
                Class = "EdgeChamferAttributeFile", Extension = ".dcha", Table = "EdgeChamfer", Title = "edge chamfer properties in drawings (.dcha)",
                Summary = "Edge chamfer properties in drawings (.dcha): the file the Edge chamfer properties dialog of the drawing editor saves.",
                OpenApi = "<c>new EdgeChamfer.EdgeChamferAttributes(name)</c>", LegacyKeyPrefix = "edge_chamfer_dial.", DialogSampleFile = "OBJECT-TEST.dcha",
                Pages = new[] { ("Properties", "EdgeChamferProperties", "", "The Edge chamfer properties dialog (the top-level keys).") },
            },
            new ObjectFileSpec
            {
                Class = "ReinforcementAttributeFile", Extension = ".drbr", Table = "Reinforcement", Title = "reinforcement properties in drawings (.drbr)",
                Summary = "Reinforcement properties in drawings (.drbr): the file the Reinforcement properties dialog of the drawing editor saves.",
                OpenApi = "the attribute classes of every reinforcement (<c>ReinforcementBase.ReinforcementSingleAttributes</c>, <c>ReinforcementGroupAttributes</c>, <c>ReinforcementSetGroupAttributes</c>, <c>ReinforcementStrandAttributes</c>, and <c>ReinforcementMeshAttributes</c>, which reads .drbr, not .drmsh)",
                LegacyKeyPrefix = "rebar_dial.", DialogSampleFile = "OBJECT-TEST.drbr",
                Pages = new[] { ("Properties", "ReinforcementProperties", "", "The Reinforcement properties dialog (the top-level keys).") },
            },
            new ObjectFileSpec
            {
                Class = "ReinforcementMeshAttributeFile", Extension = ".drmsh", Table = "ReinforcementMesh", Title = "reinforcement mesh properties in drawings (.drmsh)",
                Summary = "Reinforcement mesh properties in drawings (.drmsh): the file the Reinforcement mesh properties dialog of the drawing editor saves (the Open API reads the meshes' settings from .drbr files, <see cref=\"ReinforcementAttributeFile\"/>).",
                LegacyKeyPrefix = "rebar_mesh_dial.", DialogSampleFile = "OBJECT-TEST.drmsh",
                Pages = new[] { ("Properties", "ReinforcementMeshProperties", "", "The Reinforcement mesh properties dialog (the top-level keys).") },
            },
            new ObjectFileSpec
            {
                Class = "ReferenceObjectAttributeFile", Extension = ".drmp", Table = "ReferenceObject", Title = "reference object properties in drawings (.drmp)",
                Summary = "Reference object properties in drawings (.drmp): the file the Reference object properties dialog of the drawing editor saves. It holds the keys of the Reference object page of the view properties, so <see cref=\"Properties\"/> is that page's class.",
                LegacyKeyPrefix = "refobj_dial.", DialogSampleFile = "OBJECT-TEST.drmp",
                Pages = new[] { ("Properties", "ReferenceObject", "", "The reference object settings (the top-level keys).") },
            },
            new ObjectFileSpec
            {
                Class = "GridAttributeFile", Extension = ".dgr", Table = "Grid", Title = "grid properties in drawings (.dgr)",
                Summary = "Grid properties in drawings (.dgr): the file the Grid properties dialog of the drawing editor saves. It holds the keys of the Grid page of the view properties, so <see cref=\"Properties\"/> is that page's class.",
                OpenApi = "<c>new Grid.GridAttributes(name)</c>", LegacyKeyPrefix = "grid_dial.", DialogSampleFile = "OBJECT-TEST.dgr",
                Pages = new[] { ("Properties", "GridObject", "", "The grid settings (the top-level keys).") },
            },
            new ObjectFileSpec
            {
                Class = "GridLineAttributeFile", Extension = ".grl", Table = "GridLine", Title = "grid line properties in drawings (.grl)",
                Summary = "Grid line properties in drawings (.grl): the file the Grid line properties dialog of the drawing editor saves. It holds the keys of the Grid page of the view properties, so <see cref=\"Properties\"/> is that page's class.",
                OpenApi = "<c>new GridLine.GridLineAttributes(name)</c>", LegacyKeyPrefix = "grid_line_dial.", DialogSampleFile = "OBJECT-TEST.grl",
                Pages = new[] { ("Properties", "GridObject", "", "The grid line settings (the top-level keys).") },
            },
            new ObjectFileSpec
            {
                Class = "SectionMarkAttributeFile", Extension = ".cs", Table = "SectionMark", Title = "section mark properties (.cs)",
                Summary = "Section mark properties (.cs): the file the Section mark properties dialog of the drawing editor saves; they are the keys under <c>dc.SectionMark.</c> of a drawing file.",
                OpenApi = "<c>new SectionMarkBase.SectionMarkAttributes(name)</c>", LegacyKeyPrefix = "csym_dial.", DialogSampleFile = "OBJECT-TEST.cs",
                Pages = new[] { ("Properties", "SectionMarkProperties", "", "The Section mark properties dialog (the top-level keys and the label rows <c>SectionMarkA1.*</c> ... and <c>ViewLabelMarkA1.*</c> ...).") },
            },
            new ObjectFileSpec
            {
                Class = "DetailMarkAttributeFile", Extension = ".detail", Table = "DetailMark", Title = "detail mark properties (.detail)",
                Summary = "Detail mark properties (.detail): the file the Detail mark properties dialog of the drawing editor saves.",
                OpenApi = "<c>new DetailMark.DetailMarkAttributes(name)</c>", LegacyKeyPrefix = "detail_dial.", DialogSampleFile = "OBJECT-TEST.detail",
                Pages = new[] { ("Properties", "DetailMarkProperties", "", "The Detail mark properties dialog (the top-level keys and the label rows <c>DetailMarkA1.*</c> ... and <c>ViewMarkA1.*</c> ...).") },
            },
            new ObjectFileSpec
            {
                Class = "GADimensionSetupAttributeFile", Extension = ".ga_dim_setup", Table = "GADimensionSetup", Title = "GA drawing dimension setup (.ga_dim_setup)",
                Summary = "Settings of the semi-automatic dimensioning of GA drawings (.ga_dim_setup).",
                LegacyKeyPrefix = "ga_dim_setup_dial.", DialogSampleFile = "OBJECT-TEST.ga_dim_setup",
                Pages = new[] { ("Properties", "GADimensionSetupProperties", "", "The settings dialog (the top-level keys).") },
            },
        };
    }
}
