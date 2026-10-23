using System;
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
        public void Tables_OfAPageInAFile_UseItsPrefix()
        {
            var view = new ViewProperties();
            view.Marks.ReinforcementMark.MeshElements = Named("mesh");

            Assert.Contains("mesh", view.GetString("vrm.ContentString4"));
        }
    }
}
