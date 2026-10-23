using System.Collections.Generic;
using TeklaHelper.DrawingAttribute.Filters;
using TeklaHelper.DrawingAttribute.Pages;
using TeklaHelper.DrawingAttribute.Sections;

namespace TeklaHelper.DrawingAttribute.Drawings
{
    /// <summary>
    /// Settings of a general arrangement drawing (.gd), with its user-defined attributes (.gd.more) and filters
    /// (.gd.filter, .gd.npfilter): the four files the Open API reads for a GA drawing. The files of single pages
    /// (<c>.gdf</c>, <c>.gdgr</c>, ...) are only for loading by hand in the dialogs.
    /// <para>
    /// Use the saved file with <c>new GADrawing(name, attributeFileName)</c>, or load it into an existing drawing from the
    /// drawing properties dialog.
    /// </para>
    /// <code>
    /// var settings = AttributeFile.FromFile&lt;GADrawingAttributeFile&gt;(@"...\attributes\standard.gd");
    /// settings.Title1 = "PLAN +3.600";
    /// settings.Views.Scale = 100;
    /// settings.Objects.Grid.Content.Text.Height = 3.5;
    /// settings.UserAttributes.DrawnBy = "NT";
    /// settings.SaveTo(Path.Combine(modelPath, "attributes"), "plan_100");
    ///
    /// var drawing = new GADrawing("PLAN +3.600", "plan_100");
    /// </code>
    /// </summary>
    public sealed class GADrawingAttributeFile : DrawingAttributeFile
    {
        /// <summary>File extension.</summary>
        public const string FileExtension = ".gd";

        /// <summary>
        /// Creates an empty file.
        /// </summary>
        public GADrawingAttributeFile()
            : base(PageHost.GADrawing, ".gd.more", "gadrawing_attributes")
        {
            Filter = new FilterSection(Values, "dfilter.", FilterFileExtensions.GADrawingAttributeFilter);
            NeighborPartFilter = new FilterSection(Values, "dnp_filter.", FilterFileExtensions.GADrawingAttributeNeighborPartFilter);
        }

        /// <summary>
        /// Creates a file with every key the Tekla dialog saves. A key a loaded file lacks takes the value of the
        /// <c>standard</c> file of the environment, so an incomplete file draws differently from one environment to the next.
        /// Page keys take the defaults of their page (<c>SetDefaults()</c>), the other keys the value most files shipped with
        /// Tekla hold; layouts and attribute files use names every environment has, titles are empty. On top, the setup of
        /// this library: parts exact with hidden lines and dash-dot center lines of main beams, part, bolt and grid lines
        /// black, bolts, welds, surface treatments, the grid and connected neighbor parts shown, part marks with the assembly
        /// position, no bolt marks, black view labels. The View creation table names no view properties file, so the views
        /// Tekla creates use these settings, except a 3D view, which then uses the environment's standard view properties:
        /// save a <c>ViewProperties.CreateDefault()</c> and name it with <c>Views.SetViewAttributeFile</c> to control every
        /// view. The view switches follow the table, and the .more file holds the empty fields of a file saved from the
        /// dialog. The file uses code page 1252: set
        /// <see cref="AttributeFile.CodePage"/> (932, 949, ...) before saving Japanese or Korean text.
        /// </summary>
        public static GADrawingAttributeFile CreateDefault() => CreateDefault<GADrawingAttributeFile>(FileDefaults.GADrawing.Table);

        /// <inheritdoc />
        public override string Extension => FileExtension;

        /// <summary>Filter: the <c>dfilter.*</c> settings and the rows in name.gd.filter.</summary>
        public FilterSection Filter { get; }

        /// <summary>Neighbor part filter: the <c>dnp_filter.*</c> settings and the rows in name.gd.npfilter.</summary>
        public FilterSection NeighborPartFilter { get; }

        /// <inheritdoc />
        protected override string LegacyKeyPrefix => "gdraw_dial.";

        private protected override IEnumerable<IAttributeFileCompanion> FilterCompanions
        {
            get
            {
                yield return Filter.File;
                yield return NeighborPartFilter.File;
            }
        }
    }
}
