<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PrintEstimateComparison.aspx.cs" Inherits="Metricon.Silverlight.MetriconRetailSystem.Host.PrintEstimateComparison" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
    body 
    {
        font-family:Tahoma, Verdana, Arial;
        font-size: 12px;
    }
    .ComparisonTable
    {
        border-color:#6699CC;
        border-width:0 0 1px 1px;
        border-style:solid;
    }
    td
    {
        border-color:#6699CC;
        border-width:1px 1px 0 0;
        border-style:solid;
        padding:4px;
        margin:0;
    }
    .SummaryTd
    {
        border-style:none;
    }
    .TableHeader
    {
        background-color:#AFEEEE;
        text-align:center;
    }
    .SourceRevision
    {
        background-color:#F0F8FF;
    }
    .DestinationRevision
    {
        background-color:#F5F5DC;
    }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <table border="0">
        <tr>
            <td colspan="2" class="SummaryTd"><b>Estimate 100 Comparison</b>&nbsp;&nbsp;&nbsp;&nbsp;Source: Revision 1(SC)&nbsp;&nbsp;&nbsp;&nbsp;Destination:Revision 2(SA)<br /></td>
        </tr>
        <tr>
            <td class="SummaryTd">Customer:</td>
            <td class="SummaryTd">&nbsp;</td>
        </tr>
        <tr>
            <td class="SummaryTd">Correspondence Address:</td>
            <td class="SummaryTd">&nbsp;</td>
        </tr>
        <tr>
            <td class="SummaryTd">Lot Address:</td>
            <td class="SummaryTd">&nbsp;</td>
        </tr>
        <tr>
            <td class="SummaryTd">House and Land Package:</td>
            <td class="SummaryTd">&nbsp;</td>
        </tr>
        <tr>
            <td class="SummaryTd">House Name:</td>
            <td class="SummaryTd">&nbsp;</td>
        </tr>
    </table>
    <br />
    <table class="comparisonTable" cellpadding="0" cellspacing="0">
        <tr>
            <td colspan="3" class="TableHeader">Estimate Header</td>
        </tr>
        <tr>
            <td width="250" class="TableHeader">Field</td>
            <td width="250" class="TableHeader">Revision 1(SC)</td>
            <td width="250" class="TableHeader">Revision 2(SA)</td>
        </tr>
        <tr>
            <td>Price Effective Date</td>
            <td class="SourceRevision">XX/XX/XXXX</td>
            <td class="DestinationRevision">XX/XX/XXXX</td>
        </tr>
        <tr>
            <td>Home Price</td>
            <td class="SourceRevision">$XXX,XXX.XX</td>
            <td class="DestinationRevision">$XXX,XXX.XX</td>
        </tr>
        <tr>
            <td>Upgrade Value</td>
            <td class="SourceRevision">$XX,XXX.XX</td>
            <td class="DestinationRevision">$XX,XXX.XX</td>
        </tr>
        <tr>
            <td>Site Work Value</td>
            <td class="SourceRevision">$X,XXX.XX</td>
            <td class="DestinationRevision">$X,XXX.XX</td>
        </tr>
        <tr>
            <td>Total Price</td>
            <td class="SourceRevision">$XXX,XXX.XX</td>
            <td class="DestinationRevision">$XXX,XXX.XX</td>
        </tr>
        <tr>
            <td>Estimate Status</td>
            <td class="SourceRevision">Accepted</td>
            <td class="DestinationRevision">Rejected</td>
        </tr>
        <tr>
            <td>Owner</td>
            <td class="SourceRevision">XXXXX XXXXXXX</td>
            <td class="DestinationRevision">XXXXX XXXXXXX</td>
        </tr>
    </table> 
    <br />
    <table class="comparisonTable" cellpadding="0" cellspacing="0">
        <tr>
            <td colspan="9" class="TableHeader">Estimate Details</td>
        </tr>
        <tr>
            <td colspan="4" class="TableHeader">Revision</td>
            <td colspan="4" class="TableHeader">Revision</td>
            <td>&nbsp;</td>
        </tr>
        <tr>
            <td class="SourceRevision" width="200">Product</td>
            <td class="SourceRevision" width="50">UOM</td>
            <td class="SourceRevision" width="50">Qty</td>
            <td class="SourceRevision" width="100">Price</td>
            <td class="DestinationRevision" width="200">Product</td>
            <td class="DestinationRevision" width="50">UOM</td>
            <td class="DestinationRevision" width="50">Qty</td>
            <td class="DestinationRevision" width="100">Price</td>
            <td width="75">Changes</td>
        </tr>
    </table> 
    </div>
    </form>
</body>
</html>
