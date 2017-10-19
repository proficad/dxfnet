using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DxfNet
{

    public enum ObjType
    {
        OTnothing,
        OTrectdraw,
        OTpoly,
        OTpolyline,
        OTbezier,
        OTtext,
        OTelem,
        OTspoj,
        OTOLEObj,
        OTjunction,
        OTimage
    };

    public enum Shape
    {
        shapeNone, 
        rectangle, 
        roundRectangle, 
        ellipse, 
        line, 
        poly, 
        polyline, 
        bezier, 
        chord, pie, arc, spoj, text, soucastka, outlet, cable };

    public abstract class DrawObj
    {
        public DrawObj(Shape a_shape, Rectangle a_rect)
        {
            m_nShape = a_shape;
            m_objProps = new ObjProps();
            m_position = a_rect;
            
        }

        //for DrawPoly
        public DrawObj(Shape a_shape)
        {
            m_nShape = a_shape;
            m_objProps = new ObjProps();
        }

        public Rectangle m_position;
        public string m_text;
        bool m_bTurnWithSymbol;
        public ObjProps m_objProps;
        ObjType m_ObjType;
        public Shape m_nShape;
        public Layer m_layer;

        internal abstract void Write2Xml(System.Xml.XmlWriter a_xmlWriter);

        internal Point GetCenterPoint()
        {
            Point l_result = new Point();
            l_result.X = (m_position.Left + m_position.Right) / 2;
            l_result.Y = (m_position.Top + m_position.Bottom) / 2;
            return l_result;
        }




        internal abstract void RecalcBounds(ref MyRect l_bounds);

        internal abstract void MoveBy(Size l_offset);

        public abstract void RecalcPosition();

    }
}
