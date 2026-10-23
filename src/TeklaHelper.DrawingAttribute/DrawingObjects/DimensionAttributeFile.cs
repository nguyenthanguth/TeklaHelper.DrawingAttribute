using System.Collections.Generic;

namespace TeklaHelper.DrawingAttribute.DrawingObjects
{
    /// <summary>
    /// Dimension properties (.dim): the file the Dimension properties dialog of the drawing editor saves, which dimensions
    /// load by name. <see cref="Properties"/> holds the dialog (the top-level keys); each mark of the dimension — prefix,
    /// postfix and the seven tags A ... G — is a <see cref="DimensionMark"/> under its own prefix (<c>PrefixMark.</c>,
    /// <c>LeftUpperTagMark.</c>, ...).
    /// <code>
    /// var dimension = DimensionAttributeFile.CreateDefault();
    /// dimension.Properties.General.DimensionTypes.Straight = DimensionType.Absolute;
    /// dimension.Properties.Appearance.Text.Height = 3.5;
    /// dimension.PrefixMark.Font.Height = 2.5;
    /// dimension.SaveTo(attributes, "absolute");       // absolute.dim
    /// </code>
    /// </summary>
    public sealed class DimensionAttributeFile : AttributeFile
    {
        /// <summary>File extension.</summary>
        public const string FileExtension = ".dim";

        /// <summary>
        /// Creates empty dimension properties.
        /// </summary>
        public DimensionAttributeFile()
        {
            Properties = new DimensionProperties(Values, string.Empty);
            PrefixMark = new DimensionMark(Values, "PrefixMark.");
            PostfixMark = new DimensionMark(Values, "PostfixMark.");
            LeftUpperTagMark = new DimensionMark(Values, "LeftUpperTagMark.");
            LeftMiddleTagMark = new DimensionMark(Values, "LeftMiddleTagMark.");
            LeftLowerTagMark = new DimensionMark(Values, "LeftLowerTagMark.");
            MiddleLowerTagMark = new DimensionMark(Values, "MiddleLowerTagMark.");
            RightUpperTagMark = new DimensionMark(Values, "RightUpperTagMark.");
            RightMiddleTagMark = new DimensionMark(Values, "RightMiddleTagMark.");
            RightLowerTagMark = new DimensionMark(Values, "RightLowerTagMark.");
        }

        /// <summary>
        /// Creates dimension properties with every key of a .dim file, so that Tekla takes nothing from the <c>standard</c>
        /// file of the environment. The values are the defaults of the pages (<c>SetDefaults()</c>: the value most of the
        /// .dim files shipped with Tekla hold, else the dialog's default). The file uses code page 1252: set
        /// <see cref="AttributeFile.CodePage"/> before saving Japanese or Korean text.
        /// </summary>
        public static DimensionAttributeFile CreateDefault()
        {
            var dimension = new DimensionAttributeFile();
            FileDefaults.Apply(dimension.Values, FileDefaults.Dimension.Table);
            dimension.Properties.SetDefaults();
            foreach (DimensionMark mark in dimension.Marks)
            {
                mark.SetDefaults();
            }

            return dimension;
        }

        /// <inheritdoc />
        public override string Extension => FileExtension;

        /// <summary>The Dimension properties dialog: the tabs General, Appearance, Marks and Tags (the top-level keys).</summary>
        public DimensionProperties Properties { get; }

        /// <summary>The mark before the dimension value (<c>PrefixMark.*</c>; "..." next to Prefix on the Marks tab).</summary>
        public DimensionMark PrefixMark { get; }

        /// <summary>The mark after the dimension value (<c>PostfixMark.*</c>; "..." next to Postfix on the Marks tab).</summary>
        public DimensionMark PostfixMark { get; }

        /// <summary>Tag A, left upper (<c>LeftUpperTagMark.*</c>).</summary>
        public DimensionMark LeftUpperTagMark { get; }

        /// <summary>Tag B, left middle (<c>LeftMiddleTagMark.*</c>).</summary>
        public DimensionMark LeftMiddleTagMark { get; }

        /// <summary>Tag C, left lower (<c>LeftLowerTagMark.*</c>).</summary>
        public DimensionMark LeftLowerTagMark { get; }

        /// <summary>Tag D, middle lower (<c>MiddleLowerTagMark.*</c>).</summary>
        public DimensionMark MiddleLowerTagMark { get; }

        /// <summary>Tag E, right upper (<c>RightUpperTagMark.*</c>).</summary>
        public DimensionMark RightUpperTagMark { get; }

        /// <summary>Tag F, right middle (<c>RightMiddleTagMark.*</c>).</summary>
        public DimensionMark RightMiddleTagMark { get; }

        /// <summary>Tag G, right lower (<c>RightLowerTagMark.*</c>).</summary>
        public DimensionMark RightLowerTagMark { get; }

        /// <summary>The nine marks: prefix, postfix and the tags A to G.</summary>
        public IEnumerable<DimensionMark> Marks => new[]
        {
            PrefixMark, PostfixMark, LeftUpperTagMark, LeftMiddleTagMark, LeftLowerTagMark, MiddleLowerTagMark,
            RightUpperTagMark, RightMiddleTagMark, RightLowerTagMark,
        };

        /// <inheritdoc />
        protected override string LegacyKeyPrefix => "dim_dial.";
    }
}
