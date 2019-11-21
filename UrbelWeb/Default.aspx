<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
    <style type="text/css">
        .style1
        {
            width: 239px;
        }
    </style>
    <link href="Style.css" rel="stylesheet" />
    <link href="Styles/bootstrap.min.css" rel="stylesheet" />
    <script src="Scripts/bootstrap.min.js"></script>
</head>
<script language="javascript">
    function Vidljivo()
     {
        document.getElementById("uputstvo").style.visibility="visible";
    }
    function Nevidljivo()
    {
        document.getElementById("uputstvo").style.visibility="hidden";
    }
    
</script>
<body>
    
    <form id="form1" runat="server">
    <div id="divHeader" class="row">
        
        <div align="left" class="col-lg-3">
        <img src="Grb-grada-250x300pix.jpg" style="height: 130px; width: 111px" />
        </div>
        
        <div align="center" class="col-ig-10">
        <h3>ODELJENJE ZA DOKUMENTACIJU</h3>
        <h5>Pretraga uz pomoć kartografskog broja</h5>
        <span id="uputstvo" style="color:Red; visibility:hidden">Ukucajte kartografski broj predmeta i pritisnite taster ENTER</span>
        </div>
        
   </div>
        <div id="divUnos" class="row">
        <hr /><br />
        <div onmouseover="Vidljivo()" onmouseout="Nevidljivo()" class="col-ig-12">
        <strong>Kartografski broj:</strong>
        <asp:TextBox ID="txtLart" runat="server" AutoPostBack="true" OnLoad="txtLart_Load" OnTextChanged="txtLart_TextChanged" /> 
        </div>
        </div>
        <hr /><br />
        <div id="divRezultat" class="row">
            <div class="col-md-auto" align="center">
                <asp:Literal ID="lit" runat="server" />
            </div>
        </div>
    
    </form>
 <footer>
     <div id="divFooter">
  <p>Posted by: Hege Refsnes</p>
  <p>Contact information: <a href="mailto:someone@example.com">
  someone@example.com</a>.</p>
</div>
</footer>
  
</body>
</html>
