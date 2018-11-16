using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace EVRCTest
{
    public partial class Form1 : Form
    {
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
        private static int MAXRECBUFFER = 24 * 10;
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
        private System.Windows.Forms.Timer tcptimer, udptimer;

        private PcmAudio.WaveDelegate _wd, _wdPlay;

        #region PCM播放

        private WaveOutPlayer.WaveOut m_Player;
        private WaveOutPlayer.WaveFormat m_Format;
        private WaveOutPlayer.WaveOutStream m_AudioStream;
        private bool isPlaying;

        #endregion

        public Form1()
        {
            InitializeComponent();
            _wd = WaveInProc;
            _wdPlay = WaveOutProc;

            ////以下注册回调
            //var call1 = new EvrcProtocal.CallBackHeartBeat(HeartBeat);//DLL心跳回调
            //EvrcProtocal.SetHeartBeat(call1);
            //var call2 = new EvrcProtocal.CallBackSendVoice(VoiceSend);//DLL音频回调
            //EvrcProtocal.SetSendVoice(call2);
            //EvrcProtocal.InitCCS(100);
        }

        private void Tcptimer_Tick(object sender, EventArgs e)
        {

        }

        private void Udptimer_Tick(object sender, EventArgs e)
        {
            if (_voiceSocket == null || !_voiceSocket.Connected)
                return;
            if (gRunPar.udpHbCnt++ >= 4)
            {
                //以下发送心跳包
                var udpsndbuf = new byte[100];
                var uiSndLen = EvrcProtocal.proto_make_ping_packet((uint)user.uid, ref udpsndbuf[0]);//测试ping
                ShowMsg("ping服务器：" + uiSndLen);
                if (uiSndLen > 0)
                {
                    SendUdpPacket(_voiceSocket, user.iSetPar.ip, POC_NET_UDP_PORT, udpsndbuf, uiSndLen);
                    ShowMsg("已发送UDP心跳包。");
                }
                gRunPar.udpHbCnt = 0;
            }
        }


        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //tcptimer.Stop();
            udptimer.Stop();
            if (_clientSocket != null && _clientSocket.Connected)
                _clientSocket.Close();
            if (_voiceSocket != null && _voiceSocket.Connected)
                _voiceSocket.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            btnDo.Enabled = false;
            btnLogin.Enabled = true;
            btnLogout.Enabled = false;
            //以下初始化音频采集
            for (int i = 0; i < MAXRECBUFFER; i++)
            {
                var hdr = new PcmAudio.WaveHdr
                {
                    lpData = Marshal.AllocHGlobal(RECBUFFER),
                    dwBufferLength = (UInt32) RECBUFFER
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


            //以下建立服务器连接

            _voiceSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _voiceSocket.Connect(new IPEndPoint(IPAddress.Parse(txtIP.Text), POC_NET_UDP_PORT));
            _voiceSocket.ReceiveTimeout = 30 * 1000;
            _voiceSocket.SendTimeout = 30 * 1000;

            //tcptimer = new System.Windows.Forms.Timer();
            //tcptimer.Tick += Tcptimer_Tick;
            //tcptimer.Interval = 20 * 1000;
            //tcptimer.Enabled = true;
            //tcptimer.Start();

            udptimer = new System.Windows.Forms.Timer();
            udptimer.Tick += Udptimer_Tick;
            udptimer.Interval = 6 * 1000;
            udptimer.Enabled = true;
            udptimer.Start();
        }

        private void ShowMsg(string msg)
        {
            this.Invoke(new EventHandler(delegate
            {
                rtbMsg.AppendText(msg + "\r\n");
                rtbMsg.ScrollToCaret();
            }));
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
        private void btnLogin_Click(object sender, EventArgs e)
        {
            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                ReceiveTimeout = 30 * 1000,
                SendTimeout = 30 * 1000
            };
            _clientSocket.Connect(new IPEndPoint(IPAddress.Parse(txtIP.Text), POC_NET_TCP_PORT));

            tvGroup.Nodes.Clear();
            lvGroup.Items.Clear();

            //登录参数
            user = new EvrcProtocal.stUser
            {
                iSetPar =
                {
                    ip = txtIP.Text,
                    account = txtUsr.Text,
                    passwd = txtPwd.Text
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
            ShowMsg("登陆解包res：" + (loginunpack != 0) + ";uid=" + user.uid + ";username=" + name + ";defaultgroup=" + user.default_group);

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
            ShowMsg("获取组解包res：" + (upkgroup != 0));

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
            ShowMsg("进入群组解包res：" + (entgack != 0));

            //3、解析组
            var gl = EvrcProtocal.proto_get_group_list();
            var grouplist = (EvrcProtocal.stGroupList)Marshal.PtrToStructure(gl, typeof(EvrcProtocal.stGroupList));
            ShowMsg("获取组结果：组数" + grouplist.uiTotal);
            var nowhead = grouplist.pHead;
            var now = (EvrcProtocal.stGroupNode)Marshal.PtrToStructure(nowhead, typeof(EvrcProtocal.stGroupNode));
            while (true)
            {
                if (now == null) break;
                var name1 = TrimStr(Encoding.UTF8.GetString(now.iGroup.gname));
                ShowMsg("解析组：gid=" + now.iGroup.gid + "；gname=" + name1 + "；number_n=" + now.iGroup.number_n);
                tvGroup.Nodes.Add(now.iGroup.gid.ToString(), name1);

                //4、根据组获取用户
                _sendbuff = new byte[1000];
                var mbr = EvrcProtocal.proto_make_query_member_packet(now.iGroup.gid, ref _sendbuff[0]);//now.iGroup.gid|user.default_group
                _clientSocket.Send(_sendbuff, mbr, SocketFlags.None);
                _recvbuff = new byte[1000];
                len = _clientSocket.Receive(_recvbuff, 1000, SocketFlags.None);
                checkedrtn = CrcCheck(_recvbuff, len, ref vlen);//CRC校验
                var dembr = EvrcProtocal.proto_unpack_query_member_ack(ref checkedrtn[5], vlen - 5);
                ShowMsg("解包用户res：" + (dembr != 0));

                var ml = EvrcProtocal.proto_get_user_list();
                var mbrlist = (EvrcProtocal.stUserList)Marshal.PtrToStructure(ml, typeof(EvrcProtocal.stUserList));

                ShowMsg("获取用户结果：用户数" + mbrlist.uiTotal);
                var head = mbrlist.pHead;
                var mbrnow = (EvrcProtocal.stUserNode)Marshal.PtrToStructure(head, typeof(EvrcProtocal.stUserNode));
                while (true)
                {
                    if (mbrnow == null) break;
                    var name2 = TrimStr(Encoding.UTF8.GetString(mbrnow.uname));
                    ShowMsg("解析用户：uid=" + mbrnow.uid + "；online=" + mbrnow.online + "；uname=" + name2);
                    tvGroup.Nodes.Find(now.iGroup.gid.ToString(), false)[0].Nodes.Add(mbrnow.uid.ToString(), name2);

                    mbrnow = (EvrcProtocal.stUserNode)Marshal.PtrToStructure(mbrnow.pNext, typeof(EvrcProtocal.stUserNode));
                }
                now = (EvrcProtocal.stGroupNode)Marshal.PtrToStructure(now.pNext, typeof(EvrcProtocal.stGroupNode));
            }
            tvGroup.ExpandAll();
            PubUser = user;
            btnDo.Enabled = true;
            btnLogin.Enabled = false;
            btnLogout.Enabled = true;
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (_clientSocket != null && _clientSocket.Connected)
            {
                _clientSocket.Disconnect(true);
            }
            btnLogin.Enabled = true;
            btnLogout.Enabled = false;
            btnDo.Enabled = true;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (tvGroup.SelectedNode == null) return;
            foreach (ListViewItem groupItem in lvGroup.Items)
            {
                if (groupItem.Name == tvGroup.SelectedNode.Name)
                {
                    return;
                }
            }
            var item = new ListViewItem
            {
                Name = tvGroup.SelectedNode.Name,
                Tag = tvGroup.SelectedNode.Name,
                Text = tvGroup.SelectedNode.Text
            };
            lvGroup.Items.Add(item);
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (lvGroup.SelectedItems.Count == 0) return;
            for (var i = lvGroup.SelectedItems.Count - 1; i >= 0; i--)
            {
                var item = lvGroup.SelectedItems[i];
                lvGroup.Items.Remove(item);
            }
        }

        /// <summary>
        /// 按下喊话
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDo_MouseDown(object sender, MouseEventArgs e)
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
                    ShowMsg("开始喊话req：没有接收到服务器响应。");
                    return;
                }
                uint vlen = 0;
                var checkedrtn = CrcCheck(_recvbuff, len, ref vlen); //CRC校验
                //解包请求
                var entgack = EvrcProtocal.proto_unpack_request_mic_ack(ref checkedrtn[5], vlen - 5);
                EvrcPtr = 0; //流计算偏移量重置
                StartCollectSound();//开始喊话
                ShowMsg("开始喊话res：" + (entgack != 0));
            }
            catch (Exception ex)
            {
                ShowMsg(ex.Message);
            }
        }

        /// <summary>
        /// 弹起取消喊话
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDo_MouseUp(object sender, MouseEventArgs e)
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
                    ShowMsg("停止喊话req：没有接收到服务器响应。");
                    return;
                }
                uint vlen = 0;
                var checkedrtn = CrcCheck(_recvbuff, len, ref vlen);//CRC校验
                //解包请求
                var entgack = EvrcProtocal.proto_unpack_release_mic_ack(ref checkedrtn[5], vlen - 5);
                StopCollectSound();
                ShowMsg("停止喊话res：" + (entgack != 0));
            }
            catch (Exception ex)
            {
                ShowMsg(ex.Message);
            }
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
                        ShowMsg("正在采集音频数据...");
                    }
                    else
                    {
                        ShowMsg("采集音频失败");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMsg(ex.Message);
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
                    ShowMsg("停止采集音频数据。");
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
                ShowMsg(ex.Message);
            }
        }

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
            SendUdpPacket(_voiceSocket, txtIP.Text, POC_NET_UDP_PORT, pPacket, uiDataLen);
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
            ShowMsg(DateTime.Now + "--send udp pack：" + snd);
        }

        #region 测试方法

        private PcmAudio.WaveHdr pcmcache;

        private unsafe void btnTest_Click(object sender, EventArgs e)
        {
            try
            {
                #region 测试：PCM音频采集 + evrc编码 + evrc解码 + PCM音频播放

                //hPlay = IntPtr.Zero;
                //PcmAudio.WaveHdr hdr;
                //var plyrts = PcmAudio.waveOutOpen(out hPlay, -1, ref lpFormatPlay, _wdPlay, 0, 0x00030000);
                //if (plyrts == 0)//打开成功
                //{
                //    plyrts = PcmAudio.waveOutSetVolume(hPlay, 0xffffffff);//设置音量
                //    if (plyrts == 0)
                //    {
                //        //evrc编码后数据存放：EvrcBuff
                //        for (var i = 0; i < EvrcBuff.Length; i++)
                //        {
                //            //二维数组复制到一维数组
                //            var tmp = new byte[AUDIOPACKLENGTH];
                //            for (var j = 0; j < AUDIOPACKLENGTH; j++)
                //            {
                //                tmp[j] = EvrcBuff[i, j];
                //            }
                //            //数组存buffer转为Intptr
                //            //var dec = Marshal.AllocHGlobal(RECBUFFER);
                //            hdr = new PcmAudio.WaveHdr
                //            {
                //                lpData = Marshal.AllocHGlobal(RECBUFFER),
                //                dwBufferLength = (UInt32)RECBUFFER
                //            };
                //            //var buffer = Marshal.AllocHGlobal(AUDIOPACKLENGTH);
                //            //Marshal.Copy(tmp, 0, buffer, AUDIOPACKLENGTH);


                //            EvrcProtocal.evrc_decode(tmp, ref hdr.lpData);//解码得到byte[]

                //            //将要输出的数据写入buffer

                //            PcmAudio.WaveHdr hdrcopy = hdr;//todo:将最后一帧播放出来
                //            plyrts = PcmAudio.waveOutPrepareHeader(hPlay, ref hdrcopy, (UInt32)Marshal.SizeOf(typeof(PcmAudio.WaveHdr)));
                //            if (plyrts == 0)//播放成功
                //            {
                //                plyrts = PcmAudio.waveOutWrite(hPlay, ref hdrcopy, (UInt32)Marshal.SizeOf(typeof(PcmAudio.WaveHdr)));
                //            }
                //        }
                //        ShowMsg("播放成功");
                //    }
                //}


                //int i;
                //byte[] pDat = new byte[RECBUFFER];
                //int uiDatLen = 16;
                //byte[] ucEvrcDat = new byte[AUDIOPACKLENGTH];
                //byte* pPcm;

                //for (int j = 0; j < globalhdr.Length; j++)
                //{
                //    Marshal.Copy(globalhdr[j].lpData, pDat, 0, RECBUFFER);
                //    byte ucFrameNum = pDat[12];

                //    if (ucFrameNum <= 0x0a)
                //    {
                //        for (i = 0; i < ucFrameNum; i++)
                //        {
                //            switch ((pDat[13 + (i >> 2)] >> (6 - (i % 4) * 2)) & 0x03)
                //            {
                //                case 0x02:
                //                    ucEvrcDat[0] = 3;
                //                    memcpy(&ucEvrcDat[1], &pDat[uiDatLen], 10);
                //                    pPcm = (uint8*)malloc(PCM_PRE_FRAME_SIZE);
                //                    evrc_decode(ucEvrcDat, pPcm);//解码
                //                    m_pPlaySound->PostThreadMessage(WM_PLAYSOUND_PLAYBLOCK, (WPARAM)PCM_PRE_FRAME_SIZE, (LPARAM)pPcm);//触发播放事件
                //                    uiDatLen += 10;
                //                    break;

                //                case 0x03:
                //                    ucEvrcDat[0] = 4;
                //                    memcpy(&ucEvrcDat[1], &pDat[uiDatLen], 22);
                //                    pPcm = (uint8*)malloc(PCM_PRE_FRAME_SIZE);
                //                    evrc_decode(ucEvrcDat, pPcm);
                //                    m_pPlaySound->PostThreadMessage(WM_PLAYSOUND_PLAYBLOCK, (WPARAM)PCM_PRE_FRAME_SIZE, (LPARAM)pPcm);
                //                    uiDatLen += 22;
                //                    break;

                //                default:
                //                    //_DEBUG("OutWrite unknow FrameRate!\r\n");
                //                    break;
                //            }
                //        }
                //    }
                //}

                #endregion

                #region 测试：PCM音频采集 + PCM音频播放

                hPlay = IntPtr.Zero;
                var plyrts = PcmAudio.waveOutOpen(out hPlay, -1, ref lpFormatPlay, _wdPlay, 0, 0x00030000);
                if (plyrts == 0)//打开成功
                {
                    plyrts = PcmAudio.waveOutSetVolume(hPlay, 0xffffffff);//设置音量
                    if (plyrts == 0)
                    {
                        pcmcache = new PcmAudio.WaveHdr
                        {
                            dwBufferLength = (UInt32)RECBUFFER
                        };
                        for (int i = 0; i < globalhdr.Length; i++)
                        {
                            pcmcache.lpData = globalhdr[i].lpData;
                            //将要输出的数据写入buffer
                            plyrts = PcmAudio.waveOutPrepareHeader(hPlay, ref pcmcache, (UInt32)Marshal.SizeOf(typeof(PcmAudio.WaveHdr)));
                            if (plyrts == 0)//播放成功
                            {
                                plyrts = PcmAudio.waveOutWrite(hPlay, ref pcmcache, (UInt32)Marshal.SizeOf(typeof(PcmAudio.WaveHdr)));
                            }
                        }
                    }
                }
                ShowMsg("播放成功");

                #endregion

                #region 测试：编码数据传输 + APP音频解析（通过）

                //Func1();
                //string arr =
                //    "C0 6F 00 19 00 00 00 00 00 00 10 6A 0A AA AA A0 4C F9 E5 00 0C 1C E2 6E 1D 07 23 C5 2C C0 D3 5C 27 70 5A 17 32 96 F9 68 C2 98 7E 2C 38 56 EA 98 48 B0 3C 1C 7F F6 96 FB 3A 94 F9 37 79 73 35 99 17 5D BC 15 D9 01 60 76 DF D8 83 7D DC E8 48 FB 88 F1 CA B7 73 BA 94 E2 59 51 DE AC BA BA 00 CC 95 00 D9 4F 90 77 E9 79 CD FD AD C3 D9 37 41 77 BF 9D FC 9F ";
                //var arrsnd = HexStringToByteArray(arr);
                //SendUdpPacket(_voiceSocket, txtIP.Text, POC_NET_UDP_PORT, arrsnd, arrsnd.Length);
                //Func2();

                #endregion
            }
            catch (Exception exception)
            {
                ShowMsg("测试按钮：" + exception.Message);
            }
        }

        /// <summary>
        /// waveOutOpen的回调方法
        /// </summary>
        public void WaveOutProc(IntPtr hwi, UInt32 uMsg, UInt32 dwInstance, UInt32 dwParam1, UInt32 dwParam2)
        {
            switch (uMsg)
            {
                case PcmAudio.MM_WIM_OPEN: break;
                case PcmAudio.MM_WIM_DATA: break;
                case PcmAudio.MM_WIM_CLOSE: break;
            }
        }

        /// <summary>
        /// 十六进制样式字符串转byte[]
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public byte[] HexStringToByteArray(string s)
        {
            try
            {
                s = s.Replace(" ", "");
                byte[] buffer = new byte[s.Length / 2];
                for (int i = 0; i < s.Length; i += 2)
                    buffer[i / 2] = Convert.ToByte(s.Substring(i, 2), 16);
                return buffer;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        /// byte[]转十六进制样式字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public string ToHexString(byte[] bytes) // 0xae00cf => "AE00CF "
        {
            var hexString = string.Empty;
            if (bytes != null)
            {
                var strB = new StringBuilder();
                foreach (var t in bytes)
                {
                    strB.Append(t.ToString("X2"));
                }
                hexString = strB.ToString();
            }
            return hexString;
        }

        /// <summary>
        /// byte[]转intptr
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static IntPtr BytesToIntptr(byte[] bytes)
        {
            int size = bytes.Length;
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(bytes, 0, buffer, size);
                return buffer;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        #endregion
    }


    public struct PocRunPar
    {
        public byte udpHbCnt;
        public byte tcpHbCnt;
    }
}
