using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DebugEngine.Interfaces;
using DebugEngine.Debugee.Wrappers;

namespace DebugEngine.MetaDataUtils
{
    class MetaModule
    {
        CorModule m_module;
        IMetadataImport m_importer;
        private List<MetaType> classes;
        public List<MetaType> Types {
            get {
                if (classes == null) {
                    GetClassess();
                }
                return classes;
            }
        }
        /** The name of the module. */
        public String Name
        {
            get
            {
               return m_module.Name;
            }
        }
        void GetClassess() {
            classes = new List<MetaType>();
            IntPtr m_enum = new IntPtr();
            int token = 0;
            uint c = 0;
            do
            {
                m_importer.EnumTypeDefs(ref m_enum, out token, 1, out c);
                classes.Add(new MetaType(m_importer, token));
            } while (c != 0);
            m_importer.CloseEnum(m_enum);
        }
        public MetaModule(ICorDebugModule module) {
            m_module = new CorModule(module);
            Guid interfaceGuid = typeof(IMetadataImport).GUID;
            module.GetMetaDataInterface(ref interfaceGuid, out m_importer);
             
            
        }
        public MetaType FindType(uint classToken)
        {
            foreach (MetaType type in Types)
            {
                if (type.Token == (int)classToken)
                {
                    return type;
                }
            }
            return null;
        }

        public MetadataMethodInfo FindMethod(uint classToken, uint funtionToken) {
            MetadataMethodInfo methodInfo = null;
            foreach (MetaType type in Types)
            {
                if (type.Token == (int)classToken) {
                    methodInfo = type.FindMethod((int)funtionToken);
                    break;
                }
            }
            return methodInfo;
        }

    }
}
