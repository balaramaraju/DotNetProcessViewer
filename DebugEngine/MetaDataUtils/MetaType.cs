using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DebugEngine.Interfaces;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections;
using DebugEngine.Debugee.Wrappers;

namespace DebugEngine.MetaDataUtils
{
    public enum MetadataTokenType
    {
        Module = 0x00000000,
        TypeRef = 0x01000000,
        TypeDef = 0x02000000,
        FieldDef = 0x04000000,
        MethodDef = 0x06000000,
        ParamDef = 0x08000000,
        InterfaceImpl = 0x09000000,
        MemberRef = 0x0a000000,
        CustomAttribute = 0x0c000000,
        Permission = 0x0e000000,
        Signature = 0x11000000,
        Event = 0x14000000,
        Property = 0x17000000,
        ModuleRef = 0x1a000000,
        TypeSpec = 0x1b000000,
        Assembly = 0x20000000,
        AssemblyRef = 0x23000000,
        File = 0x26000000,
        ExportedType = 0x27000000,
        ManifestResource = 0x28000000,
        GenericPar = 0x2a000000,
        MethodSpec = 0x2b000000,
        String = 0x70000000,
        Name = 0x71000000,
        BaseType = 0x72000000,
        Invalid = 0x7FFFFFFF,
    }
    public enum CorCallingConvention
    {
        Default = 0x0,

        VarArg = 0x5,
        Field = 0x6,
        LocalSig = 0x7,
        Property = 0x8,
        Unmanaged = 0x9,
        GenericInst = 0xa,  // generic method instantiation
        NativeVarArg = 0xb,  // used ONLY for 64bit vararg PInvoke calls

        // The high bits of the calling convention convey additional info
        Mask = 0x0f,  // Calling convention is bottom 4 bits
        HasThis = 0x20,  // Top bit indicates a 'this' parameter
        ExplicitThis = 0x40,  // This parameter is explicitly in the signature
        Generic = 0x10,  // Generic method sig with explicit number of type arguments (precedes ordinary parameter count)
    };
    class MetaType :  Type
    {
        int m_token = 0;
        IMetadataImport m_importer;
        string m_name;
        bool m_isEnum;
        protected CorElementType m_enumUnderlyingType;
        bool m_isFlagsEnum;
        IList<MetadataMethodInfo> methods = new List<MetadataMethodInfo>();

        
        public int Token {
            get {
                return m_token;
            }
        }
        public MetaType(IMetadataImport importer, int token) {
            m_importer = importer;
            m_token = token;
            Init();
        }
        
        public new List<MetadataMethodInfo> GetMethods(string Name)
        {
            List<MetadataMethodInfo> methods = new List<MetadataMethodInfo>();
            foreach (MetadataMethodInfo mi in GetMethods())
            {
                int index = mi.Name.LastIndexOf('.');
                string methodName = mi.Name;
                if (Name == methodName)
                {
                    methods.Add(mi);
                }
                else
                {
                    if (index > 0)
                    {
                        methodName = methodName.Substring(index + 1, methodName.Length - (index + 1));
                    }
                    if (Name == methodName)
                    {
                        methods.Add(mi);
                    }
                }
            }
            return methods;
        }
        public new MetadataMethodInfo  GetMethod(string Name) {
            foreach (MetadataMethodInfo mi in GetMethods())
            {
                int index = mi.Name.LastIndexOf('.');
                string methodName = mi.Name;
                if (Name == methodName)
                {
                    return mi;
                }
                else
                {
                    if (index > 0){
                        methodName = methodName.Substring(index + 1, methodName.Length - (index + 1));
                    }
                    if (Name == methodName)
                    {
                        return mi;
                    }
                }
            }
            return null;
        }
        void Init() {

            if (m_token == 0)
            {
                // classToken of 0 represents a special type that contains
                // fields of global parameters.
                m_name = "";
            }
            else
            {
                // get info about the type
                int size;
                int ptkExtends;
                TypeAttributes pdwTypeDefFlags;
                m_importer.GetTypeDefProps(m_token,
                                         null,
                                         0,
                                         out size,
                                         out pdwTypeDefFlags,
                                         out ptkExtends
                                         );
                StringBuilder szTypedef = new StringBuilder(size);
                m_importer.GetTypeDefProps(m_token,
                                         szTypedef,
                                         szTypedef.Capacity,
                                         out size,
                                         out pdwTypeDefFlags,
                                         out ptkExtends
                                         );

                m_name = GetNestedClassPrefix( pdwTypeDefFlags) + szTypedef.ToString();

                // Check whether the type is an enum
                string baseTypeName = GetTypeName(ptkExtends);

                IntPtr ppvSig;
                if (baseTypeName == "System.Enum")
                {
                    m_isEnum = true;
                    m_enumUnderlyingType = GetEnumUnderlyingType();

                    // Check for flags enum by looking for FlagsAttribute
                    uint sigSize = 0;
                    ppvSig = IntPtr.Zero;
                    int hr = m_importer.GetCustomAttributeByName(m_token, "System.FlagsAttribute", out ppvSig, out sigSize);
                    if (hr < 0)
                    {
                        throw new COMException("Exception looking for flags attribute", hr);
                    }
                    m_isFlagsEnum = (hr == 0);  // S_OK means the attribute is present.
                }


                //////////////////
                
            }
        }
        private CorElementType GetEnumUnderlyingType()
        {
            IntPtr hEnum = IntPtr.Zero;
            int mdFieldDef;
            uint numFieldDefs;
            int fieldAttributes;
            int nameSize;
            int cPlusTypeFlab;
            IntPtr ppValue;
            int pcchValue;
            IntPtr ppvSig;
            int size;
            int classToken;

            m_importer.EnumFields(ref hEnum, m_token, out mdFieldDef, 1, out numFieldDefs);
            while (numFieldDefs != 0)
            {
                m_importer.GetFieldProps(mdFieldDef, out classToken, null, 0, out nameSize, out fieldAttributes, out ppvSig, out size, out cPlusTypeFlab, out ppValue, out pcchValue);
               
                // Enums should have one instance field that indicates the underlying type
                if ((((FieldAttributes)fieldAttributes) & FieldAttributes.Static) == 0)
                {
                    IntPtr ppvSigTemp = ppvSig;
                    CorCallingConvention callingConv = MetadataHelperFunctions.CorSigUncompressCallingConv(ref ppvSigTemp);
                    

                    return MetadataHelperFunctions.CorSigUncompressElementType(ref ppvSigTemp);
                }

                m_importer.EnumFields(ref hEnum, m_token, out mdFieldDef, 1, out numFieldDefs);
            }

            throw new ArgumentException("Non-enum passed to GetEnumUnderlyingType.");
        }

