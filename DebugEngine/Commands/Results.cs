using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DebugEngine.Debugee.Wrappers;
using DebugEngine.Debugee;

namespace DebugEngine.Commands
{
        public abstract class Result {
            public bool CommadStatus {
                get;internal set;
            }

            public Exception FailureException {
                get;
                internal set;
            }
            public string Description {
                get;
                internal set;
            }
            public uint ProcessID { get; internal set; }
            public uint CommandUID { get; internal set; }
        }
        public class AttachResult : Result{}
        public class DetachResult : Result{}
        public class DynamicResult : Result {
            public bool isEventResult;
        }
        public class ThreadsResult : DynamicResult  {
            public IList<uint> ThreadIDs { get; internal set; }
            public ThreadsResult() {
                isEventResult = false;
                ThreadIDs = new List<uint>();
            }
        }
        public class AppDomainsResult : Result
        {
            public IList<string> AppDomains { get; internal set; }
            public AppDomainsResult(){
                AppDomains = new List<string>();
            }
        }
        public class AssembliesResult : Result
        {
            public string AppDomainName { get; internal set; }
            public IList<string> Assemblies { get; internal set; }
            public AssembliesResult(){
                Assemblies = new List<string>();
            }
        }
        public class CallStackResult : Result {
            public IList<string> CallStack { get; internal set; }
            public CallStackResult(){
                CallStack = new List<string>();
            }
        }
        public class BreakPointResult : Result{
            //internal CorBreakPoint breakPoint { get; internal set; }
            public bool IsPending { get; internal set; }
            public string Module { get; internal set; }
            public string Class { get; internal set; }
            public string Method { get; internal set; }
            public uint Line { get; internal set; }
        }
        public class ExceptionResult : Result { }
        public class GoResult : Result { }
        public class AssemblyLoadResult : Result { }
        public class RemoveBreakPointResult : Result {
            public string Module { get; internal set; }
            public string Class { get; internal set; }
            public string Method { get; internal set; }
            public uint Line { get; internal set; }
        }
        public class ArgumentsResult : Result {
            public IList<PARAMETER> Args { get; internal set; }
        }
        public class MembersResult : Result{
            public IList<PARAMETER> Members { get; internal set; }
        }
    public class MemberValueResult : Result {
            public string Value { get; internal set; }
        }
        public class ResultFactory {
            public static Result CreateResultObject(string command) {
                Result result = null;
                switch (command) { 
                    case "attach":
                        result = new AttachResult();
                        break;
                    case "detach":
                        result = new DetachResult();
                        break;
                    case "getthreads":
                        result = new ThreadsResult();
                        break;
                    case "appdomains":
                        result = new AppDomainsResult();
                        break;
                    case "assemblies":
                        result = new AssembliesResult();
                        break;
                    case "callstack":
                        result = new CallStackResult();
                        break;
                    case "setbreakpoint":
                        result = new BreakPointResult();
                        break;
                    case "exception":
                        result = new ExceptionResult();
                        break;
                    case "go":
                        result = new GoResult();
                        break;
                    case "assemblyloadevent":
                        result = new AssemblyLoadResult();
                        break;
                    case "removebreakpoint":
                        result = new RemoveBreakPointResult();
                        break;
                    case "arguments":
                        result = new ArgumentsResult();
                        break;
                    case "members":
                        result = new MembersResult();
                        break;
                    case "value":
                        result = new MemberValueResult();
                        break;
                }
                return result;
            }
        }
        
}