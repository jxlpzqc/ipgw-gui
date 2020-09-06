using NEU.IPGateway.Core.Services;
using Splat;
using System;
using System.Collections.Generic;
using System.Text;

namespace NEU.IPGateway.Driver
{
    public static class Register
    {
        public static void Regist(string ipgwFullname)
        {
            Locator.CurrentMutable.Register<IInternetGatewayService>(() => new Service(ipgwFullname));
        }

        public static void Regist()
        {
            Locator.CurrentMutable.Register<IInternetGatewayService>(() => new Service());
        }
    }
}
