using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using DebugEngine.Interfaces;
using DebugEngine.Debugee;
using DebugEngine.Debugee.Wrappers;
using DebugEngine.MetaDataUtils;

namespace DebugEngine
{
    class DebugeeProcess
    {
        private string processName;
        private bool isDebugMode = false;
        public bool IsDebugMode {
            get { return isDebugMode; }
        }
        private static Guid clsIdClrDebuggingLegacy = new Guid("DF8395B5-A4BA-450b-A77C-A9A47762C520");
        private uint processID;
        
        private ICLRMetaHost iClrMetaHost;
        TargetProcess process; 
        static ManagedCallback callback = null;
        public string Name {
            get {return processName;}
        }
        public uint ID {
            get { return processID; }
        }
        public List<CorThread> Threads{
            get {
                throw new NotImplementedException();            
            }
        }
        public DebugeeProcess(int processID, ICLRMetaHost iClrMetaHost, DebugEng eventDashboard)
        {
            Process debugee = Process.GetProcessById(processID);
            this.processID = (uint) processID;
            this.processName = debugee.ProcessName;
            this.iClrMetaHost = iClrMetaHost;
            ICLRRuntimeInfo iRuntime = FindRuntimeVersion(debugee.Handle);
            ICorDebug icorDebug = GetDebugger(iRuntime);
            icorDebug.Initialize();
            callback = new ManagedCallback(eventDashboard);
            icorDebug.SetManagedHandler(callback);
            ICorDebugProcess iCorProcess = null;
            icorDebug.DebugActiveProcess((uint)processID, 0, out iCorProcess);
            isDebugMode = true;
            process = new TargetProcess(iCorProcess);
            
        }
        public void Detach() {
            process.Detach();
        }
        private ICLRRuntimeInfo FindRuntimeVersion(IntPtr processHandle)
        {
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
        private ICorDebug GetDebugger(ICLRRuntimeInfo runTime)
        {
            Guid iidICorDebug = typeof(ICorDebug).GUID;
            Guid clsId = clsIdClrDebuggingLegacy;
            ICorDebug debugger = (ICorDebug)runTime.GetInterface(ref clsId, ref iidICorDebug);
            return debugger;
        }
        
    }
}
