namespace DialogClassGenerator
{
    /// <summary>
    /// A page of the View properties dialog and where its keys live in each kind of file.
    /// </summary>
    internal sealed class PageSpec
    {
        public string Class { get; set; }

        public string Json { get; set; }

        public string Namespace { get; set; } = "TeklaHelper.DrawingAttribute.Pages";

        public string Title { get; set; }

        public string ViewPrefix { get; set; }

        public string GADrawingPrefix { get; set; }

        public string AssemblyDrawingPrefix { get; set; }

        public string ViewFile { get; set; }

        public string GADrawingFile { get; set; }

        public string AssemblyDrawingFile { get; set; }

        /// <summary>For the view attributes page: only the keys without a dot belong to it.</summary>
        public bool TopLevelOnly { get; set; }

        /// <summary>Key of the loaded page file name (<c>in_file</c>, <c>aInFile</c>, ...), from the decoded dialog.</summary>
        public string FileInKey { get; set; } = "in_file";

        /// <summary>Key of the "Save as" name (<c>out_file</c>, <c>aOutFile</c>, ...), from the decoded dialog.</summary>
        public string FileOutKey { get; set; } = "out_file";

        /// <summary>
        /// For a page of the drawing properties only: the first sentence of the class summary (where the dialog shows it).
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The class is also written by hand (a class released before the page was generated): the generated part has no
        /// constructor, and skips the keys and member names of the hand-written part.
        /// </summary>
        public bool HandWrittenPart { get; set; }

        /// <summary>Keys (relative to the prefix) that properties of the hand-written part read and write.</summary>
        public string[] HandWrittenKeys { get; set; } = new string[0];

        /// <summary>Member names of the hand-written part, which generated properties must not take.</summary>
        public string[] HandWrittenMembers { get; set; } = new string[0];

        /// <summary>
        /// Properties of the hand-written part that <c>SetDefaults()</c> assigns too: (property, key, C# type). Keys that
        /// name a resource of the environment (the View creation table, the 3D view file) are left out.
        /// </summary>
        public (string Property, string Key, string Type)[] HandWrittenDefaults { get; set; } = new (string, string, string)[0];

        /// <summary>
        /// Rows of label texts A1 to A5, each a <c>ViewLabelMark</c> under <c>stem + row + "."</c>: (property name without the
        /// row number, key stem relative to the prefix, summary of the row).
        /// </summary>
        public (string Property, string KeyStem, string Summary)[] LabelMarks { get; set; } = new (string, string, string)[0];

        /// <summary>A page of the drawing properties dialogs, not of the View properties dialog.</summary>
        public bool DrawingPage => ViewPrefix == null && FileExtension == null;

        /// <summary>For a page of a drawing object file (the dimension properties, .dim): the extension of that file.</summary>
        public string FileExtension { get; set; }

        /// <summary>The prefixes of the page in its object file: <c>""</c> for the top-level keys, one per mark otherwise.</summary>
        public string[] FilePrefixes { get; set; }

        /// <summary>Sample of the object file, relative to the folder of the environments shipped with Tekla.</summary>
        public string SampleFile { get; set; }

        /// <summary>
        /// Sample of the object file saved from its dialog (Save as), relative to the folder of the sample .vi: used instead
        /// of <see cref="SampleFile"/> by the pages generated after the dimension properties.
        /// </summary>
        public string DialogSampleFile { get; set; }

        /// <summary>Extension of the file the page's own Save button writes (only for the dialogs).</summary>
        public string PageFileExtension { get; set; }

        /// <summary>A page of a drawing object file.</summary>
        public bool ObjectFilePage => FileExtension != null;

        /// <summary>The prefix the samples of the page use: the first of <see cref="FilePrefixes"/>.</summary>
        public string FileSamplePrefix => FilePrefixes?[0];
    }

    internal static class PageSpecs
    {
        private const string ViewCreation = "global::TeklaHelper.DrawingAttribute.Drawings.ViewCreation";

