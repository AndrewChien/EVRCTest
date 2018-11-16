/**************************************************************************************
˵����
	���ļ��е����ݽṹ����ļ��е�ʹ�õĽṹ������ϵ���벻Ҫ����Ķ��ṹ������Ĳ�����
	������Ե��³������в��ȶ����߲��ֹ����޷�ʵ�֡�
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
����:��ʼ��������
���룺
���أ�
**********************************************************/
__declspec(dllexport) void InitDecoder(void);

/*********************************************************
����:EVRC����,һ�ε���ֻ�ܽ���һ֡���ݣ����ݳ��ȣ�EVRC 10  PCM 320
����:pEvrc
����:pPcm
**********************************************************/
__declspec(dllexport) void evrc_decode(unsigned char *pEvrc, unsigned char *pPcm);

/*********************************************************
����:��ʼ��������
���룺
���أ�
**********************************************************/
__declspec(dllexport) void InitEncoder(void);

/*********************************************************
����:EVRC����,һ�ε���ֻ�ܱ���һ֡���ݣ����ݳ��ȣ�EVRC 10  PCM 320
����:pPcm
����:pEvrc
**********************************************************/
__declspec(dllexport) void evrc_encode(unsigned char *pEvrc, unsigned char *pPcm);

#endif