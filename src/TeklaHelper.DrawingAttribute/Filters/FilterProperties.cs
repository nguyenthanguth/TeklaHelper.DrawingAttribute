// Generated from Tekla.Structures.Filtering.Categories of Tekla Structures 2025 (Tekla.Structures.dll 2025.0) by
// writing one filter per property with Filter.CreateFile and reading the tokens back (tools/FilterCatalogGenerator).
// TeklaOracleTests checks it against Tekla.Structures.dll when Tekla Structures is installed.

namespace TeklaHelper.DrawingAttribute.Filters
{
    /// <summary>
    /// The filter properties of <c>Tekla.Structures.Filtering.Categories</c>, with the tokens Tekla writes for them. Each
    /// nested class is one category (<c>PartFilterExpressions</c> is <see cref="Part"/>), and has CustomString,
    /// CustomNumber, CustomDateTime and CustomBoolean for user-defined attributes:
    /// <code>
    /// FilterProperties.Part.Name.IsEqual("BEAM")
    /// FilterProperties.Part.Phase.GreaterOrEqual(2)
    /// FilterProperties.Assembly.CustomString("USER_FIELD_1").Contains("A")
    /// </code>
    /// The value types are those of the Tekla API, which is why for example <see cref="Assembly.Phase"/> is text.
    /// </summary>
    public static class FilterProperties
    {
        /// <summary>Properties of assemblies (<c>co_assembly</c>), like <c>AssemblyFilterExpressions</c>.</summary>
        public static class Assembly
        {
            /// <summary>Category token.</summary>
            public const string Category = "co_assembly";

            /// <summary>Guid (<c>proGUID</c>).</summary>
            public static readonly StringFilterProperty Guid = new StringFilterProperty(Category, "proGUID", "albl_Guid");

            /// <summary>Id number (<c>proId</c>).</summary>
            public static readonly StringFilterProperty IdNumber = new StringFilterProperty(Category, "proId", "albl_Id_number");

            /// <summary>Level (<c>proASSEMBLY_LEVEL</c>).</summary>
            public static readonly StringFilterProperty Level = new StringFilterProperty(Category, "proASSEMBLY_LEVEL", "albl_Assembly_level");

            /// <summary>Name (<c>proNAME</c>).</summary>
            public static readonly StringFilterProperty Name = new StringFilterProperty(Category, "proNAME", "albl_Name");

            /// <summary>Phase (<c>proPHASE</c>).</summary>
            public static readonly StringFilterProperty Phase = new StringFilterProperty(Category, "proPHASE", "albl_Phase");

            /// <summary>Position number (<c>proASSEMBLY_POSITION</c>).</summary>
            public static readonly StringFilterProperty PositionNumber = new StringFilterProperty(Category, "proASSEMBLY_POSITION", "albl_Position_number");

            /// <summary>Prefix (<c>proNUMBERING_SERIE</c>).</summary>
            public static readonly StringFilterProperty Prefix = new StringFilterProperty(Category, "proNUMBERING_SERIE", "albl_Prefix");

            /// <summary>Series (<c>proAssemblySeries</c>).</summary>
            public static readonly StringFilterProperty Series = new StringFilterProperty(Category, "proAssemblySeries", "albl_Assembly_serie");

            /// <summary>Start number (<c>proSTARTNUMBER</c>).</summary>
            public static readonly StringFilterProperty StartNumber = new StringFilterProperty(Category, "proSTARTNUMBER", "albl_Start_number");

            /// <summary>Type (<c>proAssemblyType</c>).</summary>
            public static readonly StringFilterProperty Type = new StringFilterProperty(Category, "proAssemblyType", "albl_Assembly_type");

            /// <summary>A text user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static StringFilterProperty CustomString(string name) => new StringFilterProperty(Category, name);

            /// <summary>A numeric user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static NumericFilterProperty CustomNumber(string name) => new NumericFilterProperty(Category, name);

            /// <summary>A date user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static DateFilterProperty CustomDateTime(string name) => new DateFilterProperty(Category, name);

            /// <summary>A true / false user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static BooleanFilterProperty CustomBoolean(string name) => new BooleanFilterProperty(Category, name);
        }

