using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using DebugEngine.Debugee;
using DebugEngine.Debugee.Wrappers;
using DebugEngine.MetaDataUtils;
using DebugEngine.Utilities;
using DebugEngine.Interfaces;

namespace DebugEngine
{
    public class DebugFacade
    {
        private Dictionary<string, CorBreakPoint> bpDictionary = new Dictionary<string, CorBreakPoint>();
        private DebugEng debug;
        private bool isAttached = false;
        private bool isFreezed = false;
        public bool IsAttached {
            get {
                return isAttached;
            }
        }
        public delegate void BreakPointNotifcation(BREAKPOINT breakp);
        //public delegate void ExceptionCatcherNotification(EXCEPTION exceptionInfo);
        public delegate void AssemblyLoad_UnloadNotification(ASSEMBLY assemblyInfo);
        public delegate void TreadCreate_TerminateNotification(THREAD threadInfo);
        public delegate void ProcessTerminated(PROCESS process);
        private event AssemblyLoad_UnloadNotification AssmNotification;
        private event TreadCreate_TerminateNotification ThreadNotification;
        //private event ExceptionCatcherNotification ExceptionNotification;
        private event ProcessTerminated ProcessTerminatedNotification;
        private event BreakPointNotifcation BreakPointFunctionNotifcation;
        private Dictionary<uint, CorThread> liveThreads = new Dictionary<uint, CorThread>();
        private Dictionary<uint, List<String>> deadThreads = new Dictionary<uint, List<String>>();
        public DebugFacade() {
            debug = new DebugEng();
        }
        public void OpenDebugSession(uint processID){
            if (debug.Attach((int)processID) == null) { 
                throw new Exception("Unable to Attach " + processID);
            }
            isAttached = true;
        }
        public void CloseDebugSession(){
            debug.Stop();
            debug.Detach();
            liveThreads.Clear();
            deadThreads.Clear();
            isAttached = false;
        }
        public void FreezeDebugee() {
            if (!isFreezed){
                isFreezed = true;
                debug.Stop();
            }
        }

        public void ActivateDebugee() {
            if (isFreezed) {
                isFreezed = false;
                debug.Continue();
            }
        }
        private void StopDebugee() {
            if (!isFreezed) {
                debug.Stop();
            }
        }
        private void WakeDebugee(){
            if (!isFreezed) {
                debug.Continue();
            }
        }
        public List<string> GetStackTrace(uint ThreadOSid){
            if (!isAttached) {
                throw new Exception("Not attched to the process");
            }
            StopDebugee();
            List<string> stackTrace = null;
            CorThread thread = null;
            if (liveThreads.TryGetValue(ThreadOSid, out thread)) {
                stackTrace = GetCallStack(thread);
            } else {
                deadThreads.TryGetValue(ThreadOSid, out stackTrace);
            }
            
            WakeDebugee();
            return stackTrace;
        }
        public IList<PARAMETER> GetArguments(uint ThreadOSid) {
            IList<PARAMETER> args = null;
            if (!isAttached){
                throw new Exception("Not attched to the process");
            }
            StopDebugee();
            CorThread thread = null;
            if (liveThreads.TryGetValue(ThreadOSid,out thread)) {
                args = thread.ActiveFrame.GetParamInfo();
                IList<PARAMETER> metaArgs = MetadataMgr.GetFunArguments(thread.ActiveFrame.Function);
                int index = 0;
                int hasThis = 0;
                //Hack : this condition need modify after check the HasThis
                if (args.Count == metaArgs.Count + 1) {
                    hasThis = 1;
                    args[0].name = "this";
                    index++;
                }
                for (; index < args.Count; index++) {
                    args[index].name = metaArgs[index-hasThis].name;
                }
            }
            
            WakeDebugee();
            return args;
        }
        private string GetCurrentSourceCode(CorSourcePosition source)
        {
            SourceFileReader sourceReader = new SourceFileReader(source.Path);
            StringBuilder sb = new StringBuilder();
            // Print three lines of code
            if (source.StartLine >= sourceReader.LineCount ||
                source.EndLine >= sourceReader.LineCount)
                return string.Empty;

            for (Int32 i = source.StartLine; i <= source.EndLine; i++)
            {
                String line = sourceReader[i];
                sb.AppendLine(line);

            }
            return sb.ToString();
        }
        private List<string> GetCallStack(CorThread thread) {
            List<string> stackTrace = new List<string>();
            
            foreach (CorFunctionFrame fnFrame in thread.StackFrames){
                if (fnFrame.Function != null) {
                   string funName = MetadataMgr.GetFullName(fnFrame.Function);
                    stackTrace.Add(funName);
                }
            }
            
            if (stackTrace.Count == 0)
            {
                stackTrace.Add(thread.StackFailureReason);
            }
            return stackTrace;
        }

        public void RegisterForBreakPointNotification(BreakPointNotifcation notification)
        {
            debug.FuntionBreakPoint += new DebugEng.BreakPoint(debug_FuntionBreakPoint);
            BreakPointFunctionNotifcation += notification;
        }

