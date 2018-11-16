using System;
using System.Runtime.InteropServices;
using System.Text;

// ReSharper disable InconsistentNaming

namespace EVRCTest
{
    public static class EvrcProtocal
    {
        public enum PTT_UDP_PACK_TYPE
        {
            PTT_UDP_HEART_BEAT = 0x58,
            PTT_UDP_VOICE = 0x6F
        }

        public enum PTT_REQ_PACK_TYPE
        {
            PTT_PROTO_LOGIN = 0x01,
            PTT_PROTO_QUERY_GROUP = 0x02,
            PTT_PROTO_QUERY_MEMBER = 0x03,
            PTT_PROTO_ENTER_GROUP = 0x04,
            PTT_PROTO_LEAVE_GROUP = 0x05,
            PTT_PROTO_REQUE_MIC = 0x06,
            PTT_PROTO_RELEA_MIC = 0x07,
            PTT_PROTO_LOGOUT = 0x08,
            PTT_PROTO_TMP_CALL = 0x09,
            PTT_PROTO_REPORT_LOC = 0x0a,
            PTT_PROTO_MODIFY_PSWD = 0x0b,

            PTT_PROTO_REQUE_LOC = 0x30,
            PTT_PROTO_QUERY_ALL_MEM = 0x31,

            PTT_PROTO_GET_IP = 0x40,
            PTT_PROTO_PONG = 0x41
        }

        public enum PTT_PUSH_PACK_TYPE
        {
            PTT_PROTO_PING = 0xc1,
            PTT_PROTO_LOST_MIC = 0xc2,
            PTT_PROTO_KICKOUT = 0xc3,
            PTT_PROTO_RECONFIG = 0xc4,
            PTT_PROTO_CUR_GROUP = 0xc5,
            PTT_PROTO_GRO_LIST_CHG = 0xc6,
            PTT_PROTO_MEM_GET_MIC = 0xc7,
            PTT_PROTO_MEM_LOST_MIC = 0xc8,
            PTT_PROTO_LOG_CHANGE = 0xc9,
            PTT_PROTO_TMP_CALL_STA = 0xca,
            PTT_PROTO_MSG_ARRIVED = 0xcb,
            PTT_PROTO_TMP_CALL_ARR = 0xcc
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct stPocSetPar
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string ip;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string account;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string passwd;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string apnname;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string apnacc;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string apnpwd;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string agentpwd;

            public char inviteFlg;
            public char offlineFlg;

