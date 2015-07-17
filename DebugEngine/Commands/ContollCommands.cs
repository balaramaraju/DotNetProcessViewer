using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using DebugEngine.Interfaces;
using DebugEngine.Debugee;
using DebugEngine.Debugee.Wrappers;
using DebugEngine.MetaDataUtils;
using System.Threading;
using DebugEngine.Utilities;

namespace DebugEngine.Commands{
    public class AttachCmd : Command{
        public AttachCmd(uint _processId, IReciver _target):base(_processId,"attach",_target){}
        protected override void CoreExecute(ref Result _result){
            Process debugee = Process.GetProcessById((int)m_ProcessID);
            ICorDebug icorDebug = CLRHelperMethods.GetDebugger(debugee.Handle);
            icorDebug.Initialize();
            MProcess mprocess = new MProcess(m_ProcessID);
            ManagedCallback callback = new ManagedCallback(mprocess.Listner);
            icorDebug.SetManagedHandler(callback);
            ICorDebugProcess iCorProcess = null;
            icorDebug.DebugActiveProcess(m_ProcessID, 0, out iCorProcess);
            if (iCorProcess != null) {
                mprocess.SetDebugProcess(iCorProcess);
                //mprocess.Continue();
                _result.CommadStatus = true;
                _result.Description = "Process " + m_ProcessID + " Attached";
                
            }
            //Just to complete the Attach process completetly .
            //This a need to be fixed.
            Thread.Sleep(2000);
        }
    }
    public abstract class StateLessCommand : Command
    {
        public StateLessCommand(uint _processId, string _name, IReciver _target) : base(_processId, _name, _target) { }
        protected override void CoreExecute(ref Result _result)  {
            MProcess mprocess = MDBGService.GetDebugee(m_ProcessID);
            if (mprocess != null) {
                InnerExec(mprocess,ref _result);
            }else{
                _result.CommadStatus = false;
                _result.Description = "Process " + m_ProcessID + " not attached";
            }
        }
        protected abstract void InnerExec(MProcess _process, ref Result _result);
        //protected abstract void InnerExec(MProcess _process, ref Result _result);
    }
    
    public abstract class StatedCommand : Command {
        public StatedCommand(uint _processId, string _name, IReciver _target) : base(_processId, _name, _target) { }
        protected override void CoreExecute(ref Result _result) {
            MProcess mprocess = MDBGService.GetDebugee(m_ProcessID);
            try{
                mprocess.Frezee();
                InnerExec(mprocess, ref _result);
            }catch (Exception){
                throw;
            }finally{
                
                mprocess.Continue();
            }
        }
        protected abstract void InnerExec(MProcess _process, ref Result _result);
    }
    public abstract class InteruptCommand : StateLessCommand {
        public InteruptCommand(uint _processId, string command,IReciver _target) : base(_processId, "detach", _target) { }
    }
    public class GoCmd : StateLessCommand
    {
        public GoCmd(uint _processId, IReciver _target) : base(_processId, "go", _target) { }
        protected override void InnerExec(MProcess _process, ref Result _result){
            _process.Continue();
            _result.CommadStatus = true;
            _result.Description = "Process " + m_ProcessID + " Continue..";
        }
    }
    public class DetachCmd : InteruptCommand {
        public DetachCmd(uint _processId, IReciver _target) : base(_processId, "detach", _target) { }
        protected override void InnerExec(MProcess _mprocess, ref Result _result) {
            _mprocess.Dispose();
            _mprocess = null;
            _result.CommadStatus = true;
            _result.Description = "Process " + m_ProcessID + " Detached";
        }
    }
    public class ThreadsCmd : StatedCommand {
        IReciver m_Target;
        INotification m_Notification;
        public ThreadsCmd(uint _processId, IReciver _target) : base(_processId, "getthreads", _target) {
            m_Target = _target;
        }
        public ThreadsCmd(uint _processId, IReciver _target,INotification _notifer )
            : base(_processId, "getthreads", _target)
        {
            m_Target = _target;
            m_Notification = _notifer;
        }
        protected override void InnerExec(MProcess _mprocess, ref Result _result) {
            ThreadsResult tresult = _result as ThreadsResult;
            foreach (CorDebugThread thread in _mprocess.GetThreads()) {
                tresult.ThreadIDs.Add(thread.ID);
            }
            _result.CommadStatus = true;
            _result.Description = 
                "Total number of threads " + tresult.ThreadIDs.Count + 
                "in process " + m_ProcessID ;
            if (m_Notification != null){
                _mprocess.SubscribeForThreadsNotification(m_Notification);
            }
        }
        
    }
    public class AppDomainCmd : StateLessCommand {
        public AppDomainCmd(uint _processId, IReciver _target) : base(_processId, "appdomains",_target) { }
        protected override void InnerExec(MProcess _mprocess, ref Result _result) {
                AppDomainsResult appResult = _result as AppDomainsResult;
                foreach (CorDebugAppDomain appDomain in _mprocess.GetAppDomains()) {
                    appResult.AppDomains.Add(appDomain.Name);
                }
                _result.CommadStatus = true;
                _result.Description = "Total number of AppDomains " + appResult.AppDomains.Count + 
                    "in process " + m_ProcessID ;
        }
    }
    public class LoadedAssembliesCmd : StateLessCommand{
        string m_AppDomainName;
        
