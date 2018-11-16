using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Timers;
using Newtonsoft.Json;

namespace EvrcService
{
    public class Kernel
    {
        #region 变量区

        private readonly string IP = "", Usr = "", Pwd = "", debugswitch = "";
        private readonly int POC_NET_TCP_PORT = 2074;
        private readonly int POC_NET_UDP_PORT = 2075;
        private int POC_GET_IP_PORT = 3074;
        private int POC_NET_TCP_SOCKET = 0;
        private int POC_NET_UDP_SOCKET = 1;
        private int TCP_SND_SIZE = 1024;
        private int TCP_REC_SIZE = 1024000;
        private int TCP_MAX_PACK_SIZE = 20480;
        private int UDP_BUF_SIZE = 300;
        private PocRunPar gRunPar;
        private EvrcProtocal.stUser user;
        private Socket _clientSocket;
        private Socket _voiceSocket;
        private EvrcProtocal.stUser PubUser;
        /// <summary>
        /// 音频格式
        /// </summary>
        private PcmAudio.WaveFormatEx lpFormat, lpFormatPlay;
        private static int MAXRECBUFFER = 24 * 15;
        private static int RECBUFFER = 320;
        private PcmAudio.WaveHdr[] globalhdr = new PcmAudio.WaveHdr[MAXRECBUFFER];
        /// <summary>
        /// 标识当前是否在录制音频
        /// </summary>
        private bool IsRecoding = false;
        /// <summary>
        /// 音频输入设备句柄
        /// </summary>
        private IntPtr hRecord;
        /// <summary>
        /// 播放句柄
        /// </summary>
        private IntPtr hPlay;
        /// <summary>
        /// 音频帧长度
        /// </summary>
        private static readonly int AUDIOPACKLENGTH = 23;
        /// <summary>
        /// 音频流缓存区
        /// </summary>
        private byte[,] EvrcBuff = new byte[11, AUDIOPACKLENGTH];
        /// <summary>
        /// 音频流行计算偏移量
        /// </summary>
        private int EvrcPtr = 0;
        private System.Timers.Timer tcptimer, udptimer;
        private PcmAudio.WaveDelegate _wd, _wdPlay;
        #endregion

        public Kernel(Configuration config)
        {
            IP = config.Ip;
            Usr = config.User;
            Pwd = config.Password;
            POC_NET_TCP_PORT = int.Parse(config.TcpPort);
            POC_NET_UDP_PORT = int.Parse(config.UdpPort);
            debugswitch = config.DebugSwitch;
            _wd = WaveInProc;
        }

        /// <summary>
        /// 开始喊话
        /// </summary>
        private void StartCollectSound()
        {
            try
            {
                if (IsRecoding) return;

                ////初始化音频编码解码
                //EvrcProtocal.InitEncoder();
                //EvrcProtocal.InitDecoder();
                EvrcProtocal.StartSpeech();
                var rtn = 0;
                hRecord = IntPtr.Zero;
                rtn = PcmAudio.waveInOpen(out hRecord, -1, ref lpFormat, _wd, 0, 0x00030000);

                if (rtn == 0)//成功返回0
                {
                    //将准备好的buffer提供给音频输入设备
                    for (int i = 0; i < MAXRECBUFFER; i++)
                    {
                        //准备一个bufrer给输入设备
                        PcmAudio.waveInPrepareHeader(hRecord, ref globalhdr[i], (UInt32)Marshal.SizeOf(typeof(PcmAudio.WaveHdr)));
                        //发送一个buffer给指定的输入设备，当buffer填满将会通知程序
                        PcmAudio.waveInAddBuffer(hRecord, ref globalhdr[i], (UInt32)Marshal.SizeOf(typeof(PcmAudio.WaveHdr)));
                    }
                    //开启指定的输入采集设备
                    rtn = PcmAudio.waveInStart(hRecord);
                    if (rtn == 0)
                    {
                        IsRecoding = true;
                        //ShowMsg("正在采集音频数据...");
                    }
                    else
                    {
                        //ShowMsg("采集音频失败");
                    }
                }
            }
            catch (Exception ex)
            {
                //ShowMsg(ex.Message);
            }
        }

