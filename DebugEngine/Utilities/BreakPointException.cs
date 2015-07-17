using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugEngine.Utilities
{
    public class BreakPointException : Exception
    {
        public BreakPointException(string str) : base(str) { }
    }
}
