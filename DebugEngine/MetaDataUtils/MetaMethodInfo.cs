using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DebugEngine.Interfaces;
using System.Reflection;
using System.Collections;
using System.Globalization;
using System.Runtime.Serialization;
using DebugEngine.Utilities;
using System.Runtime.InteropServices;
using DebugEngine.Debugee;
using System.Text.RegularExpressions;

namespace DebugEngine.MetaDataUtils
{
    
    
    public sealed class MetadataMethodInfo {

        bool isGeneric = false;
        string returnType = string.Empty;
        string signature = string.Empty;
        MetaType type = null;
        IList<PARAMETER> args = new List<PARAMETER>();
        public IList<PARAMETER> Arguments {
            get {
                return args;
            }
        }
        public string Signature {
            get {
                return signature;
            }
        }
        private bool hasThis = false;
        public bool IsStatic {
            get {
                return !hasThis;
            }
        }
        internal MetadataMethodInfo(IMetadataImport importer, int methodToken, MetaType type)
        {
            this.type = type;
            if (!importer.IsValidToken((uint)methodToken))
                throw new ArgumentException();

            m_importer = importer;
            m_methodToken = methodToken;

            int size;
            uint pdwAttr;
            IntPtr ppvSigBlob;
            uint pulCodeRVA, pdwImplFlags;
            uint pcbSigBlob;

            m_importer.GetMethodProps((uint)methodToken,
                                      out m_classToken,
                                      null,
                                      0,
                                      out size,
                                      out pdwAttr,
                                      out ppvSigBlob,
                                      out pcbSigBlob,
                                      out pulCodeRVA,
                                      out pdwImplFlags);

            StringBuilder szMethodName = new StringBuilder(size);
            m_importer.GetMethodProps((uint)methodToken,
                                    out m_classToken,
                                    szMethodName,
                                    szMethodName.Capacity,
                                    out size,
                                    out pdwAttr,
                                    out ppvSigBlob,
                                    out pcbSigBlob,
                                    out pulCodeRVA,
                                    out pdwImplFlags);

            m_name = szMethodName.ToString();
            m_methodAttributes = (MethodAttributes)pdwAttr;

            byte[] by = new byte[pcbSigBlob];
            Marshal.Copy(ppvSigBlob, by, 0,(int) pcbSigBlob);
           signature = GetFunctionSig(ref ppvSigBlob, (int)pcbSigBlob);
            
        }

        public string GetFunctionSig(ref IntPtr  mSig,int length) {
            ParameterInfo[] param = GetParameters();
            StringBuilder str = new StringBuilder();
            
            IntPtrSq _internalPtr = new IntPtrSq(mSig);
            // Calling convention 
            int conv = _internalPtr.ReadByte();
            hasThis = false;
            if ((conv & ((int)CorCallingConvention.HasThis)) != 0) {
                hasThis = true;
            }
            if ((conv & ((int)CorCallingConvention.Generic)) != 0) {
                uint typeParams = MetadataHelperFunctions.CorSigUncompressData(_internalPtr);
                isGeneric = true; 
            }

            int argCount = _internalPtr.ReadByte();
            returnType = GetParamType(_internalPtr); ;
            str.Append(returnType + " ");
            str.Append(Name + " ");
            str.Append("(");
            if (argCount> 0) {
                args = new List<PARAMETER>();
            }
            for (int index = 0; index < argCount; index++) {
                PARAMETER arg = new PARAMETER();
                arg.type = GetParamType(_internalPtr);
                arg.name = param[index].Name;
                str.Append(arg.type + " ");
                str.Append(arg.name);
                if (index < argCount - 1) {
                    str.Append(" , ");
                }
                args.Add(arg);
            }
            if(conv == (int)CorCallingConvention.VarArg){
                str.Append(Name + "...");
            }
            str.Append(")");
            return str.ToString();
        }
        
