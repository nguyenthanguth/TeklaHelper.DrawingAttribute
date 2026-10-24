using System;
using System.Linq;
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

            AssertTable(page, "ContentString1", () => page.MainPartElements, c => page.MainPartElements = c);
            AssertTable(page, "ContentString2", () => page.SecondaryPartElements, c => page.SecondaryPartElements = c);
            AssertTable(page, "ContentString3", () => page.SubAssemblyMainPartElements, c => page.SubAssemblyMainPartElements = c);
            AssertTable(page, "ContentString4", () => page.SubAssemblySecondaryPartElements, c => page.SubAssemblySecondaryPartElements = c);
        }

        [Fact]
        public void BoltMark_TablesBySiteAndWorkshop()
        {
            BoltMark page = new ViewProperties().Marks.BoltMark;

            AssertTable(page, "ContentString1", () => page.SiteBoltElements, c => page.SiteBoltElements = c);
            AssertTable(page, "ContentString2", () => page.WorkshopBoltElements, c => page.WorkshopBoltElements = c);
        }

        [Fact]
        public void ReinforcementMark_TablesBySingleGroupMergedMesh()
        {
            ReinforcementMark page = new ViewProperties().Marks.ReinforcementMark;

            AssertTable(page, "ContentString1", () => page.SingleElements, c => page.SingleElements = c);
            AssertTable(page, "ContentString2", () => page.GroupElements, c => page.GroupElements = c);
            AssertTable(page, "ContentString3", () => page.MergedMarkElements, c => page.MergedMarkElements = c);
            AssertTable(page, "ContentString4", () => page.MeshElements, c => page.MeshElements = c);
        }

        [Fact]
        public void AssociativeNote_TablesByKindOfObject()
        {
            AssociativeNoteProperties page = AssociativeNoteAttributeFile.CreateDefault().Properties;

            AssertTable(page, "ContentString1", () => page.PartElements, c => page.PartElements = c);
            AssertTable(page, "ContentString2", () => page.BoltElements, c => page.BoltElements = c);
            AssertTable(page, "ContentString3", () => page.SurfaceTreatmentElements, c => page.SurfaceTreatmentElements = c);
            AssertTable(page, "ContentString4", () => page.ReinforcementElements, c => page.ReinforcementElements = c);
            AssertTable(page, "ReferenceObjectContentString", () => page.ReferenceObjectElements, c => page.ReferenceObjectElements = c);
        }

        [Fact]
        public void Defaults_AreMarkContentOfTheirKind()
        {
            AssociativeNoteProperties note = AssociativeNoteAttributeFile.CreateDefault().Properties;

            Assert.Equal(
                new[] { MarkElementTypes.PartMark.PartPosition, MarkElementTypes.PartMark.Profile },
                note.PartElements.OfType<MarkPropertyElement>().Select(e => e.Type));
            Assert.True(Assert.Single(note.BoltElements.OfType<MarkPropertyElement>()).Type.IsSizeElement);
            Assert.Contains(note.ReinforcementElements, e => e is MarkXmlElement x && x.Tag == "PullOut");
            Assert.Equal("POUR BREAK", Assert.IsType<MarkTextElement>(Assert.Single(note.GetMarkContent("PourBreakContentString"))).Text);
            Assert.All(note.PartElements.OfType<MarkPropertyElement>(), e => Assert.Equal(DrawingColor.NewLine2, e.Format.Color));

            PourMarkAttributeFile pourMark = PourMarkAttributeFile.CreateDefault();
            Assert.Equal(MarkElementTypes.PourMark.Material, Assert.IsType<MarkPropertyElement>(Assert.Single(pourMark.Properties.GetMarkContent("PourContentString"))).Type);
        }

        [Fact]
        public void KeysOfEveryMarkPage_HaveContentProperties()
        {
            var view = new ViewProperties();
            foreach (AttributeSection page in new AttributeSection[] { view.Marks.PartMark, view.Marks.BoltMark, view.Marks.ConnectionMark, view.Marks.ReinforcementMark })
            {
                dynamic typed = page;
                AssertTable(page, "PourContentString", () => typed.PourElements, c => typed.PourElements = c);
                AssertTable(page, "PourBreakContentString", () => typed.PourBreakElements, c => typed.PourBreakElements = c);
                AssertTable(page, "ChamferContentString", () => typed.EdgeChamferElements, c => typed.EdgeChamferElements = c);
                AssertTable(page, "ReferenceObjectContentString", () => typed.ReferenceObjectElements, c => typed.ReferenceObjectElements = c);
            }

            var drawing = new Drawings.GADrawingAttributeFile();
            drawing.Marks.PartMark.PourElements = Named("pour");
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
            rm.Properties.MergedMarkElements = Named("merged");
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

            page.MainPartElements = new MarkContent();
            Assert.Equal(string.Empty, view.GetString("vpm.ContentString1", null));

            page.MainPartElements = Named("x");
            page.MainPartElements = new MarkContent();
            Assert.Equal(string.Empty, view.GetString("vpm.ContentString1"));
            Assert.Null(page.MainPartElements);

            // Content without elements Tekla wrote as XML is kept as it was
            string xml = new MarkContent().ToXml();
            view.SetString("vpm.ContentString2", xml);
            page.SecondaryPartElements = new MarkContent();
            Assert.Equal(xml.Replace("\n", Environment.NewLine), view.GetString("vpm.ContentString2"));
        }

        [Fact]
        public void PagesWithMarkContentProperties_HaveNoStringPropertyForTheSameKey()
        {
            Assert.Null(typeof(PartMark.AdvancedGroup).GetProperty("ContentString1"));
            Assert.Null(typeof(PartMark.AdvancedGroup).GetProperty("PourContentString"));
            Assert.Null(typeof(ViewLabelMark.AdvancedGroup).GetProperty("ContentString"));

            // Tables no probe named stay as text
            Assert.NotNull(typeof(ConnectionMark.AdvancedGroup).GetProperty("ContentString1"));
        }

        [Fact]
        public void Tables_OfAPageInAFile_UseItsPrefix()
        {
            var view = new ViewProperties();
            view.Marks.ReinforcementMark.MeshElements = Named("mesh");

            Assert.Contains("mesh", view.GetString("vrm.ContentString4"));
        }
    }
}
