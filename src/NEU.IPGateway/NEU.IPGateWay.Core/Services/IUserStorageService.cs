using NEU.IPGateWay.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NEU.IPGateWay.Core.Services
{
    public interface IUserStorageService
    {
        Task<bool> SaveUser(string username, string password, string pin = "");
        Task<User> GetUser(string username);
        
        Task<User> GetDefaultUser();
        Task<bool> SetDefaultUser(string username);

        Task<IEnumerable<User>> GetUsers();
        Task<bool> DeleteUser(string username);
        Task<bool> ResetUserPassword(string username, string newpassword,string pin="");
        Task<string> DecryptedUserPassword(string username, string pin);

        // PIN 操作相关方法
        Task<bool> ResetUserPin(string username, string oldPin, string newPin);
        Task<bool> CheckUserPinExist(string username);
        Task<bool> CheckUserPinValid(string username, string sourcePin);

    }
}
