using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Drawing;

using WW.Math;
using WW.Cad.IO;
using WW.Cad.Model;
using WW.Cad.Model.Entities;
using WW.Cad.Model.Tables;

using DxfNet;

namespace Loader
{
    internal static class ExporterGate
    {
        static int m_gateName = 1;

        internal static void ExportQGate(WW.Cad.Model.Entities.DxfEntityCollection a_coll, DxfNet.QGate a_gate, DxfNet.PCadDoc a_pCadDoc)
        {
            DxfBlock l_block = new DxfBlock("gate " + m_gateName.ToString());
            m_gateName++;
            ExportContext.Current.BlockCollection.Add(l_block);
            ExportQGateInner(l_block.Entities, a_gate);
            Exporter.ExportInsert(a_coll, a_gate, a_pCadDoc, l_block, null);
        }


        internal static void ExportQGateInner(WW.Cad.Model.Entities.DxfEntityCollection a_coll, DxfNet.QGate a_gate)
        {
            DxfNet.PositionAspect l_posAspect = new PositionAspect(new Point(0, 0), 0, false, false);

            System.Drawing.Color l_textColor = System.Drawing.Color.Black;
            System.Drawing.Color l_penColor = a_gate.m_objProps.m_logpen.m_color;


            int X = (int)l_posAspect.m_pivot.X;//(m_position.left + m_position.right)/2;
            int Y = (int)l_posAspect.m_pivot.Y;//(m_position.top  + m_position.bottom)/2;


            int li_lineThickness = a_gate.m_objProps.m_logpen.m_width;

            DrawPoly l_pouzdro = null;

            //nakreslit obrys
	        if (a_gate.m_ASA)
	        {
            switch(a_gate.m_tvar)
	        {
            case GateShapeType.gst_inv:
		        My_Kruh(a_coll, new PointF(X + 30,Y), 8);
                ExportBud(a_coll, X, Y, l_penColor);
    	        break;
            case GateShapeType.gst_bud:
                ExportBud(a_coll, X, Y, l_penColor);
    	        break;
            case GateShapeType.gst_nand:
		        My_Kruh(a_coll, new PointF(X + 79,Y), 8);
                ExportAnd(a_coll, X, Y, li_lineThickness, l_penColor);
                break;
            case GateShapeType.gst_and:
                ExportAnd(a_coll, X, Y, li_lineThickness, l_penColor);
    	        break;
            case GateShapeType.gst_nor:
		        My_Kruh(a_coll, new PointF(X + 84,Y), 8);
                ExportOr(a_coll, X, Y, l_penColor);
                break;
            case GateShapeType.gst_or:
                ExportOr(a_coll, X, Y, l_penColor);
    	        break;
            case GateShapeType.gst_exnor:
		        My_Kruh(a_coll, new PointF(X + 98,Y), 8);
                ExportExOr(a_coll, X, Y, li_lineThickness, l_penColor);
                break;
            case GateShapeType.gst_exor:
                ExportExOr(a_coll, X, Y, li_lineThickness, l_penColor);
    	        break;
            }//switch
	        }//if
	        else{//ČSN
                Helper.ExportRectangleAux(a_coll, new Point(X - 36, Y - 71), new Point(X + 36, Y + 71), li_lineThickness, l_penColor, true);
                l_pouzdro = new DrawPoly(Shape.polyline, li_lineThickness, l_penColor, new Point(X + 36, Y), new Point(X + 51, Y));
                Helper.ExportPolylineAux(a_coll, l_pouzdro, true);
                if (
                    (a_gate.m_tvar == GateShapeType.gst_exnor) ||
                    (a_gate.m_tvar == GateShapeType.gst_nor) ||
                    (a_gate.m_tvar == GateShapeType.gst_inv) ||
                    (a_gate.m_tvar == GateShapeType.gst_nand)
                )
                {
                    DrawPoly l_vyvod = new DrawPoly(Shape.polyline, li_lineThickness, l_penColor, new Point(X + 36, Y - 10), new Point(X + 51, Y));
                    Helper.ExportPolylineAux(a_coll, l_vyvod, true);
                }

	        }//else

	        //rozšíření kvůli vývodům
            if (!a_gate.m_stesnat)
            {//ne stěsnaně
                l_pouzdro = new DrawPoly(Shape.polyline, li_lineThickness, l_penColor, new Point(X - 36, Y - 36), new Point(X - 36, Y - 80));
                Helper.ExportPolylineAux(a_coll, l_pouzdro, true);
                l_pouzdro = new DrawPoly(Shape.polyline, li_lineThickness, l_penColor, new Point(X - 36, Y + 36), new Point(X - 36, Y + 80));
                Helper.ExportPolylineAux(a_coll, l_pouzdro, true);
	        }
        	    
	        //nakreslit vývody
            if ((a_gate.m_stesnat) && (a_gate.m_ASA))
            {//kreslit stěsnaně a je to asa
                switch (a_gate.m_tvar)
                {
                case GateShapeType.gst_nand:
                case GateShapeType.gst_and:
	            //vývody
                    switch (a_gate.m_pocetvstupu)
                    {
	            case 0://1 vývod
                            MyVyvod(a_coll, X - 36, Y, a_gate.m_c1, li_lineThickness, l_penColor);
	    	        break;
	            case 1://2 vývody
                    MyVyvod(a_coll, X - 36, Y + 20, a_gate.m_c1, li_lineThickness, l_penColor);
                    MyVyvod(a_coll, X - 36, Y - 20, a_gate.m_c2, li_lineThickness, l_penColor);
	    	        break;
	            case 2://3 vývody
                    MyVyvod(a_coll, X - 36, Y + 20, a_gate.m_c1, li_lineThickness, l_penColor);
                    MyVyvod(a_coll, X - 36, Y, a_gate.m_c2, li_lineThickness, l_penColor);
                    MyVyvod(a_coll, X - 36, Y - 20, a_gate.m_c3, li_lineThickness, l_penColor);
	    	        break;
	            case 3://4 vývody
                    MyVyvod(a_coll, X - 36, Y + 25, a_gate.m_c1, li_lineThickness, l_penColor);
                    MyVyvod(a_coll, X - 36, Y + 13, a_gate.m_c2, li_lineThickness, l_penColor);
                    MyVyvod(a_coll, X - 36, Y - 13, a_gate.m_c3, li_lineThickness, l_penColor);
                    MyVyvod(a_coll, X - 36, Y - 25, a_gate.m_c4, li_lineThickness, l_penColor);
	    	        break;
	            case 4://5 vývody
                    MyVyvod(a_coll, X - 36, Y + 25, a_gate.m_c1, li_lineThickness, l_penColor);
                    MyVyvod(a_coll, X - 36, Y + 13, a_gate.m_c2, li_lineThickness, l_penColor);
                    MyVyvod(a_coll, X - 36, Y, a_gate.m_c3, li_lineThickness, l_penColor);
                    MyVyvod(a_coll, X - 36, Y - 13, a_gate.m_c4, li_lineThickness, l_penColor);
                    MyVyvod(a_coll, X - 36, Y - 25, a_gate.m_c5, li_lineThickness, l_penColor);
	    	        break;
	            case 5://6 vývodů
                    MyVyvod(a_coll, X - 36, Y + 23, a_gate.m_c1, li_lineThickness, l_penColor);
                    MyVyvod(a_coll, X - 36, Y + 15, a_gate.m_c2, li_lineThickness, l_penColor);
                    MyVyvod(a_coll, X - 36, Y + 8, a_gate.m_c3, li_lineThickness, l_penColor);
                    MyVyvod(a_coll, X - 36, Y - 8, a_gate.m_c4, li_lineThickness, l_penColor);
                    MyVyvod(a_coll, X - 36, Y - 15, a_gate.m_c5, li_lineThickness, l_penColor);
                    MyVyvod(a_coll, X - 36, Y - 23, a_gate.m_c6, li_lineThickness, l_penColor);
	    	        break;
	            case 6://7 vývody
	    	        MyVyvod(a_coll, X - 36, Y + 23 , a_gate.m_c1, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y + 15 , a_gate.m_c2, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y + 8, a_gate.m_c3, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y, a_gate.m_c4, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y - 8 , a_gate.m_c5, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y - 15 , a_gate.m_c6, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y - 23 , a_gate.m_c7, li_lineThickness, l_penColor);
	    	        break;
	            case 7://8 vývodů
	    	        MyVyvod(a_coll, X - 36, Y + 30 , a_gate.m_c1, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y + 23 , a_gate.m_c2, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y + 15 , a_gate.m_c3, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y + 8, a_gate.m_c4, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y - 8, a_gate.m_c5, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y - 15 , a_gate.m_c6, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y - 23 , a_gate.m_c7, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y - 30 , a_gate.m_c8, li_lineThickness, l_penColor);
	    	        break;
	            }
	            break;
                case GateShapeType.gst_nor:
                case GateShapeType.gst_or:
                case GateShapeType.gst_exnor:
                case GateShapeType.gst_exor:
	            //vývody - pravá strana do oblouku
                switch (a_gate.m_pocetvstupu)
                {
	            case 0://1 vývod
	    	        MyVyvod(a_coll, X - 30, Y, a_gate.m_c1, li_lineThickness, l_penColor);
	    	        break;
	            case 1://2 vývody
	    	        MyVyvod(a_coll, X - 30, Y + 20 , a_gate.m_c1, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 30, Y - 20 , a_gate.m_c2, li_lineThickness, l_penColor);
	    	        break;
	            case 2://3 vývody
	    	        MyVyvod(a_coll, X - 30, Y + 20 , a_gate.m_c1, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 28, Y, a_gate.m_c2, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 30, Y - 20 , a_gate.m_c3, li_lineThickness, l_penColor);
	    	        break;
	            case 3://4 vývody
	    	        MyVyvod(a_coll, X - 30, Y + 25 , a_gate.m_c1, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 28, Y + 13 , a_gate.m_c2, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 28, Y - 13 , a_gate.m_c3, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 30, Y - 25 , a_gate.m_c4, li_lineThickness, l_penColor);
	    	        break;
	            case 4://5 vývody
	    	        MyVyvod(a_coll, X - 30, Y + 25 , a_gate.m_c1, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 28, Y + 13 , a_gate.m_c2, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 28, Y , a_gate.m_c3, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 28, Y - 13 , a_gate.m_c4, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 30, Y - 25 , a_gate.m_c5, li_lineThickness, l_penColor);
	    	        break;
	            case 5://6 vývodů
	    	        MyVyvod(a_coll, X - 30, Y + 23 , a_gate.m_c1, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 28, Y + 15 , a_gate.m_c2, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 28, Y + 8, a_gate.m_c3, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 28, Y - 8 , a_gate.m_c4, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 30, Y - 15 , a_gate.m_c5, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 30, Y - 23 , a_gate.m_c6, li_lineThickness, l_penColor);
	    	        break;
	            case 6://7 vývody
	    	        MyVyvod(a_coll, X - 30, Y + 23 , a_gate.m_c1, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 28, Y + 15 , a_gate.m_c2, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 28, Y + 8, a_gate.m_c3, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 28, Y, a_gate.m_c4, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 28, Y - 8 , a_gate.m_c5, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 30, Y - 15 , a_gate.m_c6, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 30, Y - 23 , a_gate.m_c7, li_lineThickness, l_penColor);
	    	        break;
	            case 7://8 vývodů
	    	        MyVyvod(a_coll, X - 30, Y + 30 , a_gate.m_c1, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 30, Y + 23 , a_gate.m_c2, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 28, Y + 15 , a_gate.m_c3, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 28, Y + 8, a_gate.m_c4, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 28, Y - 8 , a_gate.m_c5, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 30, Y - 15 , a_gate.m_c6, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 30, Y - 23 , a_gate.m_c7, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 28, Y - 30, a_gate.m_c8, li_lineThickness, l_penColor);
	    	        break;
	            }
	            break;
            }
            }
            else//kreslit ne stěsnaně
            {
                switch (a_gate.m_pocetvstupu)
                {
	            case 7://8 vývodů
	    	        MyVyvod(a_coll, X - 36, Y + 80, a_gate.m_c1, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y + 60, a_gate.m_c2, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y + 40, a_gate.m_c3, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y + 20, a_gate.m_c4, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y - 20, a_gate.m_c5, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y - 40, a_gate.m_c6, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y - 60, a_gate.m_c7, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y - 80, a_gate.m_c8, li_lineThickness, l_penColor);
	    	        break;
	            case 5://6 vývodů
	    	        MyVyvod(a_coll, X - 36, Y + 60, a_gate.m_c1, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y + 40, a_gate.m_c2, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y + 20, a_gate.m_c3, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y - 20, a_gate.m_c4, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y - 40, a_gate.m_c5, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y - 60, a_gate.m_c6, li_lineThickness, l_penColor);
	    	        break;
	            case 3://4 vývody
	    	        MyVyvod(a_coll, X - 36, Y + 60, a_gate.m_c1, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y + 20, a_gate.m_c2, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y - 20, a_gate.m_c3, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y - 60, a_gate.m_c4, li_lineThickness, l_penColor);
	    	        break;
	            case 1://2 vývody
	    	        MyVyvod(a_coll, X - 36, Y + 20, a_gate.m_c1, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y - 20, a_gate.m_c2, li_lineThickness, l_penColor);
	    	        break;
	            case 6://7 vývody
	    	        MyVyvod(a_coll, X - 36, Y + 60, a_gate.m_c1, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y + 40, a_gate.m_c2, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y + 20, a_gate.m_c3, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y, a_gate.m_c4, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y - 20, a_gate.m_c5, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y - 40, a_gate.m_c6, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y - 60, a_gate.m_c7, li_lineThickness, l_penColor);
	    	        break;
	            case 4://5 vývody
	    	        MyVyvod(a_coll, X - 36, Y + 40, a_gate.m_c1, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y + 20, a_gate.m_c2, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y, a_gate.m_c3, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y - 20, a_gate.m_c4, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y - 40, a_gate.m_c5, li_lineThickness, l_penColor);
	    	        break;
	            case 2://3 vývody
	    	        MyVyvod(a_coll, X - 36, Y + 40, a_gate.m_c1, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y, a_gate.m_c2, li_lineThickness, l_penColor);
	    	        MyVyvod(a_coll, X - 36, Y - 40, a_gate.m_c3, li_lineThickness, l_penColor);
	    	        break;
	            case 0://1 vývod
                    MyVyvod(a_coll, X - 36, Y, a_gate.m_c1, li_lineThickness, l_penColor);
	    	        break;
	            }
            }



	        if (!a_gate.m_ASA)
	        {
                string ls_label = null;
                /*
		        LOGFONT lf;
		        memset(&lf, 0, sizeof(LOGFONT));       // zero out structure
		        lf.lfHeight = -10;                      // request a 10-pixel-height font
		        lf.lfWeight = 400;
		        _tcscpy(lf.lfFaceName, _T(""));        // request a face name "Arial"
		        Point point = Helper.PrevodBodu(new Point(X - 20, Y + 51));
		        CString ls_char;
                */ 

                switch (a_gate.m_tvar)
	           {
                   case GateShapeType.gst_nand:
                   case GateShapeType.gst_and:
                       ls_label = "&";
    		        break;
                   case GateShapeType.gst_nor:
                   case GateShapeType.gst_or:
                    ls_label = ">1";
    		        break;
                   case GateShapeType.gst_inv:
                   case GateShapeType.gst_bud:
                    ls_label = "1";
    		        break;
                   case GateShapeType.gst_exnor:
                   case GateShapeType.gst_exor:
                    ls_label = "=1";
    		        break;
		        }//switch

                Point3D l_center = new Point3D(X - 20, Y + 51, 0);
                EFont l_font = ExportContext.Current.PCadDocument.Parent.m_fonts.m_fontValue;
                int li_height = Exporter.GetFontAscentSize(l_font);
                DxfMText dxfText = new DxfMText(ls_label, l_center, li_height);

                dxfText.Layer = ExportContext.Current.Layer;
                a_coll.Add(dxfText);
	        }
	

        }

