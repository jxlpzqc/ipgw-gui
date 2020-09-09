using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NEU.IPGateway.UI.Utils
{
    static class HyperlinkUtil
    {
        public static void Open(string url)
        {

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/C start " + url,
                UseShellExecute = false,
                CreateNoWindow = true
            });
        
        }


    }
}
