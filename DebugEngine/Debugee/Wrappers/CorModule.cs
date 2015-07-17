using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DebugEngine.Interfaces;
using System.Diagnostics.SymbolStore;
using System.Runtime.InteropServices;

namespace DebugEngine.Debugee.Wrappers
{
    public class CorModule
    {
        internal ICorDebugModule corModule;
        //private static ISymbolBinder2 g_symBinder;
        string fullName;
        public CorModule(ICorDebugModule md) {
            corModule = md;
            char[] name = new char[300];
            uint sz = 0;
            corModule.GetName((uint)name.Length, out sz, name);
            fullName = new String(name, 0, (int)(sz - 1));
        }
        public string Name {
            get {
                //Need to fix by apply a proper pattren
                int index = fullName.LastIndexOf("\\");
                int endindex = fullName.Substring(index).IndexOf(".");
                string str = fullName.Substring(index + 1, endindex - 1);
                return str;
            }
        }
        public string FullName{
            get{
                return fullName;
            }
        }
        public uint Token {
            get {
                uint value = 0;
                corModule.GetToken(out value);
                return value;
            }
        }
        // lazy loading of Importer
        /// <summary>
        /// Gets the metadata importer for the Module.
        /// </summary>
        /// <value>The CorMetadataImport.</value>
        /// <remarks> The metadata provides rich compile-time information such
        /// as all the functions and types in the module. </remarks>
        public IMetadataImport Importer
        {
            get
            {
                return GetImporter();
            }
        }
        private IMetadataImport GetImporter() { 
            IMetadataImport res = null;
            Guid guid = typeof(IMetadataImport).GUID;
            corModule.GetMetaDataInterface(ref guid, out res);
            return res;
        }
        public CorFunction GetCorFuntion(uint token) { 
           ICorDebugFunction fun = null;
            CorFunction corFun = null;
           corModule.GetFunctionFromToken(token,out fun);
           if (fun != null) {
               corFun = new CorFunction(fun);
           }
           return corFun;
        }
        public ISymbolReader GetSymReader(){ 
            

            IntPtr uImporter = Marshal.GetIUnknownForObject(GetImporter());
            SymBinder binder = new SymBinder();
            ISymbolReader reader = binder.GetReader(uImporter,
                fullName,
                Environment.GetEnvironmentVariable("_NT_SYMBOL_PATH"));

            
            return reader;
        }
    }
}
