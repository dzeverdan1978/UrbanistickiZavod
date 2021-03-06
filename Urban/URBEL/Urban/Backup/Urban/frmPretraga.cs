using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Data.SqlClient;
using System.Configuration;

namespace Urban
{
    

    public partial class frmPretraga : Form
    {
        public string filter;
        public PretragaUslovi pu;
        public ArrayList tipplakt, dpplakt, donosl,vrsttxt,oznmat,inref,brsllst,blbrprp,dosije,kart,opstine,cdi,stat;
        bool prvi_put=true;
        public UrbanDataSet.FirmaDataTable obradjivaci;
        UrbanDataSet.FirmaDataTable sel_obrad;

        public frmPretraga()
        {
            InitializeComponent();
        }

        private void ProveriListu(ListBox lista,ComboBox cb,ArrayList spisak)
        {
            bool nema = true;
            foreach (string stavka in lista.Items)
            {
                if (stavka == cb.Text)
                {
                    nema = false;
                    break;
                }
            }

            if (nema && cb.Text != "---" && !prvi_put)
            {
                lista.Items.Add(cb.Text);
                spisak.Add(cb.Text);
            }
        }

        private string Prefix
        {
            get
            {
                string izlaz = (filter==null || filter.Length == 0) ? " " : " AND";
                return izlaz;
            }
        }
       

        private void optDATUMI_CheckedChanged(object sender, EventArgs e)
        {
            panel2.Visible = !panel2.Visible;
            pu.datIma = optDATUMI.Checked;
        }

       

      


        private string DajLB(ListBox lb)
        {
            string izlaz = "";
            foreach (string stavka in lb.Items)
                izlaz += "'" + stavka + "',";
            izlaz = izlaz.Substring(0, izlaz.Length - 1);
            return izlaz;
        }
        private string DajKritsrijum(string ulaz)
        {
            string cs = ConfigurationManager.ConnectionStrings["Urban.Properties.Settings.UrbanConnectionString"].ConnectionString;
            SqlConnection veza = new SqlConnection(cs);
            veza.Open();
            string upit = @"select id from regupr where opstina like '%" + ulaz + "%'";
            SqlCommand kom = new SqlCommand(upit, veza);
            SqlDataReader rd = kom.ExecuteReader();
            string rezultat = "";
            while (rd.Read())
            {
                rezultat += rd.GetInt32(0).ToString() + ",";
            }
            rd.Close();
            veza.Close();

            rezultat = rezultat.Substring(0, rezultat.Length - 1);
            return rezultat;


        }

