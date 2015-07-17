using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DebugEngine.Interfaces;

namespace DebugEngine.Debugee.Wrappers
{
    public class CorProcess
    {
        protected ICorDebugProcess corProcess;
        public CorProcess(ICorDebugProcess process) {
            corProcess = process;
        }
        internal IList<CorDebugThread> Threads {
            get {
                List<CorDebugThread> threads = new List<CorDebugThread>();
                ICorDebugController iDebugController = corProcess;
                ICorDebugThreadEnum threadsEnum = null;
                iDebugController.EnumerateThreads(out threadsEnum);
                ICorDebugThread[] thread = new ICorDebugThread[1];
                uint c = 0;
                uint threadcount = 0;
                threadsEnum.GetCount(out threadcount);
                //threads = new List<CorThread>();
                for (; threadsEnum.Next(1, thread, out c) == 0; /* empty */){
                    threads.Add(new CorDebugThread(thread[0]));
                }
                return threads; 
            }
        }

        internal IList<CorDebugAppDomain> AppDomains{
            get{
                List<CorDebugAppDomain> appdomains = new List<CorDebugAppDomain>();
                ICorDebugAppDomainEnum appDomainsEnum = null;
                corProcess.EnumerateAppDomains(out appDomainsEnum);
                ICorDebugAppDomain[] appDomain = new ICorDebugAppDomain[1];
                uint c = 0;
                uint threadcount = 0;
                appDomainsEnum.GetCount(out threadcount);
                for (; appDomainsEnum.Next(1, appDomain, out c) == 0; /* empty */){
                    appdomains.Add(new CorDebugAppDomain(appDomain[0]));
                }
                return appdomains;
            }
        }
        public uint ID {
            get {
                uint id = 0;
                corProcess.GetID(out id);
                return id;
            }
        }
        public void Detach() {
            corProcess.Detach();
        }
        public ICorDebugThread GetThread(uint _threadID)
        { 
            ICorDebugThread thread = null;
            corProcess.GetThread(_threadID, out thread);
            return thread;
        }
        public void Freeze() {
            corProcess.Stop(0);
        }
        public void Continue() {
            corProcess.Continue(0);
        }
    }
    enum DebugProcState : byte {
            Detached = 0,
            Attached = 1,
            Freezed = 2
        }
    class TargetProcess : CorProcess{
        uint currentState = 0;

        public TargetProcess(ICorDebugProcess debugee)
            : base(debugee)
        {
            currentState = (uint) DebugProcState.Attached;
        }
        public  void Resume(){
            lock (this)
            {
                if ((currentState & (uint)DebugProcState.Attached) == 0)
                {
                    throw new NotSupportedException("Debugee is not attached");
                }
                else if ((currentState & (uint)(DebugProcState.Attached | DebugProcState.Freezed)) != 0)
                {
                    corProcess.Continue(0);
                    currentState = (uint)(DebugProcState.Attached);
                }
            }
        }
        public void Pause() {
            lock (this)
            {
                if ((currentState & (uint)DebugProcState.Attached) == 0)
                {
                    throw new NotSupportedException("Debugee is not attached");
                }
                else if ((currentState & (uint)(DebugProcState.Attached | DebugProcState.Freezed)) == (uint)DebugProcState.Attached)
                {
                    corProcess.Stop(0);
                    currentState = (uint)(DebugProcState.Attached | DebugProcState.Freezed);
                }
            }
        }
        public void Detach() {
            lock (this) {
                currentState = (uint)DebugProcState.Detached;
                corProcess.Detach();
                corProcess = null;
            }
        }
    }
}
