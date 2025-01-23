/*
  plcLib Version 1.2.0, last updated 21 December, 2015.
  
  A simple Programmable Logic Controller (PLC) library for the
  Ardno and compatibles.

  Author:    W. Ditch - converted to C# by S Todman
  Publisher: www.electronics-micros.com

  This program is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be usef,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICAR PURPOSE.  See the
  GNU General Public License for more details, available from:
  <http://www.gnu.org/licenses/>
*/

using System;
using System.Numerics;

namespace plc3D
{
    public static class plcLibrary
    {
        private static Compiler compiler = new Compiler();
        private static bool compiled = false;

        const int HIGH = 1;
        const int LOW = 0;

        // Define basic I/O pins for Arduino Uno and compatibles
        // (or leave default I/O pins unconfigured if noPinDefs is set in the user sketch)
        public const int X0 = 0; //A0;
        public const int X1 = 1; //A1;
        public const int X2 = 2; //A2;
        public const int X3 = 3; //A3;

        public const int Y0 = 4;
        public const int Y1 = 5;
        public const int Y2 = 6;
        public const int Y3 = 7;

        // Define Motor Shield pin names
        public const int DIRA = 12;
        public const int DIRB = 13;
        public const int PWMA = 3;
        public const int PWMB = 11;
        public const int BRAKEA = 9;
        public const int BRAKEB = 8;
        public const int CURRENTA = 0; //A0;
        public const int CURRENTB = 1; //A1;

        // Define additional I/O pins for Mega, Mega 2560 and Due
        public const int X4 = 6; //A6;
        public const int X5 = 7; //A7;
        public const int X6 = 8; //A8;
        public const int X7 = 9; //A9;
        public const int Y4 = 4;
        public const int Y5 = 7;
        public const int Y6 = 8;
        public const int Y7 = 12;

        // User Methods

        public static int scanValue = 0;
        private static int[] pins = new int[14]; //bool
        private static int millis100 = 0;
        private static DateTime startTime = DateTime.MinValue;

        public static void COM_setup(string source)
        {
            compiled = compiler.Compile(source);
            setupPLC();
            startTime = DateTime.Now;

            if (compiled)
            {
                compiler.methodSetup.Invoke(null, new object[] { });
            }
            else
            {
                plcCode.setup();
            }
        }

        public static void COM_loop()
        {
            setMillis((int)(DateTime.Now - startTime).TotalMilliseconds);

            if (compiled)
            {
                compiler.methodLoop.Invoke(null, new object[] { });
            }
            else
            {
                plcCode.loop();
            }
        }

        public static void COM_setPin(int pin, int value)
        {
            pins[pin] = value;
        }

        public static int COM_getPin(int pin)
        {
            return pins[pin];
        }

        public static void digitalWrite(int a, int b)
        {
            pins[a] = b;
            return;
        }

        public static int digitalRead(int a)
        {
            return pins[a];
        }

        public static void analogWrite(int a, int b)
        {
            return;
        }

        public static int analogRead(int a)
        {
            return 1;
        }

        public static void bitClear(ref int a, int b)
        {

            a &= ~(1 << b);
            return;
        }

        public static void bitSet(ref int a, int b)
        {
            a |= 1 << b;
            return;
        }

        public static int bitRead(int a, int b)
        {
            a &= 1 << b;
            a >>= b;
            return a;
        }

        public static void bitStackClear(ref int a, int b)
        {

            a &= ~(1 << b);
            return;
        }

        public static void bitStackSet(ref int a, int b)
        {
            a |= 1 << b;
            return;
        }

        public static int millis()
        {
            return millis100;
        }

        // Control Methods

        public static void setupPLC()
        {
            for (int i = 0; i < 14; i++)
            {
                pins[i] = 0;
            }
        }

        public static void setMillis(int value)
        {
            millis100 = value;
        }

        // PLC Methods

        // Read an input pin
        public static int _in(int input)
        {
            scanValue = digitalRead(input);
            return (scanValue);
        }

        // Read an inverted input
        public static int inNot(int input)
        {
            if (digitalRead(input) == 1)
            {
                scanValue = 0;
            }

            else
            {
                scanValue = 1;
            }
            return (scanValue);
        }

        // Read an analogue input
        public static int inAnalog(int input)
        {
            scanValue = analogRead(input);
            return (scanValue);
        }

