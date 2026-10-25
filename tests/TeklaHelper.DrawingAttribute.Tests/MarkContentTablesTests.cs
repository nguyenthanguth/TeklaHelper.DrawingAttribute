using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TeklaHelper.DrawingAttribute.DrawingObjects;
using TeklaHelper.DrawingAttribute.Marks;
using TeklaHelper.DrawingAttribute.Pages;
using TeklaHelper.DrawingAttribute.Views;
using Xunit;

namespace TeklaHelper.DrawingAttribute.Tests
{
    /// <summary>The content table each kind of object shows, as Tekla 2025 read them (see CLAUDE.md).</summary>
    public sealed class MarkContentTablesTests
    {
        private static MarkContent Named(string text) => new MarkContent().AddText(text);

        private static void AssertTable(AttributeSection section, string key, Func<MarkContent> get, Action<MarkContent> set)
        {
            set(Named(key));

            Assert.Equal(key, Assert.IsType<MarkTextElement>(Assert.Single(section.GetMarkContent(key))).Text);
            Assert.Equal(key, Assert.IsType<MarkTextElement>(Assert.Single(get())).Text);

            set(null);
            Assert.Equal(string.Empty, section.GetString(key));
            Assert.Null(get());
        }

        [Fact]
        public void PartMark_TablesByKindOfPart()
        {
            PartMark page = new ViewProperties().Marks.PartMark;

            AssertTable(page, "ContentString1", () => page.Content.MainPartElements, c => page.Content.MainPartElements = c);
            AssertTable(page, "ContentString2", () => page.Content.SecondaryPartElements, c => page.Content.SecondaryPartElements = c);
            AssertTable(page, "ContentString3", () => page.Content.SubAssemblyMainPartElements, c => page.Content.SubAssemblyMainPartElements = c);
            AssertTable(page, "ContentString4", () => page.Content.SubAssemblySecondaryPartElements, c => page.Content.SubAssemblySecondaryPartElements = c);
        }

        [Fact]
        public void BoltMark_TablesBySiteAndWorkshop()
        {
            BoltMark page = new ViewProperties().Marks.BoltMark;

            AssertTable(page, "ContentString1", () => page.Content.SiteBoltElements, c => page.Content.SiteBoltElements = c);
            AssertTable(page, "ContentString2", () => page.Content.WorkshopBoltElements, c => page.Content.WorkshopBoltElements = c);
        }

        [Fact]
        public void ReinforcementMark_TablesBySingleGroupMergedMesh()
        {
            ReinforcementMark page = new ViewProperties().Marks.ReinforcementMark;

            AssertTable(page, "ContentString1", () => page.Content.SingleElements, c => page.Content.SingleElements = c);
            AssertTable(page, "ContentString2", () => page.Content.GroupElements, c => page.Content.GroupElements = c);
            AssertTable(page, "ContentString3", () => page.Merging.MergedMarkElements, c => page.Merging.MergedMarkElements = c);
            AssertTable(page, "ContentString4", () => page.Content.MeshElements, c => page.Content.MeshElements = c);
        }

        [Fact]
        public void AssociativeNote_TablesByKindOfObject()
        {
            AssociativeNoteProperties page = AssociativeNoteAttributeFile.CreateDefault().Properties;

            AssertTable(page, "ContentString1", () => page.Content.PartElements, c => page.Content.PartElements = c);
            AssertTable(page, "ContentString2", () => page.Content.BoltElements, c => page.Content.BoltElements = c);
            AssertTable(page, "ContentString3", () => page.Content.SurfaceTreatmentElements, c => page.Content.SurfaceTreatmentElements = c);
            AssertTable(page, "ContentString4", () => page.Content.ReinforcementElements, c => page.Content.ReinforcementElements = c);
            AssertTable(page, "ReferenceObjectContentString", () => page.Content.ReferenceObjectElements, c => page.Content.ReferenceObjectElements = c);
        }

