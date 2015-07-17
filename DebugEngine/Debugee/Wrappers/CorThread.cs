using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DebugEngine.Interfaces;
using DebugEngine.MetaDataUtils;
using System.Reflection;
using DebugEngine.Utilities;
using System.Threading;

namespace DebugEngine.Debugee.Wrappers
{
    public class CorDebugThread {
        ICorDebugThread m_corThread;
        public UInt32 ID { get; private set; }
        public string StackFailureReason { get; private set; }
        public CorDebugThread(ICorDebugThread _thread){
            m_corThread = _thread;
            uint id;
            m_corThread.GetID(out id);
            ID = id;
        }
        public CorFunctionFrame ActiveFrame {
            get {
                ICorDebugFrame threadFrame = null;
                m_corThread.GetActiveFrame(out threadFrame);
                return new CorFunctionFrame(threadFrame);
            }
        }
        public IList<CorFunctionFrame> CallStack {
            get
            {
                List<CorFunctionFrame> frames = new List<CorFunctionFrame>();
                ICorDebugFrame threadFrame = null;
                m_corThread.GetActiveFrame(out threadFrame);
                if (threadFrame != null){
                    frames.AddRange(CreateFrames(threadFrame));
                }
                
                if(frames.Count ==0 ){
                    ICorDebugChain ichain = null;
                    ICorDebugChain prevChain = null;
                    m_corThread.GetActiveChain(out ichain);
                    if (ichain != null){
                        int isManaged = 0;
                        do {
                            prevChain = ichain;
                            ichain.IsManaged(out isManaged);
                            if (isManaged != 0)
                            {
                                break;
                            }
                            ichain.GetCaller(out ichain);
                        } while (ichain != null);
                        if (isManaged != 0){
                            ichain.GetActiveFrame(out threadFrame);
                            if (threadFrame != null)
                            {
                                frames.AddRange(CreateFrames(threadFrame));
                            }

                        }
                    }

                    if (frames.Count == 0)
                    {
                        CorDebugChainReason reason;

                        // Get & print chain's reason
                        prevChain.GetReason(out reason);

                        switch (reason){
                            case CorDebugChainReason.CHAIN_PROCESS_START:
                            case CorDebugChainReason.CHAIN_THREAD_START:
                                StackFailureReason = "Thread Start..";
                                break;

                            case CorDebugChainReason.CHAIN_ENTER_MANAGED:
                                StackFailureReason = "Managed transition";
                                break;

                            case CorDebugChainReason.CHAIN_ENTER_UNMANAGED:
                                StackFailureReason = "Unmanaged transition";
                                break;

                            case CorDebugChainReason.CHAIN_CLASS_INIT:
                                StackFailureReason = "Class initialization";
                                break;

                            case CorDebugChainReason.CHAIN_DEBUGGER_EVAL:
                                StackFailureReason = "Debugger evaluation";
                                break;

                            case CorDebugChainReason.CHAIN_EXCEPTION_FILTER:
                                StackFailureReason = "Exception filter";
                                break;

                            case CorDebugChainReason.CHAIN_SECURITY:
                                StackFailureReason = "Security";
                                break;

                            case CorDebugChainReason.CHAIN_CONTEXT_POLICY:
                                StackFailureReason = "Context policy";
                                break;

                            case CorDebugChainReason.CHAIN_CONTEXT_SWITCH:
                                StackFailureReason = "Context switch";
                                break;

                            case CorDebugChainReason.CHAIN_INTERCEPTION:
                                StackFailureReason = "Interception";
                                break;

                            case CorDebugChainReason.CHAIN_FUNC_EVAL:
                                StackFailureReason = "Function Evaluation";
                                break;


                        }
                    }
                }

                return frames;
            }
        }
        private List<CorFunctionFrame> CreateFrames(ICorDebugFrame frame)
        {
            List<CorFunctionFrame> frames = new List<CorFunctionFrame>();
            if (frame is ICorDebugILFrame){
                frames.Add(new CorFunctionFrame(frame));
            } else if (frame is ICorDebugNativeFrame) {
                //((ICorDebugNativeFrame)frame).
            }
            ICorDebugFrame callerframe = null;
            frame.GetCaller(out callerframe);
            if (callerframe != null){
                frames.AddRange(CreateFrames(callerframe));
            }
            return frames;
        }
    }
    public class CorThread
    {
        ICorDebugThread corThread;
        uint id;
        public string StackFailureReason;
        public UInt32 ID {
            get {
                return id;
            }
        }
        public CorDebugThreadState State{
            get{
                CorDebugThreadState cor;
                corThread.GetDebugState(out cor);
                return cor;
            }
            set {
                corThread.SetDebugState(value);
            }
        }
        //public string SourceLine {
        //    get {
        //        string source = string.Empty;
        //        corThread.GetActiveFrame(out threadFrame);
        //        return source;
        //    }
        //}

