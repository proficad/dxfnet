using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Drawing;

using WW.Math;
using WW.Cad.Base;
using WW.Cad.IO;
using WW.Cad.Model;
using WW.Cad.Model.Objects;
using WW.Cad.Model.Entities;
using WW.Cad.Model.Tables;

using DxfNet;
using System.Globalization;
using System.IO;
using System.Collections;
using Microsoft.Win32;
using WW.Cad.Drawing;

namespace Loader
{



    public static class Exporter
    {
        public static readonly int REVERSE_Y = -1;
        static int m_trafoName = 1;


        internal static void Export(PCadDoc a_doc, string as_pathDxf)
        {
            DxfModel model = new DxfModel(DxfVersion.Dxf21);//2012-12-02 vracime se k dxf21

            Setup_DimStyle(model, a_doc.m_dim_style);

            //add layers
            foreach (Layer l_layer in a_doc.m_layers)
            { 
                string ls_name = l_layer.Name;
                if (!model.Layers.Contains(ls_name))
                {
                    model.Layers.Add(new DxfLayer(ls_name));
                }
            }


            ExportContext.Setup(model, a_doc);
            //          model.Entities.Add(new DxfText("Hatch bounded by multiple boundary paths (R14)", new Point3D(0, 1.5d, 0d), 0.1d));

            HybridDictionary l_dictRepo = new HybridDictionary();
            foreach (PpdDoc ppdDoc in a_doc.Parent.m_repo.m_listPpd)
            {
                ExportBlockPpd(model.Blocks, ppdDoc, l_dictRepo);
            }
            foreach (QImageDesc l_imgDesc in a_doc.Parent.m_repo.m_listImgDesc)
            {
                ExportQImageDesc(model.Images, l_imgDesc, l_dictRepo);
            }
            foreach (PtbDoc l_ptb in a_doc.Parent.m_repo.m_listPtb)
            {
                ExportTitleBlocks(model.Blocks, l_ptb, l_dictRepo);
            }

            ExportDrawDoc(model.Entities, a_doc, l_dictRepo, false);



            // title block
            if (ExportContext.Current.PCadDocument.Parent.Version > 7)
            {
                if (ExportContext.Current.PCadDocument.m_ptbPosition.m_useTb)//page level switch
                {
                    if(!string.IsNullOrWhiteSpace(ExportContext.Current.PCadDocument.m_ptbPosition.Path))//page level path
                    {
                        ExportTbInsert(model.Entities, ExportContext.Current.PCadDocument.m_ptbPosition, a_doc.Parent.m_ref_grid, l_dictRepo);//page TB
                    }
                    else
                    {
                        if(ExportContext.Current.PCadDocument.Parent.m_ptbPosition.m_useTb)
                        {
                            if(!string.IsNullOrWhiteSpace(ExportContext.Current.PCadDocument.Parent.m_ptbPosition.Path))
                            {
                                ExportTbInsert(model.Entities, ExportContext.Current.PCadDocument.Parent.m_ptbPosition, a_doc.Parent.m_ref_grid, l_dictRepo);//global TB
                            }
                        }
                    }
                }
            }
            else
            {   // old version had only global TB
                ExportTb(model.Entities, ExportContext.Current.PCadDocument.Parent.m_ptbPosition);//global TB
            }



            if (ExportContext.Current.PCadDocument.Parent.m_settingsPage.DrawFrame)
            {
                ExportFrame(model.Entities);
            }


            HelperRefGrid.DrawRefGridForSheets(model.Entities, a_doc.GetRect(), a_doc.Parent.m_ref_grid);

            //fix 2010-03-17
            DxfVPort vport = DxfVPort.CreateActiveVPort();
            vport.Height = 3000;
            model.VPorts.Add(vport);

            AdjustScale(a_doc, model);

            // obsolete 2010-05-11            DxfWriter.WriteDxf(as_pathDxf, model, false);
            DxfWriter.Write(as_pathDxf, model, false);
            System.Diagnostics.Trace.WriteLine("done");
        }

        private static void AdjustScale(PCadDoc a_doc, DxfModel model)
        {
            double li_scale_factor = 1;

            //SCALE to adjust the coordinates to the scale (Lehmann)
            if (a_doc.Scale != 0)
            {
                li_scale_factor = a_doc.Scale / 10d;
                Matrix4D l_tr = Transformation4D.Scaling(li_scale_factor);
                WW.Cad.Drawing.TransformConfig l_config = new WW.Cad.Drawing.TransformConfig();
                foreach (DxfEntity l_entity in model.Entities)
                {
                    l_entity.TransformMe(l_config, l_tr);
                }


                foreach (DxfLineType l_line_type in ExportContext.Current.Model.LineTypes)
                {
                    foreach (var l_elem in l_line_type.Elements)
                    {
                        l_elem.Length *= li_scale_factor;
                    }
                }


                model.CurrentDimensionStyle.ScaleFactor = li_scale_factor;

                double ld_linear_scale = 10d;//no scale
                switch (a_doc.m_dim_style.m_unit)
                {
                    case Core.QDimStyle.Unit.unit_mm:
                        ld_linear_scale = 1d;
                        break;
                    case Core.QDimStyle.Unit.unit_cm:
                        ld_linear_scale = 0.1d;
                        break;
                    case Core.QDimStyle.Unit.unit_m:
                        ld_linear_scale = 0.001d;
                        break;
                    case Core.QDimStyle.Unit.unit_km:
                        ld_linear_scale = 0.000001d;
                        break;
                }

                model.CurrentDimensionStyle.LinearScaleFactor = ld_linear_scale;
            }
        }

        private static void ExportQImageDesc(DxfImageDefCollection a_coll, QImageDesc a_imgDesc, HybridDictionary a_dict)
        {

            if (string.IsNullOrEmpty(a_imgDesc.ImgEncoded))
            {
                return;
            }
             
            if (a_imgDesc.ImgType == 10) // do not support WMF
            {
                return;
            }

            DxfImageDef l_imgDef = new DxfImageDef(ExportContext.Current.Model);

            byte[] l_rawImg;
            try
            {
                l_rawImg = Convert.FromBase64String(a_imgDesc.ImgEncoded);
            }
            catch (System.Exception )
            {
            	return;
            }
            

            Random l_rnd = new Random();
            string ls_ext = GetImageExtension(a_imgDesc.ImgType);
            
            string ls_path = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + "." + ls_ext;
            FileStream l_tempFile = File.Create(ls_path);
            BinaryWriter l_writer = new BinaryWriter(l_tempFile);
            l_writer.Write(l_rawImg);
            l_writer.Close();

            Image l_img = Image.FromFile(ls_path);

            l_imgDef.Filename = ls_path;
            l_imgDef.Size = new Size2D(l_img.Width, l_img.Height);
            l_imgDef.DefaultPixelSize = new Size2D(1d, 1d);
            l_imgDef.ImageIsLoaded = true;
            
            //model.Images.Add(imageDef);

            a_coll.Add(l_imgDef);
            a_dict[a_imgDesc.LastGuid] = l_imgDef;

        }

        private static string GetImageExtension(int a_imgType)
        {
            switch (a_imgType)
            {
                case 1: return "bmp";
                case 3: return "jpg";
                case 4: return "png";
                case 2: return "gif";
                case 10: return "wmf";
                default: return "bmp";
                    
            }
        }

        private static void ExportFrame(DxfEntityCollection a_coll)
        {
            Size l_size = ExportContext.Current.PCadDocument.GetSize();
            
            Rectangle l_rect = new Rectangle(0, 0, l_size.Width, l_size.Height);
            DrawRect l_frame = new DrawRect(Shape.rectangle, l_rect);
            l_frame.m_objProps.m_bBrush = false;
            l_frame.m_objProps.m_logpen.m_width = 5;

            ExportRect(a_coll, l_frame, false);

        }

        private static void ExportTb(DxfEntityCollection a_coll, PtbPosition a_ptb_pos)
        {
            DxfBlock l_block = new DxfBlock("title_block");
            ExportContext.Current.BlockCollection.Add(l_block);
           
            bool lb_turn = a_ptb_pos.m_turn;

 

            HybridDictionary l_dictRepo = new HybridDictionary();
            foreach (QImageDesc l_imgDesc in a_ptb_pos.m_pPtb.m_repo.m_listImgDesc)
            {
                ExportQImageDesc(ExportContext.Current.Model.Images, l_imgDesc, l_dictRepo);
            }
            ExportDrawDoc(l_block.Entities, a_ptb_pos.m_pPtb, l_dictRepo, false);





            //Size l_size = ExportContext.Current.PCadDocument.Parent.GetSize();
            Size l_size = ExportContext.Current.PCadDocument.GetSize();



            //calc center point of the TB
            Rectangle l_rectUsed = a_ptb_pos.m_pPtb.GetUsedRect();

            int li_tbBorderRight = l_rectUsed.Right;
            int li_tbBorderBottom = l_rectUsed.Bottom;

            Point3D l_centerPoint = new Point3D();
            int li_turns = lb_turn ? 2 : 0;
            if (li_turns == 2)
            {
                li_tbBorderRight = l_rectUsed.Bottom;
                li_tbBorderBottom = -l_rectUsed.Left;

                l_centerPoint.X = l_size.Width - li_tbBorderRight;
                l_centerPoint.Y = l_rectUsed.Right;
            }
            else
            {
                l_centerPoint.X = l_size.Width - li_tbBorderRight;
                l_centerPoint.Y = l_size.Height - li_tbBorderBottom;
            }


            l_centerPoint.X -= a_ptb_pos.m_horDist;
            l_centerPoint.Y -= a_ptb_pos.m_verDist;


            l_centerPoint.Y *= REVERSE_Y;

            DxfInsert l_insert = new DxfInsert(l_block, l_centerPoint);

            l_insert.Rotation = (Math.PI * li_turns) / 4;
            l_insert.Layer = ExportContext.Current.Layer;
            a_coll.Add(l_insert);
        }

        
        private static void ExportTbInsert(DxfEntityCollection a_coll, PtbPosition a_ptb_pos, RefGridSettings a_ref_grid_settings, HybridDictionary a_dict)
        {
            string ls_name = a_ptb_pos.Path;
            if(string.IsNullOrWhiteSpace(ls_name))
            {
                return;
            }
            ls_name = Sanitize(ls_name);



            DxfBlock l_block = (DxfBlock)a_dict[ls_name];

            bool lb_turn = a_ptb_pos.m_turn;
            

            //Size l_size = ExportContext.Current.PCadDocument.Parent.GetSize();
            Size l_size = ExportContext.Current.PCadDocument.GetSize();


            //calc center point of the TB
            PtbDoc l_ptb_doc = ExportContext.Current.PCadDocument.GetRepo().GetPtb(ls_name);

            if(l_ptb_doc == null)
            {
                return;
            }

            Rectangle l_rectUsed = l_ptb_doc.GetUsedRect();


            int li_tbBorderRight = l_rectUsed.Right;
            int li_tbBorderBottom = l_rectUsed.Bottom;

            Point3D l_centerPoint = new Point3D();
            int li_turns = lb_turn ? 2 : 0;
            if (li_turns == 2)
            {
                li_tbBorderRight = l_rectUsed.Bottom;
                li_tbBorderBottom = -l_rectUsed.Left;

                l_centerPoint.X = l_size.Width - li_tbBorderRight;
                l_centerPoint.Y = l_rectUsed.Right;
            }
            else
            {
                l_centerPoint.X = l_size.Width - li_tbBorderRight;
                l_centerPoint.Y = l_size.Height - li_tbBorderBottom;
            }


            l_centerPoint.X -= a_ptb_pos.m_horDist;
            l_centerPoint.Y -= a_ptb_pos.m_verDist;


            // if ref grid, shift the TB
            if (a_ref_grid_settings.Right)
            {
                l_centerPoint.X -= RefGridSettings.GRID_THICKNESS;
            }
            if (li_turns == 2)
            {
                if (a_ref_grid_settings.Top)
                {
                    l_centerPoint.Y += RefGridSettings.GRID_THICKNESS;
                }
            }
            else
            {
                if(a_ref_grid_settings.Bottom)
                {
                    l_centerPoint.Y -= RefGridSettings.GRID_THICKNESS;
                }
            }



            l_centerPoint.Y *= REVERSE_Y;

            DxfInsert l_insert = new DxfInsert(l_block, l_centerPoint);

            l_insert.Rotation = (Math.PI * li_turns) / 4;
            l_insert.Layer = ExportContext.Current.Layer;
            a_coll.Add(l_insert);
            
        }
        