        private static void ExportBud(WW.Cad.Model.Entities.DxfEntityCollection a_coll, int ai_x, int ai_y, System.Drawing.Color l_penColor)
        {
            DrawPoly l_pouzdro = new DrawPoly(Shape.poly, 2, l_penColor, new Point[] {
            new Point(ai_x - 36, ai_y + 3),
            new Point(ai_x - 36, ai_y - 6),
            new Point(ai_x + 25, ai_y),
            new Point(ai_x - 36, ai_y + 36),
            });
            Helper.ExportPolylineAux(a_coll, l_pouzdro, true);
        }

        private static void ExportAnd(WW.Cad.Model.Entities.DxfEntityCollection a_coll, int ai_x, int ai_y, int ai_lineThickness, System.Drawing.Color l_penColor)
        {
            DrawPoly l_pouzdro = new DrawPoly(Shape.poly, 2, l_penColor, new PointF[] {
                    new Point(ai_x + 36, ai_y + 36),
                    new Point(ai_x - 36, ai_y + 36),
                    new Point(ai_x - 36, ai_y - 36),
                    new Point(ai_x + 36, ai_y - 36),
                });
            Helper.ExportPolylineAux(a_coll, l_pouzdro, true);
            Helper.My_Bezier(
                                a_coll,
                                new PositionAspect(),
                                new Point(36, 36),
                                new Point(36 + 42, 27),
                                new Point(36 + 42, -27),
                                new Point(36, -36),
                                ai_lineThickness, l_penColor, true);

        }

