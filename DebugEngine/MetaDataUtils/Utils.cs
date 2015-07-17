using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DebugEngine.Interfaces;
using System.Diagnostics;
using DebugEngine.Debugee;

namespace DebugEngine.MetaDataUtils
{
    static class MetadataHelperFunctions
    {
        private static uint TokenFromRid(uint rid, uint tktype) { return (rid) | (tktype); }

        // The below have been translated manually from the inline C++ helpers in cor.h            
        internal static uint CorSigUncompressBigData(
            ref IntPtr pData)             // [IN,OUT] compressed data 
        {
            unsafe
            {
                byte* pBytes = (byte*)pData;
                uint res;

                // 1 byte data is handled in CorSigUncompressData   
                //  Debug.Assert(*pBytes & 0x80);    

                // Medium.  
                if ((*pBytes & 0xC0) == 0x80)  // 10?? ????  
                {
                    res = (uint)((*pBytes++ & 0x3f) << 8);
                    res |= *pBytes++;
                }
                else // 110? ???? 
                {
                    res = (uint)(*pBytes++ & 0x1f) << 24;
                    res |= (uint)(*pBytes++) << 16;
                    res |= (uint)(*pBytes++) << 8;
                    res |= (uint)(*pBytes++);
                }

                pData = (IntPtr)pBytes;
                return res;
            }
        }

        internal static uint CorSigUncompressData(
            ref IntPtr pData)             // [IN,OUT] compressed data 
        {
            unsafe
            {
                byte* pBytes = (byte*)pData;

                // Handle smallest data inline. 
                if ((*pBytes & 0x80) == 0x00)        // 0??? ????    
                {
                    uint retval = (uint)(*pBytes++);
                    pData = (IntPtr)pBytes;
                    return retval;
                }
                return CorSigUncompressBigData(ref pData);
            }
        }

       
        static uint[] g_tkCorEncodeToken = { (uint)MetadataTokenType.TypeDef, (uint)MetadataTokenType.TypeRef, (uint)MetadataTokenType.TypeSpec, (uint)MetadataTokenType.BaseType };

        // uncompress a token
        internal static uint CorSigUncompressToken(   // return the token.    
            ref IntPtr pData)             // [IN,OUT] compressed data 
        {
            uint tk;
            uint tkType;

            tk = CorSigUncompressData(ref pData);
            tkType = g_tkCorEncodeToken[tk & 0x3];
            tk = TokenFromRid(tk >> 2, tkType);
            return tk;
        }

        internal static uint CorSigUncompressToken(   // return the token.    
             IntPtrSq pData)             // [IN,OUT] compressed data 
        {
            uint tk;
            uint tkType;

            tk = CorSigUncompressData(pData);
            tkType = g_tkCorEncodeToken[tk & 0x3];
            tk = TokenFromRid(tk >> 2, tkType);
            return tk;
        }

        internal static uint CorSigUncompressData(IntPtrSq pData){
                Byte pBytes = pData.ReadByte(); ;
                uint retval =0;
                // Handle smallest data inline. 
                if ((pBytes & 0x80) == 0x00) {
                    retval = (uint)(pBytes);
                }else if ((pBytes & 0xC0) == 0x80) {
                    retval = (uint)((pBytes & 0x3f) << 8);
                    retval |= pData.ReadByte();
                } else {
                    retval = (uint)(pBytes & 0x1f) << 24;
                    retval |= (uint)(pData.ReadByte()) << 16;
                    retval |= (uint)(pData.ReadByte()) << 8;
                    retval |= (uint)(pData.ReadByte());
                }

                return retval;

        }


        internal static CorCallingConvention CorSigUncompressCallingConv(
            ref IntPtr pData)             // [IN,OUT] compressed data 
        {
            unsafe
            {
                byte* pBytes = (byte*)pData;
                CorCallingConvention retval = (CorCallingConvention)(*pBytes++);
                pData = (IntPtr)pBytes;
                return retval;
            }
        }

 

        // uncompress encoded element type
        internal static CorElementType CorSigUncompressElementType(//Element type
            ref IntPtr pData)             // [IN,OUT] compressed data 
        {
            unsafe
            {
                byte* pBytes = (byte*)pData;

                CorElementType retval = (CorElementType)(*pBytes++);
                pData = (IntPtr)pBytes;
                return retval;
            }
        }

        // Function translated directly from cor.h but never tested; included here in case someone wants to use it in future
        /*        internal static uint CorSigUncompressElementType(// return number of bytes of that compressed data occupied in pData
                    IntPtr pData,              // [IN] compressed data 
                    out CorElementType pElementType)       // [OUT] the expanded *pData    
                {  
                    unsafe
                    {
                        byte *pBytes = (byte*)pData;
                        pElementType = (CorElementType)(*pBytes & 0x7f);    
                        return 1;   
                    }
                }*/

        static internal string[] GetGenericArgumentNames(IMetadataImport importer,
                                                int typeOrMethodToken)
        {
            IMetadataImport2 importer2 = (importer as IMetadataImport2);
            if (importer2 == null)
                return new string[0]; // this means we're pre v2.0 debuggees.

            Debug.Assert(importer2 != null);

            string[] genargs = null;

            IntPtr hEnum = IntPtr.Zero;
            try
            {
                int i = 0;
                do
                {
                    uint nOut;
                    int genTypeToken;
                    importer2.EnumGenericParams(ref hEnum, typeOrMethodToken,
                                                out genTypeToken, 1, out nOut);
                    if (genargs == null)
                    {
                        int count;
                        importer.CountEnum(hEnum, out count);
                        genargs = new string[count];
                    }
                    if (nOut == 0)
                        break;

                    Debug.Assert(nOut == 1);
                    if (nOut == 1)
                    {
                        uint genIndex;
                        int genFlags, ptkOwner, ptkKind;
                        ulong genArgNameSize;

                        importer2.GetGenericParamProps(genTypeToken,
                                                       out genIndex,
                                                       out genFlags,
                                                       out ptkOwner,
                                                       out ptkKind,
                                                       null,
                                                       0,
                                                       out genArgNameSize);
                        StringBuilder genArgName = new StringBuilder((int)genArgNameSize);
                        importer2.GetGenericParamProps(genTypeToken,
                                                       out genIndex,
                                                       out genFlags,
                                                       out ptkOwner,
                                                       out ptkKind,
                                                       genArgName,
                                                       (ulong)genArgName.Capacity,
                                                       out genArgNameSize);

                        genargs[i] = genArgName.ToString();
                    }
                    ++i;
                } while (i < genargs.Length);
            }
            finally
            {
                if (hEnum != IntPtr.Zero)
                    importer2.CloseEnum(hEnum);
            }
            return genargs;
        }
    }
}
