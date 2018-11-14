using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using DxfNet;

namespace Core
{
    public class PageSizeSettings
    {
        public int PaperSizeEnum;
        public Size sheet_size;

        public enum EnumPaperSizeSource { PSS_Print /*by print settings*/, PSS_Custom, PSS_Predefined };
        public EnumPaperSizeSource m_source;

        public MyRect PageMargins;
        public Size SheetsCount { get; set; }



        public void Init_From_Old_Page_Settings(SettingsPage a_page, PrintSettings a_print, SettingsPrinter a_printer)
        {
            bool lb_wantsCustom = a_print.WantsCustom;
            if (lb_wantsCustom)
            {
                sheet_size = new Size(a_print.SheetSizeX, a_print.SheetSizeY);
                m_source = EnumPaperSizeSource.PSS_Custom;
            }
            else
            {
                sheet_size = Helper.Size_Tenths_mm_2_mm(a_printer.PaperSize);
                m_source = EnumPaperSizeSource.PSS_Print;
            }

            PaperSizeEnum = a_print.PaperSizeEnum;

            SheetsCount = new Size(a_page.PagesHor, a_page.PagesVer);
            PageMargins = a_page.PageMargins;

            if (!a_page.IncludeMargins)//old setting to use all possible space
            {
                //try to calculate margins
                //const int li_margin_hor = 
                int li_size_diff_x = sheet_size.Width - a_print.SheetSizeX;
                int li_size_diff_y = sheet_size.Height - a_print.SheetSizeY;

                if((li_size_diff_x > 0) && (li_size_diff_y > 0))
                {
                    PageMargins.Left = li_size_diff_x / 2;
                    PageMargins.Top = li_size_diff_y / 2;
                    PageMargins.Right = li_size_diff_x - PageMargins.Left;
                    PageMargins.Bottom = li_size_diff_y - PageMargins.Top;
                }

            }
        }

    }
}