        /// <summary>Properties of bolts (<c>co_bolt</c>), like <c>BoltFilterExpressions</c>.</summary>
        public static class Bolt
        {
            /// <summary>Category token.</summary>
            public const string Category = "co_bolt";

            /// <summary>Length (<c>proLENGTH</c>).</summary>
            public static readonly NumericFilterProperty Length = new NumericFilterProperty(Category, "proLENGTH", "albl_Length");

            /// <summary>Phase (<c>proPHASE</c>).</summary>
            public static readonly NumericFilterProperty Phase = new NumericFilterProperty(Category, "proPHASE", "albl_Phase");

            /// <summary>Site workshop (<c>proBOLT_TYPE</c>).</summary>
            public static readonly StringFilterProperty SiteWorkshop = new StringFilterProperty(Category, "proBOLT_TYPE", "albl_Site__workshop");

            /// <summary>Size (<c>proSIZE</c>).</summary>
            public static readonly NumericFilterProperty Size = new NumericFilterProperty(Category, "proSIZE", "albl_Size");

            /// <summary>Standard (<c>proSTANDARD</c>).</summary>
            public static readonly StringFilterProperty Standard = new StringFilterProperty(Category, "proSTANDARD", "albl_Standard");

            /// <summary>A text user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static StringFilterProperty CustomString(string name) => new StringFilterProperty(Category, name);

            /// <summary>A numeric user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static NumericFilterProperty CustomNumber(string name) => new NumericFilterProperty(Category, name);

            /// <summary>A date user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static DateFilterProperty CustomDateTime(string name) => new DateFilterProperty(Category, name);

            /// <summary>A true / false user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static BooleanFilterProperty CustomBoolean(string name) => new BooleanFilterProperty(Category, name);
        }

        /// <summary>Properties of components (<c>co_component</c>), like <c>ComponentFilterExpressions</c>.</summary>
        public static class Component
        {
            /// <summary>Category token.</summary>
            public const string Category = "co_component";

            /// <summary>Connection code (<c>proComponentCode</c>).</summary>
            public static readonly StringFilterProperty ConnectionCode = new StringFilterProperty(Category, "proComponentCode", "albl_Joint_code");

            /// <summary>Name (<c>proSTRIPPED_NAME</c>).</summary>
            public static readonly StringFilterProperty Name = new StringFilterProperty(Category, "proSTRIPPED_NAME", "albl_Name");

            /// <summary>Phase (<c>proPHASE</c>).</summary>
            public static readonly NumericFilterProperty Phase = new NumericFilterProperty(Category, "proPHASE", "albl_Phase");

            /// <summary>Running number (<c>proComponentRunningNumber</c>).</summary>
            public static readonly StringFilterProperty RunningNumber = new StringFilterProperty(Category, "proComponentRunningNumber", "albl_Running_number");

            /// <summary>A text user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static StringFilterProperty CustomString(string name) => new StringFilterProperty(Category, name);

            /// <summary>A numeric user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static NumericFilterProperty CustomNumber(string name) => new NumericFilterProperty(Category, name);

            /// <summary>A date user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static DateFilterProperty CustomDateTime(string name) => new DateFilterProperty(Category, name);

            /// <summary>A true / false user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static BooleanFilterProperty CustomBoolean(string name) => new BooleanFilterProperty(Category, name);
        }

        /// <summary>Properties of construction objects (<c>co_construction_object</c>), like <c>ConstructionObjectFilterExpressions</c>.</summary>
        public static class ConstructionObject
        {
            /// <summary>Category token.</summary>
            public const string Category = "co_construction_object";

            /// <summary>Phase (<c>proPHASE</c>).</summary>
            public static readonly NumericFilterProperty Phase = new NumericFilterProperty(Category, "proPHASE", "albl_Phase");

            /// <summary>Type (<c>proOBJECT_TYPE</c>).</summary>
            public static readonly NumericFilterProperty Type = new NumericFilterProperty(Category, "proOBJECT_TYPE", "albl_Type");

            /// <summary>A text user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static StringFilterProperty CustomString(string name) => new StringFilterProperty(Category, name);

