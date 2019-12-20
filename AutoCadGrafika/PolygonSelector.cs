using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using System.ComponentModel;
using System.Windows.Forms;

namespace AutoCadGrafika
{
    public  class Commands
    {
        [CommandMethod("SelectObjectsByPolygon")]
        public  void SelectObjectsByCrossingWindow()
        {
            try
            {
                // Get the current document editor
                Editor acDocEd = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
                var acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                Database acCurDb = acDoc.Database;

                // Selektuj poligon za izdvajanje planova
                PromptSelectionResult acSSPrompt = acDoc.Editor.GetSelection();

                // If the prompt status is OK, objects were selected
                if (acSSPrompt.Status == PromptStatus.OK)
                {
                    SelectionSet acSSet = acSSPrompt.Value;
                    if (acSSet.Count > 1)
                        Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("Selektovano je vise objekata. Selektujte samo jedan! ");
                    else
                    {
                        // Vrati skup tacaka za poligon
                        var selPolygon = acSSet[0];
                        if (selPolygon != null)
                        {
                            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                            {
                                BlockTableRecord btr = (BlockTableRecord)acTrans.GetObject(
                                    acCurDb.CurrentSpaceId, OpenMode.ForRead);

                                Entity acEnt = acTrans.GetObject(selPolygon.ObjectId,
                                                            OpenMode.ForRead) as Entity;
                                if (acEnt != null)
                                {

                                    Point3dCollection entPts = CollectPoints(acTrans, acEnt);
                                    if (entPts != null && entPts.Count > 0)
                                    {
                                        // Selektuj planove na osnovu poligona
                                        var selPlanovi = acDoc.Editor.SelectCrossingPolygon(entPts);
                                        string karts = "";

                                        foreach (SelectedObject plan in selPlanovi.Value)
                                        {
                                            // procitaj atribut KARTBROJ i dodaj ga u rezultat
                                            Entity planEnt = acTrans.GetObject(plan.ObjectId,
                                                            OpenMode.ForRead) as Entity;
                                            object acadObj = planEnt.AcadObject;
                                            var props = TypeDescriptor.GetProperties(acadObj);
                                            foreach (PropertyDescriptor prop in props)
                                            {
                                                if (prop.DisplayName.ToLower().Contains("kart"))
                                                {
                                                    object value = prop.GetValue(acadObj);
                                                    if (value != null)
                                                        karts += value.ToString() + ",";
                                                }
                                            }
                                        }
                                        if (karts.EndsWith(","))
                                            karts = karts.Substring(0, karts.Length - 1);
                                        if (karts.Length > 0)
                                        {
                                            Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("Izdvojeni su sledeci katografski brojevi\n"
                                                + karts + "\nKartografski brojevi su spremni za koriscenje u aplikaciji");

                                            Clipboard.SetText(karts);

                                        }
                                        else
                                            Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("Nema ni jednog kartografskog broja. Pokusajte ponovo");

                                    }
                                }

                                acTrans.Commit();
                            }
                        }
                    }
                }
                else
                {
                    Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("Nista nije elektovano. Molim selektujte poligon za pretragu!");
                }
            }
            catch (System.Exception ex)
            {
                string poruka = ex.Message;
                if (ex.InnerException != null)
                    poruka += " " + ex.InnerException.Message;

                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog(poruka);
            }
        
        }

        private Point3dCollection CollectPoints(Transaction tr, Entity ent)