        [Fact]
        public void Defaults_AreMarkContentOfTheirKind()
        {
            AssociativeNoteProperties note = AssociativeNoteAttributeFile.CreateDefault().Properties;

            Assert.Equal(
                new[] { MarkElementTypes.PartMark.PartPosition, MarkElementTypes.PartMark.Profile },
                note.Content.PartElements.OfType<MarkPropertyElement>().Select(e => e.Type));
            Assert.True(Assert.Single(note.Content.BoltElements.OfType<MarkPropertyElement>()).Type.IsSizeElement);
            Assert.Contains(note.Content.ReinforcementElements, e => e is MarkXmlElement x && x.Tag == "PullOut");
            Assert.Null(note.Advanced.PourBreakElements);
            Assert.Equal(string.Empty, note.GetString("PourBreakContentString", null));
            Assert.Null(new ViewProperties().Marks.PartMark.Advanced.PourElements);
            Assert.All(note.Content.PartElements.OfType<MarkPropertyElement>(), e => Assert.Equal(DrawingColor.DarkGreen, e.Format.Color));

            Assert.Null(ViewProperties.CreateDefault().Marks.PartMark.Advanced.PourElements);

            // The pour mark keeps its own table, which its dialog edits
            Assert.Equal(MarkElementTypes.PourMark.Material, Assert.IsType<MarkPropertyElement>(Assert.Single(ViewProperties.CreateDefault().Marks.PourMark.Content.PourElements)).Type);
            PourMarkAttributeFile pourMark = PourMarkAttributeFile.CreateDefault();
            Assert.Equal(MarkElementTypes.PourMark.Material, Assert.IsType<MarkPropertyElement>(Assert.Single(pourMark.Properties.GetMarkContent("PourContentString"))).Type);
        }

        [Fact]
        public void OtherMarkPages_TablesByKindOfObject()
        {
            var view = new ViewProperties();

            NeighborPartMark neighbor = view.Marks.NeighborPartMark;
            AssertTable(neighbor, "ContentString1", () => neighbor.Content.MainPartElements, c => neighbor.Content.MainPartElements = c);
            AssertTable(neighbor, "ContentString2", () => neighbor.Content.SecondaryPartElements, c => neighbor.Content.SecondaryPartElements = c);

            NeighborReinforcementMark neighborRebar = view.Marks.NeighborReinforcementMark;
            AssertTable(neighborRebar, "ContentString1", () => neighborRebar.Content.SingleElements, c => neighborRebar.Content.SingleElements = c);
            AssertTable(neighborRebar, "ContentString2", () => neighborRebar.Content.GroupElements, c => neighborRebar.Content.GroupElements = c);
            AssertTable(neighborRebar, "ContentString3", () => neighborRebar.Merging.MergedMarkElements, c => neighborRebar.Merging.MergedMarkElements = c);
            AssertTable(neighborRebar, "ContentString4", () => neighborRebar.Content.MeshElements, c => neighborRebar.Content.MeshElements = c);

            ConnectionMark connection = view.Marks.ConnectionMark;
            AssertTable(connection, "ContentString1", () => connection.Content.Elements, c => connection.Content.Elements = c);
            SurfaceTreatmentMark surface = view.Marks.SurfaceTreatmentMark;
            AssertTable(surface, "ContentString1", () => surface.Content.Elements, c => surface.Content.Elements = c);

            DimensionMark tag = DimensionAttributeFile.CreateDefault().LeftUpperTagMark;
            AssertTable(tag, "ContentString1", () => tag.PartElements, c => tag.PartElements = c);
            AssertTable(tag, "ContentString2", () => tag.BoltElements, c => tag.BoltElements = c);
            AssertTable(tag, "ContentString3", () => tag.SurfaceTreatmentElements, c => tag.SurfaceTreatmentElements = c);
            AssertTable(tag, "ContentString4", () => tag.ReinforcementElements, c => tag.ReinforcementElements = c);
        }

