using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using DebugEngine.Utilities;
using DebugEngine.Interfaces;
using System.Collections;
using DebugEngine.Debugee.Wrappers;
using DebugEngine.MetaDataUtils;
using System.Text.RegularExpressions;

namespace DebugEngine.Utilities
{
    public sealed class MDbgValue : MarshalByRefObject
    {

        /// <summary>
        /// Creates a new instance of the MDbgValue Object.
        /// This constructor is public so that applications can use this class to print values (CorValue).
        /// CorValue's can be returned for example by funceval(CorEval.Result).
        /// </summary>
        /// <param name="value">The CorValue that this MDbgValue will start with.</param>
        public MDbgValue( CorValue value)
        {
            // value can be null, but we should always know what process we are
            // looking at.
            Initialize( null, value);
        }

        /// <summary>
        /// Creates a new instance of the MDbgValue Object.
        /// This constructor is public so that applications can use this class to print values (CorValue).
        /// CorValue's can be returned for example by funceval(CorEval.Result).
        /// </summary>
        /// <param name="process">The Process that will own the Value.</param>
        /// <param name="name">The name of the variable.</param>
        /// <param name="value">The CorValue that this MDbgValue will start with.</param>
        public MDbgValue( string name, CorValue value) {
            Initialize( name, value);
        }

        private void Initialize(string name, CorValue value)
        {
            m_name = name;
            m_corValue = value;
        }

        /// <summary>
        /// The CorValue stored in the MDbgValue.
        /// </summary>
        /// <value>The CorValue.</value>
        public CorValue CorValue
        {
            get
            {
                return m_corValue;
            }
        }


        /// <summary>
        /// The Name of this Value.
        /// </summary>
        /// <value>The Name.</value>
        public string Name
        {
            get
            {
                return m_name;
            }
        }

        /// <summary>
        /// The Name of this Type.
        /// </summary>
        /// <value>The TypeName.</value>
        public string TypeName
        {
            get
            {
                if (CorValue == null)
                {
                    return "N/A";
                }

                // Every Value should have a non-null type associated with it.
                CorType t = CorValue.ExactType;
                return InternalUtil.PrintCorType( t);
            }
        }

        /// <summary>
        /// Is this type a complex type.
        /// </summary>
        /// <value>true if it is complex, else false.</value>
        public bool IsComplexType
        {
            get
            {
                if (CorValue == null)
                    return false;
                CorValue value;
                try
                {
                    value = Dereference(CorValue, null);
                }
                catch (COMException ce)
                {
                    if (ce.ErrorCode == (int)HResult.CORDBG_E_BAD_REFERENCE_VALUE)
                        return false;
                    throw;
                }
                if (value == null)
                    return false;
                return (value.Type == CorElementType.ELEMENT_TYPE_CLASS ||
                        value.Type == CorElementType.ELEMENT_TYPE_VALUETYPE);
            }
        }

        /// <summary>
        /// Is this type an array type.
        /// </summary>
        /// <value>true if it is an array type, else false.</value>
        public bool IsArrayType
        {
            get
            {
                if (CorValue == null)
                    return false;
                CorValue value;
                try
                {
                    value = Dereference(CorValue, null);
                }
                catch (COMException ce)
                {
                    if (ce.ErrorCode == (int)HResult.CORDBG_E_BAD_REFERENCE_VALUE)
                        return false;
                    throw;
                }

                if (value == null)
                    return false;

                return (value.Type == CorElementType.ELEMENT_TYPE_SZARRAY ||
                        value.Type == CorElementType.ELEMENT_TYPE_ARRAY);
            }
        }

        /// <summary>
        /// Is this Value Null.
        /// </summary>
        /// <value>true if it is Null, else false.</value>
        public bool IsNull
        {
            get
            {
                if (CorValue == null)
                    return true;
                CorValue value;
                try
                {
                    value = Dereference(CorValue, null);
                }
                catch (COMException ce)
                {
                    if (ce.ErrorCode == (int)HResult.CORDBG_E_BAD_REFERENCE_VALUE)
                        return false;
                    throw;
                }
                return (value == null);
            }
        }

