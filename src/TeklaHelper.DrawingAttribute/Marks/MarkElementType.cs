using System;
using System.Collections.Generic;
using System.Globalization;

namespace TeklaHelper.DrawingAttribute.Marks
{
    /// <summary>
    /// A property element of mark content: the <c>subType</c> of an <c>&lt;Element&gt;</c> in a <c>ContentString</c>. Tekla
    /// decides by the number alone (an element named <c>PART_POS</c> with sub-type 82 is the view name, and an
    /// <c>&lt;Element&gt;</c> with sub-type 47 is the bolt size the dialog writes as <c>&lt;SizeElement&gt;</c>), so two types
    /// with one number are equal. The numbers are Tekla's own: they equal the values of the Open API's
    /// <c>PropertyElement.PropertyElementType.PropertyTypes</c> for 1 to 53 and 57 to 70 only (Tekla 2025 read sub-type 79 as
    /// <c>GR_SCALE</c>, which the API numbers 80, and 71 as the mesh size, which the API numbers 73). The elements of each
    /// kind of mark are in <see cref="MarkElementTypes"/>.
    /// </summary>
    public sealed class MarkElementType : IEquatable<MarkElementType>
    {
        private static readonly Dictionary<int, MarkElementType> Known = new Dictionary<int, MarkElementType>();

        private MarkElementType(int subType, string name, string description, bool sizeElement)
        {
            SubType = subType;
            Name = name;
            Description = description;
            IsSizeElement = sizeElement;
        }

        /// <summary>The number Tekla stores (<c>subType</c>).</summary>
        public int SubType { get; }

        /// <summary>The <c>name</c> the dialogs write with the element (often empty); Tekla does not read it.</summary>
        public string Name { get; }

        /// <summary>What the element shows.</summary>
        public string Description { get; }

        /// <summary>Stored as <c>&lt;SizeElement&gt;</c> instead of <c>&lt;Element&gt;</c> (the size of a bolt).</summary>
        public bool IsSizeElement { get; }

        internal string Tag => IsSizeElement ? "SizeElement" : "Element";

        /// <summary>
        /// The element type of <paramref name="subType"/>: one of <see cref="MarkElementTypes"/>, or an unnamed type for a
        /// number they do not list.
        /// </summary>
        public static MarkElementType FromSubType(int subType)
        {
            // The catalog registers its types when it is first used
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(MarkElementTypes).TypeHandle);
            lock (Known)
            {
                return Known.TryGetValue(subType, out MarkElementType type) ? type : new MarkElementType(subType, string.Empty, "Sub-type " + subType.ToString(CultureInfo.InvariantCulture), false);
            }
        }

        internal static MarkElementType FromXml(int subType, bool sizeElement)
        {
            MarkElementType known = FromSubType(subType);
            return known.IsSizeElement == sizeElement ? known : new MarkElementType(subType, known.Name, known.Description, sizeElement);
        }

        internal static MarkElementType Define(int subType, string name, string description, bool sizeElement = false)
        {
            lock (Known)
            {
                if (!Known.TryGetValue(subType, out MarkElementType type))
                {
                    type = new MarkElementType(subType, name, description, sizeElement);
                    Known[subType] = type;
                }

                return type;
            }
        }

        /// <summary>The same number: Tekla reads nothing else (the tag only decides how the element is written back).</summary>
        public bool Equals(MarkElementType other) => other != null && other.SubType == SubType;

        /// <inheritdoc />
        public override bool Equals(object obj) => Equals(obj as MarkElementType);

        /// <inheritdoc />
        public override int GetHashCode() => SubType;

