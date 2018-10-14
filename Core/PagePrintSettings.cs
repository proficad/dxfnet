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

        public void Init_From_Old_Page_Settings(SettingsPage a_page, PrintSettings a_print, SettingsPrinter a_printer)
        {
            PaperSizeEnum = a_print.PaperSizeEnum;
            sheet_size = Helper.Size_Tenths_mm_2_mm(a_printer.PaperSize);
            
        }
    }
}