            /// <summary>A numeric user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static NumericFilterProperty CustomNumber(string name) => new NumericFilterProperty(Category, name);

            /// <summary>A date user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static DateFilterProperty CustomDateTime(string name) => new DateFilterProperty(Category, name);

            /// <summary>A true / false user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static BooleanFilterProperty CustomBoolean(string name) => new BooleanFilterProperty(Category, name);
        }

        /// <summary>Properties of loads (<c>loadm_pointload</c>), like <c>LoadFilterExpressions</c>.</summary>
        public static class Load
        {
            /// <summary>Category token.</summary>
            public const string Category = "loadm_pointload";

            /// <summary>Group (<c>LGROUP.NAME</c>).</summary>
            public static readonly StringFilterProperty Group = new StringFilterProperty(Category, "LGROUP.NAME", "albl_Load_group");

            /// <summary>Phase (<c>proPHASE</c>).</summary>
            public static readonly NumericFilterProperty Phase = new NumericFilterProperty(Category, "proPHASE", "albl_Phase");

            /// <summary>Type (<c>TypeName</c>).</summary>
            public static readonly StringFilterProperty Type = new StringFilterProperty(Category, "TypeName", "albl_Load_type");

            /// <summary>A text user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static StringFilterProperty CustomString(string name) => new StringFilterProperty(Category, name);

            /// <summary>A numeric user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static NumericFilterProperty CustomNumber(string name) => new NumericFilterProperty(Category, name);

            /// <summary>A date user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static DateFilterProperty CustomDateTime(string name) => new DateFilterProperty(Category, name);

            /// <summary>A true / false user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static BooleanFilterProperty CustomBoolean(string name) => new BooleanFilterProperty(Category, name);
        }

        /// <summary>Properties of locations (logical building areas) (<c>LogicalBuildingAreaCategory</c>), like <c>LogicalAreaFilterExpressions</c>.</summary>
        public static class LogicalArea
        {
            /// <summary>Category token.</summary>
            public const string Category = "LogicalBuildingAreaCategory";

            /// <summary>Building (<c>proLBABuilding</c>).</summary>
            public static readonly StringFilterProperty Building = new StringFilterProperty(Category, "proLBABuilding", "albl_LBA_Building");

            /// <summary>Section (<c>proLBASection</c>).</summary>
            public static readonly StringFilterProperty Section = new StringFilterProperty(Category, "proLBASection", "albl_LBA_Section");

            /// <summary>Site (<c>proLBASite</c>).</summary>
            public static readonly StringFilterProperty Site = new StringFilterProperty(Category, "proLBASite", "albl_LBA_Site");

            /// <summary>Story (<c>proLBAStorey</c>).</summary>
            public static readonly StringFilterProperty Story = new StringFilterProperty(Category, "proLBAStorey", "albl_LBA_Storey");

            /// <summary>A text user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static StringFilterProperty CustomString(string name) => new StringFilterProperty(Category, name);

            /// <summary>A numeric user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static NumericFilterProperty CustomNumber(string name) => new NumericFilterProperty(Category, name);

            /// <summary>A date user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static DateFilterProperty CustomDateTime(string name) => new DateFilterProperty(Category, name);

            /// <summary>A true / false user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static BooleanFilterProperty CustomBoolean(string name) => new BooleanFilterProperty(Category, name);
        }

        /// <summary>Properties of any model object (<c>co_object</c>), like <c>ObjectFilterExpressions</c>.</summary>
        public static class Object
        {
            /// <summary>Category token.</summary>
            public const string Category = "co_object";

            /// <summary>Guid (<c>proGUID</c>).</summary>
            public static readonly StringFilterProperty Guid = new StringFilterProperty(Category, "proGUID", "albl_Guid");

            /// <summary>Id number (<c>proId</c>).</summary>
            public static readonly NumericFilterProperty IdNumber = new NumericFilterProperty(Category, "proId", "albl_Id_number");

            /// <summary>Is component (<c>proIsComponent</c>).</summary>
            public static readonly NumericFilterProperty IsComponent = new NumericFilterProperty(Category, "proIsComponent", "albl_Is_component");

            /// <summary>Phase (<c>proPHASE</c>).</summary>
            public static readonly NumericFilterProperty Phase = new NumericFilterProperty(Category, "proPHASE", "albl_Phase");