        // Output to an output pin
        public static int _out(int output)
        {
            if (scanValue == 1)
            {
                digitalWrite(output, HIGH);
            }

            else
            {
                digitalWrite(output, LOW);
            }
            return (scanValue);
        }

        // Output to an output pin (inverted)
        public static int outNot(int output)
        {
            if (scanValue == 1)
            {
                digitalWrite(output, LOW);
            }

            else
            {
                digitalWrite(output, HIGH);
            }
            return (scanValue);
        }

        // Output to an auxiliary variable (inverted)
        public static int outNot(ref int output)
        {
            if (scanValue == 1)
            {
                output = 0;
            }

            else
            {
                output = 1;
            }
            return (scanValue);
        }

        // Output a PWM value to an output pin (scanValue in range 0-1023)
        public static int outPWM(int output)
        {
            analogWrite(output, scanValue / 4);
            return (scanValue);
        }

        // AND scanValue with input
        public static int andBit(int input)
        {
            scanValue = scanValue & digitalRead(input);
            return (scanValue);
        }

        // AND scanValue with inverted input
        public static int andNotBit(int input)
        {
            scanValue = scanValue & ~digitalRead(input);
            return (scanValue);
        }

        // OR scanValue with input
        public static int orBit(int input)
        {
            scanValue = scanValue | digitalRead(input);
            return (scanValue);
        }

        // OR scanValue with inverted input
        public static int orNotBit(int input)
        {
            if (scanValue == 1)
            {
            }

            else
            {
                if (digitalRead(input) == 0)
                {
                    scanValue = 1;
                }
                else
                {
                    scanValue = 0;
                }
            }
            return (scanValue);
        }

        // XOR scanValue with input
        public static int xorBit(int input)
        {
            scanValue = scanValue ^ digitalRead(input);
            return (scanValue);
        }

        // Set - Reset latch
        public static int latch(int output, int reset)
        {
            scanValue = scanValue | digitalRead(output);        // Self latch by ORing with Output pin (Q)
            scanValue = scanValue & ~digitalRead(reset);        // AND-Not with Reset Pin
            if (scanValue == 1)
            {
                digitalWrite(output, HIGH);
            }
            else
            {
                digitalWrite(output, LOW);
            }
            return (scanValue);
        }

        // Set - Reset latch
        public static int latch(ref int output, int reset)
        {
            scanValue = scanValue | output;                     // Self latch by ORing with Output pin (Q)
            scanValue = scanValue & ~digitalRead(reset);        // AND-Not with Reset Pin
            if (scanValue == 1)
            {
                output = 1;
            }
            else
            {
                output = 0;
            }
            return (scanValue);
        }

        // On delay timer - reqres continuously enabled input from previous scanValue
        public static int timerOn(ref int timerState, int timerPeriod)
        {
            if (scanValue == 0)
            {                                   // timer is disabled
                timerState = 0;                                     // Clear timerState (0 = 'not started')
            }

            else
            {                                                   // Timer is enabled
                if (timerState == 0)
                {                               // Timer hasn't started counting yet
                    timerState = millis();                          // Set timerState to current time in milliseconds
                    scanValue = 0;                                  // Result = 'not finished' (0)
                }
                else
                {                                               // Timer is active and counting
                    if (millis() - timerState >= timerPeriod)
                    {       // Timer has finished
                        scanValue = 1;                              // Result = 'finished' (1)
                    }
                    else
                    {                                           // Timer has not finished
                        scanValue = 0;                              // Result = 'not finished' (0)
                    }
                }
            }
            return (scanValue);                                     // Return result (1 = 'finished',
                                                                    // 0 = 'not started' / 'not finished')
        }