        void debug_FuntionBreakPoint(CorThread thread)
        {
            BREAKPOINT bp = new BREAKPOINT();
            bp.Threadid = thread.ID;
            bp.frame = MetadataMgr.GetFullName(thread.StackFrames[0].Function);
            BreakPointFunctionNotifcation(bp);
        }
       
        
        public void RegisterForAssemblyLoad_Unload(AssemblyLoad_UnloadNotification notification){
            debug.AssemblyNotification += new DebugEng.AssemblyLoad_UnloadNotification(debug_AssemblyNotification);
            AssmNotification += notification;
        }
        public void UnRegisterForAssemblyLoad_Unload(AssemblyLoad_UnloadNotification notification){
            debug.AssemblyNotification -= new DebugEng.AssemblyLoad_UnloadNotification(debug_AssemblyNotification);
            AssmNotification -= notification;
        }
        
        public void RegisterForTreadCreate_TerminateNotification( TreadCreate_TerminateNotification notification)
        {
            debug.ThreadNotification += new DebugEng.TreadCreate_TerminateNotification(debug_ThreadNotification);
            ThreadNotification += notification;
        }
        public void UnRegisterForTreadCreate_TerminateNotification( TreadCreate_TerminateNotification notification)
        {
            debug.ThreadNotification -= new DebugEng.TreadCreate_TerminateNotification(debug_ThreadNotification);
            ThreadNotification -= notification;
        }

        public void RegiesterForProcessTermintatedNotification(ProcessTerminated notification) {
            debug.Process_Exit += new DebugEng.ProcessExit(debug_Process_Exit);
            ProcessTerminatedNotification += notification;
        }
        public void UnregiesterForProcessTermintatedNotification(ProcessTerminated notification)
        {
            debug.Process_Exit -= new DebugEng.ProcessExit(debug_Process_Exit);
            ProcessTerminatedNotification -= notification;
        }
        void debug_Process_Exit(CorProcess process)
        {
            PROCESS prs = new PROCESS();
            prs.processid = process.ID;
            prs.terminated = true;
            //Hard coded for now. will be fixed later
            prs.exitCode = 0;
            ProcessTerminatedNotification(prs);
        }
        void debug_AssemblyNotification(CorDebugAppDomain app, CorAssembly assm, bool isLoad){
            ASSEMBLY assembly = new ASSEMBLY();
            assembly.Name = assm.Name;
            assembly.Path = assm.Location;
            if (isLoad) {
                assembly.LoadedTime = DateTime.Now;
            }else{
                assembly.UnloadedTime = DateTime.Now;
            }
            AssmNotification(assembly);
        }
        void debug_ThreadNotification(CorDebugAppDomain app, CorThread thread, bool isAlive) {
            THREAD _thread = new THREAD();
            _thread.Id = thread.ID;
            _thread.ProcessId = debug.Process.ID;
            if (isAlive){
                _thread.State = THREADSTATE.RUNNING;
                liveThreads.Add(thread.ID, thread);
            } else {
                _thread.State = THREADSTATE.TERMINATED;
                liveThreads.Remove(thread.ID);
                deadThreads.Add(thread.ID, GetCallStack(thread));
            }
            ThreadNotification(_thread);
        }
        Dictionary<uint, string> threadVsExceptionSrc = new Dictionary<uint, string>();
        
        public bool SetBreakPoint(string module,string className, string function) {
            bool isbpActivated = false;
            CorBreakPoint bfp = null;
            if (!bpDictionary.TryGetValue(module + "!" + className + "#" + function, out bfp))
            {
                CorFunction fun = MetadataMgr.GetFuntion(module, className, function);
                bfp = fun.CreateBreakPoint();
                if (bfp != null)
                {
                    bpDictionary.Add(module + "!" + className + "#" + function, bfp);
                    isbpActivated = true;
                }
            }
            else {
                bfp.Activate();
                isbpActivated = true;
            }
            return isbpActivated;
        }
        public bool DeActivateBreakPoint(string module, string className, string function) { 
            CorBreakPoint bfp = null;
            if (bpDictionary.TryGetValue(module + "!" + className + "#" + function, out bfp)) {
                bfp.DeActivate();
                return true;
            }
            return false;
        
        }
        public VARIABLE GetParamValue(DEBUGPARAM param) {
            return new VARIABLE();
            //if (!param.isProperty)
            //{q
            //    return CorValue2Text.GetValue(param.corValue, param.corType);
            //}
            //else {
            //    return new VARIABLE();
            //}
            ///
        }
        //static AutoResetEvent myResetEvent = new AutoResetEvent(false);
        //temp method
        public VARIABLE GetParamValue(DEBUGPARAM param,uint threadID)
        {
            
            if (!param.isProperty){
                return new VARIABLE();
               // return CorValue2Text.GetValue(param.corValue, param.corType);
            } else {
                CorThread corThread = null;
                if (liveThreads.TryGetValue(threadID, out corThread)) {
                    corThread.GetPropertyValue(param);
                }
                ControllerStateMgr.ReleaseCallBackFlag();
                TempFuncEval.mEvent.WaitOne();
                //ControllerStateMgr.SetState(ControllerState.Paused);
                //ControllerStateMgr.RaiseCallBackFlag();
                return CorValue2Text.GetValue(TempFuncEval.value, CorElementType.ELEMENT_TYPE_VOID);

                //return new VARIABLE();
            }
            
        }
    }
}
