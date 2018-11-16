using System;
using System.Runtime.InteropServices;
using System.Text;

// ReSharper disable InconsistentNaming

namespace EVRCTest
{
    public class PcmAudio
    {
        public const int MMSYSERR_NOERROR = 0; // no error

        public const int MM_WOM_OPEN = 0x3BB;
        public const int MM_WOM_CLOSE = 0x3BC;
        public const int MM_WOM_DONE = 0x3BD;

        public const int MM_WIM_OPEN = 0x3BE;
        public const int MM_WIM_CLOSE = 0x3BF;
        public const int MM_WIM_DATA = 0x3C0;

        public const int CALLBACK_TYPEMASK = 0x00070000;    /* callback type mask */
        public const int CALLBACK_NULL = 0x00000000;    /* no callback */
        public const int CALLBACK_WINDOW = 0x00010000;    /* dwCallback is a HWND */
        public const int CALLBACK_TASK = 0x00020000;    /* dwCallback is a HTASK */
        public const int CALLBACK_FUNCTION = 0x00030000;    /* dwCallback is a FARPROC */
        public const int CALLBACK_THREAD = (CALLBACK_TASK);/* thread ID replaces 16 bit task */
        public const int CALLBACK_EVENT = 0x00050000;    /* dwCallback is an EVENT Handle */

        //调用wavein的dll
        [DllImport("winmm.dll")]
        //获取有多少可用输入设备
        public static extern int waveInGetNumDevs();
        [DllImport("winmm.dll")]
        //增加一个缓冲区
        public static extern int waveInAddBuffer(IntPtr hwi, ref WaveHdr pwh, UInt32 cbwh);
        [DllImport("winmm.dll")]
        //关闭麦克风
        public static extern int waveInClose(IntPtr hwi);
        [DllImport("winmm.dll")]
        //打开麦克风
        public static extern int waveInOpen(out IntPtr phwi, int uDeviceID, ref WaveFormatEx lpFormat, WaveDelegate dwCallback, UInt32 dwInstance, UInt32 dwFlags);
        [DllImport("winmm.dll")]
        //标记为可用的缓冲区
        public static extern int waveInPrepareHeader(IntPtr hWaveIn, ref WaveHdr lpWaveInHdr, UInt32 uSize);
        [DllImport("winmm.dll")]
        //标记为不可用的缓冲区
        public static extern int waveInUnprepareHeader(IntPtr hWaveIn, ref WaveHdr lpWaveInHdr, UInt32 uSize);
        [DllImport("winmm.dll")]
        //把缓冲区内容重置
        public static extern int waveInReset(IntPtr hwi);
        [DllImport("winmm.dll")]
        //开始录制
        public static extern int waveInStart(IntPtr hwi);
        [DllImport("winmm.dll")]
        //停止录制
        public static extern int waveInStop(IntPtr hwi);

        [StructLayout(LayoutKind.Sequential)]
        //接受的波形数据放入的缓冲区
        public struct WaveHdr
        {

            public IntPtr lpData;//缓冲区
            public UInt32 dwBufferLength;//缓冲区长度
            public UInt32 dwBytesRecorded;//某一刻读取到了多少字节的数据
            public UInt32 dwUser;//自定义数据
            public UInt32 dwFlags;
            public UInt32 dwLoops;//是否循环
            public IntPtr lpNext;//链表的下一缓冲区
            public UInt32 reserved;//没实际意义
        }

        [StructLayout(LayoutKind.Sequential)]
        //波形格式
        public struct WaveFormatEx
        {
            public UInt16 wFormatTag;//波形的类型
            public UInt16 nChannels;//通道数（1，单声道   2，立体音）
            public UInt32 nSamplesPerSec;//采样率
            public UInt32 nAvgBytesPerSec;//字节率
            public UInt16 nBlockAlign;
            public UInt16 wBitsPerSample;//每个样多少位
            public UInt16 cbSize;//长度
        }

        public delegate void WaveDelegate(IntPtr hwi, UInt32 uMsg, UInt32 dwInstance, UInt32 dwParam1, UInt32 dwParam2);

        //// consts
        //public const int MMSYSERR_NOERROR = 0; // no error

        //public const int MM_WOM_OPEN = 0x3BB;
        //public const int MM_WOM_CLOSE = 0x3BC;
        //public const int MM_WOM_DONE = 0x3BD;

        //public const int MM_WIM_OPEN = 0x3BE;
        //public const int MM_WIM_CLOSE = 0x3BF;
        //public const int MM_WIM_DATA = 0x3C0;