        /// <summary>
        /// 结束喊话
        /// </summary>
        private void StopCollectSound()
        {
            try
            {
                if (!IsRecoding) return;
                //停止音频采集
                var rtn = PcmAudio.waveInStop(hRecord);
                if (rtn == 0) //停止采集成功，立即重置设备,重置设备将会导致所有的buffer反馈给程序
                {
                    IsRecoding = false;
                    //ShowMsg("停止采集音频数据。");
                    rtn = PcmAudio.waveInReset(hRecord);  //重置设备
                }
                Thread.Sleep(500);
                if (rtn == 0) //重置设备成功，立即关闭设备
                {
                    rtn = PcmAudio.waveInClose(hRecord); //关闭设备
                }
                //GC.Collect();
            }
            catch (Exception ex)
            {
                //ShowMsg(ex.Message);
            }
        }

        /// <summary>
        /// CRC校验
        /// </summary>
        /// <param name="recvbuf"></param>
        /// <param name="recvlength"></param>
        /// <param name="vLen"></param>
        /// <returns></returns>
        private static byte[] CrcCheck(byte[] recvbuf, int recvlength, ref uint vLen)
        {
            uint uiValidLen = (((uint)recvbuf[0]) << 24) | (((uint)recvbuf[1]) << 16) |
                              (((uint)recvbuf[2]) << 8) | ((uint)recvbuf[3]);
            vLen = uiValidLen;
            if ((uiValidLen + 4) > recvlength || recvlength < 9)
            {
                return null;
            }
            var uiReCrc = (((uint)recvbuf[uiValidLen]) << 24) | (((uint)recvbuf[uiValidLen + 1]) << 16) |
                          (((uint)recvbuf[uiValidLen + 2]) << 8) | ((uint)recvbuf[uiValidLen + 3]);
            if (uiReCrc != EvrcProtocal.ZTE_CRC(ref recvbuf[0], uiValidLen))
            {
                return null;
            }
            return recvbuf;
        }

        public string TrimStr(string text)
        {
            var aaa = text.IndexOf("\0", StringComparison.Ordinal);
            return text.Substring(0, aaa);
        }

        private byte[] _sendbuff, _recvbuff;

        #region 音频处理

        /// <summary>
        /// waveInOpen的回调方法
        /// </summary>
        public void WaveInProc(IntPtr hwi, UInt32 uMsg, UInt32 dwInstance, UInt32 dwParam1, UInt32 dwParam2)
        {
            switch (uMsg)
            {
                case PcmAudio.MM_WIM_OPEN: break;
                case PcmAudio.MM_WIM_DATA:
                    unsafe
                    {
                        //var waveHdr = (PcmAudio.WaveHdr*)dwParam1;
                        var hdr = (PcmAudio.WaveHdr)Marshal.PtrToStructure((IntPtr)dwParam1, typeof(PcmAudio.WaveHdr));
                        OnSoundData(hdr);
                    }
                    break;
                case PcmAudio.MM_WIM_CLOSE: break;
            }
        }

        /// <summary>
        /// 喊话处理
        /// </summary>
        /// <param name="hdr"></param>
        private void OnSoundData(PcmAudio.WaveHdr hdr)
        {
            if (!IsRecoding) return;
            if (hdr.dwBytesRecorded == 0)
            {
                return;
            }
            //使采集过程，直到此buffer已经沾满，不能再填充
            PcmAudio.waveInUnprepareHeader(hRecord, ref hdr, (UInt32)Marshal.SizeOf(typeof(PcmAudio.WaveHdr)));
            //将采集到的声音发送给播放线程
            if (hdr.lpData != IntPtr.Zero)
            {
                DataProc(hdr.dwBytesRecorded, hdr.lpData);
            }
        }

        private void DataProc(uint br, IntPtr data)
        {
            //byte[] ucEvrc = new byte[AUDIOPACKLENGTH];
            //ucEvrc[0] = 3;//语音数据标记
            ////音频数据压缩，数据长度：输出EVRC 10  输入PCM 320，测试bug在此处后面
            //EvrcProtocal.evrc_encode(ref ucEvrc[1], ref data);//todo:原有Evrc.dll方法

            byte[] recv = new byte[23];
            var rtn = EvrcProtocal.CCSEncode(data);//todo:替换为CCS.dll方法
            Marshal.Copy(rtn, recv, 0, 23);

            //复制编码结果到音频流缓存区EvrcBuff中
            for (var i = 0; i < 11; i++)
            {
                EvrcBuff[EvrcPtr, i] = recv[i];
            }
            EvrcPtr++;
            //音频流缓存区EvrcBuff满则发UDP包至服务器
            if (EvrcPtr >= 10)
            {
                SendEvrcPacket(10);
                EvrcPtr = 0;//重置音频流行计算偏移量
            }
        }

