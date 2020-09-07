using NEU.IPGateway.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NEU.IPGateway.Core.Services
{
    public interface ISettingStorageService
    {
        Task SaveSetting(Setting setting);

        Task<Setting> ReadSetting();

    }
}
