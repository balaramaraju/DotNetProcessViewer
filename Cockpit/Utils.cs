using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cockpit
{
    internal class Utils
    {
        public static string getBPuid(string _module, string _className, string _methodName)
        {
            StringBuilder strID = new StringBuilder();
            strID.Append(_module);
            strID.Append("!");
            strID.Append(_className);
            strID.Append("#");
            strID.Append(_methodName);
            return strID.ToString();
        }
    }
}
