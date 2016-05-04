using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using WW.Math;
using WW.Cad.Base;
using WW.Cad.IO;
using WW.Cad.Model;
using WW.Cad.Model.Objects;
using WW.Cad.Model.Entities;
using WW.Cad.Model.Tables;



//using Loader;

namespace Tester
{
    class Program
    {


        static void Main(string[] args)
        {

            //Calculate_Line_Thickness();

            TestImageTransform();

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


    //-------------------
    }
}
