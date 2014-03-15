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


        public PrintSettings m_printSettings;
        public SettingsPage m_settingsPage;

        public Color m_paperColor;

        public Hashtable m_summInfo;
        public QFontsCollection m_fonts;

        public Repo m_repo;
        public string m_path;
        public PtbPosition m_ptbPosition;
        public bool m_show_types;
        public bool m_show_values;


        public CollPages()
        {
            m_repo = new Repo();
            m_fonts = new QFontsCollection();
            m_ptbPosition = new PtbPosition();
            m_summInfo = new Hashtable();
            m_printSettings = new PrintSettings();
            m_settingsPage = new SettingsPage();
        }

        public Size GetSize()
        {
            if (m_printSettings.WantsCustom)
            {
                return new Size(10 * m_printSettings.CustomSizeX, 10 * m_printSettings.CustomSizeY);
            }
            else
            {
                return new Size(10 * m_settingsPage.PagesHor * m_printSettings.SheetSizeX, 10 * m_settingsPage.PagesVer * m_printSettings.SheetSizeY);
            }
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









        //--------------------------
    }
}
