using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DebugEngine.Debugee;
using DebugEngine.Debugee.Wrappers;
using DebugEngine.Interfaces;
using DebugEngine.Utilities;

namespace DebugEngine.MetaDataUtils
{
    class MetadataMgr
    {
        public static string GetFullName(CorFunction function)
        {
            //IMetadataImport m_importer;
            
            MetaModule module = new MetaModule(function.Module.corModule);
            //MetadataMethodInfo method = module.FindMethod(function.Class.Token, function.Token);
            MetaType type = module.FindType(function.Class.Token);
            MetadataMethodInfo method = type.FindMethod((int)function.Token);
            return method.Signature;
        }
        public static IList<PARAMETER> GetFunArguments(CorFunction function)
        {
            //IMetadataImport m_importer;

            MetaModule module = new MetaModule(function.Module.corModule);
            //MetadataMethodInfo method = module.FindMethod(function.Class.Token, function.Token);
            MetaType type = module.FindType(function.Class.Token);
            MetadataMethodInfo method = type.FindMethod((int)function.Token);
            return method.Arguments;
        }
        
        public static METHODSig GetMethodSig(CorFunction function) {
            METHODSig methodsig = new METHODSig();
            MetaModule module = new MetaModule(function.Module.corModule);
            //MetadataMethodInfo method = module.FindMethod(function.Class.Token, function.Token);
            MetaType type = module.FindType(function.Class.Token);
            if (type != null){
                MetadataMethodInfo method = type.FindMethod((int)function.Token);
                if (method != null){
                    methodsig.name = type.Name + "." + method.Name;
                    methodsig.isStatic = method.IsStatic;
                    methodsig.isGeneric = method.IsGenericMethod;
                    //methodsig.retuntype
                    //ParameterInfo[] paramaters = method.GetParameters();
                    //if ((paramaters != null) && (paramaters.Length > 0)){
                    //    List<PARAMETER> prms = new List<PARAMETER>();
                    //    foreach (ParameterInfo arg in paramaters){
                    //        PARAMETER param = new PARAMETER();
                    //        param.name = arg.Name;
                    //        //param.name = type???
                    //    }
                    //} 
                }
            }

            return methodsig;
        }
        public static MetaType GetClass(CorClass fnClass)
        {
            //IMetadataImport m_importer;
            string name = string.Empty;
            MetaModule module = new MetaModule(fnClass.m_module);
            //MetadataMethodInfo method = module.FindMethod(function.Class.Token, function.Token);
            MetaType type = module.FindType(fnClass.Token);
             
            return type ;
        }
        public static CorFunction GetFuntion(string Module, string className, string funtionName)
        {
            CorModule module = null;
            CorFunction fun = null;
            DebugCache.LoadedModules.TryGetValue(Module, out module);
            if (module == null) {
                throw new BreakPointException("Module not loaded " + Module);
            }
            int token = 0;
            try
            {
                module.Importer.FindTypeDefByName(className, 0, out token);
            }
            catch (Exception) {
                throw new BreakPointException(className + " class is not found in" + Module);
            }
            MetaType type = new MetaType(module.Importer, token);
            try
            {
                MetadataMethodInfo method = type.GetMethod(funtionName);
                fun = module.GetCorFuntion((uint)method.MetadataToken);
            }
            catch (Exception) {
                throw new BreakPointException(funtionName + " Method is not found in" + className);
            }
             
            return fun;
            //method.MetadataToken
        }
        public static CorFunction GetFuntion(CorModule module, string className, string funtionName){
            CorFunction fun = null;
            int token = 0;
            try
            {
                module.Importer.FindTypeDefByName(className, 0, out token);
            }
            catch (Exception)
            {
                throw new BreakPointException(className + " class is not found in" + module.Name);
            }
            MetaType type = new MetaType(module.Importer, token);
            try
            {
                MetadataMethodInfo method = type.GetMethod(funtionName);
                fun = module.GetCorFuntion((uint)method.MetadataToken);
            }
            catch (Exception)
            {
                throw new BreakPointException(funtionName + " Method is not found in" + className);
            }

            return fun;
            //method.MetadataToken
        }
        public static CorFunction GetFuntion(string Module, uint token)
        {
            CorModule module = null;
            CorFunction fun = null;
            DebugCache.LoadedModules.TryGetValue(Module, out module);
            if (module == null){
                throw new BreakPointException("Module not loaded " + Module);
            }
            
            try{
                fun = module.GetCorFuntion((uint)token);
            }
            catch (Exception)
            {
                throw;
            }

            return fun;
            //method.MetadataToken
        }
    }
}
