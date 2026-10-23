using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TeklaHelper.DrawingAttribute.Filters
{
    /// <summary>
    /// The values of the Object type row of the Tekla filter dialog (<see cref="FilterProperties.Object.Type"/>):
    /// <code>
    /// FilterProperties.Object.Type.IsNotEqual(FilterObjectType.ReinforcingBar)   // every object but reinforcing bars
    /// </code>
    /// The dialog writes a label key (<c>albl_Reinforcement_bar</c>), see <see cref="FilterObjectTypes"/>. The keys are the
    /// list the filter of Tekla Structures 2025 evaluates (<c>TeklaModel.dll</c>), the same as in the filter files
    /// shipped with Tekla.
    /// </summary>
    public enum FilterObjectType
    {
        /// <summary>Parts (<c>albl_Part</c>).</summary>
        Part,

        /// <summary>Assemblies (<c>albl_Assembly</c>).</summary>
        Assembly,

        /// <summary>Surface treatments (<c>albl_Surfacing</c>).</summary>
        SurfaceTreatment,

        /// <summary>Surface objects (<c>albl_Surface_object</c>).</summary>
        Surface,

        /// <summary>Pour objects (<c>albl_Pour_object</c>).</summary>
        PourObject,

        /// <summary>Pour breaks (<c>albl_Pour_break</c>).</summary>
        PourBreak,

        /// <summary>Pour units (<c>albl_pour_unit</c>).</summary>
        PourUnit,

        /// <summary>Bolt groups (<c>albl_BoltGroup</c>).</summary>
        BoltGroup,

        /// <summary>
        /// Components (<c>albl_Joint</c>). In Tekla 2025 a connection (Bent plate) matched, plug-ins did not.
        /// </summary>
        Component,

        /// <summary>Welds (<c>albl_Weld</c>).</summary>
        Weld,

        /// <summary>Reference objects (<c>albl_Reference_object</c>).</summary>
        ReferenceObject,

        /// <summary>Reinforcing bars (<c>albl_Reinforcement_bar</c>).</summary>
        ReinforcingBar,
    }

    /// <summary>
    /// Conditions on the object type, and the label keys the Tekla filter dialog writes for <see cref="FilterObjectType"/>.
    /// </summary>
    public static class FilterObjectTypes
    {
        private static readonly string[] Values =
        {
            "albl_Part",
            "albl_Assembly",
            "albl_Surfacing",
            "albl_Surface_object",
            "albl_Pour_object",
            "albl_Pour_break",
            "albl_pour_unit",
            "albl_BoltGroup",
            "albl_Joint",
            "albl_Weld",
            "albl_Reference_object",
            "albl_Reinforcement_bar",
        };

        /// <summary>Every object type in the order of the list Tekla evaluates.</summary>
        public static IReadOnlyList<FilterObjectType> All { get; } =
            new ReadOnlyCollection<FilterObjectType>(Enumerable.Range(0, Values.Length).Select(i => (FilterObjectType)i).ToArray());

        /// <summary>
        /// The object type is one of <paramref name="types"/>, written as the dialog writes it (several values separated
        /// by spaces): <c>FilterProperties.Object.Type.IsEqual(FilterObjectType.Part, FilterObjectType.BoltGroup)</c>.
        /// </summary>
        /// <param name="property"><see cref="FilterProperties.Object.Type"/>.</param>
        /// <param name="types">One or more object types.</param>
        /// <exception cref="ArgumentException"><paramref name="property"/> is not the object type of <c>co_object</c>, or
        /// <paramref name="types"/> is empty.</exception>
        public static FilterCondition IsEqual(this NumericFilterProperty property, params FilterObjectType[] types) =>
            Check(property).Is(FilterOperator.Equal, Join(types));

        /// <summary>
        /// The object type is none of <paramref name="types"/>, written as the dialog writes it:
        /// <c>FilterProperties.Object.Type.IsNotEqual(FilterObjectType.ReinforcingBar)</c>.
        /// </summary>
        /// <param name="property"><see cref="FilterProperties.Object.Type"/>.</param>
        /// <param name="types">One or more object types.</param>
        /// <exception cref="ArgumentException"><paramref name="property"/> is not the object type of <c>co_object</c>, or
        /// <paramref name="types"/> is empty.</exception>
        public static FilterCondition IsNotEqual(this NumericFilterProperty property, params FilterObjectType[] types) =>
            Check(property).Is(FilterOperator.NotEqual, Join(types));

        /// <summary>The label key the dialog writes for <paramref name="type"/>, e.g. <c>albl_Reinforcement_bar</c>.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="type"/> is not a defined value.</exception>
        public static string ToFilterValue(this FilterObjectType type)
        {
            int index = (int)type;
            if (index < 0 || index >= Values.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(type), type, "Not an object type of the filter dialog.");
            }

            return Values[index];
        }

        /// <summary>
        /// Reads one value of an object type row (<see cref="FilterCondition.Value"/>, <see cref="FilterRow.Value"/>);
        /// false for numbers, several values and other values.
        /// </summary>
        /// <param name="value">Value of the row, e.g. <c>albl_Part</c>.</param>
        /// <param name="type">The object type when the value is one of the label keys.</param>
        public static bool TryParse(string value, out FilterObjectType type)
        {
            int index = Array.IndexOf(Values, value);
            type = (FilterObjectType)Math.Max(index, 0);
            return index >= 0;
        }

        private static string Join(FilterObjectType[] types)
        {
            if (types == null || types.Length == 0)
            {
                throw new ArgumentException("Give at least one object type.", nameof(types));
            }

            return string.Join(" ", types.Select(ToFilterValue));
        }

        private static NumericFilterProperty Check(NumericFilterProperty property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (property.Category != FilterProperties.Object.Category || property.Name != FilterProperties.Object.Type.Name)
            {
                throw new ArgumentException($"Object types are values of FilterProperties.Object.Type, not of {property}.", nameof(property));
            }

            return property;
        }
    }
}
