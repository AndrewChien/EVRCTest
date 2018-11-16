#include<stdexcept>
#include <windows.h>
#include <iostream>
#include "Evrc.h"
#define PCM_PRE_FRAME_SIZE		320

//����
extern "C" __declspec(dllexport) double add(double a, double b)
{
	return a + b;
}

//evrc���룬�ز�evrc������23�����ȵ�unsigned char��Unsigned 8  bit value��
extern "C" __declspec(dllexport) void encode(uint8 *evrc, uint8 *pcm)
{
	HMODULE hMod = LoadLibrary(L"Evrc.dll");//dll·��
	if (hMod)
	{
		typedef void(*FUNB)();
		FUNB funb = (FUNB)GetProcAddress(hMod, LPCSTR("InitEncoder"));//ֱ��ʹ��ԭ���̺����� 
		if (funb != NULL)
		{
			//InitEncoder();
			funb();
			typedef void(*FUNA)(uint8*, uint8*);
			FUNA fun = (FUNA)GetProcAddress(hMod, LPCSTR("evrc_encode"));//ֱ��ʹ��ԭ���̺����� 
			if (fun != NULL)
			{
				uint8 ucEvrc[23];
				ucEvrc[0] = HALF_RATE;
				//PCM��Ƶ���ݱ��룬�����������UDP��
				//evrc_encode(&ucEvrc[1], pcm);
				fun(&ucEvrc[1], pcm);
				evrc = ucEvrc;
			}
		}
		FreeLibrary(hMod);
	}
}

//evrc����
extern "C" __declspec(dllexport) void decode(uint8 *evrc, uint8 *pcm)
{
	HMODULE hMod = LoadLibrary(L"Evrc.dll");//dll·��
	if (hMod)
	{
		typedef void(*FUNB)();
		FUNB funb = (FUNB)GetProcAddress(hMod, LPCSTR("InitDecoder"));//ֱ��ʹ��ԭ���̺����� 
		if (funb != NULL)
		{
			//InitDecoder();
			funb();
			typedef void(*FUNA)(uint8*, uint8*);
			FUNA fun = (FUNA)GetProcAddress(hMod, LPCSTR("evrc_decode"));//ֱ��ʹ��ԭ���̺����� 
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
							//evrc_decode(ucEvrcDat, pcm);//����
							fun(ucEvrcDat, pcm);//����
												//m_pPlaySound->PostThreadMessage(WM_PLAYSOUND_PLAYBLOCK, (WPARAM)PCM_PRE_FRAME_SIZE, (LPARAM)pPcm);//���������¼�
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
