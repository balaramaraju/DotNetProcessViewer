using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using DebugEngine.Interfaces;
using DebugEngine.Utilities;
using DebugEngine.Debugee.Wrappers;

namespace DebugEngine
{
    
    public class DebugEng{
        private static readonly Guid clsidCLRMetaHost = new Guid("9280188D-0E8E-4867-B30C-7FA83884E8DE");
        private DebugeeProcess debugee = null;
        private ICLRMetaHost iClrMetaHost = null;
        internal DebugeeProcess Process
        {
            get {
                return debugee;
            }
        }
        public DebugEng() {
            object clrMetaHost;
            Guid ifaceId = typeof(ICLRMetaHost).GUID;
            Guid clsid = clsidCLRMetaHost;
            NativeAPI.CLRCreateInstance(ref clsid, ref ifaceId, out clrMetaHost);
            iClrMetaHost = (ICLRMetaHost)clrMetaHost;
        }

        public static bool IsManagedProcess(IntPtr processHandle)
        {
            object clrMetaHost;
            Guid ifaceId = typeof(ICLRMetaHost).GUID;
            Guid clsid = clsidCLRMetaHost;
            NativeAPI.CLRCreateInstance(ref clsid, ref ifaceId, out clrMetaHost);
            ICLRMetaHost _iClrMetaHost = (ICLRMetaHost)clrMetaHost;
            IEnumUnknown enumRuntimes = _iClrMetaHost.EnumerateLoadedRuntimes(processHandle);
            List<ICLRRuntimeInfo> versions = new List<ICLRRuntimeInfo>();
            for (object iUnknw; enumRuntimes.Next(1, out iUnknw, IntPtr.Zero) == 0; /* empty */) {
                return true;
            }

            return false;
        }
        internal DebugeeProcess Attach(int processID)
        {
            debugee = new DebugeeProcess(processID, iClrMetaHost, this);
            return debugee;   
        }
        public void Continue(){
            ControllerStateMgr.SetState(ControllerState.Running);
        }
        public void Stop(){
            ControllerStateMgr.SetState(ControllerState.Paused);
        }
        public void Detach(){
            debugee.Detach();
            ControllerStateMgr.ReleaseStateObj();
        }
        ICorDebugBreakpoint liveBP = null;
        internal delegate void BreakPoint(CorThread thread);
        internal delegate void ProcessExit(CorProcess process);
        internal delegate void AssemblyLoad_UnloadNotification(CorDebugAppDomain app,CorAssembly assm ,bool act);
        internal delegate void TreadCreate_TerminateNotification(CorDebugAppDomain app, CorThread thread, bool act);
        internal delegate void Exception_Catcher(CorDebugAppDomain app, CorThread thread, CorFunctionFrame frame,CorDebugExceptionCallbackType exState,uint offset);
        internal event AssemblyLoad_UnloadNotification AssemblyNotification;
        internal event TreadCreate_TerminateNotification ThreadNotification;
        internal event Exception_Catcher ExceptionCatcherNotification;
        internal event ProcessExit Process_Exit;
        internal event BreakPoint FuntionBreakPoint;
        public void AssemblyLoad_Unload(ICorDebugAppDomain appDomain, ICorDebugAssembly assembly,bool action) {
            AssemblyNotification(new CorDebugAppDomain(appDomain),new CorAssembly(assembly), action);
        }
        public void ThreadCreate_Terminate(ICorDebugAppDomain appDomain, ICorDebugThread thread, bool action){
            ThreadNotification(new CorDebugAppDomain(appDomain),new CorThread(thread), action);
        }
        public void ExceptionCatcher(ICorDebugAppDomain appDomain, ICorDebugThread pThread, ICorDebugFrame pFrame, uint nOffset, CorDebugExceptionCallbackType dwEventType, uint dwFlags) {
            ExceptionCatcherNotification(new CorDebugAppDomain(appDomain), new CorThread(pThread), new CorFunctionFrame(pFrame), dwEventType,nOffset); 
        }
        public void ProcessTerminated(ICorDebugProcess process){
            Process_Exit(new CorProcess(process));
        }
        public void BreakPointCatcher(ICorDebugAppDomain pAppDomain, ICorDebugThread pThread, ICorDebugBreakpoint pBreakpoint) {
            FuntionBreakPoint(new CorThread(pThread));
            liveBP = pBreakpoint;
        }
    }
}
