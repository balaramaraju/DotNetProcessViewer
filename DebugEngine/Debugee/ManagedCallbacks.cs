using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DebugEngine.Interfaces;
using System.Diagnostics;
using System.Threading;
using DebugEngine.Debugee.Wrappers;
using DebugEngine.Utilities;
using DebugEngine.MetaDataUtils;

using DebugEngine.Commands;

namespace DebugEngine.Debugee
{
    public class ManagedCallback : ICorDebugManagedCallback, ICorDebugManagedCallback2, ICorDebugManagedCallback3
    {
        DebugEng blackBoard = null;
        MDBGEventListner m_listner;
        public ManagedCallback(DebugEng dashBoard) {
            blackBoard = dashBoard;
        }
        internal ManagedCallback(MDBGEventListner listner) {
            m_listner = listner;
        }

        ICorDebugController controller = null;
        public void Breakpoint(
            ICorDebugAppDomain pAppDomain, 
            ICorDebugThread pThread, 
            ICorDebugBreakpoint pBreakpoint){
                m_listner.PostBreakPoint(new CorThread(pThread));
            //controller.Continue(0);
        }

        public void StepComplete(ICorDebugAppDomain pAppDomain, ICorDebugThread pThread, ICorDebugStepper pStepper, CorDebugStepReason reason)
        {
            controller.Continue(0);
        }

        public void Break(ICorDebugAppDomain pAppDomain, ICorDebugThread thread){
            m_listner.PostBreakPoint(new CorThread(thread));
            //controller.Continue(0);
        }

        public void Exception(ICorDebugAppDomain pAppDomain, ICorDebugThread pThread, int unhandled)
        {
            controller.Continue(0);
        }

        public void EvalComplete(ICorDebugAppDomain pAppDomain, ICorDebugThread pThread, ICorDebugEval pEval)
        {
            controller.Continue(0);
        }

        public void EvalException(ICorDebugAppDomain pAppDomain, ICorDebugThread pThread, ICorDebugEval pEval)
        {
            controller.Continue(0);
        }

        public void CreateProcess(ICorDebugProcess pProcess)
        {
            controller = pProcess;
            controller.Continue(0);
            
            
        }

        public void ExitProcess(ICorDebugProcess pProcess)
        {
            controller.Continue(0);
        }

        public void CreateThread(ICorDebugAppDomain pAppDomain, ICorDebugThread thread)
        {
            m_listner.PostThreadNotification(new CorDebugThread(thread), true);
            controller.Continue(0);
        }

        public void ExitThread(ICorDebugAppDomain pAppDomain, ICorDebugThread thread)
        {
            m_listner.PostThreadNotification(new CorDebugThread(thread), false);
            controller.Continue(0);
        }

        public void LoadModule(ICorDebugAppDomain pAppDomain, ICorDebugModule pModule)
        {
            //Fix Needed : to take care if same module loaded in diffent appDomains.
            //m_listner.LoadedNewAssembly(new CorModule(pModule));
            
            controller.Continue(0);
        }

        public void UnloadModule(ICorDebugAppDomain pAppDomain, ICorDebugModule pModule)
        {
            controller.Continue(0);
        }

        public void LoadClass(ICorDebugAppDomain pAppDomain, ICorDebugClass c)
        {
            controller.Continue(0);
        }

        public void UnloadClass(ICorDebugAppDomain pAppDomain, ICorDebugClass c)
        {
            controller.Continue(0);
        }

        public void DebuggerError(ICorDebugProcess pProcess, int errorHR, uint errorCode)
        {
            controller.Continue(0);
        }

        public void LogMessage(ICorDebugAppDomain pAppDomain, ICorDebugThread pThread, int lLevel, string pLogSwitchName, string pMessage)
        {
            controller.Continue(0);
        }

        public void LogSwitch(ICorDebugAppDomain pAppDomain, ICorDebugThread pThread, int lLevel, uint ulReason, string pLogSwitchName, string pParentName)
        {
            controller.Continue(0);
        }

        public void CreateAppDomain(ICorDebugProcess pProcess, ICorDebugAppDomain pAppDomain)
        {
            pAppDomain.Attach();
            controller.Continue(0);
        }

        public void ExitAppDomain(ICorDebugProcess pProcess, ICorDebugAppDomain pAppDomain)
        {
            controller.Continue(0);
        }

        public void LoadAssembly(ICorDebugAppDomain pAppDomain, ICorDebugAssembly pAssembly)
        {
            m_listner.LoadedNewAssembly(new CorAssembly(pAssembly), new CorDebugAppDomain(pAppDomain));
            controller.Continue(0);
        }

        public void UnloadAssembly(ICorDebugAppDomain pAppDomain, ICorDebugAssembly pAssembly)
        {
            controller.Continue(0);
        }

        public void ControlCTrap(ICorDebugProcess pProcess)
        {
            controller.Continue(0);
        }

        public void NameChange(ICorDebugAppDomain pAppDomain, ICorDebugThread pThread)
        {
            controller.Continue(0);
        }

        public void UpdateModuleSymbols(ICorDebugAppDomain pAppDomain, ICorDebugModule pModule, System.Runtime.InteropServices.ComTypes.IStream pSymbolStream)
        {
            controller.Continue(0);
        }

        public void EditAndContinueRemap(ICorDebugAppDomain pAppDomain, ICorDebugThread pThread, ICorDebugFunction pFunction, int fAccurate)
        {
            controller.Continue(0);
        }

        public void BreakpointSetError(ICorDebugAppDomain pAppDomain, ICorDebugThread pThread, ICorDebugBreakpoint pBreakpoint, uint dwError)
        {
            controller.Continue(0);
        }

        public void FunctionRemapOpportunity(ICorDebugAppDomain pAppDomain, ICorDebugThread pThread, ICorDebugFunction pOldFunction, ICorDebugFunction pNewFunction, uint oldILOffset)
        {
            controller.Continue(0);
        }

        public void CreateConnection(ICorDebugProcess pProcess, uint dwConnectionId, ref ushort pConnName)
        {
            controller.Continue(0);
        }

        public void ChangeConnection(ICorDebugProcess pProcess, uint dwConnectionId)
        {
            controller.Continue(0);
        }

        public void DestroyConnection(ICorDebugProcess pProcess, uint dwConnectionId)
        {
            controller.Continue(0);
        }

        public void Exception(
            ICorDebugAppDomain pAppDomain, 
            ICorDebugThread pThread, 
            ICorDebugFrame pFrame, 
            uint nOffset, 
            CorDebugExceptionCallbackType dwEventType, 
            uint dwFlags){
            m_listner.PostExceptionNotification(
                new CorThread(pThread), 
                new CorFunctionFrame(pFrame), 
                dwEventType, 
                dwFlags);
            controller.Continue(0);
        }

        public void ExceptionUnwind(ICorDebugAppDomain pAppDomain, ICorDebugThread pThread, CorDebugExceptionUnwindCallbackType dwEventType, uint dwFlags)
        {
            controller.Continue(0);
        }

        public void FunctionRemapComplete(ICorDebugAppDomain pAppDomain, ICorDebugThread pThread, ICorDebugFunction pFunction)
        {
            controller.Continue(0);
        }

        public void MDANotification(ICorDebugController pController, ICorDebugThread pThread, ICorDebugMDA pMDA)
        {
            controller.Continue(0);
        }

        public void CustomNotification(ICorDebugThread pThread, ICorDebugAppDomain pAppDomain)
        {
            controller.Continue(0);
        }
    }
}
