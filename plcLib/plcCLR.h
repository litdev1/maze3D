#pragma once

#include <chrono>

using namespace std::chrono;
using namespace System;

namespace plcLib 
{
    public ref class plcAPI
    {
    public:
        plcAPI();
        ~plcAPI();

        void COM_setupPLC();
        void COM_SetPin(int pin, int value);
        int COM_GetPin(int pin);

        void COM_setup();
        void COM_loop();

        unsigned int COM_in(int input);
        unsigned int COM_out(int output);
    };
}
