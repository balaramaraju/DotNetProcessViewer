using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace DebugEngine.Utilities
{
    static class NativeAPI {
        [
        System.Runtime.ConstrainedExecution.ReliabilityContract(System.Runtime.ConstrainedExecution.Consistency.WillNotCorruptState, System.Runtime.ConstrainedExecution.Cer.Success),
        DllImport("kernel32.dll")
       ]
        public static extern bool CloseHandle(IntPtr handle);

        [
         DllImport("mscoree.dll", CharSet = CharSet.Unicode, PreserveSig = false)
        ]
        public static extern void CLRCreateInstance(ref Guid clsid, ref Guid riid,
            [MarshalAs(UnmanagedType.Interface)]out object metahostInterface);
    }
}
