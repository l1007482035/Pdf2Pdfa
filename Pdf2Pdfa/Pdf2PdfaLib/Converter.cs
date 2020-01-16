using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Pdf2PdfaLib
{
    public class Converter
    {
        private static string _inputFilePath;
        private static PdfAConformanceLevel _pdfaConformanceLevel;

        private static void SetPdfAConformance(PdfReader reader, Document doc, MemoryStream ms)
        {
            // Create PdfAWriter with PdfAConformanceLevel.PDF_A_3B option if you
            // want to get a PDF/A-3b compliant document.
            PdfAWriter writer = PdfAWriter.GetInstance(doc, ms, _pdfaConformanceLevel);
            // Create XMP metadata. It's a PDF/A requirement.
            writer.CreateXmpMetadata();

            doc.Open();

            // Set output intent. PDF/A requirement.
            ICC_Profile icc = ICC_Profile
                .GetInstance(new FileStream(@"resources/sRGB Color Space Profile.icm", FileMode.Open));
            writer.SetOutputIntents("Custom", "", "http://www.color.org",
                "sRGB IEC61966-2.1", icc);

            // Creating PDF/A-3 compliant attachment.
            PdfDictionary parameters = new PdfDictionary();
            parameters.Put(PdfName.MODDATE, new PdfDate());
            PdfFileSpecification fileSpec = PdfFileSpecification.FileEmbedded(
                writer, _inputFilePath,
                "test.pdf", null, "application/pdf", parameters, 0);
            fileSpec.Put(new PdfName("AFRelationship"), new PdfName("Data"));
            writer.AddFileAttachment("test.pdf", fileSpec);
            PdfArray array = new PdfArray {fileSpec.Reference};
            writer.ExtraCatalog.Put(new PdfName("AF"), array);

            doc.AddDocListener(writer);
            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                doc.SetPageSize(reader.GetPageSize(i));
                doc.NewPage();
                PdfContentByte cb = writer.DirectContent;
                PdfImportedPage page = writer.GetImportedPage(reader, i);
                int rotation = reader.GetPageRotation(i);
                if (rotation == 90 || rotation == 270)
                    cb.AddTemplate(page, 0, -1.0F, 1.0F, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                else
                    cb.AddTemplate(page, 1.0F, 0, 0, 1.0F, 0, 0);
            }
        }

        public static void Convert(string inputPath, string outputPath, string pdfAConformanceLevel)
        {
            _inputFilePath = inputPath;
            _pdfaConformanceLevel = (PdfAConformanceLevel)Enum.Parse(typeof(PdfAConformanceLevel), pdfAConformanceLevel);

            PdfReader reader = new PdfReader(_inputFilePath);

            Document doc = new Document();
            using (MemoryStream ms = new MemoryStream())
            {
                SetPdfAConformance(reader, doc, ms);

                doc.Close();
                reader.Close();

                using (FileStream fs = new FileStream(outputPath, FileMode.Create))
                {
                    // this is the part stumping me; I need to use a PdfStamper to write 
                    // out some values to fields on the form AFTER the pages are removed.
                    // This works, but there doesn't seem to be a form on the copied page...
                    var stamper = new PdfStamper(new PdfReader(ms.ToArray()), fs) {FormFlattening = true};
                    // write out fields here...
                    stamper.SetFullCompression();
                    stamper.Close();
                }
            }
        }

        //public static MemoryStream Convert(MemoryStream inputStream, string pdfAConformanceLevel)
        //{
        //    _pdfaConformanceLevel = (PdfAConformanceLevel)Enum.Parse(typeof(PdfAConformanceLevel), pdfAConformanceLevel);

        //    PdfReader reader = new PdfReader(inputStream);

        //    Document doc = new Document();
        //    MemoryStream ms = new MemoryStream();

        //    SetPdfAConformance(reader, doc, ms);
        //    doc.Close();
        //    reader.Close();

        //    return ms;
        //}
    }
}