        // Drawing extensions were matched by key set against the files shipped with Tekla (see SectionFileExtensions)
        public static readonly PageSpec[] All =
        {
            new PageSpec { Class = "ViewAttributes", Json = "Attributes", Namespace = "TeklaHelper.DrawingAttribute.Views", Title = "Attributes", ViewPrefix = "", TopLevelOnly = true },
            new PageSpec { Class = "ViewDimensioning", Json = "ViewDimensioning", Namespace = "TeklaHelper.DrawingAttribute.Views", Title = "Dimensioning", ViewPrefix = "ViewDimensioning.", ViewFile = ".viewdim" },
            new PageSpec { Class = "ViewLabelMark", Json = "ViewLabelMark", Title = "View label mark (the \"...\" dialog of the label texts A1 to A5)", ViewPrefix = "ViewLabelMarkA1." },
            new PageSpec { Class = "ProtectionPage", Json = "Protection", Title = "Protection", ViewPrefix = "Protection.", GADrawingPrefix = "dpro.", AssemblyDrawingPrefix = "dpro.", GADrawingFile = ".gdrp", AssemblyDrawingFile = ".adr" },

            new PageSpec { Class = "PartMark", Json = "PartMark", Title = "Part mark", ViewPrefix = "vpm.", GADrawingPrefix = "dpm.", AssemblyDrawingPrefix = "dpm.", ViewFile = ".vpm", GADrawingFile = ".gpm", AssemblyDrawingFile = ".apm" },
            new PageSpec { Class = "BoltMark", Json = "BoltMark", Title = "Bolt mark", ViewPrefix = "vsm.", GADrawingPrefix = "dsm.", AssemblyDrawingPrefix = "dsm.", ViewFile = ".vsm", GADrawingFile = ".gsm", AssemblyDrawingFile = ".asm" },
            new PageSpec { Class = "NeighborPartMark", Json = "NeighborPartMark", Title = "Neighbor part mark", ViewPrefix = "vnpm.", GADrawingPrefix = "dnpm.", AssemblyDrawingPrefix = "dnpm.", ViewFile = ".vnpm", GADrawingFile = ".gnpm" },
            new PageSpec { Class = "SurfaceTreatmentMark", Json = "SurfaceTreatmentMark", Title = "Surface treatment mark", ViewPrefix = "vsurfm.", GADrawingPrefix = "dsurf_mark.", AssemblyDrawingPrefix = "dsurfmark.", ViewFile = ".vsurfm", GADrawingFile = ".gdsurfm" },
            new PageSpec { Class = "ConnectionMark", Json = "ConnectionMark", Title = "Connection mark", ViewPrefix = "vjm.", GADrawingPrefix = "djm.", AssemblyDrawingPrefix = "djm.", ViewFile = ".vjm", GADrawingFile = ".gjm" },
            new PageSpec { Class = "WeldMark", Json = "WeldMark", Title = "Weld mark", ViewPrefix = "vwel.", GADrawingPrefix = "dwel.", AssemblyDrawingPrefix = "dwel.", ViewFile = ".vw", GADrawingFile = ".gdw" },
            new PageSpec { Class = "ReinforcementMark", Json = "ReinforcementMark", Title = "Reinforcement mark", ViewPrefix = "vrm.", GADrawingPrefix = "drebar_mark.", AssemblyDrawingPrefix = "drmark.", ViewFile = ".vrm", GADrawingFile = ".gdrm" },
            new PageSpec { Class = "NeighborReinforcementMark", Json = "NeighborReinforcementMark", Title = "Neighbor reinforcement mark", ViewPrefix = "NeighborRebarMark.", GADrawingPrefix = "NeighborRebarMark.", AssemblyDrawingPrefix = "NeighborRebarMark.", ViewFile = ".vnrm", GADrawingFile = ".gnrm" },
            new PageSpec { Class = "PourMark", Json = "PourMark", Title = "Pour mark", ViewPrefix = "PourMark.", GADrawingPrefix = "PourMark.", AssemblyDrawingPrefix = "PourMark.", ViewFile = ".vpom", GADrawingFile = ".gpom" },

            new PageSpec { Class = "PartObject", Json = "PartObject", Title = "Part", ViewPrefix = "vp.", GADrawingPrefix = "dp.", AssemblyDrawingPrefix = "dp.", ViewFile = ".vp", GADrawingFile = ".gdp" },
            new PageSpec { Class = "BoltObject", Json = "BoltObject", Title = "Bolt", ViewPrefix = "vs.", GADrawingPrefix = "ds.", AssemblyDrawingPrefix = "ds.", ViewFile = ".vs", GADrawingFile = ".gds" },
            new PageSpec { Class = "NeighborPartObject", Json = "NeighborPartObject", Title = "Neighbor part", ViewPrefix = "vnp.", GADrawingPrefix = "dnpart.", AssemblyDrawingPrefix = "dnp.", ViewFile = ".vnp", GADrawingFile = ".gdnp", AssemblyDrawingFile = ".adnp" },
            new PageSpec { Class = "SurfaceTreatmentObject", Json = "SurfaceTreatmentObject", Title = "Surface treatment", ViewPrefix = "vsurf.", GADrawingPrefix = "dsurf.", AssemblyDrawingPrefix = "dsurf.", ViewFile = ".vsurf", GADrawingFile = ".gdsrf" },
            new PageSpec { Class = "WeldObject", Json = "WeldObject", Title = "Weld", ViewPrefix = "ViewWeld.", GADrawingPrefix = "DrawingWeld.", AssemblyDrawingPrefix = "DrawingWeld.", ViewFile = ".vwo", GADrawingFile = ".gdwo", AssemblyDrawingFile = ".adwo" },
            new PageSpec { Class = "ReinforcementObject", Json = "ReinforcementObject", Title = "Reinforcement", ViewPrefix = "vrebar.", GADrawingPrefix = "drebar.", AssemblyDrawingPrefix = "drebar.", ViewFile = ".vr", GADrawingFile = ".gdr" },
            new PageSpec { Class = "ReferenceObject", Json = "ReferenceObject", Title = "Reference object", ViewPrefix = "vref.", GADrawingPrefix = "drefobj.", ViewFile = ".vrmp", GADrawingFile = ".gdrmp" },
            new PageSpec { Class = "GridObject", Json = "GridObject", Title = "Grid", ViewPrefix = "vg.", GADrawingPrefix = "dgrid.", AssemblyDrawingPrefix = "grid.", ViewFile = ".vg", GADrawingFile = ".gdgr", AssemblyDrawingFile = ".adgr" },
            new PageSpec { Class = "PourBreakObject", Json = "PourBreakObject", Title = "Pour break", ViewPrefix = "PourBreaks.", GADrawingPrefix = "PourBreaks.", AssemblyDrawingPrefix = "PourBreaks.", ViewFile = ".vpbr", GADrawingFile = ".gpbr" },
            new PageSpec { Class = "NeighborReinforcementObject", Json = "NeighborReinforcementObject", Title = "Neighbor reinforcement", ViewPrefix = "NeighborRebar.", GADrawingPrefix = "NeighborRebar.", AssemblyDrawingPrefix = "NeighborRebar.", ViewFile = ".vnr", GADrawingFile = ".gnr" },
            new PageSpec { Class = "PourObject", Json = "PourObject", Title = "Pour object", ViewPrefix = "PourObjects.", GADrawingPrefix = "PourObjects.", AssemblyDrawingPrefix = "PourObjects.", ViewFile = ".vpo", GADrawingFile = ".gpo" },

            // Pages of the drawing properties only (DRAWING_PAGES of tools/DialogDecoder/extract.py); after ViewLabelMark,
            // whose defaults their label rows take
            new PageSpec
            {
                Class = "DrawingViewCreationSection", Json = "DrawingView", Namespace = "TeklaHelper.DrawingAttribute.Drawings", Title = "View",
                Description = "The \"View...\" page of the GA drawing properties (the view settings of the assembly, single-part and cast unit drawing properties): view size, shortening, labels, anchor bolt plans, and the views the drawing creates",
                GADrawingPrefix = "dview.", AssemblyDrawingPrefix = "dv.", GADrawingFile = ".gdv", AssemblyDrawingFile = ".adv",
                HandWrittenPart = true,
                HandWrittenKeys = new[]
                {
                    "main_scale", "cre_front_view", "cre_top_view", "cre_back_view", "cre_bottom_view", "cre_cuts", "cre_end_views", "Create3dView",
                    "aDrawingViews", "aCreate3dViewAttrib", "ViewShorteningOn", "cut_length", "cut_min_dist", "label_height", "label_position", "label_colour",
                },
                HandWrittenMembers = new[]
                {
                    "GAPrefix", "AssemblyPrefix", "Scale", "CreateFrontView", "CreateTopView", "CreateBackView", "CreateBottomView", "CreateSectionViews",
                    "CreateEndViews", "Create3dView", "GetViewTable", "SetViewTable", "SetViewAttributeFile", "SyncSwitchesWithViewTable", "SwitchKeys",
                    "GetCreation", "SetCreation", "TryGetViewTable", "View3dAttributeFile", "ShorteningOn", "ShorteningCutLength",
                    "ShorteningMinimumDistance", "LabelHeight", "LabelPosition", "LabelColor",
                },
                HandWrittenDefaults = new[]
                {
                    ("Scale", "main_scale", "double"),
                    ("CreateFrontView", "cre_front_view", ViewCreation), ("CreateTopView", "cre_top_view", ViewCreation),
                    ("CreateBackView", "cre_back_view", ViewCreation), ("CreateBottomView", "cre_bottom_view", ViewCreation),
                    ("CreateSectionViews", "cre_cuts", ViewCreation), ("CreateEndViews", "cre_end_views", ViewCreation),
                    ("Create3dView", "Create3dView", "bool"), ("ShorteningOn", "ViewShorteningOn", "bool"),
                    ("ShorteningCutLength", "cut_length", "double"), ("ShorteningMinimumDistance", "cut_min_dist", "double"),
                    ("LabelHeight", "label_height", "double"), ("LabelPosition", "label_position", "int"),
                    ("LabelColor", "label_colour", "global::TeklaHelper.DrawingAttribute.DrawingColor"),
                },
                LabelMarks = new[] { ("LabelMarkA", "ViewLabelMarkA", "view label") },
            },
            new PageSpec
            {
                Class = "SectionViewPage", Json = "SectionView", Namespace = "TeklaHelper.DrawingAttribute.Drawings", Title = "Section view",
                Description = "The section view settings of the drawing properties (the \"Section view\" panel of the assembly drawing properties of Tekla 2026; the GA drawing properties show only the start number): scale and direction of the section views, the cutting line and the section mark",
                GADrawingPrefix = "dc.", AssemblyDrawingPrefix = "dc.", AssemblyDrawingFile = ".adc",
                LabelMarks = new[] { ("SectionMarkA", "SectionMark.SectionMarkA", "section mark text"), ("LabelMarkA", "SectionMark.ViewLabelMarkA", "section view label") },
            },
            new PageSpec
            {
                Class = "DetailViewPage", Json = "DetailView", Namespace = "TeklaHelper.DrawingAttribute.Drawings", Title = "Detail view",
                Description = "The \"Detail view...\" page of the drawing properties: the detail view label, the detail boundary and the detail mark",
                GADrawingPrefix = "DetailLabel.", AssemblyDrawingPrefix = "DetailLabel.", GADrawingFile = ".detail_label", AssemblyDrawingFile = ".detail_label",
                LabelMarks = new[] { ("DetailMarkA", "DetailMarkA", "detail mark text"), ("LabelMarkA", "ViewLabelMarkA", "detail view label") },
            },
            new PageSpec
            {
                Class = "DimensionPage", Json = "Dimensioning", Namespace = "TeklaHelper.DrawingAttribute.Drawings", Title = "Dimension",
                Description = "The \"Dimension...\" page of the drawing properties: dimension types, units, precision, placing and appearance",
                GADrawingPrefix = "dd.", AssemblyDrawingPrefix = "dd.", GADrawingFile = ".gdd",
            },
            new PageSpec
            {
                Class = "DimensioningPage", Json = "DimensioningRules", Namespace = "TeklaHelper.DrawingAttribute.Drawings", Title = "Dimensioning",
                Description = "The \"Dimensioning...\" page of the GA drawing properties (GA and multi drawings only): grid line dimensions and the dimensioning of parts",
                GADrawingPrefix = "Dimensioning.",
            },
            new PageSpec
            {
                Class = "AssemblyDimensioningPage", Json = "AssemblyDimensioning", Namespace = "TeklaHelper.DrawingAttribute.Drawings", Title = "Dimensioning",
                Description = "The \"Dimensioning...\" page of the assembly, single-part and cast unit drawing properties (not in GA drawings): the automatic dimensioning of parts, bolts, positions and sub-assemblies",
                AssemblyDrawingPrefix = "dcd.",
            },

            // Drawing object files (FILE_PAGES of tools/DialogDecoder/extract.py)
            new PageSpec
            {
                Class = "DimensionProperties", Json = "Dimension", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "Dimension properties",
                Description = "The Dimension properties dialog of the drawing editor (dialog <c>gr_dim_attrib</c>), the top-level keys of a .dim file: dimension types, format, placing, appearance, marks and tags",
                FileExtension = ".dim", FilePrefixes = new[] { string.Empty }, SampleFile = "common/system/standard.dim", TopLevelOnly = true,
            },
            new PageSpec
            {
                Class = "DimensionMark", Json = "DimensionMark", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "Dimension mark",
                Description = "A mark of a dimension: the dialog of the \"...\" buttons on the Marks and Tags tabs of the Dimension properties (dialog <c>gr_dim_mark_attrib</c>): content, frame, font, units and rotation; the other keys are those of every mark, which this dialog does not show",
                FileExtension = ".dim", SampleFile = "common/system/standard.dim", PageFileExtension = ".dimension_mark",
                FilePrefixes = new[]
                {
                    "PrefixMark.", "PostfixMark.", "LeftUpperTagMark.", "LeftMiddleTagMark.", "LeftLowerTagMark.", "MiddleLowerTagMark.",
                    "RightUpperTagMark.", "RightMiddleTagMark.", "RightLowerTagMark.",
                },
            },

            // The other drawing object files with a dialog of their own; the marks, the grid, bolts, welds, pour breaks and
            // reference objects store the keys of a view page and use its class (see ObjectFileSpecs)
            new PageSpec
            {
                Class = "TextProperties", Json = "TextProperties", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "Text properties",
                Description = "The Text properties dialog of the drawing editor (dialog <c>gr_text_attrib</c>): text, frame, leader line arrow and placing",
                FileExtension = ".drtxt", FilePrefixes = new[] { string.Empty }, DialogSampleFile = "OBJECT-TEST.drtxt",
            },
            new PageSpec
            {
                Class = "SymbolProperties", Json = "SymbolProperties", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "Symbol properties",
                Description = "The Symbol properties dialog of the drawing editor (dialog <c>gr_symbol_attrib</c>): symbol, frame and placing",
                FileExtension = ".sbl", FilePrefixes = new[] { string.Empty }, DialogSampleFile = "OBJECT-TEST.sbl",
            },
            new PageSpec
            {
                Class = "LevelMarkProperties", Json = "LevelMarkProperties", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "Level mark properties",
                Description = "The Level mark properties dialog of the drawing editor (dialog <c>gr_level_attrib</c>): content and format of the level, text, frame, arrow and placing",
                FileExtension = ".lev", FilePrefixes = new[] { string.Empty }, DialogSampleFile = "OBJECT-TEST.lev",
            },
            new PageSpec
            {
                Class = "RevisionMarkProperties", Json = "RevisionMarkProperties", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "Revision mark properties",
                Description = "The Revision mark properties dialog of the drawing editor (dialog <c>gr_revision_attrib</c>): revision texts, text, arrow and frame",
                FileExtension = ".rev", FilePrefixes = new[] { string.Empty }, DialogSampleFile = "OBJECT-TEST.rev",
            },
            new PageSpec
            {
                Class = "AssociativeNoteProperties", Json = "AssociativeNoteProperties", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "Associative note properties",
                Description = "The Associative note properties dialog of the drawing editor (dialog <c>gr_note_attrib</c>): content, frames, font, units, leader line and placing; the other keys are those of every mark, which the View properties show on the mark pages",
                FileExtension = ".note", FilePrefixes = new[] { string.Empty }, DialogSampleFile = "OBJECT-TEST.note",
            },
            new PageSpec
            {
                Class = "LineProperties", Json = "LineProperties", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "Line properties",
                Description = "The Line properties dialog of the drawing editor (dialog <c>gr_go_line_attrib</c>): line and arrow",
                FileExtension = ".gln", FilePrefixes = new[] { string.Empty }, DialogSampleFile = "OBJECT-TEST.gln",
            },
            new PageSpec
            {
                Class = "ArcProperties", Json = "ArcProperties", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "Arc properties",
                Description = "The Arc properties dialog of the drawing editor (dialog <c>gr_go_arc_attrib</c>): line and arrow",
                FileExtension = ".gar", FilePrefixes = new[] { string.Empty }, DialogSampleFile = "OBJECT-TEST.gar",
            },
            new PageSpec
            {
                Class = "PolylineProperties", Json = "PolylineProperties", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "Polyline properties",
                Description = "The Polyline properties dialog of the drawing editor (dialog <c>gr_go_polyline_attrib</c>): line and arrow",
                FileExtension = ".gpl", FilePrefixes = new[] { string.Empty }, DialogSampleFile = "OBJECT-TEST.gpl",
            },
            new PageSpec
            {
                Class = "CircleProperties", Json = "CircleProperties", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "Circle properties",
                Description = "The Circle properties dialog of the drawing editor (dialog <c>gr_go_circle_attrib</c>): line and fill",
                FileExtension = ".gci", FilePrefixes = new[] { string.Empty }, DialogSampleFile = "OBJECT-TEST.gci",
            },
            new PageSpec
            {
                Class = "RectangleProperties", Json = "RectangleProperties", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "Rectangle properties",
                Description = "The Rectangle properties dialog of the drawing editor (dialog <c>gr_go_rectangle_attrib</c>): line and fill",
                FileExtension = ".grt", FilePrefixes = new[] { string.Empty }, DialogSampleFile = "OBJECT-TEST.grt",
            },
            new PageSpec
            {
                Class = "PolygonProperties", Json = "PolygonProperties", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "Polygon properties",
                Description = "The Polygon properties dialog of the drawing editor (dialog <c>gr_go_polygon_attrib</c>), which clouds use too: line and fill",
                FileExtension = ".gpg", FilePrefixes = new[] { string.Empty }, DialogSampleFile = "OBJECT-TEST.gpg",
            },
            new PageSpec
            {
                Class = "TextFileProperties", Json = "TextFileProperties", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "Text file properties",
                Description = "The Text file properties dialog of the drawing editor (dialog <c>gr_fo_asc_attrib</c>): text style, frame, scaling and file",
                FileExtension = ".fas", FilePrefixes = new[] { string.Empty }, DialogSampleFile = "OBJECT-TEST.fas",
            },
            new PageSpec
            {
                Class = "DwgProperties", Json = "DwgProperties", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "DWG/DXF properties",
                Description = "The DWG/DXF properties dialog of the drawing editor (dialog <c>gr_fo_dwg_attrib</c>): scaling, frame and file",
                FileExtension = ".fdg", FilePrefixes = new[] { string.Empty }, DialogSampleFile = "OBJECT-TEST.fdg",
            },
            new PageSpec
            {
                Class = "ImageProperties", Json = "ImageProperties", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "Image properties",
                Description = "The Image properties dialog of the drawing editor (dialog <c>gr_fo_image_attrib</c>): scaling, frame and file",
                FileExtension = ".idg", FilePrefixes = new[] { string.Empty }, DialogSampleFile = "OBJECT-TEST.idg",
            },
            new PageSpec
            {
                Class = "HyperlinkProperties", Json = "HyperlinkProperties", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "Hyperlink properties",
                Description = "The Hyperlink properties dialog of the drawing editor (dialog <c>gr_fo_link_attrib</c>): text style, frame, scaling and link",
                FileExtension = ".fhl", FilePrefixes = new[] { string.Empty }, DialogSampleFile = "OBJECT-TEST.fhl",
            },
            new PageSpec
            {
                Class = "LinkedDrawingProperties", Json = "LinkedDrawingProperties", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "Linked drawing properties",
                Description = "The Linked drawing properties dialog of the drawing editor (dialog <c>gr_link_drawing_attrib</c>): text style, frame, scaling and link",
                FileExtension = ".ldr", FilePrefixes = new[] { string.Empty }, DialogSampleFile = "OBJECT-TEST.ldr",
            },
            new PageSpec
            {
                Class = "PartProperties", Json = "PartProperties", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "Part properties",
                Description = "The Part properties dialog of the drawing editor (dialog <c>gr_part_attrib</c>): representation, lines, additional marks and fill of a part in a drawing",
                FileExtension = ".dprt", FilePrefixes = new[] { string.Empty }, DialogSampleFile = "OBJECT-TEST.dprt",
            },
            new PageSpec
            {
                Class = "SurfaceTreatmentProperties", Json = "SurfaceTreatmentProperties", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "Surface treatment properties",
                Description = "The Surface treatment properties dialog of the drawing editor (dialog <c>gr_surfacing_attrib</c>): visible and hidden lines; the other keys are those of the part properties, whose structure the file shares",
                FileExtension = ".dsrf", FilePrefixes = new[] { string.Empty }, DialogSampleFile = "OBJECT-TEST.dsrf",
            },
            new PageSpec
            {
                Class = "PourProperties", Json = "PourProperties", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "Pour object properties",
                Description = "The Pour object properties dialog of the drawing editor (dialog <c>grPourAttrib</c>): lines, additional marks and fill",
                FileExtension = ".po", FilePrefixes = new[] { string.Empty }, DialogSampleFile = "OBJECT-TEST.po",
            },
            new PageSpec
            {
                Class = "EdgeChamferProperties", Json = "EdgeChamferProperties", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "Edge chamfer properties",
                Description = "The Edge chamfer properties dialog of the drawing editor (dialog <c>grChamferAttrib</c>): the visible lines",
                FileExtension = ".dcha", FilePrefixes = new[] { string.Empty }, DialogSampleFile = "OBJECT-TEST.dcha",
            },
            new PageSpec
            {
                Class = "ReinforcementProperties", Json = "ReinforcementProperties", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "Reinforcement properties",
                Description = "The Reinforcement properties dialog of the drawing editor (dialog <c>gr_rebar_attrib</c>): representation, lines, symbols and line appearance",
                FileExtension = ".drbr", FilePrefixes = new[] { string.Empty }, DialogSampleFile = "OBJECT-TEST.drbr",
            },
            new PageSpec
            {
                Class = "ReinforcementMeshProperties", Json = "ReinforcementMeshProperties", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "Reinforcement mesh properties",
                Description = "The Reinforcement mesh properties dialog of the drawing editor (dialog <c>gr_rebar_mesh_attrib</c>): representation, lines, symbols and line appearance",
                FileExtension = ".drmsh", FilePrefixes = new[] { string.Empty }, DialogSampleFile = "OBJECT-TEST.drmsh",
            },
            new PageSpec
            {
                Class = "DrawingWeldMarkProperties", Json = "DrawingWeldMarkProperties", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "Weld mark properties",
                Description = "The Weld mark properties dialog of a weld mark drawn in the drawing editor (dialog <c>gr_wld_attrib</c>; the weld marks of model welds are in .wel files): weld symbols above and below the line, text and placing",
                FileExtension = ".wls", FilePrefixes = new[] { string.Empty }, DialogSampleFile = "OBJECT-TEST.wls",
            },
            new PageSpec
            {
                Class = "SectionMarkProperties", Json = "SectionMarkProperties", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "Section mark properties",
                Description = "The Section mark properties dialog of the drawing editor (dialog <c>gr_csym_attrib</c>): cutting line, section mark and view label",
                FileExtension = ".cs", FilePrefixes = new[] { string.Empty }, DialogSampleFile = "OBJECT-TEST.cs",
                LabelMarks = new[] { ("SectionMarkA", "SectionMarkA", "section mark text"), ("LabelMarkA", "ViewLabelMarkA", "section view label") },
            },
            new PageSpec
            {
                Class = "DetailMarkProperties", Json = "DetailMarkProperties", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "Detail mark properties",
                Description = "The Detail mark properties dialog of the drawing editor (dialog <c>gr_detail_symbol_attrib</c>): view label, detail boundary and detail mark",
                FileExtension = ".detail", FilePrefixes = new[] { string.Empty }, DialogSampleFile = "OBJECT-TEST.detail",
                LabelMarks = new[] { ("DetailMarkA", "DetailMarkA", "detail mark text"), ("LabelMarkA", "ViewMarkA", "detail view label") },
            },
            new PageSpec
            {
                Class = "GADimensionSetupProperties", Json = "GADimensionSetupProperties", Namespace = "TeklaHelper.DrawingAttribute.DrawingObjects", Title = "GA drawing dimension setup",
                Description = "The settings dialog of the semi-automatic dimensioning of GA drawings (dialog <c>dim_ga_auto_setup</c>)",
                FileExtension = ".ga_dim_setup", FilePrefixes = new[] { string.Empty }, DialogSampleFile = "OBJECT-TEST.ga_dim_setup",
            },
        };
    }
}