        private static void ExportOr(WW.Cad.Model.Entities.DxfEntityCollection a_coll, int ai_x, int ai_y, System.Drawing.Color l_penColor)
        {
            DrawPoly l_pouzdro = new DrawPoly(Shape.poly, 2, l_penColor, new PointF[] {
                    new Point(ai_x + 20, ai_y + 36),
                    new Point(ai_x - 36, ai_y + 36)
                });
            Helper.ExportPolylineAux(a_coll, l_pouzdro, true);

            l_pouzdro = new DrawPoly(Shape.poly, 2, l_penColor, new PointF[] {
                    new Point(ai_x + 20, ai_y - 36),
                    new Point(ai_x - 36, ai_y - 36),
                });
            Helper.ExportPolylineAux(a_coll, l_pouzdro, true);

            ExportSpicatyZadek(a_coll, 2, l_penColor);
    

        }

        private static void ExportSpicatyZadek(WW.Cad.Model.Entities.DxfEntityCollection a_coll, int ai_lineThickness, System.Drawing.Color a_penColor)
        {
            PositionAspect l_posAspect = new PositionAspect();
            Point[] body = new Point[4];
            body[3] = new Point(-36, + 36);
            body[2] = new Point(-25, + 25);
            body[1] = new Point(-25, - 25);
            body[0] = new Point(-36, - 36);
            Helper.My_Bezier(a_coll, l_posAspect, body[0], body[1], body[2], body[3], ai_lineThickness, a_penColor, true);
            body[3] = new Point(20, 36);
            body[2] = new Point(51, 28);
            body[1] = new Point(71, 10);
            body[0] = new Point(79, 0);
            Helper.My_Bezier(a_coll, l_posAspect, body[0], body[1], body[2], body[3], ai_lineThickness, a_penColor, true);
            body[3] = new Point(20, - 36);
            body[2] = new Point(51, - 28);
            body[1] = new Point(71, - 10);
            body[0] = new Point(79, 0);
            Helper.My_Bezier(a_coll, l_posAspect, body[0], body[1], body[2], body[3], ai_lineThickness, a_penColor, true);
        }

