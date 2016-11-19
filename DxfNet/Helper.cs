using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DxfNet
{
    public static class Helper
    {
        internal static string EncodeHtml(string as_input)
        {
            as_input = as_input.Replace("&", "&amp;");
            as_input = as_input.Replace("<", "&lt;");
            as_input = as_input.Replace(">", "&gt;");
            as_input = as_input.Replace("'", "&apos;");
            as_input = as_input.Replace("\"", "&quot;");
            return as_input;
        }

        internal static string Color2String(Color a_input)
        {
            string ls_color = string.Format("{0:D2}{1:D2}{2:D2}", a_input.R, a_input.G, a_input.B);
            return ls_color;
        }

        public static Point GetRectCenterPoint(MyRect a_rect)
        {
            int li_x = (a_rect.Left + a_rect.Right) / 2;
            int li_y = (a_rect.Top + a_rect.Bottom) / 2;
            return new Point(li_x, li_y);
        }

        public static Point GetRectCenterPoint(Rectangle a_rect)
        {
            int li_x = a_rect.Left + (a_rect.Width / 2);
            int li_y = a_rect.Top + (a_rect.Height / 2);
            return new Point(li_x, li_y);
        }

        public static Point GetRectCenterPoint(RectangleF a_rect)
        {
            int li_x = (int)(a_rect.Left + (a_rect.Width / 2));
            int li_y = (int)(a_rect.Top + (a_rect.Height / 2));
            return new Point(li_x, li_y);
        }

        public static void Swap<T>(ref T x, ref T y)
        {
            T t = y;
            y = x;
            x = t;
        }

        internal static int EasyDistance2Points(Point a_point1, Point a_point2)
        {
            return
            Math.Abs(a_point1.X - a_point2.X) + Math.Abs(a_point1.Y - a_point2.Y);
        }
    }
}
