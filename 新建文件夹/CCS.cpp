#include<stdexcept>
#include <windows.h>
#include <iostream>
#include "Evrc.h"
#define PCM_PRE_FRAME_SIZE		320

//测试
extern "C" __declspec(dllexport) double add(double a, double b)
{
	return a + b;
}

//evrc编码，回参evrc必须是23个长度的unsigned char（Unsigned 8  bit value）
extern "C" __declspec(dllexport) void encode(uint8 *evrc, uint8 *pcm)
{
	HMODULE hMod = LoadLibrary(L"Evrc.dll");//dll路径
	if (hMod)
	{
		typedef void(*FUNB)();
		FUNB funb = (FUNB)GetProcAddress(hMod, LPCSTR("InitEncoder"));//直接使用原工程函数名 
		if (funb != NULL)
		{
			//InitEncoder();
			funb();
			typedef void(*FUNA)(uint8*, uint8*);
			FUNA fun = (FUNA)GetProcAddress(hMod, LPCSTR("evrc_encode"));//直接使用原工程函数名 
			if (fun != NULL)
			{
				uint8 ucEvrc[23];
				ucEvrc[0] = HALF_RATE;
				//PCM音频数据编码，并向服务器发UDP包
				//evrc_encode(&ucEvrc[1], pcm);
				fun(&ucEvrc[1], pcm);
				evrc = ucEvrc;
			}
		}
		FreeLibrary(hMod);
	}
}

//evrc解码
extern "C" __declspec(dllexport) void decode(uint8 *evrc, uint8 *pcm)
{
	HMODULE hMod = LoadLibrary(L"Evrc.dll");//dll路径
	if (hMod)
	{
		typedef void(*FUNB)();
		FUNB funb = (FUNB)GetProcAddress(hMod, LPCSTR("InitDecoder"));//直接使用原工程函数名 
		if (funb != NULL)
		{
			//InitDecoder();
			funb();
			typedef void(*FUNA)(uint8*, uint8*);
			FUNA fun = (FUNA)GetProcAddress(hMod, LPCSTR("evrc_decode"));//直接使用原工程函数名 
			if (fun != NULL)
			{
				uint8 i;
				uint8 ucFrameNum = evrc[12];
				uint16 uiDatLen = 16;
				uint8 ucEvrcDat[23];
				//uint8 *pPcm;

				if (ucFrameNum <= 0x0a)
				{
					for (i = 0; i<ucFrameNum; i++)
					{
						switch ((evrc[13 + (i >> 2)] >> (6 - (i % 4) * 2)) & 0x03)
						{
						case 0x02:
							ucEvrcDat[0] = HALF_RATE;
							memcpy(&ucEvrcDat[1], &evrc[uiDatLen], 10);
							pcm = (uint8*)malloc(PCM_PRE_FRAME_SIZE);
							//evrc_decode(ucEvrcDat, pcm);//解码
							fun(ucEvrcDat, pcm);//解码
												//m_pPlaySound->PostThreadMessage(WM_PLAYSOUND_PLAYBLOCK, (WPARAM)PCM_PRE_FRAME_SIZE, (LPARAM)pPcm);//触发播放事件
							uiDatLen += 10;
							break;

						case 0x03:
							ucEvrcDat[0] = FULL_RATE;
							memcpy(&ucEvrcDat[1], &evrc[uiDatLen], 22);
							pcm = (uint8*)malloc(PCM_PRE_FRAME_SIZE);
							//evrc_decode(ucEvrcDat, pcm);
							fun(ucEvrcDat, pcm);
							//m_pPlaySound->PostThreadMessage(WM_PLAYSOUND_PLAYBLOCK, (WPARAM)PCM_PRE_FRAME_SIZE, (LPARAM)pPcm);
							uiDatLen += 22;
							break;

						default:
							//_DEBUG("OutWrite unknow FrameRate!\r\n");
							break;
						}
					}
				}
			}
		}
		FreeLibrary(hMod);
	}
}
