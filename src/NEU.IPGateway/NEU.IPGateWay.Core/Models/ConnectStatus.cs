using System;
using System.Collections.Generic;
using System.Text;

namespace NEU.IPGateway.Core.Models
{
    /// <summary>
    /// 一个表示和网关连接状态的枚举
    /// </summary>
    public enum ConnectStatus
    {
        /// <summary>
        /// 未连接
        /// </summary>
        Disconnected,
        /// <summary>
        /// 正在连接
        /// </summary>
        Connecting,
        /// <summary>
        /// 正在断开连接
        /// </summary>
        Disconnecting,
        /// <summary>
        /// 已连接
        /// </summary>
        Connected,
        /// <summary>
        /// 未知状态或正在检测连接状态
        /// </summary>
        Checking,
        /// <summary>
        /// 未连接到网络
        /// </summary>
        DisconnectedFromNetwork
    }
}
