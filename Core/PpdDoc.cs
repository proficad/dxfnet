using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using DxfNet;

namespace Core
{
    public class PpdDoc : DrawDoc
    {
        public string m_name;
        public string m_fG;
        public string m_lG;
        public string m_defType;
        public string m_defValue;
        public string m_norm;
        public string m_memo;

        public SizeF m_offset;


        public PpdDoc()
        {
            m_fG = m_lG = Guid.NewGuid().ToString();
        }

        public void Purge()
        {
            List<String> l_list_lg = new List<string>();

            foreach (DrawObj l_obj in this)
            {
                if (l_obj is Insert l_insert)
                {
                    if (!string.IsNullOrEmpty(l_insert.m_lG))
                    {
                        l_list_lg.Add(l_insert.m_lG);
                    }
                }
            }


            //delete all items from repo that are not in l_list_lg
            this.m_repo.m_listPpd.RemoveAll(x => !l_list_lg.Contains(x.m_lG));
        }


        public override void Save(string as_path)
        {
            Purge();


            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;

            XmlWriter l_xmlWriter = XmlWriter.Create(as_path, settings);

            if (l_xmlWriter != null)
            {
                l_xmlWriter.WriteStartDocument();

                l_xmlWriter.WriteStartElement("document");
                l_xmlWriter.WriteAttributeString("type", "ProfiCAD ppd");
                l_xmlWriter.WriteAttributeString("version", "12.0");
                
                //a_xmlWriter.WriteAttributeString("name", m_name);
                
                l_xmlWriter.WriteAttributeString("fG", m_fG);
                l_xmlWriter.WriteAttributeString("lG", m_lG);

                SaveToXmlInner(l_xmlWriter);
               
                l_xmlWriter.Close();
            }

        }


        internal void SaveToXml(System.Xml.XmlWriter a_xmlWriter)
        {
            if(m_layers.Count == 0)
            {
                return;
            }

            a_xmlWriter.WriteStartElement("ppd");
            a_xmlWriter.WriteAttributeString("name", m_name);
            a_xmlWriter.WriteAttributeString("version", "12.0");

            a_xmlWriter.WriteAttributeString("fG", m_fG);
            a_xmlWriter.WriteAttributeString("lG", m_lG);


            SaveToXmlInner(a_xmlWriter);

            a_xmlWriter.WriteEndElement();

        }


  
        public void RecalcToBeInCenterPoint(PointF a_basePoint)
        {
            DrawObj l_obj = (DrawObj)this.GetEnumerator().Current;

            if (l_obj == null)
            { 
                return;
            }
            

            //verify it
            RectangleF l_rectPos = GetPosition();
            PointF l_centerPoint = Helper.GetRectCenterPoint(l_rectPos);


            //calc bounds
            MyRect l_bounds = new MyRect();
            //init the rect from the 1st object
            l_bounds.Left   = l_obj.m_position.Left;
            l_bounds.Right  = l_obj.m_position.Right;
            l_bounds.Top    = l_obj.m_position.Top;
            l_bounds.Bottom = l_obj.m_position.Bottom;


            foreach (DrawObj a_obj in this)
            {
                System.Diagnostics.Debug.WriteLine(a_obj.GetType());
                a_obj.RecalcBounds(ref l_bounds);
            }

            //calc center
            PointF l_center = Helper.GetRectCenterPoint(l_bounds);

            m_offset = new SizeF(l_center.X, l_center.Y);

            //move all points to make the centerpoint be 0,0
            SizeF l_offsetOpposite = new SizeF(-m_offset.Width, -m_offset.Height);
            foreach (DrawObj a_obj in this)
            {
                string ls_debugEcho = "moving from " + a_obj.m_position.ToString();
                a_obj.MoveBy(l_offsetOpposite);
                ls_debugEcho += " to " + a_obj.m_position.ToString();
                System.Diagnostics.Debug.WriteLine(ls_debugEcho);
            }

            //verify it
            l_rectPos = GetPosition();
            l_centerPoint = Helper.GetRectCenterPoint(l_rectPos);

            //correct offset according to the basepoint
            m_offset.Width -= a_basePoint.X;
            m_offset.Height -= a_basePoint.Y;

        }


        public RectangleF GetPosition()
        {
            DrawObj l_obj = (DrawObj)this.GetEnumerator().Current;

            if (l_obj == null)
            {
                return new Rectangle();
            }


            l_obj.RecalcPosition();
            RectangleF l_rect = l_obj.m_position;
            foreach (DrawObj a_obj in this)
            {
                a_obj.RecalcPosition();
                System.Diagnostics.Debug.WriteLine("GetPosition Union with " + a_obj.m_position.ToString());
                l_rect = RectangleF.Union(l_rect, a_obj.m_position);
            }
            return l_rect;
        }


        public Repo GetRepo()
        {
            return m_repo;
        }


        private void SaveToXmlInner(System.Xml.XmlWriter a_xmlWriter)
        {
            WriteRepo(a_xmlWriter);
            WriteElements(a_xmlWriter);
        }


        public void Recalc_Size(float af_x, float af_y)
        {
            foreach (DrawObj a_obj in this)
            {
                a_obj.Recalc_Size(af_x, af_y);
                //System.Diagnostics.Debug.WriteLine("GetPosition Union with " + a_obj.m_position.ToString());
            }
        }


        public List<Point> Vyvody
        {
            get
            {
                List<Point> l_vyvody = new List<Point>();
                foreach(var l_object in this)
                {
                    if(l_object is Outlet lOutlet)
                    {
                        Point l_point = new Point();
                        l_point.X = lOutlet.X;
                        l_point.Y = lOutlet.Y;
                        l_vyvody.Add(l_point);
                    }
                }
                return l_vyvody;
            }
        }



        //---------------------------

        
    }
}
