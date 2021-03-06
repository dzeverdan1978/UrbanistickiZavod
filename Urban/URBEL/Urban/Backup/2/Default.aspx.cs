using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Data;
using System.Data.SqlClient;

public partial class _Default : System.Web.UI.Page 
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }
    protected void txtLart_Load(object sender, EventArgs e)
    {
        txtLart.Focus();
    }

    private string Rimski(string ulaz)
    {
        string[] rimski ={ "/I/", "/II/", "/III/", "/IV/", "/V/", "/VI", "/VII/", "/VIII/", "/IX/", "/X/", "/XI/", "/XII/" };
        foreach (string test in rimski)
        {
            if (ulaz.Contains(test))
                return test;
        }
        return "";
    }

    private string ObradiNaziv(string ulaz)
    {
        try
        {
            int index = -1;
            string rim = Rimski(ulaz);
            if ((index = ulaz.IndexOf("SL.L.")) >= 0)
            {
                if (rim.Length == 0)
                    return ulaz.Substring(0, index + 11);
                else
                    return ulaz.Substring(0, index + 11 + rim.Length);
            }
            else if ((index = ulaz.IndexOf("SL.GL.")) >= 0)
            {
                if (rim.Length == 0)
                    return ulaz.Substring(0, index + 15);
                else
                    return ulaz.Substring(0, index + 15 + rim.Length);
            }
            return ulaz;
        }
        catch
        {
            return ulaz;
        }
    }

    protected void txtLart_TextChanged(object sender, EventArgs e)
    {
        SqlConnection veza = null;
        try
        {
            if (txtLart.Text == "0")
                throw new Exception("Greska u unosu kartografskog broja!");
            veza = new SqlConnection(ConfigurationManager.ConnectionStrings["cs"].ConnectionString);
            veza.Open();
            try
            {
                int kbr = -1;
                try
                {
                    kbr = int.Parse(txtLart.Text);
                }
                catch (FormatException)
                {
                    kbr = -1;
                }
                if (kbr >= 5001 || txtLart.Text.Substring(0,1).ToUpper()=="P")
                {
                    string query = "select naziv,obradjivac,vektor,sken from program where kartbr='" + txtLart.Text + "'";
                    if (txtLart.Text.Substring(0,1).ToUpper()=="P")
                        query = "select naziv,' ',vektor,sken from program where kartbr='" + txtLart.Text + "'";
                    SqlDataAdapter f = new SqlDataAdapter(query, veza);
                    DataTable t = new DataTable();
                    f.Fill(t);
                    if (t.Rows.Count == 0)
                    {
                        lit.Text = "<p><strong>Uneti kartografski broj nije pronadjen u bazi podataka!</strong></p>";
                        return;
                    }
                    else
                    {
                        DataRow kandidat = t.Rows[0];
                        string ispis = "<table border='1' bordercolor=#000000 bgcolor='gray'>";
                        ispis += "<tr><td><strong>Naziv predmeta</strong></td><td><strong>Rezultat</strong></td></tr>";
                        ispis += "<tr><td align='left' bgcolor=#cccccc><strong>" + ObradiNaziv(kandidat[0].ToString()) + "</strong></td>";
                        ispis += "<td bgcolor='#ffff00'><strong>";
                        if (txtLart.Text.Substring(0, 1).ToUpper() == "P")
                            ispis += "Planski dokument se nalazi u centru.";
                        else
                            ispis += "Informacije o programu možete pronaći kod obrađivača " + kandidat[1].ToString();
                        // dodaci za vektor i sken
                        string digplus = "";
                        if (Convert.ToBoolean(kandidat[2]))
                            digplus += "<div style='background-color:orange'>Planski dokument postoji u vektorskom obliku.</div>";
                        else
                            digplus += "<div style='background-color:orange'>Planski dokument ne postoji u vektorskom obliku.</div>";
                        string skenplus = "";
                        if (Convert.ToBoolean(kandidat[3]))
                            skenplus += "<div style='background-color:azure'>Planski dokument je skeniran.</div>";
                        else
                            skenplus += "<div style='background-color:azure'>Planski dokument nije skeniran.</div>";
                        ispis += "</strong>"+digplus+skenplus+"</td></tr>";
                        ispis += "</table>";
                        lit.Text = ispis;
                        return;
                    }
                }

            }
            catch
            {

            }
            string qry = "DajZaduzenja";
            SqlDataAdapter filter = new SqlDataAdapter(qry, veza);
            filter.SelectCommand.CommandType = CommandType.StoredProcedure;
            filter.SelectCommand.Parameters.Add("@kart", SqlDbType.VarChar).Value = txtLart.Text.Trim();
            DataTable pom = new DataTable();
            filter.Fill(pom);

            if (pom.Rows.Count == 0)
            {
                string upit = "select nazprp,bblbrprp,cd,digitalan,skeniran from regupr where kartbroj=@kart";
                SqlDataAdapter ad = new SqlDataAdapter(upit, veza);
                
                ad.SelectCommand.Parameters.Add("@kart", SqlDbType.VarChar).Value = txtLart.Text.Trim();
                DataTable tab = new DataTable();
                ad.Fill(tab);
                if (tab.Rows.Count == 0)
                    lit.Text = "<p><strong>Uneti kartografski broj nije pronadjen u bazi podataka!</strong></p>";
                else
                {
                    DataRow r = tab.Rows[0];
                    string ispis = "<table border='1' bordercolor=#000000 bgcolor='gray'>";
                    ispis += "<tr><td><strong>Naziv predmeta</strong></td><td><strong>Rezultat</strong></td></tr>";
                    ispis += "<tr><td align='left' bgcolor=#cccccc><strong>" + ObradiNaziv(r[0].ToString()) + "</strong></td>";
                    ispis += "<td bgcolor='#ffff00'><strong>";
                    if (r.IsNull(1) || r[1].ToString().Length == 0 || r[1].ToString().Contains("NEMA ELAB."))
                    {
                         
                            ispis += "Planski dokument se ne nalazi u centru. Pokušajte u pisarnici.";
                    }
                    else
                        ispis += "Planski dokument se nalazi u centru.";
                    // dodatak za cd
                    string digplus = "";
                    if (Convert.ToBoolean(r[3]))
                        digplus += "<div style='background-color:orange'>Planski dokument postoji u vektorskom obliku.</div>";
                    else
                        digplus += "<div style='background-color:orange'>Planski dokument ne postoji u vektorskom obliku.</div>";
                    string skenplus = "";
                    if (Convert.ToBoolean(r[4]))
                        skenplus += "<div style='background-color:azure'>Planski dokument je skeniran.</div>";
                    else
                        skenplus += "<div style='background-color:azure'>Planski dokument nije skeniran.</div>";
                    string pluscd = "";

                    if (!r.IsNull(2) && r[2].ToString().Length > 0)
                    {
                        string joscd = DuznikCD(r[2].ToString(), veza);
                        pluscd += "<div style='background-color:green'>Planski dokument postoji u obliku CD-a. " + joscd + ".</div>";
                    }
                        ispis += "</strong>"+pluscd+digplus+skenplus+"</td></tr>";
                    ispis += "</table>";
                    lit.Text = ispis;
                }
            }
            else
            {
                string ispis = "<table border='1' bordercolor=#000000 bgcolor='gray'>";
                ispis += "<tr><td><strong>Naziv predmeta</strong></td><td><strong>Rezultat</strong></td></tr>";
                foreach (DataRow red in pom.Rows)
                {
                    ispis += "<tr><td align='left' bgcolor=#cccccc><strong>" + ObradiNaziv(red[0].ToString()) + "</strong></td>";
                    ispis += "<td bgcolor='#ffff00'><strong>";
                    int broj = -1;
                    try
                    {
                        broj = int.Parse(txtLart.Text);
                    }
                    catch (FormatException)
                    {
                        broj = -1;
                    }
                    if (broj >= 700 && broj <= 999 && red.IsNull(4))
                        ispis += "Informacije o izradi planskog dokumenta možete pronaći kod obrađivača.";
                    else
                    {

                        int test = Convert.ToInt32(red[1]);
                        if (test == -1)
                            ispis += "Planski dokument se nalazi u centru.";
                        else if (test == 0)
                            ispis += "Planski dokument je zadužio/la " + red[2].ToString();
                        else if (test == 100)
                            ispis += "Planski dokument se ne nalazi u centru. Pokusajte u pisarnici.";
                    }
                    // dodatak za cd
                    // dodatak za dig i sken
                    string digplus = "";
                    if (Convert.ToBoolean(red[6]))
                        digplus += "<div style='background-color:orange'>Planski dokument postoji u vektorskom obliku.</div>";
                    else
                        digplus += "<div style='background-color:orange'>Planski dokument ne postoji u vektorskom obliku.</div>";
                    string skenplus = "";
                    if (Convert.ToBoolean(red[7]))
                        skenplus += "<div style='background-color:azure'>Planski dokument je skeniran.</div>";
                    else
                        skenplus += "<div style='background-color:azure'>Planski dokument nije skeniran.</div>";
                    string pluscd = "";
                    
                    if (red[5] != null && red[5].ToString().Length > 0)
                    {
                        string joscd = DuznikCD(red[5].ToString(), veza);
                        pluscd += "<div style='background-color:green'>Planski dokument postoji u obliku CD-a. "+joscd+".</div>";
                    }
                    ispis += "</strong>"+pluscd+digplus+skenplus+"</td></tr>";
                }
                ispis += "</table>";
                lit.Text = ispis;
            }
        }
        catch (SqlException ex)
        {
            lit.Text = "<p>Greska u citanju podataka!"+ex.Message+"</p>";
        }
        catch (Exception ex)
        {
            lit.Text = "<p>" + ex.Message + "</p>";
        }
        finally
        {
            if (veza != null && veza.State == ConnectionState.Open)
                veza.Close();
        }
    }

    string DuznikCD(string cd, SqlConnection veza)
    {
        try
        {
            string upit = "select duznik from zaduzenje where sifra like '" + cd + "%' and raz=0";
            string izlaz = "";
            SqlCommand cmd = new SqlCommand(upit, veza);
            SqlDataReader rd = cmd.ExecuteReader();
            if (rd.Read())
                izlaz = "CD se nalazi kod " + rd.GetString(0);
            rd.Close();
            return izlaz;
        }
        catch (SqlException ex)
        {
            throw ex;
        }
    }
}
