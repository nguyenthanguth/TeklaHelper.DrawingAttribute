using System.Collections.Generic;

namespace DialogClassGenerator
{
    /// <summary>
    /// A tab, group box or sub-dialog of a page: a nested class with a property on its parent.
    /// </summary>
    internal sealed class Container
    {
        public string Label { get; set; }

        /// <summary>Tab, Group, Dialog or Page.</summary>
        public string Kind { get; set; }

        public string ClassName { get; set; }

        public string PropertyName { get; set; }

        public string Description { get; set; }

        public List<object> Items { get; } = new List<object>();
    }

    /// <summary>
    /// A control of the dialog that writes one key: a property.
    /// </summary>
    internal sealed class Control
    {
        public string Label { get; set; }

        public string Key { get; set; }

        public string Type { get; set; }

        public string EnableKey { get; set; }

        public List<Option> Options { get; } = new List<Option>();

        public int? ValueOn { get; set; }

        public int? ValueOff { get; set; }

        public int? DialogDefault { get; set; }

        public string Confidence { get; set; }

        public string Format { get; set; }

        /// <summary>Why the control is in the Advanced group: where the dialog shows it, if anywhere.</summary>
        public string Origin { get; set; }

        /// <summary>For a control of another dialog with the same keys: that dialog (and panel).</summary>
        public string AidDialog { get; set; }

        public double Position { get; set; }

        /// <summary>Values found in the sample files that are items of the menu: value → number of files.</summary>
        public Dictionary<int, int> SamplesInMenu { get; } = new Dictionary<int, int>();

        /// <summary>Values found in the sample files that are not items of the menu: value → number of files.</summary>
        public Dictionary<int, int> SamplesOutsideMenu { get; } = new Dictionary<int, int>();

        // Resolved by the generator
        public string PropertyName { get; set; }

        public string ClrType { get; set; }

        public string EnumName { get; set; }

        public int BoolOn { get; set; } = 1;

        public int BoolOff { get; set; }
    }

    internal sealed class Option
    {
        public string Label { get; set; }

        public int Value { get; set; }

        public string ApiName { get; set; }

        public string Bitmap { get; set; }

        /// <summary>How the value was found, when it is not certain.</summary>
        public string Note { get; set; }
    }

    internal sealed class EnumModel
    {
        /// <summary>
        /// Members that are not items of the menu but are common in files saved by Tekla, with the controls (page: key) whose
        /// files hold them: an enum shared by several pages gets the values of each, and its comment must say whose they are.
        /// </summary>
        public Dictionary<int, SortedSet<string>> FromFiles { get; } = new Dictionary<int, SortedSet<string>>();

        public string Name { get; set; }

        public string Description { get; set; }

        public List<(string Name, int Value, string Label)> Members { get; } = new List<(string, int, string)>();
    }
}