        {

            // The collection of points to populate and return



            Point3dCollection pts = new Point3dCollection();



            // We'll start by checking a block reference for

            // attributes, getting their bounds and adding

            // them to the point list. We'll still explode

            // the BlockReference later, to gather points

            // from other geometry, it's just that approach

            // doesn't work for attributes (we only get the

            // AttributeDefinitions, which don't have bounds)



            BlockReference br = ent as BlockReference;

            if (br != null)

            {

                foreach (ObjectId arId in br.AttributeCollection)

                {

                    DBObject obj = tr.GetObject(arId, OpenMode.ForRead);

                    if (obj is AttributeReference)

                    {

                        AttributeReference ar = (AttributeReference)obj;

                        ExtractBounds(ar, pts);

                    }

                }

            }



            // If we have a curve - other than a polyline, which

            // we will want to explode - we'll get points along

            // its length



            Curve cur = ent as Curve;

            if (cur != null &&

                !(cur is Polyline ||

                  cur is Polyline2d ||

                  cur is Polyline3d))

            {

                // Two points are enough for a line, we'll go with

                // a higher number for other curves



                int segs = (ent is Line ? 2 : 20);



                double param = cur.EndParam - cur.StartParam;

                for (int i = 0; i < segs; i++)

                {

                    try

                    {

                        Point3d pt =

                          cur.GetPointAtParameter(

                            cur.StartParam + (i * param / (segs - 1))

                          );

                        pts.Add(pt);

                    }

                    catch { }

                }

            }

            else if (ent is DBPoint)

            {

                // Points are easy



                pts.Add(((DBPoint)ent).Position);

            }

            else if (ent is DBText)

            {

                // For DBText we use the same approach as

                // for AttributeReferences



                ExtractBounds((DBText)ent, pts);

            }

            else if (ent is MText)

            {

                // MText is also easy - you get all four corners

                // returned by a function. That said, the points

                // are of the MText's box, so may well be different

                // from the bounds of the actual contents



                MText txt = (MText)ent;

                Point3dCollection pts2 = txt.GetBoundingPoints();

                foreach (Point3d pt in pts2)

                {

                    pts.Add(pt);

                }

            }

            else if (ent is Face)

            {

                Face f = (Face)ent;

                try

                {

                    for (short i = 0; i < 4; i++)

                    {

                        pts.Add(f.GetVertexAt(i));

                    }

                }

                catch { }

            }

            else if (ent is Solid)

            {

                Solid sol = (Solid)ent;

                try

                {

                    for (short i = 0; i < 4; i++)

                    {

                        pts.Add(sol.GetPointAt(i));

                    }

                }

                catch { }

            }

            else

            {

                // Here's where we attempt to explode other types

                // of object



                DBObjectCollection oc = new DBObjectCollection();

                try

                {

                    ent.Explode(oc);

                    if (oc.Count > 0)

                    {

                        foreach (DBObject obj in oc)

                        {

                            Entity ent2 = obj as Entity;

                            if (ent2 != null && ent2.Visible)

                            {

                                foreach (Point3d pt in CollectPoints(tr, ent2))

                                {

                                    pts.Add(pt);

                                }

                            }

                            obj.Dispose();

                        }

                    }

                }

                catch { }

            }

            return pts;

        }



        private void ExtractBounds(DBText txt, Point3dCollection pts)

        {

            // We have a special approach for DBText and

            // AttributeReference objects, as we want to get

            // all four corners of the bounding box, even

            // when the text or the containing block reference

            // is rotated



            if (txt.Bounds.HasValue && txt.Visible)

            {

                // Create a straight version of the text object

                // and copy across all the relevant properties

                // (stopped copying AlignmentPoint, as it would

                // sometimes cause an eNotApplicable error)



                // We'll create the text at the WCS origin

                // with no rotation, so it's easier to use its

                // extents



                DBText txt2 = new DBText();

                txt2.Normal = Vector3d.ZAxis;

                txt2.Position = Point3d.Origin;



                // Other properties are copied from the original



                txt2.TextString = txt.TextString;

                txt2.TextStyleId = txt.TextStyleId;

                txt2.LineWeight = txt.LineWeight;

                txt2.Thickness = txt2.Thickness;

                txt2.HorizontalMode = txt.HorizontalMode;

                txt2.VerticalMode = txt.VerticalMode;

                txt2.WidthFactor = txt.WidthFactor;

                txt2.Height = txt.Height;

                txt2.IsMirroredInX = txt2.IsMirroredInX;

                txt2.IsMirroredInY = txt2.IsMirroredInY;

                txt2.Oblique = txt.Oblique;



                // Get its bounds if it has them defined

                // (which it should, as the original did)



                if (txt2.Bounds.HasValue)

                {

                    Point3d maxPt = txt2.Bounds.Value.MaxPoint;



                    // Place all four corners of the bounding box

                    // in an array



                    Point2d[] bounds =

                      new Point2d[] {

              Point2d.Origin,

              new Point2d(0.0, maxPt.Y),

              new Point2d(maxPt.X, maxPt.Y),

              new Point2d(maxPt.X, 0.0)

                      };



                    // We're going to get each point's WCS coordinates

                    // using the plane the text is on



                    Plane pl = new Plane(txt.Position, txt.Normal);



                    // Rotate each point and add its WCS location to the

                    // collection



                    foreach (Point2d pt in bounds)

                    {

                        pts.Add(

                          pl.EvaluatePoint(

                            pt.RotateBy(txt.Rotation, Point2d.Origin)

                          )

                        );

                    }

                }

            }

        }

    }
}
