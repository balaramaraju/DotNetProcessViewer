
using System;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Diagnostics;
using DebugEngine.Interfaces;



namespace DebugEngine.MetaDataUtils
{
    public sealed class MetadataFieldInfo : FieldInfo
    {
        internal MetadataFieldInfo(IMetadataImport importer,int fieldToken, MetaType declaringType)
        {
            m_importer = importer;
            m_fieldToken = fieldToken;
            m_declaringType = declaringType;

            // Initialize
            int mdTypeDef;
            int pchField,pcbSigBlob,pdwCPlusTypeFlab,pcchValue, pdwAttr;
            IntPtr ppvSigBlob;
            IntPtr ppvRawValue;
            m_importer.GetFieldProps(m_fieldToken,
                                     out mdTypeDef,
                                     null,
                                     0,
                                     out pchField,
                                     out pdwAttr,
                                     out ppvSigBlob,
                                     out pcbSigBlob,
                                     out pdwCPlusTypeFlab,
                                     out ppvRawValue,
                                     out pcchValue
                                     );
            
            StringBuilder szField = new StringBuilder(pchField);
            m_importer.GetFieldProps(m_fieldToken,
                                     out mdTypeDef,
                                     szField,
                                     szField.Capacity,
                                     out pchField,
                                     out pdwAttr,
                                     out ppvSigBlob,
                                     out pcbSigBlob,
                                     out pdwCPlusTypeFlab,
                                     out ppvRawValue,
                                     out pcchValue
                                     );
            m_fieldAttributes = (FieldAttributes)pdwAttr;
            m_name = szField.ToString();

            // Get the values for static literal fields with primitive types
            FieldAttributes staticLiteralField = FieldAttributes.Static | FieldAttributes.HasDefault | FieldAttributes.Literal;
            if ((m_fieldAttributes & staticLiteralField) == staticLiteralField)
            {
                m_value = ParseDefaultValue(declaringType,ppvSigBlob,ppvRawValue);
            }
        }

        private static object ParseDefaultValue(MetaType declaringType, IntPtr ppvSigBlob, IntPtr ppvRawValue)
        {
                IntPtr ppvSigTemp = ppvSigBlob;
                CorCallingConvention callingConv = MetadataHelperFunctions.CorSigUncompressCallingConv(ref ppvSigTemp);
                Debug.Assert(callingConv == CorCallingConvention.Field);

                CorElementType elementType = MetadataHelperFunctions.CorSigUncompressElementType(ref ppvSigTemp);
                if (elementType == CorElementType.ELEMENT_TYPE_VALUETYPE)
                {
                        uint token = MetadataHelperFunctions.CorSigUncompressToken(ref ppvSigTemp);

                        if (token == declaringType.Token)
                        {
                            // Static literal field of the same type as the enclosing type
                            // may be one of the value fields of an enum
                            //if (declaringType.IsEnum)
                            //{
                            //    // If so, the value will be of the enum's underlying type,
                            //    // so we change it from VALUETYPE to be that type so that
                            //    // the following code will get the value
                            //    elementType = declaringType.;
                            //}                           
                        }
                }
                
                switch (elementType)
                {
                    case CorElementType.ELEMENT_TYPE_CHAR:
                        return (char)Marshal.ReadByte(ppvRawValue);
                    case CorElementType.ELEMENT_TYPE_I1:
                        return (sbyte)Marshal.ReadByte(ppvRawValue);
                    case CorElementType.ELEMENT_TYPE_U1:
                        return Marshal.ReadByte(ppvRawValue);
                    case CorElementType.ELEMENT_TYPE_I2:
                        return Marshal.ReadInt16(ppvRawValue);
                    case CorElementType.ELEMENT_TYPE_U2:
                        return (ushort)Marshal.ReadInt16(ppvRawValue);
                    case CorElementType.ELEMENT_TYPE_I4:
                        return Marshal.ReadInt32(ppvRawValue);
                    case CorElementType.ELEMENT_TYPE_U4:
                        return (uint)Marshal.ReadInt32(ppvRawValue);
                    case CorElementType.ELEMENT_TYPE_I8:
                        return Marshal.ReadInt64(ppvRawValue);
                    case CorElementType.ELEMENT_TYPE_U8:
                        return (ulong)Marshal.ReadInt64(ppvRawValue);
                    case CorElementType.ELEMENT_TYPE_I:
                        return Marshal.ReadIntPtr(ppvRawValue);
                    case CorElementType.ELEMENT_TYPE_U:
                    case CorElementType.ELEMENT_TYPE_R4:
                    case CorElementType.ELEMENT_TYPE_R8:
                    // Technically U and the floating-point ones are options in the CLI, but not in the CLS or C#, so these are NYI
                    default:
                        return null;
                }
        }

        public override Object GetValue(Object obj)
        {
            FieldAttributes staticLiteralField = FieldAttributes.Static | FieldAttributes.HasDefault | FieldAttributes.Literal;
            if ((m_fieldAttributes & staticLiteralField) != staticLiteralField)
            {
                throw new InvalidOperationException("Field is not a static literal field.");
            }
            if (m_value == null)
            {
                throw new NotImplementedException("GetValue not implemented for the given field type.");
            }
            else
            {
                return m_value;
            }
        }

        public override void SetValue(Object obj, Object value,BindingFlags invokeAttr,Binder binder,CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override Object[] GetCustomAttributes(bool inherit)
        {
            throw new NotImplementedException();
        }

        public override Object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override bool IsDefined (Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }


        public  override Type FieldType 
        {
            get 
            {
                throw new NotImplementedException();
            }
        }   

        public override RuntimeFieldHandle FieldHandle 
        {
            get 
            {
                throw new NotImplementedException();
            }
        }

        public override FieldAttributes Attributes 
        {
            get 
            {
                return m_fieldAttributes;
            }
        }

        public override MemberTypes MemberType 
        {
            get 
            {
                throw new NotImplementedException();
            }
        }
    
        public override String Name 
        {
            get 
            {
                return m_name;
            }
        }
    
        public override Type DeclaringType 
        {
            get 
            {
                throw new NotImplementedException();
            }
        }
    
        public override Type ReflectedType 
        {
            get 
            {
                throw new NotImplementedException();
            }
        }

        public override int MetadataToken 
        {
            get 
            {
                return m_fieldToken;
            }
        }

        private IMetadataImport m_importer;
        private int m_fieldToken;
        private MetaType m_declaringType;

        private string m_name;
        private FieldAttributes m_fieldAttributes;
        private Object m_value;
    }
}