        string DajKriterijum(UrbanDataSet.FirmaDataTable ulaz)
        {
            if (ulaz == null)
                return "";
            if (ulaz.Rows.Count == 0)
                return "";
            try
            {
                UrbanDataSetTableAdapters.FirmaTableAdapter ta = new Urban.UrbanDataSetTableAdapters.FirmaTableAdapter();
                if (ta.Connection.State == ConnectionState.Closed)
                    ta.Connection.Open();
                string upit = "select id_plan from Obradjivac where id_firma in (";
                foreach (DataRow red in ulaz.Rows)
                    upit += red[0].ToString() + ",";
                upit = upit.Substring(0, upit.Length - 1) + ")";
                SqlCommand cmd = new SqlCommand(upit, ta.Connection);
                SqlDataReader rd = cmd.ExecuteReader();
                DataTable temp = new DataTable();
                temp.Load(rd);
                rd.Close();
                if (temp.Rows.Count == 0)
                    return "";
                else
                {
                    string izlaz = "(";
                    foreach (DataRow red in temp.Rows)
                        izlaz += "id=" + red[0].ToString() + " OR ";
                    izlaz = izlaz.Substring(0, izlaz.Length - 4) + ")";
                    return izlaz;
                }
                return "";

            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
                return "";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            filter += (pu.tip.Count==0) ? "" : Prefix +" "+"TIPPLAKT in ("+ DajLB(lbTIPPLAKT)+")";
            filter += (txtSlLst.Text == "") ? "" : Prefix + " " + "BRSLLST like '%" + txtSlLst.Text + "%'";
            if (!n1.Checked)
            {
                filter += (n3.Checked) ? Prefix + " " + "NASNZ=1" : Prefix + " " + "NASNZ=0";
            }
            if (optDATUMI.Checked && datumOD.Text.Length>0 && datumDO.Text.Length>0)
            {
                filter += Prefix + " DATSLLST>='" + datumOD.Text + "' AND DATSLLST<='" + datumDO.Text + "'";
            }
            filter += (txtBBLBR.Text == "") ? "" : Prefix + " " + "BBLBRPRP LIKE '%" + txtBBLBR.Text + "%'";
            // prazan bibliotecki broj
            filter += (optBibl.Checked == false) ? "" : Prefix + " " + "(BBLBRPRP IS NULL OR LEN(BBLBRPRP)=0)";
            filter += (pu.dp.Count==0) ? "" : Prefix + " " + "DPPLAKT in (" + DajLB(lbDPPLAKT) + ")";
            filter += (pu.donos.Count==0) ? "" : Prefix + " " + "DONOSL in (" + DajLB(lbDONOSL) + ")";
            filter += (pu.vrst.Count==0) ? "" : Prefix + " " + "VRSTTXT in (" + DajLB(lbVRSTTXT) + ")";
            // I oznmat se obradjuje po novom nacinu kao i opstina

            string[] oznSearch = txtOzn.Text.Split(',');
            string kriterijum1 = "";
            if (oznSearch.Length == 1)
                kriterijum1 = "OZNMAT LIKE '%" + txtOzn.Text + "%'";
            else
            {
                kriterijum1 = "OZNMAT LIKE '%" + oznSearch[0] + "%'";
                for (int i = 1; i < oznSearch.Length; i++)
                {
                    kriterijum1 += " OR OZNMAT LIKE '%" + oznSearch[i] + "%'";
                }
                
            }
            filter += (txtOzn.Text == "") ? "" : Prefix + " " + kriterijum1;
            // Opstina se pretrazuje po vise kriterijuma na bilo kom mestu
            string[] opstinaSearch = txtOpstina.Text.Split(',');
            string kriterijum = "";
            if (opstinaSearch.Length == 1)
                kriterijum = "OPSTINA LIKE '%"+txtOpstina.Text+"%'";
            else
            {
                kriterijum = "(OPSTINA LIKE '%" + opstinaSearch[0] + "%'";
               for (int i=1;i<opstinaSearch.Length;i++)
                {
                    kriterijum += " OR OPSTINA LIKE '%" + opstinaSearch[i] + "%'";
                }
                kriterijum += ")";
                //kriterijum = kriterijum.Substring(0, kriterijum.Length - 1);
                //kriterijum = "ID IN (" + DajKritsrijum(kriterijum) + ")";
            }
            filter += (txtOpstina.Text == "") ? "" : Prefix + " "+kriterijum ;
            
            string rez = DajKriterijum(sel_obrad);
            string dod = (rez.Length == 0) ? "" : Prefix +" "+ rez;
            filter += (sel_obrad.Rows.Count == 0) ? "" : dod;
            filter += (txtDosije.Text == "") ? "" : Prefix + " " + "DOSIJE LIKE '%" + txtDosije.Text + "%'";
            filter += (txtSekcija.Text == "") ? "" : Prefix + " " + "SEKCIJA LIKE '%" + txtSekcija.Text   + "%'";
           
            filter += (pu.inr.Count==0) ? "" : Prefix + " " + "INREF in (" + DajLB(lbINREF) + ")";
            if (!p1.Checked)
            {
                filter += (p3.Checked) ? Prefix + " " + "PREAL=1" : Prefix + " " + "PREAL=0";
            }
            if (!t1.Checked)
            {
                filter += (t3.Checked) ? Prefix + " " + "TREAL=1" : Prefix + " " + "TREAL=0";
            }
            if (!d1.Checked)
            {
                filter += (d3.Checked) ? Prefix + " " + "DIGITALAN=1" : Prefix + " " + "DIGITALAN=0";
            }
            if (!s1.Checked)
            {
                filter += (s3.Checked) ? Prefix + " " + "SKENIRAN=1" : Prefix + " " + "SKENIRAN=0";
            }
            if (!g1.Checked)
            {
                filter += (g3.Checked) ? Prefix + " " + "GUP=1" : Prefix + " " + "GUP=0";
            }
            if (!r1.Checked)
            {
                filter += (r3.Checked) ? Prefix + " " + "RREAL=1" : Prefix + " " + "RREAL=0";
            }
            filter += (txtKartBroj.Text == "") ? "" : Prefix + " " + "KARTBROJ='" + txtKartBroj.Text + "'";
            filter += (txtID.Text == "") ? "" : Prefix + " " + "ID=" + txtID.Text ;
            filter += (txtNazPrp.Text == "") ? "" : Prefix + " " + "NAZPRP LIKE '%" + txtNazPrp.Text + "%'";
            filter += (txtNapomena.Text == "") ? "" : Prefix + " " + "NAPOM LIKE '%" + txtNapomena.Text + "%'";
            filter += (txtOCENA.Text == "") ? "" : Prefix + " " + "OCENA LIKE '%" + txtOCENA.Text + "%'";
            filter += (txtCD.Text == "") ? "" : Prefix + " " + "CD like '%" + txtCD.Text + "%'";
            filter += (txtKRUG.Text == "") ? "" : Prefix + " " + "STATKRUG like '%" + txtKRUG.Text + "%'";
        }

        private bool ProveriArrayList(ArrayList a)
        {
            if (a != null && a.Count > 0)
                return true;
            else
                return false;
        }

        private RadioButton PostaviRB(int opcija, string sta)
        {
            switch (sta)
            {
                case "n":
                    switch (opcija)
                    {
                        case 0:
                            return n1;
                        case 1:
                            return n2;
                        case 2:
                            return n3;
                    }
                    break;
                case "p":
                    switch (opcija)
                    {
                        case 0:
                            return p1;
                        case 1:
                            return p2;
                        case 2:
                            return p3;
                    }
                    break;
                case "t":
                    switch (opcija)
                    {
                        case 0:
                            return t1;
                        case 1:
                            return t2;
                        case 2:
                            return t3;
                    }
                    break;
                case "r":
                    switch (opcija)
                    {
                        case 0:
                            return r1;
                        case 1:
                            return r2;
                        case 2:
                            return r3;
                    }
                    break;
                case "g":
                    switch (opcija)
                    {
                        case 0:
                            return g1;
                        case 1:
                            return g2;
                        case 2:
                            return g3;
                    }
                    break;
                case "d":
                    switch (opcija)
                    {
                        case 0:
                            return d1;
                        case 1:
                            return d2;
                        case 2:
                            return d3;
                    }
                    break;
                case "s":
                    switch (opcija)
                    {
                        case 0:
                            return s1;
                        case 1:
                            return s2;
                        case 2:
                            return s3;
                    }
                    break;
            }
            return null;
        }

        private void frmPretraga_Load(object sender, EventArgs e)
        {
            sel_obrad = (UrbanDataSet.FirmaDataTable)obradjivaci.Clone();
            if (pu.sel_obrad == null)
                pu.sel_obrad = sel_obrad;
            else
                sel_obrad = pu.sel_obrad;
            UrbanDataSet.FirmaRow crta = obradjivaci.NewFirmaRow();
            crta.naziv = "---";
            obradjivaci.Rows.Add(crta);
            cbObrad.DataSource = obradjivaci;
            cbObrad.DisplayMember = "naziv";
            cbObrad.ValueMember = "id";
            cbObrad.SelectedIndex = cbObrad.Items.Count - 1;
            
            lbObrad.DataSource = sel_obrad;
            lbObrad.DisplayMember = "naziv";
            lbObrad.ValueMember = "id";
            if (ProveriArrayList(pu.tip))
            {
                foreach (string s in pu.tip)
                    lbTIPPLAKT.Items.Add(s);
            }
            if (ProveriArrayList(pu.dp))
            {
                foreach (string s in pu.dp)
                    lbDPPLAKT.Items.Add(s);
            }
            if (ProveriArrayList(pu.donos))
            {
                foreach (string s in pu.donos)
                    lbDONOSL.Items.Add(s);
            }
            if (ProveriArrayList(pu.vrst))
            {
                foreach (string s in pu.vrst)
                    lbVRSTTXT.Items.Add(s);
            }
           
            if (ProveriArrayList(pu.inr))
            {
                foreach (string s in pu.inr)
                    lbINREF.Items.Add(s);
            }
            // Postavljanje RadioButton-a
            PostaviRB(pu.nasn, "n").Checked = true;
            PostaviRB(pu.preal, "p").Checked = true;
            PostaviRB(pu.treal, "t").Checked = true;
            PostaviRB(pu.rreal, "r").Checked = true;
            PostaviRB(pu.gup, "g").Checked = true;
            PostaviRB(pu.vektor, "d").Checked = true;
            PostaviRB(pu.sken, "s").Checked = true;

            datumOD.Text = pu.datod;
            datumDO.Text = pu.datdo;

            txtBBLBR.Text = pu.bibl;
            txtCD.Text = pu.cd;
            txtDosije.Text = pu.dos;
            txtID.Text = pu.id;
            txtKartBroj.Text = pu.kar;
            txtNapomena.Text = pu.nap;
            txtNazPrp.Text = pu.naz;
            txtOpstina.Text = pu.opst;
            txtKRUG.Text = pu.stat;
            txtOCENA.Text = pu.ocena;
            txtOzn.Text = pu.ozn;
            txtSekcija.Text = pu.sek;
            txtSlLst.Text = pu.sll;
            optDATUMI.Checked = pu.datIma;
           // ovde dodati novu logiku za radio buttone

            string cs = ConfigurationManager.ConnectionStrings["Urban.Properties.Settings.UrbanConnectionString"].ConnectionString;
            SqlConnection veza = new SqlConnection(cs);
            veza.Open();
            tipplakt = DajKolonu(veza, "TIPPLAKT");
           
            dpplakt = DajKolonu(veza, "DPPLAKT");

            stat = DajKolonu(veza, "STATKRUG");

            donosl = DajKolonu(veza, "DONOSL");
           
            vrsttxt = DajKolonu(veza, "VRSTTXT");
           
            oznmat = DajKolonu(veza, "OZNMAT");
            
            inref = DajKolonu(veza, "INREF");

            brsllst = DajKolonu(veza, "BRSLLST");

            dosije = DajKolonu(veza, "DOSIJE");

            kart = DajKolonu(veza, "KARTBROJ");

            opstine = DajKolonu(veza, "OPSTINA");

            cdi = DajKolonu(veza, "CD");
            AutoCompleteStringCollection asc1 = new AutoCompleteStringCollection();
            foreach (string s in brsllst)
                asc1.Add(s);
            txtSlLst.AutoCompleteCustomSource = asc1;

            blbrprp = DajKolonu(veza, "BBLBRPRP");
            AutoCompleteStringCollection asc2 = new AutoCompleteStringCollection();
            foreach (string s in blbrprp)
                asc2.Add(s);
            txtBBLBR.AutoCompleteCustomSource = asc2;

            AutoCompleteStringCollection ac = new AutoCompleteStringCollection();
            foreach (string s in stat)
                ac.Add(s);
            txtKRUG.AutoCompleteCustomSource = ac;


            AutoCompleteStringCollection asc3 = new AutoCompleteStringCollection();
            foreach (string s in dosije)
                asc3.Add(s);
            txtDosije.AutoCompleteCustomSource = asc3;

            AutoCompleteStringCollection asc4 = new AutoCompleteStringCollection();
            foreach (string s in kart)
                asc4.Add(s);
            txtKartBroj.AutoCompleteCustomSource = asc4;

            AutoCompleteStringCollection asc5 = new AutoCompleteStringCollection();
            foreach (string s in opstine)
                asc5.Add(s);
            txtOpstina.AutoCompleteCustomSource = asc5;

            AutoCompleteStringCollection asc55 = new AutoCompleteStringCollection();
            foreach (string s in oznmat)
                asc55.Add(s);
            txtOzn.AutoCompleteCustomSource = asc55;

            AutoCompleteStringCollection asc6 = new AutoCompleteStringCollection();
            foreach (string s in cdi)
                asc6.Add(s);
            txtCD.AutoCompleteCustomSource = asc6;

            AutoCompleteStringCollection asc10 = new AutoCompleteStringCollection();
            foreach (string s in oznmat)
                asc10.Add(s);
            txtOzn.AutoCompleteCustomSource = asc10;
            

            tipplakt.Add("---");
            dpplakt.Add("---");
            donosl.Add("---");
            oznmat.Add("---");
            inref.Add("---");
            vrsttxt.Add("---");

            cbTipPlakt.DataSource = tipplakt;
            cbDpPlakt.DataSource = dpplakt;
            cbDONOSL.DataSource = donosl;
           
            
            cbInref.DataSource = inref;
            cbVrst.DataSource = vrsttxt;

            cbTipPlakt.Text = "---";
            cbDpPlakt.Text = "---";
            cbDONOSL.Text = "---";
           
            cbInref.Text = "---";
            cbVrst.Text = "---";

            veza.Close();

            prvi_put = false;
            /*lbTIPPLAKT.Items.Clear();
            lbDPPLAKT.Items.Clear();
            lbDONOSL.Items.Clear();
            lbOZNMAT.Items.Clear();
            lbINREF.Items.Clear();
            lbVRSTTXT.Items.Clear(); */ 
        }

        private ArrayList DajKolonu(SqlConnection veza, string naziv)
        {
            try
            {
                if (veza.State == ConnectionState.Closed)
                    veza.Open();
                string upit = "select distinct " + naziv + " from regupr where " + naziv + " is not null";
                SqlCommand kom = new SqlCommand(upit, veza);
                SqlDataReader rd = kom.ExecuteReader();
                ArrayList izlaz = new ArrayList();
                while (rd.Read())
                {
                    if (naziv != "KARTBROJ")
                        izlaz.Add(rd.GetString(0));
                    else
                        izlaz.Add(rd.GetString(0));
                }
                rd.Close();
                return izlaz;
            }
            catch (SqlException ex)
            {
                throw new Exception("Greska u citanju " + naziv);
            }
        }

    

        private void cbTipPlakt_SelectedIndexChanged(object sender, EventArgs e)
        {
            ProveriListu(lbTIPPLAKT, cbTipPlakt,pu.tip);
        }

        private void cbDpPlakt_SelectedIndexChanged(object sender, EventArgs e)
        {
            ProveriListu(lbDPPLAKT, cbDpPlakt,pu.dp);
        }

        private void cbDONOSL_SelectedIndexChanged(object sender, EventArgs e)
        {
            ProveriListu(lbDONOSL, cbDONOSL,pu.donos);
        }

        

        private void cbInref_SelectedIndexChanged(object sender, EventArgs e)
        {
            ProveriListu(lbINREF, cbInref,pu.inr);
        }

        private void ObrisiStavku(ListBox cb,ArrayList pod)
        {
            if (cb.SelectedIndex >= 0)
            {
                pod.RemoveAt(cb.SelectedIndex);
                cb.Items.RemoveAt(cb.SelectedIndex);
                
            }
        }

        private void lbTIPPLAKT_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                ObrisiStavku(lbTIPPLAKT,pu.tip);
        }

