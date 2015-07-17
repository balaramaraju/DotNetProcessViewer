using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DebugEngine.Interfaces;
using DebugEngine.Debugee.Wrappers;
using System.Collections.Concurrent;
using DebugEngine.MetaDataUtils;
using DebugEngine.Utilities;
using System.Threading;
using DebugEngine.Debugee;
using DebugEngine.Commands;

namespace DebugEngine.Commands
{
    public class MDBGService 
    {
        private static IDictionary<uint, MProcess> debugeeCache
            = new ConcurrentDictionary <uint, MProcess>();
        internal static void Register(uint _pid, MProcess debugee){
            debugeeCache.Add(_pid, debugee);
        }
        internal static void Unregister(uint _pid){
            debugeeCache.Remove(_pid);
        }
        internal static MProcess GetDebugee(uint _pid) { 
            MProcess mprocess = null;
            debugeeCache.TryGetValue(_pid, out mprocess);
            return mprocess;
        }
        
        private static IList<Command> pendingCommandsList = new List<Command>();
        private static IList<Command> interruptCommandsList = new List<Command>();
        private static Thread serviceThread;
        static ManualResetEvent commadPendingEvent = new ManualResetEvent(true);
        public static void PostCommand(Command cmd) {
            if (serviceThread == null || !serviceThread.IsAlive) {
                serviceThread = new Thread(Invoker);
                serviceThread.IsBackground = true;
                serviceThread.Name = "Debuger Thread";
                serviceThread.Start();
            }
            lock (pendingCommandsList) {
                if (cmd is InteruptCommand){
                    interruptCommandsList.Add(cmd);
                }else{
                    pendingCommandsList.Add(cmd);
                }
                if (pendingCommandsList.Count == 1 || interruptCommandsList.Count ==1){
                    commadPendingEvent.Set();
                }
            }
        }

        private static void Invoker() {
            while (true){
                Command cmd = null;
                lock (interruptCommandsList) {
                    if (interruptCommandsList.Count != 0) {
                        cmd = interruptCommandsList[0];
                        interruptCommandsList.RemoveAt(0);
                    }
                }
                if(cmd == null){
                    lock (pendingCommandsList){
                        if (pendingCommandsList.Count != 0){
                            cmd = pendingCommandsList[0];
                            pendingCommandsList.RemoveAt(0);
                        }
                    }
                }
                if (cmd != null){
                    cmd.Execute();
                    if (cmd is DetachCmd) {
                        pendingCommandsList.Clear();
                        break;
                    }
                }
                if (pendingCommandsList.Count == 0) { 
                    commadPendingEvent.WaitOne();
                    commadPendingEvent.Reset();
                }
            }
        }
    }
    
    /// <summary>
    /// Need fix : dosen't support overload methods.
    /// </summary>
    public sealed class BreakPointManager : IDisposable {
        public static string getUID(string moduleName, string className, string methodName) {
            return moduleName + "!" + className + "#" + methodName;
        }
        private struct BreakPointInfo {
            public BreakPointInfo(string _module, string _class, string _method, CorBreakPoint _bpoint, INotification _callback)
            {
                module = _module;
                className = _class;
                methodName = _method;
                bpoint = _bpoint;
                evnt = _callback;
            }
            public string module;
            public string className;
            public string methodName;

            internal string Identifier {
                get {
                    return getUID(module , className , methodName);
                }
            }
            
            internal CorBreakPoint bpoint;
            internal INotification evnt;
        }
        private IDictionary<string, List<BreakPointInfo>> breakStringVsBP
            = new ConcurrentDictionary<string, List<BreakPointInfo>>();
        