        private string GetParamType(IntPtrSq sig) {
           
                Byte byt = sig.ReadByte();
                switch ((CorElementType)byt)
                {
                    case CorElementType.ELEMENT_TYPE_VOID: return "Void";
                    case CorElementType.ELEMENT_TYPE_BOOLEAN: return "Boolean";
                    case CorElementType.ELEMENT_TYPE_I1: return "SByte";
                    case CorElementType.ELEMENT_TYPE_U1: return "Byte";
                    case CorElementType.ELEMENT_TYPE_I2: return "Int16";
                    case CorElementType.ELEMENT_TYPE_U2: return "UInt16";
                    case CorElementType.ELEMENT_TYPE_CHAR: return "Char";
                    case CorElementType.ELEMENT_TYPE_I4: return "Int32";
                    case CorElementType.ELEMENT_TYPE_U4: return "UInt32";
                    case CorElementType.ELEMENT_TYPE_I8: return "Int64";
                    case CorElementType.ELEMENT_TYPE_U8: return "UInt64";
                    case CorElementType.ELEMENT_TYPE_R4: return "Single";
                    case CorElementType.ELEMENT_TYPE_R8: return "Double";
                    case CorElementType.ELEMENT_TYPE_OBJECT: return "Object";
                    case CorElementType.ELEMENT_TYPE_STRING: return "String";
                    case CorElementType.ELEMENT_TYPE_I: return "Int";
                    case CorElementType.ELEMENT_TYPE_U: return "UInt";

                    case CorElementType.ELEMENT_TYPE_GENERICINST:
                        {
                            return "<Unknown>";
                            //cb += AddType(&sigBlob[cb]); 
                            //DWORD n; 
                            //cb += CorSigUncompressData(&sigBlob[cb], &n); 
                            //return "<"); 
                            //for (DWORD i = 0; i < n; i++) 
                            //{ 
                            //    if (i > 0) 
                            //        return ","); 
                            //    cb += AddType(&sigBlob[cb]); 
                            //} 
                            //return ">"); 

                        }


                    case CorElementType.ELEMENT_TYPE_MVAR:
                        {
                            return "<Unknown>";
                            //int ix; 
                            //cb += CorSigUncompressData(&sigBlob[cb], &ix); 
                            //WCHAR smallbuf[20]; 
                            //wsprintf(smallbuf, "!!%d", ix); 
                            //return smallbuf); 
                        }


                    case CorElementType.ELEMENT_TYPE_VAR:
                            uint value = MetadataHelperFunctions.CorSigUncompressData(sig);
                            return " !" + value.ToString();
                    case CorElementType.ELEMENT_TYPE_VALUETYPE:
                    case CorElementType.ELEMENT_TYPE_CLASS:
                            uint tk;
                            tk = MetadataHelperFunctions.CorSigUncompressToken(sig);
                            MetaType retType = new MetaType(m_importer, (int)tk);
                            string validCeck = retType.Name;
                            if (!Regex.IsMatch(validCeck, @"^[a-zA-Z0-9_.]+$")) {
                                validCeck = "**UNK**";
                            }
                            return validCeck;

                    case CorElementType.ELEMENT_TYPE_TYPEDBYREF:
                            return "TypedReference";
                    case CorElementType.ELEMENT_TYPE_BYREF: 
                        return " ByRef" + GetParamType(sig); 
                    case CorElementType.ELEMENT_TYPE_SZARRAY:
                        return GetParamType(sig)+"[]";
                    case CorElementType.ELEMENT_TYPE_ARRAY:        // General Array 
                        {
                            StringBuilder type = new StringBuilder();
                            type.Append(GetParamType(sig));
                            type.Append("[");

                            // Skip over rank 
                            uint rank = MetadataHelperFunctions.CorSigUncompressData(sig); 

                            if (rank > 0) { 
                                // how many sizes? 
                                uint sizes =  MetadataHelperFunctions.CorSigUncompressData(sig);
                            
                                // read out all the sizes 
                                for (uint i = 0; i < sizes; i++) { 
                                    uint dimSize =  MetadataHelperFunctions.CorSigUncompressData(sig);
                                    
                                    if (i > 0) 
                                        type.Append(","); 
                                } 

                                // how many lower bounds? 
                                uint lowers =  MetadataHelperFunctions.CorSigUncompressData(sig);
                                // read out all the lower bounds. 
                                for (uint index = 0; index < lowers; index++){ 
                                    uint lowerBound =  MetadataHelperFunctions.CorSigUncompressData(sig); 
                                } 
                            } 

                            type.Append("]");
                            return type.ToString();
                        }


                    case CorElementType.ELEMENT_TYPE_PTR: 
                            return GetParamType(sig) + "*";
                    case CorElementType.ELEMENT_TYPE_CMOD_REQD: 
                            return "CMOD_REQD" + GetParamType(sig); 
                    case CorElementType.ELEMENT_TYPE_CMOD_OPT:
                            return "CMOD_OPT " + GetParamType(sig);
                    case CorElementType.ELEMENT_TYPE_MODIFIER:
                            return GetParamType(sig);
                    case CorElementType.ELEMENT_TYPE_PINNED: 
                            return "pinned " + GetParamType(sig); 
                    case CorElementType.ELEMENT_TYPE_SENTINEL:
                            return string.Empty; 
                    default:
                        return "**UNKNOWN TYPE**";
                }
            
        }

