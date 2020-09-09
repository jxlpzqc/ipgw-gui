using System;
using System.Collections.Generic;
using System.Text;

namespace NEU.IPGateway.UI.Languages
{
    static class I18NStringUtil
    {
        public static string GetString(string key)
        {
            var str = App.Current.Resources[key] as string;
            if (str == null) return "";
            else return str;
        }
    }
}
