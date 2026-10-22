using System.Collections.Generic;
using TeklaHelper.DrawingAttribute.Filters;
using TeklaHelper.DrawingAttribute.Pages;

namespace TeklaHelper.DrawingAttribute.Drawings
{
    /// <summary>
    /// Settings of a single-part drawing (.wd), with its user-defined attributes (.wd.more) and filter (.wd.filter). Use
    /// the saved file with <c>new SinglePartDrawing(partIdentifier, attributeFileName)</c>.
    /// <para>
    /// New .more files use the object type <c>singledrawing_attributes</c>, the most common one in the environments
    /// shipped with Tekla 2025; some of them use <c>wdraw_dial_attributes</c> instead, and loading such a file keeps it.
    /// </para>
    /// </summary>
    public sealed class SinglePartDrawingAttributeFile : DrawingAttributeFile
    {
        /// <summary>File extension.</summary>
        public const string FileExtension = ".wd";

        /// <summary>
        /// Creates an empty file.
        /// </summary>
        public SinglePartDrawingAttributeFile()
            : base(PageHost.AssemblyDrawing, ".wd.more", "singledrawing_attributes")
        {
        }

        /// <inheritdoc />
        public override string Extension => FileExtension;

        /// <summary>Drawing filter (.wd.filter).</summary>
        public FilterFile Filter { get; } = new FilterFile(FilterFileExtensions.SinglePartDrawingAttributeFilter);

        /// <summary>Sheet number (<c>sheet_number</c>).</summary>
        public int SheetNumber
        {
            get => GetInt32("sheet_number");
            set => SetInt32("sheet_number", value);
        }

        /// <inheritdoc />
        protected override string LegacyKeyPrefix => "wdraw_dial.";

        private protected override IEnumerable<IAttributeFileCompanion> FilterCompanions
        {
            get { yield return Filter; }
        }
    }
}
