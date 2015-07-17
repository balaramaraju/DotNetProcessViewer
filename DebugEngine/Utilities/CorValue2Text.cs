using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DebugEngine.Interfaces;
using DebugEngine.Debugee.Wrappers;
using DebugEngine.MetaDataUtils;
using System.Reflection;
using System.Runtime.InteropServices;
using DebugEngine.Debugee;

namespace DebugEngine.Utilities
{
    class CorValue2Text
    {
        
        public static DEBUGPARAM GetParamInfo(ICorDebugValue value) {
            DEBUGPARAM debugParam = new DEBUGPARAM();
            ICorDebugGenericValue gvalue = value as ICorDebugGenericValue;
            CorElementType type = CorElementType.ELEMENT_TYPE_VOID;
            value.GetType(out type);
            //debugParam.corType = type;
            debugParam.corValue = new MDbgValue(new CorValue(value));
            debugParam.isComplex = false;
            if (gvalue != null){
                switch (type){
                    case CorElementType.ELEMENT_TYPE_BOOLEAN:debugParam.type = "bool";break;
                    case CorElementType.ELEMENT_TYPE_CHAR: debugParam.type = "char";break;
                    case CorElementType.ELEMENT_TYPE_I1: debugParam.type = "sbyte";break;
                    case CorElementType.ELEMENT_TYPE_U1:debugParam.type = "byte";break;
                    case CorElementType.ELEMENT_TYPE_I2: debugParam.type = "Int16";break;
                    case CorElementType.ELEMENT_TYPE_U2: debugParam.type = "UInt16";break;
                    case CorElementType.ELEMENT_TYPE_I4: debugParam.type = "Int32";break;
                    case CorElementType.ELEMENT_TYPE_U4: debugParam.type = "UInt32";break;
                    case CorElementType.ELEMENT_TYPE_I8: debugParam.type = "Int64";break;
                    case CorElementType.ELEMENT_TYPE_U8: debugParam.type = "UIn64";break;
                    case CorElementType.ELEMENT_TYPE_R4: debugParam.type = "Single";break;
                    case CorElementType.ELEMENT_TYPE_R8: debugParam.type = "Double";break;
                    case CorElementType.ELEMENT_TYPE_PTR: debugParam.type = "IntPtr";break;
                    case CorElementType.ELEMENT_TYPE_U: debugParam.type = "IntPtr32";break;
                    default: debugParam.type = "***UNK***"; break;
                }
            } else if 
            (
            (type == CorElementType.ELEMENT_TYPE_CLASS) || 
            (type == CorElementType.ELEMENT_TYPE_VALUETYPE)
            ){
                ICorDebugObjectValue objectValue = value as ICorDebugObjectValue;
                ICorDebugClass _class = null;
                ICorDebugReferenceValue refVal = value as ICorDebugReferenceValue;
                ICorDebugValue pDeRef = null;
                try {
                    refVal.Dereference(out pDeRef);
                    objectValue = pDeRef as ICorDebugObjectValue;
                    objectValue.GetClass(out _class);
                    MetaType metatype = MetaDataUtils.MetadataMgr.GetClass(new CorClass(_class));
                    debugParam.type = metatype.Name;
                    debugParam.isComplex = true;
                } catch (COMException e) {
                    if ((uint)e.ErrorCode == 0x80131305){
                        debugParam.type = "Value Might be Null"; 
                    }
                }
                
            }else if (type == CorElementType.ELEMENT_TYPE_STRING){
                debugParam.type = "System.String";
            }else if(type == CorElementType.ELEMENT_TYPE_SZARRAY ||
                    type == CorElementType.ELEMENT_TYPE_ARRAY){
                    var arrayReference = value as ICorDebugReferenceValue;
                    ICorDebugValue arrayDereferenced;
                    arrayReference.Dereference(out arrayDereferenced);
                    ICorDebugArrayValue array = arrayDereferenced as ICorDebugArrayValue;
                    CorElementType arraytype = CorElementType.ELEMENT_TYPE_VOID;
                    //array.GetElementType(out 
                    ICorDebugObjectValue objectValue = arrayDereferenced as ICorDebugObjectValue;
                    ICorDebugClass _class = null;
                    objectValue.GetClass(out _class);
                    MetaType metatype = MetaDataUtils.MetadataMgr.GetClass(new CorClass(_class));
                    debugParam.type = metatype.Name;
                    debugParam.isComplex = true;
            }
            else{
                debugParam.type ="NotSupported for now";
            }
            return debugParam;
        }
        public static string GetCorValue2Text(ICorDebugValue value,int depth) {
                StringBuilder sb = new StringBuilder();
                ICorDebugGenericValue gvalue = value as ICorDebugGenericValue;
                CorElementType type = CorElementType.ELEMENT_TYPE_VOID;
                value.GetType(out type);
                if (gvalue != null)
                {
                    uint size = 0;
                    gvalue.GetSize(out size);
                    unsafe
                    {
                        byte[] corValue = new byte[size];
                        fixed (byte* pValue = corValue)
                        {
                            gvalue.GetValue(new IntPtr(pValue));
                            switch (type)
                            {
                                case CorElementType.ELEMENT_TYPE_BOOLEAN: sb.Append((*((System.Boolean*)pValue)).ToString()); break;
                                case CorElementType.ELEMENT_TYPE_CHAR: sb.Append((*((System.Char*)pValue)).ToString()); break;
                                case CorElementType.ELEMENT_TYPE_I1: sb.Append((*((System.SByte*)pValue)).ToString()); break;
                                case CorElementType.ELEMENT_TYPE_U1: sb.Append((*((System.Byte*)pValue)).ToString()); break;
                                case CorElementType.ELEMENT_TYPE_I2: sb.Append((*((System.Int16*)pValue)).ToString()); break;
                                case CorElementType.ELEMENT_TYPE_U2: sb.Append((*((System.UInt16*)pValue)).ToString()); break;
                                case CorElementType.ELEMENT_TYPE_I4: sb.Append((*((System.Int32*)pValue)).ToString()); break;
                                case CorElementType.ELEMENT_TYPE_U4: sb.Append((*((System.UInt32*)pValue)).ToString()); break;
                                case CorElementType.ELEMENT_TYPE_I8: sb.Append((*((System.Int64*)pValue)).ToString()); break;
                                case CorElementType.ELEMENT_TYPE_U8: sb.Append((*((System.UInt64*)pValue)).ToString()); break;
                                case CorElementType.ELEMENT_TYPE_R4: sb.Append((*((System.Single*)pValue)).ToString()); break;
                                case CorElementType.ELEMENT_TYPE_R8: sb.Append((*((System.Double*)pValue)).ToString()); break;
                                case CorElementType.ELEMENT_TYPE_PTR: sb.Append((*((System.IntPtr*)pValue)).ToString()); break;
                                case CorElementType.ELEMENT_TYPE_U: sb.Append((*((System.UIntPtr*)pValue)).ToString()); break;

                                default: sb.Append("Type is not known"); break;
                            }
                        }
                    }
                }
                else if((type == CorElementType.ELEMENT_TYPE_CLASS) || (type == CorElementType.ELEMENT_TYPE_VALUETYPE)){
                    ICorDebugObjectValue objectValue =  value as ICorDebugObjectValue;
                    if (objectValue == null)
                    {
                        ICorDebugReferenceValue refVal = value as ICorDebugReferenceValue;
                        ICorDebugValue pDeRef = null;
                        try
                        {
                            refVal.Dereference(out pDeRef);
                            objectValue = pDeRef as ICorDebugObjectValue;
                        }
                        catch (COMException e) {
                            if ((uint)e.ErrorCode == 0x80131305) {
                                sb.AppendLine("null");
                            }
                        }
                        
                    }
                    if (objectValue != null) {
                        ICorDebugClass _class = null;
                        objectValue.GetClass(out _class);
                        MetaType metatype = MetaDataUtils.MetadataMgr.GetClass(new CorClass(_class));
                        sb.AppendLine("Type <" + metatype.Name + "> Begin");
                        List<FieldInfo> metadata = metatype.GetFieldInfo();
                        foreach (FieldInfo fi in metadata) {
                            ICorDebugValue exceptVal = null;
                            try{   
                                objectValue.GetFieldValue(_class, (uint)fi.MetadataToken, out exceptVal);
                                DEBUGPARAM member = GetParamInfo(exceptVal);
                                member.name = fi.Name;
                            } catch (COMException e) {
                                //Need log
                                sb.AppendLine("Value : Not able to deduce");
                            }
                        }
                        sb.AppendLine("Type <" + metatype.Name + "> End");
                    }
                }
                else if (type == CorElementType.ELEMENT_TYPE_STRING)
                {
                    ICorDebugReferenceValue refVal2 = value as ICorDebugReferenceValue;
                    ICorDebugValue pDeRef2 = null;
                    refVal2.Dereference(out pDeRef2);
                    ICorDebugStringValue _msgString = pDeRef2 as ICorDebugStringValue;
                    uint stringSize;
                    uint length;
                    _msgString.GetLength(out length);
                    sb = new StringBuilder((int)length + 1); // we need one extra char for null
                    _msgString.GetString((uint)sb.Capacity, out stringSize, sb);
                }
                else {
                    sb.AppendLine("NotSupported for now");
                }
                return sb.ToString();
              
            }
        
