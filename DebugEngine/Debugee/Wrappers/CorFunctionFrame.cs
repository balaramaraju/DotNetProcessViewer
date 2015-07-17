using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DebugEngine.Interfaces;
using System.Diagnostics;
using DebugEngine.Utilities;
using System.Collections;
using System.Runtime.InteropServices;

namespace DebugEngine.Debugee.Wrappers
{
    public enum CorFrameType
    {
        ILFrame, NativeFrame, InternalFrame,
        RuntimeUnwindableFrame
    }
    public class CorFunctionFrame
    {
        internal ICorDebugFrame frame;
        CorFunction function;
        public string Description {
            get {
                //function.
                return "";
            }
        }
        public CorFunctionFrame Calleer {
            get {
                ICorDebugFrame caller = null;
                frame.GetCaller(out caller);
                return (caller == null) ? null : new CorFunctionFrame(caller);
            }
        }
        public CorFunction Function {
            get {
                if (function == null) {
                    try
                    {
                        ICorDebugFunction ifunction = null;
                        frame.GetFunction(out ifunction);
                        function = new CorFunction(ifunction);
                    }
                    catch (System.Runtime.InteropServices.COMException e)
                    {
                        if (e.ErrorCode == (int)HResult.CORDBG_E_CODE_NOT_AVAILABLE)
                        {
                            return null;
                        }
                        else
                        {
                            throw;
                        }
                    }
                    catch {
                        return null;
                    }
                }
                return function;
            }
        }

        public IList<PARAMETER> GetParamInfo() {
            ICorDebugILFrame ilFrame = frame as ICorDebugILFrame;
            IList<PARAMETER> args = null;
            if (ilFrame != null){
                ICorDebugValue value = null;
                ICorDebugValueEnum values = null;
                ilFrame.EnumerateArguments(out values);
                uint val = 0;
                values.GetCount(out val);
                if (val > 0) {
                    args = new List<PARAMETER>();
                }
                for (uint index = 0; index < val; index++){
                    try{
                        ilFrame.GetArgument(index, out value);
                        MDbgValue mdgbVal = new MDbgValue(new CorValue(value));
                        DEBUGPARAM param = new DEBUGPARAM();
                        param.name = mdgbVal.Name;
                        param.type = mdgbVal.TypeName;
                        param.isComplex = mdgbVal.IsComplexType;
                        param.isNull = mdgbVal.IsNull;
                        param.corValue = mdgbVal;
                        args.Add(param);
                    } catch (COMException e){
                        if ((uint)e.ErrorCode == 0x80131304)
                        {
                            DEBUGPARAM degParam = new DEBUGPARAM();
                            degParam.corValue = null;
                            degParam.inValid = true;
                            //degParam.corType = CorElementType.;
                            args.Add(degParam);
                            //return null;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
            return args;
        }
        public string GetParamArguments() {
            StringBuilder sb = new StringBuilder();
            ICorDebugILFrame ilFrame = frame as ICorDebugILFrame;
            if (ilFrame != null) { 
                ICorDebugValue value = null;
                ICorDebugValueEnum values = null;
                ilFrame.EnumerateArguments(out values);
                uint val = 0;
                values.GetCount(out val);
                
                for (uint index = 0; index < val; index++ ){
                    try {
                        ilFrame.GetArgument(index, out value);
                        sb.Append("Parameter " + (index + 1) + " : ");
                        sb.AppendLine(CorValue2Text.GetCorValue2Text(value,0));
                    } catch (COMException e) {
                        if ((uint)e.ErrorCode == 0x80131304) {
                            return null;
                        }
				    throw;
			        }
                }
               // ilFrame.GetArgument(1, out value);
                
                
            }
            return  sb.ToString();;

        }
        public CorFunctionFrame(ICorDebugFrame frm) {
            frame = frm;
        }
        public CorFrameType FrameType
        {
            get
            {
                ICorDebugILFrame ilframe = frame as ICorDebugILFrame; ;
                if (ilframe != null)
                    return CorFrameType.ILFrame;

                ICorDebugInternalFrame iframe = frame as ICorDebugInternalFrame;
                if (iframe != null)
                    return CorFrameType.InternalFrame;

                ICorDebugRuntimeUnwindableFrame ruf = frame as ICorDebugRuntimeUnwindableFrame;
                if (ruf != null)
                    return CorFrameType.RuntimeUnwindableFrame;
                return CorFrameType.NativeFrame;
            }
        }
        /// <summary>
        /// Gets the IP.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="mappingResult">The mapping result.</param>
        public void GetIP(out UInt32 offset, out CorDebugMappingResult mappingResult)
        {
            ICorDebugILFrame ilframe = frame as ICorDebugILFrame;
            if (ilframe == null)
            {
                offset = 0;
                mappingResult = CorDebugMappingResult.MAPPING_NO_INFO;
            }
            else
            {
                ilframe.GetIP(out offset, out mappingResult);
            }
        }
       
    }
}
