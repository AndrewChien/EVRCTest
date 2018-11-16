/**************************************************************************************
说明：
	本文件中的数据结构与库文件中的使用的结构紧密联系，请不要随意改动结构体里面的参数，
	否则可以导致程序运行不稳定或者部分功能无法实现。
***************************************************************************************/
#ifndef __ZPOC_H__
#define __ZPOC_H__

typedef  unsigned char      boolean;     /* Boolean value type. */  
typedef  unsigned long      uint64;  
typedef  unsigned int       uint32;      /* Unsigned 32 bit value */  
typedef  unsigned short     uint16;      /* Unsigned 16 bit value */  
typedef  unsigned char      uint8;       /* Unsigned 8  bit value */  
  
typedef  signed long int    int32;       /* Signed 32 bit value */  
typedef  signed short       int16;       /* Signed 16 bit value */  
typedef  signed char        int8;        /* Signed 8  bit value */  
  

#define NET_WORK_SEND_FLG		MSG_NOSIGNAL

#define POC_NET_TCP_PORT		2074
#define POC_NET_UDP_PORT		2075

#define POC_GET_IP_PORT			3074

#define POC_NET_TCP_SOCKET		0
#define POC_NET_UDP_SOCKET		1

#define TCP_SND_SIZE			1024
#define TCP_REC_SIZE			102400

#define TCP_MAX_PACK_SIZE		20480

#define UDP_BUF_SIZE 			300

#define POC_SETING_IP_MAX_LEN		16
#define POC_SETING_ACC_MAX_LEN		20
#define POC_SETING_PASS_MAX_LEN		20
#define POC_SETING_DOMAIN_MAX_LEN	50

#define POC_SETING_APN_INFOR_LEN	50
#define POC_SETING_AGENT_PWD_LEN	20

typedef struct
{
	char ip[POC_SETING_IP_MAX_LEN];
	char account[POC_SETING_ACC_MAX_LEN];
	char passwd[POC_SETING_PASS_MAX_LEN];

	char apnname[POC_SETING_APN_INFOR_LEN];
	char apnacc[POC_SETING_APN_INFOR_LEN];
	char apnpwd[POC_SETING_APN_INFOR_LEN];

	char agentpwd[POC_SETING_AGENT_PWD_LEN];

	char inviteFlg;
	char offlineFlg;

	uint32 uiApnValidFlg;
	uint32 uiServerCfg;
	uint32 ucValidFlg;
}stPocSetPar;


typedef struct tagUser
{
	int32 uid;
	stPocSetPar iSetPar;
	char *pValidIp;
	
	char username[50];
    int32 version;
    char platform[10];
    char device[10];
    char meid[16];
    uint32 expect_payload;

    uint32 default_group;
    uint32 loc_report_period;
    boolean audio_enabled;
    uint32 cfg_ptt_timeout;
    uint32 heart_inter;
}stUser;

typedef struct
{
	uint8 udpHbCnt;
	uint8 tcpHbCnt;
}stPocRunPar;

typedef struct
{
	stUser iUser;
	stPocRunPar iPocRun;
}stPocSeting;

typedef struct tagLocPoint
{
	uint32 uid;
    char longitude[12];
    char latitude[12];
    char time[20];
}stLocPoint;

struct stLocNode
{
	stLocPoint iPoint;
	struct stLocNode *pNext;
	struct stLocNode *pPrev;
};

typedef struct tagLocList
{
	uint32 uiTotal;
	struct stLocNode *pCurr;
	struct stLocNode *pHead;
	struct stLocNode *pTial;
}stLocList;

typedef struct tagGroup
{
	uint32 gid;
	uint32 number_n;
	char gname[50];
}stGroup;

struct stGroupNode
{
	stGroup iGroup;
	struct stGroupNode *pNext;
	struct stGroupNode *pPrev;
};

typedef struct tagGroupList
{
	uint16 uiTotal;
	struct stGroupNode *pEnter;
	struct stGroupNode *pCurr;
	struct stGroupNode *pHead;
	struct stGroupNode *pTial;
}stGroupList;

struct stUserNode
{
	unsigned int uid;
	unsigned char ingroup;
	unsigned char online;
	unsigned char uname[50];

	struct stUserNode *pNext;
	struct stUserNode *pPrev;
};

typedef struct tagUserList
{
	uint16 uiTotal;
	struct stUserNode *pCurr;
	struct stUserNode *pHead;
	struct stUserNode *pTial;
	struct stUserNode *pAddName;
}stUserList;

extern unsigned char tcpsndbuf[TCP_SND_SIZE];
extern unsigned char tcprecvbuf[TCP_REC_SIZE];
extern unsigned char udpsndbuf[UDP_BUF_SIZE];
extern unsigned char udprecvbuf[UDP_BUF_SIZE];
extern stGroupList iGroupList;
extern stUserList  iUserList;
extern stPocSeting iPocSeting;
extern int signal_event_fd[2];

#endif