        private static void ExportDrawDoc(DxfEntityCollection a_coll, DrawDoc a_doc, HybridDictionary a_dict, bool ab_block)
        {
            foreach (DrawObj obj in a_doc.m_objects)
            {
                if(obj.m_layer != null)
                {
                    ExportContext.Current.Layer = ExportContext.Current.Model.Layers[obj.m_layer.Name];
                }

                if (obj is QImage)
                {
                    if (a_dict != null)
                    {
                        QImage l_image = obj as QImage;
                        DxfImageDef l_imgDesc = (DxfImageDef)a_dict[l_image.LastGuid];
                        if (l_imgDesc == null)
                        {
                            //throw new Exception("l_block is null");
                        }
                        else
                        {
                            ExportQImage(a_coll, obj as QImage, a_doc as PCadDoc, l_imgDesc);
                        }
                        
                    }
                   
                    continue;
                }

                if (obj is Trafo)
                {
                    ExportTrafo(a_coll, obj as Trafo, a_doc as PCadDoc, a_dict);
                    continue;
                }
                if (obj is QIC)
                {
                    ExporterQic.ExportQic(a_coll, obj as QIC, a_doc as PCadDoc, a_dict);
                    continue;
                }
                if (obj is QGate)
                {
                    ExporterGate.ExportQGate(a_coll, obj as QGate, a_doc as PCadDoc);
                    continue;
                }
                if (obj is Insert)
                {
                    Insert l_insert = obj as Insert;
                    DxfBlock l_block = (DxfBlock)a_dict[l_insert.m_lG];
                    if (l_block == null)
                    {
                        throw new Exception("l_block is null");
                    }
                    ExportInsert(a_coll, obj as Insert, a_doc as PCadDoc, l_block, a_dict);
                    continue;
                }

                switch (obj.m_nShape)
                {
                    case Shape.poly:            ExportPolygon   (a_coll, obj as DrawPoly, ab_block); break;
                    case Shape.polyline:        ExportPolyline  (a_coll, obj as DrawPoly, ab_block); break;
                    case Shape.spoj:            ExportWire      (a_coll, obj as DxfNet.Wire, a_doc as PCadDoc); break;
                    case Shape.bezier:          ExportBezier    (a_coll, obj as DrawPoly, ab_block); break;
                    case Shape.ellipse:         ExportEllipse   (a_coll, obj as DrawRect, ab_block); break;
                    case Shape.circle:          ExportCircle    (a_coll, obj as QCircle, ab_block); break;
                    case Shape.pie:
                    case Shape.chord:           ExportPieChord  (a_coll, obj as DrawRect, ab_block); break;
                    case Shape.arc:             ExportArc       (a_coll, obj as DrawRect, ab_block); break;
                    case Shape.rectangle:       ExportRect      (a_coll, obj as DrawRect, ab_block); break;
                    case Shape.roundRectangle:  ExportRoundRect (a_coll, obj as DrawRect, ab_block); break;
                    case Shape.text:            ExportFreeText  (a_coll, obj as FreeText); break;
                    case Shape.cable:           ExportCable     (a_coll, obj as CableSymbol, a_doc as PCadDoc); break;
                    case Shape.dim_line:        ExportDimLine   (a_coll, obj as QDimLine, ab_block); break;
                    case Shape.dim_circle:      ExportDimCircle (a_coll, obj as QDimCircle, ab_block); break;
                }
            }
        }

        private static void ExportCable(DxfEntityCollection a_coll, CableSymbol a_cable_symbol, PCadDoc a_pCadDoc)
        {
            const int MARGIN_POSITION = 20;
            Point2D[] l_arrPoints = new Point2D[2];

            if(a_cable_symbol.Hor)
            {
                l_arrPoints[0].X = a_cable_symbol.Min + MARGIN_POSITION;
                l_arrPoints[1].X = a_cable_symbol.Max - MARGIN_POSITION;

                l_arrPoints[0].Y = a_cable_symbol.Common;
                l_arrPoints[1].Y = a_cable_symbol.Common;
            }
            else
            {
                l_arrPoints[0].Y = a_cable_symbol.Min + MARGIN_POSITION;
                l_arrPoints[1].Y = a_cable_symbol.Max - MARGIN_POSITION;

                l_arrPoints[0].X = a_cable_symbol.Common;
                l_arrPoints[1].X = a_cable_symbol.Common;
            }

            l_arrPoints[0].Y *= REVERSE_Y;
            l_arrPoints[1].Y *= REVERSE_Y;

            DxfPolyline2D l_dxfPolyline = new DxfPolyline2D(l_arrPoints);

            const int CABLE_SYMBOL_THICKNESS = 3;
            l_dxfPolyline.DefaultStartWidth = CABLE_SYMBOL_THICKNESS;
            l_dxfPolyline.DefaultEndWidth = CABLE_SYMBOL_THICKNESS;

            a_coll.Add(l_dxfPolyline);


            foreach (Insert.Satelite l_sat in a_cable_symbol.m_satelites)
            {
                if (l_sat.m_visible)
                {


                    ExportSatelite(a_coll, l_sat, a_pCadDoc);
                }
            }

        }

        private static void ExportQImage(DxfEntityCollection a_coll, QImage a_image, PCadDoc pCadDoc, DxfImageDef a_imgDef)
        {
            Point3D l_inserionPoint = new Point3D(a_image.m_position.Left, a_image.m_position.Top, 0);
            l_inserionPoint.Y += a_image.GetHeight();
        
            l_inserionPoint.Y *= REVERSE_Y;

            Point l_center_point_2D = a_image.GetCenterPoint();
            l_center_point_2D.Y *= REVERSE_Y;
            Point3D l_center_point_3D = new Point3D(l_center_point_2D.X, l_center_point_2D.Y, 0);

            DxfImage l_img = new DxfImage();
            l_img.ImageDef = a_imgDef;
            l_img.InsertionPoint = l_inserionPoint;

            l_img.SetDefaultBoundaryVertices();

            double li_coef_x = a_image.GetWidth() / a_imgDef.Size.X;
            double li_coef_y = a_image.GetHeight() / a_imgDef.Size.Y;
        

            l_img.XAxis = new Vector3D(li_coef_x, 0D, 0D);
            l_img.YAxis = new Vector3D(0D, li_coef_y, 0D);

            TransformConfig tc = new TransformConfig();

            double ld_angle_rad = Angle2Radians((int)a_image.m_angle_tenths);

            double ld_hor = a_image.m_hor ? -1 : 1;
            double ld_ver = a_image.m_ver ? -1 : 1;


            Matrix4D transform =
                        Transformation4D.Translation((Vector3D)l_center_point_3D) *
                        Transformation4D.RotateZ(ld_angle_rad) *
                        Transformation4D.Scaling(ld_ver, ld_hor, 1) *
                        Transformation4D.Translation(-(Vector3D)l_center_point_3D);

            l_img.ImageDisplayFlags = ImageDisplayFlags.ShowImage | ImageDisplayFlags.ShowUnalignedImage;
            l_img.TransformMe(tc, transform);

            a_coll.Add(l_img);
        }


        private static void ExportRoundRect(DxfEntityCollection a_coll, DrawRect a_drawRect, bool ab_block)
        {
            System.Drawing.Drawing2D.GraphicsPath l_path = Helper.GetRoundedRect(a_drawRect.m_position, a_drawRect.m_rX, a_drawRect.m_rY);
            l_path.Flatten(null, 1);
            PointF[] l_points = l_path.PathPoints;

            DrawPoly l_polyArc = new DrawPoly(Shape.poly);
            foreach (PointF l_pointF in l_points)
            {
                l_polyArc.AddPoint(Point.Truncate(l_pointF));
            }
            

            l_polyArc.m_objProps = a_drawRect.m_objProps;
            
            ExportPolyline(a_coll, l_polyArc, ab_block);
            ExportPolygon(a_coll, l_polyArc, ab_block);

            Point3D l_center = GetRectCenterPoint(a_drawRect.m_position);
            l_center.Y = -l_center.Y;
            ExportTextInRect(a_coll, l_center, a_drawRect.m_text, a_drawRect.m_efont, a_drawRect.m_text_angle);
        }



        private static void ExportTrafo(DxfEntityCollection a_coll, Trafo a_trafo, PCadDoc a_pCadDoc, HybridDictionary a_dict)
        {
            DxfBlock l_block = new DxfBlock("trafo " + m_trafoName.ToString(System.Globalization.CultureInfo.InvariantCulture));
            m_trafoName++;
            ExportContext.Current.BlockCollection.Add(l_block);
            ExportTrafoInner(l_block.Entities, a_trafo);
            ExportInsert(a_coll, a_trafo, a_pCadDoc, l_block, null);
        }

        private static void ExportTrafoInner(DxfEntityCollection a_coll, Trafo a_trafo)
        {
            a_trafo.CalculatePosition();

            //disregard the position of the trafo, because this is a block
            //position will be applied in dxfInsert
            int li_x = 0; //a_trafo.m_position.X;
            int li_y = 0; //a_trafo.m_position.Y;

            ExportWinding(a_coll, a_trafo.m_pri, li_x - 25, true);
            ExportWinding(a_coll, a_trafo.m_sec, li_x + 25, false);

            Point start = new Point();
            Point cil = new Point();
            start.X = li_x;
            start.Y = li_y - a_trafo.m_stairjadro - (a_trafo.m_vyskajadra / 2);
            cil.X = li_x;
            cil.Y = li_y - a_trafo.m_stairjadro + (a_trafo.m_vyskajadra / 2);

            DrawPoly l_poly = new DrawPoly(Shape.polyline);
            l_poly.AddPoint(start.X, start.Y);
            l_poly.AddPoint(cil.X, cil.Y);

            ExportPolyline(a_coll, l_poly, true);

        }

        private static void ExportWinding(DxfEntityCollection a_coll, TrafoWinding a_winding, int ai_x, bool ab_prim)
        {
            int Y = 0;// a_trafo.m_position.Y;
            int zacatek = Y + (a_winding.WindingHeight / 2) - a_winding.Stair;
            if (a_winding.m_w1 != 0)
            {
                Civka(a_coll, ai_x, zacatek, a_winding.m_w1, ab_prim); zacatek -= ((a_winding.m_w1 + 1) * Trafo.VYSKA_OBLOUKU);
                if (a_winding.m_w2 != 0)
                {
                    Civka(a_coll, ai_x, zacatek, a_winding.m_w2, ab_prim); zacatek -= ((a_winding.m_w2 + 1) * Trafo.VYSKA_OBLOUKU);
                    if (a_winding.m_w3 != 0)
                    {
                        Civka(a_coll, ai_x, zacatek, a_winding.m_w3, ab_prim); zacatek -= ((a_winding.m_w3 + 1) * Trafo.VYSKA_OBLOUKU);
                        if (a_winding.m_w4 != 0)
                        {
                            Civka(a_coll, ai_x, zacatek, a_winding.m_w4, ab_prim); zacatek -= ((a_winding.m_w4 + 1) * Trafo.VYSKA_OBLOUKU);
                            if (a_winding.m_w5 != 0)
                            {
                                Civka(a_coll, ai_x, zacatek, a_winding.m_w5, ab_prim); zacatek -= ((a_winding.m_w5 + 1) * Trafo.VYSKA_OBLOUKU);
                                if (a_winding.m_w6 != 0)
                                {
                                    Civka(a_coll, ai_x, zacatek, a_winding.m_w6, ab_prim); zacatek -= ((a_winding.m_w6 + 1) * Trafo.VYSKA_OBLOUKU);
                                }
                            }
                        }
                    }
                }
            }
            
        }

