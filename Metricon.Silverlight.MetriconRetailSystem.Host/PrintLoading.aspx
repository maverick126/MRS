<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PrintLoading.aspx.cs" Inherits="Metricon.Silverlight.MetriconRetailSystem.Host.PrintLoading" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Loading...</title>
    <style type="text/css">
    body {
        margin:0;
        padding:0;
        background-color:#E8E8E8;
    }
    </style>
    <script type="text/javascript">
        function Redirect() 
        {
           var para="<%=Request.QueryString["type"]%>";
           var includestd="<%=Request.QueryString["includestd"]%>";
           if (para=="studiom")
           {
              window.location.href = "PrintEstimateStudioMTemplate.aspx?merge=<%=Request.QueryString["merge"]%>&type=<%=Request.QueryString["type"]%>&EstimateRevisionId=<%=Request.QueryString["EstimateRevisionId"]%>";
           }
           else
           {
               window.location.href = "PrintEstimateNewTemplate.aspx?merge=<%=Request.QueryString["merge"]%>&type=<%=Request.QueryString["type"]%>&version=<%=Request.QueryString["version"]%>&EstimateRevisionId=<%=Request.QueryString["EstimateRevisionId"]%>&IncludeProductNameAndCode=<%=Request.QueryString["IncludeProductNameAndCode"]%>&IncludeUOMAndQuantity=<%=Request.QueryString["IncludeUOMAndQuantity"]%>&includeSpecifications=<%=Request.QueryString["includeSpecifications"]%>&includestd=<%=Request.QueryString["includestd"]%>";
           }


        }
    </script>
</head>
<body onload="Redirect();">
    <form id="form1" runat="server">
        <telerik:RadScriptManager ID="RadScriptManager1" Runat="server" />
        <div style="width:100%; text-align:center;">
            <br/>
            <br/>
            <br/>
            <br/>
            <br/>
            <br/>
            <br/>
            <img alt="Generating PDF..." src='<%= RadAjaxLoadingPanel.GetWebResourceUrl(Page, "Telerik.Web.UI.Skins.Default.Ajax.loading.gif") %>' style="border:0;" />
        </div>
    </form>
</body>
</html>
