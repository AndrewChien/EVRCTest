#pragma once

#ifndef Ccreate_H_
#define Ccreatel_H_

typedef  int(*addP)(int, int);
typedef void(*CallbackFun)(const char* input);//回调委托
typedef  void(*CallBackHeartBeat)(char*, char*);
typedef  void(*CallBackSendVoice)(char*, char*);




#ifdef _EXPORTING 
#define API_DECLSPEC extern "C" _declspec(dllexport) 
#else 
#define API_DECLSPEC  extern "C" _declspec(dllimport) 
#endif

API_DECLSPEC int CallPFun(addP callback, int a, int b);
API_DECLSPEC void SetCallBackFun(CallbackFun callbackfun);//设置回调
API_DECLSPEC void SetHeartBeat(CallBackHeartBeat callback);
API_DECLSPEC void SetSendVoice(CallBackSendVoice callback);


API_DECLSPEC void StartSpeech();

API_DECLSPEC void StopSpeech();

API_DECLSPEC void InitCCS(int heartbeatcycle);

API_DECLSPEC void ReleaseCCS();
#endif