        private static int _uiSeq = 0;
        private void SendEvrcPacket(byte ucFrameNum)
        {
            int i;
            int uiDataLen;
            //_uiSeq = 0;
            byte[] pPacket = new byte[300];
            _uiSeq++;
            pPacket[0] = 0xC0;
            pPacket[1] = 0x6F;
            pPacket[2] = (byte)(_uiSeq >> 8);
            pPacket[3] = (byte)_uiSeq;
            pPacket[4] = 0x00;
            pPacket[5] = 0x00;
            pPacket[6] = 0;
            pPacket[7] = 0;
            pPacket[8] = (byte)(user.uid >> 24);
            pPacket[9] = (byte)(user.uid >> 16);
            pPacket[10] = (byte)(user.uid >> 8);
            pPacket[11] = (byte)(user.uid);
            pPacket[12] = ucFrameNum;
            pPacket[13] = pPacket[14] = pPacket[15] = 0;
            uiDataLen = 16;
            for (i = 0; i < ucFrameNum; i++)
            {
                switch (EvrcBuff[i, 0])
                {
                    case 3://语音数据
                        pPacket[13 + (i >> 2)] |= (byte)(0x02 << (6 - (i % 4) * 2));
                        for (int j = 0; j < 10; j++)
                        {
                            pPacket[uiDataLen + j] = EvrcBuff[i, 1 + j];
                        }
                        uiDataLen += 10;
                        break;
                    case 4:
                        pPacket[13 + (i >> 2)] |= (byte)(0x03 << (6 - (i % 4) * 2));
                        for (int j = 0; j < 22; j++)
                        {
                            pPacket[uiDataLen + j] = EvrcBuff[i, 1 + j];
                        }
                        uiDataLen += 22;
                        break;
                    default:
                        break;
                }
            }
            //发UDP包
            SendUdpPacket(_voiceSocket, IP, POC_NET_UDP_PORT, pPacket, uiDataLen);
        }

        /// <summary>
        /// Socket发UDP包
        /// </summary>
        /// <param name="sock"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="buffer"></param>
        /// <param name="len"></param>
        private void SendUdpPacket(Socket sock, string ip, int port, byte[] buffer, int len)
        {
            var ipp = new IPEndPoint(IPAddress.Parse(ip), port);
            var snd = sock.SendTo(buffer, len, SocketFlags.None, ipp);
            //ShowMsg(DateTime.Now + "--send udp pack：" + snd);
        }

        #endregion

        /// <summary>
        /// 服务启动调用
        /// </summary>
        public void Init()
        {
            //以下初始化音频采集
            for (int i = 0; i < MAXRECBUFFER; i++)
            {
                var hdr = new PcmAudio.WaveHdr
                {
                    lpData = Marshal.AllocHGlobal(RECBUFFER),
                    dwBufferLength = (UInt32)RECBUFFER
                };
                globalhdr[i] = hdr;
            }

            lpFormat.wFormatTag = 1;//声音格式为PCM
            lpFormat.nChannels = 1;                   //采样声道数，对于单声道音频设置为1，立体声设置为2
            lpFormat.wBitsPerSample = 16;             //采样比特  16bits/次
            lpFormat.cbSize = 0;                      //一般为0
            lpFormat.nSamplesPerSec = 8000;           //采样率 8000 次/秒
            lpFormat.nBlockAlign = 2;                 //一个块的大小，采样bit的字节数乘以声道数
            lpFormat.nAvgBytesPerSec = 16000;           //每秒的数据率，就是每秒能采集多少字节的数据

            ////初始化音频格式结构体
            //lpFormatPlay.wFormatTag = 1;//声音格式为PCM
            //lpFormatPlay.nChannels = 1;
            //lpFormatPlay.wBitsPerSample = 16;
            //lpFormatPlay.cbSize = 0;
            //lpFormatPlay.nSamplesPerSec = 8000;
            //lpFormatPlay.nAvgBytesPerSec = 16000;
            //lpFormatPlay.nBlockAlign = 2;
        }

