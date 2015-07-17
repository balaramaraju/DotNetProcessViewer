using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DebugEngine.Debugee.Wrappers;

namespace DebugEngine.Utilities
{
    class DebugCache
    {
        public static IDictionary<string, CorModule> LoadedModules =
            new System.Collections.Concurrent.ConcurrentDictionary<string, CorModule>();
    }
}