        //Civka(CDC* pDC, int X, int Y, int pocet, BOOL druh)
        private static void Civka(DxfEntityCollection a_coll, int ai_x, int ai_y, int ai_pocet, bool ab_prim)
        {
            for (int i = 0; i < ai_pocet; i++)
            {
                Point l_start = new Point();
                Point l_cil = new Point();
                Point l_aux1 = new Point();
                Point l_aux2 = new Point();

                l_start.X = ai_x;
                l_start.Y = ai_y - Trafo.VYSKA_OBLOUKU;
                l_cil.X = ai_x;
                l_cil.Y = ai_y;

                if (ab_prim)
                {//primar
                    //								stred							 polomer			 start             cil
                    //My_BOblouk(pDC, CPoint(X, Y + m_vyskaoblouku / 2), CSize(10, 10), CPoint(X, Y - m_vyskaoblouku), CPoint(X, Y)); Y -= m_vyskaoblouku;

                    l_aux1.X = l_start.X + 20;
                    l_aux1.Y = l_start.Y - 1;
                    l_aux2.X = l_cil.X + 20;
                    l_aux2.Y = l_cil.Y + 1; 
                }
                else
                {//sek
                    //My_BOblouk(pDC, CPoint(X, Y + m_vyskaoblouku / 2), CSize(-10, 10), CPoint(X, Y - m_vyskaoblouku), CPoint(X, Y)); Y -= m_vyskaoblouku;

                    l_aux1.X = l_start.X - 20;
                    l_aux1.Y = l_start.Y - 1;
                    l_aux2.X = l_cil.X - 20;
                    l_aux2.Y = l_cil.Y + 1; 
                
                }

                DrawPoly l_bezier = new DrawPoly(Shape.bezier);
                l_bezier.AddPoint(l_start.X, l_start.Y);
                l_bezier.AddPoint(l_aux1.X, l_aux1.Y);
                l_bezier.AddPoint(l_aux2.X, l_aux2.Y);
                l_bezier.AddPoint(l_cil.X, l_cil.Y);

                ExportBezier(a_coll, l_bezier, true);

                ai_y -= Trafo.VYSKA_OBLOUKU;
            }
        }

        private static void ExportWire(DxfEntityCollection a_coll, DxfNet.Wire a_wire, PCadDoc a_pCadDoc)
        {
            ExportPolyline(a_coll, a_wire, false);

            if (a_wire.GetDrop1())
            {
                ExportJoint(a_coll, a_wire.m_points[0], a_wire.m_objProps.m_logpen.m_color);
            }
            if (a_wire.GetDrop2())
            {
                int li_lastIndex = a_wire.m_points.Count - 1;
                ExportJoint(a_coll, a_wire.m_points[li_lastIndex], a_wire.m_objProps.m_logpen.m_color);
            }


            ExportWireLabels(a_coll, a_pCadDoc.Parent.m_fonts.m_fontType, a_pCadDoc.Parent.m_settingsNumberingWire, a_wire);
        }



        private static void ExportWireLabels(DxfEntityCollection a_coll, EFont a_efont, SettingsNumberingWire pSettings, DxfNet.Wire a_wire)
        {
            if ( string.IsNullOrEmpty(a_wire.GetName()))
            {
                return;
            }

           

            SettingsNumberingWire.EnumShowWireNumbers l_swn = pSettings.ShowWireNumbers;
			if (l_swn != SettingsNumberingWire.EnumShowWireNumbers.swn_no)
			{
                if ((l_swn == SettingsNumberingWire.EnumShowWireNumbers.swn_both) && (a_wire.IsWireShort(pSettings.Long_Wire_Len)))
                {
                    Export_Straight_Wire_Label(a_coll, a_wire, a_efont, pSettings);
                }
                else
                {
                    if ((!a_wire.Is_connected_first) || l_swn == SettingsNumberingWire.EnumShowWireNumbers.swn_both)
                    {
                        ExportWireLabel(a_coll, a_wire, true, a_efont,
                            pSettings.WireLabelDist_A, pSettings.WireLabelDist_B, pSettings.Vertically);
                    }
                    if ((!a_wire.Is_connected_last) || l_swn == SettingsNumberingWire.EnumShowWireNumbers.swn_both)
                    {
                        ExportWireLabel(a_coll, a_wire, false, a_efont,
                            pSettings.WireLabelDist_A, pSettings.WireLabelDist_B, pSettings.Vertically);
                    }
                }
			}
	
        }
        


        private static void ExportWireLabel(DxfEntityCollection a_coll, DxfNet.Wire a_wire, bool ab_first, EFont a_efont, int ai_a, int ai_b, bool ab_vertically)
        {
            Point l_point_nearest, l_point_next;
            if (ab_first)
            {
                l_point_nearest = a_wire.m_points[0];
                l_point_next = a_wire.m_points[1];
            }
            else
            {
                l_point_nearest = a_wire.m_points.Last();
                l_point_next = a_wire.GetLastButOne();
            }

            UtilsMath.cardinal_directions l_cd = UtilsMath.GetDirection(l_point_nearest, l_point_next);
            if (l_cd == UtilsMath.cardinal_directions.cd_none)
            {
                return;
            }

            DrawWireLabelInternal(a_coll, a_wire, ab_first, a_efont, l_cd, l_point_nearest, ai_a, ai_b, ab_vertically);
        }


        // a = distance from wire
        // b = distance from end of wire
        private static void DrawWireLabelInternal(DxfEntityCollection a_coll, DxfNet.Wire a_wire, bool ab_first, EFont a_efont, UtilsMath.cardinal_directions a_cd, Point a_point_nearest, int ai_a, int ai_b, bool ab_vertically)
        {

            AttachmentPoint l_attachment_point = AttachmentPoint.MiddleCenter;
            Vector3D l_axis = TurnsToVector3D(0);


            if ((a_cd == UtilsMath.cardinal_directions.cd_west) || (a_cd == UtilsMath.cardinal_directions.cd_east))
            {
                a_point_nearest.Y -= ai_a;
                if(a_cd == UtilsMath.cardinal_directions.cd_west)
                {
                    a_point_nearest.X -= ai_b;
                }
                else
                {
                    a_point_nearest.X += ai_b;
                }

                l_attachment_point = 
                                a_cd == UtilsMath.cardinal_directions.cd_east ? 
                                AttachmentPoint.BottomLeft : 
                                AttachmentPoint.BottomRight;
                l_axis = TurnsToVector3D(0);
            }

            else if ((a_cd == UtilsMath.cardinal_directions.cd_north) || (a_cd == UtilsMath.cardinal_directions.cd_south))
            {
                a_point_nearest.X -= ai_a;
                if (a_cd == UtilsMath.cardinal_directions.cd_north)
                {
                    a_point_nearest.Y -= ai_b;
                }
                else
                {
                    a_point_nearest.Y += ai_b;
                }

                if (ab_vertically)
                {
                    l_attachment_point =
                                    a_cd == UtilsMath.cardinal_directions.cd_north ?
                                    AttachmentPoint.BottomLeft :
                                    AttachmentPoint.BottomRight;
                    l_axis = TurnsToVector3D(2);
                }
                else
                {
                    l_attachment_point =
                                    a_cd == UtilsMath.cardinal_directions.cd_north ?
                                    AttachmentPoint.BottomRight :
                                    AttachmentPoint.TopRight;
                    l_axis = TurnsToVector3D(0);
                }

            }


            Point3D l_anchor = new Point3D(a_point_nearest.X, a_point_nearest.Y, 0);


            WireLabelPos l_label = a_wire.GetEndingLabelVis(ab_first);
            if(l_label != null)
            {
                l_anchor = Helper.Point_To_Point3D(l_label.m_point);
                l_attachment_point = AttachmentPoint.MiddleCenter;
            }

            l_anchor.Y *= REVERSE_Y;
            int li_height = GetFontAscentSize(a_efont);

            DxfMText dxfText = new DxfMText(a_wire.GetName(), l_anchor, li_height);
            dxfText.Color = Helper.MakeEntityColorByBlock(a_efont.m_color, false);

            dxfText.AttachmentPoint = l_attachment_point;
            dxfText.XAxis = l_axis;

            a_coll.Add(dxfText);
        }



        private static void ExportJoint(DxfEntityCollection a_coll, Point a_point, System.Drawing.Color a_color)
        {
            const int li_radiusJoint = 7;

            Rectangle l_rect = new Rectangle(a_point.X - li_radiusJoint, a_point.Y - li_radiusJoint, 2 * li_radiusJoint, 2 * li_radiusJoint);
            DrawRect l_ellipse = new DrawRect(Shape.ellipse, l_rect);
            l_ellipse.m_objProps.m_bBrush = true;
            l_ellipse.m_objProps.m_logbrush.m_color = a_color;
            ExportEllipse(a_coll, l_ellipse, false);
        }

