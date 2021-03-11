using System;
using System.Text;
using System.Linq;
using System.Xml;
using System.Reflection;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
//using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.GraphicsInterface;



using MgdAcApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using MgdAcDocument = Autodesk.AutoCAD.ApplicationServices.Document;
using AcWindowsNS = Autodesk.AutoCAD.Windows;

namespace AutoCadGrafika
{
    public class PointCollector2:IDisposable
    {
        #region Types/Consts/Members

        public enum Shape
        {
            Window,
            Box,
            RegularPolygon,
            Fence,
            Polygon,
        }

        private const int constWindowColorIndex = 172;
        private const int constCrossingWindowColorIndex = 110;
        private const byte constWindowTransparency = 50;
        private const short constBoundaryColorIndex = 7;

        private int mSegmentCount = 72;

        private Shape mShape;
        private Point3d m1stPoint;
        private double mDist;

        private Autodesk.AutoCAD.DatabaseServices.Hatch mTempHatch;
        private short mHatchColorIndex = constCrossingWindowColorIndex;
        private byte mHatchTransparency = constWindowTransparency;
        private TransientDrawingMode mHatchDrawMode;

        private Autodesk.AutoCAD.DatabaseServices.Polyline mTempPline;
        private TransientDrawingMode mBoundaryDrawMode;

        #endregion

        #region Constructors

        public PointCollector2(Shape shape, bool isCrossing)
        {
            mShape = shape;
            IsCrossing = isCrossing;

            mTempPline = new Autodesk.AutoCAD.DatabaseServices.Polyline();
            mTempPline.SetDatabaseDefaults();
            mTempPline.ColorIndex = constBoundaryColorIndex;

            mTempHatch = new Hatch();
            mTempHatch.SetDatabaseDefaults();
            mTempHatch.ColorIndex = mHatchColorIndex;
            mTempHatch.SetHatchPattern(HatchPatternType.PreDefined, "SOLID");
            mTempHatch.Transparency = new Autodesk.AutoCAD.Colors.Transparency(mHatchTransparency);

            mHatchDrawMode = TransientDrawingMode.Main;
            if (IsCrossing)
            {
                mHatchColorIndex = constCrossingWindowColorIndex;
                mBoundaryDrawMode = TransientDrawingMode.Highlight;
            }
            else
            {
                mHatchColorIndex = constWindowColorIndex;
                mBoundaryDrawMode = TransientDrawingMode.DirectTopmost;
            }

            CollectedPoints = new Point3dCollection();
            CollectionStatus = PromptStatus.OK;
        }

        #endregion

        #region Properties

        public Point3dCollection CollectedPoints { get; private set; }
        public PromptStatus CollectionStatus { get; private set; }
        public bool IsCrossing { get; private set; }

        private Editor CurEditor
        {
            get
            {
                return MgdAcApplication.DocumentManager.MdiActiveDocument.Editor;
            }
        }

        #endregion

        #region Point Monitor

