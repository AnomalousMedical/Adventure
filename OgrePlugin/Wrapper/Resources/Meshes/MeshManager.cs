﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine.Attributes;
using System.Runtime.InteropServices;

namespace OgreWrapper
{
    [NativeSubsystemType]
    public class MeshManager : IDisposable
    {
        static MeshManager instance = new MeshManager();

        public static MeshManager getInstance()
        {
            return instance;
        }

        private SharedPtrCollection<Mesh> meshPtrCollection = new SharedPtrCollection<Mesh>(Mesh.createWrapper, MeshPtr_createHeapPtr, MeshPtr_Delete);

        public void Dispose()
        {
            meshPtrCollection.Dispose();
        }

        internal MeshPtr getObject(IntPtr nativeMaterial)
        {
            return new MeshPtr(meshPtrCollection.getObject(nativeMaterial));
        }

        internal ProcessWrapperObjectDelegate ProcessWrapperObjectCallback
        {
            get
            {
                return meshPtrCollection.ProcessWrapperCallback;
            }
        }

#region PInvoke

        //MeshPtr
        [DllImport("OgreCWrapper")]
        private static extern IntPtr MeshPtr_createHeapPtr(IntPtr stackSharedPtr);

        [DllImport("OgreCWrapper")]
        private static extern void MeshPtr_Delete(IntPtr heapSharedPtr);

#endregion
    }
}
