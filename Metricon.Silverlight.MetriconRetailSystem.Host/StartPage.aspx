<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="StartPage.aspx.cs" Inherits="Metricon.Silverlight.MetriconRetailSystem.Host.StartPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script type="text/javascript">
        function openWindow() {
            window.opener = top;
            window.open('', '_self', '');
            mywindow = window.open('Default.aspx', '', 'toolbar=0,status=1,location=no,menubar=no,directories=no,scrollbars=yes,resizable=yes,screenX=0,screenY=0,left=0,top=0,width=' + (screen.availWidth - 12) + ',height=' + (screen.availHeight - 55));
            if (mywindow != null) {
                mywindow.moveTo(0,0);
                window.close();
            }
        }

        function openWindowFromSQS() {
            window.opener = top;
            window.open('', '_self', '');
            mywindow = window.open('Default.aspx#/MainPage.xaml', '', 'toolbar=0,status=1,location=no,menubar=no,directories=no,scrollbars=yes,resizable=yes,screenX=0,screenY=0,left=0,top=0,width=' + (screen.availWidth - 12) + ',height=' + (screen.availHeight - 55));
            if (mywindow != null) {
                mywindow.moveTo(0, 0);
                window.close();
            }
        } 
    </script>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    
    </div>
    </form>
</body>
</html>