        /// <summary>
        /// Gets the Value.
        /// </summary>
        /// <param name="expand">Should it expand inner objects.</param>
        /// <returns>A string representation of the Value.</returns>
        public string GetStringValue(bool expand)
        {
            return GetStringValue(expand ? 1 : 0);
        }

        /// <summary>
        /// Gets the Value.
        /// </summary>
        /// <param name="expandDepth">How deep inner objects should be expanded. Value
        /// 0 means don't expand at all.</param>
        /// <returns>A string representation of the Value.</returns>
        public string GetStringValue(int expandDepth)
        {
            // by default we can do funcevals.
            return GetStringValue(expandDepth, true);
        }

        /// <summary>
        /// Gets the Value.
        /// </summary>
        /// <param name="expandDepth">How deep inner objects should be expanded. Value
        /// 0 means don't expand at all.</param>
        /// <param name="canDoFunceval">Set to true if ToString() should be called to get better description.</param>
        /// <returns>A string representation of the Value.</returns>
        public string GetStringValue(int expandDepth, bool canDoFunceval)
        {
            return InternalGetValue(0, expandDepth, canDoFunceval);
        }

        /// <summary>
        /// Gets the specified Field.
        /// </summary>
        /// <param name="name">The Name of the Field to get.</param>
        /// <returns>The Value of the specified Field.</returns>
        public MDbgValue GetField(string name)
        {
            MDbgValue ret = null;
            foreach (MDbgValue v in GetFields())
                if (v.Name.Equals(name))
                {
                    ret = v;
                    break;
                }
            if (ret == null)
                throw new Exception("Field '" + name + "' not found.");
            return ret;
        }

        /// <summary>
        /// Gets all the Fields
        /// </summary>
        /// <returns>An array of all Fields.</returns>
        public MDbgValue[] GetFields()
        {
            if (!IsComplexType)
                throw new Exception("Type is not complex");

            if (m_cachedFields == null)
                m_cachedFields = InternalGetFields();

            return m_cachedFields;
        }

        private MDbgValue[] InternalGetFields()
        {
            List<MDbgValue> al = new List<MDbgValue>();

            //dereference && (unbox);
            CorValue value = Dereference(CorValue, null);
            if (value == null)
            {
                throw new Exception("null value");
            }
            Unbox(ref value);
            CorObjectValue ov = value.CastToObjectValue();

            CorType cType = ov.ExactType;

            CorFunctionFrame cFrame = null;
           

            // initialization
            CorClass corClass = ov.Class;
            
            // iteration through class hierarchy
            while (true)
            {
                Type classType = new MetaType(new CorModule(corClass.m_module).Importer, (int)corClass.Token);

                foreach (MetadataFieldInfo fi in classType.GetFields()){
                    CorValue fieldValue = null;
                    try{
                        if (fi.IsLiteral){
                            fieldValue = null;
                            // for now we just hide the constant fields.
                            continue;
                        }else if (fi.IsStatic){
                            if (cFrame == null){
                                // Without a frame, we won't be able to find static values.  So
                                // just skip this guy
                                continue;
                            }

                            fieldValue = cType.GetStaticFieldValue(fi.MetadataToken, cFrame);
                        }
                        else
                        {
                            // we are asuming normal field value
                            fieldValue = ov.GetFieldValue(corClass, fi.MetadataToken);
                        }
                    }
                    catch (COMException)
                    {
                        // we won't report any problems.
                    }
                    al.Add(new MDbgValue( fi.Name, fieldValue));
                }
                cType = cType.Base;
                if (cType == null)
                    break;
                corClass = cType.Class;
                //classModule = Process.Modules.Lookup(corClass.Module);
            }

            return al.ToArray();
        }
        /// <summary>
        /// Gets Array Items.  This function can be called only on one dimensional arrays.
        /// </summary>
        /// <returns>An array of the values for the Array Items.</returns>
        public MDbgValue[] GetArrayItems()
        {
            if (!IsArrayType)
                throw new Exception("Type is not array type");

            CorValue value = Dereference(CorValue, null);

            CorArrayValue av = value.CastToArrayValue();
            int[] dims = av.GetDimensions();
            

            ArrayList al = new ArrayList();
            for (int i = 0; i < dims[0]; i++)
            {
                MDbgValue v = new MDbgValue( "[" + i + "]", av.GetElementAtPosition(i));
                al.Add(v);
            }
            return (MDbgValue[])al.ToArray(typeof(MDbgValue));
        }

