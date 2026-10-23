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
            new PageSpec { Class = "ViewDimensioning", Json = "ViewDimensioning", Namespace = "TeklaHelper.DrawingAttribute.Views", Title = "Dimensioning", ViewPrefix = "ViewDimensioning." },
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
                GADrawingPrefix = "DetailLabel.", AssemblyDrawingPrefix = "DetailLabel.",
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
        };
    }
}
