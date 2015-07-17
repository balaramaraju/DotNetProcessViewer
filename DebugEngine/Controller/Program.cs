using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DebugEngine;
using DebugEngine.Debugee;

namespace Controller
{
    class Program
    {
        static DebugFacade debugMgr = new DebugFacade();
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                debugMgr.RegisterForAssemblyLoad_Unload(AssemblyNotifiaction);
                debugMgr.RegisterForTreadCreate_TerminateNotification(ThreadNotification);
                debugMgr.OpenDebugSession(Convert.ToUInt32(args[0]));
                
            }
            Console.ReadLine();
        }
        static private void AssemblyNotifiaction(ASSEMBLY assm)
        {
            Console.WriteLine(assm.Name);
        }

        static private void ThreadNotification(THREAD thread)
        {
            Console.WriteLine("Thread" + thread.Id);
        }
    }
}