        /// <summary>
        /// Gets the Array Item for the specified indexes
        /// </summary>
        /// <param name="indexes">Which indexes to get the Array Item for.</param>
        /// <returns>The Value for the given indexes.</returns>
        public MDbgValue GetArrayItem(params int[] indexes)
        {
            if (!IsArrayType)
                throw new Exception("Type is not array type");

            CorValue value = Dereference(CorValue, null);
            CorArrayValue av = value.CastToArrayValue();
            if (av.Rank != indexes.Length)
                throw new Exception("Invalid number of dimensions.");

            StringBuilder sb = new StringBuilder("[");
            for (int i = 0; i < indexes.Length; ++i)
            {
                if (i != 0)
                    sb.Append(",");
                sb.Append(indexes[i]);
            }
            sb.Append("]");

            MDbgValue v = new MDbgValue( sb.ToString(), av.GetElement(indexes));
            return v;
        }

        /// <summary>
        /// Gets or Sets the Value of the MDbgValue to the given value.
        /// </summary>
        /// <value>This is exposed as an Object but can a primitive type, CorReferenceValue, or CorGenericValue.</value>
        public Object Value
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                if (value is CorReferenceValue)
                {
                    CorReferenceValue lsValRef = CorValue.CastToReferenceValue();
                    if (lsValRef == null)
                    {
                        throw new Exception("cannot assign reference value to non-reference value");
                    }
                    lsValRef.Value = ((CorReferenceValue)value).Value;
                }
                else if (value is CorGenericValue)
                {
                    CorGenericValue lsValGen = GetGenericValue();
                    lsValGen.SetValue(((CorGenericValue)value).GetValue());
                }
                else if (value.GetType().IsPrimitive)
                {
                    // trying to set a primitive generic value, let the corapi layer attempt to convert the type                
                    CorGenericValue gv = GetGenericValue();
                    gv.SetValue(value);
                }
                else
                {
                    throw new Exception("Value is of unsupported type.");
                }
            }
        }

        internal void InternalSetName(string variableName)
        {
            m_name = variableName;
        }
        //////////////////////////////////////////////////////////////////////////////////
        //
        // Implementation Part
        //
        //////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Small helper used by InternalGetValue to put parens around the
        /// ptrstring generated by Dereference()
        /// </summary>
        /// <param name="ptrStrBuilder">ptrstring generated by Dereference()</param>
        /// <returns>String with parens around the input string</returns>
        private string MakePrefixFromPtrStringBuilder(StringBuilder ptrStrBuilder)
        {
            if (ptrStrBuilder == null)
                return String.Empty;

            string ptrStr = ptrStrBuilder.ToString();
            if (String.IsNullOrEmpty(ptrStr))
                return String.Empty;

            return "(" + ptrStr + ") ";
        }

        private string InternalGetValue(int indentLevel, int expandDepth, bool canDoFunceval)
        {
            CorValue value = this.CorValue;
            if (value == null)
            {
                return "<N/A>";
            }

            // Record the memory addresses if displaying them is enabled
            string prefix = String.Empty;
            StringBuilder ptrStrBuilder = null;
            ptrStrBuilder = new StringBuilder();

            try
            {
                value = Dereference(value, ptrStrBuilder);
            }
            catch (COMException ce)
            {
                if (ce.ErrorCode == (int)HResult.CORDBG_E_BAD_REFERENCE_VALUE)
                {
                    return MakePrefixFromPtrStringBuilder(ptrStrBuilder) + "<invalid reference value>";
                }
                throw;
            }

            prefix = MakePrefixFromPtrStringBuilder(ptrStrBuilder);

            if (value == null)
            {
                return prefix + "<null>";
            }

            Unbox(ref value);

            switch (value.Type)
            {
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                case CorElementType.ELEMENT_TYPE_I1:
                case CorElementType.ELEMENT_TYPE_U1:
                case CorElementType.ELEMENT_TYPE_I2:
                case CorElementType.ELEMENT_TYPE_U2:
                case CorElementType.ELEMENT_TYPE_I4:
                case CorElementType.ELEMENT_TYPE_U4:
                case CorElementType.ELEMENT_TYPE_I:
                case CorElementType.ELEMENT_TYPE_U:
                case CorElementType.ELEMENT_TYPE_I8:
                case CorElementType.ELEMENT_TYPE_U8:
                case CorElementType.ELEMENT_TYPE_R4:
                case CorElementType.ELEMENT_TYPE_R8:
                case CorElementType.ELEMENT_TYPE_CHAR:
                    {
                        object v = value.CastToGenericValue().GetValue();
                        string result;

                        IFormattable vFormattable = v as IFormattable;
                        if (vFormattable != null)
                            result = vFormattable.ToString(null, System.Globalization.CultureInfo.CurrentUICulture);
                        else
                            result = v.ToString();

                        // let's put quotes around char values
                        if (value.Type == CorElementType.ELEMENT_TYPE_CHAR)
                            result = "'" + result + "'";

                        return prefix + result;
                    }

                case CorElementType.ELEMENT_TYPE_CLASS:
                case CorElementType.ELEMENT_TYPE_VALUETYPE:
                    CorObjectValue ov = value.CastToObjectValue();
                    return prefix + "Need Fix";
                        //PrintObject(indentLevel, ov, expandDepth, canDoFunceval);

                case CorElementType.ELEMENT_TYPE_STRING:
                    CorStringValue sv = value.CastToStringValue();
                    return prefix + '"' + sv.String + '"';

                case CorElementType.ELEMENT_TYPE_SZARRAY:
                case CorElementType.ELEMENT_TYPE_ARRAY:
                    CorArrayValue av = value.CastToArrayValue();
                    return prefix + PrintArray(indentLevel, av, expandDepth, canDoFunceval);

                case CorElementType.ELEMENT_TYPE_PTR:
                    return prefix + "<non-null pointer>";

                case CorElementType.ELEMENT_TYPE_FNPTR:
                    return prefix + "0x" + value.CastToReferenceValue().Value.ToString("X");

                case CorElementType.ELEMENT_TYPE_BYREF:
                case CorElementType.ELEMENT_TYPE_TYPEDBYREF:
                case CorElementType.ELEMENT_TYPE_OBJECT:
                default:
                    return prefix + "<printing value of type: " + value.Type + " not implemented>";
            }
        }

        private void Unbox(ref CorValue value)
        {
            CorBoxValue boxVal = value.CastToBoxValue();
            if (boxVal != null)
                value = boxVal.GetObject();
        }

        /// <summary>
        /// Recursively dereference the input value until we finally find a non-dereferenceable
        /// value.  Along the way, optionally build up a "ptr string" that shows the addresses
        /// we dereference, separated by "->".
        /// </summary>
        /// <param name="value">Value to dereference</param>
        /// <param name="ptrStringBuilder">StringBuilder if caller wants us to generate
        /// a "ptr string" (in which case we'll stick it there).  If caller doesn't want
        /// a ptr string, this can be null</param>
        /// <returns>CorValue we arrive at after dereferencing as many times as we can</returns>
        private CorValue Dereference(CorValue value, StringBuilder ptrStringBuilder)
        {
            while (true)
            {
                CorReferenceValue rv = value.CastToReferenceValue();
                if (rv == null)
                    break; // not a reference

                if (ptrStringBuilder != null)
                {
                    if (ptrStringBuilder.Length > 0)
                    {
                        ptrStringBuilder.Append("->");
                    }
                    ptrStringBuilder.Append("0x" + rv.Value.ToString("X", System.Globalization.CultureInfo.CurrentUICulture));
                }

                // Bail as soon as we hit a reference to NULL
                if (rv.IsNull)
                    return null;    // reference to null

                CorValue newValue = null;
                try
                {
                    newValue = rv.Dereference();
                }
                catch (COMException ce)
                {
                    // Check for any errors that are expected
                    if (ce.ErrorCode != (int)HResult.CORDBG_E_VALUE_POINTS_TO_FUNCTION)
                    {
                        throw;  // some other error
                    }
                }

                if (newValue == null)
                    break;  // couldn't dereference the reference (eg. void*)

                value = newValue;
            }
            return value;
        }

        //Yet to implements ???
        bool IsNullableType(CorType ct)
        {
            if (ct.Type != CorElementType.ELEMENT_TYPE_VALUETYPE)
                return false;

            //MDbgModule m = m_process.Modules.Lookup(ct.Class.Module);
            String name = "";//m.Importer.GetType(ct.Class.Token).FullName;

            return name.Equals("System.Nullable`1");
        }


       
        private string PrintArray(int indentLevel, CorArrayValue av, int expandDepth, bool canDoFunceval)
        {

            StringBuilder txt = new StringBuilder();
            txt.Append("array [");
            int[] dims = av.GetDimensions();

            for (int i = 0; i < dims.Length; ++i)
            {
                if (i != 0)
                    txt.Append(",");
                txt.Append(dims[i]);
            }
            txt.Append("]");

            if (expandDepth > 0 && av.Rank == 1 && av.ElementType != CorElementType.ELEMENT_TYPE_VOID)
            {
                for (int i = 0; i < dims[0]; i++)
                {
                    MDbgValue v = new MDbgValue(av.GetElementAtPosition(i));
                    txt.Append("\n").Append(IndentedString(indentLevel + 1, "[" + i + "] = ")).
            Append(IndentedBlock(indentLevel + 2,
                           v.GetStringValue(expandDepth - 1, canDoFunceval)));
                }
            }
            return txt.ToString();
        }

        private string IndentedString(int indent, string txt)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('\t', indent)
                .Append(txt);
            return sb.ToString();
        }

        private string IndentedBlock(int indent, string text)
        {
            string[] lines = text.Split('\n');
            StringBuilder result = new StringBuilder();

            result.Append(lines[0]); // 1 line is always there since text is not null.
            for (int i = 1; i < lines.Length; ++i)
                result.Append('\n').Append(IndentedString(indent, lines[i]));

            return result.ToString();
        }

        private CorGenericValue GetGenericValue()
        {
            CorGenericValue gv = CorValue.CastToGenericValue();
            if (gv == null)
                throw new Exception("Wrong Type Exception");
            return gv;
        }

        private string m_name;
        private CorValue m_corValue;
        private MDbgValue[] m_cachedFields;
    }

    public class InternalUtil
    {
        // Helper to append generic args from tyenum in pretty format.
        // This will add a string like '<int, Foo<string>>'
        internal static void AddGenericArgs(StringBuilder sb, IEnumerable tyenum)
        {
            int i = 0;
            foreach (CorType t1 in tyenum)
            {
                sb.Append((i == 0) ? '<' : ',');
                InternalUtil.PrintCorType(sb,  t1);
                i++;
            }
            if (i > 0)
            {
                sb.Append('>');
            }
        }


        // Return class as a string.
        /// <summary>
        /// Creates a string representation of CorType.
        /// </summary>
        /// <param name="proc">A debugged process.</param>
        /// <param name="ct">A CorType object representing a type in the debugged process.</param>
        /// <returns>String representaion of the passed in type.</returns>
        public static string PrintCorType(CorType ct)
        {
            StringBuilder sb = new StringBuilder();
            PrintCorType(sb, ct);
            return sb.ToString();
        }

        // Print CorType to the given string builder.
        // Will print generic info. 

        internal static void PrintCorType(StringBuilder sb, CorType ct)
        {
            switch (ct.Type)
            {
                case CorElementType.ELEMENT_TYPE_CLASS:
                case CorElementType.ELEMENT_TYPE_VALUETYPE:
                    // We need to get the name from the metadata. We can get a cached metadata importer
                    // from a MDbgModule, or we could get a new one from the CorModule directly.
                    // Is this hash lookup to get a MDbgModule cheaper than just re-querying for the importer?
                    CorClass cc = ct.Class;
                    MetaType retType = new MetaType(new CorModule(cc.m_module).Importer, (int)cc.Token);
                    string typeName = retType.Name;
                    if (!Regex.IsMatch(typeName, @"^[a-zA-Z0-9_.]+$")) {
                        typeName = "**UNK**";
                    }
                    sb.Append(typeName);
                    return;

                // Primitives
                case CorElementType.ELEMENT_TYPE_BOOLEAN:
                    sb.Append("System.Boolean"); return;
                case CorElementType.ELEMENT_TYPE_CHAR:
                    sb.Append("System.Char"); return;
                case CorElementType.ELEMENT_TYPE_I1:
                    sb.Append("System.SByte"); return;
                case CorElementType.ELEMENT_TYPE_U1:
                    sb.Append("System.Byte"); return;
                case CorElementType.ELEMENT_TYPE_I2:
                    sb.Append("System.Int16"); return;
                case CorElementType.ELEMENT_TYPE_U2:
                    sb.Append("System.UInt16"); return;
                case CorElementType.ELEMENT_TYPE_I4:
                    sb.Append("System.Int32"); return;
                case CorElementType.ELEMENT_TYPE_U4:
                    sb.Append("System.Uint32"); return;
                case CorElementType.ELEMENT_TYPE_I8:
                    sb.Append("System.Int64"); return;
                case CorElementType.ELEMENT_TYPE_U8:
                    sb.Append("System.UInt64"); return;
                case CorElementType.ELEMENT_TYPE_I:
                    sb.Append("System.IntPtr"); return;
                case CorElementType.ELEMENT_TYPE_U:
                    sb.Append("System.UIntPtr"); return;
                case CorElementType.ELEMENT_TYPE_R4:
                    sb.Append("System.Single"); return;
                case CorElementType.ELEMENT_TYPE_R8:
                    sb.Append("System.Double"); return;

                // Well known class-types.
                case CorElementType.ELEMENT_TYPE_OBJECT:
                    sb.Append("System.Object"); return;
                case CorElementType.ELEMENT_TYPE_STRING:
                    sb.Append("System.String"); return;


                // Special compound types. Based off first type-param
                case CorElementType.ELEMENT_TYPE_SZARRAY:
                case CorElementType.ELEMENT_TYPE_ARRAY:
                case CorElementType.ELEMENT_TYPE_BYREF:
                case CorElementType.ELEMENT_TYPE_PTR:
                    CorType t = ct.FirstTypeParameter;
                    PrintCorType(sb,  t);
                    switch (ct.Type)
                    {
                        case CorElementType.ELEMENT_TYPE_SZARRAY:
                            sb.Append("[]");
                            return;
                        case CorElementType.ELEMENT_TYPE_ARRAY:
                            int rank = ct.Rank;
                            sb.Append('[');
                            for (int i = 0; i < rank - 1; i++)
                            {

                                sb.Append(',');
                            }
                            sb.Append(']');
                            return;
                        case CorElementType.ELEMENT_TYPE_BYREF:
                            sb.Append("&");
                            return;
                        case CorElementType.ELEMENT_TYPE_PTR:
                            sb.Append("*");
                            return;
                    }
                                
                    return;

                case CorElementType.ELEMENT_TYPE_FNPTR:
                    sb.Append("*(...)");
                    return;
                case CorElementType.ELEMENT_TYPE_TYPEDBYREF:
                    sb.Append("typedbyref");
                    return;
                default:
                    sb.Append("<unknown>");
                    return;
            }
        } // end PrintClass
    } // end class InternalUtil
}
