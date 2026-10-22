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