        private void Udptimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (_voiceSocket == null || !_voiceSocket.Connected)
                return;
            if (gRunPar.UdpHbCnt++ >= 4)
            {
                //以下发送心跳包
                var udpsndbuf = new byte[100];
                var uiSndLen = EvrcProtocal.proto_make_ping_packet((uint)user.uid, ref udpsndbuf[0]);//测试ping
                //ShowMsg("ping服务器：" + uiSndLen);
                if (uiSndLen > 0)
                {
                    SendUdpPacket(_voiceSocket, user.iSetPar.ip, POC_NET_UDP_PORT, udpsndbuf, uiSndLen);
                    //ShowMsg("已发送UDP心跳包。");
                }
                gRunPar.UdpHbCnt = 0;
            }
        }

        /// <summary>
        /// 服务关闭调用
        /// </summary>
        public void Release()
        {
            foreach (PcmAudio.WaveHdr hdr in globalhdr)
            {
                Marshal.FreeHGlobal(hdr.lpData);
            }
            globalhdr = null;
            GC.Collect();

            udptimer?.Stop();
            if (_voiceSocket != null && _voiceSocket.Connected)
            {
                //关闭Socket之前，首选需要把双方的Socket Shutdown掉
                _voiceSocket.Shutdown(SocketShutdown.Both);
                //Shutdown掉Socket后主线程停止10ms，保证Socket的Shutdown完成
                Thread.Sleep(10);
                //关闭客户端Socket,清理资源
                _voiceSocket.Close();
            }
            if (_clientSocket != null && _clientSocket.Connected)
            {
                //关闭Socket之前，首选需要把双方的Socket Shutdown掉
                _clientSocket.Shutdown(SocketShutdown.Both);
                //Shutdown掉Socket后主线程停止10ms，保证Socket的Shutdown完成
                Thread.Sleep(10);
                //关闭客户端Socket,清理资源
                _clientSocket.Close();
            }
        }

