using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DebugEngine.Interfaces;
using System.IO;

namespace DebugEngine.Debugee.Wrappers
{
    public class CorAssembly
    {
        private ICorDebugAssembly corAssembly = null;
        private string fullName;
        private string assemblyname;
        private string location;

        private List<CorModule> loadedmodules;
        public CorAssembly(ICorDebugAssembly assembly) {
            corAssembly = assembly;
            char[] name = new char[300];
            uint sz = 0;
            corAssembly.GetName((uint)name.Length, out sz, name);
            fullName = new String(name, 0, (int)(sz - 1));
            if (fullName.Contains(Path.DirectorySeparatorChar))
            {
                assemblyname = Path.GetFileName(fullName);
                location = Path.GetDirectoryName(fullName);
            }
            else {
                assemblyname = location = fullName;
            }
            
        }
        
        public string Name {
            get {
                return assemblyname;
            }
        }
        public string Location {
            get {
                return location;
            }
        }
        public string FullName {
            get { return fullName; }
        }
        public List<CorModule> Modules {
            get {
                List<CorModule> modules = new List<CorModule>();
                ICorDebugModuleEnum corModules = null;
                corAssembly.EnumerateModules(out corModules);
                ICorDebugModule[] corModule = new ICorDebugModule[1];
                uint c = 0;
                uint modulesCount = 0;
                corModules.GetCount(out modulesCount);
                while(corModules.Next(1, corModule, out c) == 0){
                    modules.Add(new CorModule(corModule[0]));
                }
                return modules;
            }
        }

        
    }
}
