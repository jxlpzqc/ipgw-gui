using NEU.IPGateway.Core.Models;
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
    public class Service : IInternetGatewayService
    {
        public string IPGWFullName { get; set; }

        public int timeout = 15000;

        public Service(string ipgwFullname)
        {
            if (!File.Exists(ipgwFullname))
            {
                throw new Exception("驱动加载错误：IPGW程序不存在");
            }

            IPGWFullName = ipgwFullname;

        }

        public Service()
            : this(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "ipgw.exe"))
        {
        }


        private async Task<(string stdout,string stderr)> GetResult(string args)
        {
            var p = Process.Start(new ProcessStartInfo
            {
                FileName = IPGWFullName,
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.Default,
                StandardErrorEncoding = Encoding.Default
            });
            var isExit = await Task.Run(() => p.WaitForExit(timeout));
            if (isExit)
            {

                return (await p.StandardOutput.ReadToEndAsync(), await p.StandardError.ReadToEndAsync());
                
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
            if (result.stdout.Contains("登陆成功"))
            {
                return true;
            }
            else
            {
                if (result.stderr.Contains("学号或密码错误 请重试"))
                    throw new IPGWException(Core.ConnectionError.InvalidCredient);
                else
                    throw new IPGWException(result.stderr);
            }
        }

        public async Task<bool> Disconnect(string username, string password)
        {
            var result = await GetResult($"logout -u {username} -p {password}");
            if (result.stdout.Contains("登出成功"))
            {
                return true;
            }
            else
            {
                if (result.stderr.Contains("学号或密码错误 请重试"))
                    throw new IPGWException(Core.ConnectionError.InvalidCredient);
                else
                    throw new IPGWException(result.stderr);
            }
        }

        public async Task<bool> Disconnect()
        {
            var result = await GetResult($"logout");
            if (result.stdout.Contains("登出成功"))
            {
                return true;
            }
            else
            {
                throw new IPGWException(result.stderr);
            }
        }

        public async Task<bool> ForceDisconnect(string username, string password)
        {
            var results = (await GetResult("list -d")).stdout.Split('\n').Skip(6);
            foreach (var result in results)
            {
                var sid = result.Split(' ').Where(u => !string.IsNullOrEmpty(u)).Last();
                var pstr = (await GetResult($"kick {sid}")).stdout;
                if (!pstr.Contains("强制下线成功"))
                    throw new IPGWException("强制下线出现异常，请重试");
            }
            return true;
        }


        public async Task<(bool connected, bool logedin)> Test()
        {
            var results = await GetResult("test");
            bool c, l;

            c = results.stdout.Contains("已连接校园网");            
            l = results.stdout.Contains("已登陆校园网");

            return (c, l);
        }

        private string GetLabelInfo(string str)
        {
            return str.Split('\t',' ').Where(u => !string.IsNullOrEmpty(u)).Last();
        }

        public async Task<AccountInfo> GetAccountInfo()
        {
            var (stdout,_) = await GetResult("list -i");
            var lines = stdout.Split('\n');
            var usedDataStr = GetLabelInfo(lines[7]);
            var usedData = double.Parse(usedDataStr.Substring(0, usedDataStr.Length - 1));
            var moneyStr = GetLabelInfo(lines[10]);
            var money = double.Parse(moneyStr.Substring(0, moneyStr.Length - 1));
            var timeStr = lines[8];
            var timelist = timeStr.Split(' ', '\t', '时', '分', '秒').Where(u => !string.IsNullOrEmpty(u)).ToList();
            var timelistLen = timelist.Count;
            TimeSpan time = new TimeSpan(
                int.Parse(timelist[timelistLen - 3]),
                int.Parse(timelist[timelistLen - 2]),
                int.Parse(timelist[timelistLen - 1])
            );

            return new AccountInfo
            {
                Name = GetLabelInfo(lines[2]),
                StudentNo = GetLabelInfo(lines[3]),
                Plan = GetLabelInfo(lines[6]),
                UsedData = usedData,
                Times = int.Parse(GetLabelInfo(lines[9])),
                RemainMoney = money,
                UsedTime = time
            };

        }
    }
}