        // Fixed width pulse - enabled by momentary input from previous scanValue 
        public static int timerPulse(ref int timerState, int timerPeriod)
        {
            if (scanValue == 1 || timerState != 0)
            {                   // Timer is enabled
                if (timerState == 0)
                {                               // Timer hasn't started counting yet
                    timerState = millis();                          // Set timerState to current time in milliseconds
                    scanValue = 1;                                  // Pulse = 'Active' (1)
                }

                else
                {                                               // Timer is active and counting
                    if (millis() - timerState >= timerPeriod)
                    {       // Timer has finished
                        if (scanValue == 0)
                        {                       // Finished AND trigger is low
                            timerState = 0;                         // Re-enabled timer
                            scanValue = 0;                          // Pulse = 'finished' (0)
                        }
                        else
                        {                                      // Finished but trigger is still high
                            scanValue = 0;                          // Wait for trigger to go low before re-enabling
                        }
                    }
                    else
                    {                                           // Timer has not finished
                        scanValue = 1;                              // Pulse = 'Active' (1)
                    }
                }
            }
            return (scanValue);                                     // Return result (1 = 'active',
                                                                    // 0 = 'not started' / 'not yet re-enabled')
        }
        // Off delay timer - turns on immediately when enabled, then delays turning off when previous scanValue goes low
        public static int timerOff(ref int timerState, int timerPeriod)
        {
            if (scanValue == 0)
            {                                   // Timer input is off (scanValue = 0)
                if (timerState == 0)
                {                               // Timer is not started so do nothing
                }

                else
                {                                               // Timer is active and counting
                    if (millis() - timerState >= timerPeriod)
                    {       // Timer has finished
                        scanValue = 0;                              // Result = 'turn-off delay finished' (0)
                    }
                    else
                    {                                           // Timer has not finished
                        scanValue = 1;                              // Result = 'turn-off delay not finished' (1)
                    }
                }
            }

            else
            {                                                   // Timer input is high (scanValue = 1)
                timerState = millis();                              // Set timerState to current time in milliseconds
            }
            return (scanValue);                                     // Return result (1 = 'pulse On' / 'turn-off delay in progress',
                                                                    // 0 = 'not started' / 'finished')
        }

        // Cycle timer - creates a repeating pulse waveform when enabled by previous scanValue
        public static int timerCycle(ref int timer1State, int timer1Period, ref int timer2State, int timer2Period)
        {
            if (scanValue == 0)
            {                                   // Enable input is off (scanValue = 0)
                timer2State = 0;                                    // Ready to start LOW pulse period when enabled
                timer1State = 1;
            }

            else
            {                                                   // Enabled
                if (timer2State == 0)
                {                               // Low pulse Active
                    if (timer1State == 1)
                    {                           // LOW pulse period starting
                        timer1State = millis();                     // Set timerState to current time in milliseconds
                    }
                    else if (millis() - timer1State >= timer1Period)
                    {   // Low pulse period has finished
                        timer1State = 0;
                        timer2State = 1;                            // Ready to start HIGH pulse period
                    }
                    scanValue = 0;                                  // Result = 'Pulse LOW' (0)
                }
                if (timer1State == 0)
                {                               // High pulse Active
                    if (timer2State == 1)
                    {                           // HIGH pulse period starting
                        timer2State = millis();                     // Set timerState to current time in milliseconds
                    }
                    else if (millis() - timer2State >= timer2Period)
                    {   // High pulse has finished
                        timer2State = 0;
                        timer1State = 1;                            // Ready to start LOW pulse period
                    }
                    scanValue = 1;                                  // Result = 'Pulse HIGH' (1)
                }
            }
            return (scanValue);
        }


        // Test whether an analogue input is greater than a second analogue input
        public static int compareGT(int input)
        {
            if (scanValue > analogRead(input))
            {
                scanValue = 1;
            }

            else
            {
                scanValue = 0;
            }
            return (scanValue);
        }

        // Test whether an analogue input is less than a second analogue input
        public static int compareLT(int input)
        {
            if (scanValue < analogRead(input))
            {
                scanValue = 1;
            }

            else
            {
                scanValue = 0;
            }
            return (scanValue);
        }

        // Set a latched output
        public static int set(int output)
        {
            scanValue = scanValue | digitalRead(output);        // Self latch by ORing with Output pin
            if (scanValue == 1)
            {
                digitalWrite(output, HIGH);
            }
            return (scanValue);
        }

        // reset (or clear) a latched output
        public static int reset(int output)
        {
            if (scanValue == 1)
            {
                digitalWrite(output, LOW);
            }
            return (scanValue);
        }
    }

    public static class Counter
    {
        private static int _pv;
        private static int _ct = 0;
        private static int _ctUpEdge = 0;
        private static int _ctDownEdge = 0;
        private static int _uQ = 0;
        private static int _lQ = 1;

        // Up, down, or up-down counter
        public static void Init(int pv) // Counter constructor method
                        //void newCounter(Counter *pctr, int pv)
        {                                   // (Default values are for an up counter)
            _pv = pv;                       // Set preset value using supplied parameter
            _ct = 0;                        // Running count = zero
            _uQ = 0;                        // Up counter upper Q output = 0
            _lQ = 1;                        // Down counter lower Q output = 1
            _ctUpEdge = 0;                  // Prepare rising edge detect for up counter
            _ctDownEdge = 0;                // Prepare rising edge detect for down counter
        }

