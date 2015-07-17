using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DebugEngine.Interfaces;
using System.Diagnostics.SymbolStore;

namespace DebugEngine.Debugee.Wrappers
{
    public class CorFunction
    {
        ICorDebugFunction corFunction;
        CorModule module;
        UInt32 token;
        public ICorDebugFunction CorFun {
            get {
                return corFunction;
            }
        }
        public CorClass Class {
            get {
                ICorDebugClass iclass;
                corFunction.GetClass(out iclass);
                return new CorClass(iclass);
            }
        }
        public CorFunction(ICorDebugFunction funtion) {
            corFunction = funtion;
            ICorDebugModule imodule = null;
            corFunction.GetModule(out imodule);
            module = new CorModule(imodule);
            corFunction.GetToken(out token);
            //corFunction.GetLocalVarSigToken(
            //ICorDebugFunctionBreakpoint breakPoint = null;
            
            //corFunction.
            
        }
        public CorBreakPoint CreateBreakPoint()
        {
            ICorDebugFunctionBreakpoint fbp = null;
            corFunction.CreateBreakpoint(out fbp);
            if (fbp != null) {
                fbp.Activate(1);
            }
            return (fbp == null) ? null : new CorBreakPoint(fbp);
        }
        public CorModule Module {
            get {
                return module;
            }
        }

        public UInt32 Token {
            get {
                return token;
            }
        }
        private bool p_symbolsInitialized = false;
        private bool p_hasSymbols = false;
        private ISymbolMethod p_symMethod;
        private Int32[] p_SPoffsets;
        private ISymbolDocument[] p_SPdocuments;
        private Int32[] p_SPstartLines, p_SPendLines, p_SPstartColumns, p_SPendColumns;
        private Int32 p_SPcount;
        /// <summary>
        /// Constant to indicate if a Sequence Point is Special.
        /// </summary>
        public const int SpecialSequencePoint = 0xfeefee;

        private void InitSymbolInformation()
        {
            if (p_symbolsInitialized)
                return;

            p_symbolsInitialized = true;

            ISymbolReader symreader = Module.GetSymReader();
            p_hasSymbols = symreader != null;
            if (p_hasSymbols)
            {
                ISymbolMethod sm = null;
                sm = symreader.GetMethod(new SymbolToken((Int32)Token)); 
                if (sm == null)
                {
                    p_hasSymbols = false;
                    return;
                }
                p_symMethod = sm;
                p_SPcount = p_symMethod.SequencePointCount;
                p_SPoffsets = new Int32[p_SPcount];
                p_SPdocuments = new ISymbolDocument[p_SPcount];
                p_SPstartLines = new Int32[p_SPcount];
                p_SPendLines = new Int32[p_SPcount];
                p_SPstartColumns = new Int32[p_SPcount];
                p_SPendColumns = new Int32[p_SPcount];

                p_symMethod.GetSequencePoints(p_SPoffsets, p_SPdocuments, p_SPstartLines,
                                                p_SPstartColumns, p_SPendLines, p_SPendColumns);
            }
        }
        
        public CorSourcePosition GetSourcePositionFromIP(Int32 ip)
        {
            InitSymbolInformation();
            
            if (!p_hasSymbols)
                return null;

            if (p_SPcount > 0 && p_SPoffsets[0] <= ip)
            {
                Int32 i;
                // find a sequence point that the given instruction
                // pointer belongs to
                for (i = 0; i < p_SPcount; i++)
                {
                    if (p_SPoffsets[i] >= ip)
                        break;
                }

                // ip does not belong to any sequence point
                if (i == p_SPcount || p_SPoffsets[i] != ip)
                    i--;

                CorSourcePosition sp = null;
                if (p_SPstartLines[i] == SpecialSequencePoint)
                {
                    // special type of sequence point
                    // it indicates that the source code 
                    // for this part is hidden from the debugger

                    // search backward for the last known line 
                    // which is not a special sequence point
                    Int32 noSpecialSequencePointInd = i;
                    while (--noSpecialSequencePointInd >= 0)
                        if (p_SPstartLines[noSpecialSequencePointInd] != SpecialSequencePoint)
                            break;

                    if (noSpecialSequencePointInd < 0)
                    {
                        // if not found in backward search
                        // search forward for the first known line
                        // which is not a special sequence point
                        noSpecialSequencePointInd = i;
                        while (++noSpecialSequencePointInd < p_SPcount)
                            if (p_SPstartLines[noSpecialSequencePointInd] != SpecialSequencePoint)
                                break;
                    }

                    
                    if (noSpecialSequencePointInd < p_SPcount)
                    {
                        sp = new CorSourcePosition(true,
                                                   p_SPdocuments[noSpecialSequencePointInd].URL,
                                                   p_SPstartLines[noSpecialSequencePointInd],
                                                   p_SPendLines[noSpecialSequencePointInd],
                                                   p_SPstartColumns[noSpecialSequencePointInd],
                                                   p_SPendColumns[noSpecialSequencePointInd]);
                    }
                }
                else
                {
                    sp = new CorSourcePosition(false, p_SPdocuments[i].URL, p_SPstartLines[i], p_SPendLines[i],
                                                p_SPstartColumns[i], p_SPendColumns[i]);
                }
                return sp;
            }
            return null;
        }
        
    }
}
