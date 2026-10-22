using System.Collections.Generic;
using TeklaHelper.DrawingAttribute.Filters;
using TeklaHelper.DrawingAttribute.Pages;

namespace TeklaHelper.DrawingAttribute.Drawings
{
    /// <summary>
    /// Settings of an assembly drawing (.ad), with its user-defined attributes (.ad.more) and filter (.ad.filter). Use
    /// the saved file with <c>new AssemblyDrawing(assemblyIdentifier, attributeFileName)</c>.
    /// <para>
    /// New .more files use the object type <c>adraw_dial_attributes</c>, the most common one in the environments shipped
    /// with Tekla 2025; some of them use <c>assemblydrawing_attributes</c> instead, and loading such a file keeps it.
    /// </para>
    /// </summary>
    public sealed class AssemblyDrawingAttributeFile : DrawingAttributeFile
    {
        /// <summary>File extension.</summary>
        public const string FileExtension = ".ad";

        /// <summary>
        /// Creates an empty file.
        /// </summary>
        public AssemblyDrawingAttributeFile()
            : base(PageHost.AssemblyDrawing, ".ad.more", "adraw_dial_attributes")
        {
        }

        /// <inheritdoc />
        public override string Extension => FileExtension;

        /// <summary>Drawing filter (.ad.filter).</summary>
        public FilterFile Filter { get; } = new FilterFile(FilterFileExtensions.AssemblyDrawingAttributeFilter);

        /// <summary>Sheet number (<c>sheet_number</c>).</summary>
        public int SheetNumber
        {
            get => GetInt32("sheet_number");
            set => SetInt32("sheet_number", value);
        }

        /// <inheritdoc />
        protected override string LegacyKeyPrefix => "adraw_dial.";

        private protected override IEnumerable<IAttributeFileCompanion> FilterCompanions
        {
            get { yield return Filter; }
        }
    }
}