        private List<CorFunctionFrame> CreateFrames(ICorDebugFrame frame) {
            List<CorFunctionFrame> frames = new List<CorFunctionFrame>();
            if (frame is ICorDebugILFrame){
                CorFunctionFrame corFrame = new CorFunctionFrame(frame);
                frames.Add(corFrame);
            } else if (frame is ICorDebugNativeFrame) {
                //DthreadFrame.
            }
            ICorDebugFrame callerframe = null;
            frame.GetCaller(out callerframe);
            if (callerframe != null) {
              frames.AddRange(CreateFrames(callerframe));
            }
            return frames;
        }
        public CorFunctionFrame ActiveFrame {
            get {
                ICorDebugFrame threadFrame = null;
                corThread.GetActiveFrame(out threadFrame);
                return new CorFunctionFrame(threadFrame);
            }
        }
        public List<CorFunctionFrame> StackFrames {
            get {
                List<CorFunctionFrame> frames = new List<CorFunctionFrame>();
                ICorDebugFrame threadFrame = null;
                corThread.GetActiveFrame(out threadFrame);
                if (threadFrame != null) {
                    frames.AddRange(CreateFrames(threadFrame));
                }
                else {
                    ICorDebugChain ichain = null;
                    ICorDebugChain prevChain = null;
                    corThread.GetActiveChain(out ichain);
                    if (ichain != null) { 
                      int isManaged = 0;
                      do{
                          prevChain = ichain;
                          ichain.IsManaged(out isManaged);
                          if (isManaged != 0){
                            break;
                          }
                          ichain.GetCaller(out ichain);
                      }while(ichain != null);
                      if (isManaged != 0) {
                          ichain.GetActiveFrame(out threadFrame);
                          if (threadFrame != null) {
                              frames.AddRange(CreateFrames(threadFrame));
                          }
                          
                      }
                      }  
                    
                    if (frames.Count == 0) { 
                     CorDebugChainReason reason;

                    // Get & print chain's reason
                    prevChain.GetReason(out reason);

                    switch (reason)
                    {
                    case CorDebugChainReason.CHAIN_PROCESS_START:
                    case CorDebugChainReason.CHAIN_THREAD_START:
                        //StackFailureReason = "Thread Start..";
                        break;

                    case CorDebugChainReason.CHAIN_ENTER_MANAGED:
                        StackFailureReason = "Managed transition";
                        break;

                    case CorDebugChainReason.CHAIN_ENTER_UNMANAGED:
                        StackFailureReason = "Unmanaged transition";
                        break;

                    case CorDebugChainReason.CHAIN_CLASS_INIT:
                        StackFailureReason = "Class initialization";
                        break;

                    case CorDebugChainReason.CHAIN_DEBUGGER_EVAL:
                        StackFailureReason = "Debugger evaluation";
                        break;

                    case CorDebugChainReason.CHAIN_EXCEPTION_FILTER:
                        StackFailureReason = "Exception filter";
                        break;

                    case CorDebugChainReason.CHAIN_SECURITY:
                        StackFailureReason = "Security";
                        break;

                    case CorDebugChainReason.CHAIN_CONTEXT_POLICY:
                        StackFailureReason = "Context policy";
                        break;

                    case CorDebugChainReason.CHAIN_CONTEXT_SWITCH:
                        StackFailureReason = "Context switch";
                        break;

                    case CorDebugChainReason.CHAIN_INTERCEPTION:
                        StackFailureReason = "Interception";
                        break;

                    case CorDebugChainReason.CHAIN_FUNC_EVAL:
                        StackFailureReason = "Function Evaluation";
                        break;

                    
                    }
                    }
                }
                
                return frames;
            }
        }
        public List<CorFunction> ActiveFunctions {
            get {
                List<CorFunction> funs = new List<CorFunction>();
                    ICorDebugThread2 corTh2 = corThread as ICorDebugThread2;
                    uint pfCount = 0;
                    corTh2.GetActiveFunctions(0, out pfCount, null);
                    var afunctions = new COR_ACTIVE_FUNCTION[pfCount];
                    foreach (var fun in afunctions) {
                        if (fun.pFunction != null)
                        {
                            funs.Add(new CorFunction(fun.pFunction as ICorDebugFunction));
                        }
                    }
                    return funs;
            }
        }
        public string ExceptionString {
            get {
                //List<MDbgValue> al = new List<MDbgValue>();
                ICorDebugValue exception = null;
                corThread.GetCurrentException(out exception);
                StringBuilder excepStr = new StringBuilder();
                if (exception != null)
                {
                    ICorDebugReferenceValue refVal = exception as ICorDebugReferenceValue;
                    ICorDebugValue pDeRef = null;
                    refVal.Dereference(out pDeRef);
                    //exception.G
                    ICorDebugObjectValue excp = pDeRef as ICorDebugObjectValue;
                    ICorDebugClass debugObj = null;
                    excp.GetClass(out debugObj);
                    CorClass exClass = new CorClass(debugObj);
                    MetaType type = MetaDataUtils.MetadataMgr.GetClass(exClass);
                    excepStr.AppendLine("Exception Type : " + type.Name);
                    ICorDebugValue2 v2 = (ICorDebugValue2)pDeRef;
                    ICorDebugType dt;
                    v2.GetExactType(out dt);
                    ICorDebugClass c = debugObj;
                    while (true){
                        List<FieldInfo> metadata = type.GetFieldInfo();
                        foreach (FieldInfo fi in metadata) {
                            if (fi.Name.Contains("_message")) {
                                ICorDebugValue exceptVal = null;
                                excp.GetFieldValue(c, (uint)fi.MetadataToken, out exceptVal);
                                ICorDebugReferenceValue refVal2 = exceptVal as ICorDebugReferenceValue;
                                ICorDebugValue pDeRef2 = null;
                                refVal2.Dereference(out pDeRef2);
                                ICorDebugStringValue _msgString = pDeRef2 as ICorDebugStringValue;
                                uint stringSize;
                                uint length;
                                _msgString.GetLength(out length);
                                StringBuilder sb = new StringBuilder((int)length + 1); // we need one extra char for null
                                _msgString.GetString((uint)sb.Capacity, out stringSize, sb);
                                excepStr.AppendLine("__message : "+sb.ToString());
                                
                            }
                        }                       
                        dt.GetBase(out dt);
                        if(dt == null) break;
                        
                        dt.GetClass(out c);                        
                        exClass = new CorClass(c);
                        type = MetaDataUtils.MetadataMgr.GetClass(exClass);
                    }
                    //Need to get the message field token and dump the stack
                    //ICorDebugObjectValue exMessage = exception as ICorDebugObjectValue;
                    //ICorDebugValue exceptVal = null;
                    //excp.GetFieldValue(debugObj, exClass.Token,out exceptVal);
                    //ICorDebugStringValue _msgString = exceptVal as ICorDebugStringValue;
                    ////debugObj.
                    //uint stringSize;
                    //uint length;
                    //_msgString.GetLength(out length);
                    //StringBuilder sb = new StringBuilder((int)length + 1); // we need one extra char for null
                    //_msgString.GetString((uint)sb.Capacity, out stringSize, sb);
                    return excepStr.ToString();
                }
                return string.Empty;
            }
        }
        public CorThread(ICorDebugThread thread) {
            corThread = thread;
            corThread.GetID(out id);
            //corThread.G
        }
        
