using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DebugEngine.Interfaces;
using DebugEngine.Utilities;

namespace DebugEngine
{
    static class CLRHelperMethods {
        private static readonly Guid clsidCLRMetaHost = new Guid("9280188D-0E8E-4867-B30C-7FA83884E8DE");
        private static Guid clsIdClrDebuggingLegacy = new Guid("DF8395B5-A4BA-450b-A77C-A9A47762C520");
        private static ICLRMetaHost iClrMetaHost = null;
        internal static ICLRRuntimeInfo FindRuntimeVersion(IntPtr processHandle)
        {
            if (iClrMetaHost == null){
                object clrMetaHost;
                Guid ifaceId = typeof(ICLRMetaHost).GUID;
                Guid clsid = clsidCLRMetaHost;
                NativeAPI.CLRCreateInstance(ref clsid, ref ifaceId, out clrMetaHost);
                iClrMetaHost = (ICLRMetaHost)clrMetaHost;
            }
            IEnumUnknown enumRuntimes = iClrMetaHost.EnumerateLoadedRuntimes(processHandle);
            List<ICLRRuntimeInfo> versions = new List<ICLRRuntimeInfo>();
            for (object iUnknw; enumRuntimes.Next(1, out iUnknw, IntPtr.Zero) == 0; /* empty */)
            {
                StringBuilder verion = new StringBuilder();
                int length = 26;//possible length is 24 for version +1 
                ((ICLRRuntimeInfo)iUnknw).GetVersionString(verion, ref length);
                versions.Add((ICLRRuntimeInfo)iUnknw);
            }
            if (versions.Count > 1)
            {
                throw new Exception("Multiple .Net Versions Loaded in this Procces");
            }
            else if (versions.Count == 0)
            {
                throw new Exception(" Unmanaged process.");
            }
            return versions[0];
        }
        internal static ICorDebug GetDebugger(IntPtr processHandle)
        {
            ICLRRuntimeInfo runTime = FindRuntimeVersion(processHandle);
            Guid iidICorDebug = typeof(ICorDebug).GUID;
            Guid clsId = clsIdClrDebuggingLegacy;
            ICorDebug debugger = (ICorDebug)runTime.GetInterface(ref clsId, ref iidICorDebug);
            return debugger;
        }
        internal static bool IsManagedProcess(IntPtr processHandle)
        {
            object clrMetaHost;
            Guid ifaceId = typeof(ICLRMetaHost).GUID;
            Guid clsid = clsidCLRMetaHost;
            NativeAPI.CLRCreateInstance(ref clsid, ref ifaceId, out clrMetaHost);
            ICLRMetaHost _iClrMetaHost = (ICLRMetaHost)clrMetaHost;
            IEnumUnknown enumRuntimes = _iClrMetaHost.EnumerateLoadedRuntimes(processHandle);
            List<ICLRRuntimeInfo> versions = new List<ICLRRuntimeInfo>();
            for (object iUnknw; enumRuntimes.Next(1, out iUnknw, IntPtr.Zero) == 0; /* empty */)
            {
                return true;
            }

            return false;
        }
        public static ICLRMetaHost CLRMetaHost {
            get {
                if (iClrMetaHost == null)
                {
                    object clrMetaHost;
                    Guid ifaceId = typeof(ICLRMetaHost).GUID;
                    Guid clsid = clsidCLRMetaHost;
                    NativeAPI.CLRCreateInstance(ref clsid, ref ifaceId, out clrMetaHost);
                    iClrMetaHost = (ICLRMetaHost)clrMetaHost;
                }
                return iClrMetaHost;
            }
        }
        public static void Attach(int processID) {
            
        }
    }
}
