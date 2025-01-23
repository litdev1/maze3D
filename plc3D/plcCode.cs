namespace plc3D
{
    public static class plcCode
    {
        private static int TIMER0 = 0;
        private static int TIMER1 = 0;

        public static void setup()
        {

        }

        public static void loop()
        {
            //Forwards W key or toggle forward in bursts S key
            plcLibrary._in(plcLibrary.X3);
            plcLibrary.timerCycle(ref TIMER0, 200, ref TIMER1, 500);   // cycle activated by X3 (S)
            plcLibrary.orBit(plcLibrary.X0);
            plcLibrary._out(plcLibrary.Y0);

            //Left A key
            plcLibrary._in(plcLibrary.X1);
            plcLibrary._out(plcLibrary.Y1);

            //Right D key
            plcLibrary._in(plcLibrary.X2);
            plcLibrary._out(plcLibrary.Y2);
        }
    }
}
