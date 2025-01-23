using System.Runtime.InteropServices;

namespace plc3D
{
    /// <summary>
    /// PInvoke methods
    /// </summary>
    public class User32
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern short GetKeyState(int nVirtKey);
    }
}
