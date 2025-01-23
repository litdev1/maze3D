/*
  plcLib Version 1.2.0, last updated 21 December, 2015.

  A simple Programmable Logic Controller (PLC) library for the
  Arduino and compatibles.

  Author:    W. Ditch
  Publisher: www.electronics-micros.com

  This program is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details, available from:
  <http://www.gnu.org/licenses/>
*/

#define HIGH 1
#define LOW 0

#ifndef plcLib_h
#define plcLib_h

#ifndef noPinDefs
// Define basic I/O pins for Arduino Uno and compatibles
// (or leave default I/O pins unconfigured if noPinDefs is set in the user sketch)
const int X0 = 0; //A0;
const int X1 = 1; //A1;
const int X2 = 2; //A2;
const int X3 = 3; //A3;

const int Y0 = 4;
const int Y1 = 5;
const int Y2 = 6;
const int Y3 = 7;

// Define Motor Shield pin names
const int DIRA = 12;
const int DIRB = 13;
const int PWMA = 3;
const int PWMB = 11;
const int BRAKEA = 9;
const int BRAKEB = 8;
const int CURRENTA = 0; //A0;
const int CURRENTB = 1; //A1;

// Define additional I/O pins for Mega, Mega 2560 and Due
#if defined(__AVR_ATmega1280__) || defined(__AVR_ATmega2560__) || defined(__SAM3X8E__)
const int X4 = 6; //A6;
const int X5 = 7; //A7;
const int X6 = 8; //A8;
const int X7 = 9; //A9;
const int Y4 = 4;
const int Y5 = 7;
const int Y6 = 8;
const int Y7 = 12;
#endif
#endif

void setupPLC();
void setPin(int pin, int value);
int getPin(int pin);
void setMillis(unsigned long value);

unsigned int in(int input);
unsigned int in(unsigned int input);
unsigned int in(unsigned long input);
unsigned int inNot(int input);
unsigned int inNot(unsigned int input);
unsigned int inNot(unsigned long input);
unsigned int inAnalog(int input);
unsigned int inAnalog(unsigned int input);
unsigned int inAnalog(unsigned long input);
unsigned int out(int output);
unsigned int out(unsigned int& output);
unsigned int out(unsigned long& output);
unsigned int outNot(int output);
unsigned int outNot(unsigned int& output);
unsigned int outNot(unsigned long& output);
unsigned int outPWM(int output);
unsigned int andBit(int input);
unsigned int andBit(unsigned int input);
unsigned int andBit(unsigned long input);
unsigned int andNotBit(int input);
unsigned int andNotBit(unsigned int input);
unsigned int andNotBit(unsigned long input);
unsigned int orBit(int input);
unsigned int orBit(unsigned int input);
unsigned int orBit(unsigned long input);
unsigned int orNotBit(int input);
unsigned int orNotBit(unsigned int input);
unsigned int orNotBit(unsigned long input);
unsigned int xorBit(int input);
unsigned int xorBit(unsigned int input);
unsigned int xorBit(unsigned long input);

unsigned int latch(int output, int reset);
unsigned int latch(int output, unsigned int reset);
unsigned int latch(int output, unsigned long reset);
unsigned int latch(unsigned int& output, int reset);
unsigned int latch(unsigned long& output, int reset);
unsigned int latch(unsigned int& output, unsigned int reset);
unsigned int latch(unsigned long& output, unsigned long reset);

unsigned int timerOn(unsigned long& timerState, unsigned long timerPeriod);
unsigned int timerPulse(unsigned long& timerState, unsigned long timerPeriod);
unsigned int timerOff(unsigned long& timerState, unsigned long timerPeriod);
unsigned int timerCycle(unsigned long& timer1State, unsigned long timer1Period, unsigned long& timer2State, unsigned long timer2Period);

