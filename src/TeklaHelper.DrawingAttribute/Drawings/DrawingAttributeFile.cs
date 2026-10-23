using System.Collections.Generic;
using TeklaHelper.DrawingAttribute.Pages;

namespace TeklaHelper.DrawingAttribute.Drawings
{
    /// <summary>
    /// Base class of the drawing attribute files: GA (.gd), assembly (.ad), single-part (.wd), cast unit (.cud) and
    /// multi (.md). They share the pages of the drawing properties dialog (<see cref="Marks"/>, <see cref="Objects"/>,
    /// <see cref="Protection"/>) under two sets of prefixes: GA and multi drawings use <c>dview.*</c>, <c>dgrid.*</c>,
    /// <c>dnpart.*</c>, <c>drebar_mark.*</c>, <c>dsurf_mark.*</c>; assembly, single-part and cast unit drawings use
    /// <c>dv.*</c>, <c>grid.*</c>, <c>dnp.*</c>, <c>drmark.*</c>, <c>dsurfmark.*</c>. Each page class has the prefixes of
    /// every host as constants (<c>PartObject.GADrawingPrefix</c> is <c>dp.</c>, <c>PartObject.ViewPrefix</c> <c>vp.</c>).
    /// </summary>
    public abstract class DrawingAttributeFile : AttributeFile
    {
        private protected DrawingAttributeFile(PageHost host, string userAttributeExtension, string userAttributeObjectType)
        {
            UserAttributes = new DrawingUserAttributeFile(userAttributeExtension, userAttributeObjectType);
            Layout = new DrawingLayoutSection(Values);
            Views = new DrawingViewCreationSection(Values, host == PageHost.GADrawing ? DrawingViewCreationSection.GAPrefix : DrawingViewCreationSection.AssemblyPrefix);
            SectionView = new SectionViewPage(Values, SectionViewPage.PrefixFor(host));
            DetailView = new DetailViewPage(Values, DetailViewPage.PrefixFor(host));
            Dimension = new DimensionPage(Values, DimensionPage.PrefixFor(host));
            string dimensioning = DimensioningPage.PrefixFor(host);
            Dimensioning = dimensioning == null ? null : new DimensioningPage(Values, dimensioning);
            string assemblyDimensioning = AssemblyDimensioningPage.PrefixFor(host);
            AssemblyDimensioning = assemblyDimensioning == null ? null : new AssemblyDimensioningPage(Values, assemblyDimensioning);
            Protection = new ProtectionPage(Values, ProtectionPage.PrefixFor(host));
            Marks = new MarkPages(Values, host);
            Objects = new ObjectPages(Values, host);
        }

        /// <summary>
        /// Creates a file of type <typeparamref name="T"/> from the table of every key of its type: the <c>CreateDefault()</c>
        /// of each drawing type describes the values.
        /// </summary>
        private protected static T CreateDefault<T>((string Key, string Value)[] table)
            where T : DrawingAttributeFile, new()
        {
            var file = new T();
            FileDefaults.Apply(file.Values, table);
            file.Protection.SetDefaults();
            file.Marks.SetDefaults();
            file.Objects.SetDefaults();
            DefaultSetup.Apply(file);

            // The shipped files vote key by key: make the cre_* switches say what the view table says, as Tekla follows the table
            file.Views.SyncSwitchesWithViewTable();
            file.UserAttributes.SetEmptyFields();
            return file;
        }

        /// <summary>User-defined attributes, saved as the .more file with the same name.</summary>
        public DrawingUserAttributeFile UserAttributes { get; }

        /// <summary>Layout settings (<c>dl.*</c>).</summary>
        public DrawingLayoutSection Layout { get; }

        /// <summary>
        /// The "View..." page (<c>dview.*</c> or <c>dv.*</c>): view size, shortening, labels (with <c>LabelMarkA1</c> ...
        /// <c>LabelMarkA5</c>), anchor bolt plans, and the views the drawing creates.
        /// </summary>
        public DrawingViewCreationSection Views { get; }

        /// <summary>
        /// The section view settings (<c>dc.*</c>): scale and direction of the section views, the cutting line, the section
        /// mark and the section view label.
        /// </summary>
        public SectionViewPage SectionView { get; }

        /// <summary>The "Detail view..." page (<c>DetailLabel.*</c>): detail view label, detail boundary and detail mark.</summary>
        public DetailViewPage DetailView { get; }

        /// <summary>The "Dimension..." page (<c>dd.*</c>): dimension types, units, precision, placing and appearance.</summary>
        public DimensionPage Dimension { get; }

        /// <summary>
        /// The "Dimensioning..." page of GA and multi drawings (<c>Dimensioning.*</c>): grid line dimensions and the
        /// dimensioning of parts. Null for assembly, single-part and cast unit drawings: see <see cref="AssemblyDimensioning"/>.
        /// </summary>
        public DimensioningPage Dimensioning { get; }

        /// <summary>
        /// The "Dimensioning..." page of assembly, single-part and cast unit drawings (<c>dcd.*</c>): the automatic
        /// dimensioning of parts, bolts, positions and sub-assemblies. Null for GA and multi drawings: see
        /// <see cref="Dimensioning"/>.
        /// </summary>
        public AssemblyDimensioningPage AssemblyDimensioning { get; }

        /// <summary>The "Protection" page (<c>dpro.*</c>).</summary>
        public ProtectionPage Protection { get; }

        /// <summary>The "Marks" pages: part, bolt, neighbor part, surface treatment, connection, weld, reinforcement, neighbor reinforcement and pour marks.</summary>
        public MarkPages Marks { get; }

        /// <summary>The "Objects" pages: part, bolt, neighbor part, surface treatment, weld, reinforcement, reference object, grid, pour break and pour objects.</summary>
        public ObjectPages Objects { get; }

        /// <summary>Drawing description (<c>description</c>).</summary>
        public string Description
        {
            get => GetString("description");
            set => SetString("description", value);
        }

        /// <summary>Title 1 (<c>title_1</c>).</summary>
        public string Title1
        {
            get => GetString("title_1");
            set => SetString("title_1", value);
        }

        /// <summary>Title 2 (<c>title_2</c>).</summary>
        public string Title2
        {
            get => GetString("title_2");
            set => SetString("title_2", value);
        }

        /// <summary>Title 3 (<c>title_3</c>).</summary>
        public string Title3
        {
            get => GetString("title_3");
            set => SetString("title_3", value);
        }

        /// <summary>Name of the object level settings style (<c>StyleName</c>).</summary>
        public string StyleName
        {
            get => GetString("StyleName");
            set => SetString("StyleName", value);
        }

        /// <summary>
        /// Use detailed object level settings: the Yes / No choice in front of the Edit settings button of the drawing
        /// properties dialog (<c>UseDetailedObjectLevelSettings</c>, 1 / 0).
        /// </summary>
        public bool UseDetailedObjectLevelSettings
        {
            get => GetBoolean("UseDetailedObjectLevelSettings");
            set => SetBoolean("UseDetailedObjectLevelSettings", value);
        }

        /// <inheritdoc />
        protected override IEnumerable<IAttributeFileCompanion> CompanionFiles
        {
            get
            {
                yield return UserAttributes;
                foreach (IAttributeFileCompanion companion in FilterCompanions)
                {
                    yield return companion;
                }
            }
        }

        /// <summary>
        /// Filter files saved with this file type, after the .more file.
        /// </summary>
        private protected virtual IEnumerable<IAttributeFileCompanion> FilterCompanions
        {
            get { yield break; }
        }
    }
}
