using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace Vortex.AutoClick
{
    [Flags]
    public enum eMouseEventFlags : uint
    {
        kLeftDown = 0x00000002,
        kLeftUp = 0x00000004,
        kMiddleDown = 0x00000020,
        kMiddleUp = 0x00000040,
        kMove = 0x00000001,
        kAbsolute = 0x00008000,
        kRightDown = 0x00000008,
        kRightUp = 0x00000010
    }
    public class CAutoClick
    {
        #region Public Variables
        public Key KeyOn { get; set; }
        public Key KeyOff { get; set; }
        public int Speed { get; set; }
        public int MouseButton { get; set; }
        #endregion

        #region Constructor
        public CAutoClick(Key keyOn, Key keyOff, int iSpeed, int iMouseButton = 0x2)
        {
            KeyOn = keyOn;
            KeyOff = keyOff;
            Speed = iSpeed;
            MouseButton = iMouseButton;
        }
        #endregion

        #region Private Methods
        [DllImport("user32.dll", SetLastError = true)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
        #endregion

        #region Public Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Click()
        {
            if(MouseButton == 0x2)
            {
                mouse_event((uint)eMouseEventFlags.kLeftDown, 0, 0, 0, UIntPtr.Zero);
                mouse_event((uint)eMouseEventFlags.kLeftUp, 0, 0, 0, UIntPtr.Zero);
                return;
            }

            mouse_event((uint)eMouseEventFlags.kRightDown, 0, 0, 0, UIntPtr.Zero);
            mouse_event((uint)eMouseEventFlags.kRightUp, 0, 0, 0, UIntPtr.Zero);
        }
        #endregion
    }
}
