using System;
using System.IO;
using System.Text;
using TeklaHelper.DrawingAttribute.Drawings;
using TeklaHelper.DrawingAttribute.Marks;
using TeklaHelper.DrawingAttribute.Views;

namespace TeklaHelper.DrawingAttribute.Samples.Offline
{
    /// <summary>
    /// Code pages: attribute files are written in the ANSI code page of their "encoding" line (1252, 932 Japanese, 949
    /// Korean ...), mark contents in UTF-8 inside them, and saving refuses characters the code page cannot hold instead of
    /// writing "?".
    /// </summary>
    internal sealed class CodePageExample : Example
    {
        public override string Title => "Code pages: Japanese / Korean files, UTF-8 mark content, characters that do not fit";

        public override string Description => "CodePage 932 / 949, the encoding line, UTF-8 ContentString in a 932 file, and the error for a missing character.";

        public override void Run(ExampleContext context)
        {
            context.Step("A GA drawing in code page 932 (Japanese) with a Japanese title");
            GADrawingAttributeFile japanese = GADrawingAttributeFile.CreateDefault();
            japanese.CodePage = 932;
            japanese.Title1 = "一階平面図";
            string path = japanese.SaveTo(context.OutputDirectory, "sample_japanese");
            Console.WriteLine("    First line: " + File.ReadAllLines(path)[0]);
            Console.WriteLine("    title_1 bytes: " + Bytes(FindLine(path, 932, "title_1"), 932));
            Console.WriteLine("    Read back the same: " + (AttributeFile.FromFile<GADrawingAttributeFile>(path).Title1 == japanese.Title1));

            context.Step("A view label with Japanese text: mark content is UTF-8 even in a 932 file");
            ViewProperties view = ViewProperties.CreateDefault();
            view.CodePage = 932;
            view.Attributes.LabelMarkA1.Content.Elements = new MarkContent()
                .AddText("断面 ")
                .Add(MarkElementTypes.ViewLabel.ViewName);
            string viewPath = view.SaveTo(context.OutputDirectory, "sample_japanese_label");
            MarkContent label = AttributeFile.FromFile<ViewProperties>(viewPath).Attributes.LabelMarkA1.Content.Elements;
            Console.WriteLine("    Read back the same: " + label.Equals(view.Attributes.LabelMarkA1.Content.Elements) + " (the console may not show the characters)");

            context.Step("Korean (949)");
            GADrawingAttributeFile korean = GADrawingAttributeFile.CreateDefault();
            korean.CodePage = 949;
            korean.Title1 = "평면도";
            string koreanPath = korean.SaveTo(context.OutputDirectory, "sample_korean");
            Console.WriteLine("    title_1 bytes: " + Bytes(FindLine(koreanPath, 949, "title_1"), 949));
            Console.WriteLine("    Read back the same: " + (AttributeFile.FromFile<GADrawingAttributeFile>(koreanPath).Title1 == korean.Title1));

            context.Step("Japanese in a 1252 file: saving throws instead of writing \"?\"");
            GADrawingAttributeFile western = GADrawingAttributeFile.CreateDefault();
            western.Title1 = "一階平面図";
            try
            {
                western.SaveTo(context.OutputDirectory, "sample_does_not_fit");
                Console.WriteLine("    Saved (unexpected)");
            }
            catch (EncoderFallbackException exception)
            {
                Console.WriteLine($"    {exception.GetType().Name}: {exception.Message}");
                Console.WriteLine("    Nothing was written: " + !File.Exists(Path.Combine(context.OutputDirectory, "sample_does_not_fit.gd")));
            }
        }

        private static string Bytes(string line, int codePage) =>
            BitConverter.ToString(Encoding.GetEncoding(codePage).GetBytes(line)).Replace("-", " ");

        private static string FindLine(string path, int codePage, string key)
        {
            foreach (string line in File.ReadAllLines(path, Encoding.GetEncoding(codePage)))
            {
                if (line.StartsWith(key + " ", StringComparison.Ordinal))
                {
                    return line;
                }
            }

            return "(none)";
        }
    }
}