        private void Editor_PointMonitor(object sender, PointMonitorEventArgs e)
        {
            CleanupGraphics();

            mTempPline = new Autodesk.AutoCAD.DatabaseServices.Polyline();
            mTempPline.SetDatabaseDefaults();
            mTempPline.ColorIndex = constBoundaryColorIndex;

            if (mShape != Shape.Fence)
            {
                mTempHatch = new Hatch();
                mTempHatch.SetDatabaseDefaults();
                mTempHatch.ColorIndex = mHatchColorIndex;
                mTempHatch.SetHatchPattern(HatchPatternType.PreDefined, "SOLID");
                mTempHatch.Transparency = new Autodesk.AutoCAD.Colors.Transparency(mHatchTransparency);
            }

            Point3d compPt = e.Context.ComputedPoint.TransformBy(CurEditor.CurrentUserCoordinateSystem.Inverse());

            if (mShape == Shape.Window)
            {
                BuildupWindowVertices(m1stPoint, compPt);
            }
            else if (mShape == Shape.Box)
            {
                BuildupWindowVertices(m1stPoint, compPt);
                IsCrossing = compPt.X <= m1stPoint.X;
            }
            else if (mShape == Shape.RegularPolygon)
            {
                BuildupRegularPolygonVertices(compPt);
            }
            else if (mShape == Shape.Fence)
            {
                BuildupFenceVertices(compPt);
            }
            else if (mShape == Shape.Polygon)
            {
                BuildupPolygonVertices(compPt);
            }

            mTempPline.TransformBy(CurEditor.CurrentUserCoordinateSystem);

            if (mShape != Shape.Fence)
            {
                mTempHatch.Normal = mTempPline.Normal;
                HatchLoop loop = new HatchLoop(HatchLoopTypes.Default);
                for (int i = 0; i < mTempPline.NumberOfVertices; i++)
                {
                    loop.Curves.Add(mTempPline.GetLineSegment2dAt(i));
                }
                mTempHatch.AppendLoop(loop);
                mTempHatch.EvaluateHatch(true);
                TransientManager.CurrentTransientManager.AddTransient(mTempHatch, mHatchDrawMode, 0, new IntegerCollection());
            }

            if (IsCrossing)
            {
                mHatchColorIndex = constCrossingWindowColorIndex;
                mBoundaryDrawMode = TransientDrawingMode.Highlight;
            }
            else
            {
                mHatchColorIndex = constWindowColorIndex;
                mBoundaryDrawMode = TransientDrawingMode.DirectTopmost;
            }

            TransientManager.CurrentTransientManager.AddTransient(mTempPline, mBoundaryDrawMode, 0, new IntegerCollection());
        }

        #endregion

        #region Point Collectors & Shape Builders

        #region Window & Box

        private void CollectWindowPoints()
        {
            PromptPointResult prPntRes1 = CurEditor.GetPoint("\nSpecify first corner");
            if (prPntRes1.Status != PromptStatus.OK)
            {
                CollectionStatus = prPntRes1.Status;
                return;
            }

            m1stPoint = prPntRes1.Value;
            CollectedPoints.Add(m1stPoint);
            BuildupWindowVertices(m1stPoint, m1stPoint);

            CurEditor.PointMonitor += Editor_PointMonitor;

            PromptPointResult prPntRes2 = CurEditor.GetPoint("\nSpecify opposite corner");
            if (prPntRes2.Status != PromptStatus.OK)
            {
                CollectionStatus = prPntRes2.Status;
                CurEditor.PointMonitor -= Editor_PointMonitor;
                return;
            }

            CollectedPoints.Add(prPntRes2.Value);

            CurEditor.PointMonitor -= Editor_PointMonitor;
        }

        private void BuildupWindowVertices(Point3d corner1, Point3d corner2)
        {
            mTempPline.Closed = true;

            mTempPline.AddVertexAt(mTempPline.NumberOfVertices, new Point2d(corner1.X, corner1.Y), 0, 1, 1);
            mTempPline.AddVertexAt(mTempPline.NumberOfVertices, new Point2d(corner2.X, corner1.Y), 0, 1, 1);
            mTempPline.AddVertexAt(mTempPline.NumberOfVertices, new Point2d(corner2.X, corner2.Y), 0, 1, 1);
            mTempPline.AddVertexAt(mTempPline.NumberOfVertices, new Point2d(corner1.X, corner2.Y), 0, 1, 1);
        }

        #endregion

        #region Regular Polygon & Circle

