#pragma once

#include "plcLib.h"

unsigned long TIMER0 = 0;
unsigned long TIMER1 = 0;

void setup()
{
}

void loop()
{
	//Forwards W key or toggle forward in bursts S key
	in(X3);
	timerCycle(TIMER0, 200, TIMER1, 500);   // cycle activated by X3 (S)
	orBit(X0);
	out(Y0);

	//Left A key
	in(X1);
	out(Y1);

	//Right D key
	in(X2);
	out(Y2);
}