        // Up, down, or up-down counter

        public static int setPresetValue(int pv) // Counter method
                                   //int setPresetValue(Counter *pctr, int pv)	// Counter method
        {                                   // (Default values are for an up counter)
            _pv = pv;                       // Set preset value using supplied parameter
            return 0;
        }

        public static void Init(int pv, int direction)  // Counter constructor method
                                        //void newCounter2(Counter *pctr, int pv, int direction)	// Counter constructor method
        {                                   // (Second parameter sets default direction)
            _pv = pv;                       // Set preset value using supplied parameter
            if (direction == 0)
            {               // Set start values for an up counter
                _ct = 0;                    // Running count = zero
                _uQ = 0;                    // Up counter upper Q output = 0
                _lQ = 1;                    // Down counter lower Q output = 1
            }
            else
            {                           // Set start values for a down counter
                _ct = _pv;                  // Running count = preset value
                _uQ = 1;                    // Up counter upper Q output = 1
                _lQ = 0;                    // Down counter lower Q output = 0
            }
            _ctUpEdge = 0;                  // Prepare rising edge detect for up counter
            _ctDownEdge = 0;                // Prepare rising edge detect for down counter
        }

        public static int presetValue()   // Return preset value method
                            //int presetValue(Counter *pctr)	// Return preset value method
        {
            return (_pv);                   // Return preset value
        }

        public static void clear()                // Clear counter method
                                    //void clear(Counter *pctr)				// Clear counter method
        {
            if (plcLibrary.scanValue == 1)
            {           // Enabled if scanValue = 1
                _ct = 0;                    // Running count = 0
                _uQ = 0;                    // Up counter upper Q output = 0
                _lQ = 1;                    // Down counter lower Q output = 1
                _ctUpEdge = 0;              // Prepare rising edge detect for up counter
                _ctDownEdge = 0;            // Prepare rising edge detect for down counter
            }
        }

        public static void preset()               // Preset counter method
                                    //void preset(Counter *pctr)				// Preset counter method
        {
            if (plcLibrary.scanValue == 1)
            {           // Enabled if scanValue = 1
                _ct = _pv;                  // Running count = preset value
                _uQ = 1;                    // Up counter upper Q output = 1
                _lQ = 0;                    // Down counter lower Q output = 0
                _ctUpEdge = 0;              // Prepare rising edge detect for up counter
                _ctDownEdge = 0;            // Prepare rising edge detect for down counter
            }
        }

        public static int upperQ()        // Read up counter upper Q output method
                            //int upperQ(Counter *pctr)		// Read up counter upper Q output method
        {
            if (_uQ == 1)
            {                   // Set scanValue = 1 if upper Q = 1
                plcLibrary.scanValue = 1;
            }

            else
            {
                plcLibrary.scanValue = 0;              // Otherwise set scanValue = 0
            }
            return (_uQ);                   // Return upper Q value
        }

        public static int lowerQ()        // Read down counter lower Q output method
                            //int lowerQ(Counter *pctr)		// Read down counter lower Q output method
        {
            if (_lQ == 1)
            {                   // Set scanValue = 1 if lower Q = 1
                plcLibrary.scanValue = 1;
            }

            else
            {
                plcLibrary.scanValue = 0;              // Otherwise set scanValue = 0
            }
            return (_lQ);                   // Return lower Q value
        }

        public static int count()     // Return current count value method
                        //int count(Counter *pctr)		// Return current count value method
        {
            return (_ct);                   // Return count value
        }

        public static void countUp()              // Count up method
                                    //void countUp(Counter *pctr)				// Count up method
        {
            if (_ct != _pv)
            {               // Not yet finished counting so continue
                if (plcLibrary.scanValue == 0)
                {       // clock = 0 so clear counter edge detect
                    _ctUpEdge = 0;
                }
                else
                {                       // Clock = 1
                    if (_ctUpEdge == 0)
                    {   // Rising edge detected so increment count
                        _ctUpEdge = 1;      // Set counter edge
                        _ct++;              // Increment count
                        if (_ct == _pv)
                        {   // Counter has reached final value
                            _uQ = 1;        // Counter upper Q output = 1
                            _lQ = 0;        // Counter lower Q output = 0
                        }
                        if (_ct != _pv)
                        {       // Counter is not yet finished
                            _uQ = 0;        // Counter upper Q output = 0
                            _lQ = 0;        // Counter lower Q output = 0
                        }
                    }
                }
            }
        }