        private void CollectRegularPolygonPoints()
        {
            PromptPointResult prPntRes1;
            PromptPointOptions prPntOpt = new PromptPointOptions("\nCenter");

            prPntRes1 = CurEditor.GetPoint(prPntOpt);
            if (prPntRes1.Status != PromptStatus.OK)
            {
                CollectionStatus = prPntRes1.Status;
                return;
            }

            m1stPoint = prPntRes1.Value;

            CurEditor.PointMonitor += Editor_PointMonitor;

            PromptDistanceOptions prPntOpt2 = new PromptDistanceOptions("\nRadius");
            prPntOpt2.AllowArbitraryInput = true;
            prPntOpt2.AllowNegative = false;
            prPntOpt2.AllowNone = true;
            prPntOpt2.AllowZero = false;
            prPntOpt2.BasePoint = m1stPoint;
            prPntOpt2.DefaultValue = 10.0;
            prPntOpt2.Only2d = true;
            prPntOpt2.UseBasePoint = true;
            prPntOpt2.UseDashedLine = true;
            prPntOpt2.UseDefaultValue = true;

            PromptDoubleResult prPntRes2 = CurEditor.GetDistance(prPntOpt2);
            if (prPntRes2.Status != PromptStatus.OK)
            {
                CollectionStatus = prPntRes2.Status;
                return;
            }

            CurEditor.PointMonitor -= Editor_PointMonitor;
        }

        private int GetRegularPolygonSideCount()
        {
            PromptIntegerResult prIntRes;
            PromptIntegerOptions prIntOpt = new PromptIntegerOptions("\nRegular polygon side");
            prIntOpt.AllowNone = true;
            prIntOpt.AllowArbitraryInput = true;
            prIntOpt.AllowNegative = false;
            prIntOpt.AllowZero = false;
            prIntOpt.DefaultValue = 5;
            prIntOpt.LowerLimit = 3;
            prIntOpt.UpperLimit = 36;
            prIntOpt.UseDefaultValue = true;

            prIntRes = CurEditor.GetInteger(prIntOpt);
            if (prIntRes.Status != PromptStatus.OK)
            {
                CollectionStatus = prIntRes.Status;
                return -1;
            }

            return mSegmentCount = prIntRes.Value;
        }

        private void BuildupRegularPolygonVertices(Point3d tempPt)
        {
            mTempPline.Closed = true;

            mDist = m1stPoint.DistanceTo(tempPt);
            double angle = m1stPoint.GetVectorTo(tempPt).AngleOnPlane(new Plane(Point3d.Origin, Vector3d.ZAxis));
            CollectedPoints.Clear();
            for (int i = 0; i < mSegmentCount; i++)
            {
                Point3d pt = m1stPoint.Add(new Vector3d(mDist * (Math.Cos(angle + Math.PI * 2 * i / mSegmentCount)),
                                                        mDist * (Math.Sin(angle + Math.PI * 2 * i / mSegmentCount)),
                                                        m1stPoint.Z));
                CollectedPoints.Add(pt);
                mTempPline.AddVertexAt(mTempPline.NumberOfVertices, new Point2d(pt.X, pt.Y), 0, 1, 1);
            }
        }

        #endregion

        #region Fence

        private void CollectFencePoints()
        {
            PromptPointOptions prPntOpt = new PromptPointOptions("\nFence point (Enter to finish)");
            prPntOpt.AllowNone = true;

            CurEditor.PointMonitor += Editor_PointMonitor;
            PromptPointResult prPntRes1;
            do
            {
                prPntRes1 = CurEditor.GetPoint(prPntOpt);
                if (prPntRes1.Status == PromptStatus.OK)
                    CollectedPoints.Add(prPntRes1.Value);
            } while (prPntRes1.Status == PromptStatus.OK);
            CurEditor.PointMonitor -= Editor_PointMonitor;
        }

        private void BuildupFenceVertices(Point3d tempPt)
        {
            mTempPline.Closed = false;

            foreach (Point3d pt in CollectedPoints)
            {
                mTempPline.AddVertexAt(mTempPline.NumberOfVertices, new Point2d(pt.X, pt.Y), 0, 1, 1);
            }
            mTempPline.AddVertexAt(mTempPline.NumberOfVertices, new Point2d(tempPt.X, tempPt.Y), 0, 1, 1);
        }

        #endregion

        #region Irregular Polygon

