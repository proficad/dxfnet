using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;



namespace DxfNet
{
    public class CollPages
    {
        public List<PCadDoc> m_pages = new List<PCadDoc>();


        public PrintSettings m_printSettings = new PrintSettings();
        public SettingsPage m_settingsPage = new SettingsPage();
        public SettingsPrinter m_settingsPrinter = new SettingsPrinter();
        public SettingsNumberingWire m_settingsNumberingWire = new SettingsNumberingWire();
        public RefGridSettings m_ref_grid = new RefGridSettings();

        public Color m_paperColor;

        public Hashtable m_summInfo = new Hashtable();
        public QFontsCollection m_fonts = new QFontsCollection();

        public Repo m_repo = new Repo();
        public string m_path;
        public PtbPosition m_ptbPosition = new PtbPosition();
        public bool m_show_types;
        public bool m_show_values;
        public int Version;

        public CollPages()
        {
        }

        public Size GetSize()
        {
            if (m_printSettings.WantsCustom)
            {
                return new Size(10 * m_printSettings.CustomSizeX, 10 * m_printSettings.CustomSizeY);
            }
            else
            {
                if (m_settingsPage.IncludeMargins)
                {
                    Rectangle l_rect_result = new Rectangle();
                    Point l_point = new Point();
                    bool lb_landscape = IsLandscape();
                    Get_Intersection_PA_DA(out l_rect_result, l_point, 
                        m_settingsPage, m_printSettings, m_settingsPrinter, lb_landscape, false);


                    return new Size(
                        m_settingsPage.PagesHor * l_rect_result.Width,
                        m_settingsPage.PagesVer * l_rect_result.Height
                    );
                }
                else
                {
                    return new Size(10 * m_settingsPage.PagesHor * m_printSettings.SheetSizeX, 10 * m_settingsPage.PagesVer * m_printSettings.SheetSizeY);
                }
               
            }
        }

        private bool IsLandscape()
        {
            return m_printSettings.IsLandscape();
        }

        void Get_Intersection_PA_DA(out Rectangle a_rectResult,
                                System.Drawing.Point a_origin,
                                SettingsPage pSettingsPage,
                                PrintSettings pSettingsPrint,
                                SettingsPrinter pSettingsPrinter,
                                bool ab_landscape,
                                bool ab_do_not_swap_sizes)
        {


            // Get the size of the PA in mm
            int li_sheet_width = pSettingsPrint.SheetSizeX;
            int li_sheet_height = pSettingsPrint.SheetSizeY;

            bool lb_is_landscape = li_sheet_width > li_sheet_height;
            bool lb_need_to_swap = (lb_is_landscape != ab_landscape);

		    if (ab_do_not_swap_sizes)
		    {
			    lb_need_to_swap = false;
		    }


            //pokus o zjisteni velikosti papiru desetiny mm
            int li_paperWidth = pSettingsPrinter.PaperSize.Width;
            int li_paperHeight = pSettingsPrinter.PaperSize.Height;

            bool lb_printer_landscape = (li_paperWidth > li_paperHeight);
		    if (lb_printer_landscape != ab_landscape)
		    {
                DxfNet.Helper.Swap(ref li_paperWidth, ref li_paperHeight);
		    }



            int li_phisicalOffsetTenthMmX = pSettingsPrinter.PhysicalOffsetTenthsMm.X;
            int li_phisicalOffsetTenthMmY = pSettingsPrinter.PhysicalOffsetTenthsMm.Y;

		    if (lb_need_to_swap)
		    {
                DxfNet.Helper.Swap(ref li_sheet_width, ref li_sheet_height);
                DxfNet.Helper.Swap(ref li_phisicalOffsetTenthMmX, ref li_phisicalOffsetTenthMmY);
		    }

		    a_origin.X = li_phisicalOffsetTenthMmX;
		    a_origin.Y = li_phisicalOffsetTenthMmY;


		    //Printable area (where to print if ignoring page margin settings), desetiny mm
		    Rectangle l_rectPhysical = new Rectangle(
                li_phisicalOffsetTenthMmX,
                li_phisicalOffsetTenthMmY,
                10 * li_sheet_width,
                10 * li_sheet_height
            );

            MyRect l_pageMargins = pSettingsPage.PageMargins;

            //area user wants
            Rectangle l_rectUser = new Rectangle(
			            10 * l_pageMargins.Left,
			            10 * l_pageMargins.Top,
                        li_paperWidth  - (10 * l_pageMargins.Right)  - (10 * l_pageMargins.Left),
			            li_paperHeight - (10 * l_pageMargins.Bottom) - (10 * l_pageMargins.Top)
			);

		    //for each edge determine who wins
		    a_rectResult = Rectangle.Intersect(l_rectPhysical, l_rectUser);

    }


        public void AddPageByName(String as_name)
        {
            PCadDoc l_doc = new PCadDoc(this);
            l_doc.Name = as_name;
            m_pages.Add(l_doc);
        }

        public PCadDoc GetLatestPage()
        {
            return m_pages.Last();
        }







        public void SetupWireStatusConnected()
        {
            foreach(PCadDoc l_page in m_pages)
            {
                l_page.SetupWireStatusConnected();
            }
        }

        //--------------------------


    }
}
