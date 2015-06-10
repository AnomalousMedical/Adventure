﻿using Engine;
using Engine.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OgrePlugin
{
    [NativeSubsystemType]
    public class GpuProgramParameters : IDisposable
    {
        internal static GpuProgramParameters createWrapper(IntPtr ptr)
        {
            return new GpuProgramParameters(ptr);
        }

        private IntPtr ptr;

        internal GpuProgramParameters(IntPtr ptr)
        {
            this.ptr = ptr;
        }

        public void Dispose()
        {
            ptr = IntPtr.Zero;
        }

        public void setNamedConstant(String name, float val)
        {
            GpuProgramParameters_setNamedConstant1(ptr, name, val);
        }

        public void setNamedConstant(String name, int val)
        {
            GpuProgramParameters_setNamedConstant2(ptr, name, val);
        }

        public void setNamedConstant(String name, Quaternion vec)
        {
            GpuProgramParameters_setNamedConstant3(ptr, name, vec);
        }

        public void setNamedConstant(String name, Vector3 vec)
        {
            GpuProgramParameters_setNamedConstant4(ptr, name, vec);
        }

        public void setNamedConstant(String name, Vector2 vec)
        {
            GpuProgramParameters_setNamedConstant5(ptr, name, vec);
        }

        public void setNamedConstant(String name, Color colour)
        {
            GpuProgramParameters_setNamedConstant6(ptr, name, colour);
        }

        public unsafe void setNamedConstant(String name, int* val, UIntPtr count)
        {
            GpuProgramParameters_setNamedConstant7(ptr, name, val, count);
        }

        public unsafe void setNamedConstant(String name, float* val, UIntPtr count)
        {
            GpuProgramParameters_setNamedConstant8(ptr, name, val, count);
        }

        public unsafe void setNamedConstant(String name, double* val, UIntPtr count)
        {
            GpuProgramParameters_setNamedConstant9(ptr, name, val, count);
        }


        public void setConstant(UIntPtr index, float val)
        {
            GpuProgramParameters_setConstant1(ptr, index, val);
        }

        public void setConstant(UIntPtr index, int val)
        {
            GpuProgramParameters_setConstant2(ptr, index, val);
        }

        public void setConstant(UIntPtr index, Quaternion vec)
        {
            GpuProgramParameters_setConstant3(ptr, index, vec);
        }

        public void setConstant(UIntPtr index, Vector3 vec)
        {
            GpuProgramParameters_setConstant4(ptr, index, vec);
        }

        public void setConstant(UIntPtr index, Vector2 vec)
        {
            GpuProgramParameters_setConstant5(ptr, index, vec);
        }

        public void setConstant(UIntPtr index, Color colour)
        {
            GpuProgramParameters_setConstant6(ptr, index, colour);
        }

        public unsafe void setConstant(UIntPtr index, int* val, UIntPtr count)
        {
            GpuProgramParameters_setConstant7(ptr, index, val, count);
        }

        public unsafe void setConstant(UIntPtr index, float* val, UIntPtr count)
        {
            GpuProgramParameters_setConstant8(ptr, index, val, count);
        }

        public unsafe void setConstant(UIntPtr index, double* val, UIntPtr count)
        {
            GpuProgramParameters_setConstant9(ptr, index, val, count);
        }

        #region PInvoke
        [DllImport(LibraryInfo.Name, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GpuProgramParameters_setNamedConstant1(IntPtr param, String name, float val);

        [DllImport(LibraryInfo.Name, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GpuProgramParameters_setNamedConstant2(IntPtr param, String name, int val);

        [DllImport(LibraryInfo.Name, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GpuProgramParameters_setNamedConstant3(IntPtr param, String name, Quaternion vec);

        [DllImport(LibraryInfo.Name, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GpuProgramParameters_setNamedConstant4(IntPtr param, String name, Vector3 vec);

        [DllImport(LibraryInfo.Name, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GpuProgramParameters_setNamedConstant5(IntPtr param, String name, Vector2 vec);

        [DllImport(LibraryInfo.Name, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GpuProgramParameters_setNamedConstant6(IntPtr param, String name, Color colour);

        [DllImport(LibraryInfo.Name, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe void GpuProgramParameters_setNamedConstant7(IntPtr param, String name, int* val, UIntPtr count);

        [DllImport(LibraryInfo.Name, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe void GpuProgramParameters_setNamedConstant8(IntPtr param, String name, float* val, UIntPtr count);

        [DllImport(LibraryInfo.Name, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe void GpuProgramParameters_setNamedConstant9(IntPtr param, String name, double* val, UIntPtr count);

        [DllImport(LibraryInfo.Name, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GpuProgramParameters_setConstant1(IntPtr param, UIntPtr index, float val);

        [DllImport(LibraryInfo.Name, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GpuProgramParameters_setConstant2(IntPtr param, UIntPtr index, int val);

        [DllImport(LibraryInfo.Name, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GpuProgramParameters_setConstant3(IntPtr param, UIntPtr index, Quaternion vec);

        [DllImport(LibraryInfo.Name, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GpuProgramParameters_setConstant4(IntPtr param, UIntPtr index, Vector3 vec);

        [DllImport(LibraryInfo.Name, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GpuProgramParameters_setConstant5(IntPtr param, UIntPtr index, Vector2 vec);

        [DllImport(LibraryInfo.Name, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GpuProgramParameters_setConstant6(IntPtr param, UIntPtr index, Color colour);

        [DllImport(LibraryInfo.Name, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe void GpuProgramParameters_setConstant7(IntPtr param, UIntPtr index, int* val, UIntPtr count);

        [DllImport(LibraryInfo.Name, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe void GpuProgramParameters_setConstant8(IntPtr param, UIntPtr index, float* val, UIntPtr count);

        [DllImport(LibraryInfo.Name, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe void GpuProgramParameters_setConstant9(IntPtr param, UIntPtr index, double* val, UIntPtr count);
        #endregion
    }
}