            /// <summary>Type (<c>proOBJECT_TYPE</c>).</summary>
            public static readonly NumericFilterProperty Type = new NumericFilterProperty(Category, "proOBJECT_TYPE", "albl_ObjectType");

            /// <summary>A text user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static StringFilterProperty CustomString(string name) => new StringFilterProperty(Category, name);

            /// <summary>A numeric user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static NumericFilterProperty CustomNumber(string name) => new NumericFilterProperty(Category, name);

            /// <summary>A date user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static DateFilterProperty CustomDateTime(string name) => new DateFilterProperty(Category, name);

            /// <summary>A true / false user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static BooleanFilterProperty CustomBoolean(string name) => new BooleanFilterProperty(Category, name);
        }

        /// <summary>Properties of object types (<c>ObjectTypesCategory</c>), like <c>ObjectTypesFilterExpressions</c>.</summary>
        public static class ObjectTypes
        {
            /// <summary>Category token.</summary>
            public const string Category = "ObjectTypesCategory";

            /// <summary>Category name (<c>proObjectTypesNodeType</c>).</summary>
            public static readonly StringFilterProperty CategoryName = new StringFilterProperty(Category, "proObjectTypesNodeType", "albl_Category_name");

            /// <summary>Entity name (<c>proNAME</c>).</summary>
            public static readonly StringFilterProperty EntityName = new StringFilterProperty(Category, "proNAME", "albl_Entity_name");

            /// <summary>A text user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static StringFilterProperty CustomString(string name) => new StringFilterProperty(Category, name);

            /// <summary>A numeric user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static NumericFilterProperty CustomNumber(string name) => new NumericFilterProperty(Category, name);

            /// <summary>A date user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static DateFilterProperty CustomDateTime(string name) => new DateFilterProperty(Category, name);

            /// <summary>A true / false user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static BooleanFilterProperty CustomBoolean(string name) => new BooleanFilterProperty(Category, name);
        }

        /// <summary>Properties of parts (<c>co_part</c>), like <c>PartFilterExpressions</c>.</summary>
        public static class Part
        {
            /// <summary>Category token.</summary>
            public const string Category = "co_part";

            /// <summary>Class (<c>proCLASS</c>).</summary>
            public static readonly NumericFilterProperty Class = new NumericFilterProperty(Category, "proCLASS", "albl_Class");

            /// <summary>Finish (<c>proFinish</c>).</summary>
            public static readonly StringFilterProperty Finish = new StringFilterProperty(Category, "proFinish", "albl_Finish");

            /// <summary>Lot (<c>proLOT</c>).</summary>
            public static readonly NumericFilterProperty Lot = new NumericFilterProperty(Category, "proLOT", "albl_Lot");

            /// <summary>Material (<c>proMATERIAL</c>).</summary>
            public static readonly StringFilterProperty Material = new StringFilterProperty(Category, "proMATERIAL", "albl_Material");

            /// <summary>Name (<c>proNAME</c>).</summary>
            public static readonly StringFilterProperty Name = new StringFilterProperty(Category, "proNAME", "albl_Name");

            /// <summary>Numbering series (<c>proPartSeries</c>).</summary>
            public static readonly StringFilterProperty NumberingSeries = new StringFilterProperty(Category, "proPartSeries", "albl_Numbering_series");

            /// <summary>Phase (<c>proPHASE</c>).</summary>
            public static readonly NumericFilterProperty Phase = new NumericFilterProperty(Category, "proPHASE", "albl_Phase");

            /// <summary>Position number (<c>proNUMBERING_POSITION</c>).</summary>
            public static readonly StringFilterProperty PositionNumber = new StringFilterProperty(Category, "proNUMBERING_POSITION", "albl_Position_number");

            /// <summary>Pour phase (<c>proPOUR_PHASE</c>).</summary>
            public static readonly NumericFilterProperty PourPhase = new NumericFilterProperty(Category, "proPOUR_PHASE", "albl_Pour_phase");

            /// <summary>Prefix (<c>proSERIE</c>).</summary>
            public static readonly StringFilterProperty Prefix = new StringFilterProperty(Category, "proSERIE", "albl_Prefix");