        public static void countDown()            // Count down method
                                    //void countDown(Counter *pctr)			// Count down method
        {
            if (_ct != 0)
            {                   // Not yet finished so continue
                if (plcLibrary.scanValue == 0)
                {       // clock = 0 so clear counter edge detect
                    _ctDownEdge = 0;
                }
                else
                {                       // Clock = 1
                    if (_ctDownEdge == 0)
                    {   // Rising edge detected so decrement count
                        _ctDownEdge = 1;    // Set counter edge
                        _ct--;              // Decrement count
                        if (_ct == 0)
                        {       // Counter has reached final value
                            _uQ = 0;        // Counter QUp output = 0
                            _lQ = 1;        // Counter QDown output = 1
                        }
                        if (_ct != 0)
                        {       // Counter is not yet finished
                            _uQ = 0;        // Counter upper Q output = 0
                            _lQ = 0;        // Counter lower Q output = 0
                        }
                    }
                }
            }
        }
    }

    public static class Shift
    {
        private static int _srLeftEdge = 0;
        private static int _srRightEdge = 0;
        private static int _sreg = 0;
        private static int _inbit;

        // Shift register
        public static void Init(int sreg)     // Shift register constructor method
                            //void newShift2(Shift *pshift, int sreg)	    // Shift register constructor method
        {                                   // (Register width = 32 bits)
            _sreg = sreg;                   // Set initial value
            _srLeftEdge = 0;                // Prepare rising edge detect for left shift
            _srRightEdge = 0;               // Prepare rising edge detect for right shift
        }

        public static int bitValue(int bitno) // Read a bit at a specified position
                                //int bitValue(Shift *pshift, int bitno)	// Read a bit at a specified position
        {
            if (plcLibrary.bitRead(_sreg, bitno) == 1)
            {
                plcLibrary.scanValue = 1;              // Tested bit = 1
            }

            else
            {
                plcLibrary.scanValue = 0;              // Tested bit = 0
            }
            return (plcLibrary.scanValue);             // Return tested bit value
        }

        public static int value()         // Return the current shift register value
                            //int value(Shift *pshift)			// Return the current shift register value
        {
            return (_sreg);
        }

        public static void reset()                    // Reset the shift register if scanValue = 0
                                        //void resetShift(Shift *pshift)					// Reset the shift register if scanValue = 0
        {
            if (plcLibrary.scanValue == 1)
            {
                _sreg = 0;                  // Set  the shift register to zero
                _srLeftEdge = 0;            // Prepare rising edge detect for left shift
                _srRightEdge = 0;           // Prepare rising edge detect for right shift
            }
        }

        public static void inputBit()             // Set the input bit of the shift register
                                    //void inputBit(Shift *pshift)				// Set the input bit of the shift register
        {
            if (plcLibrary.scanValue == 0)
            {           // If scanValue = 0, clear input bit
                _inbit = 0;
            }
            else
            {                           // Otherwise set the input bit
                _inbit = 1;
            }
        }

        public static void shiftRight()           // Shift right method
                                    //void shiftRight(Shift *pshift)			// Shift right method
        {
            if (plcLibrary.scanValue == 0)
            {           // clock = 0 so clear shift right edge detect
                _srRightEdge = 0;
            }
            else
            {                           // Clock = 1
                if (_srRightEdge == 0)
                {   // Rising edge detected so shift right
                    _srRightEdge = 1;       // Set shift right edge detect
                    _sreg = _sreg >> 1;     // Shift to the right
                    if (_inbit == 1)
                    {       // Shift-in new input bit at LHS
                            //bitSet(_sreg, 15);
                        plcLibrary.bitSet(ref _sreg, 15);
                    }
                }
            }
        }

        public static void shiftLeft()                // Shift left method
                                        //void shiftLeft(Shift *pshift)				// Shift left method
        {
            if (plcLibrary.scanValue == 0)
            {           // clock = 0 so clear shift left edge detect
                _srLeftEdge = 0;
            }
            else
            {                           // Clock = 1
                if (_srLeftEdge == 0)
                {       // Rising edge detected so shift left
                    _srLeftEdge = 1;        // Set shift left edge detect
                    _sreg = _sreg << 1;     // Shift to the left
                    if (_inbit == 1)
                    {       // Shift-in new input bit at RHS
                            //bitSet(_sreg, 0);
                        plcLibrary.bitSet(ref _sreg, 0);
                    }
                }
            }
        }
    }

