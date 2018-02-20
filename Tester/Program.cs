using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using WW.Math;
using WW.Cad.Base;
using WW.Cad.IO;
using WW.Cad.Model;
using WW.Cad.Model.Objects;
using WW.Cad.Model.Entities;
using WW.Cad.Model.Tables;
using System.IO;
using WW.Cad.Drawing;



//using Loader;

namespace Tester
{
    class Program
    {
         

        static void Main(string[] args)
        {

            //Calculate_Line_Thickness();
            TestDims();
            //TestImageTransform();

        }

        /*
        private static void Calculate_Line_Thickness()
        {
            for (int li_i = 0; li_i < 50; ++li_i)
            {
                Console.WriteLine("{0} -> {1}", li_i, Exporter.Calculate_Line_Thickness_Ellipse(li_i));
            }
        }
        */


        private static void TestImageTransform()
        {
            DxfModel model = new DxfModel();

            DxfImageDef imageDef = new DxfImageDef(model);
            imageDef.Filename = @"H:\img.png";
            imageDef.Size = new Size2D(840d, 525d);
            imageDef.DefaultPixelSize = new Size2D(1d, 1d);
            imageDef.ImageIsLoaded = true;
            model.Images.Add(imageDef);

            DxfImage image = new DxfImage();
            image.ImageDef = imageDef;
            image.InsertionPoint = new Point3D(3d, 2d, 0d);
            // Flag ImageDisplayFlags.ShowUnalignedImage is necessary only when rotating the image such that it's not aligned
            // horizontally or vertically.
            image.ImageDisplayFlags = ImageDisplayFlags.ShowImage | ImageDisplayFlags.ShowUnalignedImage;
            image.SetDefaultBoundaryVertices();
            model.Entities.Add(image);

            double li_scale_factor = 0.5;
            Matrix4D l_tr = Transformation4D.Scaling(li_scale_factor);
            WW.Cad.Drawing.TransformConfig l_config = new WW.Cad.Drawing.TransformConfig();
            foreach (DxfEntity l_entity in model.Entities)
            {
                l_entity.TransformMe(l_config, l_tr);//<- l_entity is instance of {WW.Cad.Model.Entities.DxfImage}
            }

        }