            /// <summary>Primary part (<c>proPrimaryPart</c>).</summary>
            public static readonly StringFilterProperty PrimaryPart = new StringFilterProperty(Category, "proPrimaryPart", "albl_Primary_part");

            /// <summary>Profile (<c>proPROFILE</c>).</summary>
            public static readonly StringFilterProperty Profile = new StringFilterProperty(Category, "proPROFILE", "albl_Profile");

            /// <summary>Start number (<c>proSTARTNUMBER</c>).</summary>
            public static readonly NumericFilterProperty StartNumber = new NumericFilterProperty(Category, "proSTARTNUMBER", "albl_Start_number");

            /// <summary>A text user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static StringFilterProperty CustomString(string name) => new StringFilterProperty(Category, name);

            /// <summary>A numeric user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static NumericFilterProperty CustomNumber(string name) => new NumericFilterProperty(Category, name);

            /// <summary>A date user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static DateFilterProperty CustomDateTime(string name) => new DateFilterProperty(Category, name);

            /// <summary>A true / false user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static BooleanFilterProperty CustomBoolean(string name) => new BooleanFilterProperty(Category, name);
        }

        /// <summary>Properties of pour breaks (<c>co_pour_break</c>), like <c>PourBreakFilterExpressions</c>.</summary>
        public static class PourBreak
        {
            /// <summary>Category token.</summary>
            public const string Category = "co_pour_break";

            /// <summary>A text user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static StringFilterProperty CustomString(string name) => new StringFilterProperty(Category, name);

            /// <summary>A numeric user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static NumericFilterProperty CustomNumber(string name) => new NumericFilterProperty(Category, name);

            /// <summary>A date user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static DateFilterProperty CustomDateTime(string name) => new DateFilterProperty(Category, name);

            /// <summary>A true / false user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static BooleanFilterProperty CustomBoolean(string name) => new BooleanFilterProperty(Category, name);
        }

        /// <summary>Properties of pour objects (<c>co_pour_object</c>), like <c>PourObjectFilterExpressions</c>.</summary>
        public static class PourObject
        {
            /// <summary>Category token.</summary>
            public const string Category = "co_pour_object";

            /// <summary>Concrete mixture (<c>proPOUR_CONCRETE_MIXTURE</c>).</summary>
            public static readonly StringFilterProperty ConcreteMixture = new StringFilterProperty(Category, "proPOUR_CONCRETE_MIXTURE", "albl_Pour_concrete_mixture");

            /// <summary>Material (<c>proMATERIAL</c>).</summary>
            public static readonly StringFilterProperty Material = new StringFilterProperty(Category, "proMATERIAL", "albl_Material");

            /// <summary>Pour number (<c>proPOUR_NUMBER</c>).</summary>
            public static readonly StringFilterProperty PourNumber = new StringFilterProperty(Category, "proPOUR_NUMBER", "albl_Pour_number");

            /// <summary>Pour phase (<c>proPOUR_PHASE</c>).</summary>
            public static readonly StringFilterProperty PourPhase = new StringFilterProperty(Category, "proPOUR_PHASE", "albl_Pour_phase");

            /// <summary>Pour type (<c>proPOUR_TYPE</c>).</summary>
            public static readonly StringFilterProperty PourType = new StringFilterProperty(Category, "proPOUR_TYPE", "albl_Pour_type");

            /// <summary>A text user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static StringFilterProperty CustomString(string name) => new StringFilterProperty(Category, name);

            /// <summary>A numeric user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static NumericFilterProperty CustomNumber(string name) => new NumericFilterProperty(Category, name);

            /// <summary>A date user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static DateFilterProperty CustomDateTime(string name) => new DateFilterProperty(Category, name);

            /// <summary>A true / false user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static BooleanFilterProperty CustomBoolean(string name) => new BooleanFilterProperty(Category, name);
        }

        /// <summary>Properties of pour units (<c>co_pour_unit</c>), like <c>PourUnitFilterExpressions</c>.</summary>
        public static class PourUnit
        {
            /// <summary>Category token.</summary>
            public const string Category = "co_pour_unit";