        /// <inheritdoc />
        public override string ToString() => Description + " (" + SubType.ToString(CultureInfo.InvariantCulture) + ")";
    }

    /// <summary>
    /// The property elements the mark dialogs offer, by kind of mark (the lists of the Open API's
    /// <c>PropertyElementType.*PropertyElementTypes</c>, without the merged mark settings, which the files store as
    /// <c>&lt;MergeElement&gt;</c>, and without the "class" of pour marks, which the API maps to plain text). The sub-type of
    /// each was read back through the Open API of Tekla 2025: a file holding only that element, loaded with
    /// <c>Mark.MarkAttributes</c>, <c>View.ViewAttributes</c>, <c>SectionMarkAttributes</c> and <c>DetailMarkAttributes</c>,
    /// gave the element named here, the same in all four.
    /// </summary>
    public static class MarkElementTypes
    {
        // The sub-types Tekla 2025 read as property elements of the marks the Open API lists (not 39 TYPE and 99, which no
        // list offers, nor the merged mark settings 53 to 56 and the pull-out picture 60, which the dialogs write as tags of
        // their own: MarkXmlElement keeps them). The name is the one the files of the Tekla environments hold most often,
        // where one is given, else the name the Open API gives the element where it is an attribute name, else empty;
        // Tekla does not read it
        private static readonly MarkElementType BoltLength = MarkElementType.Define(7, "LENGTH", "Bolt length");
        private static readonly MarkElementType BoltDiameter = MarkElementType.Define(8, "SCREW_DIAMETER", "Bolt diameter");
        private static readonly MarkElementType HoleDiameter = MarkElementType.Define(9, "SCREW_HOLE_DIAMETER", "Hole diameter");
        private static readonly MarkElementType MaterialElement = MarkElementType.Define(10, "MATERIAL", "Material");
        private static readonly MarkElementType BoltStandard = MarkElementType.Define(11, "BOLT_STANDARD", "Bolt standard");
        private static readonly MarkElementType BoltShortName = MarkElementType.Define(12, "BOLT_SHORT_NAME", "Bolt short name");
        private static readonly MarkElementType BoltFullName = MarkElementType.Define(13, "BOLT_FULL_NAME", "Bolt full name");
        private static readonly MarkElementType BoltAssemblyType = MarkElementType.Define(14, "BOLT_SITE_WORKSHOP", "Site/workshop");
        private static readonly MarkElementType NumberOfBolts = MarkElementType.Define(15, string.Empty, "Number of bolts");
        private static readonly MarkElementType SlotLengthX = MarkElementType.Define(17, "LONG_HOLE_X", "Slot length in x");
        private static readonly MarkElementType SlotLengthY = MarkElementType.Define(18, "LONG_HOLE_Y", "Slot length in y");
        private static readonly MarkElementType SlotHeight = MarkElementType.Define(19, "LONGHOLE_MIN", "Slot height");
        private static readonly MarkElementType SlotLength = MarkElementType.Define(20, "LONGHOLE_MAX", "Slot length");
        private static readonly MarkElementType Countersunk = MarkElementType.Define(21, "COUNTERSUNK", "Countersunk");
        private static readonly MarkElementType AssemblyPosition = MarkElementType.Define(22, "ASSEMBLY_POS", "Assembly position");
        private static readonly MarkElementType PartPosition = MarkElementType.Define(23, "PART_POS", "Part position");
        private static readonly MarkElementType Profile = MarkElementType.Define(24, "PROFILE", "Profile");
        private static readonly MarkElementType NameElement = MarkElementType.Define(25, "NAME", "Name");
        private static readonly MarkElementType PartLength = MarkElementType.Define(26, "LENGTH", "Length");
        private static readonly MarkElementType Camber = MarkElementType.Define(27, string.Empty, "Camber");
        private static readonly MarkElementType PartSize = MarkElementType.Define(28, string.Empty, "Size");
        private static readonly MarkElementType FittingsNsFs = MarkElementType.Define(29, string.Empty, "Fittings (NS/FS)");
        private static readonly MarkElementType JointCode = MarkElementType.Define(30, "JOINT_CODE", "Connection code");
        private static readonly MarkElementType DstvCode = MarkElementType.Define(31, "JOINT_DSTV", "DSTV code");
        private static readonly MarkElementType JointNumber = MarkElementType.Define(32, "JOINT_NUMBER", "Connection number");
        private static readonly MarkElementType JointGroup = MarkElementType.Define(33, "JOINT_GROUP", "Connection group");
        private static readonly MarkElementType JointError = MarkElementType.Define(34, "JOINT_ERROR", "Connection error");
        private static readonly MarkElementType ObjectName = MarkElementType.Define(35, "NAME", "Name");
        private static readonly MarkElementType Grade = MarkElementType.Define(36, string.Empty, "Grade");
        private static readonly MarkElementType Diameter = MarkElementType.Define(37, string.Empty, "Diameter");
        private static readonly MarkElementType ClassElement = MarkElementType.Define(38, "CLASS_ATTR", "Class");
        private static readonly MarkElementType ObjectLength = MarkElementType.Define(40, "LENGTH", "Length");
        private static readonly MarkElementType Number = MarkElementType.Define(41, string.Empty, "Number (quantity)");
        private static readonly MarkElementType ObjectPosition = MarkElementType.Define(42, string.Empty, "Position");
        private static readonly MarkElementType Shape = MarkElementType.Define(43, string.Empty, "Shape");
        private static readonly MarkElementType Weight = MarkElementType.Define(44, "WEIGHT", "Weight");
        private static readonly MarkElementType Cc = MarkElementType.Define(45, "CC", "Center-to-center distance (c/c)");
        private static readonly MarkElementType BoltSize = MarkElementType.Define(47, string.Empty, "Bolt size", sizeElement: true);
        private static readonly MarkElementType GageOfOutstandingLeg = MarkElementType.Define(48, string.Empty, "Gage of outstanding leg");
        private static readonly MarkElementType CenterToCenterDistance = MarkElementType.Define(49, string.Empty, "Center-to-center distance");
        private static readonly MarkElementType CcMin = MarkElementType.Define(50, "CC_MIN", "Minimum c/c distance");
        private static readonly MarkElementType CcMax = MarkElementType.Define(51, "CC_MAX", "Maximum c/c distance");
        private static readonly MarkElementType CcExact = MarkElementType.Define(52, "CC_EXACT", "Exact c/c distances");
        private static readonly MarkElementType FaceDirection = MarkElementType.Define(57, string.Empty, "Face direction");
        private static readonly MarkElementType Finish = MarkElementType.Define(58, "FINISH", "Finish");
        private static readonly MarkElementType LabelElement = MarkElementType.Define(59, "LABEL", "Label");
        private static readonly MarkElementType MeshLength = MarkElementType.Define(61, "proLENGTH", "Mesh length");
        private static readonly MarkElementType MeshWidth = MarkElementType.Define(62, "proWIDTH", "Mesh width");
        private static readonly MarkElementType CcLongitudinal = MarkElementType.Define(63, "proCC_LONG", "Longitudinal c/c distance");
        private static readonly MarkElementType CcMinLongitudinal = MarkElementType.Define(64, "proCC_MIN_LONG", "Minimum longitudinal c/c distance");
        private static readonly MarkElementType CcMaxLongitudinal = MarkElementType.Define(65, "proCC_MAX_LONG", "Maximum longitudinal c/c distance");
        private static readonly MarkElementType CcExactLongitudinal = MarkElementType.Define(66, "proCC_EXACT_LONG", "Exact longitudinal c/c distances");
        private static readonly MarkElementType CcCrossing = MarkElementType.Define(67, "proCC_CROSS", "Crossing c/c distance");
        private static readonly MarkElementType CcMinCrossing = MarkElementType.Define(68, "proCC_MIN_CROSS", "Minimum crossing c/c distance");
        private static readonly MarkElementType CcMaxCrossing = MarkElementType.Define(69, "proCC_MAX_CROSS", "Maximum crossing c/c distance");
        private static readonly MarkElementType CcExactCrossing = MarkElementType.Define(70, "proCC_EXACT_CROSS", "Exact crossing c/c distances");
        private static readonly MarkElementType MeshSize = MarkElementType.Define(71, "SIZE", "Mesh size");
        private static readonly MarkElementType DiameterLongitudinal = MarkElementType.Define(72, "proCC_DIAMETER_LONG", "Longitudinal bar diameter");
        private static readonly MarkElementType DiameterCrossing = MarkElementType.Define(73, "proCC_DIAMETER_CROSS", "Crossing bar diameter");
        private static readonly MarkElementType SurfacingName = MarkElementType.Define(74, "SURFACING_NAME", "Surface treatment name");
        private static readonly MarkElementType SurfacingCode = MarkElementType.Define(75, "SURFACING_CODE", "Surface treatment code");
        private static readonly MarkElementType JointRunningNumber = MarkElementType.Define(78, "JOINT_RUNNING_NUMBER", "Connection running number");
        private static readonly MarkElementType ScaleElement = MarkElementType.Define(79, "SCALE", "Scale");
        private static readonly MarkElementType DetailNameElement = MarkElementType.Define(80, "NAME", "Detail name");
        private static readonly MarkElementType SectionNameElement = MarkElementType.Define(81, "NAME", "Section name");
        private static readonly MarkElementType ViewNameElement = MarkElementType.Define(82, "NAME", "View name");
        private static readonly MarkElementType DrawingNameElement = MarkElementType.Define(83, "DRAWING_NAME", "Drawing name");
        private static readonly MarkElementType SourceDrawingName = MarkElementType.Define(84, "SOURCE_DRAWING_NAME", "Source drawing name");
        private static readonly MarkElementType SourceDrawingNameWhenMoved = MarkElementType.Define(85, "SOURCE_DRAWING_NAME_WHEN_MOVED", "Source drawing name when moved");
        private static readonly MarkElementType ChamferDx = MarkElementType.Define(87, "X_DIRECTION_WIDTH", "Chamfer x");
        private static readonly MarkElementType ChamferDy = MarkElementType.Define(88, "Y_DIRECTION_WIDTH", "Chamfer y");
        private static readonly MarkElementType CcTarget = MarkElementType.Define(92, "CC_TARGET", "Target c/c distance");
        private static readonly MarkElementType PourNumberElement = MarkElementType.Define(93, "POUR_NUMBER", "Pour number");
        private static readonly MarkElementType PourTypeElement = MarkElementType.Define(94, "POUR_TYPE", "Pour type");
        private static readonly MarkElementType ConcreteMixture = MarkElementType.Define(95, "CONCRETE_MIXTURE", "Concrete mixture");
        private static readonly MarkElementType RotationAngle = MarkElementType.Define(96, string.Empty, "Rotation angle");
        private static readonly MarkElementType Layer = MarkElementType.Define(97, "LAYER", "Layer");
        private static readonly MarkElementType HoleDepth = MarkElementType.Define(98, "HOLE_DEPTH", "Hole depth");

        /// <summary>The elements of part marks.</summary>
        public static class PartMark
        {
            /// <summary>Assembly position (22).</summary>
            public static readonly MarkElementType AssemblyPosition = MarkElementTypes.AssemblyPosition;

            /// <summary>Part position (23).</summary>
            public static readonly MarkElementType PartPosition = MarkElementTypes.PartPosition;

            /// <summary>Profile (24).</summary>
            public static readonly MarkElementType Profile = MarkElementTypes.Profile;

            /// <summary>Material (10).</summary>
            public static readonly MarkElementType Material = MaterialElement;

            /// <summary>Name (25).</summary>
            public static readonly MarkElementType Name = NameElement;

            /// <summary>Class (38).</summary>
            public static readonly MarkElementType Class = ClassElement;

            /// <summary>Finish (58).</summary>
            public static readonly MarkElementType Finish = MarkElementTypes.Finish;

            /// <summary>Size (28).</summary>
            public static readonly MarkElementType Size = PartSize;

            /// <summary>Length (26).</summary>
            public static readonly MarkElementType Length = PartLength;

            /// <summary>Camber (27).</summary>
            public static readonly MarkElementType Camber = MarkElementTypes.Camber;

            /// <summary>Fittings NS/FS (29).</summary>
            public static readonly MarkElementType FittingsNsFs = MarkElementTypes.FittingsNsFs;

            /// <summary>Face direction (57).</summary>
            public static readonly MarkElementType FaceDirection = MarkElementTypes.FaceDirection;

            /// <summary>Gage of outstanding leg (48).</summary>
            public static readonly MarkElementType GageOfOutstandingLeg = MarkElementTypes.GageOfOutstandingLeg;

            /// <summary>Center-to-center distance (49).</summary>
            public static readonly MarkElementType CenterToCenterDistance = MarkElementTypes.CenterToCenterDistance;

            /// <summary>Rotation angle (96).</summary>
            public static readonly MarkElementType RotationAngle = MarkElementTypes.RotationAngle;
        }

        /// <summary>The elements of bolt marks.</summary>
        public static class BoltMark
        {
            /// <summary>Bolt length (7).</summary>
            public static readonly MarkElementType BoltLength = MarkElementTypes.BoltLength;

            /// <summary>Bolt diameter (8).</summary>
            public static readonly MarkElementType BoltDiameter = MarkElementTypes.BoltDiameter;

            /// <summary>Hole diameter (9).</summary>
            public static readonly MarkElementType HoleDiameter = MarkElementTypes.HoleDiameter;

            /// <summary>Material (10).</summary>
            public static readonly MarkElementType Material = MaterialElement;

            /// <summary>Bolt standard (11).</summary>
            public static readonly MarkElementType Standard = BoltStandard;

            /// <summary>Short name (12).</summary>
            public static readonly MarkElementType ShortName = BoltShortName;

            /// <summary>Full name (13).</summary>
            public static readonly MarkElementType FullName = BoltFullName;

            /// <summary>Site or workshop (14).</summary>
            public static readonly MarkElementType AssemblyType = BoltAssemblyType;

            /// <summary>Number of bolts (15).</summary>
            public static readonly MarkElementType NumberOfBolts = MarkElementTypes.NumberOfBolts;

            /// <summary>Slot length in x (17).</summary>
            public static readonly MarkElementType SlotLengthX = MarkElementTypes.SlotLengthX;

            /// <summary>Slot length in y (18).</summary>
            public static readonly MarkElementType SlotLengthY = MarkElementTypes.SlotLengthY;

            /// <summary>Slot height (19).</summary>
            public static readonly MarkElementType SlotHeight = MarkElementTypes.SlotHeight;

            /// <summary>Slot length (20).</summary>
            public static readonly MarkElementType SlotLength = MarkElementTypes.SlotLength;

            /// <summary>Bolt size (47, stored as <c>&lt;SizeElement&gt;</c> like the dialog does).</summary>
            public static readonly MarkElementType Size = BoltSize;

            /// <summary>Countersunk (21).</summary>
            public static readonly MarkElementType Countersunk = MarkElementTypes.Countersunk;

            /// <summary>Gage of outstanding leg (48).</summary>
            public static readonly MarkElementType GageOfOutstandingLeg = MarkElementTypes.GageOfOutstandingLeg;

            /// <summary>Center-to-center distance (49).</summary>
            public static readonly MarkElementType CenterToCenterDistance = MarkElementTypes.CenterToCenterDistance;

            /// <summary>Hole depth (98).</summary>
            public static readonly MarkElementType HoleDepth = MarkElementTypes.HoleDepth;
        }

        /// <summary>The elements of connection marks.</summary>
        public static class ConnectionMark
        {
            /// <summary>Connection code (30).</summary>
            public static readonly MarkElementType Code = JointCode;

            /// <summary>Name (25).</summary>
            public static readonly MarkElementType Name = NameElement;

            /// <summary>DSTV code (31).</summary>
            public static readonly MarkElementType DstvCode = MarkElementTypes.DstvCode;

            /// <summary>Connection number (32).</summary>
            public static readonly MarkElementType ConnectionNumber = JointNumber;

            /// <summary>Running number (78).</summary>
            public static readonly MarkElementType RunningNumber = JointRunningNumber;

            /// <summary>Connection group (33).</summary>
            public static readonly MarkElementType Group = JointGroup;

            /// <summary>Connection error (34).</summary>
            public static readonly MarkElementType ConnectionError = JointError;
        }

        /// <summary>The elements of reinforcement marks (bars and groups).</summary>
        public static class ReinforcementMark
        {
            /// <summary>Name (35).</summary>
            public static readonly MarkElementType Name = ObjectName;

            /// <summary>Grade (36).</summary>
            public static readonly MarkElementType Grade = MarkElementTypes.Grade;

            /// <summary>Diameter (37).</summary>
            public static readonly MarkElementType Diameter = MarkElementTypes.Diameter;

            /// <summary>Class (38).</summary>
            public static readonly MarkElementType Class = ClassElement;

            /// <summary>Length (40).</summary>
            public static readonly MarkElementType Length = ObjectLength;

            /// <summary>Number of bars (41).</summary>
            public static readonly MarkElementType Number = MarkElementTypes.Number;

            /// <summary>Position (42).</summary>
            public static readonly MarkElementType Position = ObjectPosition;

            /// <summary>Shape (43).</summary>
            public static readonly MarkElementType Shape = MarkElementTypes.Shape;

            /// <summary>Weight (44).</summary>
            public static readonly MarkElementType Weight = MarkElementTypes.Weight;

            /// <summary>Center-to-center distance (45).</summary>
            public static readonly MarkElementType Cc = MarkElementTypes.Cc;

            /// <summary>Minimum c/c distance (50).</summary>
            public static readonly MarkElementType CcMin = MarkElementTypes.CcMin;

            /// <summary>Maximum c/c distance (51).</summary>
            public static readonly MarkElementType CcMax = MarkElementTypes.CcMax;

            /// <summary>Exact c/c distances (52).</summary>
            public static readonly MarkElementType CcExact = MarkElementTypes.CcExact;

            /// <summary>Target c/c distance (92).</summary>
            public static readonly MarkElementType CcTarget = MarkElementTypes.CcTarget;
        }

        /// <summary>The elements of reinforcement mesh marks.</summary>
        public static class ReinforcementMeshMark
        {
            /// <summary>Name (35).</summary>
            public static readonly MarkElementType Name = ObjectName;

            /// <summary>Grade (36).</summary>
            public static readonly MarkElementType Grade = MarkElementTypes.Grade;

            /// <summary>Mesh size (71).</summary>
            public static readonly MarkElementType Size = MeshSize;

            /// <summary>Class (38).</summary>
            public static readonly MarkElementType Class = ClassElement;

            /// <summary>Mesh length (61).</summary>
            public static readonly MarkElementType MeshLength = MarkElementTypes.MeshLength;

            /// <summary>Mesh width (62).</summary>
            public static readonly MarkElementType MeshWidth = MarkElementTypes.MeshWidth;

            /// <summary>Number (41).</summary>
            public static readonly MarkElementType Number = MarkElementTypes.Number;

            /// <summary>Position (42).</summary>
            public static readonly MarkElementType Position = ObjectPosition;

            /// <summary>Shape (43).</summary>
            public static readonly MarkElementType Shape = MarkElementTypes.Shape;

            /// <summary>Weight (44).</summary>
            public static readonly MarkElementType Weight = MarkElementTypes.Weight;

            /// <summary>Longitudinal c/c distance (63).</summary>
            public static readonly MarkElementType CcLongitudinal = MarkElementTypes.CcLongitudinal;

            /// <summary>Minimum longitudinal c/c distance (64).</summary>
            public static readonly MarkElementType CcMinLongitudinal = MarkElementTypes.CcMinLongitudinal;

            /// <summary>Maximum longitudinal c/c distance (65).</summary>
            public static readonly MarkElementType CcMaxLongitudinal = MarkElementTypes.CcMaxLongitudinal;

            /// <summary>Exact longitudinal c/c distances (66).</summary>
            public static readonly MarkElementType CcExactLongitudinal = MarkElementTypes.CcExactLongitudinal;

            /// <summary>Crossing c/c distance (67).</summary>
            public static readonly MarkElementType CcCrossing = MarkElementTypes.CcCrossing;

            /// <summary>Minimum crossing c/c distance (68).</summary>
            public static readonly MarkElementType CcMinCrossing = MarkElementTypes.CcMinCrossing;

            /// <summary>Maximum crossing c/c distance (69).</summary>
            public static readonly MarkElementType CcMaxCrossing = MarkElementTypes.CcMaxCrossing;

            /// <summary>Exact crossing c/c distances (70).</summary>
            public static readonly MarkElementType CcExactCrossing = MarkElementTypes.CcExactCrossing;

            /// <summary>Longitudinal bar diameter (72).</summary>
            public static readonly MarkElementType DiameterLongitudinal = MarkElementTypes.DiameterLongitudinal;

            /// <summary>Crossing bar diameter (73).</summary>
            public static readonly MarkElementType DiameterCrossing = MarkElementTypes.DiameterCrossing;
        }

        /// <summary>The elements of surface treatment marks.</summary>
        public static class SurfaceTreatmentMark
        {
            /// <summary>Name (35).</summary>
            public static readonly MarkElementType Name = ObjectName;

            /// <summary>Material (10).</summary>
            public static readonly MarkElementType Material = MaterialElement;

            /// <summary>Class (38).</summary>
            public static readonly MarkElementType Class = ClassElement;

            /// <summary>Surface treatment code (75).</summary>
            public static readonly MarkElementType Code = SurfacingCode;

            /// <summary>Surface treatment name (74).</summary>
            public static readonly MarkElementType SurfaceTreatmentName = SurfacingName;
        }

        /// <summary>The elements of edge chamfer marks (<c>ChamferContentString</c>).</summary>
        public static class EdgeChamferMark
        {
            /// <summary>Name (35).</summary>
            public static readonly MarkElementType Name = ObjectName;

            /// <summary>Length (26).</summary>
            public static readonly MarkElementType Length = PartLength;

            /// <summary>Chamfer x (87).</summary>
            public static readonly MarkElementType Dx = ChamferDx;

            /// <summary>Chamfer y (88).</summary>
            public static readonly MarkElementType Dy = ChamferDy;
        }

        /// <summary>The elements of pour marks.</summary>
        public static class PourMark
        {
            /// <summary>Material (10).</summary>
            public static readonly MarkElementType Material = MaterialElement;

            /// <summary>Name (25).</summary>
            public static readonly MarkElementType Name = NameElement;

            /// <summary>Pour number (93).</summary>
            public static readonly MarkElementType PourNumber = PourNumberElement;

            /// <summary>Pour type (94).</summary>
            public static readonly MarkElementType PourType = PourTypeElement;

            /// <summary>Concrete mixture (95).</summary>
            public static readonly MarkElementType ConcreteMixture = MarkElementTypes.ConcreteMixture;
        }

        /// <summary>The elements of view labels (<c>ViewLabelMarkA1.ContentString</c> ...).</summary>
        public static class ViewLabel
        {
            /// <summary>View name (82).</summary>
            public static readonly MarkElementType ViewName = ViewNameElement;

            /// <summary>Scale (79).</summary>
            public static readonly MarkElementType Scale = ScaleElement;

            /// <summary>Drawing name (83).</summary>
            public static readonly MarkElementType DrawingName = DrawingNameElement;

            /// <summary>Source drawing name (84).</summary>
            public static readonly MarkElementType SourceDrawingName = MarkElementTypes.SourceDrawingName;

            /// <summary>Source drawing name when moved (85).</summary>
            public static readonly MarkElementType SourceDrawingNameWhenMoved = MarkElementTypes.SourceDrawingNameWhenMoved;
        }

        /// <summary>
        /// The elements of section marks and of section view labels. The Open API names the section name of a section view
        /// label <c>SectionViewLabelMarkPropertyElementTypes.SectionName</c> but stores it as the view name (82); the section
        /// mark's own section name is 81.
        /// </summary>
        public static class SectionMark
        {
            /// <summary>Section name of a section mark (81).</summary>
            public static readonly MarkElementType SectionName = SectionNameElement;

            /// <summary>Section name of a section view label: the view name (82).</summary>
            public static readonly MarkElementType ViewName = ViewNameElement;

            /// <summary>Scale, in a section view label (79).</summary>
            public static readonly MarkElementType Scale = ScaleElement;

            /// <summary>Drawing name, in a section view label (83).</summary>
            public static readonly MarkElementType DrawingName = DrawingNameElement;

            /// <summary>Source drawing name (84).</summary>
            public static readonly MarkElementType SourceDrawingName = MarkElementTypes.SourceDrawingName;

            /// <summary>Source drawing name when moved (85).</summary>
            public static readonly MarkElementType SourceDrawingNameWhenMoved = MarkElementTypes.SourceDrawingNameWhenMoved;
        }

        /// <summary>
        /// The elements of detail marks and of detail view labels. As for sections, a detail view label stores its detail
        /// name as the view name (82); the detail mark's own detail name is 80.
        /// </summary>
        public static class DetailMark
        {
            /// <summary>Detail name of a detail mark (80).</summary>
            public static readonly MarkElementType DetailName = DetailNameElement;

            /// <summary>Detail name of a detail view label: the view name (82).</summary>
            public static readonly MarkElementType ViewName = ViewNameElement;

            /// <summary>Scale, in a detail view label (79).</summary>
            public static readonly MarkElementType Scale = ScaleElement;

            /// <summary>Drawing name, in a detail view label (83).</summary>
            public static readonly MarkElementType DrawingName = DrawingNameElement;

            /// <summary>Source drawing name (84).</summary>
            public static readonly MarkElementType SourceDrawingName = MarkElementTypes.SourceDrawingName;

            /// <summary>Source drawing name when moved (85).</summary>
            public static readonly MarkElementType SourceDrawingNameWhenMoved = MarkElementTypes.SourceDrawingNameWhenMoved;
        }

        /// <summary>Elements Tekla reads in every kind of mark but that no list above offers.</summary>
        public static class Other
        {
            /// <summary>Label (59).</summary>
            public static readonly MarkElementType Label = LabelElement;

            /// <summary>Layer (97).</summary>
            public static readonly MarkElementType Layer = MarkElementTypes.Layer;
        }
    }
}