    // Single-bit Software Stack
    public static class Stack
    {
        private static int _sreg = 0;

        public static void Init()                     // Stack constructor method
                                    //void newStack(Stack *pStack)						// Stack constructor method
        {                                   // (Register width = 32 bits)
            _sreg = 0;                      // Set the stack to zero
        }

        public static void push()                  // Push scanValue bit onto the stack method
                                     //void push(Stack *pStack)					// Push scanValue bit onto the stack method
        {
            _sreg = _sreg >> 1;             // Shift stack 1-bit to the right
            if (plcLibrary.scanValue == 1)
            {           // Shift-in scanValue bit at LHS
                        //bitSet(_sreg, 31);
                plcLibrary.bitStackSet(ref _sreg, 31);
                //bitSet(ref _sreg, 15);
            }
            else
            {
                //bitClear(_sreg, 31);
                plcLibrary.bitStackClear(ref _sreg, 31);
                //bitSet(ref _sreg, 15);
            }
        }

        public static void pop()                   // Pop scanValue bit from the stack method
                                     //void pop(Stack *pStack)					// Pop scanValue bit from the stack method
        {
            plcLibrary.scanValue = plcLibrary.bitRead(_sreg, 31); // Set scanValue to leftmost bit of stack
            _sreg = _sreg << 1;             // Shift stack 1-bit to the left
        }

        public static void orBlock()               // OR scanValue with value Popped from stack method
                                     //void orBlock(Stack *pStack)				// OR scanValue with value Popped from stack method
        {
            plcLibrary.scanValue = plcLibrary.scanValue | plcLibrary.bitRead(_sreg, 31); // OR scanValue with top of stack
            _sreg = _sreg << 1;             // Shift stack 1-bit to the left
        }

        public static void andBlock()              // AND scanValue with value Popped from stack method
                                     //void andBlock(Stack *pStack)				// AND scanValue with value Popped from stack method
        {
            plcLibrary.scanValue = plcLibrary.scanValue & plcLibrary.bitRead(_sreg, 31); // AND scanValue with top of stack
            _sreg = _sreg << 1;             // Shift stack 1-bit to the left
        }
    }

    // Single scan cycle Pulse with rising or falling edge detection
    public static class Pulse
    {
        private static int _pulseInput = 0;
        private static int _pulseUpEdge = 0;
        private static int _pulseDownEdge = 0;

        public static void Init()                     // Pulse constructor method
                                    //void newPulse(Pulse *pPulse)						// Pulse constructor method
        {
            _pulseInput = 0;                // Set pulse input tracker to zero
            _pulseUpEdge = 0;               // Prepare rising edge detect
            _pulseDownEdge = 0;             // Prepare falling edge detect
        }

        public static void inClock()              // Read the clock input method
                                    //void inClock(Pulse *pPulse)				// Read the clock input method
        {
            if (plcLibrary.scanValue != _pulseInput)
            {   // Rising or falling edge detected
                if (plcLibrary.scanValue == 1)
                {       // Rising edge detected
                    _pulseUpEdge = 1;       // Set rising edge detect value
                    _pulseDownEdge = 0;     // Clear falling edge detect value
                    _pulseInput = 1;        // Pulse input tracker = 1
                }
                else
                {                       // Falling edge detected
                    _pulseUpEdge = 0;       // Clear rising edge detect value
                    _pulseDownEdge = 1;     // Set falling edge detect value
                    _pulseInput = 0;        // Pulse input tracker = 0
                }
            }
            else
            {                           // No change detected
                _pulseUpEdge = 0;               // Set both edge detect values to zero
                _pulseDownEdge = 0;             // (and leave pulse tracker unchanged)
            }
        }

        public static void rising()               // Pulse rising edge detected method
                                    //void rising(Pulse *pPulse)		        // Pulse rising edge detected method
        {
            plcLibrary.scanValue = _pulseUpEdge;       // scanValue = 1 if rising edge detected, 0 otherwise
        }

        public static void falling()              // Pulse falling edge detected method
                                    //void falling(Pulse *pPulse)		        // Pulse falling edge detected method
        {
            plcLibrary.scanValue = _pulseDownEdge;     // scanValue = 1 if falling edge detected, 0 otherwise
        }
    }
}