            /// <summary>Guid (<c>proGUID</c>).</summary>
            public static readonly StringFilterProperty Guid = new StringFilterProperty(Category, "proGUID", "albl_Guid");

            /// <summary>Name (<c>proNAME</c>).</summary>
            public static readonly StringFilterProperty Name = new StringFilterProperty(Category, "proNAME", "albl_Name");

            /// <summary>A text user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static StringFilterProperty CustomString(string name) => new StringFilterProperty(Category, name);

            /// <summary>A numeric user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static NumericFilterProperty CustomNumber(string name) => new NumericFilterProperty(Category, name);

            /// <summary>A date user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static DateFilterProperty CustomDateTime(string name) => new DateFilterProperty(Category, name);

            /// <summary>A true / false user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static BooleanFilterProperty CustomBoolean(string name) => new BooleanFilterProperty(Category, name);
        }

        /// <summary>Properties of reference objects (<c>reference_object</c>), like <c>ReferenceObjectFilterExpressions</c>.</summary>
        public static class ReferenceObject
        {
            /// <summary>Category token.</summary>
            public const string Category = "reference_object";

            /// <summary>A text user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static StringFilterProperty CustomString(string name) => new StringFilterProperty(Category, name);

            /// <summary>A numeric user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static NumericFilterProperty CustomNumber(string name) => new NumericFilterProperty(Category, name);

            /// <summary>A date user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static DateFilterProperty CustomDateTime(string name) => new DateFilterProperty(Category, name);

            /// <summary>A true / false user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static BooleanFilterProperty CustomBoolean(string name) => new BooleanFilterProperty(Category, name);
        }

        /// <summary>Properties of reinforcement (<c>CoRebar.Group</c>), like <c>ReinforcingBarFilterExpressions</c>.</summary>
        public static class ReinforcingBar
        {
            /// <summary>Category token.</summary>
            public const string Category = "CoRebar.Group";

            /// <summary>Class (<c>proCLASS</c>).</summary>
            public static readonly NumericFilterProperty Class = new NumericFilterProperty(Category, "proCLASS", "albl_Class");

            /// <summary>Diameter (<c>proREBAR_DIAMETER</c>).</summary>
            public static readonly NumericFilterProperty Diameter = new NumericFilterProperty(Category, "proREBAR_DIAMETER", "albl_Diameter");

            /// <summary>Join type (<c>proSPLICE_TYPE</c>).</summary>
            public static readonly StringFilterProperty JoinType = new StringFilterProperty(Category, "proSPLICE_TYPE", "albl_Rebar_splice_joint_type");

            /// <summary>Length (<c>proLENGTH</c>).</summary>
            public static readonly NumericFilterProperty Length = new NumericFilterProperty(Category, "proLENGTH", "albl_Length");

            /// <summary>Material (<c>proMATERIAL</c>).</summary>
            public static readonly StringFilterProperty Material = new StringFilterProperty(Category, "proMATERIAL", "albl_Material");

            /// <summary>Name (<c>proNAME</c>).</summary>
            public static readonly StringFilterProperty Name = new StringFilterProperty(Category, "proNAME", "albl_Name");

            /// <summary>Numbering series (<c>proREBAR_NUMBERING_SERIES</c>).</summary>
            public static readonly StringFilterProperty NumberingSeries = new StringFilterProperty(Category, "proREBAR_NUMBERING_SERIES", "albl_Numbering_series");

            /// <summary>Phase (<c>proPHASE</c>).</summary>
            public static readonly NumericFilterProperty Phase = new NumericFilterProperty(Category, "proPHASE", "albl_Phase");

            /// <summary>Position (<c>proSPLICE_BAR_POSITION</c>).</summary>
            public static readonly StringFilterProperty Position = new StringFilterProperty(Category, "proSPLICE_BAR_POSITION", "albl_Bar_positions");

            /// <summary>Position number (<c>proNUMBERING_POSITION</c>).</summary>
            public static readonly StringFilterProperty PositionNumber = new StringFilterProperty(Category, "proNUMBERING_POSITION", "albl_Position_number");

