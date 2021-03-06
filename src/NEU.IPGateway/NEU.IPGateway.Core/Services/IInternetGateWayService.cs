﻿using NEU.IPGateway.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NEU.IPGateway.Core.Services
{
    public interface IInternetGatewayService
    {
        Task<bool> Connect(string username,string password);
        Task<bool> Disconnect(string username,string password);
        Task<bool> Disconnect();
        Task<bool> ForceDisconnect(string username, string password);

        Task<(bool connected,bool logedin)> Test();
        Task<AccountInfo> GetAccountInfo();
        Task<string> GetDriverVersion();
    }
}
