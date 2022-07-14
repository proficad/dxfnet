using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WW.Math;
using WW.Cad.Base;
using WW.Cad.IO;
using WW.Cad.Model;
using WW.Cad.Model.Objects;
using WW.Cad.Model.Entities;
using WW.Cad.Model.Tables;
using WW.Cad.Drawing;
using WW.Math.Geometry;
using WW.Drawing;


namespace Dxf2ProfiCAD
{
    
    ///// <summary>
    ///// Specialized wireframe graphics factory which converts path data to 2D polylines.
    ///// </summary>
    ///// <remarks>
    ///// Texts, points, rays, xlines and images are ignored.
    ///// </remarks>
    //public class PolygonWireframeGraphicsFactory : IWireframeGraphicsFactory
    //{
    //    private readonly List<Polyline2D> collectedPolylines = new List<Polyline2D>();

    //    /// <summary>
    //    /// Convert the relevant part of a model to polylines.
    //    /// </summary>
    //    /// <reamrks>
    //    /// The polylines are sorted in drawing order.
    //    /// </reamrks>
    //    /// <param name="model">The model.</param>
    //    /// <param name="graphicsConfig">Graphics configuration, useful for tuning accuracy</param>
    //    /// <returns>List of polylines.</returns>
    //    public static List<Polyline2D> ConvertModelToPolylines(DxfModel model, GraphicsConfig graphicsConfig)
    //    {
    //        DrawContext.Wireframe wireframe = new DrawContext.Wireframe.ModelSpace(model, graphicsConfig, Matrix4D.Identity);
    //        PolygonWireframeGraphicsFactory factory = new PolygonWireframeGraphicsFactory();

    //        model.Draw(wireframe, factory);

    //        return factory.collectedPolylines;
    //    }

    //    #region IWireframeGraphicsFactory Implementation

    //    public void BeginEntity(DxfEntity entity, DrawContext.Wireframe drawContext)
    //    {
    //        // ignored
    //    }

    //    public void EndEntity()
    //    {
    //        // ignored
    //    }

    //    public void BeginInsert(DxfInsert insert)
    //    {
    //        // ignore
    //    }

    //    public void EndInsert()
    //    {
    //        // ignore
    //    }

    //    public void CreateDot(DxfEntity entity, DrawContext.Wireframe drawContext, ArgbColor color, bool forText,
    //                          Vector4D position)
    //    {
    //        // ignore point
    //    }

    //    public void CreateLine(DxfEntity entity, DrawContext.Wireframe drawContext, ArgbColor color, bool forText,
    //                           Vector4D start, Vector4D end)
    //    {
    //        collectedPolylines.Add(new Polyline2D(start.ToPoint2D(), end.ToPoint2D()));
    //    }

    //    public void CreateRay(DxfEntity entity, DrawContext.Wireframe drawContext, ArgbColor color, Segment4D segment)
    //    {
    //        // ignore ray 
    //    }

    //    public void CreateXLine(DxfEntity entity, DrawContext.Wireframe drawContext, ArgbColor color,
    //                            Vector4D? startPoint, Segment4D segment)
    //    {
    //        // ignore xline 
    //    }

    //    public void CreatePath(DxfEntity entity, DrawContext.Wireframe drawContext, ArgbColor color, bool forText,
    //                           IList<Polyline4D> polylines, bool fill)
    //    {
    //        /*
    //        if (forText)
    //        {
    //            // should not happen, see SupportsText below
    //            return;
    //        }
    //        */

    //        short lineWeight = entity.GetLineWeight(drawContext);
    //        Console.WriteLine("lineWidth={0}", lineWeight);

    //        // maybe you want to ignore filled paths, too
    //        foreach (Polyline4D poly4d in polylines)
    //        {
    //            Polyline2D poly2d = new Polyline2D(poly4d.Count, poly4d.Closed);
    //            foreach (Vector4D v4d in poly4d)
    //            {
    //                poly2d.Add(v4d.ToPoint2D());
    //            }
    //            collectedPolylines.Add(poly2d);
    //        }
    //    }

    //    public void CreatePathAsOne(DxfEntity entity, DrawContext.Wireframe drawContext, ArgbColor color, bool forText,
    //                                IList<Polyline4D> polylines, bool fill)
    //    {
    //        // maybe you want to ignore these, too
    //        CreatePath(entity, drawContext, color, forText, polylines, fill);
    //    }

    //    public void CreateShape(DxfEntity entity, DrawContext.Wireframe drawContext, ArgbColor color, bool forText,
    //                            IShape4D shape)
    //    {
    //        CreatePathAsOne(entity, drawContext, color, forText, shape.ToPolylines4D(), shape.IsFilled);
    //    }

    //    public bool SupportsText
    //    {
    //        get { return true; } // by returning true here only the text specific methods below will be use
    //    }

    //    public void CreateText(DxfText text, DrawContext.Wireframe drawContext, ArgbColor color)
    //    {
    //        // ignore text
    //        string ls_what = text.Text;
    //    }

    //    public void CreateMText(DxfMText text, DrawContext.Wireframe drawContext)
    //    {
    //        // ignore text
    //    }

    //    public void CreateImage(DxfRasterImage rasterImage, DrawContext.Wireframe drawContext, Polyline4D clipPolygon,
    //                            Polyline4D imageBoundary,
    //                            Vector4D transformedOrigin, Vector4D transformedXAxis, Vector4D transformedYAxis)
    //    {
    //        // ignore images
    //    }

    //    #endregion
    //}
}
