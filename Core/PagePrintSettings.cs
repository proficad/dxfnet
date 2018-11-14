using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using DxfNet;

namespace Core
{
    public class PagePrintSettings
    {
        public int PaperSizeEnum;
        public Size sheet_size;
        public string FormName;

        public void Init_From_Old_Page_Settings(SettingsPage a_page, PrintSettings a_print, SettingsPrinter a_printer)
        {
            PaperSizeEnum = a_print.PaperSizeEnum;

            sheet_size = Helper.Sniff_Page_Size(a_print.PaperSizeEnum, a_print.FormName);

            if (sheet_size.IsEmpty)
            {
                sheet_size = Helper.Size_Tenths_mm_2_mm(a_printer.PaperSize);
            }

            FormName = a_print.FormName;

            Helper.EnumPageOri l_ori = a_print.GetPageOri();
            SetOrientation(l_ori);

        }

        Helper.EnumPageOri GetOrientation()
        {
	        return sheet_size.Width > sheet_size.Height ? 
                Helper.EnumPageOri.OriLandscape : Helper.EnumPageOri.OriPortrait;
        }

        void SetOrientation(Helper.EnumPageOri a_val)
        {
            if (GetOrientation() != a_val)
            {
                int li_temp = sheet_size.Width;
                sheet_size.Width = sheet_size.Height;
                sheet_size.Height = li_temp;
            }
        }



        //-------------------------------
    }
}
