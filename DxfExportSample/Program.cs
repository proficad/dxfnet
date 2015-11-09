using System;
using System.Drawing;
using System.IO;
using System.Drawing.Printing;

using WW.Cad.Base;
using WW.Cad.Drawing;
using WW.Cad.Drawing.GDI;
using WW.Cad.IO;
using WW.Cad.Model;
using WW.Math;


namespace DxfExportSample
{
    class DxfExportExample
    {
        public static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: DxfExport [png|tiff|jpeg|bmp|gif|pdf] <dxf or dwg file>");
                Environment.Exit(0);
            }
            string format = args[0];
            string filename = args[1];

            DxfModel model = null;
            try
            {
                string extension = Path.GetExtension(filename);
                if (string.Compare(extension, ".dwg", true) == 0)
                {
                    model = DwgReader.Read(filename);
                }
                else
                {
                    model = DxfReader.Read(filename);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error occurred: " + e.Message);
                Environment.Exit(1);
            }
            string outfile = Path.GetFileNameWithoutExtension(Path.GetFullPath(filename));
            Stream stream;
            if (format == "pdf")
            {
                BoundsCalculator boundsCalculator = new BoundsCalculator();
                boundsCalculator.GetBounds(model);
                Bounds3D bounds = boundsCalculator.Bounds;
                PaperSize paperSize = PaperSizes.GetPaperSize(PaperKind.Letter);
                // Lengths in inches.
                float pageWidth = (float)paperSize.Width / 100f;
                float pageHeight = (float)paperSize.Height / 100f;
                float margin = 0.5f;
                // Scale and transform such that its fits max width/height
                // and the top left middle of the cad drawing will match the 
                // top middle of the pdf page.
                // The transform transforms to pdf pixels.
                Matrix4D to2DTransform = DxfUtil.GetScaleTransform(
                    bounds.Corner1,
                    bounds.Corner2,
                    new Point3D(bounds.Center.X, bounds.Corner2.Y, 0d),
                    new Point3D(new Vector3D(margin, margin, 0d) * PdfExporter.InchToPixel),
                    new Point3D(new Vector3D(pageWidth - 2d * margin, pageHeight - margin, 0d) * PdfExporter.InchToPixel),
                    new Point3D(new Vector3D(pageWidth / 2d, pageHeight - margin, 0d) * PdfExporter.InchToPixel)
                );
                using (stream = File.Create(outfile + ".pdf"))
                {
                    PdfExporter pdfGraphics = new PdfExporter(stream);
                    pdfGraphics.DrawPage(
                        model,
                        GraphicsConfig.WhiteBackgroundCorrectForBackColor,
                        to2DTransform,
                        paperSize
                    );
                    pdfGraphics.EndDocument();
                }
            }
            else
            {
                GDIGraphics3D graphics = new GDIGraphics3D(GraphicsConfig.BlackBackgroundCorrectForBackColor);
                Size maxSize = new Size(5000, 5000);

                
                Bitmap bitmap =
                    ImageExporter.CreateAutoSizedBitmap(
                        model,
                        graphics,
                        Matrix4D.Identity,
                        System.Drawing.Color.Black,
                        maxSize
                    );
                


                 


                switch (format)
                {
                    case "bmp":
                        using (stream = File.Create(outfile + ".bmp"))
                        {
                            ImageExporter.EncodeImageToBmp(bitmap, stream);
                        }
                        break;
                    case "gif":
                        using (stream = File.Create(outfile + ".gif"))
                        {
                            ImageExporter.EncodeImageToGif(bitmap, stream);
                        }
                        break;
                    case "tiff":
                        using (stream = File.Create(outfile + ".tiff"))
                        {
                            ImageExporter.EncodeImageToTiff(bitmap, stream);
                        }
                        break;
                    case "png":
                        using (stream = File.Create(outfile + ".png"))
                        {
                            ImageExporter.EncodeImageToPng(bitmap, stream);
                        }
                        break;
                    case "jpg":
                        using (stream = File.Create(outfile + ".jpg"))
                        {
                            ImageExporter.EncodeImageToJpeg(bitmap, stream);
                        }
                        break;
                    default:
                        Console.WriteLine("Unknown format " + format + ".");
                        break;
                }
            }
        }
    }

}
