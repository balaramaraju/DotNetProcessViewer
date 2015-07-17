using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DebugEngine.Interfaces;

namespace DebugEngine.Debugee.Wrappers
{
    class CorDebugAppDomain{
        public string Name { get; private set; }
        ICorDebugAppDomain m_appdomain;
        public CorDebugAppDomain(ICorDebugAppDomain _appdomain) {
            m_appdomain = _appdomain;
            uint size = 0;
            m_appdomain.GetName(0, out size, null);
            StringBuilder name = new StringBuilder((int)size);
            _appdomain.GetName((uint)name.Capacity, out size, name);
            Name = name.ToString();
        }
        public IList<CorAssembly> LoadedAssemblies { 
            get {
                IList<CorAssembly> assms = new List<CorAssembly>();
                ICorDebugAssemblyEnum assmEnum = null;
                m_appdomain.EnumerateAssemblies(out assmEnum);
                ICorDebugAssembly[] assm = new ICorDebugAssembly[1];
                uint c = 0;
                uint threadcount = 0;
                assmEnum.GetCount(out threadcount);
                for (; assmEnum.Next(1, assm, out c) == 0; /* empty */){
                    assms.Add(new CorAssembly(assm[0]));
                }
                return assms;
            }
        }
    }
}