        public void GetPropertyValue(DEBUGPARAM property)
        {
            ICorDebugEval eVal = null;
            corThread.CreateEval(out eVal);
            uint pramsCount = 0;
            ICorDebugValue[] value = new ICorDebugValue[2];
            if (property.corValue != null)
            {
                //value[0] = property.corValue;
                pramsCount++;

                if (property.indexValue != null)
                {
                    ICorDebugValue newVal = null;
                    eVal.CreateValue(CorElementType.ELEMENT_TYPE_I4, null, out newVal);
                    ICorDebugGenericValue dGenricVal = newVal as ICorDebugGenericValue;
                    if (dGenricVal != null)
                    {
                        Byte bv = Convert.ToByte(property.indexValue);
                        unsafe
                        {
                            dGenricVal.SetValue(new IntPtr(&bv));
                        }
                    }
                    value[1] = newVal;
                    pramsCount++;
                }
                
            }
            //newVal.
            //value[0] = property.corValue;
            eVal.CallFunction(property.property.CorFun, pramsCount, value);
            
            //VARIABLE var = new VARIABLE();
            //var.
            //ICorDebugValue val = null;
            //eVal.GetResult(out val);
        }

        public void GetPropertyValue(ICorDebugFunction fn)
        {
            ICorDebugEval eVal = null;
            corThread.CreateEval(out eVal);
            ICorDebugValue[] value = new ICorDebugValue[1];
            //value[0] = property.corValue;
            eVal.CallFunction(fn, 0, value);
            //ICorDebugValue val = null;
            //eVal.GetResult(out val);
        }
    }
}
