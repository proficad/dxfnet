using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DxfNet;
using WW.Math;
using WW.Cad.Model.Entities;


namespace Loader
{
    class HelperRefGrid
    {
        const int GRID_THICKNESS = 50;
        const int CENTERING_MARK_LEN = 100;
        const int FIELD_SIZE_MIN = 200;
        const int FIELD_SIZE_MAX = 2000;
        const int FIELD_SIZE_DEFAULT = 500;

        enum TypeOfEdge { rgEdgeLeft, rgEdgeTop, rgEdgeRight, rgEdgeBottom };


        static public void DrawRefGridForSheets(DxfEntityCollection a_coll, System.Drawing.Rectangle a_rect, RefGridSettings a_settings)
        {
            int li_fieldSize_tenth_mm = 10 * a_settings.FieldSize;
            if ((li_fieldSize_tenth_mm < FIELD_SIZE_MIN) || (li_fieldSize_tenth_mm > FIELD_SIZE_MAX))
            {
                li_fieldSize_tenth_mm = FIELD_SIZE_DEFAULT;
            }

            /*
            CPen penFrame; penFrame.CreatePen(PS_SOLID, 3, RGB(0, 0, 0));
            CPen penThick; penThick.CreatePen(PS_SOLID, 7, RGB(0, 0, 0));

            
            
            CPen * const pOldPen = pDC->SelectObject(&penFrame);
            CGdiObject * const pOldBrush = pDC->SelectStockObject(NULL_BRUSH);

            
            //prepare font
            LOGFONT lf;
            memset(&lf, 0, sizeof(LOGFONT));
            lf.lfHeight = -35;
            lf.lfWeight = 300;
            CString ls_fontName("Arial");
            _tcscpy_s(lf.lfFaceName, LF_FACESIZE, ls_fontName);
            CFont l_font;
            l_font.CreateFontIndirect(&lf);
            CFont* pOldFont = pDC->SelectObject(&l_font);

            int oldmode = pDC->SetBkMode(TRANSPARENT);
            */

            if (a_settings.Left)
            {
                DrawEdge(a_coll, TypeOfEdge.rgEdgeLeft, a_rect.Left, a_rect.Top, a_rect.Bottom, a_settings.Top, a_settings.Bottom, li_fieldSize_tenth_mm);
            }
            if (a_settings.Top)
            {
                DrawEdge(a_coll, TypeOfEdge.rgEdgeTop, a_rect.Top, a_rect.Left, a_rect.Right, a_settings.Left, a_settings.Right, li_fieldSize_tenth_mm);
            }
            if (a_settings.Right)
            {
                DrawEdge(a_coll, TypeOfEdge.rgEdgeRight, a_rect.Right, a_rect.Top, a_rect.Bottom, a_settings.Top, a_settings.Bottom, li_fieldSize_tenth_mm);
            }
            if (a_settings.Bottom)
            {
                DrawEdge(a_coll, TypeOfEdge.rgEdgeBottom, a_rect.Bottom, a_rect.Left, a_rect.Right, a_settings.Left, a_settings.Right, li_fieldSize_tenth_mm);
            }
        }


        private static void DrawEdge(DxfEntityCollection a_coll, TypeOfEdge a_edge, int ai_outer, int ai_start, int ai_end, bool ab_start, bool ab_end, int ai_fieldSize)
        {
            const int LINE_THIN = 3;
            const int LINE_THICK = 7;


            if ((ai_fieldSize < FIELD_SIZE_MIN) || (ai_fieldSize > FIELD_SIZE_MAX))
            {
                ai_fieldSize = FIELD_SIZE_DEFAULT;
            }
            int li_min_field_size = ai_fieldSize / 2;


            int li_inner = 0;
            //int li_centeringMarkEnd = 0;
            int li_posTextX = 0;
            bool lb_turn = false;
            if ((a_edge == TypeOfEdge.rgEdgeTop) || (a_edge == TypeOfEdge.rgEdgeBottom))
            {
                lb_turn = true;
            }

            switch (a_edge)
            {
                case TypeOfEdge.rgEdgeLeft:
                case TypeOfEdge.rgEdgeTop:
                    li_inner = ai_outer + GRID_THICKNESS;
                    li_posTextX = ai_outer + (GRID_THICKNESS / 2);
                    break;
                case TypeOfEdge.rgEdgeRight:
                case TypeOfEdge.rgEdgeBottom:
                    li_inner = ai_outer - GRID_THICKNESS;
                    li_posTextX = ai_outer - (GRID_THICKNESS / 2);
                    break;
            }

            int li_startAltered = ai_start; if (ab_start) { li_startAltered += GRID_THICKNESS; }
            int li_endAltered = ai_end; if (ab_end) { li_endAltered -= GRID_THICKNESS; }



            //draw left edge
            LineVer(a_coll, lb_turn, ai_outer, ai_start, ai_end, LINE_THIN);
            LineVer(a_coll, lb_turn, li_inner, li_startAltered, li_endAltered, LINE_THICK);

            int li_center = (li_startAltered + li_endAltered) / 2;

            //prostredni spricle
            LineHor(a_coll, lb_turn, li_center, ai_outer, li_inner, LINE_THICK);


            int li_remainder = li_center;
            int li_distanceFromCenter = 0;


            //pocet celych poli
            int li_numberOfFieldsHalved = GetNumberOfFieldsHalved(li_center - li_startAltered, ai_fieldSize);


            int li_steps = 0;
            string ls_letter = string.Empty;

            while ((li_remainder - li_startAltered) > (ai_fieldSize + li_min_field_size))
            {
                li_remainder -= ai_fieldSize;
                li_distanceFromCenter += ai_fieldSize;

                LineHor(a_coll, lb_turn, li_center - li_distanceFromCenter, ai_outer, li_inner, LINE_THIN);
                LineHor(a_coll, lb_turn, li_center + li_distanceFromCenter, ai_outer, li_inner, LINE_THIN);

                Translate2Letters(li_numberOfFieldsHalved - li_steps, ref ls_letter, !lb_turn);
                LetterInRefGrid(a_coll, lb_turn, li_posTextX, li_center - li_distanceFromCenter + (ai_fieldSize / 2), ls_letter);
                Translate2Letters(li_numberOfFieldsHalved + li_steps + 1, ref ls_letter, !lb_turn);
                LetterInRefGrid(a_coll, lb_turn, li_posTextX, li_center + li_distanceFromCenter - (ai_fieldSize / 2), ls_letter);
                ++li_steps;
            }

            //last letter up
            int li_posFromEnd = (li_center - li_distanceFromCenter - li_startAltered) / 2;
            int li_lastLetter = li_startAltered + li_posFromEnd;
            Translate2Letters(li_numberOfFieldsHalved - li_steps, ref ls_letter, !lb_turn);
            LetterInRefGrid(a_coll, lb_turn, li_posTextX, li_lastLetter, ls_letter);
            //last letter down
            li_lastLetter = ai_end - li_posFromEnd;
            Translate2Letters(li_numberOfFieldsHalved + li_steps + 1, ref ls_letter, !lb_turn);
            LetterInRefGrid(a_coll, lb_turn, li_posTextX, li_lastLetter, ls_letter);

        }

