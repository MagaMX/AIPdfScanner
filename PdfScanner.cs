using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Tesseract;
using Ghostscript.NET.Rasterizer;
using System.Text;

namespace AIPdfScanner
{
    class PdfScanner
    {
        //Read Pdf file  without  OCR
        public async Task<string> ExtractTextFromPdf(string filePath)
        {
            StringBuilder text = new StringBuilder();
            using (PdfReader reader = new PdfReader(filePath))
            {
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    text.Append(PdfTextExtractor.GetTextFromPage(reader, i) + "\n");
                }
            }

            if (string.IsNullOrWhiteSpace(text.ToString()))
            {
                text = await PerformOCR(filePath);
            }

            return text.ToString();
        }

        //If standart method cannot read file, using OCR
        private async Task<StringBuilder> PerformOCR(string filePath)
        {
            string tempFolder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filePath), "Temp");
            StringBuilder extractedText = new StringBuilder();

            try
            {
                if (!Directory.Exists(tempFolder))
                    Directory.CreateDirectory(tempFolder); 

                ConvertPdfToImage(filePath, tempFolder, extractedText);
            }
            finally
            {
                DeleteTempFolder(tempFolder); 
            }

            
            return await Task.FromResult(extractedText);
        }

        //Convert Pdf file to image so Tesseract can process it
        private static void ConvertPdfToImage(string pdfPath, string tempFolder, StringBuilder extractedText)
        {
            using (var rasterizer = new GhostscriptRasterizer())
            {
                rasterizer.Open(pdfPath);
                int totalPages = rasterizer.PageCount;

                for (int pageNumber = 1; pageNumber <= totalPages; pageNumber++)
                {
                    string outputImage = System.IO.Path.Combine(tempFolder, $"{System.IO.Path.GetFileNameWithoutExtension(pdfPath)}_{pageNumber}.png");

                    using (var img = rasterizer.GetPage(300, pageNumber)) // 300 DPI
                    {
                        img.Save(outputImage, System.Drawing.Imaging.ImageFormat.Png);
                        RecognizeText(outputImage, extractedText); 
                    }
                }
            }
        }

        static void RecognizeText(string imagePath, StringBuilder extractedText)
        {
            string tessPath = @"C:\Program Files\Tesseract-OCR\tessdata"; 

            using (var engine = new TesseractEngine(tessPath, "eng+rus", EngineMode.Default))
            {
                using (var img = Pix.LoadFromFile(imagePath))
                {
                    using (var page = engine.Process(img))
                    {
                        extractedText.Append(page.GetText());
                    }
                }
            }
        }

        static void DeleteTempFolder(string folderPath)
        {
            try
            {
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true); 
                    Console.WriteLine($"Папка {folderPath} удалена.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении папки: {ex.Message}");
            }
        }
    }
}
