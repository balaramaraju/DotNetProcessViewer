using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DebugEngine;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Cockpit
{
    static class Program
    {
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
        private const int ATTACH_PARENT_PROCESS = -1;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main(string[] args)
        {
            DebugFacade debugMgr = new DebugFacade();
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            CockPitLauncher form = new CockPitLauncher();
                    Application.Run(form);
        }

       
    }
    
}