        // returns "" for normal classes, returns prefix for nested classes
        private  string GetNestedClassPrefix( TypeAttributes attribs)
        {
            if ((attribs & TypeAttributes.VisibilityMask) > TypeAttributes.Public)
            {
                // it is a nested class
                int enclosingClass;
                m_importer.GetNestedClassProps(m_token, out enclosingClass);
                MetaType mt = new MetaType(m_importer, enclosingClass);

                return mt.Name + "+";
            }
            else
                return String.Empty;
        }
        private string GetTypeName(int token)
        {
            // Get the base type name
            StringBuilder sbBaseName = new StringBuilder();
            
            int size;
            TypeAttributes pdwTypeDefFlags;
            int ptkExtends;
            //check if it is 
            if (token == (int)MetadataTokenType.TypeDef)
            {
                m_importer.GetTypeDefProps(token,
                                    null,
                                    0,
                                    out size,
                                    out pdwTypeDefFlags,
                                    out ptkExtends
                                    );
                sbBaseName.Capacity = size;
                m_importer.GetTypeDefProps(token,
                                    sbBaseName,
                                    sbBaseName.Capacity,
                                    out size,
                                    out pdwTypeDefFlags,
                                    out ptkExtends
                                    );
            }
            else if (token == (int)MetadataTokenType.TypeRef)
            {
                // Some types extend TypeRef 0x02000000 as a special-case
                // But that token does not exist so we can't get a name for it
                if ((m_token &  0x00FFFFFF) != 0)
                {
                    int resolutionScope;
                    m_importer.GetTypeRefProps(token,
                                        out resolutionScope,
                                        null,
                                        0,
                                        out size
                                        );
                    sbBaseName.Capacity = size;
                    m_importer.GetTypeRefProps(token,
                                        out resolutionScope,
                                        sbBaseName,
                                        sbBaseName.Capacity,
                                        out size
                                        );
                }
            }
            // Note the base type can also be a TypeSpec token, but that only happens
            // for arrays, generics, that sort of thing. In this case, we'll leave the base
            // type name stringbuilder empty, and thus know it's not an enum.

            return sbBaseName.ToString();
        }
        public MetadataMethodInfo FindMethod(int token) {
            return new MetadataMethodInfo(m_importer, token,this);
        }
        public List<FieldInfo> GetFieldInfo() {
            List<FieldInfo> al = new List<FieldInfo>();
            IntPtr hEnum = new IntPtr();

            int fieldToken;
            try
            {
                while (true)
                {
                    uint size;
                    m_importer.EnumFields(ref hEnum, Token, out fieldToken, 1, out size);
                    if (size == 0)
                        break;
                    
                    al.Add(new MetadataFieldInfo(m_importer, fieldToken, this));
                }
            }
            finally
            {
                m_importer.CloseEnum(hEnum);
            }
            return al;
        }