        private void lbDPPLAKT_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                ObrisiStavku(lbDPPLAKT,pu.dp);
        }

        private void lbDONOSL_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                ObrisiStavku(lbDONOSL,pu.donos);
        }

       

        private void lbINREF_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                ObrisiStavku(lbINREF,pu.inr);
        }

        private void lbVRSTTXT_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                ObrisiStavku(lbVRSTTXT,pu.vrst);
        }

        private void cbVrst_SelectedIndexChanged(object sender, EventArgs e)
        {
            ProveriListu(lbVRSTTXT, cbVrst,pu.vrst);
        }

        private void txtSlLst_TextChanged(object sender, EventArgs e)
        {
            pu.sll = txtSlLst.Text;
        }

        private void datumOD_TextChanged(object sender, EventArgs e)
        {
            pu.datod = datumOD.Text;
        }

        private void datumDO_TextChanged(object sender, EventArgs e)
        {
            pu.datdo = datumDO.Text;
        }

        private void txtBBLBR_TextChanged(object sender, EventArgs e)
        {
            pu.bibl = txtBBLBR.Text;
        }

        private void txtOpstina_TextChanged(object sender, EventArgs e)
        {
            pu.opst = txtOpstina.Text;
        }

        private void txtDosije_TextChanged(object sender, EventArgs e)
        {
            pu.dos = txtDosije.Text;
        }

        private void txtSekcija_TextChanged(object sender, EventArgs e)
        {
            pu.sek = txtSekcija.Text;
        }

        private void txtKartBroj_TextChanged(object sender, EventArgs e)
        {
            pu.kar = txtKartBroj.Text;
        }

        private void txtNazPrp_TextChanged(object sender, EventArgs e)
        {
            pu.naz = txtNazPrp.Text;
        }

        private void txtNapomena_TextChanged(object sender, EventArgs e)
        {
            pu.nap = txtNapomena.Text;
        }

        private void txtCD_TextChanged(object sender, EventArgs e)
        {
            pu.cd = txtCD.Text;
        }

        private void txtID_TextChanged(object sender, EventArgs e)
        {
            pu.id = txtID.Text;
        }

        private void frmPretraga_Leave(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            pu = new PretragaUslovi();
            pu.tip = new ArrayList();
            pu.dp = new ArrayList();
            pu.donos = new ArrayList();
            pu.vrst = new ArrayList();
            if (pu.sel_obrad!=null)
                pu.sel_obrad.Rows.Clear();
          
            pu.inr = new ArrayList();

            cbTipPlakt.Text = "---";
            lbTIPPLAKT.Items.Clear();
            txtSlLst.Text = "";
            n1.Checked = true;
            datumOD.Text = "";
            datumDO.Text = "";
            optDATUMI.Checked = false;
            txtBBLBR.Text = "";
            cbDpPlakt.Text = "---";
            lbDPPLAKT.Items.Clear();
            cbDONOSL.Text = "---";
            lbDONOSL.Items.Clear();
            cbVrst.Text = "---";
            lbVRSTTXT.Items.Clear();
            txtOpstina.Text = "";
            txtOzn.Text = "";
            txtDosije.Text = "";
            txtSekcija.Text = "";
            txtKRUG.Text = "";
            txtOCENA.Text = "";


            p1.Checked = true;
            t1.Checked = true;
            r1.Checked = true;
            g1.Checked = true;
            d1.Checked = true;
            s1.Checked = true;
            cbInref.Text = "---";
            lbINREF.Items.Clear();
            txtKartBroj.Text = "";
            txtCD.Text = "";
            txtKRUG.Text = "";
            txtID.Text = "";
            txtNazPrp.Text = "";
            txtNapomena.Text = "";
        }

        private void label8_MouseHover(object sender, EventArgs e)
        {
            ToolTip tip = new ToolTip();
            tip.IsBalloon = true;
            tip.AutoPopDelay = 10000;
            string caption = "01 Barajevo 02 Vozdovac 03 Vracar 04 Grocka\n 05 Zvezdara 06 Zemun 07 Lazarevac 08 Mladenovac\n 09 N.Beograd 10 Obrenovac 11 Palilula 12 S.Venac\n 13 Sopot 14 S.Grad 15 Cukarica 16 Rakovica 17 Surcin";
            tip.SetToolTip(label8, caption);
        }

        private void txtOzn_TextChanged(object sender, EventArgs e)
        {
            pu.ozn = txtOzn.Text;
        }

        private void n1_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            if (rb.Name.Substring(0, 1) == "n")
                pu.nasn = Convert.ToInt32(rb.Name.Substring(1, 1))-1;
            if (rb.Name.Substring(0, 1) == "p")
                pu.preal = Convert.ToInt32(rb.Name.Substring(1, 1))-1;
            if (rb.Name.Substring(0, 1) == "t")
                pu.treal = Convert.ToInt32(rb.Name.Substring(1, 1))-1;
            if (rb.Name.Substring(0, 1) == "r")
                pu.rreal = Convert.ToInt32(rb.Name.Substring(1, 1)) - 1;
            if (rb.Name.Substring(0, 1) == "g")
                pu.gup = Convert.ToInt32(rb.Name.Substring(1, 1)) - 1;
            if (rb.Name.Substring(0, 1) == "d")
                pu.vektor = Convert.ToInt32(rb.Name.Substring(1, 1)) - 1;
            if (rb.Name.Substring(0, 1) == "s")
                pu.sken = Convert.ToInt32(rb.Name.Substring(1, 1)) - 1;

        }

        private void label5_MouseHover(object sender, EventArgs e)
        {
            ToolTip tip = new ToolTip();
            tip.IsBalloon = true;
            tip.AutoPopDelay = 10000;
            string caption = "Faza planskog dokumenta gde je: \nD - Donet, usvojen \nP - Pristupanje izradi, Priprema";
            tip.SetToolTip(label5, caption);
        }

        private void label6_MouseHover(object sender, EventArgs e)
        {
            ToolTip tip = new ToolTip();
            tip.IsBalloon = true;
            tip.AutoPopDelay = 10000;
            string caption = "G - Grad \nO - opstina \nR - republika \nS - sekretarijat \nI - izvrsni odbor";
            tip.SetToolTip(label6, caption);
        }

        private void label9_MouseHover(object sender, EventArgs e)
        {
            ToolTip tip = new ToolTip();
            tip.IsBalloon = true;
            tip.AutoPopDelay = 10000;
            string caption = "OSN - Osnovni \nDOP - Dopuna \nISP - Ispravka \nIZD - izmena i dopuna \nIZM - izmena";
            tip.SetToolTip(label9, caption);
        }

        private void label12_MouseHover(object sender, EventArgs e)
        {
            ToolTip tip = new ToolTip();
            tip.IsBalloon = true;
            tip.AutoPopDelay = 10000;
            string caption = "CEN – Centri, zanatski centri, poslovanje \nELE – Elktro vodovi – podzemni vodovi, struja \nGAS – Gasovodi , gasifikacija, naftovodi, pumpe \nGRB – Groblja \nIND – industijske zone, proizvodnja \nINF – Infrastruktura, trafostanice \nKAN – Kolektori, kanalizacioni sistemi \nKAT – Katastar - premer \nOST – Zaštita područja, zemljišta, komasacije, sanitarne zaštite \nPOL – Poljoprivreda \nPOV – Poverljivi planovi, vojska milicija \nREK – Rekreacija, sport \nSAB – Saobraćaj \nSPE – Posebna, specijalna namena \nSTN – Stanovanje \nSVE – Sve teme se obrađuju, to rade Generalni planovi, Prostorni.... \nTER – Sajmovi, domovi kulture, trgovi, razne privremene lokacije \nTKS – Deponije \nTOP – Toplovod \nTTM – Telekomunikacione mreže,  vodovi \nVIK – Vodovod i kanalizacija, sanitarni čvorovi \nVOD – Samo vodovod \nVPR – Sanitarna zaštita, uređenje obala \n ZEL – Izletišta, parkovi \nZKN – Zakonski akti, zaštićena područja, strateške procene ";
            tip.SetToolTip(label12, caption);
        }

        private void datumOD_TextChanged_1(object sender, EventArgs e)
        {
            pu.datod = datumOD.Text;
        }

        private void datumDO_TextChanged_1(object sender, EventArgs e)
        {
            pu.datdo = datumDO.Text;
        }

        private void label1_MouseHover(object sender, EventArgs e)
        {
            ToolTip tip = new ToolTip();
            tip.IsBalloon = true;
            tip.AutoPopDelay = 10000;
            string caption = "UP - Urbanisticki projekat\nPP - Prostorni plan\nOST - Ostalo\nGUP - Generalni urbanisticki plan\nRP - Regulacioni plan\nPAR - Plan parcelacije\nDUP - Detaljni urbanisticki plan\nPRIV - Privremene dozvole\nPGR - Plan generalne regulacije\nDRUP - Drugi propisi\nSTR - Strateske procene\nGP - Generalni plan\nUUP - Uslovi za uredjenje prostora\nPDR - Plan detaljne regulacije\nPOU - Plan opsteg uredjenja";
            tip.SetToolTip(label1, caption);
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void cbObrad_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbObrad.SelectedIndex!=cbObrad.Items.Count-1 && !prvi_put)
            {
                DataRow[] test = sel_obrad.Select("id=" + obradjivaci[cbObrad.SelectedIndex].id.ToString());
                if (test.Length == 0)
                    sel_obrad.ImportRow(obradjivaci[cbObrad.SelectedIndex]);
            }
        }

        private void lbObrad_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && lbObrad.SelectedIndex >= 0)
                sel_obrad.Rows.RemoveAt(lbObrad.SelectedIndex);
        }

        private void txtKRUG_TextChanged(object sender, EventArgs e)
        {
            pu.stat = txtKRUG.Text;
        }

        private void txtOCENA_TextChanged(object sender, EventArgs e)
        {
            pu.ocena = txtOCENA.Text;
        }

    

        
        
    }

    public class PretragaUslovi
    {
        public ArrayList tip, dp, donos, vrst, inr;
        public string sll, datod, datdo, bibl, opst, dos, sek, kar, naz, nap, cd, id,ozn,stat,ocena;
        public bool nasnIma, datIma, prealIma, trealIma, rrealIma;
        public int nasn, preal, treal, rreal,gup,vektor,sken;
        public UrbanDataSet.FirmaDataTable sel_obrad;

    }
}