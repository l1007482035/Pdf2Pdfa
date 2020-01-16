using System;
using iTextSharp.text.pdf;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Pdf2PdfaTest
{
    [TestClass]
    public class ConvertTest
    {
        [TestMethod]
        public void Pdf2PdfaTest()
        {
            Pdf2PdfaLib.Converter.Convert(@"resources/test.pdf", @"resources/test_pdfa.pdf", "PDF_A_3B");
        }
    }
}
