using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using WW.Cad.Base;
using WW.Cad.Model;
using WW.Cad.Model.Entities;
using WW.Cad.Model.Tables;
using DxfNet;
using Color = System.Drawing.Color;
using System.Drawing;

namespace Dxf2ProfiCAD
{
    public static class Helper
    {
        public static RectangleF RectangleF(double ld_left, double ld_top, double ld_width, double ld_height)
        {
            RectangleF l_rect = new RectangleF((float)ld_left, (float)ld_top, (float)ld_width, (float)ld_height);
            return l_rect;
        }


        public static System.Drawing.Color DxfEntityColor2Color(DxfEntity a_dxf_entity)
        {
            System.Drawing.Color l_color = DxfEntityColor2Color_Internal(a_dxf_entity);
            if (l_color.R + l_color.G + l_color.B > 400)
            {
                l_color = Color.Black;
            }

            return l_color;
        }

        private static System.Drawing.Color DxfEntityColor2Color_Internal(DxfEntity a_dxf_entity)
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
                    // this works
                    WW.Drawing.ArgbColor li_rgb = a_dxf_entity.Layer.Color.ToArgbColor(DxfIndexedColorSet.AcadClassicIndexedColors);

                    System.Drawing.Color l_color_layer = System.Drawing.Color.FromArgb(li_rgb.Argb);
                    
                    if(l_color_layer == System.Drawing.Color.FromArgb(255,255,255))
                    {
                        l_color_layer = System.Drawing.Color.FromArgb(0, 0, 0);
                    }
                    return l_color_layer;
                }

                if(a_dxf_entity.Color.ColorType == ColorType.ByColorIndex)
                {
                    //does not work
                    WW.Drawing.ArgbColor li_rgb = a_dxf_entity.Color.ToArgbColor(DxfIndexedColorSet.AcadClassicIndexedColors);
                    System.Drawing.Color l_color_layer = System.Drawing.Color.FromArgb(li_rgb.Argb);
                    
                    if (l_color_layer == System.Drawing.Color.FromArgb(255, 255, 255))
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

        public static DxfNet.QLin DxfLineType_2_QLin(DxfLineType a_dxf_line_type, double a_scale_drawing, double a_scale_line_type)
        {
            QLin l_lin = new QLin();

            if (0 == String.Compare(a_dxf_line_type.Name, DxfLineType.LineTypeNameByLayer, StringComparison.OrdinalIgnoreCase))
            {
                return l_lin;
            }

      

            if (a_dxf_line_type == null)
            {
                return l_lin;
            }

            l_lin.m_name = a_dxf_line_type.Name;


            

            string ls_body = "A,";
            bool lb_first = true;
            foreach (DxfLineType.Element l_item in a_dxf_line_type.Elements)
            {
                if (!lb_first)
                {
                    ls_body += ",";
                }

                double ld_item = a_scale_drawing * a_scale_line_type * l_item.Length / 100d;
                string ls_item = ld_item.ToString(CultureInfo.InvariantCulture);
                ls_body += ls_item;
                lb_first = false;
            }
            l_lin.m_body = ls_body;

            return l_lin;
        }


    }
}