        [Theory]
        [InlineData(typeof(PartMark.ContentTab), "MainPartElements SecondaryPartElements SubAssemblyMainPartElements SubAssemblySecondaryPartElements")]
        [InlineData(typeof(BoltMark.ContentTab), "SiteBoltElements WorkshopBoltElements")]
        [InlineData(typeof(ReinforcementMark.ContentTab), "SingleElements GroupElements MeshElements")]
        [InlineData(typeof(ReinforcementMark.MergingTab), "MergedMarkElements")]
        [InlineData(typeof(NeighborPartMark.ContentTab), "MainPartElements SecondaryPartElements")]
        [InlineData(typeof(NeighborReinforcementMark.ContentTab), "SingleElements GroupElements MeshElements")]
        [InlineData(typeof(NeighborReinforcementMark.MergingTab), "MergedMarkElements")]
        [InlineData(typeof(ConnectionMark.ContentTab), "Elements")]
        [InlineData(typeof(SurfaceTreatmentMark.ContentTab), "Elements")]
        [InlineData(typeof(PourMark.ContentTab), "PourElements")]
        [InlineData(typeof(ViewLabelMark.ContentTab), "Elements")]
        [InlineData(typeof(AssociativeNoteProperties.ContentTab), "PartElements BoltElements SurfaceTreatmentElements ReinforcementElements EdgeChamferElements ReferenceObjectElements")]
        [InlineData(typeof(DimensionMark), "PartElements BoltElements SurfaceTreatmentElements ReinforcementElements")]
        [InlineData(typeof(DimensionMark.AdvancedGroup), "PourElements PourBreakElements EdgeChamferElements ReferenceObjectElements")]
        [InlineData(typeof(PartMark.AdvancedGroup), "PourElements PourBreakElements EdgeChamferElements ReferenceObjectElements")]
        [InlineData(typeof(PourMark.AdvancedGroup), "PourBreakElements EdgeChamferElements ReferenceObjectElements")]
        [InlineData(typeof(AssociativeNoteProperties.AdvancedGroup), "PourElements PourBreakElements")]
        public void ContentProperties_AreWhereTheDialogShowsTheirTable(Type section, string properties)
        {
            // The tables of the "Mark content of" menu are in the tab of the dialog's elements table (the merged mark in the
            // Merging tab); the keys the dialog does not show are in Advanced
            string[] expected = properties.Split(' ');
            string[] actual = section.GetProperties().Where(p => p.PropertyType == typeof(MarkContent) && p.DeclaringType == section).Select(p => p.Name).ToArray();

            Assert.Equal(expected.OrderBy(n => n), actual.OrderBy(n => n));

            // Nowhere else in the page: neither on the page nor in another of its tabs or groups
            Type page = section;
            while (page.DeclaringType != null)
            {
                page = page.DeclaringType;
            }

            Assert.All(expected, name => Assert.Equal(new[] { section }, Classes(page).Where(c => c.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) != null)));
        }

        private static IEnumerable<Type> Classes(Type type)
        {
            yield return type;
            foreach (Type nested in type.GetNestedTypes().SelectMany(Classes))
            {
                yield return nested;
            }
        }

        [Fact]
        public void KeysOfEveryMarkPage_HaveContentProperties()
        {
            var view = new ViewProperties();
            foreach (AttributeSection page in new AttributeSection[] { view.Marks.PartMark, view.Marks.BoltMark, view.Marks.ConnectionMark, view.Marks.ReinforcementMark })
            {
                dynamic typed = ((dynamic)page).Advanced;
                AssertTable(page, "PourContentString", () => typed.PourElements, c => typed.PourElements = c);
                AssertTable(page, "PourBreakContentString", () => typed.PourBreakElements, c => typed.PourBreakElements = c);
                AssertTable(page, "ChamferContentString", () => typed.EdgeChamferElements, c => typed.EdgeChamferElements = c);
                AssertTable(page, "ReferenceObjectContentString", () => typed.ReferenceObjectElements, c => typed.ReferenceObjectElements = c);
            }

            var drawing = new Drawings.GADrawingAttributeFile();
            drawing.Marks.PartMark.Advanced.PourElements = Named("pour");
            Assert.Contains("pour", drawing.GetString("dpm.PourContentString"));
        }