            public uint uiApnValidFlg;
            public uint uiServerCfg;
            public uint ucValidFlg;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct stUser
        {
            public int uid;
            public stPocSetPar iSetPar;
            public string pValidIp;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public byte[] username;
            public int version;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
            public string platform;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
            public string device;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string meid;
            public uint expect_payload;

            public uint default_group;
            public uint loc_report_period;
            public bool audio_enabled;
            public uint cfg_ptt_timeout;
            public uint heart_inter;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct stPocRunPar
        {
            public byte udpHbCnt;
            public byte tcpHbCnt;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct stPocSeting
        {
            public stUser iUser;
            public stPocRunPar iPocRun;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct stLocPoint
        {
            public uint uid;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
            public string longitude;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
            public string latitude;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
            public string time;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public struct stLocNode
        {
            public stLocPoint iPoint;
            public IntPtr pNext;//C#结构体不能引用自身，无法做显性链表，用IntPtr指向stLocNode
            public IntPtr pPrev;//C#结构体不能引用自身，无法做显性链表，用IntPtr指向stLocNode
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct stLocList
        {
            public uint uiTotal;
            public stLocNode pCurr;
            public stLocNode pHead;
            public stLocNode pTial;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public class stGroup
        {
            public uint gid;
            public uint number_n;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public byte[] gname;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public class stGroupNode
        {
            public stGroup iGroup;//stGroup
            public IntPtr pNext;//C#结构体不能引用自身，无法做显性链表，用IntPtr指向stGroupNode
            public IntPtr pPrev;//C#结构体不能引用自身，无法做显性链表，用IntPtr指向stGroupNode
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public class stGroupList
        {
            public ushort uiTotal;
            public IntPtr pEnter;//stGroupNode
            public IntPtr pCurr;//stGroupNode
            public IntPtr pHead;//stGroupNode
            public IntPtr pTial;//stGroupNode

            public stGroupList()
            {
                uiTotal = 0;

                var pEnter_s = new stGroupNode();
                pEnter = Marshal.AllocHGlobal(Marshal.SizeOf(pEnter_s));
                Marshal.StructureToPtr(pEnter_s, pEnter, false);

                var pCurr_s = new stGroupNode();
                pCurr = Marshal.AllocHGlobal(Marshal.SizeOf(pCurr_s));
                Marshal.StructureToPtr(pCurr_s, pCurr, false);

                var pHead_s = new stGroupNode();
                pHead = Marshal.AllocHGlobal(Marshal.SizeOf(pHead_s));
                Marshal.StructureToPtr(pHead_s, pHead, false);

                var pTial_s = new stGroupNode();
                pTial = Marshal.AllocHGlobal(Marshal.SizeOf(pTial_s));
                Marshal.StructureToPtr(pTial_s, pTial, false);
            }
        }

        ///// <summary>
        ///// 自定义结构体，用来解析链表结构
        ///// </summary>
        //[StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        //public struct StructLinkedFirst
        //{
        //    public IntPtr pNext;
        //}

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public class stUserNode
        {
            public uint uid;
            public byte ingroup;
            public byte online;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public byte[] uname;
            public IntPtr pNext;//C#结构体不能引用自身，无法做显性链表，用IntPtr指向stUserNode
            public IntPtr pPrev;//C#结构体不能引用自身，无法做显性链表，用IntPtr指向stUserNode
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public class stUserList
        {
            public ushort uiTotal;
            public IntPtr pCurr;//stUserNode
            public IntPtr pHead;//stUserNode
            public IntPtr pTial;//stUserNode
            public IntPtr pAddName;//stUserNode

            public stUserList()
            {
                uiTotal = 0;
                var pCurr_s = new stUserNode();
                pCurr = Marshal.AllocHGlobal(Marshal.SizeOf(pCurr_s));
                Marshal.StructureToPtr(pCurr_s, pCurr, false);

                var pHead_s = new stUserNode();
                pHead = Marshal.AllocHGlobal(Marshal.SizeOf(pHead_s));
                Marshal.StructureToPtr(pHead_s, pHead, false);

                var pTial_s = new stUserNode();
                pTial = Marshal.AllocHGlobal(Marshal.SizeOf(pTial_s));
                Marshal.StructureToPtr(pTial_s, pTial, false);

                var pAddName_s = new stUserNode();
                pAddName = Marshal.AllocHGlobal(Marshal.SizeOf(pAddName_s));
                Marshal.StructureToPtr(pAddName_s, pAddName, false);
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public struct PTT_MemberGetMic
        {
            public uint gid;
            public uint uid;
            public int has_allow;
            public uint allow;
            public int has_name;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
            public string name;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public struct PTT_MemberLostMic
        {
            public uint gid;
            public uint uid;
        }

        [DllImport("CCS.dll", EntryPoint = "InitCCS", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public static extern void InitCCS(int a);

        [DllImport("CCS.dll", EntryPoint = "StartSpeech", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public static extern void StartSpeech();

        /// <summary>
        /// 心跳回调
        /// </summary>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallBackHeartBeat([MarshalAs(UnmanagedType.LPStr, SizeConst = 1024)] string param1,
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 1024)] string param2);
        [DllImport("CCS.dll", EntryPoint = "SetHeartBeat", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetHeartBeat(CallBackHeartBeat pfun);
        /// <summary>
        /// 音频发送回调
        /// </summary>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CallBackSendVoice([MarshalAs(UnmanagedType.LPStr, SizeConst = 1024)] string param1,
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 1024)] string param2);
        [DllImport("CCS.dll", EntryPoint = "SetSendVoice", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetSendVoice(CallBackSendVoice pfun);


        ///// <summary>
        ///// 测试回调2
        ///// </summary>
        ///// <param name="s"></param>
        //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        //public delegate void TestcallBack([MarshalAs(UnmanagedType.LPStr, SizeConst = 1024)] string s);
        //[DllImport("CCS.dll", EntryPoint = "SetCallBackFun", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        //public static extern void SetCallBackFun(TestcallBack pfun);

        ///// <summary>
        ///// 测试回调1
        ///// </summary>
        ///// <param name="num1"></param>
        ///// <param name="num2"></param>
        ///// <returns></returns>
        //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        //public delegate int DllcallBack(int num1, int num2);
        //[DllImport("CCS.dll", EntryPoint = "CallPFun", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = false, CallingConvention = CallingConvention.Cdecl)]
        //public static extern int CallPFun(DllcallBack pfun, int a, int b);


        /// <summary>
        /// 
        /// </summary>
        [DllImport("CCS.dll", EntryPoint = "add", CallingConvention = CallingConvention.Cdecl,CharSet = CharSet.Ansi)]
        public static extern double CCSAdd(double a, double b);

        /// <summary>
        /// 输入:pcm 320 返回:evrc byte[23]*8bit 
        /// </summary>
        [DllImport("CCS.dll", EntryPoint = "encode", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Auto)]
        public static extern IntPtr CCSEncode(IntPtr pcm);

        /// <summary>
        /// 输入:evrc byte[23]*8bit  返回:pcm 320
        /// </summary>
        [DllImport("CCS.dll", EntryPoint = "decode", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Ansi)]
        public static extern void CCSDecode([MarshalAs(UnmanagedType.LPArray, SizeConst = 23)] byte[] evrc,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 320)] ref byte[] pcm);

        /*********************************************************
        功能:初始化解码器
        输入：
        返回：
        **********************************************************/
        //void InitDecoder(void);
        [DllImport("Evrc.dll", EntryPoint = "InitDecoder", CallingConvention = CallingConvention.Cdecl)]
        public static extern void InitDecoder();

        /*********************************************************
        功能:EVRC解码,一次调用只能解析一帧数据，数据长度：EVRC 10  PCM 320
        输入:pEvrc （原型是byte[23]）
        返回:pPcm （原型是WaveHdr.lpData，本程序中为RECBUFFER=320长度的IntPtr）
        **********************************************************/
        //void evrc_decode(unsigned char* pEvrc, unsigned char* pPcm);
        [DllImport("Evrc.dll", EntryPoint = "evrc_decode", CallingConvention = CallingConvention.Cdecl)]
        public static extern void evrc_decode(byte[] pEvrc, ref IntPtr pPcm);

        /*********************************************************
        功能:初始化编码器
        输入：
        返回：
        **********************************************************/
        //void InitEncoder(void);
        [DllImport("Evrc.dll", EntryPoint = "InitEncoder", CallingConvention = CallingConvention.Cdecl)]
        public static extern void InitEncoder();

        /*********************************************************
        功能:EVRC编码,一次调用只能编码一帧数据，数据长度：EVRC 10  PCM 320
        输入:pPcm 原型是char型指针，指向WaveHdr.lpData
        返回:pEvrc 原型是byte[23]
        **********************************************************/
        //void evrc_encode(unsigned char* pEvrc, unsigned char* pPcm);
        [DllImport("Evrc.dll", EntryPoint = "evrc_encode", CallingConvention = CallingConvention.Cdecl)]
        public static extern void evrc_encode(ref byte pEvrc, ref IntPtr pPcm);

        /******************************************************************************************
        功能:数据包CRC校验
        输入：Data 数据流  len 数据长度
        返回：32位CRC校验值
        ******************************************************************************************/
        //uint32 ZTE_CRC(uint8* Data, uint32 len);
        [DllImport("Protocal.dll", EntryPoint = "ZTE_CRC", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint ZTE_CRC(ref byte Data, uint len);

        /******************************************************************************************
        功能:组建   登陆请求   压缩数据包
        输入：pUser 指向stUser结构体，登陆时需要填充stUser结果体中stPocSetPar结构，pBuff指向接收缓冲区
        返回：数据包长度
        ******************************************************************************************/
        //int proto_make_login_packet(stUser* pUser, uint8* pBuff);
        [DllImport("Protocal.dll", EntryPoint = "proto_make_login_packet", CallingConvention = CallingConvention.Cdecl,CharSet=CharSet.Ansi)]
        public static extern int proto_make_login_packet(ref stUser pUser,ref byte pBuff);

        /******************************************************************************************
        功能:组建   查询组列表   压缩数据包
        输入：pBuff指向接收缓冲区
        返回：数据包长度
        ******************************************************************************************/
        //int proto_make_query_group_packet(uint8* pBuff);
        [DllImport("Protocal.dll", EntryPoint = "proto_make_query_group_packet", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int proto_make_query_group_packet(ref byte pBuff);

        /******************************************************************************************
        功能:组建   查询成员列表   压缩数据包
        输入：gid组ID，pBuff指向接收缓冲区
        返回：数据包长度
        ******************************************************************************************/
        //int proto_make_query_member_packet(uint32 gid, uint8* pBuff);
        [DllImport("Protocal.dll", EntryPoint = "proto_make_query_member_packet", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int proto_make_query_member_packet(uint gid, ref byte pBuff);

        /******************************************************************************************
        功能:组建   进入群组   压缩数据包
        输入：gid组ID，pBuff指向接收缓冲区
        返回：数据包长度
        ******************************************************************************************/
        //int proto_make_enter_group_packet(uint32 gid, uint8* pBuff);
        [DllImport("Protocal.dll", EntryPoint = "proto_make_enter_group_packet", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int proto_make_enter_group_packet(uint gid, ref byte pBuff);

        /******************************************************************************************
        功能:组建   离开群组   压缩数据包
        输入：gid组ID，pBuff指向接收缓冲区
        返回：数据包长度
        ******************************************************************************************/
        //int proto_make_leave_group_packet(uint32 gid, uint8* pBuff);
        [DllImport("Protocal.dll", EntryPoint = "proto_make_leave_group_packet", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int proto_make_leave_group_packet(uint gid, ref byte pBuff);

        /******************************************************************************************
        功能:组建   呼叫请求   缩数据包
        输入：gid组ID，pBuff指向接收缓冲区
        返回：数据包长度
        ******************************************************************************************/
        //int proto_make_request_mic_packet(uint32 gid, uint8* pBuff);
        [DllImport("Protocal.dll", EntryPoint = "proto_make_request_mic_packet", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int proto_make_request_mic_packet(uint gid, ref byte pBuff);

        /******************************************************************************************
        功能:组建   呼叫释放   压缩数据包
        输入：gid组ID，pBuff指向接收缓冲区
        返回：数据包长度
        ******************************************************************************************/
        //int proto_make_release_mic_packet(uint32 gid, uint8* pBuff);
        [DllImport("Protocal.dll", EntryPoint = "proto_make_release_mic_packet", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int proto_make_release_mic_packet(uint gid, ref byte pBuff);

        /******************************************************************************************
        功能:组建   单呼   压缩数据包
        输入：uid组ID，pBuff指向接收缓冲区
        返回：数据包长度
        ******************************************************************************************/
        //int proto_make_temp_call_packet(uint32 uid, uint8* pBuff);
        [DllImport("Protocal.dll", EntryPoint = "proto_make_temp_call_packet", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int proto_make_temp_call_packet(uint gid, ref byte pBuff);

        /******************************************************************************************
        功能:组建   TCP心跳   压缩数据包
        输入：pBuff指向接收缓冲区
        返回：数据包长度
        ******************************************************************************************/
        //int proto_make_pong_packet(uint8* pBuff);
        [DllImport("Protocal.dll", EntryPoint = "proto_make_pong_packet", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int proto_make_pong_packet(ref byte pBuff);

        /******************************************************************************************
        功能:组建   UDP心跳   压缩数据包
        输入：pBuff指向接收缓冲区
        返回：数据包长度
        ******************************************************************************************/
        //int proto_make_ping_packet(uint32 uid, uint8* pBuff);
        //int proto_make_get_ip_packet(char *pAcc, char *pPswd, uint8 *pBuff);
        //int proto_make_local_report_packet(uint32 uid, double lat, double lon, uint8 *pBuff);
        [DllImport("Protocal.dll", EntryPoint = "proto_make_ping_packet", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int proto_make_ping_packet(uint gid, ref byte pBuff);

        /******************************************************************************************
        功能:组建   GPS请求数据   压缩数据包
        输入：uid被查询用户ID, ucReqType请求类型0表示查询最后一次数据1表示连续查询多个数据轨迹,
              pStart, pEnd, 起始时间和结束时间，这两个参数仅对ucReqType=1的情况生效，
              pBuff指向接收缓冲区
        返回：数据包长度
        ******************************************************************************************/
        //int proto_make_local_request_packet(uint32 uid, unsigned char ucReqType, char* pStart, char* pEnd, uint8* pBuff);
        [DllImport("Protocal.dll", EntryPoint = "proto_make_local_request_packet", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int proto_make_local_request_packet(uint uid, byte ucReqType, string pStart, string pEnd, ref byte pBuff);

        //=====================================================================================================================================================

        /******************************************************************************************
        功能:登陆请求    解包函数
        输入：pUser返回服务器上用户的配置信息，函数会自动填充结构体上没有被初始化的数据供应用程序使用
              pBuff指向数据包缓冲地址
              uiBufSize数据包长度
        返回：数据包长度
        ******************************************************************************************/
        //int proto_unpack_login_ack(stUser* pUser, uint8* pBuff, uint32 uiBufSize);
        [DllImport("Protocal.dll", EntryPoint = "proto_unpack_login_ack", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int proto_unpack_login_ack(ref stUser pUser,ref byte pBuff, uint uiBufSize);

        /******************************************************************************************
        功能:查询组列表   解包函数
        输入：pBuff指向数据包缓冲地址
              uiBufSize数据包长度
        返回：不为0表示查询成功
        注意：使用该函数解包成功后组列表会被保存到stGroupList结构体形成的链表中，
              使用proto_get_group_list函数可以获取链表头指针位置，用户可以使用链表的正向或反向指针
              访问节点，知道指向NULL结束
        ******************************************************************************************/
        //int proto_unpack_query_group_ack(uint8* pBuff, uint32 uiBufSize);
        [DllImport("Protocal.dll", EntryPoint = "proto_unpack_query_group_ack", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int proto_unpack_query_group_ack(ref byte pBuff, uint uiBufSize);

        /******************************************************************************************
        功能:查询当前组内成员列表   解包函数
        输入：pBuff指向数据包缓冲地址
              uiBufSize数据包长度
        返回：不为0表示查询成功
        注意：使用该函数解包成功后组列表会被保存到stUserList结构体形成的链表中，
              使用proto_get_user_list函数可以获取链表头指针位置，用户可以使用链表的正向或反向指针
              访问节点，知道指向NULL结束
        ******************************************************************************************/
        //int proto_unpack_query_member_ack(uint8* pBuff, uint32 uiBufSize);
        [DllImport("Protocal.dll", EntryPoint = "proto_unpack_query_member_ack", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int proto_unpack_query_member_ack(ref byte pBuff, uint uiBufSize);

        /******************************************************************************************
        功能:进入群组ACK   解析
        输入：pBuff指向接收缓冲区 uiBufSize缓冲区长度
        返回：非0表示成功
        ******************************************************************************************/
        //int proto_unpack_enter_group_ack(uint8* pBuff, uint32 uiBufSize);
        [DllImport("Protocal.dll", EntryPoint = "proto_unpack_enter_group_ack", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int proto_unpack_enter_group_ack(ref byte pBuff, uint uiBufSize);

        /******************************************************************************************
        功能:申请话语权ACK
        输入：pBuff指向接收缓冲区 uiBufSize缓冲区长度
        返回：非0表示成功
        ******************************************************************************************/
        //int proto_unpack_request_mic_ack(uint8* pBuff, uint32 uiBufSize);
        [DllImport("Protocal.dll", EntryPoint = "proto_unpack_request_mic_ack", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int proto_unpack_request_mic_ack(ref byte pBuff, uint uiBufSize);

        /******************************************************************************************
        功能:申请释放话语权ACK
        输入：pBuff指向接收缓冲区 uiBufSize缓冲区长度
        返回：非0表示成功
        ******************************************************************************************/
        //int proto_unpack_release_mic_ack(uint8* pBuff, uint32 uiBufSize);
        [DllImport("Protocal.dll", EntryPoint = "proto_unpack_release_mic_ack", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int proto_unpack_release_mic_ack(ref byte pBuff, uint uiBufSize);

        /******************************************************************************************
        功能:申请进入单呼模式ACK
        输入：pBuff指向接收缓冲区 uiBufSize缓冲区长度
        返回：非0表示成功
        ******************************************************************************************/
        //int proto_unpack_temp_call_ack(uint8* pBuff, uint32 uiBufSize);
        [DllImport("Protocal.dll", EntryPoint = "proto_unpack_temp_call_ack", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int proto_unpack_temp_call_ack(ref byte pBuff, uint uiBufSize);


        //int proto_unpack_get_ip_ack(char* pIP, uint8 *pBuff, uint32 uiBufSize);


        //=====================================================================================================================================================
        /************************以下函数需要在服务器主动推送信息时调用***************************/

        /******************************************************************************************
        功能:其他用户发起呼叫时解析该用户信息时使用
        输入：PTT_MemberGetMic发起呼叫的用户的详细信息，pBuff指向接收缓冲区 uiBufSize缓冲区长度
        返回：非0表示成功
        ******************************************************************************************/
        //int proto_unpack_member_get_mic(PTT_MemberGetMic* pMgm, uint8* pBuff, uint32 uiBufSize);
        [DllImport("Protocal.dll", EntryPoint = "proto_unpack_member_get_mic", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int proto_unpack_member_get_mic(PTT_MemberGetMic pMgm, ref byte pBuff, uint uiBufSize);

        /******************************************************************************************
        功能:其他用户释放当前呼叫时解析该用户信息时使用
        输入：PTT_MemberLostMic发起呼叫的用户的详细信息，pBuff指向接收缓冲区 uiBufSize缓冲区长度
        返回：非0表示成功
        ******************************************************************************************/
        //int proto_unpack_member_lost_mic(PTT_MemberLostMic* pMlm, uint8* pBuff, uint32 uiBufSize);
        [DllImport("Protocal.dll", EntryPoint = "proto_unpack_member_lost_mic", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int proto_unpack_member_lost_mic(PTT_MemberLostMic pMgm, ref byte pBuff, uint uiBufSize);

        /******************************************************************************************
        功能:获取GPS信息ACK
        输入：pBuff指向接收缓冲区 uiBufSize缓冲区长度
        返回：非0表示成功
        注意：使用该函数解包成功后组列表会被保存到stLocList结构体形成的链表中，
              使用proto_get_local_list函数可以获取链表头指针位置，用户可以使用链表的正向或反向指针
              访问节点，知道指向NULL结束
        ******************************************************************************************/
        //int proto_unpack_local_request_ack(uint8* pBuff, uint32 uiBufSize);
        [DllImport("Protocal.dll", EntryPoint = "proto_unpack_local_request_ack", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern int proto_unpack_local_request_ack(ref byte pBuff, uint uiBufSize);
        /*
        int proto_unpack_temp_call_arrived(PTT_TempCallArrived *pTca, uint8 * pBuff, uint32 uiBufSize);
        int proto_unpack_local_report_ack(uint8 *pBuff, uint32 uiBufSize);
        */
        /******************************************************************************************
        功能:获取组内成员列表指针，需要先发起获取请求再调用该函数
        ******************************************************************************************/
        //stUserList* proto_get_user_list(void);
        [DllImport("Protocal.dll", EntryPoint = "proto_get_user_list", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //[return: MarshalAs(UnmanagedType.Struct)]
        public static extern IntPtr proto_get_user_list();//stUserList

        /******************************************************************************************
        功能:获取组列表指针，需要先发起获取请求再调用该函数
        ******************************************************************************************/
        //stGroupList* proto_get_group_list(void);
        [DllImport("Protocal.dll", EntryPoint = "proto_get_group_list", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr proto_get_group_list();//stGroupList

        /******************************************************************************************
        功能:获取定位信息点指针，需要先发起获取请求再调用该函数
        ******************************************************************************************/
        //stLocList* proto_get_local_list(void);
        [DllImport("Protocal.dll", EntryPoint = "proto_get_local_list", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern IntPtr proto_get_local_list();//stLocList
    }
}
