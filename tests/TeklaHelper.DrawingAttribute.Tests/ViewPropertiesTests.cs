using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TeklaHelper.DrawingAttribute.Filters;
using TeklaHelper.DrawingAttribute.Pages;
using TeklaHelper.DrawingAttribute.Views;
using Xunit;

namespace TeklaHelper.DrawingAttribute.Tests
{
    public sealed class ViewPropertiesTests : IDisposable
    {
        private readonly string _directory = Path.Combine(Path.GetTempPath(), "TeklaHelper.DrawingAttribute.Tests", Guid.NewGuid().ToString("N"));

        public void Dispose()
        {
            if (Directory.Exists(_directory))
            {
                Directory.Delete(_directory, recursive: true);
            }
        }

        /// <summary>
        /// Every page of the Tekla View properties dialog, its section in the .vi file and the extension its Save button
        /// writes (checked against files saved from the Tekla 2025 dialogs, see <see cref="SubDialogFiles_MatchTheTreeOfALoadedView"/>).
        /// </summary>
        public static IEnumerable<object[]> Pages()
        {
            yield return Page(v => v.Attributes, ViewAttributes.ViewPrefix, null);
            yield return Page(v => v.Attributes.LabelMarkA1, "ViewLabelMarkA1.", null);
            yield return Page(v => v.Attributes.LabelMarkA5, "ViewLabelMarkA5.", null);
            yield return Page(v => v.Protection, "Protection.", null);
            yield return Page(v => v.Dimensioning, "ViewDimensioning.", null);
            yield return Page(v => v.Marks.PartMark, "vpm.", PartMark.ViewFileExtension);
            yield return Page(v => v.Marks.BoltMark, "vsm.", BoltMark.ViewFileExtension);
            yield return Page(v => v.Marks.NeighborPartMark, "vnpm.", NeighborPartMark.ViewFileExtension);
            yield return Page(v => v.Marks.SurfaceTreatmentMark, "vsurfm.", SurfaceTreatmentMark.ViewFileExtension);
            yield return Page(v => v.Marks.ConnectionMark, "vjm.", ConnectionMark.ViewFileExtension);
            yield return Page(v => v.Marks.WeldMark, "vwel.", WeldMark.ViewFileExtension);
            yield return Page(v => v.Marks.ReinforcementMark, "vrm.", ReinforcementMark.ViewFileExtension);
            yield return Page(v => v.Marks.NeighborReinforcementMark, "NeighborRebarMark.", NeighborReinforcementMark.ViewFileExtension);
            yield return Page(v => v.Marks.PourMark, "PourMark.", null);
            yield return Page(v => v.Objects.Part, "vp.", PartObject.ViewFileExtension);
            yield return Page(v => v.Objects.Bolt, "vs.", BoltObject.ViewFileExtension);
            yield return Page(v => v.Objects.NeighborPart, "vnp.", NeighborPartObject.ViewFileExtension);
            yield return Page(v => v.Objects.SurfaceTreatment, "vsurf.", SurfaceTreatmentObject.ViewFileExtension);
            yield return Page(v => v.Objects.Weld, "ViewWeld.", WeldObject.ViewFileExtension);
            yield return Page(v => v.Objects.Reinforcement, "vrebar.", ReinforcementObject.ViewFileExtension);
            yield return Page(v => v.Objects.ReferenceObject, "vref.", ReferenceObject.ViewFileExtension);
            yield return Page(v => v.Objects.Grid, "vg.", GridObject.ViewFileExtension);
            yield return Page(v => v.Objects.PourBreak, "PourBreaks.", PourBreakObject.ViewFileExtension);
            yield return Page(v => v.Objects.NeighborReinforcement, "NeighborRebar.", NeighborReinforcementObject.ViewFileExtension);
            yield return Page(v => v.Objects.PourObject, "PourObjects.", PourObject.ViewFileExtension);
            yield return Page(v => v.Filter, "vfilter.", null);
            yield return Page(v => v.NeighborPartFilter, "vnp_filter.", null);
        }

        [Theory]
        [MemberData(nameof(Pages))]
        public void EveryPage_UsesItsPrefix(Func<ViewProperties, AttributeSection> page, string prefix, string extension)
        {
            var view = new ViewProperties();

            Assert.Equal(prefix, page(view).Prefix);
            Assert.True(extension == null || extension.StartsWith(".v", StringComparison.Ordinal));
        }

