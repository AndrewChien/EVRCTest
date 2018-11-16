/**************************************************************************************
说明：
	本文件中的数据结构与库文件中的使用的结构紧密联系，请不要随意改动结构体里面的参数，
	否则可以导致程序运行不稳定或者部分功能无法实现。
***************************************************************************************/
#ifndef __EVRC_H__
#define __EVRC_H__
#include "zpoc.h"

enum 
{
   BLANK_RATE,          // Indicates data was blanked
   EIGHTH_RATE,         // Indicates rate 1/8 data
   QUARTER_RATE,        // Indicates rate 1/4 data
   HALF_RATE,           // Indicates rate 1/2 data
   FULL_RATE,           // Indicates rate 1   data
   ERASURE,             // Indicates erasure frame
   MAX_RATE             // last enum
};

typedef struct _PTT_MemberGetMic 
{
    uint32 gid;
    uint32 uid;
    int has_allow;
    uint32 allow;
    int has_name;
    char name[50];
} PTT_MemberGetMic;

typedef struct _PTT_MemberLostMic
{
    uint32 gid;
    uint32 uid;
} PTT_MemberLostMic;

/*********************************************************
功能:初始化解码器
输入：
返回：
**********************************************************/
__declspec(dllexport) void InitDecoder(void);

/*********************************************************
功能:EVRC解码,一次调用只能解析一帧数据，数据长度：EVRC 10  PCM 320
输入:pEvrc
返回:pPcm
**********************************************************/
__declspec(dllexport) void evrc_decode(unsigned char *pEvrc, unsigned char *pPcm);

/*********************************************************
功能:初始化编码器
输入：
返回：
**********************************************************/
__declspec(dllexport) void InitEncoder(void);

/*********************************************************
功能:EVRC编码,一次调用只能编码一帧数据，数据长度：EVRC 10  PCM 320
输入:pPcm
返回:pEvrc
**********************************************************/
__declspec(dllexport) void evrc_encode(unsigned char *pEvrc, unsigned char *pPcm);

#endif