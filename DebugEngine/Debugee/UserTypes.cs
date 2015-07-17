using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DebugEngine.Interfaces;
using System.Runtime.InteropServices;
using DebugEngine.Debugee.Wrappers;
using DebugEngine.Utilities;

namespace DebugEngine.Debugee
{
    public enum THREADSTATE { 
        SUSPENDED,
        RUNNING,
        TERMINATED,
        INVALID
    }
    public struct THREAD
    {
        public uint Id;
        public uint ProcessId;
        public THREADSTATE State;
        internal THREADSTATE ConvertState(CorDebugThreadState internalState) {
            THREADSTATE state = THREADSTATE.INVALID;
            switch (internalState) { 
                case CorDebugThreadState.THREAD_RUN:
                    state = THREADSTATE.RUNNING;
                    break;
                case CorDebugThreadState.THREAD_SUSPEND:
                    state = THREADSTATE.SUSPENDED;
                    break;
                default:
                    state = THREADSTATE.INVALID;
                    break;
            }
            return state;
        }
    }
   public enum ASSEMBLYACTION { 
        LOAD,
        UNLOAD
    }
    public struct ASSEMBLY {
        public string Name;
        public string Path;
        public bool Bitness;
        public ASSEMBLYACTION Activity;
        public DateTime LoadedTime;
        public DateTime UnloadedTime;
    }

 

    public struct PROCESS {
        public uint processid;
        public bool terminated;
        public int exitCode;
    }

    public class PARAMETER{
        public string type;
        public string name;
        public bool isComplex;
        public bool isNull;
        public bool isArray;
    }
    public class DEBUGPARAM : PARAMETER
    {
        //public CorElementType corType;
        public MDbgValue corValue;
        internal bool isProperty;
        public bool isIndexProperty;
        internal CorFunction property;
        public int? indexValue;
        public bool inValid;
    }
    public struct METHODSig {
        public bool isStatic;
        public bool isGeneric;
        public string name;
        public string retuntype;
        List<PARAMETER> parameters;
    }
    public sealed class IntPtrSq {
        
        IntPtr  _pointer;
        int    _offSet;
        public IntPtrSq(IntPtr pointer) {
            _pointer = pointer;
            _offSet = 0;
        }
        public Byte ReadByte() {
            lock (this) {
                Byte _byte = Marshal.ReadByte(_pointer, _offSet);
                _offSet++;
                return _byte;
            }            
        }
        //public Byte[] ReadBytes() {
        //    lock (this)
        //    {
        //        Byte _byte = Marshal.(_pointer, _offSet);
        //        _offSet++;
        //        return _byte;
        //    } 
        //}
    }
    public struct BREAKPOINT {
        public string frame;
        public uint Threadid;
    }
    public struct VARIABLE {
        internal bool isComplex;
        internal IList<PARAMETER> parameters;
        internal string innerValue;
        internal bool isArray;
        public bool IsArray {
            get { return isArray; }
        }
        public bool IsComplex {
            get {
                return isComplex;
            }
        }

        public IList<PARAMETER> Memebers {
            get {
                return parameters;
            }
        }

        public string Value {
            get {
                return innerValue;
            }
        }
    }
}
