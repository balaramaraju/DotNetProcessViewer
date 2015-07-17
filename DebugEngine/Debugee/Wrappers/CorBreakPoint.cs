using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DebugEngine.Interfaces;

namespace DebugEngine.Debugee.Wrappers
{
    public class CorBreakPoint
    {
        ICorDebugFunctionBreakpoint breakPoint;
        public CorBreakPoint(ICorDebugFunctionBreakpoint _bp) {
            breakPoint = _bp;
        }
        public void DeActivate(){
            breakPoint.Activate(0);
        }
        public void Activate(){
            breakPoint.Activate(1);
        }
    }
}
