using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WW.Cad.Base;
using WW.Cad.Model;
using WW.Cad.Model.Entities;

namespace Dxf2ProfiCAD
{
    public static class Helper
    {
 
        public static System.Drawing.Color DxfEntityColor2Color(DxfEntity a_dxf_entity)
        {
            if(a_dxf_entity.DxfColor != null)
            {

                byte l_b_R = a_dxf_entity.DxfColor.Color.R;
                byte l_b_G = a_dxf_entity.DxfColor.Color.G;
                byte l_b_B = a_dxf_entity.DxfColor.Color.B;

                System.Drawing.Color l_color = System.Drawing.Color.FromArgb(l_b_R,l_b_G,l_b_B);
                return l_color;
            }
            else
            {
                if (a_dxf_entity.Color == EntityColor.ByLayer)
                {

                    int li_i = a_dxf_entity.Layer.Color.ToArgb(3);

                    //                    int li_rgb = a_dxf_entity.Layer.Color.Rgb;
                    WW.Drawing.ArgbColor li_rgb = a_dxf_entity.Layer.Color.ToArgbColor(DxfIndexedColorSet.AcadClassicIndexedColors);

                    System.Drawing.Color l_color_layer = System.Drawing.Color.FromArgb(li_rgb.Argb);
                    //Console.WriteLine(l_color_layer.ToString());
                    if(l_color_layer == System.Drawing.Color.FromArgb(255,255,255))
                    {
                        l_color_layer = System.Drawing.Color.FromArgb(0, 0, 0);
                    }
                    return l_color_layer;
                }


                byte l_b_R = a_dxf_entity.Color.R;
                byte l_b_G = a_dxf_entity.Color.G;
                byte l_b_B = a_dxf_entity.Color.B;

                System.Drawing.Color l_color = System.Drawing.Color.FromArgb(l_b_R, l_b_G, l_b_B);
                return l_color;
            }
        }

        public static bool IsSame(Double ad_1, Double ad_2)
        {
            return Math.Abs(ad_1 - ad_2) < 0.0001;
        }


    }
}
