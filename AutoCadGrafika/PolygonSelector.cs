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

using MgdAcApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using MgdAcDocument = Autodesk.AutoCAD.ApplicationServices.Document;
using AcWindowsNS = Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.Internal.PropertyInspector;
using System.Runtime.InteropServices;
using System.Reflection;
using Autodesk.Gis.Map.Classification;
using System.Collections;

namespace AutoCadGrafika
{
    public  class Commands
    {
        [CommandMethod("SelectObjectsByPolygon",CommandFlags.Modal | CommandFlags.Redraw)]
        public static  void SelectObjectsByCrossingWindow()
        {
            string status = "";
            Editor ed = MgdAcApplication.DocumentManager.MdiActiveDocument.Editor;
            var acDoc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            status = "Pocetak";

            try
            {
                using (PointCollector2 pc = new PointCollector2(PointCollector2.Shape.Polygon, true))
                {
                    using (SelectionSet ss = pc.Select())
                        if (ss != null && ss.Count > 0)
                        {
                            status = "Vraceni selektovani objekti";
                            ed.SetImpliedSelection(ss.GetObjectIds().ToArray());
                            List<string> karts = new List<string>();

                            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
                            {
                                foreach (SelectedObject plan in ss)
                                {
                                    // procitaj atribut KARTBROJ i dodaj ga u rezultat
                                    Entity ent = acTrans.GetObject(plan.ObjectId, OpenMode.ForRead) as Entity;
                                    string kart = GetKart(ent);
                                    
 
                                            if (kart != null)
                                            {
                                                if (kart.Contains(","))
                                                {
                                                    string[] kbr = kart.Split(',');
                                                    foreach (string kb in kbr)
                                                        if (!karts.Contains(kb))
                                                            karts.Add(kb);
                                                }
                                                else
                                                {
                                                    if (!karts.Contains(kart))
                                                        karts.Add(kart);
                                                }

                                            }
                                           
                                        
                                        status = "Vracen propery KART";
                                    
                                }
                                acTrans.Commit();
                            }
                            string ispis = "";
                            foreach (string kb in karts)
                                ispis += kb + ",";
                            if (ispis.EndsWith(","))
                                ispis = ispis.Substring(0, ispis.Length - 1);
                            if (ispis.Length > 0)
                            {
                                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("Izdvojeni su sledeci katografski brojevi\n"
                                    + ispis + "\nKartografski brojevi su spremni za koriscenje u aplikaciji");

                                Clipboard.SetText(ispis);
                                status = "Rezultat dodat u Clipboard";

                            }
                            else
                                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog("Nema ni jednog kartografskog broja. Pokusajte ponovo");

                        }
                }
                
            }
            catch (System.Exception ex)
            {
                string poruka = ex.Message+"\n"+status;
                if (ex.InnerException != null)
                    poruka += " " + ex.InnerException.Message;

                Autodesk.AutoCAD.ApplicationServices.Application.ShowAlertDialog(poruka);
            }
        
        }


        static string GetKart(DBObject obj)
        {
            Autodesk.Gis.Map.Classification.ClassificationManager cls_manager = null;
            cls_manager = Autodesk.Gis.Map.HostMapApplicationServices.Application.ActiveProject.ClassificationManager;
            FeatureClassPropertyCollection objClass_Property = new FeatureClassPropertyCollection();
            ArrayList list_values = null;

            cls_manager.GetProperties(objClass_Property, list_values, obj.ObjectId);
            

            foreach (FeatureClassProperty cp in objClass_Property)
            {
                if (cp.Name.ToString().ToLower().Contains("kart"))
                    return cp.DefaultValue.ToString();
            }

            return null;

        }


    }
}