        private static void ExportExOr(WW.Cad.Model.Entities.DxfEntityCollection a_coll, int ai_x, int ai_y, int ai_lineThickness, System.Drawing.Color a_penColor)
        {
            DrawPoly l_pouzdro = new DrawPoly(Shape.polyline, ai_lineThickness, a_penColor,
            new Point(ai_x + 20, ai_y + 36),
            new Point(ai_x - 25, ai_y + 36)
            );
            Helper.ExportPolylineAux(a_coll, l_pouzdro, true);

            l_pouzdro = new DrawPoly(Shape.polyline, ai_lineThickness, a_penColor,
            new Point(ai_x + 20, ai_y - 36),
            new Point(ai_x - 25, ai_y - 36)
            );
            Helper.ExportPolylineAux(a_coll, l_pouzdro, true);
            PositionAspect l_posAspect = new PositionAspect();

            PointF[] body = new PointF[4];
            body[3] = Helper.PrevodBodu(new PointF(-25, 36), l_posAspect);
            body[2] = Helper.PrevodBodu(new PointF(-15, 25), l_posAspect);
            body[1] = Helper.PrevodBodu(new PointF(-15, - 25), l_posAspect);
            body[0] = Helper.PrevodBodu(new PointF(-25, - 36), l_posAspect);
            Helper.My_Bezier(a_coll, l_posAspect, body[0], body[1], body[2], body[3], ai_lineThickness, a_penColor, true);

            ExportSpicatyZadek(a_coll, ai_lineThickness, a_penColor);
            
        }

