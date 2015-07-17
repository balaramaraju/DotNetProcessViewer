using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DebugEngine.Interfaces;

namespace DebugEngine
{
    class TempFuncEval
    {
        internal static AutoResetEvent mEvent = new AutoResetEvent(false);
        internal static ICorDebugValue value = null;
    }
}