            /// <summary>Prefix (<c>proSERIE</c>).</summary>
            public static readonly StringFilterProperty Prefix = new StringFilterProperty(Category, "proSERIE", "albl_Prefix");

            /// <summary>Shape (<c>REBAR.SHAPE</c>).</summary>
            public static readonly StringFilterProperty Shape = new StringFilterProperty(Category, "REBAR.SHAPE", "albl_Shape");

            /// <summary>Size (<c>proREBAR_SIZE</c>).</summary>
            public static readonly StringFilterProperty Size = new StringFilterProperty(Category, "proREBAR_SIZE", "albl_Size");

            /// <summary>Start number (<c>proSTARTNUMBER</c>).</summary>
            public static readonly NumericFilterProperty StartNumber = new NumericFilterProperty(Category, "proSTARTNUMBER", "albl_Start_number");

            /// <summary>A text user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static StringFilterProperty CustomString(string name) => new StringFilterProperty(Category, name);

            /// <summary>A numeric user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static NumericFilterProperty CustomNumber(string name) => new NumericFilterProperty(Category, name);

            /// <summary>A date user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static DateFilterProperty CustomDateTime(string name) => new DateFilterProperty(Category, name);

            /// <summary>A true / false user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static BooleanFilterProperty CustomBoolean(string name) => new BooleanFilterProperty(Category, name);
        }

        /// <summary>Properties of surface treatments (<c>co_surface_object</c>), like <c>SurfaceFilterExpressions</c>.</summary>
        public static class Surface
        {
            /// <summary>Category token.</summary>
            public const string Category = "co_surface_object";

            /// <summary>Class (<c>proCLASS</c>).</summary>
            public static readonly NumericFilterProperty Class = new NumericFilterProperty(Category, "proCLASS", "albl_Class");

            /// <summary>Name (<c>proNAME</c>).</summary>
            public static readonly StringFilterProperty Name = new StringFilterProperty(Category, "proNAME", "albl_Name");

            /// <summary>Type (<c>proGROUP_TYPE</c>).</summary>
            public static readonly StringFilterProperty Type = new StringFilterProperty(Category, "proGROUP_TYPE", "albl_Type");

            /// <summary>A text user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static StringFilterProperty CustomString(string name) => new StringFilterProperty(Category, name);

            /// <summary>A numeric user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static NumericFilterProperty CustomNumber(string name) => new NumericFilterProperty(Category, name);

            /// <summary>A date user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static DateFilterProperty CustomDateTime(string name) => new DateFilterProperty(Category, name);

            /// <summary>A true / false user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static BooleanFilterProperty CustomBoolean(string name) => new BooleanFilterProperty(Category, name);
        }

        /// <summary>Properties of tasks (<c>co_task</c>), like <c>TaskFilterExpressions</c>.</summary>
        public static class Task
        {
            /// <summary>Category token.</summary>
            public const string Category = "co_task";

            /// <summary>Actual end date (<c>proActualEndDate</c>).</summary>
            public static readonly DateFilterProperty ActualEndDate = new DateFilterProperty(Category, "proActualEndDate", "albl_Actual_end_date");

            /// <summary>Actual start date (<c>proActualStartDate</c>).</summary>
            public static readonly DateFilterProperty ActualStartDate = new DateFilterProperty(Category, "proActualStartDate", "albl_Actual_start_date");

            /// <summary>Completeness (<c>proTaskCompleteness</c>).</summary>
            public static readonly NumericFilterProperty Completeness = new NumericFilterProperty(Category, "proTaskCompleteness", "albl_Completeness");

            /// <summary>Critical (<c>proTaskCritical</c>).</summary>
            public static readonly StringFilterProperty Critical = new StringFilterProperty(Category, "proTaskCritical", "albl_Critical");

            /// <summary>Local (<c>proTaskLocal</c>).</summary>
            public static readonly StringFilterProperty Local = new StringFilterProperty(Category, "proTaskLocal", "albl_Local");

            /// <summary>Name (<c>proNAME</c>).</summary>
            public static readonly StringFilterProperty Name = new StringFilterProperty(Category, "proNAME", "albl_Name");