        public override Assembly Assembly
        {
            get { throw new NotImplementedException(); }
        }

        public override string AssemblyQualifiedName
        {
            get { throw new NotImplementedException(); }
        }

        public override Type BaseType
        {
            get { throw new NotImplementedException(); }
        }

        public override string FullName
        {
            get { throw new NotImplementedException(); }
        }

        public override Guid GUID
        {
            get { throw new NotImplementedException(); }
        }

        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            throw new NotImplementedException();
        }

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override Type GetElementType()
        {
            throw new NotImplementedException();
        }

        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            List<FieldInfo> al = new List<FieldInfo>();
            IntPtr hEnum = new IntPtr();

            int fieldToken;
            try
            {
                while (true)
                {
                    uint size;
                    m_importer.EnumFields(ref hEnum, (int)m_token, out fieldToken, 1, out size);
                    if (size == 0)
                        break;
                    
                    al.Add(new MetadataFieldInfo(m_importer, fieldToken, this));
                }
            }
            finally
            {
                m_importer.CloseEnum(hEnum);
            }
            return al.ToArray();
        }

        public override Type GetInterface(string name, bool ignoreCase)
        {
            throw new NotImplementedException();
        }

        public override Type[] GetInterfaces()
        {
            throw new NotImplementedException();
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
        throw new NotImplementedException();
        }
        public  new IList<MetadataMethodInfo> GetMethods()
        {

            if (methods.Count == 0)
            {
                IntPtr hEnum = new IntPtr();

                int methodToken;
                try
                {
                    while (true)
                    {
                        int size;
                        m_importer.EnumMethods(ref hEnum, (int)m_token, out methodToken, 1, out size);
                        if (size == 0)
                            break;
                        methods.Add(new MetadataMethodInfo(m_importer, methodToken, this));
                    }
                }
                finally
                {
                    m_importer.CloseEnum(hEnum);
                }
            }
            return (methods);
        }

        public override Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Return an array of the property tokens on the type.
        /// </summary>
        public List<MetadataMethodInfo> GetProperties(){
            List<MetadataMethodInfo> props = new List<MetadataMethodInfo>();
            foreach (MetadataMethodInfo mi in GetMethods()) {
                if (mi.Name.Contains(".get_")) {
                    if (mi.IsStatic || (mi.Arguments.Count == 0)){
                        props.Add(mi);
                    }
                    else if (mi.Arguments.Count == 1) {
                        // for index properties
                        if (mi.Arguments[0].name == "index") {
                            props.Add(mi);
                        }
                    }
                }
            }
            return props;
            
        }
        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        protected override bool HasElementTypeImpl()
        {
            throw new NotImplementedException();
        }

        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, System.Globalization.CultureInfo culture, string[] namedParameters)
        {
            throw new NotImplementedException();
        }

        protected override bool IsArrayImpl()
        {
            throw new NotImplementedException();
        }

        protected override bool IsByRefImpl()
        {
            throw new NotImplementedException();
        }

        protected override bool IsCOMObjectImpl()
        {
            throw new NotImplementedException();
        }

        protected override bool IsPointerImpl()
        {
            throw new NotImplementedException();
        }

        protected override bool IsPrimitiveImpl()
        {
            throw new NotImplementedException();
        }

        public override Module Module
        {
            get { throw new NotImplementedException(); }
        }

        public override string Namespace
        {
            get { throw new NotImplementedException(); }
        }

        public override Type UnderlyingSystemType
        {
            get { throw new NotImplementedException(); }
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override string Name
        {
            get { 
               return m_name;
            }
        }
    }
    class DebugMetaType : MetaType {

        CorType m_Type;
        public bool IsClass {
            get{
                return m_enumUnderlyingType == CorElementType.ELEMENT_TYPE_CLASS ||
                       m_enumUnderlyingType == CorElementType.ELEMENT_TYPE_OBJECT;
            }
        }
        public bool IsValueType{
            get{
                return m_enumUnderlyingType == CorElementType.ELEMENT_TYPE_VALUETYPE;
            }
        }

        public bool IsArray{
            get{
                return m_enumUnderlyingType == CorElementType.ELEMENT_TYPE_ARRAY ||
                       m_enumUnderlyingType == CorElementType.ELEMENT_TYPE_SZARRAY;
            }
        }

        DebugMetaType(IMetadataImport importer, int token, CorType type) : base(importer, token) {
            m_Type = type;
            if (this.IsClass || this.IsValueType || this.IsArray){
                ICorDebugTypeEnum typeEnum = null;
                type.Raw.EnumerateTypeParameters(out typeEnum);
                //foreach (ICorDebugType t in ){
                //    typeArguments.Add(DebugType.Create(process, t));
                //}
            }
        }

        //string TypeName { }
    }
    
}
