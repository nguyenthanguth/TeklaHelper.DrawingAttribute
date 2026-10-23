using System.Collections.Generic;
using TeklaHelper.DrawingAttribute.Filters;
using TeklaHelper.DrawingAttribute.Pages;
using TeklaHelper.DrawingAttribute.Sections;

namespace TeklaHelper.DrawingAttribute.Views
{
    /// <summary>
    /// The view properties (.vi file), organised like the tree of the Tekla View properties dialog:
    /// <code>
    /// ViewProperties
    /// ├── Attributes                Attributes1, Attributes2, Label tabs (+ LabelMarkA1 ... LabelMarkA5)
    /// ├── Filter                    (+ name.vi.filter)
    /// ├── NeighborPartFilter        (+ name.vi.npfilter)
    /// ├── Protection
    /// ├── Dimensioning
    /// ├── Marks     PartMark, BoltMark, NeighborPartMark, SurfaceTreatmentMark, ConnectionMark, WeldMark,
    /// │             ReinforcementMark, NeighborReinforcementMark, PourMark
    /// └── Objects   Part, Bolt, NeighborPart, SurfaceTreatment, Weld, Reinforcement, ReferenceObject, Grid,
    ///               PourBreak, NeighborReinforcement, PourObject
    /// </code>
    /// Saving writes the files the Save button of the dialog writes: <c>name.vi</c>, <c>name.vi.filter</c> and
    /// <c>name.vi.npfilter</c> (a filter file only when the filter has rows; an empty one would make Tekla show no objects, so an existing
    /// filter file of that name is deleted instead). Those are the files
    /// the Open API reads: <c>new View.ViewAttributes(attributeFileName)</c>, <c>view.Attributes.LoadAttributes(attributeFileName)</c>.
    /// Every page has its tabs and group boxes as nested classes, like the dialog, and every key of the page in files saved
    /// from the Tekla 2025 dialogs has a typed property (those the dialog does not show are in the page's <c>Advanced</c>
    /// group); any other key is reachable by name.
    /// The files of the single pages (<c>.vf</c>, <c>.vg</c>, <c>.vpm</c>, ...) are only for loading by hand in the
    /// dialogs; write them with <see cref="AttributeSection.ExportTo"/> when needed.
    /// <code>
    /// var view = AttributeFile.FromFile&lt;ViewProperties&gt;(@"...\attributes\standard.vi");
    /// view.Attributes.Attributes1.View.Scale = 20;
    /// view.Marks.PartMark.Content.Font.Height = 3;
    /// view.Objects.Part.Content.HiddenLines.HiddenLines = true;
    /// view.Filter.SetExpression(FilterProperties.Part.Name.IsEqual("BEAM"));
    /// view.SaveTo(Path.Combine(modelPath, "attributes"), "detail_20");
    /// </code>
    /// </summary>
    public sealed class ViewProperties : AttributeFile
    {
        /// <summary>File extension.</summary>
        public const string FileExtension = ".vi";

        /// <summary>
        /// Creates empty view properties.
        /// </summary>
        public ViewProperties()
        {
            Attributes = new ViewAttributes(Values, ViewAttributes.ViewPrefix);
            Filter = new FilterSection(Values, "vfilter.", FilterFileExtensions.ViewAttributeFilter);
            NeighborPartFilter = new FilterSection(Values, "vnp_filter.", FilterFileExtensions.ViewAttributeNeighborPartFilter);
            Protection = new ProtectionPage(Values, ProtectionPage.ViewPrefix);
            Dimensioning = new ViewDimensioning(Values, ViewDimensioning.ViewPrefix);
            Marks = new MarkPages(Values, PageHost.View);
            Objects = new ObjectPages(Values, PageHost.View);
        }

        /// <summary>
        /// Creates view properties with every key the Tekla dialog saves, so that Tekla takes nothing from the
        /// <c>standard</c> file of the environment: loading a file that lacks a key gives the key the value of that
        /// standard file, which differs between environments (checked in Tekla 2025). The values are the defaults of the
        /// pages (<c>SetDefaults()</c>), the defaults of the other keys (the value most shipped files hold), and
        /// on top a setup of this library: parts exact with hidden lines, part, bolt and grid lines black,
        /// bolts, welds, surface treatments, the grid and connected neighbor parts shown, part marks with the assembly
        /// position, no bolt marks, no filter, black view labels, scale 1:20, size fitted to the parts, no shortening. The file
        /// uses code page 1252: set <see cref="AttributeFile.CodePage"/> before saving Japanese or Korean text. Change what
        /// you need and save:
        /// <code>
        /// var view = ViewProperties.CreateDefault();
        /// view.Attributes.Attributes1.View.Scale = 50;
        /// view.SaveTo(attributes, "plan_50");
        /// </code>
        /// </summary>
        public static ViewProperties CreateDefault()
        {
            var view = new ViewProperties();
            FileDefaults.Apply(view.Values, FileDefaults.View.Table);
            view.Attributes.SetDefaults();
            foreach (ViewLabelMark labelMark in new[] { view.Attributes.LabelMarkA1, view.Attributes.LabelMarkA2, view.Attributes.LabelMarkA3, view.Attributes.LabelMarkA4, view.Attributes.LabelMarkA5 })
            {
                labelMark.SetDefaults();
            }

            view.Protection.SetDefaults();
            view.Dimensioning.SetDefaults();
            view.Marks.SetDefaults();
            view.Objects.SetDefaults();

            DefaultSetup.Apply(view);
            return view;
        }

        /// <inheritdoc />
        public override string Extension => FileExtension;

        /// <summary>The "Attributes" page: scale, shortening, size, depth, label (the top-level keys).</summary>
        public ViewAttributes Attributes { get; }

        /// <summary>Filter: the <c>vfilter.*</c> settings and the rows in name.vi.filter.</summary>
        public FilterSection Filter { get; }

        /// <summary>Neighbor part filter: the <c>vnp_filter.*</c> settings and the rows in name.vi.npfilter.</summary>
        public FilterSection NeighborPartFilter { get; }

        /// <summary>The "Protection" page (<c>Protection.*</c>).</summary>
        public ProtectionPage Protection { get; }

        /// <summary>The "Dimensioning" page (<c>ViewDimensioning.*</c>).</summary>
        public ViewDimensioning Dimensioning { get; }

        /// <summary>The "Marks" pages: part, bolt, neighbor part, surface treatment, connection, weld, reinforcement, neighbor reinforcement and pour marks.</summary>
        public MarkPages Marks { get; }

        /// <summary>The "Objects" pages: part, bolt, neighbor part, surface treatment, weld, reinforcement, reference object, grid, pour break and pour objects.</summary>
        public ObjectPages Objects { get; }

        /// <inheritdoc />
        protected override string LegacyKeyPrefix => "view_dial.";

        /// <inheritdoc />
        protected override IEnumerable<IAttributeFileCompanion> CompanionFiles
        {
            get
            {
                yield return Filter.File;
                yield return NeighborPartFilter.File;
            }
        }
    }
}
