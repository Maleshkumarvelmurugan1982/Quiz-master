using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace QuizApp.Code
{
    /// <summary>
    /// Reads plain text out of .docx / .pptx / text files.
    /// DOCX and PPTX are ZIP files containing XML, so the built-in ZipArchive + XDocument
    /// classes are enough (no DocumentFormat.OpenXml package needed).
    /// </summary>
    public static class DocumentTextExtractor
    {
        private static readonly XNamespace W = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        private static readonly XNamespace A = "http://schemas.openxmlformats.org/drawingml/2006/main";
        private static readonly XNamespace P = "http://schemas.openxmlformats.org/presentationml/2006/main";
        private static readonly XNamespace R = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private static readonly XNamespace Rel = "http://schemas.openxmlformats.org/package/2006/relationships";

        public static string ExtractDocx(Stream stream)
        {
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Read, true))
            {
                var entry = zip.GetEntry("word/document.xml");
                if (entry == null) return "";

                XDocument doc;
                using (var s = entry.Open()) doc = XDocument.Load(s);

                var sb = new StringBuilder();
                foreach (var p in doc.Descendants(W + "p"))
                {
                    var line = ParagraphText(p, W + "t", W + "tab", W + "br");
                    if (!string.IsNullOrWhiteSpace(line)) sb.AppendLine(line);
                }
                return sb.ToString();
            }
        }

        public static string ExtractPptx(Stream stream)
        {
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Read, true))
            {
                var sb = new StringBuilder();
                var n = 1;
                foreach (var slidePath in SlidePathsInOrder(zip))
                {
                    var entry = zip.GetEntry(slidePath);
                    if (entry == null) continue;

                    XDocument doc;
                    using (var s = entry.Open()) doc = XDocument.Load(s);

                    sb.AppendLine("--- Slide " + n++ + " ---");
                    foreach (var p in doc.Descendants(A + "p"))
                    {
                        var line = ParagraphText(p, A + "t", null, A + "br");
                        if (!string.IsNullOrWhiteSpace(line)) sb.AppendLine(line);
                    }
                }
                return sb.ToString();
            }
        }

        public static string ExtractPlainText(Stream stream)
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8, true))
            {
                return reader.ReadToEnd();
            }
        }

        private static string ParagraphText(XElement paragraph, XName text, XName tab, XName br)
        {
            var sb = new StringBuilder();
            foreach (var el in paragraph.Descendants())
            {
                if (el.Name == text) sb.Append(el.Value);
                else if (tab != null && el.Name == tab) sb.Append('\t');
                else if (br != null && el.Name == br) sb.Append('\n');
            }
            return sb.ToString();
        }

        /// <summary>Slide order from presentation.xml; falls back to slide1, slide2, ...</summary>
        private static List<string> SlidePathsInOrder(ZipArchive zip)
        {
            try
            {
                var presEntry = zip.GetEntry("ppt/presentation.xml");
                var relsEntry = zip.GetEntry("ppt/_rels/presentation.xml.rels");
                if (presEntry != null && relsEntry != null)
                {
                    XDocument pres, rels;
                    using (var s = presEntry.Open()) pres = XDocument.Load(s);
                    using (var s = relsEntry.Open()) rels = XDocument.Load(s);

                    var targets = rels.Descendants(Rel + "Relationship")
                        .Where(x => x.Attribute("Id") != null && x.Attribute("Target") != null)
                        .ToDictionary(x => (string)x.Attribute("Id"), x => (string)x.Attribute("Target"));

                    var ordered = new List<string>();
                    foreach (var sld in pres.Descendants(P + "sldId"))
                    {
                        var rid = (string)sld.Attribute(R + "id");
                        string target;
                        if (rid != null && targets.TryGetValue(rid, out target))
                        {
                            target = target.TrimStart('/');
                            ordered.Add(target.StartsWith("ppt/") ? target : "ppt/" + target);
                        }
                    }
                    if (ordered.Count > 0) return ordered;
                }
            }
            catch
            {
                // fall through to simple ordering
            }

            return zip.Entries
                .Select(e => e.FullName)
                .Where(n => Regex.IsMatch(n, @"^ppt/slides/slide\d+\.xml$"))
                .OrderBy(n => int.Parse(Regex.Match(n, @"\d+").Value))
                .ToList();
        }
    }
}
