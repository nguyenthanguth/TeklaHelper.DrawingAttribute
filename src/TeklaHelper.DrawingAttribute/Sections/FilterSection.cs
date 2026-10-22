using System.Collections.Generic;
using TeklaHelper.DrawingAttribute.Filters;

namespace TeklaHelper.DrawingAttribute.Sections
{
    /// <summary>
    /// A filter of a view or drawing, as in the Filter / Neighbor part filter dialogs: the settings in the attribute file
    /// (<c>vfilter.*</c>, <c>vnp_filter.*</c>, <c>dfilter.*</c>, <c>dnp_filter.*</c>) and the filter rows, saved as the
    /// companion file <c>name.vi.filter</c>, <c>name.vi.npfilter</c>, <c>name.gd.filter</c> or <c>name.gd.npfilter</c>.
    /// </summary>
    public sealed class FilterSection : AttributeSection
    {
        /// <summary>
        /// Creates the filter section of a file.
        /// </summary>
        /// <param name="values">Values of the file.</param>
        /// <param name="prefix">Prefix of the settings, e.g. <c>"vfilter."</c>.</param>
        /// <param name="fileExtension">Extension of the companion file with the rows, e.g. <c>.vi.filter</c>.</param>
        public FilterSection(AttributeCollection values, string prefix, string fileExtension)
            : base(values, prefix)
        {
            File = new FilterFile(fileExtension);
        }

        /// <summary>The filter rows, loaded and saved with the attribute file.</summary>
        public FilterFile File { get; }

        /// <summary>The filter rows (<see cref="FilterFile.Rows"/>).</summary>
        public IList<FilterRow> Rows => File.Rows;

        /// <summary>Filtering type number (<c>filtering_type</c>).</summary>
        public int FilteringType
        {
            get => GetInt32("filtering_type");
            set => SetInt32("filtering_type", value);
        }

        /// <summary>Replaces the rows with <paramref name="expression"/> (<see cref="FilterFile.SetExpression"/>).</summary>
        public void SetExpression(FilterExpression expression) => File.SetExpression(expression);

        /// <summary>Returns the rows as an expression tree (<see cref="FilterFile.ToExpression"/>).</summary>
        public FilterExpression ToExpression() => File.ToExpression();
    }
}