        private string Login()
        {
            try
            {
                //UDP socket
                _voiceSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
                {
                    ReceiveTimeout = 30 * 1000,
                    SendTimeout = 30 * 1000
                };
                _voiceSocket.Connect(new IPEndPoint(IPAddress.Parse(IP), POC_NET_UDP_PORT));
                //开始UDP心跳轮询
                udptimer = new System.Timers.Timer();
                udptimer.Elapsed += Udptimer_Elapsed;
                udptimer.Interval = 6 * 1000;
                udptimer.Enabled = true;
                udptimer.Start();

                //TCP socket
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    ReceiveTimeout = 30 * 1000,
                    SendTimeout = 30 * 1000
                };
                _clientSocket.Connect(new IPEndPoint(IPAddress.Parse(IP), POC_NET_TCP_PORT));

                //登录参数
                user = new EvrcProtocal.stUser
                {
                    iSetPar =
                    {
                        ip = IP,
                        account = Usr,
                        passwd = Pwd
                    }
                };

                //1、获取登录
                _sendbuff = new byte[1000];
                var loginpack = EvrcProtocal.proto_make_login_packet(ref user, ref _sendbuff[0]);
                _clientSocket.Send(_sendbuff, loginpack, SocketFlags.None);
                _recvbuff = new byte[1000];
                int len = _clientSocket.Receive(_recvbuff, 1000, SocketFlags.None);
                uint vlen = 0;
                byte[] checkedrtn = CrcCheck(_recvbuff, len, ref vlen);//CRC校验
                //登录解包（此处回填登录参数）
                var loginunpack = EvrcProtocal.proto_unpack_login_ack(ref user, ref checkedrtn[5], vlen - 5);
                var name = TrimStr(Encoding.UTF8.GetString(user.username));
                //ShowMsg("登陆解包res：" + (loginunpack != 0) + ";uid=" + user.uid + ";username=" + name + ";defaultgroup=" + user.default_group);
                var rtnmsg = new ServerMsg
                {
                    uid = user.uid.ToString(),
                    username = name,
                    defaultgroup = user.default_group.ToString(),
                    groups=new List<GroupMsg>()
                };

                //2、获取组
                _sendbuff = new byte[1000];
                var reqgroup = EvrcProtocal.proto_make_query_group_packet(ref _sendbuff[0]);
                //ShowMsg("获取组结果：" + reqgroup);
                _clientSocket.Send(_sendbuff, reqgroup, SocketFlags.None);
                _recvbuff = new byte[1000];
                len = _clientSocket.Receive(_recvbuff, 1000, SocketFlags.None);
                checkedrtn = CrcCheck(_recvbuff, len, ref vlen);//CRC校验
                //解包组
                var upkgroup = EvrcProtocal.proto_unpack_query_group_ack(ref checkedrtn[5], vlen - 5);
                //ShowMsg("获取组解包res：" + (upkgroup != 0));

                //0、进入群组
                _sendbuff = new byte[1000];
                var entg = EvrcProtocal.proto_make_enter_group_packet(user.default_group, ref _sendbuff[0]);
                //ShowMsg("进入群组结果："+entg);
                _clientSocket.Send(_sendbuff, entg, SocketFlags.None);
                _recvbuff = new byte[1000];
                len = _clientSocket.Receive(_recvbuff, 1000, SocketFlags.None);
                checkedrtn = CrcCheck(_recvbuff, len, ref vlen);//CRC校验
                //解包进入群组
                var entgack = EvrcProtocal.proto_unpack_enter_group_ack(ref checkedrtn[5], vlen - 5);
                //ShowMsg("进入群组解包res：" + (entgack != 0));

                //3、解析组
                var gl = EvrcProtocal.proto_get_group_list();
                var grouplist = (EvrcProtocal.stGroupList)Marshal.PtrToStructure(gl, typeof(EvrcProtocal.stGroupList));
                //ShowMsg("获取组结果：组数" + grouplist.uiTotal);
                var nowhead = grouplist.pHead;
                var now = (EvrcProtocal.stGroupNode)Marshal.PtrToStructure(nowhead, typeof(EvrcProtocal.stGroupNode));
                while (true)
                {
                    if (now == null) break;
                    var name1 = TrimStr(Encoding.UTF8.GetString(now.iGroup.gname));
                    //ShowMsg("解析组：gid=" + now.iGroup.gid + "；gname=" + name1 + "；number_n=" + now.iGroup.number_n);
                    GroupMsg gm = new GroupMsg
                    {
                        gid = now.iGroup.gid.ToString(),
                        gname = name1,
                        numbers = now.iGroup.number_n.ToString(),
                        users=new List<UserMsg>()
                    };

                    //4、根据组获取用户
                    _sendbuff = new byte[1000];
                    var mbr = EvrcProtocal.proto_make_query_member_packet(now.iGroup.gid, ref _sendbuff[0]);//now.iGroup.gid|user.default_group
                    _clientSocket.Send(_sendbuff, mbr, SocketFlags.None);
                    _recvbuff = new byte[1000];
                    len = _clientSocket.Receive(_recvbuff, 1000, SocketFlags.None);
                    checkedrtn = CrcCheck(_recvbuff, len, ref vlen);//CRC校验
                    var dembr = EvrcProtocal.proto_unpack_query_member_ack(ref checkedrtn[5], vlen - 5);
                    //ShowMsg("解包用户res：" + (dembr != 0));

                    var ml = EvrcProtocal.proto_get_user_list();
                    var mbrlist = (EvrcProtocal.stUserList)Marshal.PtrToStructure(ml, typeof(EvrcProtocal.stUserList));

                    //ShowMsg("获取用户结果：用户数" + mbrlist.uiTotal);
                    var head = mbrlist.pHead;
                    var mbrnow = (EvrcProtocal.stUserNode)Marshal.PtrToStructure(head, typeof(EvrcProtocal.stUserNode));
                    while (true)
                    {
                        if (mbrnow == null) break;
                        var name2 = TrimStr(Encoding.UTF8.GetString(mbrnow.uname));
                        //ShowMsg("解析用户：uid=" + mbrnow.uid + "；online=" + mbrnow.online + "；uname=" + name2);
                        UserMsg um = new UserMsg
                        {
                            uid = mbrnow.uid.ToString(),
                            online = mbrnow.online.ToString(),
                            uname = name2
                        };
                        gm.users.Add(um);
                        mbrnow = (EvrcProtocal.stUserNode)Marshal.PtrToStructure(mbrnow.pNext, typeof(EvrcProtocal.stUserNode));
                    }
                    rtnmsg.groups.Add(gm);
                    now = (EvrcProtocal.stGroupNode)Marshal.PtrToStructure(now.pNext, typeof(EvrcProtocal.stGroupNode));
                }
                PubUser = user;
                return JsonConvert.SerializeObject(rtnmsg);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(ex.Message);
            }
        }

        private string Logout()
        {
            //if (_voiceSocket != null && _voiceSocket.Connected)
            //{
            //    //关闭Socket之前，首选需要把双方的Socket Shutdown掉
            //    _voiceSocket.Shutdown(SocketShutdown.Both);
            //    //Shutdown掉Socket后主线程停止10ms，保证Socket的Shutdown完成
            //    Thread.Sleep(10);
            //    //关闭客户端Socket,清理资源
            //    _voiceSocket.Close();
            //}
            //if (_clientSocket != null && _clientSocket.Connected)
            //{
            //    //关闭Socket之前，首选需要把双方的Socket Shutdown掉
            //    _clientSocket.Shutdown(SocketShutdown.Both);
            //    //Shutdown掉Socket后主线程停止10ms，保证Socket的Shutdown完成
            //    Thread.Sleep(10);
            //    //关闭客户端Socket,清理资源
            //    _clientSocket.Close();
            //}
            return JsonConvert.SerializeObject(new Rst { Result = "Success" });
        }

        private string StartSpeech()
        {
            try
            {
                _sendbuff = new byte[1000];
                //请求
                var rtn = EvrcProtocal.proto_make_request_mic_packet(PubUser.default_group, ref _sendbuff[0]);
                var sendrtn = _clientSocket.Send(_sendbuff, rtn, SocketFlags.None);
                _recvbuff = new byte[1000];
                int len = _clientSocket.Receive(_recvbuff, 1000, SocketFlags.None);
                if (len == 0)
                {
                    return JsonConvert.SerializeObject(new Rst { Result = "没有接收到服务器响应。" });
                }
                uint vlen = 0;
                var checkedrtn = CrcCheck(_recvbuff, len, ref vlen); //CRC校验
                //解包请求
                var entgack = EvrcProtocal.proto_unpack_request_mic_ack(ref checkedrtn[5], vlen - 5);
                EvrcPtr = 0; //流计算偏移量重置
                StartCollectSound();//开始喊话
                //ShowMsg("开始喊话res：" + (entgack != 0));
                return JsonConvert.SerializeObject(new Rst { Result = "Success" });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new Rst { Result = ex.Message });
            }
        }

        private string StopSpeech()
        {
            try
            {
                _sendbuff = new byte[1000];
                //请求
                var rtn = EvrcProtocal.proto_make_release_mic_packet(PubUser.default_group, ref _sendbuff[0]);
                _clientSocket.Send(_sendbuff, rtn, SocketFlags.None);

                _recvbuff = new byte[1000];
                int len = _clientSocket.Receive(_recvbuff, 1000, SocketFlags.None);
                if (len == 0)
                {
                    //ShowMsg("停止喊话req：没有接收到服务器响应。");
                    return JsonConvert.SerializeObject(new Rst { Result = "没有接收到服务器响应" });
                }
                uint vlen = 0;
                var checkedrtn = CrcCheck(_recvbuff, len, ref vlen);//CRC校验
                //解包请求
                var entgack = EvrcProtocal.proto_unpack_release_mic_ack(ref checkedrtn[5], vlen - 5);
                StopCollectSound();
                //ShowMsg("停止喊话res：" + (entgack != 0));
                return JsonConvert.SerializeObject(new Rst { Result = "Success" });
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new Rst { Result = ex.Message });
            }
        }

        public string InvokeCmd(Cmd cmd)
        {
            var rtn = "";
            switch (cmd.Command)
            {
                case "login":
                    rtn = Login();
                    break;
                //case "logout":
                //    rtn = Logout();
                //    break;
                case "start":
                    rtn = StartSpeech();
                    break;
                case "stop":
                    rtn = StopSpeech();
                    break;
                default:
                    break;
            }

            return rtn;
        }
    }
}