        [Fact]
        public void MarkFiles_WriteTheirOneTableLikeTheDialog()
        {
            MarkContent content = Named("mark");
            string Text(AttributeSection section, string key) => Assert.IsType<MarkTextElement>(Assert.Single(section.GetMarkContent(key))).Text;

            var pm = new PartMarkAttributeFile();
            pm.Elements = content;
            Assert.All(new[] { "ContentString1", "ContentString2", "ContentString3", "ContentString4" }, k => Assert.Equal("mark", Text(pm.Properties, k)));

            var rm = new ReinforcementMarkAttributeFile();
            rm.Properties.Merging.MergedMarkElements = Named("merged");
            rm.Elements = content;
            Assert.All(new[] { "ContentString1", "ContentString2", "ContentString4" }, k => Assert.Equal("mark", Text(rm.Properties, k)));
            Assert.Equal("merged", Text(rm.Properties, "ContentString3"));

            var pom = new PourMarkAttributeFile();
            pom.Elements = content;
            Assert.Equal("mark", Text(pom.Properties, "PourContentString"));
            Assert.Equal("mark", Text(pom.Properties, "ContentString1"));
            Assert.False(pom.Values.ContainsKey("ContentString2"));
            Assert.Equal("mark", Assert.IsType<MarkTextElement>(Assert.Single(pom.Elements)).Text);

            pm.Elements = null;
            Assert.Null(pm.Elements);
            Assert.Equal(string.Empty, pm.GetString("ContentString4"));
        }

        [Fact]
        public void ContentWithoutElements_IsWrittenAsTheEmptyTextOfTheDialogs()
        {
            var view = new ViewProperties();
            PartMark page = view.Marks.PartMark;

            page.Content.MainPartElements = new MarkContent();
            Assert.Equal(string.Empty, view.GetString("vpm.ContentString1", null));

            page.Content.MainPartElements = Named("x");
            page.Content.MainPartElements = new MarkContent();
            Assert.Equal(string.Empty, view.GetString("vpm.ContentString1"));
            Assert.Null(page.Content.MainPartElements);

            // Content without elements Tekla wrote as XML is kept as it was
            string xml = new MarkContent().ToXml();
            view.SetString("vpm.ContentString2", xml);
            page.Content.SecondaryPartElements = new MarkContent();
            Assert.Equal(xml.Replace("\n", Environment.NewLine), view.GetString("vpm.ContentString2"));
        }

        [Fact]
        public void PagesWithMarkContentProperties_HaveNoStringPropertyForTheSameKey()
        {
            Assert.Null(typeof(PartMark.AdvancedGroup).GetProperty("ContentString1"));
            Assert.Null(typeof(PartMark.AdvancedGroup).GetProperty("PourContentString"));
            Assert.Null(typeof(ViewLabelMark.AdvancedGroup).GetProperty("ContentString"));

            Assert.Null(typeof(ConnectionMark.AdvancedGroup).GetProperty("ContentString1"));
            Assert.Null(typeof(NeighborPartMark.AdvancedGroup).GetProperty("ContentString2"));

            // Tables the dialogs do not offer stay as text
            Assert.NotNull(typeof(ConnectionMark.AdvancedGroup).GetProperty("ContentString2"));
            Assert.NotNull(typeof(BoltMark.AdvancedGroup).GetProperty("ContentString3"));
        }

        [Fact]
        public void Tables_OfAPageInAFile_UseItsPrefix()
        {
            var view = new ViewProperties();
            view.Marks.ReinforcementMark.Content.MeshElements = Named("mesh");

            Assert.Contains("mesh", view.GetString("vrm.ContentString4"));
        }
    }
}