        internal void SetBreakPoint(CorModule module, string className, string methodName, INotification _lisenter){
            BreakPointInfo breakpoint = new BreakPointInfo(module.Name, className, methodName, null, _lisenter);
            if (!breakStringVsBP.ContainsKey(breakpoint.Identifier)){
                int token = 0;
                CorFunction fun = null;
                try{
                    module.Importer.FindTypeDefByName(className, 0, out token);
                } catch (Exception){
                    throw new BreakPointException(className + " class is not found in" + module.Name);
                }
                MetaType type = new MetaType(module.Importer, token);
                try{
                    List<BreakPointInfo> bps = new List<BreakPointInfo>();
                    foreach (MetadataMethodInfo methodInfo in type.GetMethods(methodName)){
                        BreakPointInfo bp = new BreakPointInfo(module.Name, className, methodName, null, _lisenter);
                        fun = module.GetCorFuntion((uint)methodInfo.MetadataToken);
                        bp.bpoint = fun.CreateBreakPoint();
                        bp.bpoint.Activate();
                        bps.Add(bp);
                    }
                    if(bps.Count > 0){
                        breakStringVsBP.Add(bps[0].Identifier, bps);
                    }
                } catch (Exception) {
                    throw new BreakPointException(methodName + " Method is not found in" + className);
                }
            }
        }
        public INotification GetNotificaiton(string identifier) { 
            List<BreakPointInfo> breakPoints = null;
            INotification bpNotificaiton = null;
            if (breakStringVsBP.TryGetValue(identifier, out breakPoints)) {
                bpNotificaiton = breakPoints[0].evnt;
            }
            return bpNotificaiton;
        }
        public bool DeleteBreakPoint(string module, string className, string methodName) { 
            List<BreakPointInfo> breakPoints = null;
            
            if (breakStringVsBP.TryGetValue(getUID(module, className, methodName), out breakPoints)) {
                foreach (BreakPointInfo bp in breakPoints){
                    bp.bpoint.DeActivate();
                }
                breakStringVsBP.Remove(getUID(module, className, methodName));
            }
            return breakPoints != null ? true : false;
        }
        public void  Dispose() {
            foreach(string key in breakStringVsBP.Keys){
                foreach (BreakPointInfo bp in breakStringVsBP[key]){
                    ((CorBreakPoint)bp.bpoint).DeActivate();
                }
            }
 	        //throw new NotImplementedException();
        }
    }
    public sealed class MProcess : IDisposable{
        private BreakPointManager m_BreakpointMgr = new BreakPointManager();
        private CorProcess m_Process;
        private uint m_ProcessID;
        public MDBGEventListner Listner { get; private set; }
        public void SetDebugProcess(ICorDebugProcess _process) {
            m_Process = new CorProcess(_process);
            MDBGService.Register(m_ProcessID, this);
        }
        internal MProcess(uint _processID){
            m_ProcessID = _processID;
            Listner = new MDBGEventListner(_processID,m_BreakpointMgr);
        }
        public void  Dispose() {
            m_BreakpointMgr.Dispose();
            m_Process.Freeze();
            m_Process.Detach();
            Listner.Dispose();
            MDBGService.Unregister(m_ProcessID);
            Listner = null;
        }
        internal IList<CorDebugThread> GetThreads() {
            return m_Process.Threads;
        }
        internal IList<CorDebugAppDomain> GetAppDomains() {
            return m_Process.AppDomains;
        }
        internal CorDebugThread GetThread(uint _tid) {
            CorDebugThread corThread = null;
            ICorDebugThread thread = m_Process.GetThread(_tid);
            if (thread != null) {
                corThread = new CorDebugThread(thread);
            }
            return corThread;
        }
        internal bool CreateBreakPoint(string _moduleName, string _className, string _functionName, INotification _lisenter)
        {
            bool isActivated = false;
            IList<CorModule> modules = new List<CorModule>();
            foreach (CorDebugAppDomain appDomain in GetAppDomains()) {
                foreach (CorAssembly assm in appDomain.LoadedAssemblies) {
                    bool isFound = false;
                    foreach (CorModule modlue in assm.Modules) {
                        if (modlue.Name == _moduleName) {
                            modules.Add(modlue);
                            isFound = true;
                            break;
                        }
                    }
                    if (isFound) break;
                }
            }
            if (modules.Count != 0)  {
                foreach (CorModule module in modules)     {
                    m_BreakpointMgr.SetBreakPoint(module, _className, _functionName, _lisenter);
                }
                isActivated= true;
            }
            return isActivated;
        }
        internal void Frezee() {
            m_Process.Freeze();
        }
        internal void Continue() {
            m_Process.Continue();
        }
        internal void SubscribeForThreadsNotification(INotification notify) {
            Listner.SubscribeThreadNotification(notify);        
        }
        internal void SubscribeForExceptionNotification(INotification notify){
            Listner.SubscribeExceptionNotification(notify);
        }
        internal void SubscribeForAssemblyLoadedNotification(INotification notify) {
            Listner.SubscribeAssemblyLoadedNotification(notify);
        }
        internal bool DeleteBreakPoint(string _moduleName, string _className, string _functionName){
            return m_BreakpointMgr.DeleteBreakPoint(_moduleName, _className, _functionName);
        }
        
    }
    public class MDBGEventListner : IDisposable
    {
        private class InnerObject {
            public InnerObject(INotification _client, NotificationResult _result) {
                client = _client;
                result = _result;
            }
            public INotification client;
            public NotificationResult result;
        }
        uint m_ProcessID;
        BreakPointManager m_bpManger;
        public MDBGEventListner(uint _processID,BreakPointManager bpManager) {
            m_ProcessID = _processID;
            m_bpManger = bpManager;
        }
        INotification m_ThreadNotification = null;
        INotification m_ExceptionNotification = null;
        INotification m_AssemblyLoadedNotification = null;
        internal void SubscribeThreadNotification(INotification reciver){
           m_ThreadNotification = reciver;
        }
        internal void UnSubscribeThreadNotification() {
            m_ThreadNotification = null;
        }
        internal void PostThreadNotification(CorDebugThread _thread, bool isCreated) {
            if(m_ThreadNotification != null){
                ThreadNotificationResult threadResult = new ThreadNotificationResult();
                threadResult.IsCreated = isCreated;
                threadResult.ProcessID = m_ProcessID;
                threadResult.ThreadID = _thread.ID;
                ThreadPool.QueueUserWorkItem(
                    new WaitCallback(SendNotifications), 
                    new InnerObject(m_ThreadNotification, threadResult)
                    );
            }
        }
        internal void SubscribeExceptionNotification(INotification reciver){
            m_ExceptionNotification = reciver;
        }
        internal void UnSubscribeExceptionNotification(){
            m_ExceptionNotification = null;
        }

