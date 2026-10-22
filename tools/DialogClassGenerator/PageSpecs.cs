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
    }

    internal static class PageSpecs
    {
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
        };
    }
}