        private static void TestDim()
        {
            DxfModel model = new DxfModel(DxfVersion.Dxf14);

            DxfLayer layer = new DxfLayer("DIMENSIONS");
            model.Layers.Add(layer);

            DxfBlock block = new DxfBlock("LINEAR_DIMENSIONS");
            model.Blocks.Add(block);

            DxfInsert insert = new DxfInsert(block, new Point3D(5, -20, 0));
            insert.Layer = layer;
            model.Entities.Add(insert);

            // Horizontal.
            DxfDimension.Linear linearDimension1 = new DxfDimension.Linear(model.DefaultDimensionStyle);
            linearDimension1.ExtensionLine1StartPoint = new Point3D(0, 0, 0);
            linearDimension1.ExtensionLine2StartPoint = new Point3D(200, 0, 0);
            linearDimension1.DimensionLineLocation = new Point3D(200, 100, 0);
            block.Entities.Add(linearDimension1);
            /*
            // Rotated and usage of "<>" text.
            DxfDimension.Linear linearDimension2 = new DxfDimension.Linear(model.DefaultDimensionStyle);
            linearDimension2.Rotation = Math.PI / 6d;
            linearDimension2.Text = "<> cm"; // <> is replaced by measurement.
            linearDimension2.ExtensionLine1StartPoint = new Point3D(0, 2, 0);
            linearDimension2.ExtensionLine2StartPoint = new Point3D(2, 2, 0);
            linearDimension2.DimensionLineLocation = new Point3D(2, 4, 0);
            block.Entities.Add(linearDimension2);

            // Rotated to the other side with rounded measurement and aligned text.
            DxfDimension.Linear linearDimension3 = new DxfDimension.Linear(model.DefaultDimensionStyle);
            linearDimension3.DimensionStyleOverrides.TextInsideHorizontal = false;
            linearDimension3.DimensionStyleOverrides.Rounding = 0.25;
            linearDimension3.DimensionStyleOverrides.DimensionLineColor = Colors.Red;
            linearDimension3.DimensionStyleOverrides.ExtensionLineColor = Colors.Green;
            linearDimension3.Rotation = -Math.PI / 6d;
            linearDimension3.ExtensionLine1StartPoint = new Point3D(2, 4, 0);
            linearDimension3.ExtensionLine2StartPoint = new Point3D(0, 4, 0);
            linearDimension3.DimensionLineLocation = new Point3D(0, 6, 0);
            block.Entities.Add(linearDimension3);

            // Rotated.
            DxfDimension.Linear linearDimension4 = new DxfDimension.Linear(model.DefaultDimensionStyle);
            linearDimension4.TextRotation = 0;
            linearDimension4.Rotation = Math.PI / 6d;
            linearDimension4.ExtensionLine1StartPoint = new Point3D(2, 6, 0);
            linearDimension4.ExtensionLine2StartPoint = new Point3D(0, 6, 0);
            linearDimension4.DimensionLineLocation = new Point3D(2, 8, 0);
            block.Entities.Add(linearDimension4);

            // Vertical.
            DxfDimension.Linear linearDimension5 = new DxfDimension.Linear(model.DefaultDimensionStyle);
            linearDimension5.TextRotation = Math.PI / 8d;
            linearDimension5.Rotation = Math.PI / 2d;
            linearDimension5.DimensionStyleOverrides.TextInsideHorizontal = false;
            linearDimension5.ExtensionLine1StartPoint = new Point3D(0, 10, 0);
            linearDimension5.ExtensionLine2StartPoint = new Point3D(0, 8, 0);
            linearDimension5.DimensionLineLocation = new Point3D(2, 8, 0);
            block.Entities.Add(linearDimension5);

            // Vertical.
            DxfDimension.Linear linearDimension6 = new DxfDimension.Linear(model.DefaultDimensionStyle);
            linearDimension6.Rotation = Math.PI / 2d;
            linearDimension6.DimensionStyleOverrides.TextInsideHorizontal = false;
            linearDimension6.ExtensionLine1StartPoint = new Point3D(0, 12, 0);
            linearDimension6.ExtensionLine2StartPoint = new Point3D(0, 10, 0);
            linearDimension6.DimensionLineLocation = new Point3D(2, 10, 0);
            block.Entities.Add(linearDimension6);
            */
            DxfWriter.Write(@"H:\f3\TestDim.dxf", model, false);
        }

        private static void TestDims()
        {
            DxfModel model = new DxfModel();

            model.Entities.Add(CreateAlignedDim(model, new Point3D(2, 1, 0), new Point3D(4, 2, 0), 1));
            model.Entities.Add(CreateLinearDim(model, new Point3D(2, 3, 0), new Point3D(2.1, 4, 0), new Point3D(4, 5, 0)));

            DwgWriter.Write(@"H:\f3\dim.dwg", model);
        }
      

            
            
        // For aligned dimensions it may be easier using a distance to calculate the dimension line location.
        public static DxfDimension CreateAlignedDim(DxfModel model, Point3D a, Point3D b, double d /* distance */)
        {
            DxfDimension.Aligned dim = new DxfDimension.Aligned(model.CurrentDimensionStyle);
            dim.ExtensionLine1StartPoint = a;
            dim.ExtensionLine2StartPoint = b;
            Vector3D delta = (b - a).GetUnit();
            Vector3D perpendicular = Vector3D.CrossProduct(Vector3D.ZAxis, delta); // Or change order for other side of the AB line.
            dim.DimensionLineLocation = a + perpendicular * d;
            return dim;
        }

        // For linear dimensions it makes more sense directly specifying the dimension line location as
        // the dimension line in general is not parallel to the AB line
        // (the dimension line direction is determined by the DxfDimension.Linear.Rotation angle).
        public static DxfDimension CreateLinearDim(DxfModel model, Point3D a, Point3D b, Point3D dimensionLineLocation)
        {
            DxfDimension.Linear dim = new DxfDimension.Linear(model.CurrentDimensionStyle);
            dim.ExtensionLine1StartPoint = a;
            dim.ExtensionLine2StartPoint = b;
            dim.DimensionLineLocation = dimensionLineLocation;
            return dim;
        }

        //-------------------
    }
}
