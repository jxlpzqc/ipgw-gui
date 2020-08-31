using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace NEU.IPGateWay.Core.Models
{
    public class User: ReactiveObject
    {
        [Reactive]
        public string Username { get; set; }

        [Reactive]
        public string EncryptedPassword { get; set; }

        [Reactive]
        public string Password { get; set; }

        [Reactive]
        public string AvatorUri { get; set; }

    }
}
