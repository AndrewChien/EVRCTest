// CCSTest.cpp : 定义控制台应用程序的入口点。
//

#include "stdafx.h"
#include "CCSTest.h"
#include <iostream>
//#include <libloaderapi.h>
//#include <winbase.h>

//测试一
int * fun1();
int * fun2();
void dispArr(int *arr, int n);
const int arrlen = 10;

////测试二
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
	////测试一
	////方法一,返回局部变量的首地址
	//int * arr;
	//arr = fun1();
	//std::cout << "这是方法一" << std::endl;
	//dispArr(arr, arrlen);
	////cout << fun1()[1] << endl;//我们可以通过这样的方式将数组未被销毁的内容输出，但这样做没有意义
	///*方法二，在函数内部通过new动态创建数组，
	//然后记得在main函数使用完数组后将其delete下*/
	//std::cout << "这是方法二" << std::endl;
	//int *arr1;
	//arr1 = fun2();
	//dispArr(arr1, arrlen);
	//delete arr1;

	////测试二
	//HMODULE p = LoadLibrary(L"CCS.dll");
	//SetCallBackFun setcallbackfun = (SetCallBackFun)GetProcAddress(p, "SetCallBackFun");
	//Test mytest = (Test)GetProcAddress(p, "StopSpeech");
	//setcallbackfun(myfunction);
	//mytest();

	////测试三，编码调用测试
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

	//asm测试
	TestAsm();

	return 0;
}

//注意，改方法不能返回数组
int *fun1()
{
	int temp[arrlen];

	for (int i = 0; i < arrlen; ++i)
	{
		temp[i] = i;
	}

	return temp;
}

//该方法可以返回数组
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
	std::cout << "输入一个整数：" << std::endl;
	std::cin >> a;

	unsigned int *c = &a;
	__asm
	{
		mov eax, c; //指针c中存储的内容（a的地址）移动到累积寄存器eax 
		mov eax, [eax]; //取出eax值（a的地址）所指向的地址的值（即*a），移动到eax中
		add eax, 1;//eax的值加1
		mov a, eax;//eax的值移动到a中
	}
	std::cout << a << std::endl;
	std::cin >> inputKey;
	return 0;
}