        public LoadedAssembliesCmd(uint _processId, string _appName, IReciver _target) : base(_processId, "assemblies",_target) {
            m_AppDomainName = _appName;
        }
        
        protected override void InnerExec(MProcess _mprocess, ref Result _result){
            AssembliesResult assmResult = _result as AssembliesResult;
            assmResult.AppDomainName = m_AppDomainName;
            CorDebugAppDomain desiredAppDomain = null;
            
            foreach (CorDebugAppDomain appDomain in _mprocess.GetAppDomains()){
                if (m_AppDomainName == appDomain.Name){
                    desiredAppDomain = appDomain;
                    break;
                }
            }
            if (desiredAppDomain != null) {
                foreach(CorAssembly assm in desiredAppDomain.LoadedAssemblies){
                    assmResult.Assemblies.Add(assm.FullName);
                    _result.CommadStatus = true;
                    _result.Description = "Total number of Assemblies " + assmResult.Assemblies.Count +
                        "in AppDomain " + m_Name;
                }
            }else{
                _result.CommadStatus = true;
                _result.Description = "AppDomain " + m_Name + "not alive";
            }
        }
    }
    public class CallStackCmd : StatedCommand{
        uint m_ThreadID;
        public CallStackCmd(uint _processId,uint _ThreadID, IReciver _target) : base ( _processId, "callstack",_target){
            m_ThreadID = _ThreadID;
        }
        protected override void InnerExec(MProcess _process, ref Result _result){
            CorDebugThread debugThread = _process.GetThread(m_ThreadID);
            if(debugThread != null){
                CallStackResult callResult = _result as CallStackResult;
                List<string> stackTrace = new List<string>();
                foreach (CorFunctionFrame fnFrame in debugThread.CallStack){
                    if (fnFrame.Function != null){
                        string funName = MetadataMgr.GetFullName(fnFrame.Function);
                        stackTrace.Add(funName);
                    }
                }
                if (stackTrace.Count == 0){
                    _result.CommadStatus = false;
                    _result.Description = debugThread.StackFailureReason;
                } else {
                    _result.CommadStatus = true;
                    callResult.CallStack = stackTrace;
                    _result.Description = "Stack trace succefully retrived for the thread " + m_ThreadID;
                }
            }else{
                _result.CommadStatus = false;
                _result.Description = "Thread is not available [might be terminated] " + m_ThreadID;
            }
            
        }

    }
    public class SetBreakPointCmd : StatedCommand {
        string m_ModuleName;
        string m_ClassName;
        string m_MethodName;
        uint m_Line;
        INotification m_Listen;
        public SetBreakPointCmd(
            uint _processID, 
            string _module, 
            string _class, 
            string _method, 
            uint _line, 
            IReciver _target,
            INotification _listen)
            : base(_processID, "setbreakpoint", _target)
        { 
            m_ModuleName = _module;
            m_ClassName = _class;
            m_MethodName = _method;
            m_Line = _line;
            m_Listen = _listen;
        }
        protected override void InnerExec(MProcess _process, ref Result _result){
            BreakPointResult bpResult = _result as BreakPointResult;
            bpResult.Method = m_MethodName;
            bpResult.Module = m_ModuleName;
            bpResult.Class = m_ClassName;
            bpResult.Line = m_Line;
            if(_process.CreateBreakPoint(m_ModuleName,m_ClassName,m_MethodName,m_Listen)){
                bpResult.IsPending = false;
                bpResult.Description = "Break point enable.";
            }else{
                bpResult.IsPending = true;
                bpResult.Description =  m_ModuleName + " yet to load in process memory";
            }
            bpResult.CommadStatus = true;
        }
        //private 
    }
    public class RemoveBreakPointCmd : StatedCommand
    {
        string m_ModuleName;
        string m_ClassName;
        string m_MethodName;
        uint m_Line;