        public string ReturnType {
            get{
                return returnType;
            }
        }

        /// <summary>
        /// Type that the method is declared in.
        /// </summary>
        //public override Type DeclaringType
        //{
        //    get
        //    {
        //        if (TokenUtils.IsNullToken(m_classToken))
        //            return null;                            // this is method outside of class

        //        return new MetaType(m_importer, m_classToken);
        //    }
        //}

        //internal MetaType TypeDef {
        //    get {
        //        if (TokenUtils.IsNullToken(m_classToken))
        //            return null;                            // this is method outside of class

        //        return new MetaType(m_importer, m_classToken);
        //    }
        //}
        /// <summary>
        /// Name of the Method, not including the class name.
        /// </summary>
        public string Name
        {
            get
            {
                return type.Name+"."+m_name;
            }
        }



        public System.Reflection.ParameterInfo[] GetParameters()
        {
            ArrayList al = new ArrayList();
            IntPtr hEnum = new IntPtr();
            try
            {
                while (true)
                {
                    uint count;
                    int paramToken;
                    m_importer.EnumParams(ref hEnum,
                                          m_methodToken, out paramToken, 1, out count);
                    if (count != 1)
                        break;

                    al.Add(new MetadataParameterInfo(m_importer, paramToken,
                                                     null, string.Empty));
                }
            }
            finally
            {
                m_importer.CloseEnum(hEnum);
            }
            return (ParameterInfo[])al.ToArray(typeof(ParameterInfo));
        }

        public bool IsGenericMethod {
            get {
                return isGeneric;
            }
        }
        /// <summary>
        /// Metadata token for this method. Be sure to use this in the proper scope.
        /// </summary>
        public int MetadataToken
        {
            get
            {
                return m_methodToken;
            }
        }

        


        

        private IMetadataImport m_importer;
        private string m_name;
        private int m_classToken;
        private int m_methodToken;
        private MethodAttributes m_methodAttributes;
    }
    public sealed class MetadataParameterInfo : ParameterInfo
    {
        internal MetadataParameterInfo(IMetadataImport importer, int paramToken,
                                       MemberInfo memberImpl, string typeImpl)
        {
            int parentToken;
            uint pulSequence, pdwAttr, pdwCPlusTypeFlag, pcchValue, size;

            IntPtr ppValue;
            importer.GetParamProps(paramToken,
                                   out parentToken,
                                   out pulSequence,
                                   null,
                                   0,
                                   out size,
                                   out pdwAttr,
                                   out pdwCPlusTypeFlag,
                                   out ppValue,
                                   out pcchValue
                                   );
            StringBuilder szName = new StringBuilder((int)size);
            importer.GetParamProps(paramToken,
                                   out parentToken,
                                   out pulSequence,
                                   szName,
                                   (uint)szName.Capacity,
                                   out size,
                                   out pdwAttr,
                                   out pdwCPlusTypeFlag,
                                   out ppValue,
                                   out pcchValue
                                   );
            NameImpl = typeImpl + szName.ToString();
            //ClassImpl = typeImpl;
            PositionImpl = (int)pulSequence;
            AttrsImpl = (ParameterAttributes)pdwAttr;

            MemberImpl = memberImpl;
        }

        private MetadataParameterInfo(SerializationInfo info, StreamingContext context)
        {

        }

        public override String Name
        {
            get
            {
                return NameImpl;
            }
        }

        public override int Position
        {
            get
            {
                return PositionImpl;
            }
        }
    }

    
}
