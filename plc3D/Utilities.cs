using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;

namespace plc3D
{
    /// <summary>
    /// Utility class for various functions (static)
    /// </summary>
    public static class Utilities
    {
        private static KeyConverter kc = new KeyConverter();

        /// <summary>
        /// Is key currently down
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool KeyDown(string key)
        {
            Key _key = (Key)kc.ConvertFromString(key);
            int _keycode = KeyInterop.VirtualKeyFromKey(_key);
            short result = User32.GetKeyState(_keycode);
            return (result & 0x8000) == 0x8000;
        }

        /// <summary>
        /// Copy an image
        /// </summary>
        /// <param name="bmSource"></param>
        /// <returns></returns>
        public static BitmapSource CopyImage(BitmapSource bmSource)
        {
            return bmSource.Clone();
        }

        /// <summary>
        /// Reflect an image
        /// </summary>
        /// <param name="bmSource"></param>
        /// <param name="mode">0: X flip, 1: Y flip</param>
        /// <returns></returns>
        public static BitmapSource ReflectImage(BitmapSource bmSource, int mode)
        {
            Bitmap bm = GetBitmap(bmSource);
            Bitmap _bm = new Bitmap(bm);
            _bm.RotateFlip(mode == 1 ? RotateFlipType.RotateNoneFlipY : RotateFlipType.RotateNoneFlipX);
            return GetBitmapImage(_bm);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bmSource"></param>
        /// <param name="mode">0: 90deg, 1: 180 deg, 2: 270deg</param>
        /// <returns></returns>
        public static BitmapSource RotateImage(BitmapSource bmSource, int mode)
        {
            Bitmap bm = GetBitmap(bmSource);
            Bitmap _bm = new Bitmap(bm);
            _bm.RotateFlip(mode == 1 ? RotateFlipType.Rotate180FlipNone : mode == 2 ? RotateFlipType.Rotate270FlipNone : RotateFlipType.Rotate90FlipNone);

            _bm.RotateFlip(RotateFlipType.RotateNoneFlipX);
            return GetBitmapImage(_bm);
        }

        /// <summary>
        /// Get a Bitmap from a BitmapSource
        /// </summary>
        /// <param name="bmSource"></param>
        /// <returns></returns>
        public static Bitmap GetBitmap(BitmapSource bmSource)
        {
            MemoryStream ms = new MemoryStream();
            BitmapEncoder enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bmSource));
            enc.Save(ms);
            Bitmap bm = new Bitmap(ms);
            return bm;
        }

        /// <summary>
        /// Get a BitmapImage from a Bitmap
        /// </summary>
        /// <param name="bm"></param>
        /// <returns></returns>
        public static BitmapImage GetBitmapImage(Bitmap bm)
        {
            MemoryStream ms = new MemoryStream();
            bm.Save(ms, ImageFormat.Png);
            BitmapImage bmImage = new BitmapImage();
            bmImage.BeginInit();
            bmImage.StreamSource = ms;
            bmImage.EndInit();
            return bmImage;
        }
    }
}