unsigned int compareGT(int input);
unsigned int compareGT(unsigned int input);
unsigned int compareGT(unsigned long input);
unsigned int compareLT(int input);
unsigned int compareLT(unsigned int input);
unsigned int compareLT(unsigned long input);
unsigned int set(int output);
unsigned int set(unsigned int& output);
unsigned int set(unsigned long& output);
unsigned int reset(int output);
unsigned int reset(unsigned int& output);
unsigned int reset(unsigned long& output);

class Counter
    //typedef struct counter
{
public:
    Counter(unsigned int presetValue);
    Counter(unsigned int presetValue, unsigned int direction);
    void countUp();
    void countDown();
    void preset();
    void clear();
    unsigned int upperQ();
    unsigned int lowerQ();
    unsigned int count();
    unsigned int presetValue();
    // Counter
    unsigned int setPresetValue(unsigned int pv);
    //

private:
    unsigned int _pv;
    unsigned int _ct;
    unsigned int _ctUpEdge;
    unsigned int _ctDownEdge;
    unsigned int _uQ;
    unsigned int _lQ;
};

class Shift
    //typedef struct shift 
{
public:
    Shift();
    Shift(unsigned int sreg);
    unsigned int bitValue(unsigned int bitno);
    unsigned int value();
    void reset();
    void inputBit();
    void shiftLeft();
    void shiftRight();
private:
    unsigned int _srLeftEdge;
    unsigned int _srRightEdge;
    unsigned int _sreg;
    unsigned int _inbit;
};

class Stack
    //typedef struct stack
{
public:
    Stack();
    void push();
    void pop();
    void orBlock();
    void andBlock();
private:
    unsigned long _sreg;
};

class Pulse
    //typedef struct pulse
{
public:
    Pulse();
    void inClock();
    void rising();
    void falling();
private:
    unsigned int _pulseInput;
    unsigned int _pulseUpEdge;
    unsigned int _pulseDownEdge;
};

#endif

// Functions from classes (Counter, Shift, Stack, Pulse)
// Counter
/*
void newCounter(Counter *pctr, unsigned int pv);
unsigned int setPresetValue(Counter *pctr, unsigned int pv);	// Counter method
void newCounter2(Counter *pctr, unsigned int pv, unsigned int direction);	// Counter constructor method
unsigned int presetValue(Counter *pctr);	// Return preset value method
void clear(Counter *pctr);				// Clear counter method
void preset(Counter *pctr);				// Preset counter method
unsigned int upperQ(Counter *pctr);		// Read up counter upper Q output method
unsigned int lowerQ(Counter *pctr);		// Read down counter lower Q output method
unsigned int count(Counter *pctr);		// Return current count value method
void countUp(Counter *pctr);			// Count up method
void countDown(Counter *pctr);			// Count down method
// Shift
void newShift(Shift *pshift);						// Shift register constructor method
void newShift2(Shift *pshift, unsigned int sreg);   // Shift register constructor method
unsigned int bitValue(Shift *pshift, unsigned int bitno);	// Read a bit at a specified position
unsigned int value(Shift *pshift);			// Return the current shift register value
void resetShift(Shift *pshift);					// Reset the shift register if scanValue = 0
void inputBit(Shift *pshift);				// Set the input bit of the shift register
void shiftRight(Shift *pshift);			    // Shift right method
void shiftLeft(Shift *pshift);				// Shift left method
// Stack
void newStack(Stack *pStack);				// Stack constructor method
void push(Stack *pStack);					// Push scanValue bit onto the stack method
void pop(Stack *pStack);					// Pop scanValue bit from the stack method
void orBlock(Stack *pStack);				// OR scanValue with value Popped from stack method
void andBlock(Stack *pStack);				// AND scanValue with value Popped from stack method
// Pulse
void newPulse(Pulse *pPulse);				// Pulse constructor method
void inClock(Pulse *pPulse);				// Read the clock input method
void rising(Pulse *pPulse);		            // Pulse rising edge detected method
void falling(Pulse *pPulse);		        // Pulse falling edge detected method
*/