        //public const int CALLBACK_TYPEMASK = 0x00070000;    /* callback type mask */
        //public const int CALLBACK_NULL = 0x00000000;    /* no callback */
        //public const int CALLBACK_WINDOW = 0x00010000;    /* dwCallback is a HWND */
        //public const int CALLBACK_TASK = 0x00020000;    /* dwCallback is a HTASK */
        //public const int CALLBACK_FUNCTION = 0x00030000;    /* dwCallback is a FARPROC */
        //public const int CALLBACK_THREAD = (CALLBACK_TASK);/* thread ID replaces 16 bit task */
        //public const int CALLBACK_EVENT = 0x00050000;    /* dwCallback is an EVENT Handle */

        //public const int TIME_MS = 0x0001;  // time in milliseconds 
        //public const int TIME_SAMPLES = 0x0002;  // number of wave samples 
        //public const int TIME_BYTES = 0x0004;  // current byte offset 

        //// callbacks
        //public delegate void WaveDelegate(IntPtr hdrvr, int uMsg, int dwUser, ref WaveHdr wavhdr, int dwParam2);

        //// structs 

        //[StructLayout(LayoutKind.Sequential)]
        //public struct WaveHdr
        //{
        //    public string lpData; // pointer to locked data buffer
        //    public int dwBufferLength; // length of data buffer
        //    public int dwBytesRecorded; // used for input only
        //    public IntPtr dwUser; // for client's use
        //    public int dwFlags; // assorted flags (see defines)
        //    public int dwLoops; // loop control counter
        //    public IntPtr lpNext; // PWaveHdr, reserved for driver
        //    public int reserved; // reserved for driver
        //}

        private const string mmdll = "winmm.dll";

        // WaveOut calls
        [DllImport(mmdll)]
        public static extern int waveOutGetNumDevs();
        [DllImport(mmdll)]
        public static extern int waveOutPrepareHeader(IntPtr hWaveOut, ref WaveHdr lpWaveOutHdr, UInt32 uSize);
        [DllImport(mmdll)]
        public static extern int waveOutUnprepareHeader(IntPtr hWaveOut, ref WaveHdr lpWaveOutHdr, UInt32 uSize);
        [DllImport(mmdll)]
        public static extern int waveOutWrite(IntPtr hWaveOut, ref WaveHdr lpWaveOutHdr, UInt32 uSize);
        [DllImport(mmdll)]
        public static extern int waveOutOpen(out IntPtr hWaveOut, int uDeviceID, ref WaveFormatEx lpFormat, WaveDelegate dwCallback, UInt32 dwInstance, UInt32 dwFlags);
        [DllImport(mmdll)]
        public static extern int waveOutReset(IntPtr hWaveOut);
        [DllImport(mmdll)]
        public static extern int waveOutClose(IntPtr hWaveOut);
        [DllImport(mmdll)]
        public static extern int waveOutPause(IntPtr hWaveOut);
        [DllImport(mmdll)]
        public static extern int waveOutRestart(IntPtr hWaveOut);
        [DllImport(mmdll)]
        public static extern int waveOutGetPosition(IntPtr hWaveOut, out int lpInfo, UInt32 uSize);
        [DllImport(mmdll)]
        public static extern int waveOutSetVolume(IntPtr hWaveOut, UInt32 dwVolume);
        [DllImport(mmdll)]
        public static extern int waveOutGetVolume(IntPtr hWaveOut, out UInt32 dwVolume);

        //// WaveIn calls
        //[DllImport(mmdll)]
        //public static extern int waveInGetNumDevs();
        //[DllImport(mmdll)]
        //public static extern int waveInAddBuffer(IntPtr hwi, ref WaveHdr pwh, int cbwh);
        //[DllImport(mmdll)]
        //public static extern int waveInClose(IntPtr hwi);
        //[DllImport(mmdll)]
        //public static extern int waveInOpen(out IntPtr phwi, UInt32 uDeviceID, ref WaveFormat lpFormat, WaveDelegate dwCallback, int dwInstance, int dwFlags);
        //[DllImport(mmdll)]
        //public static extern int waveInPrepareHeader(IntPtr hWaveIn, ref WaveHdr lpWaveInHdr, int uSize);
        //[DllImport(mmdll)]
        //public static extern int waveInUnprepareHeader(IntPtr hWaveIn, ref WaveHdr lpWaveInHdr, int uSize);
        //[DllImport(mmdll)]
        //public static extern int waveInReset(IntPtr hwi);
        //[DllImport(mmdll)]
        //public static extern int waveInStart(IntPtr hwi);
        //[DllImport(mmdll)]
        //public static extern int waveInStop(IntPtr hwi);



        ////------------------------以下是开始采集

