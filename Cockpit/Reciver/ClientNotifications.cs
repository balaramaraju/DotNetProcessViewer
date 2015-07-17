using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DebugEngine.Commands;

namespace Cockpit.Reciver
{
    class ThreadNotifacation : INotification {
        CockPit m_cockPit;
        public ThreadNotifacation(CockPit _cockPit) {
            m_cockPit = _cockPit;
        }
        public void FireEvent(NotificationResult _result) {
            ThreadNotificationResult result = _result as ThreadNotificationResult;
            m_cockPit.UpdateThreadStatus(result.ThreadID, result.IsCreated);
        }
    }

    class ExceptionNotifacation : INotification
    {
        CockPit m_cockPit;
        public ExceptionNotifacation(CockPit _cockPit){
            m_cockPit = _cockPit;
        }
        public void FireEvent(NotificationResult _result){
            ExceptioNotificationResult result = _result as ExceptioNotificationResult;
            StringBuilder strBuffer = new StringBuilder();
            strBuffer.AppendLine();
            strBuffer.AppendLine("Thread ID : " + result.ThreadID);
            strBuffer.AppendLine("Type : "+ result.ExceptionType);
            //strBuffer.AppendLine("Line : " + result.SourceLine);
            strBuffer.AppendLine("StackTrace : ");
            foreach (string trace in result.StackTrace) { 
                strBuffer.AppendLine(trace);
            }
            strBuffer.AppendLine("Mode : " + result.State.ToString());
            m_cockPit.DisplayException(strBuffer.ToString());
        }
    }
    class BreakPointHitNotifacation : INotification{
        CockPit m_cockPit;
        public BreakPointHitNotifacation(CockPit _cockPit){
            m_cockPit = _cockPit;
        }
        public void FireEvent(NotificationResult _result){
            BreakPointNotificationResult bpNtfyResult = _result as BreakPointNotificationResult;
            m_cockPit.SetBreakPointInformation(bpNtfyResult);
            m_cockPit.DisplayStackTrace(string.Empty);
        }
    }
    class LoadedAssemblyNotifacation : INotification
    {
        CockPit m_cockPit;
        public LoadedAssemblyNotifacation(CockPit _cockPit){
            m_cockPit = _cockPit;
        }
        public void FireEvent(NotificationResult _result){
            LoadedAssemblyNotificationResult bpNtfyResult = _result as LoadedAssemblyNotificationResult;
            m_cockPit.UpdateLoadedAssembly(bpNtfyResult.FullName, bpNtfyResult.AppDomainName);
        }
    }
}