        public static int GetFontAscentSize(EFont a_efont)
        {
            string ls_fontName = a_efont.m_faceName;
            int li_fontSize = a_efont.m_size;


            if (ls_fontName == "Aharoni")
            {
                ls_fontName = "Arial";
            }


            FontFamily fontFamily = null;
            try
            {
                fontFamily = new FontFamily(ls_fontName);
            }
            catch (Exception)
            {
                fontFamily = new FontFamily("Arial");
            }


            int li_ascentCore;
            try
            {
                li_ascentCore = fontFamily.GetCellAscent(FontStyle.Regular);
            }
            catch (System.Exception )
            {
                li_ascentCore = 2246;//hodnota kterou dává Arial	
            }
            

            Font font = new Font(fontFamily, li_fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
            int li_ascentPixel = (int)font.Size * li_ascentCore / fontFamily.GetEmHeight(FontStyle.Regular);

//            return (10 * li_ascentPixel / 35);
            //2012-02-02 zmensit vsechny texty
            return (65 * li_ascentPixel / 350);
        }


        private static void ExportFreeText(DxfEntityCollection a_coll, FreeText freeText)
        {
            if (freeText.m_text.Length == 0) //2012-02-02
            {
                return;
            }

            string ls_text = freeText.m_text;
            
            //resolve
            if (freeText.m_isInTb)
            {
                foreach (string ls_key in ExportContext.Current.PCadDocument.m_summInfo.Keys)
                {
                    string ls_template = string.Format("{{{0}}}", ls_key);
                    //ls_template = ls_template.ToLower();
                    ls_text = ls_text.Replace(ls_template, ExportContext.Current.PCadDocument.m_summInfo[ls_key].ToString());
                }

                foreach (string ls_key in ExportContext.Current.PCadDocument.Parent.m_summInfo.Keys)
                {
                    string ls_template = string.Format("{{{0}}}", ls_key);
                    //ls_template = ls_template.ToLower();
                    ls_text = ls_text.Replace(ls_template, ExportContext.Current.PCadDocument.Parent.m_summInfo[ls_key].ToString());
                }
            }

            ls_text = ls_text.Replace("\r\n", @"\P");
            ls_text = ls_text.Replace("\n", @"\P");
            
            Point3D l_leftTop = new Point3D(freeText.m_position.X, freeText.m_position.Y, 0);
            l_leftTop.Y *= REVERSE_Y;
            int li_height = GetFontAscentSize(freeText.m_efont);
            

            DxfMText dxfText = new DxfMText(ls_text, l_leftTop, li_height);
           
            dxfText.AttachmentPoint = GetAttachementPoint(freeText.m_alignment);//2012-11-2
            dxfText.XAxis = Vector3D.FromAngle(freeText.m_angle * WW.Math.Constants.DegreesToRadians / 10);

            dxfText.Layer = ExportContext.Current.Layer;
            dxfText.Color = Helper.MakeEntityColorByBlock(freeText.m_efont.m_color, false);

            //add style
            string ls_font_name = freeText.m_efont.m_faceName;
            if (freeText.m_efont.m_bold) ls_font_name += " Bold";
            if (freeText.m_efont.m_ital) ls_font_name += " Italic";

            string ls_font_file_name = EFont_To_Font_File_Name(ls_font_name);
            DxfTextStyle textStyle = new DxfTextStyle(ls_font_name, ls_font_file_name);
            ExportContext.Current.Model.TextStyles.Add(textStyle);
            dxfText.Style = textStyle;

            a_coll.Add(dxfText);
        }


        private static string EFont_To_Font_File_Name(string as_font_name)
        {
            RegistryKey fontKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\\Fonts", false);
            string[] names = fontKey.GetValueNames();
            foreach(string ls_name in names)
            {
                if(ls_name.Contains(as_font_name))
                {
                    string ls_temp = fontKey.GetValue(ls_name).ToString();
                    return ls_temp;
                }
            }

            return "tahoma.ttf";
        }


        private static AttachmentPoint GetAttachementPoint(QTextAlignment a_align)
        {
            switch (a_align)
            {
                case QTextAlignment.AL_LM:
                    return AttachmentPoint.MiddleLeft;
                case QTextAlignment.AL_RM:
                    return AttachmentPoint.MiddleRight;
                default:
                    return AttachmentPoint.MiddleCenter;
            }
        }

        private static Vector3D TurnsToVector3D(int ai_turns)
        {
            switch (ai_turns)
            {
                case 0: return new Vector3D(1, 0, 0);
                case 1: return new Vector3D(1, 1, 0);
                case 2: return new Vector3D(0, 1, 0);
                case 3: return new Vector3D(-1, 1, 0);
                case 4: return new Vector3D(-1, 0, 0);
                case 5: return new Vector3D(-1, -1, 0);
                case 6: return new Vector3D(0, -1, 0);
                case 7: return new Vector3D(1, -1, 0);
                default: return new Vector3D(1, 0, 0);
            }
        }

        public static void SetupLayerObsolete(DxfEntity a_entity, DrawObj a_obj)
        {
            if (a_obj.m_layer != null)
            {
                a_entity.Layer = ExportContext.Current.Model.Layers[a_obj.m_layer.Name];
            }
        }

        
        /// <summary>
        /// if the insert is parametrized, we need to make a copy of the block
        /// </summary>
        /// <param name="a_insert"></param>
        /// <param name="a_block"></param>
        /// <param name="a_dict"></param>
        private static void Make_Parametrized_Block(ref Insert a_insert, ref DxfBlock a_block, ref HybridDictionary a_dict)
        {
            if (a_insert.m_parameters.Count > 0)
            {
                //make a copy
                Clone_Block(ref a_block);

                //change the name and add it to the repo
                string ls_appendix = GetNameAppendix(a_insert.m_parameters);
                a_block.Name += ls_appendix;
                if (a_dict[a_block.Name] != null)
                {
                    return;
                }
                a_dict[a_block.Name] = a_block;
                if(ExportContext.Current.Model.Blocks.Contains(a_block.Name))
                {
                    return;
                }
                ExportContext.Current.Model.Blocks.Add(a_block);

                foreach (DxfEntity l_entity in a_block.Entities)
                {
                    if (l_entity is DxfMText)
                    {
                        DxfMText l_text = l_entity as DxfMText;
                        if (l_text != null)
                        {
                            if (IsTemplate(l_text.Text))
                            {
                                //resolve template
                                foreach (var ls_key in a_insert.m_parameters.Keys)
                                {
                                    string ls_keyWithBraces = string.Format("{0}", ls_key);
                                    string ls_new_string = a_insert.m_parameters[ls_key].ToString();
                                    l_text.Text = l_text.Text.Replace(ls_keyWithBraces, ls_new_string);
                                }
                            }
                        }
                    }//if DxfMText
                }//foreach entity
            }// if has parameters
        }


        private static void Clone_Block(ref DxfBlock a_block)
        {
            CloneContext cloneContext = new CloneContext(
                ExportContext.Current.Model,
                ExportContext.Current.Model,
                ReferenceResolutionType.CloneMissing);
            DxfBlock l_new_block = (DxfBlock)a_block.Clone(cloneContext);
            cloneContext.ResolveReferences();
            a_block = l_new_block;
        }

        private static void Make_Flipped_Block(ref Insert a_insert, ref DxfBlock a_block, ref HybridDictionary a_dict)
        {
            if(a_dict == null)
            {
                return;
            }

            if(a_insert.m_ver)
            {
                //make a copy
                Clone_Block(ref a_block);


                //change the name and add it to the repo
                string ls_appendix = "_h";
                a_block.Name += ls_appendix;
                if (a_dict[a_block.Name] != null)
                {
                    return;
                }

                a_dict[a_block.Name] = a_block;

                if (ExportContext.Current.Model.Blocks.Contains(a_block.Name))
                {
                    return;
                }
                ExportContext.Current.Model.Blocks.Add(a_block);

                foreach (DxfEntity l_entity in a_block.Entities)
                {
                    if (l_entity is DxfMText)
                    {
                        DxfMText l_text = l_entity as DxfMText;
                        if (l_text != null)
                        {
                            l_text.XAxis = new Vector3D(-1, 0, 0);
                            l_text.ZAxis = new Vector3D(0, 0, -1);
                            l_text.AttachmentPoint = Helper.FlipAlignment(l_text.AttachmentPoint);

                        }
                    }//if DxfMText
                }//foreach entity
            }
        }

        public static void ExportInsert(DxfEntityCollection a_dxfEntityCollection, Insert a_insert, PCadDoc a_pCadDoc, DxfBlock a_block, HybridDictionary a_dict)
        {
            Point3D l_centerPoint = GetRectCenterPoint(a_insert.m_position);
            l_centerPoint.Y *= REVERSE_Y;


            
            Make_Parametrized_Block(ref a_insert, ref a_block, ref a_dict);
            Make_Flipped_Block(ref a_insert, ref a_block, ref a_dict);


            DxfInsert l_insert = new DxfInsert(a_block, l_centerPoint);
            l_insert.ScaleFactor = new Vector3D(
                a_insert.m_ver ? (- a_insert.m_scaleX) : (a_insert.m_scaleX), 
                a_insert.m_hor ? (- a_insert.m_scaleY) : (a_insert.m_scaleY),
                1);

            l_insert.Color = Helper.MakeEntityColorByBlock(a_insert.m_color_border, false);

            l_insert.Rotation = Angle2Radians(a_insert.m_angle);

            l_insert.Layer = ExportContext.Current.Layer;
            a_dxfEntityCollection.Add(l_insert);

            foreach (Insert.Satelite l_sat in a_insert.m_satelites)
            {
                if (l_sat.m_visible)
                {
                    if ((l_sat.m_name == "_type") && (!a_pCadDoc.Parent.m_show_types))
                    {
                        continue;
                    }
                    if ((l_sat.m_name == "_value") && (!a_pCadDoc.Parent.m_show_values))
                    {
                        continue;
                    }

                    ExportSatelite(a_dxfEntityCollection, l_sat, a_pCadDoc);
                }
            }
            
        }

        private static string GetNameAppendix(Hashtable m_parameters)
        {
            string ls_result = "-";
            foreach(var ls_val in m_parameters.Values)
            {
                ls_result += ls_val + "-";
            }

            ls_result = ls_result.Replace("/", "-");
            return ls_result;
        }

        private static bool IsTemplate(string as_text)
        {
            if (as_text.Length < 3)
            {
                return false;
            }

            int li_pos_1 = as_text.IndexOf("{");
            if (li_pos_1 == -1)
            {
                return false;
            }
            int li_pos_2 = as_text.IndexOf("}");
            if (li_pos_2 == -1)
            {
                return false;
            }
            return (1 + li_pos_1) < li_pos_2;
        }


        public static void ExportSatelite(DxfEntityCollection a_dxfEntityCollection, Insert.Satelite a_sat, PCadDoc a_pCadDoc)
        {
            string ls_text = a_sat.m_value;

            EFont l_efont = null;
            if (a_sat.m_name == "_ref")
            {
                l_efont = a_pCadDoc.Parent.m_fonts.m_fontType;
            }
            else
            {
                l_efont = a_pCadDoc.Parent.m_fonts.m_fontValue;
            }

            if (l_efont == null)
            {
                return;
            }

            int li_height = GetFontAscentSize(l_efont);

            ls_text = ls_text.Replace("\r\n", @"\P");
            Point3D l_center3D = new Point3D(a_sat.m_x, a_sat.m_y, 0);
            l_center3D.Y *= REVERSE_Y;

            DxfMText dxfText = new DxfMText(ls_text, l_center3D, li_height);
            dxfText.Color = Helper.MakeEntityColorByBlock(l_efont.m_color, false);

            dxfText.AttachmentPoint = GetAttachementPoint(a_sat.m_alignment);
            dxfText.XAxis = TurnsToVector3D(a_sat.m_turns);
            
            a_dxfEntityCollection.Add(dxfText);
        }

        public static string Sanitize(string as_input)
        {
            string ls_out = as_input;
            ls_out = ls_out.Replace(' ', '_');
            ls_out = ls_out.Replace('.', '-');
            ls_out = ls_out.Replace(',', '-');
            ls_out = ls_out.Replace(';', '-');
            ls_out = ls_out.Replace(':', '-');
            ls_out = ls_out.Replace('/', '-');
            ls_out = ls_out.Replace('\\', '-');

            return ls_out;
        }


        private static void ExportBlockPpd(DxfBlockCollection dxfBlockCollection, PpdDoc ppdDoc, HybridDictionary a_dict)
        {
            string ls_name = ppdDoc.m_lG;
            if (ls_name.Length == 0)
            {
                throw new Exception("lG of l_block is empty");
            }

            DxfBlock l_block = new DxfBlock(ls_name);
            if (l_block == null)
            {
                throw new Exception("l_block is null");
            }

            if (!dxfBlockCollection.Contains(ls_name))
            {
                //export images
                foreach (QImageDesc l_imgDesc in ppdDoc.m_repo.m_listImgDesc)
                {
                    ExportQImageDesc(ExportContext.Current.Model.Images, l_imgDesc, a_dict);
                }
                //export ppds
                
                foreach (PpdDoc l_ppdDoc in ppdDoc.m_repo.m_listPpd)
                {
                    ExportBlockPpd(ExportContext.Current.Model.Blocks, l_ppdDoc, a_dict);
                }
                

                dxfBlockCollection.Add(l_block);
                ExportDrawDoc(l_block.Entities, ppdDoc, a_dict, true);
                a_dict[ppdDoc.m_lG] = l_block;
            }
            else 
            {
                if (null == a_dict[ppdDoc.m_lG])//this happens only if duplicate paths with different lastGUID exist
                {
                    a_dict[ppdDoc.m_lG] = dxfBlockCollection[ls_name];
                }
            }

        }


        private static void ExportRect(DxfEntityCollection a_coll, DrawRect a_drawRect, bool ab_block)
        {
            //prepare points
            int li_arrSize = 4;
            Point2D[] l_arrPoints = new Point2D[li_arrSize];

            l_arrPoints[0].X = a_drawRect.m_position.Left;
            l_arrPoints[0].Y = a_drawRect.m_position.Bottom * REVERSE_Y;
            l_arrPoints[1].X = a_drawRect.m_position.Right;
            l_arrPoints[1].Y = a_drawRect.m_position.Bottom * REVERSE_Y;
            l_arrPoints[2].X = a_drawRect.m_position.Right;
            l_arrPoints[2].Y = a_drawRect.m_position.Top * REVERSE_Y;
            l_arrPoints[3].X = a_drawRect.m_position.Left;
            l_arrPoints[3].Y = a_drawRect.m_position.Top * REVERSE_Y;

            ExportPolygonFilled(a_coll, l_arrPoints, a_drawRect.m_objProps, ab_block);

            DxfPolyline2D dxfPolyline = new DxfPolyline2D(l_arrPoints);
            dxfPolyline.Closed = true;

            dxfPolyline.DefaultStartWidth = a_drawRect.m_objProps.m_logpen.m_width;
            dxfPolyline.DefaultEndWidth = a_drawRect.m_objProps.m_logpen.m_width;

            //99dxfPolyline.ColorSource = AttributeSource.This;
            //99 dxfPolyline.Color = EntityColor.CreateFrom(a_drawRect.m_objProps.m_logpen.m_color);
            dxfPolyline.Color = Helper.MakeEntityColorByBlock(a_drawRect.m_objProps.m_logpen.m_color, ab_block);

            DxfLineType l_lineType = GetLineTypeFromObjProps(a_drawRect.m_objProps);
            dxfPolyline.LineType = l_lineType;
            //9 dxfPolyline.LineTypeSource = AttributeSource.This;


            Point3D l_center = GetRectCenterPoint(a_drawRect.m_position);
            l_center.Y = -l_center.Y;
            ExportTextInRect(a_coll, l_center, a_drawRect.m_text, a_drawRect.m_efont, a_drawRect.m_text_angle);

            dxfPolyline.Layer = ExportContext.Current.Layer;
            a_coll.Add(dxfPolyline);
        }
/*
        private static void ExportRectFilled(DxfEntityCollection a_coll, Point2D[] a_arrPoints, DrawRect a_drawRect)
        {
            //fill solid rect
            DxfHatch hatch = new DxfHatch();
            hatch.Color = a_drawRect.m_objProps.m_logbrush.m_color;

            // A boundary path bounded by lines.
            DxfHatch.BoundaryPath boundaryPath1 = new DxfHatch.BoundaryPath();
            boundaryPath1.Type = BoundaryPathType.Default;
            hatch.BoundaryPaths.Add(boundaryPath1);
            boundaryPath1.Edges.Add(new DxfHatch.BoundaryPath.LineEdge(a_arrPoints[0], a_arrPoints[1]));
            boundaryPath1.Edges.Add(new DxfHatch.BoundaryPath.LineEdge(a_arrPoints[1], a_arrPoints[2]));
            boundaryPath1.Edges.Add(new DxfHatch.BoundaryPath.LineEdge(a_arrPoints[2], a_arrPoints[3]));
            boundaryPath1.Edges.Add(new DxfHatch.BoundaryPath.LineEdge(a_arrPoints[3], a_arrPoints[0]));

            a_coll.Add(hatch);
        }
*/
        private static void ExportEllipse(DxfEntityCollection a_coll, DrawRect a_drawRect, bool ab_block)
        {
            //need center, long axis and min/maj ratio
            Point3D l_center = GetRectCenterPoint(a_drawRect.m_position);
            l_center.Y *= REVERSE_Y;
            
            Vector3D l_longAxis = new Vector3D();
            double li_otherDimensionHalf;
            double ld_ratio;
            //which axis is longer?
            double li_width = Math.Abs(a_drawRect.m_position.Width);
            double li_height = Math.Abs(a_drawRect.m_position.Height);

            if (li_width > li_height)
            {
                l_longAxis.X = li_width / 2;
                l_longAxis.Y = l_longAxis.Z = 0;
                li_otherDimensionHalf = li_height / 2;
                if (li_otherDimensionHalf == 0)
                {
                    return;
                }
                ld_ratio = li_otherDimensionHalf / l_longAxis.X;
            }
            else
            {
                l_longAxis.Y = li_height / 2;
                l_longAxis.X = l_longAxis.Z = 0;
                li_otherDimensionHalf = li_width / 2;
                if (li_otherDimensionHalf == 0)
                {
                    return;
                }
                ld_ratio = li_otherDimensionHalf / l_longAxis.Y;
            }

            if (a_drawRect.m_objProps.m_bBrush)
            {
                ExportEllipseSolid(a_coll, l_center, l_longAxis, ld_ratio, a_drawRect.m_objProps, false, ab_block);
            }
            ExportEllipseSolid(a_coll, l_center, l_longAxis, ld_ratio, a_drawRect.m_objProps, true, ab_block);
   
            DxfEllipse dxfEllipse = new DxfEllipse(l_center, l_longAxis, ld_ratio);


//            dxfEllipse. =  .DefaultStartWidth = drawRect.m_objProps.m_logpen.m_width;

          

            a_drawRect.m_objProps.m_logpen.m_width = Calculate_Line_Thickness_Ellipse(a_drawRect.m_objProps.m_logpen.m_width);

            dxfEllipse.LineWeight = (short)(10 * a_drawRect.m_objProps.m_logpen.m_width);
            //99 dxfEllipse.ColorSource = AttributeSource.This;
            dxfEllipse.Color = Helper.MakeEntityColorByBlock(a_drawRect.m_objProps.m_logpen.m_color, ab_block);

            DxfLineType l_lineType = GetLineTypeFromObjProps(a_drawRect.m_objProps);
            dxfEllipse.LineType = l_lineType;
            //9dxfEllipse.LineTypeSource = AttributeSource.This;

            dxfEllipse.Layer = ExportContext.Current.Layer; 
            a_coll.Add(dxfEllipse);

            ExportTextInRect(a_coll, l_center, a_drawRect.m_text, a_drawRect.m_efont, a_drawRect.m_text_angle);
        }

        private static void ExportCircle(DxfEntityCollection a_coll, QCircle a_circle, bool ab_block)
        {
            int li_radius = DxfNet.Helper.Distance2Points(a_circle.m_center, a_circle.m_tangent);
            Point3D l_center = new Point3D(a_circle.m_center.X, a_circle.m_center.Y * REVERSE_Y, 0);

            DxfCircle dxfCircle = new DxfCircle(l_center, li_radius);

/*
            a_circle.m_objProps.m_logpen.m_width = Calculate_Line_Thickness_Ellipse(a_drawRect.m_objProps.m_logpen.m_width);

            dxfEllipse.LineWeight = (short)(10 * a_drawRect.m_objProps.m_logpen.m_width);
            //99 dxfEllipse.ColorSource = AttributeSource.This;
            dxfEllipse.Color = Helper.MakeEntityColorByBlock(a_drawRect.m_objProps.m_logpen.m_color, ab_block);

            DxfLineType l_lineType = GetLineTypeFromObjProps(a_drawRect.m_objProps);
            dxfEllipse.LineType = l_lineType;
            //9dxfEllipse.LineTypeSource = AttributeSource.This;
*/

            dxfCircle.Layer = ExportContext.Current.Layer;
            a_coll.Add(dxfCircle);

//9            ExportTextInRect(a_coll, l_center, a_drawRect.m_text, a_drawRect.m_efont, a_drawRect.m_text_angle);

        }


        public static int Calculate_Line_Thickness_Ellipse(int ai_width)
        {
            //Lineweight may not be larger than 211. (2012-11-05)
            //pozor, síla čáry smí mít jen některé hodnoty !!!
            // http://www.woutware.com/doc/cadlib3.5/html/77645917-cadd-ddfa-f10b-57fbbaaf64ae.htm
            // http://knowledge.autodesk.com/support/autocad/learn-explore/caas/CloudHelp/cloudhelp/2015/ENU/AutoCAD-Core/files/GUID-969FE4A6-C30D-44DE-AFD4-A81B53F175F6-htm.html



            //int[] l_weights = new int[] { 0, 5, 9, 13, 15, 18, 20, 25, 30, 35, 40, 50, 53, 60, 70, 80, 90, 100, 106, 120, 140, 158, 200, 211 };

         

            const int li_maximum_thickness = 21;
            ai_width = Math.Min(ai_width, li_maximum_thickness);

            const int li_minimum_thickness = 2;
            ai_width = Math.Max(ai_width, li_minimum_thickness);

            if (ai_width == 11)
            {
                return 10;
            }
            if (ai_width == 13)
            {
                return 12;
            }
            if ((ai_width >= 15) && (ai_width <= 17))
            {
                return 14;
            }
            if ((ai_width >= 18) && (ai_width <= 20))
            {
                return 20;
            }


            return ai_width;
        }


        private static void ExportTextInRect(DxfEntityCollection a_coll, Point3D l_center, string as_text, EFont a_efont, int a_angle)
        {
            if (as_text == null)
            {
                return;
            }
            string ls_text = as_text.Replace("\r\n", @"\P");
            ls_text = ls_text.Replace("\n", @"\P");

            int li_height = GetFontAscentSize(a_efont);
            DxfMText dxfText = new DxfMText(ls_text, l_center, li_height);
            dxfText.AttachmentPoint = AttachmentPoint.MiddleCenter;
            dxfText.XAxis = Vector3D.FromAngle(a_angle * WW.Math.Constants.DegreesToRadians / 10);

            dxfText.Layer = ExportContext.Current.Layer;            
            a_coll.Add(dxfText);
        }


        private static void ExportEllipseSolid(DxfEntityCollection a_dxfEntityCollection, Point3D l_center, Vector3D l_longAxis, double ld_ratio, ObjProps a_objProps, bool ab_hatch, bool ab_block)
        {
            if (ab_hatch && (a_objProps.m_hatchtype == HatchType.NONE))
            {
                return;
            }
            DxfHatch l_hatch = new DxfHatch();
            if (!ab_hatch)
            {
                l_hatch.Color = EntityColor.CreateFrom((WW.Drawing.ArgbColor)a_objProps.m_logbrush.m_color);
                //99l_hatch.ColorSource = AttributeSource.This;
            }

            DxfHatch.BoundaryPath l_boundaryPath = new DxfHatch.BoundaryPath();
            l_boundaryPath.Type = BoundaryPathType.Outermost;
            l_hatch.BoundaryPaths.Add(l_boundaryPath);

            DxfHatch.BoundaryPath.EllipseEdge ellipseEdge = new DxfHatch.BoundaryPath.EllipseEdge();
            ellipseEdge.Center = (WW.Math.Point2D)l_center;
            ellipseEdge.CounterClockWise = true;
            ellipseEdge.MajorAxisEndPoint = Helper.Vector3DTo2D(l_longAxis);
            ellipseEdge.MinorToMajorRatio = ld_ratio;
            ellipseEdge.StartAngle = 0;
            ellipseEdge.EndAngle = System.Math.PI * 2d;

            l_boundaryPath.Edges.Add(ellipseEdge);

            if (ab_hatch)
            {
                if (a_objProps.m_hatchtype != HatchType.NONE)
                {
                    DxfPattern pattern = GetHatchPattern(a_objProps);
                    if (pattern != null)
                    {
                        l_hatch.Pattern = pattern;
                    }
                }
            }

            l_hatch.Layer = ExportContext.Current.Layer;
            a_dxfEntityCollection.Add(l_hatch);
        }


        private static DxfPattern GetHatchPattern(ObjProps a_objProps)
        {
            if (a_objProps.m_hatchtype == HatchType.NONE)
            {
                return null;
            }

            DxfPattern pattern = new DxfPattern();
  
            int li_spacing = a_objProps.m_hatchspacing;

            switch (a_objProps.m_hatchtype)
            { 
                case HatchType.HT_HORIZONTAL:
                    pattern.Lines.Add(GetPatternLine(a_objProps.m_hatchtype, li_spacing));
                    break;
                case HatchType.HT_VERTICAL:
                    pattern.Lines.Add(GetPatternLine(a_objProps.m_hatchtype, li_spacing));
                    break;
                case HatchType.HT_FDIAGONAL:
                    pattern.Lines.Add(GetPatternLine(a_objProps.m_hatchtype, li_spacing));
                    break;
                case HatchType.HT_BDIAGONAL:
                    pattern.Lines.Add(GetPatternLine(a_objProps.m_hatchtype, li_spacing));
                    break;
                case HatchType.HT_CROSS:
                    pattern.Lines.Add(GetPatternLine(HatchType.HT_HORIZONTAL, li_spacing));
                    pattern.Lines.Add(GetPatternLine(HatchType.HT_VERTICAL, li_spacing));
                    break;
                case HatchType.HT_DIAGCROSS:
                    pattern.Lines.Add(GetPatternLine(HatchType.HT_FDIAGONAL, li_spacing));
                    pattern.Lines.Add(GetPatternLine(HatchType.HT_BDIAGONAL, li_spacing));
                    break;
            }
            
            
            return pattern;
        }

        private static DxfPattern.Line GetPatternLine(HatchType a_hatchType, int ai_spacing)
        {
            if (ai_spacing == 0)
            {
                ai_spacing = 1;//2012-06-25
            }


            DxfPattern.Line l_patternLine1 = new DxfPattern.Line();
            switch (a_hatchType)
            {
                case HatchType.HT_HORIZONTAL:
                    l_patternLine1.Angle = 0;
                    l_patternLine1.Offset = new Vector2D(0, ai_spacing);
                    l_patternLine1.DashLengths.Add(0.02d);
                    break;
                case HatchType.HT_VERTICAL:
                    l_patternLine1.Angle = Math.PI / 2d;
                    l_patternLine1.Offset = new Vector2D(ai_spacing, 0);
                    l_patternLine1.DashLengths.Add(0.02d);
                    break;
                case HatchType.HT_FDIAGONAL:
                    l_patternLine1.Angle = Math.PI / 4d;
                    l_patternLine1.Offset = new Vector2D(ai_spacing * 1.41, 0);
                    l_patternLine1.DashLengths.Add(0.02d);
                    break;
                case HatchType.HT_BDIAGONAL:
                    l_patternLine1.Angle = (3d * Math.PI) / 4d;
                    l_patternLine1.Offset = new Vector2D(ai_spacing * 1.41, 0);
                    l_patternLine1.DashLengths.Add(0.02d);
                    break;
            }
            return l_patternLine1;
        }

        private static void ExportArc(DxfEntityCollection a_coll, DrawRect drawRect, bool ab_block)
        {
            if ((drawRect.m_position.Width == 0) || (drawRect.m_position.Height == 0))//fix 2009-2-6
            {
                return;
            }

            double endAngle = System.Math.Atan2(drawRect.m_arcBegin.Height, drawRect.m_arcBegin.Width);
            double startAngle = System.Math.Atan2(drawRect.m_arcEnd.Height, drawRect.m_arcEnd.Width);


            if (endAngle == startAngle)
            {
                return;//2012-04-03
            }

            startAngle *= 180 / Math.PI;
            endAngle *= 180 / Math.PI;

            double sweepAngle = endAngle - startAngle;
            if (sweepAngle < 0)
            {
                sweepAngle += 360;
            }


            RectangleF l_rect = drawRect.m_position;
            Helper.FixRectangle(ref l_rect);

            if (l_rect.Width == l_rect.Height)
            {
                ExportSquareArc(a_coll, drawRect, ab_block, startAngle, endAngle);
            }
            else
            {
                System.Drawing.Drawing2D.GraphicsPath l_path = new System.Drawing.Drawing2D.GraphicsPath();
                l_path.AddArc(l_rect, (float)startAngle, (float)sweepAngle);
                l_path.Flatten(null, 1);
                PointF[] l_points = l_path.PathPoints;

                DrawPoly l_polyArc = new DrawPoly(Shape.polyline);
                foreach (PointF l_pointF in l_points)
                {
                    l_polyArc.AddPoint(Point.Truncate(l_pointF));
                }
                l_polyArc.m_objProps.m_logpen.m_color = drawRect.m_objProps.m_logpen.m_color;
                l_polyArc.m_objProps.m_logpen.m_width = drawRect.m_objProps.m_logpen.m_width;
                l_polyArc.m_objProps.m_logpen.m_style = 0;            //prevent this line from drawing arrow, this is just the arc

                ExportPolyline(a_coll, l_polyArc, ab_block);
            }
            

            Point l_centerPoint = DxfNet.Helper.GetRectCenterPoint(l_rect);
            Point l_arcBegin = l_centerPoint + drawRect.m_arcBegin;
            Point l_arcEnd = l_centerPoint + drawRect.m_arcEnd;

            ExportArcArrow(a_coll, drawRect, l_rect, l_centerPoint, l_arcBegin, l_arcEnd, 1, 1, drawRect.m_objProps.m_logpen.m_color, ab_block);
        }


        private static void ExportSquareArc(DxfEntityCollection a_coll, DrawRect a_drawRect, bool ab_block, double startAngle, double endAngle)
        {
            Point3D l_center = GetRectCenterPoint(a_drawRect.m_position);

            l_center.Y *= REVERSE_Y;
            float l_radius = a_drawRect.m_position.Width / 2;


            a_drawRect.m_arcBegin.Height *= REVERSE_Y;
            a_drawRect.m_arcEnd.Height *= REVERSE_Y;


            double l_startAngle = System.Math.Atan2(a_drawRect.m_arcBegin.Height, a_drawRect.m_arcBegin.Width);
            double l_endAngle = System.Math.Atan2(a_drawRect.m_arcEnd.Height, a_drawRect.m_arcEnd.Width);


            DxfArc l_arc = new DxfArc(l_center, l_radius, l_startAngle, l_endAngle);
            l_arc.Layer = ExportContext.Current.Layer;

            a_drawRect.m_objProps.m_logpen.m_width = Calculate_Line_Thickness_Ellipse(a_drawRect.m_objProps.m_logpen.m_width);

            l_arc.LineWeight = (short)(10 * a_drawRect.m_objProps.m_logpen.m_width);
            l_arc.Thickness = 10 * a_drawRect.m_objProps.m_logpen.m_width;

            //99 dxfEllipse.ColorSource = AttributeSource.This;
            l_arc.Color = Helper.MakeEntityColorByBlock(a_drawRect.m_objProps.m_logpen.m_color, ab_block);


            a_coll.Add(l_arc);
        }


        private static void ExportArcArrow(DxfEntityCollection a_coll, DrawRect a_drawRect, RectangleF a_rect, Point a_centerPoint, Point a_arcBegin, Point a_arcEnd, double a_scaleX, double a_scaleY, System.Drawing.Color a_color, bool ab_block)
        {
            if (0 == a_drawRect.m_objProps.m_logpen.m_style)
            {
                return;
            }

            //pokud se sirka a vyska lisi hodne, skoncit
            int li_diff_width_height = (int)Math.Abs(a_rect.Width - a_rect.Height);
            if ((100 * li_diff_width_height / a_rect.Width) > 10)
            {
                return;
            }


            //calc tangenta points
            Point l_stred_kruhu = a_centerPoint;


            const int li_coef_angle = 4;

            ArrowType l_arrowType = (ArrowType)a_drawRect.m_objProps.m_logpen.m_style;
        
            Point l_point_ending = Helper.RotatePoint(a_arcEnd, -90 + li_coef_angle, l_stred_kruhu);
            Point l_point_begin = Helper.RotatePoint(a_arcBegin, 90 - li_coef_angle, l_stred_kruhu);
            int li_thinckness = 1;

            Flip_Point_Y(ref l_point_ending, l_stred_kruhu.Y);
            Flip_Point_Y(ref a_arcEnd, l_stred_kruhu.Y);
            Flip_Point_Y(ref l_point_begin, l_stred_kruhu.Y);
            Flip_Point_Y(ref a_arcBegin, l_stred_kruhu.Y);


            ExportSipkaWithoutStem(a_coll, l_arrowType, l_point_ending, a_arcEnd, a_color, false, !a_drawRect.m_arrow_flipped, a_scaleX, a_scaleY, li_thinckness, ab_block, 1, 1);
            ExportSipkaWithoutStem(a_coll, l_arrowType, l_point_begin, a_arcBegin, a_color, false, a_drawRect.m_arrow_flipped, a_scaleX, a_scaleY, li_thinckness, ab_block, 1, 1);

        }

        private static void ExportPieChord(DxfEntityCollection a_coll, DrawRect drawRect, bool ab_block)
        {
            if ((drawRect.m_position.Width == 0) || (drawRect.m_position.Height == 0))//2008-12-20
            {
                return;
            }

            double endAngle = System.Math.Atan2(drawRect.m_arcBegin.Height, drawRect.m_arcBegin.Width);
            double startAngle = System.Math.Atan2(drawRect.m_arcEnd.Height, drawRect.m_arcEnd.Width);

            if (startAngle == endAngle) // 2011-05-09
            {
                // 2015-09-25 just try what it does with Lehmann drawing return;
            }

            startAngle *= 180 / Math.PI;
            endAngle *= 180 / Math.PI;


            double sweepAngle = endAngle - startAngle;
            if (sweepAngle <= 0)
            {
                sweepAngle += 360;
            }



            System.Drawing.Drawing2D.GraphicsPath l_path = new System.Drawing.Drawing2D.GraphicsPath();
            l_path.AddArc(drawRect.m_position, (float)startAngle, (float)sweepAngle);
            float lf_flatness = 1f;
            l_path.Flatten(null, lf_flatness);
            PointF[] l_points;
            try // 2012-10-01 ver 2.6
            {
                l_points = l_path.PathPoints;
            }
            catch (System.Exception )
            {
                return;
            }
            

            DrawPoly l_polyArc = new DrawPoly(Shape.poly);
            Point3D l_centerPoint3D = GetRectCenterPoint(drawRect.m_position);
            Point l_centerPoint = new Point((int)l_centerPoint3D.X, (int)l_centerPoint3D.Y);

            if (drawRect.m_nShape == Shape.pie)
            {
                l_polyArc.AddPoint(l_centerPoint);
            }
            if (drawRect.m_nShape == Shape.chord)
            {
                PointF l_lastPoint = l_points[l_points.Length - 1];
                l_polyArc.AddPoint(Point.Truncate(l_lastPoint));
            }

            foreach (PointF l_pointF in l_points)
            {
                l_polyArc.AddPoint(Point.Truncate(l_pointF));
            }
            l_polyArc.m_objProps = drawRect.m_objProps;
            ExportPolygon(a_coll, l_polyArc, ab_block);

        }

  

        public static double PrapareParam(double input, double minorToMajorRatio)
        {
            double cos = System.Math.Cos(input);
            double sin = minorToMajorRatio * System.Math.Sin(input);
            double parameter = System.Math.Atan2(sin, cos);
            return parameter;
        }

        private static Point3D GetRectCenterPoint(Rectangle a_rect)
        {
            Point3D l_result = new Point3D();
            l_result.X = (a_rect.Left + a_rect.Right) / 2;
            l_result.Y = (a_rect.Top + a_rect.Bottom) / 2;
            return l_result;
        }

        private static void ExportPolyline(DxfEntityCollection a_coll, DrawPoly drawPoly, bool ab_block)
        {
            int li_arrSize = drawPoly.m_points.Count;
            ArrowType l_arrowType = (ArrowType)drawPoly.m_objProps.m_logpen.m_style;
            bool lb_isArrow = l_arrowType != 0;

            /*
            if (lb_isArrow)
            {
                li_arrSize--;//last segment will be drawn by ExportSipka()
            }
            */ 

            if (li_arrSize > 1)
            {
                Point2D[] l_arrPoints = new Point2D[li_arrSize];
                for (int i = 0; i < li_arrSize; i++)
                {
                    l_arrPoints[i].X = drawPoly.m_points[i].X;
                    l_arrPoints[i].Y = REVERSE_Y * drawPoly.m_points[i].Y;
                }

                DxfPolyline2D dxfPolyline = new DxfPolyline2D(l_arrPoints);

                dxfPolyline.DefaultStartWidth = drawPoly.m_objProps.m_logpen.m_width;
                dxfPolyline.DefaultEndWidth = drawPoly.m_objProps.m_logpen.m_width;

                if (drawPoly.m_nShape == Shape.poly)
                {
                    dxfPolyline.Closed = true;
                }

                if (!lb_isArrow)
                {
                    DxfLineType l_lineType = GetLineTypeFromObjProps(drawPoly.m_objProps);
                    dxfPolyline.LineType = l_lineType;
                    //9 dxfPolyline.LineTypeSource = AttributeSource.This;
                }

              
                // puvodni kod dxfPolyline.ColorSource = AttributeSource.This;
                //99 dxfPolyline.ColorSource = AttributeSource.This;//pokus 2013-01-18

                //HACK invert the color to enable import of DXF back to profiCAD
//2013-01-18                dxfPolyline.Color = (drawPoly.m_objProps.m_logpen.m_color == Color.Black) ? Color.White : drawPoly.m_objProps.m_logpen.m_color;


              
                dxfPolyline.Color = Helper.MakeEntityColorByBlock(drawPoly.m_objProps.m_logpen.m_color, ab_block);




                dxfPolyline.Layer = ExportContext.Current.Layer;
                a_coll.Add(dxfPolyline);

                ExportPolylineArrowWithoutStem(a_coll, drawPoly, l_arrowType, drawPoly.m_objProps.m_logpen.m_color, 1, 1, drawPoly.m_objProps.m_logpen.m_width, ab_block, drawPoly.Scale_arrow_x, drawPoly.Scale_arrow_y);

            }
       
        }

        private static void ExportPolylineArrowWithoutStem(DxfEntityCollection a_coll, DrawPoly a_drawPoly, ArrowType a_typ, System.Drawing.Color a_color, double a_scaleX, double a_scaleY, int ai_thickness, bool ab_block, double a_scale_arrow_x, double a_scale_arrow_y)
        {
            int li_points_count = a_drawPoly.m_points.Count;
            if (li_points_count < 2)
            {
                return;
            }

            ExportSipkaWithoutStem(a_coll, a_typ, a_drawPoly.m_points[li_points_count - 2], a_drawPoly.m_points[li_points_count - 1], a_color, false, true, a_scaleX, a_scaleY, ai_thickness, ab_block, a_scale_arrow_x, a_scale_arrow_y);
            ExportSipkaWithoutStem(a_coll, a_typ, a_drawPoly.m_points[1], a_drawPoly.m_points[0], a_color, false, false, a_scaleX, a_scaleY, ai_thickness, ab_block, a_scale_arrow_x, a_scale_arrow_y);
        }

        private static void ExportPolygon(DxfEntityCollection a_coll, DrawPoly a_drawPoly, bool ab_block)
        {
            int li_arrSize = a_drawPoly.m_points.Count;

            Point2D[] l_arrPoints = new Point2D[li_arrSize];
            for (int i = 0; i < li_arrSize; i++)
            {
                l_arrPoints[i].X = a_drawPoly.m_points[i].X;
                l_arrPoints[i].Y = REVERSE_Y * a_drawPoly.m_points[i].Y;
            }

            DxfPolyline2D dxfPolyline = new DxfPolyline2D(l_arrPoints);

            ExportPolygonFilled(a_coll, l_arrPoints, a_drawPoly.m_objProps, ab_block);

            dxfPolyline.DefaultStartWidth = a_drawPoly.m_objProps.m_logpen.m_width;
            dxfPolyline.DefaultEndWidth = a_drawPoly.m_objProps.m_logpen.m_width;

            if (a_drawPoly.m_nShape == Shape.poly)
            {
                dxfPolyline.Closed = true;
            }

            DxfLineType l_lineType = GetLineTypeFromObjProps(a_drawPoly.m_objProps);
            dxfPolyline.LineType = l_lineType;
            //9 dxfPolyline.LineTypeSource = AttributeSource.This;

            //99dxfPolyline.ColorSource = AttributeSource.This;
            dxfPolyline.Color = Helper.MakeEntityColorByBlock(a_drawPoly.m_objProps.m_logpen.m_color, ab_block);

            dxfPolyline.Layer = ExportContext.Current.Layer;
            a_coll.Add(dxfPolyline);
        }

        private static void ExportPolygonFilled(DxfEntityCollection a_coll, Point2D[] a_points, ObjProps a_objProps, bool ab_block)
        {
            if (a_objProps.m_bBrush)
            {
                ExportPolygonSolid(a_coll, a_points, a_objProps, false, ab_block);
            }

            if (a_objProps.m_hatchtype != HatchType.NONE)
            {
                ExportPolygonSolid(a_coll, a_points, a_objProps, true, ab_block);
            }
        }


        private static void ExportPolygonSolid(DxfEntityCollection a_coll, Point2D[] a_points, ObjProps a_objProps, bool ab_hatch, bool ab_block)
        {
            if (ab_hatch && (a_objProps.m_hatchtype == HatchType.NONE))
            {
                return;
            }
            DxfHatch l_hatch = new DxfHatch();

            if (ab_hatch)
            {
                if (a_objProps.m_hatchtype != HatchType.NONE)
                {
                    DxfPattern pattern = GetHatchPattern(a_objProps);
                    if (pattern != null)
                    {
                        l_hatch.Pattern = pattern;
                    }
                }
                l_hatch.Color = EntityColor.CreateFrom((WW.Drawing.ArgbColor)a_objProps.m_logpen.m_color);
            }
            else
            {
                l_hatch.Color = EntityColor.CreateFrom((WW.Drawing.ArgbColor)a_objProps.m_logbrush.m_color); 
            }
 
            DxfHatch.BoundaryPath boundaryPath1 = new DxfHatch.BoundaryPath();
            boundaryPath1.Type = BoundaryPathType.Outermost;
            l_hatch.BoundaryPaths.Add(boundaryPath1);


            Point2D l_pointOld = new Point2D(a_points[0].X, a_points[0].Y);
            for (int li_i = 1; li_i < a_points.Length; li_i++)
            {
                Point2D l_pointNew = new Point2D(a_points[li_i].X, a_points[li_i].Y);
                boundaryPath1.Edges.Add(new DxfHatch.BoundaryPath.LineEdge(l_pointOld, l_pointNew));
                l_pointOld = l_pointNew;
            }

            boundaryPath1.Edges.Add(new DxfHatch.BoundaryPath.LineEdge(l_pointOld, new Point2D(a_points[0].X, a_points[0].Y)));

            l_hatch.Layer = ExportContext.Current.Layer;
            a_coll.Add(l_hatch);
        }


        private static DxfLineType GetLineTypeFromObjProps(ObjProps a_objProps)
        {
            if (a_objProps.m_lin.m_name == null)
            {
                return null;
            }

            string[] ls_lineSegments = a_objProps.m_lin.m_body.Split(new char[] { ',' });
            if (ls_lineSegments.Length < 3)
            {
                throw new Exception("linestyle too short");
            }
            if (ls_lineSegments[0] != "A")
            {
                throw new Exception("linestyle does not start with A");
            }

            int li_segmentsCount = ls_lineSegments.Length - 1;
            double[] ld_arraySegments = new double[li_segmentsCount];
            for (int i = 0; i < ld_arraySegments.Length; i++)
            {
                string ls_item = ls_lineSegments[i + 1];
                ld_arraySegments[i] = 100 * MyString2Double(ls_item);
            }

//            string ls_lineName = RemoveDiacritics(a_objProps.m_lin.m_name).Trim();
            string ls_lineName = a_objProps.m_lin.m_name;
            DxfLineType l_lineType = new DxfLineType(ls_lineName, ld_arraySegments);

            if (!ExportContext.Current.Model.LineTypes.Contains(ls_lineName))
            {
                ExportContext.Current.Model.LineTypes.Add(l_lineType);
            }

            return l_lineType;
        }




        private static double MyString2Double(string as_what)
        {
            string ls_corrected = as_what;
            if (as_what[0] == '.')
            {
                ls_corrected = "0" + as_what;
            }
            if (as_what[0] == '-')
            {
                ls_corrected = "-0" + as_what.Substring(1);
            }

            return Double.Parse(ls_corrected, System.Globalization.CultureInfo.InvariantCulture);
        }


        public static void ExportBezier(DxfEntityCollection a_coll, DrawPoly drawPoly, bool ab_block)
        {
            int li_arrSize = drawPoly.m_points.Count;
            Point2D[] l_arrPoints = new Point2D[li_arrSize];
            for (int i = 0; i < li_arrSize; i++)
            {
                int li_x = drawPoly.m_points[i].X;
                int li_y = drawPoly.m_points[i].Y;

                l_arrPoints[i].X = li_x;
                l_arrPoints[i].Y = REVERSE_Y * li_y;
            }

            DxfPolyline2DSpline dxfPolyline = new DxfPolyline2DSpline(SplineType.CubicBSpline, l_arrPoints);
            dxfPolyline.DefaultStartWidth = drawPoly.m_objProps.m_logpen.m_width;
            dxfPolyline.DefaultEndWidth = drawPoly.m_objProps.m_logpen.m_width;

            //99dxfPolyline.ColorSource = AttributeSource.This;
            dxfPolyline.Color = Helper.MakeEntityColorByBlock(drawPoly.m_objProps.m_logpen.m_color, ab_block);
            dxfPolyline.Closed = false;

            DxfLineType l_lineType = GetLineTypeFromObjProps(drawPoly.m_objProps);
            dxfPolyline.LineType = l_lineType;
            //9 dxfPolyline.LineTypeSource = AttributeSource.This;

            dxfPolyline.Layer = ExportContext.Current.Layer;
            a_coll.Add(dxfPolyline);
        }


        private enum ArrowType 
        {
            at_none = 0,
            at_sip1 = 5, 
            at_sip2, 
            at_sip3, 
            at_sip4, 
            at_sip5,
            at_sip6,
            at_sip7,
            at_sip8
        }


static void ExportSipkaWithoutStem(DxfEntityCollection a_coll, ArrowType a_typ, Point a_start, Point a_cil, System.Drawing.Color a_color, bool ab_drawStem, bool ab_is_ending, 
    double a_scaleX, double a_scaleY, int ai_thickness, bool ab_block, 
    double a_scale_arrow_x, double a_scale_arrow_y)
{
	int li_coef_x_12 = (int) ((12.0 * a_scaleX * a_scale_arrow_x) + 0.499);
	int li_coef_x_24 = (int) ((24.0 * a_scaleX * a_scale_arrow_x) + 0.499);
    int li_coef_x_7  = (int) (( 7.0 * a_scaleX * a_scale_arrow_x) + 0.499);

	int li_coef_y_5  = (int) ((5.0 * a_scaleY * a_scale_arrow_y) + 0.499);
    int li_coef_y_7  = (int) ((7.0 * a_scaleY * a_scale_arrow_y) + 0.499);

    int li_dist_80 = 80;
    int li_dist_60 = 60;

    int	li_coef = 2;
	int li_len = Helper.MyHypot(a_start.X - a_cil.X, a_start.Y - a_cil.Y);

	switch(a_typ){
	case ArrowType.at_sip1://velká
		if (ab_is_ending)
		{
            DrawPoly l_poly = new DrawPoly(Shape.polyline, ai_thickness, a_color);
            Point l_point1 = Helper.PrevodBodu(a_start, a_cil, new Point(a_start.X + li_len - li_coef_x_12 * li_coef, a_start.Y - li_coef_y_5 * li_coef));
            Point l_point2 = Helper.PrevodBodu(a_start, a_cil, new Point(a_start.X + li_len - li_coef_x_12 * li_coef, a_start.Y + li_coef_y_5 * li_coef));
            l_poly.AddPoint(l_point1);
            l_poly.AddPoint(a_cil);
            l_poly.AddPoint(l_point2);
            Helper.ExportPolylineAux(a_coll, l_poly, ab_block);
		}
		break;

	case ArrowType.at_sip6://velka duta uzavrena (proudy), added to version 3.0 on 9-MAR-2004
		{
			if (ab_is_ending)
			{

				Point pointMitte =  Helper.PrevodBodu(a_start, a_cil, new Point(a_start.X + li_len - 13 * li_coef, a_start.Y - 0 * li_coef));
				Point pointA =	    Helper.PrevodBodu(a_start, a_cil, new Point(a_start.X + li_len - li_coef_x_12 * li_coef, a_start.Y - li_coef_y_5 * li_coef));
				Point pointB =	    Helper.PrevodBodu(a_start, a_cil, new Point(a_start.X + li_len - li_coef_x_12 * li_coef, a_start.Y + li_coef_y_5 * li_coef));
	            
                DrawPoly l_poly2 = new DrawPoly(Shape.polyline, ai_thickness, a_color);
                l_poly2.AddPoint(a_cil);
                l_poly2.AddPoint(pointA);
                l_poly2.AddPoint(pointB);
                l_poly2.AddPoint(a_cil);
                Helper.ExportPolylineAux(a_coll, l_poly2, ab_block);
			}
		}
		break;
	case ArrowType.at_sip2://malá

		if (ab_is_ending)
		{
            DrawPoly l_poly = new DrawPoly(Shape.polyline, ai_thickness, a_color);
            l_poly.AddPoint(a_cil);
            l_poly.AddPoint(Helper.PrevodBodu(a_start, a_cil, new Point(a_start.X + li_len - 12 * li_coef, a_start.Y - 3 * li_coef)));
            l_poly.AddPoint(Helper.PrevodBodu(a_start, a_cil, new Point(a_start.X + li_len - 12 * li_coef, a_start.Y + 3 * li_coef)));
            l_poly.AddPoint(a_cil);
            Helper.ExportPolylineAux(a_coll, l_poly, ab_block);
		}
		break;
	case ArrowType.at_sip3://malá s ocasem
		if (ab_is_ending)
		{
            DrawPoly l_poly = new DrawPoly(Shape.polyline, ai_thickness, a_color);
            l_poly.AddPoint(Helper.PrevodBodu(a_start, a_cil, new Point(a_start.X + li_len - li_coef_x_12 * li_coef, a_start.Y - 3 * li_coef)));
            l_poly.AddPoint(a_cil);
            l_poly.AddPoint(Helper.PrevodBodu(a_start, a_cil, new Point(a_start.X + li_len - li_coef_x_12 * li_coef, a_start.Y + 3 * li_coef)));
            Helper.ExportPolylineAux(a_coll, l_poly, ab_block);
		}
		else
		{
            DrawPoly l_poly = new DrawPoly(Shape.polyline, ai_thickness, a_color);
            l_poly.AddPoint(Helper.PrevodBodu(a_start, a_cil, new Point(a_start.X + li_len + li_coef_x_24, a_start.Y - 3 * li_coef)));
            l_poly.AddPoint(Helper.PrevodBodu(a_start, a_cil, new Point(a_start.X + li_len, a_start.Y)));
            l_poly.AddPoint(Helper.PrevodBodu(a_start, a_cil, new Point(a_start.X + li_len + li_coef_x_24, a_start.Y + 3 * li_coef)));
            Helper.ExportPolylineAux(a_coll, l_poly, ab_block);
		}

		break;
	case ArrowType.at_sip4://velká oboustranná
        DrawPoly l_poly_sip4 = new DrawPoly(Shape.polyline, ai_thickness, a_color);
        l_poly_sip4.AddPoint(Helper.PrevodBodu(a_start, a_cil, new Point(a_start.X + li_len - 20 * li_coef, a_start.Y - li_coef_y_5 * li_coef)));
        l_poly_sip4.AddPoint(a_cil);
        l_poly_sip4.AddPoint(Helper.PrevodBodu(a_start, a_cil, new Point(a_start.X + li_len - 20 * li_coef, a_start.Y + li_coef_y_5 * li_coef)));
        Helper.ExportPolylineAux(a_coll, l_poly_sip4, ab_block);
		break;
	case ArrowType.at_sip5://malá oboustranná
        DrawPoly l_poly_sip5 = new DrawPoly(Shape.polyline, ai_thickness, a_color);
        l_poly_sip5.AddPoint(Helper.PrevodBodu(a_start, a_cil, new Point(a_start.X + li_len - li_coef_x_12 * li_coef, a_start.Y - li_coef_y_5 * li_coef)));
        l_poly_sip5.AddPoint(a_cil);
        l_poly_sip5.AddPoint(Helper.PrevodBodu(a_start, a_cil, new Point(a_start.X + li_len - li_coef_x_12 * li_coef, a_start.Y + li_coef_y_5 * li_coef)));
        Helper.ExportPolylineAux(a_coll, l_poly_sip5, ab_block);
		break;
    case ArrowType.at_sip7://mala s sipkami dovnitr na kotovani der zvenku added version 4.1
         DrawPoly l_poly_sip_7a = new DrawPoly(Shape.polyline, ai_thickness, a_color);
        l_poly_sip_7a.AddPoint(Helper.PrevodBodu(a_start, a_cil, new Point(a_start.X + li_len - (li_dist_80 * li_coef), a_start.Y)));
        l_poly_sip_7a.AddPoint(Helper.PrevodBodu(a_start, a_cil, new Point(a_start.X + li_len - (li_dist_60 * li_coef), a_start.Y - li_coef_y_5 * li_coef)));
        Helper.ExportPolylineAux(a_coll, l_poly_sip_7a, ab_block);

        DrawPoly l_poly_sip_7b = new DrawPoly(Shape.polyline, ai_thickness, a_color);
        l_poly_sip_7b.AddPoint(Helper.PrevodBodu(a_start, a_cil, new Point(a_start.X + li_len - (li_dist_80 * li_coef), a_start.Y)));
        l_poly_sip_7b.AddPoint(Helper.PrevodBodu(a_start, a_cil, new Point(a_start.X + li_len - (li_dist_60 * li_coef), a_start.Y + li_coef_y_5 * li_coef)));
        Helper.ExportPolylineAux(a_coll, l_poly_sip_7b, ab_block);
        break;
    case ArrowType.at_sip8://    /------------------/ stavební
        DrawPoly l_poly_sip8 = new DrawPoly(Shape.polyline, ai_thickness, a_color);
        l_poly_sip8.AddPoint(Helper.PrevodBodu(a_cil, a_start, new Point(a_cil.X + li_coef_x_7 * li_coef, a_cil.Y - li_coef_y_7 * li_coef)));
        l_poly_sip8.AddPoint(Helper.PrevodBodu(a_cil, a_start, new Point(a_cil.X - li_coef_x_7 * li_coef, a_cil.Y + li_coef_y_7 * li_coef)));
        Helper.ExportPolylineAux(a_coll, l_poly_sip8, ab_block);
        break;

	}	
}

        public static string RemoveDiacritics(string as_input)
        {
            as_input = as_input.Normalize(NormalizationForm.FormD);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < as_input.Length; i++)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(as_input[i]) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(as_input[i]); 
                }
            }

