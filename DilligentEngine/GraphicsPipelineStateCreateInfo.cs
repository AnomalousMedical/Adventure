﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace DilligentEngine
{
    public class GraphicsPipelineStateCreateInfo : IDisposable
    {
        private IntPtr ptr;

        public IntPtr ObjPtr => ptr;

        public GraphicsPipelineStateCreateInfo()
        {
            this.ptr = GraphicsPipelineStateCreateInfo_Create();
        }

        public void Dispose()
        {
            GraphicsPipelineStateCreateInfo_Delete(this.ptr);
        }

        public void LazySetup(ISwapChain m_pSwapChain, IRenderDevice m_pDevice, IShader pVS, IShader pPS)
        {
            GraphicsPipelineStateCreateInfo_LazySetup(this.ptr, m_pSwapChain.ObjPtr, m_pDevice.ObjPtr, pVS.ObjPtr, pPS.ObjPtr);
        }

        public IPipelineState OneShot(ISwapChain m_pSwapChain, IRenderDevice m_pDevice)
        {
            return new IPipelineState(GraphicsPipelineStateCreateInfo_OneShot(this.ptr, m_pSwapChain.objPtr, m_pDevice.objPtr));
        }

        [DllImport(LibraryInfo.LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GraphicsPipelineStateCreateInfo_Create();

        [DllImport(LibraryInfo.LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GraphicsPipelineStateCreateInfo_Delete(IntPtr obj);

        [DllImport(LibraryInfo.LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GraphicsPipelineStateCreateInfo_LazySetup(IntPtr PSOCreateInfo, IntPtr m_pSwapChain, IntPtr m_pDevice, IntPtr pVS, IntPtr pPS);

        [DllImport(LibraryInfo.LibraryName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GraphicsPipelineStateCreateInfo_OneShot(IntPtr PSOCreateInfo, IntPtr m_pSwapChain, IntPtr m_pDevice);
    }
}
