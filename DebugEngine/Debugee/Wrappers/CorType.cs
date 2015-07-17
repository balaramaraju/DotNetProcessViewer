using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DebugEngine.Interfaces;
using System.Collections;
using DebugEngine.Utilities;

namespace DebugEngine.Debugee.Wrappers
{
    public sealed class CorType{
        internal ICorDebugType m_type;

        internal CorType(ICorDebugType type){
            m_type = type;
        }


        internal ICorDebugType GetInterface()
        {
            return m_type;
        }

        [CLSCompliant(false)]
        public ICorDebugType Raw
        {
            get
            {
                return m_type;
            }
        }

        /** Element type of the type. */
        public CorElementType Type
        {
            get
            {
                CorElementType type;
                m_type.GetType(out type);
                return type;
            }
        }

        /** Class of the type */
        public CorClass Class
        {
            get
            {
                ICorDebugClass c = null;
                m_type.GetClass(out c);
                return c == null ? null : new CorClass(c);
            }
        }

        public int Rank
        {
            get
            {
                uint pRank = 0;
                m_type.GetRank(out pRank);
                return (int)pRank;
            }
        }

        // Provide the first CorType parameter in the TypeParameters collection.
        // This is a convenience operator.
        public CorType FirstTypeParameter
        {
            get
            {
                ICorDebugType dt = null;
                m_type.GetFirstTypeParameter(out dt);
                return dt == null ? null : new CorType(dt);
            }
        }

        public CorType Base
        {
            get
            {
                ICorDebugType dt = null;
                m_type.GetBase(out dt);
                return dt == null ? null : new CorType(dt);
            }
        }

        public CorValue GetStaticFieldValue(int fieldToken, CorFunctionFrame frame)
        {
            ICorDebugValue dv = null;
            m_type.GetStaticFieldValue((uint)fieldToken, frame.frame, out dv);
            return dv == null ? null : new CorValue(dv);
        }

        // Expose IEnumerable, which can be used with for-each constructs.
        // This will provide an collection of CorType parameters.
        public IEnumerable TypeParameters
        {
            get
            {
                ICorDebugTypeEnum etp = null;
                m_type.EnumerateTypeParameters(out etp);
                if (etp == null) return null;
                return new CorTypeEnumerator(etp);
            }
        }
    }
    public class CorTypeEnumerator : IEnumerable, IEnumerator, ICloneable
    {
        private ICorDebugTypeEnum m_enum;
        private CorType m_ty;

        internal CorTypeEnumerator(ICorDebugTypeEnum typeEnumerator)
        {
            m_enum = typeEnumerator;
        }

        //
        // ICloneable interface
        //
        public Object Clone()
        {
            ICorDebugEnum clone = null;
            if (m_enum != null)
                m_enum.Clone(out clone);
            return new CorTypeEnumerator((ICorDebugTypeEnum)clone);
        }

        //
        // IEnumerable interface
        //
        public IEnumerator GetEnumerator()
        {
            return this;
        }

        //
        // IEnumerator interface
        //
        public bool MoveNext()
        {
            if (m_enum == null)
                return false;

            ICorDebugType[] a = new ICorDebugType[1];
            uint c = 0;
            int r = m_enum.Next((uint)a.Length, a, out c);
            if (r == 0 && c == 1) // S_OK && we got 1 new element
                m_ty = new CorType(a[0]);
            else
                m_ty = null;
            return m_ty != null;
        }

        public void Reset()
        {
            if (m_enum != null)
                m_enum.Reset();
            m_ty = null;
        }

        public void Skip(int celt)
        {
            m_enum.Skip((uint)celt);
            m_ty = null;
        }

        public Object Current
        {
            get
            {
                return m_ty;
            }
        }

        // Returns total elements in the collection.
        public int Count
        {
            get
            {
                if (m_enum == null) return 0;
                uint count = 0;
                m_enum.GetCount(out count);
                return (int)count;

            }
        }
    } 
}
