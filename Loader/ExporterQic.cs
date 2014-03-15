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
    internal static class ExporterQic
    {


        private static void ExportQicText(DxfEntityCollection a_dxfEntityCollection, string as_text, Point a_point, QTextAlignment a_align, bool ab_vertical, bool ab_topOrBottomEdge)
        {
            EFont l_font = ExportContext.Current.PCadDocument.Parent.m_fonts.m_fontValue;
            int li_height = Exporter.GetFontAscentSize(l_font);
            Point3D l_pivot = new Point3D(a_point.X, -a_point.Y, 0);
            Vector3D l_xAxis = new Vector3D(1, 0, 0);
            //l_pivot.Y += 100;

            AttachmentPoint l_attachmentPoint;
            if (ab_vertical)
            {
                l_xAxis = new Vector3D(0, 1, 0);
                if (a_align == QTextAlignment.AL_LM)
                {
                    l_attachmentPoint = AttachmentPoint.BottomCenter; l_attachmentPoint = AttachmentPoint.MiddleLeft;
                }
                else
                {
                    l_attachmentPoint = AttachmentPoint.TopCenter; l_attachmentPoint = AttachmentPoint.MiddleRight;
                }
            }
            else
            {
                if (a_align == QTextAlignment.AL_LM)
                {
                    l_attachmentPoint = AttachmentPoint.MiddleLeft;
                }
                else
                {
                    l_attachmentPoint = AttachmentPoint.MiddleRight;
                }
            }

            int li_correction = 5;
            if (ab_topOrBottomEdge)
            {
                li_correction = 5;
            }

            if (a_align == QTextAlignment.AL_LM)
            {
                l_pivot.Y += li_correction;
            }
            else
            {
                l_pivot.Y -= li_correction;
            }

            DxfMText dxfText = new DxfMText(as_text, l_pivot, li_height);
            
            dxfText.AttachmentPoint = l_attachmentPoint;
            dxfText.XAxis = l_xAxis;
            a_dxfEntityCollection.Add(dxfText);
        }

  

        public static void ExportQic(DxfEntityCollection a_coll, QIC a_ic, PCadDoc a_pCadDoc, HybridDictionary a_dictRepo)
        {
            DxfNet.PositionAspect l_posAspect = new PositionAspect(a_ic.m_position.Location, a_ic.m_turns, a_ic.m_hor, a_ic.m_ver);

            string ret;
            System.Drawing.Color l_textColor = System.Drawing.Color.Black;
            System.Drawing.Color l_penColor = a_ic.m_objProps.m_logpen.m_color;

            int li_stred_x, li_stred_y;
            li_stred_x = l_posAspect.m_pivot.X;//(m_position.left + m_position.right)/2;
            li_stred_y = l_posAspect.m_pivot.Y;//(m_position.top  + m_position.bottom)/2;



            int li_polomer_x, li_polomer_y;
            if (a_ic.m_numberOfOutletsHor > 5)
            {
                li_polomer_x = (a_ic.m_numberOfOutletsHor * (QIC.INT_VYV / 2)) + (QIC.INT_VYV / 2);
            }
            else
            {
                li_polomer_x = (3 * QIC.INT_VYV);
            }
            li_polomer_y = (a_ic.m_numberOfOutletsVer - 1) * (QIC.INT_VYV / 2) + 2 * QIC.INT_VYV;



            int i;

            // obdélník
            int li_left, li_top, li_bottom, li_right;

            li_left = li_stred_x - li_polomer_x;
            li_right = li_stred_x + li_polomer_x;
            li_bottom = li_stred_y + li_polomer_y;
            li_top = li_stred_y - li_polomer_y;


            string[] l_arrOut_d = a_ic.m_out_d.Split(new Char[] { ',' });
            string[] l_arrOut_n = a_ic.m_out_n.Split(new Char[] { ',' });

            //vodorovný font

            //	===============================NAPISY UVNITR ===============================
            // labely - označení vývodů - nalevo(shora dolů)
            for (i = 1; i <= a_ic.m_numberOfOutletsVer; i++)
            {
                ret = GetLabel(l_arrOut_d, i);
                QText(a_coll, l_posAspect, ret, new Point(li_left + 8, li_top + ((i + 1) * QIC.INT_VYV)), QTextAlignment.AL_LM, l_textColor, false);
            }
            // labely - označení vývodů - dole(zleva doprava)
            for (i = 1; i <= a_ic.m_numberOfOutletsHor; i++)
            {
                ret = GetLabel(l_arrOut_d, i + a_ic.m_numberOfOutletsVer);
                QText(a_coll, l_posAspect, ret, new Point(li_left + (QIC.INT_VYV * i), li_bottom - 8), QTextAlignment.AL_LM, l_textColor, true);
            }
            // labely - označení vývodů - vpravo(sdola nahoru)
            for (i = a_ic.m_numberOfOutletsVer; i > 0; i--)
            {
                ret = GetLabel(l_arrOut_d, i + a_ic.m_numberOfOutletsHor + a_ic.m_numberOfOutletsVer);
                QText(a_coll, l_posAspect, ret, new Point(li_right - 8, li_bottom - ((i + 1) * QIC.INT_VYV)), QTextAlignment.AL_RM, l_textColor, false);
            }
            // labely - označení vývodů - nahoře(zprava doleva)
            for (i = a_ic.m_numberOfOutletsHor; i > 0; i--)
            {
                ret = GetLabel(l_arrOut_d, i + a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor + a_ic.m_numberOfOutletsVer);
                QText(a_coll, l_posAspect, ret, new Point(li_left + QIC.INT_VYV + (QIC.INT_VYV * (a_ic.m_numberOfOutletsHor - i)), li_top + 8), QTextAlignment.AL_RM, l_textColor, true);
            }

            //  ==========================================================
            // labely - čísla vývodů - nalevo(shora dolů)  - NÁPISY VENKU =====vvv======
            for (i = 1; i <= a_ic.m_numberOfOutletsVer; i++)
            {
                ret = GetLabel(l_arrOut_n, i);
                QText(a_coll, l_posAspect, ret, new Point(li_left - 8, li_top + ((i + 1) * QIC.INT_VYV) + QIC.INT_VYV / 2), QTextAlignment.AL_RM, l_textColor, false);
                //SpojRect(&obrysy, &l_outRect);
            }
            // labely - čísla vývodů - dole(zleva doprava)
            for (i = 1; i <= a_ic.m_numberOfOutletsHor; i++)
            {
                ret = GetLabel(l_arrOut_n, i + a_ic.m_numberOfOutletsVer);
                QText(a_coll, l_posAspect, ret, new Point(li_left + (QIC.INT_VYV * i) - (QIC.INT_VYV / 2), li_bottom + 8), QTextAlignment.AL_RM, l_textColor, true);
                //SpojRect(&obrysy, &l_outRect);
            }
            // labely - čísla vývodů - vpravo(sdola nahoru)
            for (i = a_ic.m_numberOfOutletsVer; i > 0; i--)
            {
                ret = GetLabel(l_arrOut_n, i + a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor);
                QText(a_coll, l_posAspect, ret, new Point(li_right + 8, li_bottom - ((i + 1) * QIC.INT_VYV) + QIC.INT_VYV / 2), QTextAlignment.AL_LM, l_textColor, false);
                //SpojRect(&obrysy, &l_outRect);
            }
            // labely - čísla vývodů - nahoře(zprava doleva)
            for (i = a_ic.m_numberOfOutletsHor; i > 0; i--)
            {
                ret = GetLabel(l_arrOut_n, i + a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor + a_ic.m_numberOfOutletsVer);
                QText(a_coll, l_posAspect, ret, new Point(li_left + QIC.INT_VYV / 2 + (QIC.INT_VYV * (a_ic.m_numberOfOutletsHor - i)), li_top - 8), QTextAlignment.AL_LM, l_textColor, true);
                //SpojRect(&obrysy, &l_outRect);
            }


            //================================================KULIČKY==vvv========================================
            string[] l_arrOutNInv = a_ic.m_out_n_inv.Split(new Char[] { ',' });

            foreach (string ls_item in l_arrOutNInv)
            {
                if (ls_item.Length == 0)
                {
                    continue;
                }
                int li_cislo = int.Parse(ls_item);

                //udělej kroužek
                if ((li_cislo > 0) && (li_cislo <= a_ic.m_numberOfOutletsVer))
                {
                    My_Kruh(a_coll, l_posAspect, new Point(li_left - 5, li_top + ((li_cislo + 1) * QIC.INT_VYV)), new Size(5, 5));
                }
                if ((li_cislo > a_ic.m_numberOfOutletsVer) && (li_cislo <= (a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor)))
                {
                    li_cislo -= a_ic.m_numberOfOutletsVer;
                    My_Kruh(a_coll, l_posAspect, new Point(li_left + ((li_cislo) * QIC.INT_VYV), li_bottom + 5), new Size(5, 5));
                }
                if ((li_cislo > a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor) && (li_cislo <= (a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor + a_ic.m_numberOfOutletsVer)))
                {
                    li_cislo -= (a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor);
                    My_Kruh(a_coll, l_posAspect, new Point(li_right + 5, li_bottom - ((li_cislo + 1) * QIC.INT_VYV)), new Size(5, 5));
                }
                if ((li_cislo > a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor + a_ic.m_numberOfOutletsVer) && (li_cislo <= (a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor + a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor)))
                {
                    li_cislo -= (a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor + a_ic.m_numberOfOutletsVer);
                    My_Kruh(a_coll, l_posAspect, new Point(li_left + (QIC.INT_VYV * (a_ic.m_numberOfOutletsHor - li_cislo + 1)), li_top - 5), new Size(5, 5));
                }
            }



            // svislé oddělovače
            if (a_ic.m_ver_left)
            {
                Point l_from = Helper.PrevodBodu(new Point(li_left + 44, li_top), l_posAspect);
                Point l_to = Helper.PrevodBodu(new Point(li_left + 44, li_bottom), l_posAspect);
                DrawPoly l_poly = new DrawPoly(Shape.polyline, 2, l_penColor, l_from, l_to);
                Helper.ExportPolylineAux(a_coll, l_poly, true);
            }
            if (a_ic.m_ver_right)
            {
                Point l_from = Helper.PrevodBodu(new Point(li_right - 44, li_top), l_posAspect);
                Point l_to = Helper.PrevodBodu(new Point(li_right - 44, li_bottom), l_posAspect);
                DrawPoly l_poly = new DrawPoly(Shape.polyline, 2, l_penColor, l_from, l_to);
                Helper.ExportPolylineAux(a_coll, l_poly, true);
            }
            //vykrojení
            if (a_ic.m_mark)
            {
                int stred = (li_left + li_right) / 2;
                Helper.My_Bezier(a_coll, l_posAspect, new Point(stred - 11, li_top), new Point(stred - 10, li_top + 12)
                    , new Point(stred + 10, li_top + 12), new Point(stred + 11, li_top), 2, l_penColor, true);
            }
            // ========================   vodorovné oddělovače   ================================
            if (a_ic.m_pos_hor != null)
            {
                string[] l_arrPos_hor = a_ic.m_pos_hor.Split(new Char[] { ',' });
                foreach (string ls_item in l_arrPos_hor)
                {
                    if (ls_item.Length == 0)
                    {
                        continue;
                    }
                    int li_cislo;
                    if (!int.TryParse(ls_item, out li_cislo))
                    {
                        continue;
                    }
                    //udělej čárku
                    if ((li_cislo > 0) && (li_cislo <= (a_ic.m_numberOfOutletsVer - 1)))
                    {
                        Point l_from = Helper.PrevodBodu(new Point(li_left, li_top + ((li_cislo + 1) * QIC.INT_VYV) + QIC.INT_VYV / 2), l_posAspect);
                        Point l_to = Helper.PrevodBodu(new Point(li_left + 44, li_top + ((li_cislo + 1) * QIC.INT_VYV) + QIC.INT_VYV / 2), l_posAspect);
                        DrawPoly l_poly = new DrawPoly(Shape.polyline, 2, l_penColor, l_from, l_to);
                        Helper.ExportPolylineAux(a_coll, l_poly, true);
                    }
                    if ((li_cislo > a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor + 1) && (li_cislo <= (a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor + a_ic.m_numberOfOutletsVer)))
                    {
                        li_cislo -= (a_ic.m_numberOfOutletsVer + a_ic.m_numberOfOutletsHor);
                        Point l_from = Helper.PrevodBodu(new Point(li_right - 44, li_top + ((2 + a_ic.m_numberOfOutletsVer - li_cislo) * QIC.INT_VYV) + QIC.INT_VYV / 2), l_posAspect);
                        Point l_to = Helper.PrevodBodu(new Point(li_right, li_top + ((2 + a_ic.m_numberOfOutletsVer - li_cislo) * QIC.INT_VYV) + QIC.INT_VYV / 2), l_posAspect);
                        DrawPoly l_poly = new DrawPoly(Shape.polyline, 2, l_penColor, l_from, l_to);
                        Helper.ExportPolylineAux(a_coll, l_poly, true);
                    }
                }
            }

            // hlavní obdélník až nakonec
            Point l_leftTop = Helper.PrevodBodu(new Point(li_left, li_top), l_posAspect);
            Point l_leftBottom = Helper.PrevodBodu(new Point(li_left, li_bottom), l_posAspect);
            Point l_rightBottom = Helper.PrevodBodu(new Point(li_right, li_bottom), l_posAspect);
            Point l_rightTop = Helper.PrevodBodu(new Point(li_right, li_top), l_posAspect);

            DrawPoly l_body = new DrawPoly(Shape.polyline, 2, l_penColor, new Point[] { l_leftTop, l_leftBottom, l_rightBottom, l_rightTop, l_leftTop });
            Helper.ExportPolylineAux(a_coll, l_body, true);


            foreach (Insert.Satelite l_sat in a_ic.m_satelites)
            {
                if (l_sat.m_visible)
                {
                    Exporter.ExportSatelite(a_coll, l_sat, a_pCadDoc);
                }
            }
        }


  



        private static bool My_Kruh(DxfEntityCollection a_coll, PositionAspect a_aspect, Point a_stred, Size a_polomer)
        {
            Point s = Helper.PrevodBodu(a_stred, a_aspect);

            // update the size
            //if ((abs(polomer.cx)*2) > (m_size.cx)) m_size.cx = (abs(polomer.cx)*2);
            //if ((abs(polomer.cy)*2) > (m_size.cy)) m_size.cy = (abs(polomer.cy)*2);

            //99 return pDC->Ellipse(s.x - polomer.cx, s.y - polomer.cy, s.x + polomer.cx, s.y + polomer.cy);
            return true;
        }

        private static void QText(DxfEntityCollection a_coll, PositionAspect a_aspect, string as_ret, Point a_point, QTextAlignment a_textAlignment, System.Drawing.Color barva, bool ab_topOrBottomEdge)
        {

            if (as_ret.Length == 0)
            {
                return;
            }

            if ((a_textAlignment != QTextAlignment.AL_LM) && (a_textAlignment != QTextAlignment.AL_RM))
            {
                throw new Exception("wrong alignment " + a_textAlignment.ToString());
            }

            a_point = Helper.PrevodBodu(a_point, a_aspect);
            int li_fontotacek = 0;



            //switch the alignment if the elem is inverted
            if ((a_aspect.m_horizontal) && (ab_topOrBottomEdge))
            {
                if (a_textAlignment == QTextAlignment.AL_LM)
                    a_textAlignment = QTextAlignment.AL_RM;
                else
                    a_textAlignment = QTextAlignment.AL_LM;
            }

            if ((a_aspect.m_vertical) && (!ab_topOrBottomEdge))
            {
                if (a_textAlignment == QTextAlignment.AL_LM)
                    a_textAlignment = QTextAlignment.AL_RM;
                else
                    a_textAlignment = QTextAlignment.AL_LM;
            }

            if ((a_aspect.m_otacek == 4) || (a_aspect.m_otacek == 6))
            {
                if (a_textAlignment == QTextAlignment.AL_LM)
                    a_textAlignment = QTextAlignment.AL_RM;
                else
                    a_textAlignment = QTextAlignment.AL_LM;
            }



            int li_out;
            bool lb_isNumeric = int.TryParse(as_ret, out li_out);
            bool lb_otacet = ((as_ret.Length > 2) || (!lb_isNumeric));//not a number or longer > 2

            switch (a_aspect.m_otacek)
            {

                case 2: 	// 90 degrees left
                case 6:		// 90 degrees right
                    if (!ab_topOrBottomEdge)
                    {
                        if (lb_otacet)
                            li_fontotacek = 2;
                        else
                            li_fontotacek = 0;
                    }
                    else
                    {
                        li_fontotacek = 0;
                    }
                    ExportQicText(a_coll, as_ret, a_point, a_textAlignment, (li_fontotacek == 2), !ab_topOrBottomEdge);
                    break;

                default:							//	0	0000000000000000000000000

                    if (ab_topOrBottomEdge)
                    {
                        if (lb_otacet)
                            li_fontotacek = 2;
                        else
                            li_fontotacek = 0;
                    }
                    else
                    {
                        li_fontotacek = 0;
                    }
                    ExportQicText(a_coll, as_ret, a_point, a_textAlignment, (li_fontotacek == 2), ab_topOrBottomEdge);
                    break;

            }

        }

        private static string GetLabel(string[] as_arr, int ai_index)
        {
            //here it is 0 based
            ai_index--;
            if (ai_index < as_arr.Length)
            {
                return as_arr[ai_index];
            }
            return string.Empty;
        }


    //--------------------------------
    }
}
