using System;
using System.Drawing;
using System.IO;
using System.Drawing.Printing;
using System.Linq;

using WW.Cad.Base;
using WW.Cad.Drawing;
using WW.Cad.Drawing.GDI;
using WW.Cad.IO;
using WW.Cad.Model;
using WW.Math;


namespace A2IDLL
{
    public class Program
    {

        static Bitmap CreateOneUnitToOnePixelBitmap(
            DxfModel model,
            Matrix4D transform,
            GraphicsConfig graphicsConfig,
            System.Drawing.Drawing2D.SmoothingMode smoothingMode,
            Size maxSize
            )

        {
            // first calculate the size of the model
            BoundsCalculator boundsCalculator = new BoundsCalculator(graphicsConfig);
            boundsCalculator.GetBounds(model, transform);
            Bounds3D bounds = boundsCalculator.Bounds;
            Vector3D delta = bounds.Delta;
            // now determine image size from this
            // Note: Have to add 2 extra pixels on each side, otherwise Windows will not render
            // the graphics on the outer edges. Also there seems to be always an empty unused line 
            // around the bitmap, but that's not a problem, just a minor waste of space.
            const int margin = 2;
            int width = (int)System.Math.Ceiling(delta.X) + 2 * margin;
            int height = (int)System.Math.Ceiling(delta.Y) + 2 * margin;

            if ((width > maxSize.Width) || (height > maxSize.Height))
            {
                return null;
            }

            // Now move the model so it is centered in the coordinate ranges
            // margin <= x <= width+margin and margin <= y <= height+margin
            // Be careful: in DXF y points up, but in Bitmap it points down!
            Matrix4D to2DTransform = DxfUtil.GetScaleTransform(
                bounds.Corner1,
                bounds.Corner2,
                new Point3D(bounds.Corner1.X, bounds.Corner2.Y, 0d),
                new Point3D(0d, delta.Y, 0d),
                new Point3D(delta.X, 0d, 0d),
                new Point3D(margin, margin, 0d)
            ) * transform;
            // now use standard method to create bitmap
            return ImageExporter.CreateBitmap(model, to2DTransform,
                                              graphicsConfig, smoothingMode,
                                              width, height);
        }




        public static void Convert(string as_format, bool ab_convert, int li_size, string as_fileIn, string as_fileOut)
        {
            const int li_maxSize = 15000;

            if (li_size > li_maxSize)
            {
                li_size = li_maxSize;
            }

            GraphicsConfig l_graphicsConfig = ab_convert ? GraphicsConfig.WhiteBackgroundCorrectForBackColor : GraphicsConfig.BlackBackgroundCorrectForBackColor;
            

            DxfModel model = null;
          
            string extension = Path.GetExtension(as_fileIn);
            if (string.Compare(extension, ".dwg", true) == 0)
            {
                model = DwgReader.Read(as_fileIn);
            }
            else
            {
                model = DxfReader.Read(as_fileIn);
            }

            /*
            Example.ExportToSvgExample l_sample = new Example.ExportToSvgExample();
            l_sample.ExportToSvg(model);
            */
            
            if (as_format == "svg")
            {
                BoundsCalculator boundsCalculator = new BoundsCalculator();
                boundsCalculator.GetBounds(model);
                Bounds3D bounds = boundsCalculator.Bounds;

                double pHeight = bounds.Delta.X;
                double pWidth = bounds.Delta.Y;

                WW.Math.Point3D[] lPoint = new WW.Math.Point3D[2];

                //' Rammi's coord fixes
                lPoint[0] = new WW.Math.Point3D(0, pHeight - 1, 0); // lower left
                lPoint[1] = new WW.Math.Point3D(pWidth - 1, 0, 0); // upper right


                Matrix4D to2DTransform = DxfUtil.GetScaleTransform(
                    bounds.Corner1,
                    bounds.Corner2,
                    lPoint[0],
                    lPoint[1]
                    );

                using (Stream streamSvg = File.Create(as_fileOut))
                {

                    SvgExporter exporter = new SvgExporter(streamSvg);
                    exporter.Draw(model, GraphicsConfig.WhiteBackgroundCorrectForBackColor, to2DTransform);
                }
            }
            else if (as_format == "xaml")
            {
                BoundsCalculator boundsCalculator = new BoundsCalculator();
                boundsCalculator.GetBounds(model);
                Bounds3D bounds = boundsCalculator.Bounds;

                // Scale to fit in a 400 x 400 rectangle.
                Matrix4D to2DTransform = DxfUtil.GetScaleTransform(
                    bounds.Corner1,
                    bounds.Corner2,
                    new Point3D(bounds.Corner1.X, bounds.Corner2.Y, 0d),
                    new Point3D(0d, 800d, 0d),
                    new Point3D(800d, 0d, 0d),
                    new Point3D(0d, 0d, 0d)
                );

                GraphicsConfig graphicsConfig = GraphicsConfig.WhiteBackgroundCorrectForBackColor;
                using (Stream streamXaml = File.Create(as_fileOut)) {
                    using (System.Xml.XmlTextWriter w = new System.Xml.XmlTextWriter(streamXaml, System.Text.Encoding.UTF8))
                    {
                        w.Formatting = System.Xml.Formatting.Indented;

                        w.WriteStartElement("Page");
                        w.WriteAttributeString("xmlns", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
                        w.WriteAttributeString("xmlns:x", "http://schemas.microsoft.com/winfx/2006/xaml");
                        w.WriteStartElement("Canvas");

                        XamlExporter xamlExporter = new XamlExporter(w);
                        xamlExporter.Draw(model, graphicsConfig, to2DTransform);

                        w.WriteEndElement(); // Canvas
                        w.WriteEndElement(); // Page

                        w.Close();
                    }
                }
            }
            else
            {

                GDIGraphics3D l_graphics = new GDIGraphics3D(l_graphicsConfig);
                Size maxSize = new Size(li_size, li_size);
     


                
                Bitmap bitmap =
                    ImageExporter.CreateAutoSizedBitmap(
                        model,
                        l_graphics,
                        Matrix4D.Identity,
                        ab_convert ? System.Drawing.Color.White : System.Drawing.Color.Black,
                        maxSize
                    );



                Stream stream;
                switch (as_format)
                {
                    case "bmp":
                        using (stream = File.Create(as_fileOut))
                        {
                            ImageExporter.EncodeImageToBmp(bitmap, stream);
                        }
                        break;
                    case "gif":
                        using (stream = File.Create(as_fileOut))
                        {
                            ImageExporter.EncodeImageToGif(bitmap, stream);
                        }
                        break;
                    case "tiff":
                        using (stream = File.Create(as_fileOut))
                        {
                            ImageExporter.EncodeImageToTiff(bitmap, stream);
                        }
                        break;
                    case "png":
                        using (stream = File.Create(as_fileOut))
                        {
                            ImageExporter.EncodeImageToPng(bitmap, stream);
                        }
                        break;
                    case "jpg":
                    case "jpeg":
                        using (stream = File.Create(as_fileOut))
                        {
                            ImageExporter.EncodeImageToJpeg(bitmap, stream);
                        }
                        break;
                    default:
                        throw new Exception("Unknown format " + as_format + ".");
                }

            }
        }
    }
}








namespace Example {
}