            return sb.ToString();
        }

        private static void ExportTitleBlocks(WW.Cad.Model.Tables.DxfBlockCollection dxfBlockCollection, PtbDoc a_ptb, HybridDictionary a_dict)
        {
            string ls_name = a_ptb.Path;
            ls_name = Sanitize(ls_name);

            if (ls_name.Length == 0)
            {
                return;
            }


            // 2011-08-10 changing name to lG because it must be unique
            //DxfBlock l_block = new DxfBlock(ls_name);
            DxfBlock l_block = new DxfBlock(ls_name);


            if (l_block == null)
            {
                throw new Exception("l_block is null");
            }
            if (dxfBlockCollection.Contains(ls_name))
            {
                return;
            }
            //export images
            //HybridDictionary l_dictRepo = new HybridDictionary();
            foreach (QImageDesc l_imgDesc in a_ptb.m_repo.m_listImgDesc)
            {
                ExportQImageDesc(ExportContext.Current.Model.Images, l_imgDesc, a_dict);
            }


            foreach (PpdDoc l_ppdDoc in a_ptb.m_repo.m_listPpd)
            {
                ExportBlockPpd(ExportContext.Current.Model.Blocks, l_ppdDoc, a_dict);
            }

            dxfBlockCollection.Add(l_block);
            ExportDrawDoc(l_block.Entities, a_ptb, a_dict, true);
            a_dict[ls_name] = l_block;
            
       
        }

        private static double Angle2Radians(int ai_angle_tenth_of_degree)
        {
            return (double)(Math.PI * ai_angle_tenth_of_degree / 1800);
        }

        private static void Export_Straight_Wire_Label(DxfEntityCollection a_coll, DxfNet.Wire a_wire, EFont a_efont, SettingsNumberingWire pSettings)
        {
            string ls_name = a_wire.GetName();
            if(string.IsNullOrWhiteSpace(ls_name))
            {
                return;
            }

            AttachmentPoint l_attachment_point = AttachmentPoint.MiddleCenter;
            Vector3D l_axis = TurnsToVector3D(0);


            Point l_point_temp = a_wire.GetLineCenterPoint();
            Point3D l_center_point = new Point3D(l_point_temp.X, l_point_temp.Y, 0);

            //adjust the center point depending on the orientation of the wire
            UtilsMath.cardinal_directions l_cd = UtilsMath.GetDirection(a_wire.m_points[0], a_wire.m_points[1]);
            if (l_cd == UtilsMath.cardinal_directions.cd_none)
            {
                return;
            }



            if ((l_cd == UtilsMath.cardinal_directions.cd_west) || (l_cd == UtilsMath.cardinal_directions.cd_east))
            {
                l_center_point.Y -= pSettings.WireLabelDist_A;
                l_attachment_point = AttachmentPoint.BottomCenter;
            }
            else if ((l_cd == UtilsMath.cardinal_directions.cd_north) || (l_cd == UtilsMath.cardinal_directions.cd_south))
            {
                l_center_point.X -= pSettings.WireLabelDist_A;

                if (pSettings.Vertically)
                {
                    l_axis = TurnsToVector3D(2);
                    l_attachment_point = AttachmentPoint.BottomCenter;
                }
                else
                {
                    l_attachment_point = AttachmentPoint.MiddleRight;
                }
            }

            if(a_wire.m_label_mid != null)
            {
                l_center_point = Helper.Point_To_Point3D(a_wire.m_label_mid.m_point);
                l_attachment_point = AttachmentPoint.MiddleCenter;
            }

            Point3D l_anchor = new Point3D(l_center_point.X, l_center_point.Y, 0);
            l_anchor.Y *= REVERSE_Y;
            int li_height = GetFontAscentSize(a_efont);

            DxfMText dxfText = new DxfMText(ls_name, l_anchor, li_height);
            dxfText.Color = Helper.MakeEntityColorByBlock(a_efont.m_color, false);

            dxfText.AttachmentPoint = l_attachment_point;
            dxfText.XAxis = l_axis;

            a_coll.Add(dxfText);
        }

        private static void Flip_Point_Y(ref Point a_point, int ai_ref)
        {
            int li_dif = a_point.Y - ai_ref;
            a_point.Y = ai_ref - li_dif;
        }

        private static void ExportDimLine(DxfEntityCollection a_coll, QDimLine a_dim, bool ab_block)
        {
            Point3D A_3D = new Point3D(a_dim.A.X, a_dim.A.Y * REVERSE_Y, 0);
            Point3D B_3D = new Point3D(a_dim.B.X, a_dim.B.Y * REVERSE_Y, 0);
            Point3D C_3D = new Point3D(a_dim.C.X, a_dim.C.Y * REVERSE_Y, 0);

            

            if (a_dim.m_dir == QDimLine.DimDirection.dimdir_aligned)
            {
                DxfDimension.Aligned dxfDimAl = new DxfDimension.Aligned(ExportContext.Current.Model.CurrentDimensionStyle);
                dxfDimAl.ExtensionLine1StartPoint = A_3D;
                dxfDimAl.ExtensionLine2StartPoint = B_3D;
                dxfDimAl.DimensionLineLocation = C_3D;

                dxfDimAl.Layer = ExportContext.Current.Layer;
                a_coll.Add(dxfDimAl);
            }
            else
            {
                DxfDimension.Linear dxfDimLin = new DxfDimension.Linear(ExportContext.Current.Model.CurrentDimensionStyle);
                dxfDimLin.ExtensionLine1StartPoint = A_3D;
                dxfDimLin.ExtensionLine2StartPoint = B_3D;
                dxfDimLin.DimensionLineLocation = C_3D;

                if (a_dim.m_dir == QDimLine.DimDirection.dimdir_ver)
                {
                    dxfDimLin.Rotation = Math.PI / 2d;
                }

                dxfDimLin.Layer = ExportContext.Current.Layer;
                a_coll.Add(dxfDimLin);
            }

            
        }


        private static void ExportDimCircle(DxfEntityCollection a_coll, QDimCircle a_dim, bool ab_block)
        {
            Point3D A_3D = new Point3D(a_dim.A.X, a_dim.A.Y * REVERSE_Y, 0);
            Point3D B_3D = new Point3D(a_dim.B.X, a_dim.B.Y * REVERSE_Y, 0);
            Point3D l_pos_label = new Point3D(a_dim.m_pos_label.X, a_dim.m_pos_label.Y, 0);


            if((l_pos_label.X == 0) && (l_pos_label.Y == 0))
            {
                l_pos_label.X = (A_3D.X + B_3D.X) / 2;
                l_pos_label.Y = (A_3D.Y + B_3D.Y) / 2;
            }


            DxfDimension.Diametric dxfDim = new DxfDimension.Diametric(ExportContext.Current.Model.CurrentDimensionStyle)
            {
                ArcLineIntersectionPoint1 = A_3D,
                ArcLineIntersectionPoint2 = B_3D,

                TextMiddlePoint = l_pos_label,
                UseTextMiddlePoint = true
            };

            dxfDim.Layer = ExportContext.Current.Layer;
            a_coll.Add(dxfDim);
        }


        private static void Setup_DimStyle(DxfModel model, Core.QDimStyle a_dim_style)
        {
            if(a_dim_style == null)
            {
                return;
            }

            DxfDimensionStyle l_style = model.CurrentDimensionStyle;
            l_style.ArrowSize = 20;
            l_style.TextHeight = 20;

            //make text of aligned dims aligned
            l_style.TextInsideHorizontal  = !a_dim_style.m_align_text_with_dim_line;
            l_style.TextOutsideHorizontal = !a_dim_style.m_align_text_with_dim_line;


            l_style.DimensionLineColor = a_dim_style.m_line_dim.m_color;
            l_style.ExtensionLineColor = a_dim_style.m_line_ext.m_color;


            switch(a_dim_style.m_text_position)
            {
                case Core.QDimStyle.Text_Position.text_position_above:
                    l_style.TextVerticalAlignment = DimensionTextVerticalAlignment.Above;
                    break;

                case Core.QDimStyle.Text_Position.text_position_below:
                    l_style.TextVerticalAlignment = DimensionTextVerticalAlignment.Below;
                    break;
            }

            l_style.Name = a_dim_style.m_name;

            if(string.IsNullOrEmpty(l_style.Name))
            {
                l_style.Name = "noname";
            }
        }

        //----------------------------------------
    }
}
