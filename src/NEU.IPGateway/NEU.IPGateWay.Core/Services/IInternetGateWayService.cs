using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NEU.IPGateway.Core.Services
{
    public interface IInternetGateWayService
    {
        Task<bool> Connect(string username,string password);
        Task<bool> Disconnect();
        Task<bool> ForceDisconnect(string username, string password);
        Task<bool> ForceConnect(string username, string password);
        

    }
}
