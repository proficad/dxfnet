using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Printing;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;

namespace DxfNet
{
    public class PCadDoc : DrawDoc
    {
        public bool m_isPortrait;
        public int m_ratio_tisk;
        public int m_zoomView;
        public Rectangle m_rectUserMarginLog;
        public bool m_popisovepole;
        public bool m_oramovani;
        public Size m_size;
        public int m_rastr;//rastr in 0.1mm
        public PtbPosition m_ptbPosition = new PtbPosition();//version 8, each page may have its own TB

        public Hashtable m_summInfo = new Hashtable();//2012-11-02
 

 
        public List<Layer> m_layers;

        public PCadDoc(CollPages a_parent)
        {
            m_parent = a_parent;
            m_layers = new List<Layer>();
        }
        //-------------------

        public static Size GetPaperSize()
        {
            PageSettings pageSettings = new PageSettings();

            int li_width = 2036;
            int li_height = 2922;

            try
            {
                RectangleF rect = pageSettings.PrintableArea;

                const float INCH = 2.54F;
                li_width = (int)(rect.Width * INCH);
                li_height = (int)(rect.Height * INCH);
            }
            catch (InvalidPrinterException)
            {
            }

            return new Size(li_width, li_height);
        }



        public void Save(string as_path)
        {
            //XmlDocument l_xmlDoc = new XmlDocument();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;
            
            XmlWriter l_xmlWriter = XmlWriter.Create(as_path, settings);

            l_xmlWriter.WriteStartDocument();

            WriteIntroElement(l_xmlWriter);
            WriteFontCollection(l_xmlWriter);
            WritePageSettings(l_xmlWriter);
            WriteRepo(l_xmlWriter);


            WriteElements(l_xmlWriter);


            //close the Intro element
            l_xmlWriter.WriteEndElement();



            l_xmlWriter.WriteEndDocument();
            l_xmlWriter.Close();
        }

        internal void WriteElements(XmlWriter a_xmlWriter)
        {
            a_xmlWriter.WriteStartElement("pages");
            a_xmlWriter.WriteStartElement("page");
            a_xmlWriter.WriteAttributeString("name", "1");

            a_xmlWriter.WriteStartElement("layers");
            a_xmlWriter.WriteStartElement("layer");
            a_xmlWriter.WriteAttributeString("name", "0");
            a_xmlWriter.WriteAttributeString("v", "1");


            foreach (DrawObj a_obj in m_objects)
            {
                a_obj.Write2Xml(a_xmlWriter);
            }

            a_xmlWriter.WriteEndElement();
            a_xmlWriter.WriteEndElement();
            a_xmlWriter.WriteEndElement();
            a_xmlWriter.WriteEndElement();
        }

        private void WritePageSettings(XmlWriter a_xmlWriter)
        {
            a_xmlWriter.WriteStartElement("PageSettings");
            a_xmlWriter.WriteAttributeString("bgColor", "FFFFFF");
            a_xmlWriter.WriteAttributeString("pgsHor", Parent.m_settingsPage.PagesHor.ToString());
            a_xmlWriter.WriteAttributeString("pgsVer", Parent.m_settingsPage.PagesVer.ToString());
            a_xmlWriter.WriteEndElement();
        }

        private void WriteRepo(XmlWriter a_xmlWriter)
        {
            a_xmlWriter.WriteStartElement("repo");
            foreach (PpdDoc l_ppdDoc in GetRepo().m_listPpd)
                {
                    l_ppdDoc.SaveToXml(a_xmlWriter);
                }
            a_xmlWriter.WriteEndElement();
        }

        private void WriteFontCollection(XmlWriter a_xmlWriter)
        {
            a_xmlWriter.WriteStartElement("fonts");
                a_xmlWriter.WriteAttributeString("letter", "155,0,0,0,Lucida Sans Unicode,000000");
                a_xmlWriter.WriteAttributeString("text", "110,0,0,0,Lucida Sans Unicode,000000");
                a_xmlWriter.WriteAttributeString("type", "85,0,0,0,Arial,000000");
                a_xmlWriter.WriteAttributeString("value", "85,0,0,0,Arial,000000");
            a_xmlWriter.WriteEndElement();
        }


        /*
        private void WriteElement(XmlWriter a_xmlWriter, DrawObj a_obj)
        {
            a_xmlWriter.WriteStartElement("polyline");
            a_xmlWriter.WriteAttributeString("op-lw", "10");
            a_xmlWriter.WriteAttributeString("op-lc", "33023");
            a_xmlWriter.WriteAttributeString("pts", "0,0,200,400,");
            a_xmlWriter.WriteEndElement();
        }
        */


        private void WriteIntroElement(XmlWriter a_xmlWriter)
        {
            a_xmlWriter.WriteStartElement("document");
            a_xmlWriter.WriteAttributeString("type", "ProfiCAD sxe");
            a_xmlWriter.WriteAttributeString("version", "7.0");
            
//99            a_xmlWriter.WriteAttributeString("pgsHor", Parent.m_pagesHor.ToString());
//99            a_xmlWriter.WriteAttributeString("pgsVer", Parent.m_pagesVer.ToString());
            a_xmlWriter.WriteAttributeString("pgsOri", "LANDSCAPE");
            a_xmlWriter.WriteAttributeString("pgsMrgn", "0, 0, 1, 1");
            a_xmlWriter.WriteAttributeString("zoom", "100");
            a_xmlWriter.WriteAttributeString("rastr", "20");
        }



        //----------------------------

