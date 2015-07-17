using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DebugEngine.Interfaces;

namespace DebugEngine.Debugee.Wrappers
{
   public class CorClass
    {
        internal ICorDebugClass m_corClass;
        public ICorDebugModule m_module;
        public CorClass(ICorDebugClass corClass)
        {
            m_corClass = corClass;
            m_corClass.GetModule(out m_module);
        }
        public CorClass(ICorDebugClass corClass, ICorDebugModule module)
        {
            m_corClass = corClass;
            m_module = module;
        }
        public uint Token {
            get {
                uint value = 0;
                m_corClass.GetToken(out value);
                return value;
            }
        }
    }
}
