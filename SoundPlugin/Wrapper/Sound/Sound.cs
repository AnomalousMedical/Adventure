using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace SoundPlugin
{
    public class Sound : SoundPluginObject
    {
        internal Sound(IntPtr sound)
            :base(sound)
        {
            
        }

        public bool Repeat
        {
            get
            {
                return Sound_getRepeat(Pointer);
            }
            set
            {
                Sound_setRepeat(Pointer, value);
            }
        }

        public double Duration
        {
            get
            {
                return Sound_getDuration(Pointer);
            }
        }

        #region PInvoke

        [DllImport(SoundPluginInterface.LibraryName, CallingConvention=CallingConvention.Cdecl)]
        private static extern void Sound_setRepeat(IntPtr sound, bool value);

        [DllImport(SoundPluginInterface.LibraryName, CallingConvention=CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool Sound_getRepeat(IntPtr sound);

        [DllImport(SoundPluginInterface.LibraryName, CallingConvention=CallingConvention.Cdecl)]
        private static extern double Sound_getDuration(IntPtr sound);

        #endregion
    }
}
