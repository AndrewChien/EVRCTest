#pragma once

#pragma comment(lib,"CCS.lib")
#include "../CCS/Evrc.h"
extern "C" _declspec(dllimport) double __cdecl add(double a, double b);
extern "C" _declspec(dllimport) unsigned char* __cdecl encode(uint8 *pcm);
extern "C" _declspec(dllimport) void __cdecl decode(uint8 *evrc, uint8 *pcm);