        private void CollectPolygonPoints()
        {
            PromptPointResult prPntRes1;
            PromptPointOptions prPntOpt = new PromptPointOptions("\nPolygon point: (Enter to finish)");
            prPntOpt.AllowNone = true;

            CurEditor.PointMonitor += Editor_PointMonitor;
            do
            {
                prPntRes1 = CurEditor.GetPoint(prPntOpt);
                if (prPntRes1.Status == PromptStatus.OK)
                {
                    m1stPoint = prPntRes1.Value;
                    CollectedPoints.Add(m1stPoint);
                }
                else if (prPntRes1.Status != PromptStatus.None)
                {
                    CollectionStatus = prPntRes1.Status;
                    CurEditor.PointMonitor -= Editor_PointMonitor;
                    return;
                }
            } while (prPntRes1.Status == PromptStatus.OK);
            CurEditor.PointMonitor -= Editor_PointMonitor;
        }

        private void BuildupPolygonVertices(Point3d tempPt)
        {
            mTempPline.Closed = true;

            foreach (Point3d pt in CollectedPoints)
            {
                mTempPline.AddVertexAt(mTempPline.NumberOfVertices, new Point2d(pt.X, pt.Y), 0, 1, 1);
            }
            mTempPline.AddVertexAt(mTempPline.NumberOfVertices, new Point2d(tempPt.X, tempPt.Y), 0, 1, 1);
        }

        #endregion

        #endregion

        #region Cleanup

        public void Dispose()
        {
            CurEditor.PointMonitor -= Editor_PointMonitor;

            CleanupGraphics();
            CollectedPoints.Dispose();
        }

        private void CleanupGraphics()
        {
            if (mTempPline != null && !mTempPline.IsDisposed)
            {
                TransientManager.CurrentTransientManager.EraseTransient(mTempPline, new IntegerCollection());
                mTempPline.Dispose();
                mTempPline = null;
            }

            if (mTempHatch != null && !mTempHatch.IsDisposed)
            {
                TransientManager.CurrentTransientManager.EraseTransient(mTempHatch, new IntegerCollection());
                mTempHatch.Dispose();
                mTempHatch = null;
            }
        }

        #endregion

        #region Collect/Select Methods

        public Point3dCollection Collect()
        {
            if (mShape == Shape.Window || mShape == Shape.Box)
            {
                CollectWindowPoints();
            }
            else if (mShape == Shape.RegularPolygon)
            {
                mSegmentCount = GetRegularPolygonSideCount();
                CollectRegularPolygonPoints();
            }
            else if (mShape == Shape.Fence)
            {
                CollectFencePoints();
            }
            else if (mShape == Shape.Polygon)
            {
                CollectPolygonPoints();
            }

            return CollectedPoints;
        }

        public SelectionSet Select()
        {
            Editor ed = CurEditor;
            this.Collect();
            ed.PointMonitor -= Editor_PointMonitor;
            this.CleanupGraphics();
            if (this.CollectionStatus == PromptStatus.OK)
            {
                PromptSelectionResult prSelRes = null;
                if (mShape == Shape.Window || mShape == Shape.Box)
                {
                    if (IsCrossing)
                        prSelRes = ed.SelectCrossingWindow(CollectedPoints[0], CollectedPoints[1]);
                    else
                        prSelRes = ed.SelectWindow(CollectedPoints[0], CollectedPoints[1]);
                }
                else if (mShape == Shape.RegularPolygon || mShape == Shape.Polygon)
                {
                    if (IsCrossing)
                        prSelRes = ed.SelectCrossingPolygon(CollectedPoints);
                    else
                        prSelRes = ed.SelectWindowPolygon(CollectedPoints);
                }
                else if (mShape == Shape.Fence)
                {
                    prSelRes = ed.SelectFence(CollectedPoints);
                }

                if (prSelRes != null && prSelRes.Status == PromptStatus.OK)
                    return prSelRes.Value;
            }

            return null;
        }

        #endregion


    }
}