        public void AddPpdDoc(PpdDoc l_ppdDoc)
        {
            GetRepo().AddPpd(l_ppdDoc);
        }

        public PpdDoc FindPpdDocInRepo(string ls_lastGuid)
        {
            PpdDoc l_ppdDoc = GetRepo().FindPpdDocInRepo(ls_lastGuid);
            return l_ppdDoc;
            
        }

        public void RecalcToFitInPaper()
        {
            Rectangle l_rectDocument;

            if (m_objects.Count == 0)
            {
                return;
            }

            foreach (DrawObj a_obj in m_objects)
            {
                a_obj.RecalcPosition();
            }            
            
            l_rectDocument = m_objects[0].m_position;


            foreach (DrawObj a_obj in m_objects)
            {
                l_rectDocument = Rectangle.Union(l_rectDocument, a_obj.m_position);
            }


            Size l_offset = new Size(0, 0);
            if (l_rectDocument.X < 0)
            {
                l_offset.Width = -l_rectDocument.X;
            }
            if (l_rectDocument.Y < 0)
            {
                l_offset.Height = -l_rectDocument.Y;
            }
            if((l_offset.Width != 0) || (l_offset.Height != 0))
            {
                foreach (DrawObj a_obj in m_objects)
                {
                    a_obj.MoveBy(l_offset);
                }
            }

            Size l_paperSize = GetPaperSize();

            if (l_rectDocument.Width > l_rectDocument.Height)//landscape
            {
                int li_temp = l_paperSize.Width;
                l_paperSize.Width = l_paperSize.Height;
                l_paperSize.Height = li_temp;
            }

            Parent.m_settingsPage.PagesHor = 1 + (l_rectDocument.Width / l_paperSize.Width);
            Parent.m_settingsPage.PagesVer = 1 + (l_rectDocument.Height / l_paperSize.Height);

        }

        public void SetSize(Size a_size)
        {
            Size l_paperSize = GetPaperSize();


            Parent.m_settingsPage.PagesHor = 1 + (a_size.Width / l_paperSize.Width);
            Parent.m_settingsPage.PagesVer = 1 + (a_size.Height / l_paperSize.Height);

        }



        public string Name { get; set; }

        private CollPages m_parent;
        public CollPages Parent
        {
            get {return m_parent;}
        }

        public Repo GetRepo()
        {
            return m_parent.m_repo;
        }


        public bool OriByPage { get; set; }

        public int Orientation { get; set; }//0 == portrait    1 == landscape

        public Size GetSize()
        {
            Size l_size = Parent.GetSize();

            if (OriByPage)
            {
                bool lb_global_landscape = l_size.Width > l_size.Height;
                bool lb_this_page_landscape = (Orientation == 1);

                if (lb_global_landscape != lb_this_page_landscape)
                {
                    int li_temp = l_size.Width;
                    l_size.Width = l_size.Height;
                    l_size.Height = li_temp;
                }
            }

            return l_size;
        }

        internal void SetupWireStatusConnected()
        {
            List<Wire> l_list_of_wires = new List<Wire>();
            GetAllWiresFromThisPage(l_list_of_wires);

            List<Point> l_vyvody = new List<Point>();

            PripravVyvody(l_vyvody);

            foreach(Wire l_wire in l_list_of_wires)
            {
                SetupWireStatusConnected(l_wire, l_vyvody);
                l_wire.SetupWireStatusConnectedKapky();
            }


        }

        private void SetupWireStatusConnected(Wire a_wire, List<Point> a_vyvody)
        {
            a_wire.Is_connected_first = false;
            a_wire.Is_connected_last = false;

            foreach(Point l_point in a_vyvody)
            {
                if (l_point == a_wire.GetEndingPoint(true))
                {
                    a_wire.Is_connected_first = true;

                    if (a_wire.Is_connected_first && a_wire.Is_connected_last)
                    {
                        return;
                    }
                }
                if (l_point == a_wire.GetEndingPoint(false))
                {
                    a_wire.Is_connected_last = true;

                    if (a_wire.Is_connected_first && a_wire.Is_connected_last)
                    {
                        return;
                    }
                }


            }
        }

        private void GetAllWiresFromThisPage(List<Wire> a_list_of_wires)
        {
           foreach(DrawObj l_obj in m_objects)
           {
               if(l_obj is Wire)
               {
                   Wire l_wire = l_obj as Wire;
                   a_list_of_wires.Add(l_wire);
               }
           }
        }


        private void PripravVyvody(List<Point> a_vyvody)
        {
            List<Insert> l_list_CElems = new List<Insert>();
	
	        GetAllCElems(l_list_CElems);

            //Repo l_repo = GetRepo();

            foreach(Insert l_insert in l_list_CElems)
	        {
                Point l_center_point = Helper.GetRectCenterPoint(l_insert.m_position);

                PpdDoc l_ppd = FindPpdDocInRepo(l_insert.m_lG);
                foreach(Point l_vyvod in l_ppd.Vyvody)
                {
                    Point l_point = l_vyvod;
                    l_point.X += l_center_point.X;
                    l_point.Y += l_center_point.Y;
                    a_vyvody.Add(l_insert.RecalculatePoint(l_point));
                }
	        } 

        }


        private void GetAllCElems(List<Insert> l_list_CElems)
        {
            foreach (DrawObj l_obj in m_objects)
            {
                if (l_obj is Insert)
                {
                    Insert l_wire = l_obj as Insert;
                    l_list_CElems.Add(l_wire);
                }
            }
        }

    }
}
