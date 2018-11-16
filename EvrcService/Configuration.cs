using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace EvrcService
{    /// <summary>
    /// 系统配置类
    /// </summary>
    [Serializable]
    public class Configuration
    {
        //登录账号
        public string User;
        //登录密码
        public string Password;
        //ip地址
        public string Ip;
        //TCP端口
        public string TcpPort;
        //UDP端口
        public string UdpPort;
        //调试开关
        public string DebugSwitch;

        //本服务用地址
        public string PublishHost;
        //本服务用端口
        public string PublishPort;
    }

    public class Cmd
    {
        public string Command;
    }

    public class Rst
    {
        public string Result;
    }

    public struct PocRunPar
    {
        public byte UdpHbCnt;
        public byte TcpHbCnt;
    }

    public class ServerMsg
    {
        public string uid;
        public string username;
        public string defaultgroup;
        public List<GroupMsg> groups;
    }

    public class GroupMsg
    {
        public string gid;
        public string gname;
        public string numbers;
        public List<UserMsg> users;
    }

    public class UserMsg
    {
        public string uid;
        public string online;
        public string uname;
    }
}
