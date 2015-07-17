

using System;
using System.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


using Microsoft.Win32.SafeHandles;


namespace DebugEngine.Utilities
{
    #region X86 Context
    [StructLayout(LayoutKind.Sequential)]
    public struct WIN32_CONTEXT
    {
        public uint ContextFlags;
        public uint Dr0;
        public uint Dr1;
        public uint Dr2;
        public uint Dr3;
        public uint Dr6;
        public uint Dr7;
        public WIN32_FLOATING_SAVE_AREA FloatSave;
        public uint SegGs;
        public uint SegFs;
        public uint SegEs;
        public uint SegDs;
        public uint Edi;
        public uint Esi;
        public uint Ebx;
        public uint Edx;
        public uint Ecx;
        public uint Eax;
        public uint Ebp;
        public uint Eip;
        public uint SegCs;
        public uint EFlags;
        public uint Esp;
        public uint SegSs;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x200)]
        public byte[] ExtendedRegisters;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WIN32_FLOATING_SAVE_AREA
    {
        public uint ControlWord;
        public uint StatusWord;
        public uint TagWord;
        public uint ErrorOffset;
        public uint ErrorSelector;
        public uint DataOffset;
        public uint DataSelector;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
        public byte[] RegisterArea;
        public uint Cr0NpxState;
    }

    #endregion // X86 Context


    #region Structures for CreateProcess
    [StructLayout(LayoutKind.Sequential, Pack = 8), ComVisible(false)]
    public class PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public int dwProcessId;
        public int dwThreadId;
        public PROCESS_INFORMATION() { }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8), ComVisible(false)]
    public class SECURITY_ATTRIBUTES
    {
        public int nLength;
        private IntPtr lpSecurityDescriptor;
        public bool bInheritHandle;
        public SECURITY_ATTRIBUTES() { }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 8), ComVisible(false)]
    public class STARTUPINFO
    {
        public int cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public int dwX;
        public int dwY;
        public int dwXSize;
        public int dwYSize;
        public int dwXCountChars;
        public int dwYCountChars;
        public int dwFillAttribute;
        public int dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        private IntPtr lpReserved2;
        public SafeFileHandle hStdInput;
        public SafeFileHandle hStdOutput;
        public SafeFileHandle hStdError;
        public STARTUPINFO() { }
    }

    #endregion // Structures for CreateProcess

    public enum CorDebuggerVersion
    {
        RTM = 1, //v1.0
        Everett = 2, //v1.1
        Whidbey = 3, //v2.0
    }

    // copied from Cordebug.idl
    [Flags]
    public enum CorDebugJITCompilerFlags
    {
        CORDEBUG_JIT_DEFAULT = 0x1,
        CORDEBUG_JIT_DISABLE_OPTIMIZATION = 0x3,
        CORDEBUG_JIT_ENABLE_ENC = 0x7
    }

    // keep in sync with CorHdr.h
    public enum CorTokenType
    {
        mdtModule = 0x00000000,       //          
        mdtTypeRef = 0x01000000,       //          
        mdtTypeDef = 0x02000000,       //          
        mdtFieldDef = 0x04000000,       //           
        mdtMethodDef = 0x06000000,       //       
        mdtParamDef = 0x08000000,       //           
        mdtInterfaceImpl = 0x09000000,       //  
        mdtMemberRef = 0x0a000000,       //       
        mdtCustomAttribute = 0x0c000000,       //      
        mdtPermission = 0x0e000000,       //       
        mdtSignature = 0x11000000,       //       
        mdtEvent = 0x14000000,       //           
        mdtProperty = 0x17000000,       //           
        mdtModuleRef = 0x1a000000,       //       
        mdtTypeSpec = 0x1b000000,       //           
        mdtAssembly = 0x20000000,       //
        mdtAssemblyRef = 0x23000000,       //
        mdtFile = 0x26000000,       //
        mdtExportedType = 0x27000000,       //
        mdtManifestResource = 0x28000000,       //
        mdtGenericParam = 0x2a000000,       //
        mdtMethodSpec = 0x2b000000,       //
        mdtGenericParamConstraint = 0x2c000000,

        mdtString = 0x70000000,       //          
        mdtName = 0x71000000,       //
        mdtBaseType = 0x72000000,       // Leave this on the high end value. This does not correspond to metadata table
    }

    public abstract class TokenUtils
    {
        public static CorTokenType TypeFromToken(int token)
        {
            return (CorTokenType)((UInt32)token & 0xff000000);
        }

        public static int RidFromToken(int token)
        {
            return (int)((UInt32)token & 0x00ffffff);
        }

        public static bool IsNullToken(int token)
        {
            return (RidFromToken(token) == 0);
        }
    }


    abstract class HRUtils
    {
        public static bool IsFailingHR(int hr)
        {
            return hr < 0;
        }

        public static bool IsSOK(int hr)
        {
            return hr == 0;
        }
    }
} 
