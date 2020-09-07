using NEU.IPGateway.Core.Models;
using NEU.IPGateway.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NEU.IPGateway.UI.Services
{
    public class SettingStorageService : ISettingStorageService
    {
        public async Task<Setting> ReadSetting()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Setting));
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var path = Path.Combine(basePath, "ipgw", "settings.xml");

            return await Task.Run(() =>
            {
                using (var stream = File.Open(path,FileMode.Open,FileAccess.Read, FileShare.ReadWrite))
                {
                    return serializer.Deserialize(stream) as Setting;
                }
            });

        }

        public async Task SaveSetting(Setting setting)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Setting));

            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var path = Path.Combine(basePath, "ipgw", "settings.xml");

            await Task.Run(() =>
            {
                using (var stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    serializer.Serialize(stream, setting);
                    stream.Close();
                }
            });
        }
    }
}