        public static VARIABLE GetValue(ICorDebugValue value,CorElementType type) {
            if (type == CorElementType.ELEMENT_TYPE_VOID) {
                value.GetType(out type);
            }
            VARIABLE vari = new VARIABLE();
            ICorDebugGenericValue generic = value as ICorDebugGenericValue;
            if (generic != null) {
                vari.innerValue = GetGenericValue(generic,type);
                vari.isComplex = false;
            } else {
                switch (type) { 
                    case CorElementType.ELEMENT_TYPE_CLASS:
                    case CorElementType.ELEMENT_TYPE_VALUETYPE:
                        vari.isComplex = true;
                        vari.isArray = false;
                        vari.parameters = GetObjectMembers(value);
                        break;
                    case CorElementType.ELEMENT_TYPE_STRING:
                        vari= GetStringValue(value);
                        break;
                    case CorElementType.ELEMENT_TYPE_SZARRAY:
                    case CorElementType.ELEMENT_TYPE_ARRAY:
                        vari = GetArrayItems(value);
                        break;
                    default: 
                        break;
                }

            }
            return vari;
            
        }
        private static string GetGenericValue(ICorDebugGenericValue gvalue, CorElementType type) {
            uint size = 0;
            StringBuilder sb = new StringBuilder();
            gvalue.GetSize(out size);
            unsafe {
            byte[] corValue = new byte[size];
            fixed (byte* pValue = corValue) {
                gvalue.GetValue(new IntPtr(pValue));
                switch (type){
                    case CorElementType.ELEMENT_TYPE_BOOLEAN: sb.Append((*((System.Boolean*)pValue)).ToString()); break;
                    case CorElementType.ELEMENT_TYPE_CHAR: sb.Append((*((System.Char*)pValue)).ToString()); break;
                    case CorElementType.ELEMENT_TYPE_I1: sb.Append((*((System.SByte*)pValue)).ToString()); break;
                    case CorElementType.ELEMENT_TYPE_U1: sb.Append((*((System.Byte*)pValue)).ToString()); break;
                    case CorElementType.ELEMENT_TYPE_I2: sb.Append((*((System.Int16*)pValue)).ToString()); break;
                    case CorElementType.ELEMENT_TYPE_U2: sb.Append((*((System.UInt16*)pValue)).ToString()); break;
                    case CorElementType.ELEMENT_TYPE_I4: sb.Append((*((System.Int32*)pValue)).ToString()); break;
                    case CorElementType.ELEMENT_TYPE_U4: sb.Append((*((System.UInt32*)pValue)).ToString()); break;
                    case CorElementType.ELEMENT_TYPE_I8: sb.Append((*((System.Int64*)pValue)).ToString()); break;
                    case CorElementType.ELEMENT_TYPE_U8: sb.Append((*((System.UInt64*)pValue)).ToString()); break;
                    case CorElementType.ELEMENT_TYPE_R4: sb.Append((*((System.Single*)pValue)).ToString()); break;
                    case CorElementType.ELEMENT_TYPE_R8: sb.Append((*((System.Double*)pValue)).ToString()); break;
                    case CorElementType.ELEMENT_TYPE_PTR: sb.Append((*((System.IntPtr*)pValue)).ToString()); break;
                    case CorElementType.ELEMENT_TYPE_U: sb.Append((*((System.UIntPtr*)pValue)).ToString()); break;
                    default: sb.Append("Type is not known"); break;
                }
            }
            }
            return sb.ToString();
        }
        internal static IList<PARAMETER> GetObjectMembers(ICorDebugValue value){
            IList<PARAMETER> members = new List<PARAMETER>();   
            ICorDebugObjectValue objectValue = value as ICorDebugObjectValue;
            //incase of boxed senarion.
            if (objectValue == null){
                ICorDebugReferenceValue refVal = value as ICorDebugReferenceValue;
                ICorDebugValue pDeRef = null;
                try{
                    refVal.Dereference(out pDeRef);
                    objectValue = pDeRef as ICorDebugObjectValue;
                } catch (COMException e){
                    if ((uint)e.ErrorCode == 0x80131305)  return null;
                }
            }
            if (objectValue != null){
                ICorDebugClass _class = null;
                objectValue.GetClass(out _class);
                MetaType metatype = MetaDataUtils.MetadataMgr.GetClass(new CorClass(_class));
                List<FieldInfo> metadata = metatype.GetFieldInfo();
                foreach (FieldInfo fi in metadata) {
                    DEBUGPARAM arg = new DEBUGPARAM();
                    ICorDebugValue exceptVal = null;
                    try{   
                        objectValue.GetFieldValue(_class, (uint)fi.MetadataToken, out exceptVal);
                        arg = GetParamInfo(exceptVal);
                        arg.name = fi.Name;
                        members.Add(arg);
                    }catch (COMException) {
                        //sb.AppendLine("Value : Not able to deduce");
                    }
                }
                List<MetadataMethodInfo> metas = metatype.GetProperties();
                ICorDebugModule module = null;
                _class.GetModule(out module);
                foreach (MetadataMethodInfo fi in metas) {
                    DEBUGPARAM arg = new DEBUGPARAM();
                    try
                    {
                        //objectValue.GetFieldValue(_class, (uint)fi.MetadataToken, out exceptVal);
                        //arg = GetParamInfo(exceptVal);
                        arg.name = fi.Name;
                        arg.isProperty = true;
                        arg.type = fi.ReturnType;
                        if (!fi.IsStatic) {
                            ICorDebugReferenceValue hv = objectValue as ICorDebugReferenceValue;
                            if (hv != null){
                               // arg.corValue = hv;
                            }else{
                                //arg.corValue = value;
                            }
                        }
                        if (arg.name.Contains(".get_Item")) {
                            if (fi.Arguments.Count == 1 && fi.Arguments[0].name == "index") {
                                arg.isIndexProperty = true;
                            }
                        }
                        ICorDebugFunction fun = null;
                        module.GetFunctionFromToken((uint)fi.MetadataToken, out fun);
                        arg.property = new CorFunction(fun);
                        members.Add(arg);
                    }
                    catch (COMException)
                    {
                        //sb.AppendLine("Value : Not able to deduce");
                    }
                }
            }
            return members;
        }
        private static VARIABLE GetArrayItems(ICorDebugValue value) {
            VARIABLE vari = new VARIABLE();
            vari.isArray = true;
            vari.isComplex = false;
            var arrayReference = value as ICorDebugReferenceValue;
            ICorDebugValue arrayDereferenced;
            try{
                arrayReference.Dereference(out arrayDereferenced);
                ICorDebugArrayValue array = arrayDereferenced as ICorDebugArrayValue;
                IList<PARAMETER> members = new List<PARAMETER>();
                if (array != null) { 
                    uint noOfItems = 0;
                    array.GetCount(out noOfItems);
                    if (noOfItems == 0) vari.innerValue = "<<Zero Elements in this array>>";
                    else{
                    for (int index = 0; index < noOfItems; index++) {
                        ICorDebugValue elementVal = null;
                        array.GetElementAtPosition((uint)index, out elementVal);
                        MDbgValue mdgbVal = new MDbgValue(new CorValue(value));
                        DEBUGPARAM parm = GetParamInfo(elementVal);
                        parm.name = "Item[" + index.ToString() + "]";
                        members.Add(parm);
                    }
                    vari.parameters = members;
                    }
                }
            }
            catch (Exception) {
                vari.innerValue = "<<Not able to get the value>>";
            }
            return vari;
            
        }
        private static VARIABLE GetStringValue(ICorDebugValue value) {
            VARIABLE vari = new VARIABLE();
            ICorDebugReferenceValue refVal2 = value as ICorDebugReferenceValue;
            ICorDebugValue pDeRef2 = null;
            try{
                refVal2.Dereference(out pDeRef2);
                ICorDebugStringValue _msgString = pDeRef2 as ICorDebugStringValue;
                uint stringSize;
                uint length;
                _msgString.GetLength(out length);
                StringBuilder sb = new StringBuilder((int)length + 1); // we need one extra char for null
                _msgString.GetString((uint)sb.Capacity, out stringSize, sb);
                vari.innerValue = sb.ToString();
            }  catch (COMException e) {
                if ((uint)e.ErrorCode == 0x80131305)
                {
                    vari.innerValue = "<<Value Might be Null>>";
                }
                else {
                    vari.innerValue = "<<Error While getting value >> " + e.Message;
                }
            }
            
            vari.isArray = false;
            vari.isComplex = false;
            return vari;
        }
    }
}
