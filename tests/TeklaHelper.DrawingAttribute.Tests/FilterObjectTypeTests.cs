using System;
using System.Linq;
using TeklaHelper.DrawingAttribute.Filters;
using Xunit;

namespace TeklaHelper.DrawingAttribute.Tests
{
    /// <summary>
    /// The label keys are the object type list of TeklaModel.dll 2025; the rows are those of shipped filter files
    /// (e.g. <c>co_object proOBJECT_TYPE albl_ObjectType != albl_DoesNotEqual albl_BoltGroup</c>).
    /// </summary>
    public sealed class FilterObjectTypeTests
    {
        [Theory]
        [InlineData(FilterObjectType.Part, "albl_Part")]
        [InlineData(FilterObjectType.Assembly, "albl_Assembly")]
        [InlineData(FilterObjectType.SurfaceTreatment, "albl_Surfacing")]
        [InlineData(FilterObjectType.Surface, "albl_Surface_object")]
        [InlineData(FilterObjectType.PourObject, "albl_Pour_object")]
        [InlineData(FilterObjectType.PourBreak, "albl_Pour_break")]
        [InlineData(FilterObjectType.PourUnit, "albl_pour_unit")]
        [InlineData(FilterObjectType.BoltGroup, "albl_BoltGroup")]
        [InlineData(FilterObjectType.Component, "albl_Joint")]
        [InlineData(FilterObjectType.Weld, "albl_Weld")]
        [InlineData(FilterObjectType.ReferenceObject, "albl_Reference_object")]
        [InlineData(FilterObjectType.ReinforcingBar, "albl_Reinforcement_bar")]
        public void ToFilterValue_IsTheLabelKeyAndParsesBack(FilterObjectType type, string value)
        {
            Assert.Equal(value, type.ToFilterValue());
            Assert.True(FilterObjectTypes.TryParse(value, out FilterObjectType parsed));
            Assert.Equal(type, parsed);
        }

        [Fact]
        public void All_ListsEveryValueOnce()
        {
            Assert.Equal(Enum.GetValues(typeof(FilterObjectType)).Cast<FilterObjectType>(), FilterObjectTypes.All);
            Assert.Equal(12, FilterObjectTypes.All.Select(t => t.ToFilterValue()).Distinct(StringComparer.Ordinal).Count());
        }

        [Fact]
        public void IsNotEqual_WritesTheRowOfTheDialog()
        {
            FilterFile file = FilterFile.FromExpression(
                FilterFileExtensions.ViewFilter,
                FilterProperties.Object.Type.IsNotEqual(FilterObjectType.ReinforcingBar));

            FilterRow row = Assert.Single(file.Rows);
            Assert.Equal(
                new[] { "co_object", "proOBJECT_TYPE", "albl_ObjectType", "!=", "albl_DoesNotEqual", "albl_Reinforcement_bar" },
                new[] { row.Category, row.Property, row.PropertyLabel, row.Condition, row.ConditionLabel, row.Value });
            Assert.True(FilterObjectTypes.TryParse(file.ToExpression() is FilterCondition c ? c.Value : null, out FilterObjectType type));
            Assert.Equal(FilterObjectType.ReinforcingBar, type);
        }

        [Fact]
        public void IsEqual_WritesTheEqualsRow()
        {
            FilterCondition condition = FilterProperties.Object.Type.IsEqual(FilterObjectType.BoltGroup);

            Assert.Equal("==", condition.Operator.Symbol);
            Assert.Equal("albl_Equals", condition.Operator.Label);
            Assert.Equal("albl_BoltGroup", condition.Value);
        }

        [Fact]
        public void SeveralTypes_AreSeparatedBySpaces()
        {
            // Tekla 2025 selected both parts and bolts with "albl_Part albl_BoltGroup", and with != on reinforcing bars and
            // welds every part and bolt but no bar or weld
            Assert.Equal("albl_Part albl_BoltGroup", FilterProperties.Object.Type.IsEqual(FilterObjectType.Part, FilterObjectType.BoltGroup).Value);
            Assert.Equal("albl_Reinforcement_bar albl_Weld", FilterProperties.Object.Type.IsNotEqual(FilterObjectType.ReinforcingBar, FilterObjectType.Weld).Value);
            Assert.False(FilterObjectTypes.TryParse("albl_Part albl_BoltGroup", out _));
            Assert.Throws<ArgumentException>(() => FilterProperties.Object.Type.IsEqual());
            Assert.Throws<ArgumentException>(() => FilterProperties.Object.Type.IsNotEqual((FilterObjectType[])null));
        }

        [Fact]
        public void NumericOverloads_StillWriteNumbers()
        {
            Assert.Equal("2", FilterProperties.Object.Type.IsEqual(2).Value);
        }

        [Fact]
        public void OtherNumericProperties_AreRejected()
        {
            Assert.Throws<ArgumentException>(() => FilterProperties.Object.Phase.IsEqual(FilterObjectType.Part));
            Assert.Throws<ArgumentException>(() => FilterProperties.ConstructionObject.Type.IsNotEqual(FilterObjectType.Part));
            Assert.Throws<ArgumentNullException>(() => ((NumericFilterProperty)null).IsEqual(FilterObjectType.Part));
        }

        [Theory]
        [InlineData("2")]
        [InlineData("Empty")]
        [InlineData("albl_part")]
        [InlineData("")]
        [InlineData(null)]
        public void TryParse_RejectsOtherValues(string value)
        {
            Assert.False(FilterObjectTypes.TryParse(value, out _));
        }

        [Fact]
        public void ToFilterValue_RejectsUndefinedValues()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ((FilterObjectType)12).ToFilterValue());
            Assert.Throws<ArgumentOutOfRangeException>(() => ((FilterObjectType)(-1)).ToFilterValue());
        }
    }
}
