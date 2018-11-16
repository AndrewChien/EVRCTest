// CCS.cpp : 定义 DLL 应用程序的导出函数。
//

#include "stdafx.h"
#include<stdexcept>
#include <windows.h>
#include <iostream>

#include "Ccreate.h"
#include "Evrc.h"
#define PCM_PRE_FRAME_SIZE		320

#pragma comment(lib,"Evrc.lib")


using namespace std;

//测试
extern "C" __declspec(dllexport) double add(double a, double b)
{
	return a + b;
}

//evrc编码，回参evrc必须是23个长度的unsigned char（Unsigned 8  bit value）
extern "C" __declspec(dllexport) unsigned char* encode(unsigned char *pcm)
{
	unsigned char *ucEvrc = new unsigned char[23];
	ucEvrc[0] = HALF_RATE;
	//PCM音频数据编码，并向服务器发UDP包
	evrc_encode(&ucEvrc[1], pcm);
	return ucEvrc;
}

//evrc解码
extern "C" __declspec(dllexport) void decode(unsigned char *evrc, unsigned char *pcm)
{
	unsigned char i;
	unsigned char ucFrameNum = evrc[12];
	uint16 uiDatLen = 16;
	unsigned char ucEvrcDat[23];
	//unsigned char *pPcm;

	if (ucFrameNum <= 0x0a)
	{
		for (i = 0; i<ucFrameNum; i++)
		{
			switch ((evrc[13 + (i >> 2)] >> (6 - (i % 4) * 2)) & 0x03)
			{
			case 0x02:
				ucEvrcDat[0] = HALF_RATE;
				memcpy(&ucEvrcDat[1], &evrc[uiDatLen], 10);
				pcm = (unsigned char*)malloc(PCM_PRE_FRAME_SIZE);
				evrc_decode(ucEvrcDat, pcm);//解码
				uiDatLen += 10;
				break;

			case 0x03:
				ucEvrcDat[0] = FULL_RATE;
				memcpy(&ucEvrcDat[1], &evrc[uiDatLen], 22);
				pcm = (unsigned char*)malloc(PCM_PRE_FRAME_SIZE);
				evrc_decode(ucEvrcDat, pcm);
				uiDatLen += 22;
				break;

			default:
				break;
			}
		}
	}
}

////回调测试  
//int CallPFun(int(*callback)(int, int), int a, int b) {
//	return callback(a, b);
//}
//CallbackFun myCallback = NULL;
//void SetCallBackFun(CallbackFun call)
//{
//	myCallback = call;
//}

CallBackHeartBeat callHeartBeat = NULL;
void SetHeartBeat(CallBackHeartBeat call)
{
	callHeartBeat = call;
}
CallBackSendVoice callSendVoice = NULL;
void SetSendVoice(CallBackSendVoice call)
{
	callSendVoice = call;
}


void StartSpeech()
{
	InitEncoder();
	InitDecoder();
}

void StopSpeech()
{

}




void InitCCS(int heartbeatcycle)
{
	char *param1 = new char[1024];
	char *param2 = new char[1024];
	for(int i=0;i<10;i++)
	{
		param1[i] = i;
		param2[i] = heartbeatcycle;
	}
	callHeartBeat(param1, param2);
	callSendVoice(param1, param2);
}

void ReleaseCCS()
{

}