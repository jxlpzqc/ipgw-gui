using NEU.IPGateway.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEU.IPGateway.Driver
{
    public class Service : IInternetGateWayService
    {
        public string IPGWFullName { get; set; }

        public int timeout = 15000;

        public Service(string ipgwFullname)
        {
            IPGWFullName = ipgwFullname;

        }

        public Service()
            : this(Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location, "ipgw.exe"))
        {
        }


        private async Task<string> GetResult(string args)
        {
            var p = Process.Start(new ProcessStartInfo
            {
                FileName = IPGWFullName,
                Arguments = args,
                CreateNoWindow = false
            });
            var isExit = await Task.Run(() => p.WaitForExit(timeout));
            if (isExit)
            {
                if(p.ExitCode == 0)
                {
                    return await p.StandardOutput.ReadToEndAsync();
                }
                else
                {
                    throw new IPGWException("驱动程序异常退出！");
                }
            }
            else
            {
                p.Kill();
                throw new IPGWException("驱动程序运行超时！");
            }

        }

        public async Task<bool> Connect(string username, string password)
        {
            var result = await GetResult($"login -u {username} -p {password}");
            if (result.Contains("登陆成功"))
            {
                return true;
            }
            else
            {
                if (result.Contains("学号或密码错误 请重试"))
                    throw new IPGWException(Core.ConnectionError.InvalidCredient);
                else
                    throw new IPGWException(result.Split('\n')[1]);
            }
        }

        public async Task<bool> Disconnect(string username, string password)
        {
            var result = await GetResult($"logout -u {username} -p {password}");
            if (result.Contains("登出成功"))
            {
                return true;
            }
            else
            {
                if (result.Contains("学号或密码错误 请重试"))
                    throw new IPGWException(Core.ConnectionError.InvalidCredient);
                else
                    throw new IPGWException(result.Split('\n')[1]);
            }
        }

        public async Task<bool> Disconnect()
        {
            var result = await GetResult($"logout");
            if (result.Contains("登出成功"))
            {
                return true;
            }
            else
            {
                throw new IPGWException(result.Split('\n')[1]);
            }
        }

        public async Task<bool> ForceDisconnect(string username, string password)
        {
            var results = (await GetResult("list -d")).Split('\n').Skip(6);
            foreach (var result in results)
            {
                var sid = result.Split(' ').Where(u => !string.IsNullOrEmpty(u)).Last();
                var pstr = await GetResult($"kick {sid}");
                if (!pstr.Contains("强制下线成功"))
                    throw new IPGWException("强制下线出现异常，请重试");
            }
            return true;
        }


        public async Task<(bool connected, bool logedin)> GetInfo()
        {
            var results = await GetResult("test");
            bool c, l;

            if (results.Contains("已连接校园网"))
                c = true;
            else
                c = false;

            if (results.Contains("已登陆校园网"))
                l = true;
            else
                l = false;

            return (c, l);
        }
    }
}
