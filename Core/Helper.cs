using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Core;

namespace DxfNet
{
    public static class Helper
    {
        public enum EnumPageOri { OriPortrait, OriLandscape };


        private const short _PROFICAD_DMPAPER_A0 = -10;
        private const short _PROFICAD_DMPAPER_A1 = -11;
        private const short DMPAPER_A2 = 66;
        private const short DMPAPER_A3 = 8;
        private const short DMPAPER_A4 = 9;
        private const short DMPAPER_LETTER = 1;
        private const short DMPAPER_LEGAL = 5;
        private const short DMPAPER_LEDGER = 4;
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


        internal static Size Size_Tenths_mm_2_mm(Size a_size)
        {
            Size l_size = new Size(
                Tenths_2_mm_Round(a_size.Width),
                Tenths_2_mm_Round(a_size.Height)
            );

            return l_size;
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


        private static int Tenths_2_mm_Round(int ai_input)
        {
            int li_div = ai_input / 10;
            int li_mod = ai_input % 10;

            if (li_mod > 5)
            {
                ++li_div;
            }

            return li_div;
        }

        public static Size Sniff_Page_Size(int ai_paper_size_enum, string as_form_name)
        {
            List<QPaperSize> l_list = Get_List_Paper_Sizes();

            Size l_size = l_list.Where(x => x.PaperSizeEnum == ai_paper_size_enum).Select(q => q.SheetSize).SingleOrDefault();
          
            if(l_size.IsEmpty)
            {
                l_size = l_list.Where(x => Form_Name_Match(x.FormName, as_form_name)).Select(q => q.SheetSize).SingleOrDefault();
            }
            
            return l_size;
        }


        private static bool Form_Name_Match(string as_name_1, string as_name_2)
        {
            string ls_name_1 = Get_First_Part(as_name_1);
            string ls_name_2 = Get_First_Part(as_name_2);

            return (ls_name_1 == ls_name_2);
        }

        private static string Get_First_Part(string as_input)
        {
            return as_input.Split(' ')[0];
        }

        private static List<QPaperSize> Get_List_Paper_Sizes()
        {
            List<QPaperSize> l_list = new List<QPaperSize>();

            l_list.Add(new QPaperSize(_PROFICAD_DMPAPER_A0, "A0 (1189 x 841 mm)",   new Size(841, 1189)));
            l_list.Add(new QPaperSize(_PROFICAD_DMPAPER_A1, "A1 (841 x 594 mm)",    new Size(594, 841)));
            l_list.Add(new QPaperSize(DMPAPER_A2,           "A2 (594 x 420 mm)",    new Size(420, 594)));
            l_list.Add(new QPaperSize(DMPAPER_A3,           "A3 (420 x 297 mm)",    new Size(297, 420)));
            l_list.Add(new QPaperSize(DMPAPER_A4,           "A4 (297 x 210 mm)",    new Size(210, 297)));
            l_list.Add(new QPaperSize(DMPAPER_LETTER,       "letter (11 x 8.5\")",  new Size(216, 279)));
            l_list.Add(new QPaperSize(DMPAPER_LEGAL,        "legal (14 x 8.5\")",   new Size(216, 356)));
            l_list.Add(new QPaperSize(DMPAPER_LEDGER,       "ledger (17 x 11\")",   new Size(432, 279)));


            return l_list;
        }
     //------------------------------------  
    }
}
