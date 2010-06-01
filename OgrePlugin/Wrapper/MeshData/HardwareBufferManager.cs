﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine.Attributes;
using System.Runtime.InteropServices;

namespace OgreWrapper
{
    [NativeSubsystemType]
    public class HardwareBufferManager : IDisposable
    {
        static HardwareBufferManager instance = new HardwareBufferManager();

        public static HardwareBufferManager getInstance()
        {
            return instance;
        }

        private SharedPtrCollection<HardwareIndexBuffer> indexBuffers = new SharedPtrCollection<HardwareIndexBuffer>(HardwareIndexBuffer.createWrapper, HardwareIndexBufferPtr_createHeapPtr, HardwareIndexBufferPtr_Delete);
        private SharedPtrCollection<HardwareVertexBuffer> vertexBuffers = new SharedPtrCollection<HardwareVertexBuffer>(HardwareVertexBuffer.createWrapper, HardwareVertexBufferPtr_createHeapPtr, HardwareVertexBufferPtr_Delete);

        public void Dispose()
        {
            indexBuffers.Dispose();
            vertexBuffers.Dispose();
        }

        internal HardwareIndexBufferSharedPtr getIndexBufferObject(IntPtr hardwareIndexBuffer)
        {
            return new HardwareIndexBufferSharedPtr(indexBuffers.getObject(hardwareIndexBuffer));
        }

        internal ProcessWrapperObjectDelegate ProcessIndexBufferCallback
        {
            get
            {
                return indexBuffers.ProcessWrapperCallback;
            }
        }

        internal HardwareVertexBufferSharedPtr getVertexBufferObject(IntPtr hardwareVertexBuffer)
        {
            return new HardwareVertexBufferSharedPtr(vertexBuffers.getObject(hardwareVertexBuffer));
        }

        internal ProcessWrapperObjectDelegate ProcessVertexBufferCallback
        {
            get
            {
                return vertexBuffers.ProcessWrapperCallback;
            }
        }

#region PInvoke

        //HardwareIndexBufferPtr
        [DllImport("OgreCWrapper")]
        private static extern IntPtr HardwareIndexBufferPtr_createHeapPtr(IntPtr stackSharedPtr);

        [DllImport("OgreCWrapper")]
        private static extern void HardwareIndexBufferPtr_Delete(IntPtr heapSharedPtr);

        //HardwareVertexBufferPtr
        [DllImport("OgreCWrapper")]
        private static extern IntPtr HardwareVertexBufferPtr_createHeapPtr(IntPtr stackSharedPtr);

        [DllImport("OgreCWrapper")]
        private static extern void HardwareVertexBufferPtr_Delete(IntPtr heapSharedPtr);

#endregion
    }
}