            /// <summary>Planned end date (<c>proPlannedEndDate</c>).</summary>
            public static readonly DateFilterProperty PlannedEndDate = new DateFilterProperty(Category, "proPlannedEndDate", "albl_Planned_end_date");

            /// <summary>Planned start date (<c>proPlannedStartDate</c>).</summary>
            public static readonly DateFilterProperty PlannedStartDate = new DateFilterProperty(Category, "proPlannedStartDate", "albl_Planned_start_date");

            /// <summary>A text user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static StringFilterProperty CustomString(string name) => new StringFilterProperty(Category, name);

            /// <summary>A numeric user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static NumericFilterProperty CustomNumber(string name) => new NumericFilterProperty(Category, name);

            /// <summary>A date user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static DateFilterProperty CustomDateTime(string name) => new DateFilterProperty(Category, name);

            /// <summary>A true / false user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static BooleanFilterProperty CustomBoolean(string name) => new BooleanFilterProperty(Category, name);
        }

        /// <summary>Properties of template attributes (<c>TemplateCategory</c>), like <c>TemplateFilterExpressions</c>.</summary>
        public static class Template
        {
            /// <summary>Category token.</summary>
            public const string Category = "TemplateCategory";

            /// <summary>A text user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static StringFilterProperty CustomString(string name) => new StringFilterProperty(Category, name);

            /// <summary>A numeric user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static NumericFilterProperty CustomNumber(string name) => new NumericFilterProperty(Category, name);

            /// <summary>A date user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static DateFilterProperty CustomDateTime(string name) => new DateFilterProperty(Category, name);

            /// <summary>A true / false user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static BooleanFilterProperty CustomBoolean(string name) => new BooleanFilterProperty(Category, name);
        }

        /// <summary>Properties of welds (<c>co_weld</c>), like <c>WeldFilterExpressions</c>.</summary>
        public static class Weld
        {
            /// <summary>Category token.</summary>
            public const string Category = "co_weld";

            /// <summary>Phase (<c>proPHASE</c>).</summary>
            public static readonly NumericFilterProperty Phase = new NumericFilterProperty(Category, "proPHASE", "albl_Phase");

            /// <summary>Position number (<c>proNUMBERING_POSITION</c>).</summary>
            public static readonly StringFilterProperty PositionNumber = new StringFilterProperty(Category, "proNUMBERING_POSITION", "albl_Position_number");

            /// <summary>Reference text (<c>proText</c>).</summary>
            public static readonly StringFilterProperty ReferenceText = new StringFilterProperty(Category, "proText", "albl_Reference_text");

            /// <summary>Size above line (<c>proSIZE</c>).</summary>
            public static readonly NumericFilterProperty SizeAboveLine = new NumericFilterProperty(Category, "proSIZE", "albl_Size_above_line");

            /// <summary>Size below line (<c>proSIZE2</c>).</summary>
            public static readonly NumericFilterProperty SizeBelowLine = new NumericFilterProperty(Category, "proSIZE2", "albl_Size_below_line");

            /// <summary>Type above line (<c>proTYPE</c>).</summary>
            public static readonly NumericFilterProperty TypeAboveLine = new NumericFilterProperty(Category, "proTYPE", "albl_Type_above_line");

            /// <summary>Type below line (<c>proTYPE2</c>).</summary>
            public static readonly NumericFilterProperty TypeBelowLine = new NumericFilterProperty(Category, "proTYPE2", "albl_Type_below_line");

            /// <summary>Welding site (<c>proWELDING_SITE</c>).</summary>
            public static readonly NumericFilterProperty WeldingSite = new NumericFilterProperty(Category, "proWELDING_SITE", "albl_Welding_site");

            /// <summary>A text user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static StringFilterProperty CustomString(string name) => new StringFilterProperty(Category, name);

            /// <summary>A numeric user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static NumericFilterProperty CustomNumber(string name) => new NumericFilterProperty(Category, name);

            /// <summary>A date user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static DateFilterProperty CustomDateTime(string name) => new DateFilterProperty(Category, name);

            /// <summary>A true / false user-defined attribute, e.g. <c>USER_FIELD_1</c>.</summary>
            public static BooleanFilterProperty CustomBoolean(string name) => new BooleanFilterProperty(Category, name);
        }
    }
}
