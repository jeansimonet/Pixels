#pragma once
#ifndef _DICEDEBUG_h
#define _DICEDEBUG_h

#include "Console.h"
#include "BLEConsole.h"

#define GET_MACRO(_1, _2, NAME, ...) NAME
#define debugPrint(...) GET_MACRO(__VA_ARGS__, debugPrint2, debugPrint1)(__VA_ARGS__)
#define debugPrintln(...) GET_MACRO(__VA_ARGS__, debugPrintln2, debugPrintln1)(__VA_ARGS__)

#if defined(_CONSOLE)
#define debugPrint1(x) Systems::console.print(x)
#define debugPrint2(x, y) Systems::console.print(x, y)
#define debugPrintln1(x) Systems::console.println(x)
#define debugPrintln2(x, y) Systems::console.println(x, y)
#elif defined(_BLECONSOLE)
#define debugPrint1(x) Systems::bleConsole.print(x)
#define debugPrint2(x, y) Systems::bleConsole.print(x, y)
#define debugPrintln1(x) Systems::bleConsole.println(x)
#define debugPrintln2(x, y) Systems::bleConsole.println(x, y)
#else
#define debugPrint1(x) (void)0
#define debugPrint2(x, y) (void)0
#define debugPrintln1(x) (void)0
#define debugPrintln2(x, y) (void)0
#endif

#endif