        [Fact]
        public void TypedProperties_WriteTheKeysOfTheirPage()
        {
            var view = new ViewProperties();
            view.Attributes.Attributes1.View.Scale = 25;
            view.Marks.WeldMark.Content.Visibility.Welds = WeldMarkVisibility.Site;
            view.Objects.Weld.Appearance.VisibleLines.Color = DrawingColor.Red;
            view.Objects.Reinforcement.BarAppearance.VisibleLines.Color = DrawingColor.Blue;
            view.Objects.NeighborReinforcement.MeshAppearance.VisibleLines.Type = DrawingLineType.Dashed;
            view.Objects.SurfaceTreatment.Content.ShowPattern = true;
            view.Objects.ReferenceObject.Appearance.VisibleLines.Color = DrawingColor.Green;
            view.Objects.PourBreak.Content.HiddenLines = true;
            view.Protection.TextPart = true;
            view.Filter.FilteringType = 1;

            Assert.Equal(
                new[]
                {
                    "view_scale 25.000000", "vwel.create 1", "ViewWeld.Colour 160", "vrebar.BarAttr.line_colour 162",
                    "NeighborRebar.MeshAttr.line_type 2", "vsurf.show_pattern 1", "vref.Colour 161", "PourBreaks.HiddenLines 1",
                    "Protection.part_prot_text 1", "vfilter.filtering_type 1",
                },
                view.Values.Select(p => p.Key + " " + p.Value.ToFileText()));
        }

        [Fact]
        public void Filters_AreSavedAsCompanionFiles()
        {
            var view = new ViewProperties();
            view.Filter.SetExpression(FilterProperties.Part.Name.IsEqual("BEAM"));
            view.NeighborPartFilter.SetExpression(FilterProperties.Part.Name.IsEqual("COLUMN"));

            view.SaveTo(_directory, "v");
            var loaded = AttributeFile.FromFile<ViewProperties>(Path.Combine(_directory, "v.vi"));

            Assert.Equal("BEAM", loaded.Filter.Rows.Single().Value);
            Assert.Equal("COLUMN", loaded.NeighborPartFilter.Rows.Single().Value);
        }

        /// <summary>
        /// TEKLA_VIEW_DIALOG_SAMPLES names a folder where a view was saved from the Tekla dialog together with its pages
        /// under the same name (like the TEST.* files in D:\Tekla\Models\New model\attributes). Each page file must hold
        /// the same keys and values as the matching node of the loaded .vi. Files shipped with Tekla share names without
        /// being saved together, so this does not run on the environment folders.
        /// </summary>
        [Fact]
        public void SubDialogFiles_MatchTheTreeOfALoadedView()
        {
            string root = Environment.GetEnvironmentVariable("TEKLA_VIEW_DIALOG_SAMPLES");
            if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
            {
                return;
            }

            var failures = new List<string>();
            foreach (string viPath in Directory.EnumerateFiles(root, "*.vi", SearchOption.AllDirectories))
            {
                ViewProperties view;
                try
                {
                    // A legacy .vi (no encoding line) was not saved together with the current sub-dialog files next to it
                    if (!AttributeFileFormat.Read(viPath).HasEncodingLine)
                    {
                        continue;
                    }

                    view = AttributeFile.FromFile<ViewProperties>(viPath);
                }
                catch (FormatException)
                {
                    continue;
                }

                string basePath = viPath.Substring(0, viPath.Length - ".vi".Length);
                foreach (object[] page in Pages())
                {
                    var extension = (string)page[2];
                    if (extension == null || !File.Exists(basePath + extension))
                    {
                        continue;
                    }

                    AttributeSection section = ((Func<ViewProperties, AttributeSection>)page[0])(view);
                    AttributeFileContent file = AttributeFileFormat.Read(basePath + extension);

                    // Legacy files (no encoding line, keys like vpm_dial.for_main) shipped next to a .vi of the same name
                    // were not saved together with it
                    if (!file.HasEncodingLine)
                    {
                        continue;
                    }

                    foreach (KeyValuePair<string, AttributeValue> pair in file.Values)
                    {
                        // The file names differ by design: the host records which file was loaded into the page
                        if (pair.Key.IndexOf("in_file", StringComparison.OrdinalIgnoreCase) >= 0 || pair.Key.IndexOf("InFile", StringComparison.Ordinal) >= 0
                            || pair.Key.IndexOf("out_file", StringComparison.OrdinalIgnoreCase) >= 0 || pair.Key.IndexOf("OutFile", StringComparison.Ordinal) >= 0)
                        {
                            continue;
                        }

                        // Mark contents are XML that Tekla writes anew on every save (line breaks in the .vi file, attributes
                        // such as hidden="0" added by newer saves), so files saved minutes apart differ there
                        if (pair.Key.IndexOf("ContentString", StringComparison.Ordinal) >= 0)
                        {
                            continue;
                        }

                        if (!section.TryGetValue(pair.Key, out AttributeValue value) || value.RawText != pair.Value.RawText)
                        {
                            failures.Add($"{basePath}{extension}: {pair.Key} = {pair.Value.RawText}, view {section.Prefix}{pair.Key} = {(section.Contains(pair.Key) ? value.RawText : "<missing>")}");
                        }
                    }
                }
            }

            Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures.Take(20)));
        }

        private static object[] Page(Func<ViewProperties, AttributeSection> page, string prefix, string extension) =>
            new object[] { page, prefix, extension };
    }
}