        private static int GetNumberOfFieldsHalved(int ai_center, int ai_fieldSize)
        {
            int li_min_field_size = ai_fieldSize / 2;

            int li_numberOfFieldsHalved = (ai_center / ai_fieldSize);
            if ((ai_center % ai_fieldSize) > li_min_field_size)
            {
                ++li_numberOfFieldsHalved;
            }

            return li_numberOfFieldsHalved;
        }

        private static void LineHor(DxfEntityCollection a_coll, bool ab_turn, int ai_outer, int ai_start, int ai_end, int ai_thickness)
        {
            Point2D[] l_arrPoints;

            if(ab_turn)
            {
                l_arrPoints = new Point2D[] {
                    new Point2D(ai_outer, Exporter.REVERSE_Y * ai_start),
                    new Point2D(ai_outer, Exporter.REVERSE_Y * ai_end)
                };

            }
            else
            {
                l_arrPoints = new Point2D[] {
                    new Point2D(ai_start, Exporter.REVERSE_Y * ai_outer),
                    new Point2D(ai_end  , Exporter.REVERSE_Y * ai_outer)
                };
            }


            DxfPolyline2D l_line = new DxfPolyline2D(l_arrPoints);
            l_line.DefaultStartWidth = ai_thickness;
            l_line.DefaultEndWidth = ai_thickness;
            a_coll.Add(l_line);
        }


        private static void LineVer(DxfEntityCollection a_coll, bool ab_turn, int ai_outer, int ai_start, int ai_end, int ai_thickness)
        {
            Point2D[] l_arrPoints;

            if (ab_turn)
            {
                l_arrPoints = new Point2D[] {
                    new Point2D(ai_start, Exporter.REVERSE_Y * ai_outer),
                    new Point2D(ai_end,   Exporter.REVERSE_Y * ai_outer)
                };

            }
            else
            {
                l_arrPoints = new Point2D[] {
                    new Point2D(ai_outer, Exporter.REVERSE_Y * ai_start),
                    new Point2D(ai_outer, Exporter.REVERSE_Y * ai_end)
                };
            }


            DxfPolyline2D l_line = new DxfPolyline2D(l_arrPoints);
            l_line.DefaultStartWidth = ai_thickness;
            l_line.DefaultEndWidth = ai_thickness;
            a_coll.Add(l_line);
        }


        private static void Translate2Letters(int ai_input, ref string as_letters, bool ab_translate)
        {
            if (!ab_translate)
            {
                as_letters = Int2String(ai_input);
                return;
            }

            as_letters = string.Empty;

            --ai_input;

            const int l_start = 65;
            const int li_size = 26 - 2;

            int l_prvni = ai_input % li_size;
            int l_druha = ai_input / li_size;

            /*
            if((i % li_size) == 0)
            {
                printf("\n\n");
            }
            */

            if (l_druha > 0)
            {
                char fst = Convert.ToChar(GetCharSkipped(l_druha - 1) + l_start);
                char snd = Convert.ToChar(GetCharSkipped(l_prvni) + l_start);
                as_letters += fst;
                as_letters += snd; 
                return;
            }
            as_letters += Convert.ToChar(GetCharSkipped(l_prvni) + l_start);

        }

        private static string Int2String(int ai_input)
        {
            return ai_input.ToString();
        }

        private static int GetCharSkipped(int ai_input)
        {
            //skip I and O
            const int li_I = 8;
            const int li_O = li_I + 6;

            if (ai_input >= li_I)
            {
                ai_input++;
            }
            if (ai_input >= li_O)
            {
                ai_input++;
            }

            return ai_input;
        }

        private static void LetterInRefGrid(DxfEntityCollection a_coll, bool ab_turn, int ai_x, int ai_y, string as_what)
        {
	        if (ab_turn)
	        {
                //swap
                int li_temp = ai_x;
                ai_x = ai_y;
                ai_y = li_temp;
	        }

            ai_y *= Exporter.REVERSE_Y;

            Point3D l_point = new Point3D(ai_x, ai_y, 0);
            double ld_font_size = 20;
            DxfText l_dxf_text = new DxfText(as_what, l_point, ld_font_size);
            l_dxf_text.AlignmentPoint2 = l_point;

            a_coll.Add(l_dxf_text);
        }


        //---------------------------------------
    }
}
