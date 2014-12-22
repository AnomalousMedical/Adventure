﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace MyGUIPlugin
{
    public class ResourceManager
    {
        private static ResourceManager instance = null;

        public static ResourceManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ResourceManager();
                }
                return instance;
            }
        }

        private IntPtr resourceManager;

        private ResourceManager()
        {
            resourceManager = ResourceManager_getInstance();
        }

        public bool load(String file)
        {
            return ResourceManager_load(resourceManager, file);
        }

        public bool removeByName(String name)
        {
            return ResourceManager_removeByName(resourceManager, name);
        }

        public void clear()
        {
            ResourceManager_clear(resourceManager);
        }

        public uint getCount()
        {
            return ResourceManager_getCount(resourceManager).horriblyUnsafeToUInt32();
        }

        #region PInvoke

        [DllImport(MyGUIInterface.LibraryName, CallingConvention=CallingConvention.Cdecl)]
        private static extern IntPtr ResourceManager_getInstance();

        [DllImport(MyGUIInterface.LibraryName, CallingConvention=CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool ResourceManager_load(IntPtr resourceManager, String file);

        [DllImport(MyGUIInterface.LibraryName, CallingConvention=CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool ResourceManager_removeByName(IntPtr resourceManager, String name);

        [DllImport(MyGUIInterface.LibraryName, CallingConvention=CallingConvention.Cdecl)]
        private static extern void ResourceManager_clear(IntPtr resourceManager);

        [DllImport(MyGUIInterface.LibraryName, CallingConvention=CallingConvention.Cdecl)]
        private static extern UIntPtr ResourceManager_getCount(IntPtr resourceManager);

        #endregion
    }
}
