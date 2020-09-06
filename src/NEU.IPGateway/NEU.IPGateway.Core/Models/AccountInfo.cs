using System;
using System.Collections.Generic;
using System.Text;

namespace NEU.IPGateway.Core.Models
{
    public class AccountInfo
    {
        public string Name { get; set; }
        public string StudentNo { get; set; }
        public string Plan { get; set; }
        public TimeSpan UsedTime { get; set; }
        public int Times { get; set; }
        public double RemainMoney { get; set; }
        public string Status { get; set; }

    }
}