        //[StructLayout(LayoutKind.Sequential, Pack = 1)]
        //public struct WAVEFORMAT
        //{
        //    public ushort wFormatTag;         /* format type */
        //    public ushort nChannels;          /* number of channels (i.e. mono, stereo...) */
        //    public uint nSamplesPerSec;     /* sample rate */
        //    public uint nAvgBytesPerSec;    /* for buffer estimation */
        //    public ushort nBlockAlign;        /* block size of data */
        //    public ushort wBitsPerSample;     /* number of bits per sample of mono data */
        //    public ushort cbSize;             /* the count in bytes of the size of */
        //    /* extra information (after cbSize) */
        //}

        //public delegate void WaveDelegate(int hdrvr, int uMsg, int dwUser, int wavhdr, int dwParam2);

        ////开启音频采集
        ////waveInOpen
        //[DllImport("winmm.dll", EntryPoint = "waveInOpen")]
        //public static extern int waveInOpen(out IntPtr lphWaveIn, int uDeviceID, WAVEFORMAT lpFormat, WaveDelegate dwCallback, int dwInstance, int dwFlags);

        //[StructLayout(LayoutKind.Sequential, Pack = 1)]
        //public class WAVEHDR
        //{
        //    public string lpData;                 /* pointer to locked data buffer */
        //    public uint dwBufferLength;         /* length of data buffer */
        //    public uint dwBytesRecorded;        /* used for input only */
        //    public ulong dwUser;                 /* for client's use */
        //    public uint dwFlags;                /* assorted flags (see defines) */
        //    public uint dwLoops;                /* loop control counter */
        //    public IntPtr lpNext;     /* reserved for driver  链表wavehdr_tag类型*/
        //    public ulong reserved;               /* reserved for driver */
        //}

        ////准备一个bufrer给输入设备
        ////waveInPrepareHeader
        //[DllImport("winmm.dll", EntryPoint = "waveInPrepareHeader")]
        //public static extern int waveInPrepareHeader(IntPtr lphWaveIn, ref WAVEHDR lpWaveInHdr, int uSize);

        ////发送一个buffer给指定的输入设备，当buffer填满将会通知程序
        ////waveInAddBuffer
        //[DllImport("winmm.dll", EntryPoint = "waveInAddBuffer")]
        //public static extern int waveInAddBuffer(IntPtr hWaveIn, ref WAVEHDR lpWaveInHdr, int uSize);

        ////开启指定的输入采集设备
        ////waveInStart
        //[DllImport("winmm.dll", EntryPoint = "waveInStart")]
        //public static extern int waveInStart(IntPtr hWaveIn);

        ////-------------------------以下是停止采集

        ////停止音频采集
        ////waveInStop
        //[DllImport("winmm.dll", EntryPoint = "waveInStop")]
        //public static extern int waveInStop(IntPtr hWaveIn);

        ////重置设备
        ////waveInReset
        //[DllImport("winmm.dll", EntryPoint = "waveInReset")]
        //public static extern int waveInReset(IntPtr hWaveIn);

        ////重置设备成功，立即关闭设备
        ////waveInClose
        //[DllImport("winmm.dll", EntryPoint = "waveInClose")]
        //public static extern int waveInClose(IntPtr hWaveIn);

        ////--------------------------以下是获取到音频数据OnData

        ////使采集过程，知道此buffer已经沾满，不能再填充
        ////waveInUnprepareHeader
        //[DllImport("winmm.dll", EntryPoint = "waveInUnprepareHeader")]
        //public static extern int waveInUnprepareHeader(IntPtr hWaveIn, ref WAVEHDR lpWaveInHdr, int uSize);


        ////重新将buffer恢复到准备填充状态
        ////waveInPrepareHeader
        ////waveInAddBuffer
    }

    public enum WaveFormats
    {
        Pcm = 1,
        Float = 3
    }

    public enum Quality
    {
        low = 1,
        Normal = 0,
        Height = 2
    }

    [StructLayout(LayoutKind.Sequential)]
    public class WaveFormat
    {
        /// <summary>
        /// 
        /// </summary>
        public short wFormatTag;//波形的类型
        public short nChannels;//通道数（1，单声道   2，立体音）
        public int nSamplesPerSec;
        public int nAvgBytesPerSec;
        public short nBlockAlign;
        public short wBitsPerSample;
        public short cbSize;

        public WaveFormat(int rate, int bits, int channels)
        {
            wFormatTag = (short)WaveFormats.Pcm;
            nChannels = (short)channels;
            nSamplesPerSec = rate;
            wBitsPerSample = (short)bits;
            cbSize = 0;

            nBlockAlign = (short)(channels * (bits / 8));
            nAvgBytesPerSec = nSamplesPerSec * nBlockAlign;
        }
    }
}
