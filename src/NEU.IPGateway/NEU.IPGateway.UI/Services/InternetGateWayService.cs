using NEU.IPGateWay.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NEU.IPGateway.UI.Services
{
    class InternetGateWayService : IInternetGateWayService
    {
        public async Task<bool> Connect(string username, string password)
        {
            await Task.Delay(2000);
            return true;
        }

        public async Task<bool> Disconnect()
        {

            await Task.Delay(2000);
            return true;
        }

        public async Task<bool> ForceConnect(string username, string password)
        {

            await Task.Delay(2000);
            return true;
        }

        public async Task<bool> ForceDisconnect(string username, string password)
        {

            await Task.Delay(2000);
            return true;
        }
    }
}
