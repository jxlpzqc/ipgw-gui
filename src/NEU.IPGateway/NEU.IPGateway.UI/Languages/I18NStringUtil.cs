using System;
using System.Collections.Generic;
using System.Text;

namespace NEU.IPGateway.UI.Languages
{
    static class I18NStringUtil
    {
        public static string GetString(string key)
        {
            return App.Current.Resources[key] as string;
        }
    }
}
