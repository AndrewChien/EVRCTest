// CCSTest.cpp : �������̨Ӧ�ó������ڵ㡣
//

#include "stdafx.h"
#include "CCSTest.h"
#include <iostream>
//#include <libloaderapi.h>
//#include <winbase.h>

//����һ
int * fun1();
int * fun2();
void dispArr(int *arr, int n);
const int arrlen = 10;

////���Զ�
//typedef void(*CallbackFun)(const char* input);
//typedef void(*SetCallBackFun)(CallbackFun callbackfun);
//typedef void(*Test)();
//void myfunction(const char* input)
//{
//	printf(input);
//}

int TestAsm();

int main()
{
	////����һ
	////����һ,���ؾֲ��������׵�ַ
	//int * arr;
	//arr = fun1();
	//std::cout << "���Ƿ���һ" << std::endl;
	//dispArr(arr, arrlen);
	////cout << fun1()[1] << endl;//���ǿ���ͨ�������ķ�ʽ������δ�����ٵ������������������û������
	///*���������ں����ڲ�ͨ��new��̬�������飬
	//Ȼ��ǵ���main����ʹ�����������delete��*/
	//std::cout << "���Ƿ�����" << std::endl;
	//int *arr1;
	//arr1 = fun2();
	//dispArr(arr1, arrlen);
	//delete arr1;

	////���Զ�
	//HMODULE p = LoadLibrary(L"CCS.dll");
	//SetCallBackFun setcallbackfun = (SetCallBackFun)GetProcAddress(p, "SetCallBackFun");
	//Test mytest = (Test)GetProcAddress(p, "StopSpeech");
	//setcallbackfun(myfunction);
	//mytest();

	////��������������ò���
	//unsigned char *evrc = new unsigned char[23];
	//unsigned char Pcm[320];
	//unsigned char *rtn = encode(&Pcm[0]);
	//for (int i=0;i<23;i++)
	//{
	//	std::cout << rtn[i] << " ";
	//}
	//std::cout << std::endl;
	//delete rtn;
	//getchar();

	//asm����
	TestAsm();

	return 0;
}

//ע�⣬�ķ������ܷ�������
int *fun1()
{
	int temp[arrlen];

	for (int i = 0; i < arrlen; ++i)
	{
		temp[i] = i;
	}

	return temp;
}

//�÷������Է�������
int *fun2()
{
	int *temp = new int[arrlen];

	for (int i = 0; i < arrlen; i++)
	{
		temp[i] = i;
	}

	return temp;
}

void dispArr(int* arr, int n)
{
	for (int i = 0; i < n; i++)
	{
		std::cout << "arr" << "[" << i << "]" << " is:" << arr[i] << std::endl;
	}
}

int TestAsm()
{
	unsigned int a;
	char inputKey;
	std::cout << "����һ��������" << std::endl;
	std::cin >> a;

	unsigned int *c = &a;
	__asm
	{
		mov eax, c; //ָ��c�д洢�����ݣ�a�ĵ�ַ���ƶ����ۻ��Ĵ���eax 
		mov eax, [eax]; //ȡ��eaxֵ��a�ĵ�ַ����ָ��ĵ�ַ��ֵ����*a�����ƶ���eax��
		add eax, 1;//eax��ֵ��1
		mov a, eax;//eax��ֵ�ƶ���a��
	}
	std::cout << a << std::endl;
	std::cin >> inputKey;
	return 0;
}