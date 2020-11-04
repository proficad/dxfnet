using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Xml;
using System.Collections;
using Core;

namespace DxfNet
{
    public abstract class DrawDoc : IEnumerable
    {

        public IEnumerator MyEnum()
        {
            foreach (Layer l_layer in m_layers)
            {
                foreach (DrawObj l_obj in l_layer)
                {
                    yield return l_obj;
                }
            }
        }

        public IEnumerator GetEnumerator()
        {
            foreach (Layer l_layer in m_layers)
            {
                foreach(DrawObj l_obj in l_layer)
                {
                    yield return l_obj;
                }
            }
        }


        public readonly List<Layer> m_layers = new List<Layer>();
        public readonly Repo m_repo = new Repo();


        //used in ProfiCAD->DXF
        public void Add(DrawObj a_drawObj, Layer a_layer)
        {
            a_drawObj.m_layer = a_layer;
            Layer l_layer = FindLayer(a_layer.Name);
            l_layer?.Add(a_drawObj);
        }

        //used in DXF->ProfiCAD
        public void Add(DrawObj a_drawObj, string as_layer_name, bool ab_merge_layers)
        {
            if (ab_merge_layers)
            {
                as_layer_name = "0";
            }

            Layer l_layer = FindLayer(as_layer_name);
            if (l_layer == null)
            {
                l_layer = new Layer(as_layer_name);
                AddLayer(l_layer);
            }

            a_drawObj.m_layer = l_layer;
            l_layer.Add(a_drawObj);
        }

        internal void WriteElements(XmlWriter a_xmlWriter)
        {
            a_xmlWriter.WriteStartElement("layers");

            foreach(Layer l_layer in m_layers)
            {
                l_layer.SaveToXml(a_xmlWriter);
            }

            a_xmlWriter.WriteEndElement();
        }

        public Rectangle GetUsedRect()
        { 
	        Rectangle l_usedrect = new Rectangle();
            Rectangle l_position = new Rectangle();
	        bool	  lb_first = true;

	        foreach(DrawObj l_obj in this)
	        {
                l_obj.RecalcPosition();//2012-11-05
                l_position = l_obj.m_position;
		        //l_position.NormalizeRect();

		        if(lb_first){
			        l_usedrect = l_position;
			        lb_first = false;
		        }
		        else{
			        l_usedrect = Rectangle.Union(l_usedrect, l_position);
			        //l_usedrect.NormalizeRect();
		        }
	        }

	        if (lb_first)
		        return new Rectangle();
	        else
	        {
		        return l_usedrect;
	        }
        }

        private Layer FindLayer(string as_layername)
        {
            foreach(Layer l_layer in m_layers)
            {
                if(l_layer.Name == as_layername)
                {
                    return l_layer;
                }
            }

            return null;
        }

        public void AddLayer(Layer a_layer)
        {
            m_layers.Add(a_layer);
        }

        public abstract void Save(string ls_outputPath);
     

        public virtual void SetSize(Size a_size)
        {
            
        }

        protected void WriteRepo(XmlWriter a_xmlWriter)
        {
            a_xmlWriter.WriteStartElement("repo");
            foreach (PpdDoc l_ppdDoc in m_repo.m_listPpd)
            {
                l_ppdDoc.SaveToXml(a_xmlWriter);
            }
            a_xmlWriter.WriteEndElement();
        }
    }
}
