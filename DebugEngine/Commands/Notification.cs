using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugEngine.Commands
{
    public abstract class NotificationResult { 
        public uint ProcessID{ get ; internal set;}
    }
    public interface INotification{
         void FireEvent(NotificationResult result);
    }
    
    public class ThreadNotificationResult : NotificationResult {
        public bool IsCreated { get; internal set; }
        public uint ThreadID { get; internal set; }
    }

    
    public class ExceptioNotificationResult : NotificationResult {
        public enum EXCEPTIONSTATE {
            FIRSTCHANCE,
            HANDLED,
            UNHANDLED
        }
        public string ExceptionType;
        public string SourceLine;
        public uint ThreadID;
        public List<string> StackTrace;
        public EXCEPTIONSTATE State;
    }
    public class BreakPointNotificationResult : NotificationResult {
        public uint ThreadID { get; private set; }
        public string Module { get; private set; }
        public string Class { get; private set; }
        public string Method { get; private set; }

        public BreakPointNotificationResult(
            uint _pid, 
            uint _tid,
            string _module,
            string _class,
            string _method) {
            ProcessID = _pid;
            ThreadID = _tid;
            Module = _module;
            Class = _class;
            Method = _method;
        }
    }
    public class LoadedAssemblyNotificationResult : NotificationResult {
        public string FullName { get; internal set; }
        public string Name { get; internal set; }
        public string AppDomainName { get; internal set; }
        public LoadedAssemblyNotificationResult(uint _processID, string _fullName, string _name, string _appDomain) {
            ProcessID = _processID;
            FullName = _fullName;
            Name = _name;
            AppDomainName = _appDomain;
        }
    }
}
