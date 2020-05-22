using System;
using System.Text;

using Telerik.Windows.Controls;

namespace Metricon.Silverlight.MetriconRetailSystem
{
    public static class ExceptionHandler
    {
        public static void PopUpErrorMessage(Exception ex, string source)
        {
            StringBuilder message = new StringBuilder();

            message.Append("An error occurred in " + source + ".\r\n");
            message.Append("Time: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + ".\r\n");
            message.Append(ex.Message);

            RadWindow.Alert(message.ToString());
        }
    }
}
