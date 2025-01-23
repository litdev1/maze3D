#pragma once

#include "plcCLR.h"
#include "plcLib.h"
#include "plcCode.h"

namespace plcLib
{
	steady_clock::time_point startTime;

	plcAPI::plcAPI()
	{
		setupPLC();
	}

	plcAPI::~plcAPI()
	{
	}

	void plcAPI::COM_setupPLC()
	{
		setupPLC();
	}

	void plcAPI::COM_SetPin(int pin, int value)
	{
		setPin(pin, value);
	}

	int plcAPI::COM_GetPin(int pin)
	{
		return getPin(pin);
	}

	void plcAPI::COM_setup()
	{
		startTime = high_resolution_clock::now();

		setup();
	}

	void plcAPI::COM_loop()
	{
		nanoseconds ns = duration_cast<std::chrono::nanoseconds>(high_resolution_clock::now() - startTime);
		setMillis((unsigned long)(ns.count() / 1000000));

		loop();
	}

	unsigned int plcAPI::COM_in(int input)
	{
		return in(input);
	}

    unsigned int plcAPI::COM_out(int output)
    {
        return out(output);
    }
}

