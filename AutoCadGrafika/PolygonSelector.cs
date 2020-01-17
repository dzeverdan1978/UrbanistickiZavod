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

                                    var props = GetOPMProperties(plan.ObjectId);
                                    if (props.Count == 0)
                                    {
                                        status = "Nema properija za objekat";

                                    }
                                    else
                                        status = "Vraceni properties za object";

                                    foreach (var pair in props)
                                    {
                                        
                                        if (pair.Key.ToLower().Contains("kart"))
                                        {
                                            object value = pair.Value;
                                            if (value != null)
                                            {
                                                if (value.ToString().Contains(","))
                                                {
                                                    string[] kbr = value.ToString().Split(',');
                                                    foreach (string kb in kbr)
                                                        if (!karts.Contains(kb))
                                                            karts.Add(kb);
                                                }
                                                else
                                                {
                                                    if (!karts.Contains(value.ToString()))
                                                        karts.Add(value.ToString());
                                                }

                                            }
                                            if (Marshal.IsComObject(pair.Value))
                                                Marshal.ReleaseComObject(pair.Value);
                                        }
                                        status = "Vracen propery KART";
                                    }
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

        public static IDictionary<string, object> GetOPMProperties(ObjectId id)
        {
            Dictionary<string, object> map = new Dictionary<string, object>();
            IntPtr pUnk = ObjectPropertyManagerPropertyUtility.GetIUnknownFromObjectId(id);
            if (pUnk != IntPtr.Zero)
            {
                using (CollectionVector properties = ObjectPropertyManagerProperties.GetProperties(id, false, false))
                {
                    int cnt = properties.Count();
                    if (cnt != 0)
                    {
                        using (CategoryCollectable category = properties.Item(0) as CategoryCollectable)
                        {
                            CollectionVector props = category.Properties;
                            int propCount = props.Count();
                            for (int j = 0; j < propCount; j++)
                            {
                                using (PropertyCollectable prop = props.Item(j) as PropertyCollectable)
                                {
                                    if (prop == null)
                                        continue;
                                    object value = null;
                                    if (prop.GetValue(pUnk, ref value) && value != null)
                                    {
                                        if (!map.ContainsKey(prop.Name))
                                            map[prop.Name] = value;
                                    }
                                }
                            }
                        }
                    }
                }
                Marshal.Release(pUnk);
            }
            return map;
        }



    }
}
