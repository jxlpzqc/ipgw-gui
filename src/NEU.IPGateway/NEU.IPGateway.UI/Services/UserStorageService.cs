using NEU.IPGateway.Core.Models;
using NEU.IPGateway.Core.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NEU.IPGateway.UI.Services
{
    public class UserStorageService : IUserStorageService
    {
        #region Private Methods

        private const string PRIVATE_KEY = "EIJJnmkenfkeijfieWLKjifewjfkewjfkewjfwekjkefwjke";

        private string GetUserDBPath()
        {
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var path = Path.Combine(basePath, "ipgw", "db");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        private string GetHashString(string str)
        {
            var sha1 = SHA1.Create();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(str));
            return BitConverter.ToString(hash).Replace("-", "");

        }

        private string GetUserDBFilename(string username)
        {
            var filename = GetHashString(username);
            var f = Path.Combine(GetUserDBPath(), filename + ".IPGWUSERDAT");
            return f;
        }

        private string ToPlainStr(string src)
        {
            return Regex.Replace(src, "[^A-Za-z0-9]", "");
        }

        private byte[] Encrypt(string userData, string pin)
        {

            return ProtectedData.Protect(Encoding.UTF8.GetBytes(userData), Encoding.UTF8.GetBytes(pin), DataProtectionScope.CurrentUser);

        }

        private string Decrypt(byte[] data, string pin)
        {

            return Encoding.UTF8.GetString(ProtectedData.Unprotect(data, Encoding.UTF8.GetBytes(pin), DataProtectionScope.CurrentUser));

        }

        private void WriteToFile(User user, string pin)
        {
            var username = ToPlainStr(user.Username);
            var password = ToPlainStr(user.Password);

            var encryptedPassword = BitConverter.ToString(Encrypt(password, pin)).Replace("-", "");


            var userdatastr = $"{username}\n{encryptedPassword}\n";

            var hash = GetHashString(userdatastr);

            userdatastr += hash;

            var filedata = Encrypt(userdatastr, PRIVATE_KEY);

            using (var file = File.Open(GetUserDBFilename(user.Username), FileMode.OpenOrCreate, FileAccess.Write))
            {
                file.Write(filedata, 0, sizeof(byte) * filedata.Length);
            }
        }

        private User ReadFromFile(string filename)
        {
            var data = new List<byte>();
            using (var file = File.Open(filename, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(file))
                {
                    byte[] bytes = new byte[1024];
                    while (reader.Read(bytes, 0, 1024) != 0)
                    {
                        data.AddRange(bytes);
                    }
                }
            }
            var databytes = data.ToArray();

            var userdatastr = Decrypt(databytes, PRIVATE_KEY);

            var userdata = userdatastr.Split('\n');

            // 校验文件
            var srcStr = string.Join("\n", userdata.Take(userdata.Length - 1)) + "\n";
            var hash = userdata.Last();
            var computedHash = GetHashString(srcStr);
            if (hash != computedHash)
                throw new Exception($"{filename} 用户数据库被篡改");

            var username = userdata[0];

            var encryptedPassword = userdata[1];

            return new User
            {
                Username = username,
                EncryptedPassword = encryptedPassword
            };
        }

        private User ReadFromUsername(string username)
        {
            return ReadFromFile(GetUserDBFilename(username));
        }

        private async Task<User> SetFirstUserAsDefault()
        {
            var first = (await GetUsers()).First();
            if (first != null)
            {
                await SetDefaultUser(first.Username);
            }
            return first;

        }

        #endregion

        public async Task<bool> CheckUserPinExist(string username)
        {
            return !await CheckUserPinValid(username, "");
        }

        public async Task<bool> CheckUserPinValid(string username, string sourcePin)
        {
            try
            {
                await DecryptedUserPassword(username, sourcePin);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> DecryptedUserPassword(string username, string pin)
        {
            return await Task.Run(() =>
            {

                var user = ReadFromUsername(username);
                var enPassword = user.EncryptedPassword;

                var enPasswordBytes = new byte[enPassword.Length / 2];
                for (int i = 0; i < enPassword.Length / 2; i++)
                {
                    var bStr = enPassword.Substring(i * 2, 2);
                    enPasswordBytes[i] = Convert.ToByte(bStr, 16);
                }

                var password = Decrypt(enPasswordBytes, pin);

                return password;

            });
        }

        public async Task<bool> DeleteUser(string username)
        {
            return await Task.Run(() =>
            {
                try
                {
                    File.Delete(GetUserDBFilename(username));
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        public async Task<User> GetDefaultUser()
        {
            var defaultFile = Path.Combine(GetUserDBPath(), "default.txt");
            if (!File.Exists(defaultFile)) return await SetFirstUserAsDefault();

            var filename = File.ReadAllText(defaultFile);
            if (!File.Exists(filename)) return await SetFirstUserAsDefault();

            return await Task.Run(() => ReadFromFile(filename));

        }

        public async Task<User> GetUser(string username)
        {
            return await Task.Run(() => ReadFromUsername(username));
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            return await Task.Run(() => Directory.GetFiles(GetUserDBPath(), "*.IPGWUSERDAT").Select((u) => ReadFromFile(u)));   
        }

        public async Task<bool> ResetUserPassword(string username, string newpassword,string pin)
        {
            return await SaveUser(username, newpassword, pin);
        }

        public async Task<bool> ResetUserPin(string username, string oldPin, string newPin)
        {
            var user = await Task.Run(() => ReadFromUsername(username));
            string password = "";
            try
            {
                password = await DecryptedUserPassword(username, oldPin);
            }
            catch
            {
                throw new Exception("输入的PIN无效");
            }
            return await SaveUser(username, password, newPin);
        }

        public async Task<bool> SaveUser(string username, string password, string pin = "")
        {
            try
            {
                await Task.Run(() =>
                {
                    WriteToFile(new User
                    {
                        Username = username,
                        Password = password,
                    }, pin);
                });
                return true;
            }
            catch(Exception  ex)
            {
                return false;
            }
        }

        public async Task<bool> SetDefaultUser(string username)
        {
            var defaultFile = Path.Combine(GetUserDBPath(), "default.txt");

            try 
            {
                await Task.Run(() =>
                {
                    File.WriteAllText(defaultFile, GetUserDBFilename(username));
                });
                return true;
            }
            catch
            {
                return false;
            }



        }
    }
}
