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