        public RemoveBreakPointCmd(
            uint _processID,
            string _module,
            string _class,
            string _method,
            uint _line,
            IReciver _target)
            : base(_processID, "removebreakpoint", _target)
        {
            m_ModuleName = _module;
            m_ClassName = _class;
            m_MethodName = _method;
            m_Line = _line;

        }
        protected override void InnerExec(MProcess _process, ref Result _result)
        {
            RemoveBreakPointResult bpResult = _result as RemoveBreakPointResult;
            bpResult.Method = m_MethodName;
            bpResult.Module = m_ModuleName;
            bpResult.Class = m_ClassName;
            bpResult.Line = m_Line;
            if (_process.DeleteBreakPoint(m_ModuleName, m_ClassName, m_MethodName)){
                bpResult.Description = "Break point disabled.";
                bpResult.CommadStatus = true;
            }else{
                bpResult.Description = "break point is not found to disable";
            }
            
        }
        //private 
    }
    public class EnableExceptionCatcherCmd : StateLessCommand{
        IReciver m_Target;
        INotification m_Notification;
        public EnableExceptionCatcherCmd(uint _processId, IReciver _target, INotification _notification)
            : base(_processId, "exception", _target){
            m_Target = _target;
            m_Notification = _notification;
        }
        protected override void InnerExec(MProcess _process, ref Result _result){
            ExceptionResult expResult = _result as ExceptionResult;
            if (m_Notification != null){
                _process.SubscribeForExceptionNotification(m_Notification);
                expResult.CommadStatus = true;
                expResult.Description = "Subscribed for Exception Watcher";
            }
            else {
                expResult.CommadStatus = false;
                expResult.Description = "Failed to Subscribe : Notifier is null";
            }
        }
    }
    public class EnableAssemblyEventsCmd : StateLessCommand {
        INotification m_Notification;
        public EnableAssemblyEventsCmd(uint _processId, IReciver _target, INotification _notification)
            : base(_processId, "assemblyloadevent", _target){
                m_Notification = _notification;
        }
        protected override void InnerExec(MProcess _process, ref Result _result){
            AssemblyLoadResult expResult = _result as AssemblyLoadResult;
            if (m_Notification != null){
                _process.SubscribeForAssemblyLoadedNotification(m_Notification);
                expResult.CommadStatus = true;
                expResult.Description = "Subscribed for Assembly Load Notification";
            } else {
                expResult.CommadStatus = false;
                expResult.Description = "Failed to Subscribe : Notifier is null";
            }
        }
    }
    public class GetArgumentsCmd : StatedCommand {
        public uint m_ThreadID;
        public  GetArgumentsCmd(uint _processId, uint _threadID ,IReciver _target): base(_processId, "arguments", _target){
            m_ThreadID = _threadID;
        }
        protected override void InnerExec(MProcess _process, ref Result _result){
           ArgumentsResult argResults = _result as ArgumentsResult;
           CorDebugThread thread = _process.GetThread(m_ThreadID);
           IList<PARAMETER> args = thread.ActiveFrame.GetParamInfo();
           IList<PARAMETER> metaArgs = MetadataMgr.GetFunArguments(thread.ActiveFrame.Function);
           int index = 0;
           int hasThis = 0;
           if (args.Count == metaArgs.Count + 1){
               hasThis = 1;
               args[0].name = "this";
               index++;
           }
           for (; index < args.Count; index++){
               args[index].name = metaArgs[index - hasThis].name;
               if (((DEBUGPARAM)args[index]).inValid){
                   args[index].type = metaArgs[index - hasThis].name;
               }
           }
           argResults.CommadStatus = true;
           argResults.Description = "Retrived the Arguments";
           argResults.Args = args;
        }
    }
    public class GetMembersCmd : StatedCommand
    {
        public uint m_ThreadID;
        public PARAMETER m_Param;
        public GetMembersCmd(uint _processId, uint _threadID, PARAMETER _param ,IReciver _target)
            : base(_processId, "members", _target){
            m_ThreadID = _threadID;
            m_Param = _param;
        }
        protected override void InnerExec(MProcess _process, ref Result _result){
            MembersResult result = _result as MembersResult;
            DEBUGPARAM param = m_Param as DEBUGPARAM;
            result.CommadStatus = false;
            if(param != null){
                if (!param.inValid){
                    result.CommadStatus = true;
                    MDbgValue value = param.corValue;
                    //Needs a fix for property???
                    result.Members = new List<PARAMETER>();
                    foreach (MDbgValue mdgbVal in value.GetFields()) {
                        result.Description = mdgbVal.Name + "  " + mdgbVal.TypeName;
                        DEBUGPARAM debugParam = new DEBUGPARAM();
                        debugParam.name = mdgbVal.Name;
                        debugParam.corValue = mdgbVal;
                        debugParam.isNull = mdgbVal.IsNull;
                        debugParam.isComplex = mdgbVal.IsComplexType;
                        debugParam.isArray = mdgbVal.IsArrayType;
                        debugParam.type = mdgbVal.TypeName;
                        result.Members.Add(debugParam);
                    }
                    //result.Members = DebugEngine.Utilities.CorValue2Text.GetObjectMembers(param.corValue);
                } else {
                    result.Description = "Value not available. Optimized by JIT/CLR";
                }
            }else{
                 result.Description = "Invalid input value";
            }
        }
    }
    public class GetValueCmd : StatedCommand
    {
        public uint m_ThreadID;
        public PARAMETER m_Param;
        public GetValueCmd(uint _processId, uint _threadID, PARAMETER _param, IReciver _target)
            : base(_processId, "value", _target)
        {
            m_ThreadID = _threadID;
            m_Param = _param;
        }
        protected override void InnerExec(MProcess _process, ref Result _result)
        {
            MemberValueResult result = _result as MemberValueResult;
            DEBUGPARAM param = m_Param as DEBUGPARAM;
            result.CommadStatus = false;
            if (param != null)
            {
                if (!param.inValid)
                {
                    result.CommadStatus = true;
                    MDbgValue value = param.corValue;
                    if (!(value.IsArrayType || value.IsComplexType))
                    {
                        result.Value = CorValue2Text.GetCorValue2Text(value.CorValue.Raw, 0);
                    }
                    else {
                        result.Description = "Value type is not primitive";
                    }
                }
                else
                {
                    result.Description = "Value not available. Optimized by JIT/CLR";
                }
            }
            else
            {
                result.Description = "Invalid input value";
            }
            
        }
    }
    //Exception Catcher.
    //GetValue ""
}
