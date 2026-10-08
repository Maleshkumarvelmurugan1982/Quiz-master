using System.Text;
using DocumentFormat.OpenXml.Packaging;
using D = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace QuizApp.Services;

public static class DocumentTextExtractor
{
    public static string ExtractDocx(Stream stream)
    {
        try
        {
            Console.WriteLine("[Extractor] Starting DOCX extraction...");

            if (stream.CanSeek)
                stream.Position = 0;

            using var doc =
                WordprocessingDocument.Open(stream, false);

            Console.WriteLine("[Extractor] DOCX opened.");

            var body =
                doc.MainDocumentPart?
                   .Document?
                   .Body;

            if (body == null)
            {
                Console.WriteLine("[Extractor] DOCX body is null.");
                return "";
            }

            Console.WriteLine("[Extractor] DOCX body found.");

            var sb = new StringBuilder();

            foreach (var p in body.Descendants<W.Paragraph>())
            {
                try
                {
                    var text = p.InnerText;

                    if (!string.IsNullOrWhiteSpace(text))
                        sb.AppendLine(text);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"[Extractor] DOCX paragraph error: {ex.Message}");

                    throw;
                }
            }

            Console.WriteLine(
                $"[Extractor] DOCX extraction complete. Characters: {sb.Length}");

            return sb.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[Extractor] DOCX ERROR: {ex}");

            throw;
        }
    }


    public static string ExtractPptx(Stream stream)
    {
        try
        {
            Console.WriteLine("[Extractor] Starting PPTX extraction...");

            if (stream.CanSeek)
                stream.Position = 0;

            using var ppt =
                PresentationDocument.Open(stream, false);

            Console.WriteLine("[Extractor] PPTX opened.");

            var presentationPart =
                ppt.PresentationPart;

            var slideIds =
                presentationPart?
                    .Presentation?
                    .SlideIdList?
                    .Elements<P.SlideId>();

            if (presentationPart == null ||
                slideIds == null)
            {
                Console.WriteLine(
                    "[Extractor] PPTX presentation/slide list is null.");

                return "";
            }

            var sb = new StringBuilder();
            var n = 1;

            foreach (var slideId in slideIds)
            {
                try
                {
                    var relId =
                        slideId.RelationshipId?.Value;

                    if (relId == null)
                        continue;

                    if (presentationPart.GetPartById(relId)
                        is not SlidePart slidePart)
                    {
                        continue;
                    }

                    sb.AppendLine(
                        $"--- Slide {n++} ---");

                    foreach (var para in
                             slidePart.Slide
                                 .Descendants<D.Paragraph>())
                    {
                        var text = para.InnerText;

                        if (!string.IsNullOrWhiteSpace(text))
                            sb.AppendLine(text);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"[Extractor] PPTX slide error: {ex}");

                    throw;
                }
            }

            Console.WriteLine(
                $"[Extractor] PPTX extraction complete. Characters: {sb.Length}");

            return sb.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[Extractor] PPTX ERROR: {ex}");

            throw;
        }
    }


    public static string ExtractPlainText(Stream stream)
    {
        if (stream.CanSeek)
            stream.Position = 0;

        using var reader =
            new StreamReader(
                stream,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true);

        return reader.ReadToEnd();
    }
}