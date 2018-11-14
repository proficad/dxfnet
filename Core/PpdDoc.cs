using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DxfNet
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

        public Size m_offset;
        public Repo m_repo = new Repo();


        internal void SaveToXml(System.Xml.XmlWriter a_xmlWriter)
        {
            a_xmlWriter.WriteStartElement("ppd");//name lG fG
            a_xmlWriter.WriteAttributeString("name", m_name);
            a_xmlWriter.WriteAttributeString("fG", m_fG);
            a_xmlWriter.WriteAttributeString("lG", m_lG);
            WriteElements(a_xmlWriter);
            a_xmlWriter.WriteEndElement();
        }


        public void RecalcToBeInCenterPoint(Point a_basePoint)
        {

            if (m_objects.Count == 0)
            {
                return;
            }


            //verify it
            Rectangle l_rectPos = GetPosition();
            Point l_centerPoint = Helper.GetRectCenterPoint(l_rectPos);


            //calc bounds
            MyRect l_bounds = new MyRect();
            //init the rect from the 1st object
            l_bounds.Left = m_objects[0].m_position.Left;
            l_bounds.Right = m_objects[0].m_position.Right;
            l_bounds.Top = m_objects[0].m_position.Top;
            l_bounds.Bottom = m_objects[0].m_position.Bottom;


            foreach (DrawObj a_obj in m_objects)
            {
                System.Diagnostics.Debug.WriteLine(a_obj.GetType());
                a_obj.RecalcBounds(ref l_bounds);
            }

            //calc center
            Point l_center = Helper.GetRectCenterPoint(l_bounds);

            m_offset = new Size(l_center.X, l_center.Y);

            //move all points to make the centerpoint be 0,0
            Size l_offsetOpposite = new Size(-m_offset.Width, -m_offset.Height);
            foreach (DrawObj a_obj in m_objects)
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


        public Rectangle GetPosition()
        {

            if (m_objects.Count == 0)
            {
                return new Rectangle();
            }

            m_objects[0].RecalcPosition();
            Rectangle l_rect = m_objects[0].m_position;
            foreach (DrawObj a_obj in m_objects)
            {
                a_obj.RecalcPosition();
                System.Diagnostics.Debug.WriteLine("GetPosition Union with " + a_obj.m_position.ToString());
                l_rect = Rectangle.Union(l_rect, a_obj.m_position);
            }
            return l_rect;
        }

        public List<Point> Vyvody
        {
            get
            {
                List<Point> l_vyvody = new List<Point>();
                foreach(var l_object in m_objects)
                {
                    if(l_object is Outlet)
                    {
                        Outlet l_outlet = l_object as Outlet;
                        Point l_point = new Point();
                        l_point.X = l_outlet.X;
                        l_point.Y = l_outlet.Y;
                        l_vyvody.Add(l_point);
                    }
                }
                return l_vyvody;
            }
        }



        //---------------------------

        
    }
}
