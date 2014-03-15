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

namespace Example
{
    class ExportToSvgExample
    {
        public void ExportToSvg(DxfModel model)
        {
            BoundsCalculator boundsCalculator = new BoundsCalculator();
            boundsCalculator.GetBounds(model);
            Bounds3D bounds = boundsCalculator.Bounds;
            PaperSize paperSize = PaperSizes.GetPaperSize(PaperKind.A4);
            // Lengths in hundredths of cm.
            const float hundredthsInchToCm = 2.54f / 100f;
            float pageWidth = paperSize.Width * hundredthsInchToCm * 100f;
            float pageHeight = paperSize.Height * hundredthsInchToCm * 100f;
            float margin = 200f;
            // Scale and transform such that its fits max width/height
            // and the top left middle of the cad drawing will match the 
            // top middle of the svg page.
            Matrix4D to2DTransform = DxfUtil.GetScaleTransform(
                bounds.Corner1,
                bounds.Corner2,
                new Point3D(bounds.Center.X, bounds.Corner2.Y, 0d),
                new Point3D(margin, pageHeight - margin, 0d),
                new Point3D(pageWidth - 2d * margin, margin, 0d),
                new Point3D(pageWidth / 2d, margin, 0d)
            );
            using (Stream stream = File.Create("h:\\test.svg"))
            {
                SvgExporter exporter = new SvgExporter(stream, paperSize);
                exporter.Draw(model, GraphicsConfig.WhiteBackgroundCorrectForBackColor, to2DTransform);
            }
        }
    }
}