        private static void MyVyvod(DxfEntityCollection a_coll, int ai_x, int ai_y, bool ab_inv, int ai_lineThickness, System.Drawing.Color l_penColor)
        {
            if (ab_inv)
            {
                My_Kruh(a_coll, new Point(ai_x - 5, ai_y), 5);
            }
            else
            {
                DrawPoly l_poly = new DrawPoly(Shape.polyline, ai_lineThickness, l_penColor, new Point(ai_x, ai_y), new Point(ai_x - 15, ai_y));
                Helper.ExportPolylineAux(a_coll, l_poly, true);
            }
        }
        /*
        private static void My_Oblouk(DxfEntityCollection a_coll, Point a_center, int ai_radius, Point a_from, Point a_to, int ai_lineThickness, Color l_penColor)
        {
            Point3D l_center = new Point3D(a_center.X, a_center.Y, 0);
            DxfArc l_arc = new DxfArc(l_center, (double)ai_radius, - Math.PI / 2, Math.PI / 2);
            l_arc.Color = l_penColor;
            l_arc.ColorSource = AttributeSource.This;

            a_coll.Add(l_arc);
        }
        */
        private static void My_Kruh(DxfEntityCollection a_coll, PointF a_center, int ai_radius)
        {
            Point3D l_center = new Point3D(a_center.X, a_center.Y, 0);
            DxfCircle l_circle = new DxfCircle(l_center, ai_radius);

            l_circle.Layer = ExportContext.Current.Layer;
            a_coll.Add(l_circle);
        }


    }
}
