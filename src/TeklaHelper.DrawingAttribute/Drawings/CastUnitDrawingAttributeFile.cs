using TeklaHelper.DrawingAttribute.Pages;

namespace TeklaHelper.DrawingAttribute.Drawings
{
    /// <summary>
    /// Settings of a cast unit drawing (.cud), with its user-defined attributes (.cud.more). Use the saved file with
    /// <c>new CastUnitDrawing(castUnitIdentifier, attributeFileName)</c>.
    /// <para>
    /// New .more files use the object type <c>cudraw_dial_attributes</c>, the one the Default environment of Tekla 2025
    /// uses; some environments use <c>castunitdrawing_attributes</c> instead, and loading such a file keeps it.
    /// </para>
    /// </summary>
    public sealed class CastUnitDrawingAttributeFile : DrawingAttributeFile
    {
        /// <summary>File extension.</summary>
        public const string FileExtension = ".cud";

        /// <summary>
        /// Creates an empty file.
        /// </summary>
        public CastUnitDrawingAttributeFile()
            : base(PageHost.AssemblyDrawing, ".cud.more", "cudraw_dial_attributes")
        {
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
        public static CastUnitDrawingAttributeFile CreateDefault() => CreateDefault<CastUnitDrawingAttributeFile>(FileDefaults.CastUnitDrawing.Table);

        /// <inheritdoc />
        public override string Extension => FileExtension;

        /// <summary>Sheet number (<c>sheet_number</c>).</summary>
        public int SheetNumber
        {
            get => GetInt32("sheet_number");
            set => SetInt32("sheet_number", value);
        }

        /// <inheritdoc />
        protected override string LegacyKeyPrefix => "cudraw_dial.";
    }
}