        internal void PostExceptionNotification(
            CorThread thread, 
            CorFunctionFrame frame, 
            Interfaces.CorDebugExceptionCallbackType exState, 
            uint offset
       ){
            if (m_ExceptionNotification == null) return;
            ExceptioNotificationResult expRes = new ExceptioNotificationResult();
            string strException = thread.ExceptionString;
            string sourceLine = string.Empty;
            if (frame.Function != null && exState == CorDebugExceptionCallbackType.DEBUG_EXCEPTION_FIRST_CHANCE){
                try{
                    CorSourcePosition src = frame.Function.GetSourcePositionFromIP((int)offset);
                    //expRes.SourceLine = GetCurrentSourceCode(src);
                }catch{
                    /*Ignore*/
                }
            }
            expRes.ExceptionType = strException;
            expRes.StackTrace = GetCallStack(thread);
            expRes.ThreadID = thread.ID;
            switch (exState){
                case Interfaces.CorDebugExceptionCallbackType.DEBUG_EXCEPTION_FIRST_CHANCE:
                case Interfaces.CorDebugExceptionCallbackType.DEBUG_EXCEPTION_USER_FIRST_CHANCE:
                    expRes.State = ExceptioNotificationResult.EXCEPTIONSTATE.FIRSTCHANCE; break;
                case Interfaces.CorDebugExceptionCallbackType.DEBUG_EXCEPTION_CATCH_HANDLER_FOUND:
                    expRes.State = ExceptioNotificationResult.EXCEPTIONSTATE.HANDLED; break;
                case Interfaces.CorDebugExceptionCallbackType.DEBUG_EXCEPTION_UNHANDLED:
                    expRes.State = ExceptioNotificationResult.EXCEPTIONSTATE.UNHANDLED;break;
            }
            ThreadPool.QueueUserWorkItem(
                new WaitCallback(SendNotifications),
                new InnerObject(m_ExceptionNotification, expRes)
                );
        }
        private string GetCurrentSourceCode(CorSourcePosition source){
            SourceFileReader sourceReader = new SourceFileReader(source.Path);
            StringBuilder sb = new StringBuilder();
            // Print three lines of code
            if (source.StartLine >= sourceReader.LineCount ||
                source.EndLine >= sourceReader.LineCount)
                return string.Empty;

            for (Int32 i = source.StartLine; i <= source.EndLine; i++){
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
            
            if (stackTrace.Count == 0){
                stackTrace.Add(thread.StackFailureReason);
            }
            return stackTrace;
        }
        private  void SendNotifications(Object _notifier) {
            InnerObject notifier = _notifier as InnerObject;
            notifier.client.FireEvent(notifier.result);
        }

        internal void PostBreakPoint(CorThread thread) {
            string moduleName = thread.ActiveFrame.Function.Module.Name;
            MetaType type = new MetaType(
                thread.ActiveFrame.Function.Module.Importer, (int)thread.ActiveFrame.Function.Class.Token);
            string className = type.Name;
            string methodName = type.FindMethod((int)thread.ActiveFrame.Function.Token).Name;

            methodName = methodName.Substring(methodName.LastIndexOf('.')+1);
            string identifier = BreakPointManager.getUID(moduleName, className, methodName);
            INotification notification = m_bpManger.GetNotificaiton(identifier);
            if (notification != null) {
                InnerObject innerObject
                    = new InnerObject(
                        notification,
                        new BreakPointNotificationResult(
                            m_ProcessID, 
                            thread.ID,
                            moduleName,
                            className,
                            methodName));
                ThreadPool.QueueUserWorkItem(new WaitCallback(SendNotifications), innerObject);
            }
            //List<BreakPointInfo> m_bpManger.GetBreakPointInfo(identifier);
        }
        internal void LoadedNewAssembly(CorAssembly assm, CorDebugAppDomain appDomain) {
            if (m_AssemblyLoadedNotification != null) {
                InnerObject innerObject
                    = new InnerObject(
                        m_AssemblyLoadedNotification,
                        new LoadedAssemblyNotificationResult(
                            m_ProcessID, 
                            assm.FullName, 
                            assm.Name, 
                            appDomain.Name)
                      );
                ThreadPool.QueueUserWorkItem(new WaitCallback(SendNotifications), innerObject);
            }
        }
        internal void SubscribeAssemblyLoadedNotification(INotification reciver){
            m_AssemblyLoadedNotification = reciver;
        }
        public void Dispose(){
            m_ThreadNotification = null;
            m_ProcessID = 0;
        }
    }
}
