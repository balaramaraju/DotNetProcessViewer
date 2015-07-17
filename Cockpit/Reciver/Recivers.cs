using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DebugEngine.Commands;
using Cockpit;
using System.Windows.Forms;

namespace Cockpit.Reciver
{
    class AttachReciver : IReciver
    {
        CockPit m_form;
        public AttachReciver(CockPit _form) {
            m_form = _form;
        }
        public void Noify(Result var)
        {
            AttachResult res = var as AttachResult;
            if (res.CommadStatus) {
                EnableExceptionCatcherCmd exception = 
                    new EnableExceptionCatcherCmd(
                        res.ProcessID,
                        new ExceptionCatcherReciver(m_form),
                        new ExceptionNotifacation(m_form));
                MDBGService.PostCommand(exception);
                AppDomainCmd appCmd = new AppDomainCmd(res.ProcessID, new AppDomainReciver(m_form));
                MDBGService.PostCommand(appCmd);
                EnableAssemblyEventsCmd assmLoadEvt = new EnableAssemblyEventsCmd(
                res.ProcessID,
                new EnableAssemblyLoadNotificationReciver(m_form),
                new LoadedAssemblyNotifacation(m_form)
                );
                MDBGService.PostCommand(assmLoadEvt);
                ThreadsCmd threads = new ThreadsCmd(res.ProcessID, new ThreadReciver(m_form), new ThreadNotifacation(m_form));
                MDBGService.PostCommand(threads);                
            }
        }
    }
    class DetachReciver : IReciver
    {
        public void Noify(Result var)
        {
            //throw new NotImplementedException();
        }
    }
    class ExceptionCatcherReciver : IReciver
    {
        CockPit m_form;
        public ExceptionCatcherReciver(CockPit _form){
            m_form = _form;
        }
        public void Noify(Result var){ /*Yet to implement*/}
        }
    class AppDomainReciver : IReciver
    {
        CockPit m_form;
        public AppDomainReciver(CockPit _form)
        {
            m_form = _form;
        }
        public void Noify(Result var)
        {
            AppDomainsResult res = var as AppDomainsResult;
            if (res.CommadStatus) {
                m_form.AvailableAppDomains(res.AppDomains);
                //foreach(string appDomainName in res.AppDomains){
                //    LoadedAssembliesCmd loadAssm = new LoadedAssembliesCmd(
                //        res.ProcessID,
                //        appDomainName,
                //        new LoadedAssmbliesReciver(m_form));
                //    MDBGService.PostCommand(loadAssm);
                //}

            }
        }
    }
    class LoadedAssmbliesReciver : IReciver
    {
        CockPit m_form;
        public LoadedAssmbliesReciver(CockPit _form)
        {
            m_form = _form;
        }
        public void Noify(Result var)
        {
            AssembliesResult res = var as AssembliesResult;
            if (res.CommadStatus) {
                m_form.LoadedAssemblies(res.Assemblies);
            }
        }
    }
    class ThreadReciver : IReciver
    {
        CockPit m_form;
        public ThreadReciver(CockPit _form) {
            m_form = _form;
        }
        public void Noify(Result var){
            ThreadsResult res = var as ThreadsResult;
            if (res.CommadStatus){
                m_form.AvailbleThreads(res.ThreadIDs);
            }
        }
    }
    class CallStackReciver : IReciver
    {
        CockPit m_form;
        public CallStackReciver(CockPit _form){
            m_form = _form;
        }
        public void Noify(Result var) {
            CallStackResult callStackResult = var as CallStackResult;
            StringBuilder strBuffer = new StringBuilder();
            if(callStackResult.CommadStatus){
                IList<string> stackTrace = callStackResult.CallStack;
                if (stackTrace != null){
                    foreach (string fnTrace in stackTrace){
                        strBuffer.AppendLine(fnTrace);
                    }
                }
           }else{
               strBuffer.Append("Not able to Retrive the call stack :");
               strBuffer.Append( callStackResult.Description);
           }
            m_form.DisplayStackTrace(strBuffer.ToString());
        }
    }
    class SetBPReciver : IReciver
    {
        CockPit m_form;
        public SetBPReciver(CockPit _form)
        {
            m_form = _form;
        }
        public void Noify(Result var)
        {
            BreakPointResult bpResult = var as BreakPointResult;
            if (bpResult.CommadStatus){
                string bpString = bpResult.Module + "!" + bpResult.Class + "#" + bpResult.Method;
                m_form.SetBreakPoint(bpResult.Module, bpString, bpResult.IsPending);
               
            }
        }
    }
    class GoCmdReciver : IReciver
    {
        CockPit m_CockPit;
        public GoCmdReciver(CockPit _CockPit) {
            m_CockPit = _CockPit;
        }
        public void Noify(Result var)
        {
            GoResult goCmdRs = var as GoResult;
            if (goCmdRs.CommadStatus) {
                m_CockPit.DisableGoButton();
            }
        }
    }
    class EnableAssemblyLoadNotificationReciver : IReciver
    {
        CockPit m_form;
        public EnableAssemblyLoadNotificationReciver(CockPit _form)
        {
            m_form = _form;
        }
        public void Noify(Result var)
        {
            //Do nothing for now
        }
    }
    class RemoveBPReciver : IReciver
    {
        CockPit m_form;
        public RemoveBPReciver(CockPit _form)
        {
            m_form = _form;
        }
        public void Noify(Result var){
            RemoveBreakPointResult bpResult = var as RemoveBreakPointResult;
            if (bpResult.CommadStatus){
                string bpString = bpResult.Module + "!" + bpResult.Class + "#" + bpResult.Method;
                m_form.DisableBreakPoint( bpString);
            }
        }
    }
    class ArgumaentsReciver : IReciver
    {
        Watcher m_form;
        TreeNode m_node;
        public ArgumaentsReciver(Watcher _form, TreeNode node){
            m_form = _form;
            m_node = node;
        }
        public void Noify(Result var) {
            ArgumentsResult argResult = var as ArgumentsResult;
            if (argResult.CommadStatus) {
                m_form.UpdateParamTree(argResult.Args, m_node);
            } else {
                m_form.UpdateFieldInfo(argResult.Description, m_node);
            }
        }
    }
    class MembersReciver : IReciver { 
        Watcher m_form;
        TreeNode m_node;
        public MembersReciver(Watcher _form,TreeNode _treeNode){
            m_form = _form;
            m_node = _treeNode;
        }
        public void Noify(Result var){
            MembersResult  result= var as MembersResult;
            if (result.CommadStatus){
                m_form.UpdateParamTree(result.Members, m_node);
            }else {
                m_form.UpdateFieldInfo(result.Description, m_node);
            }
        }
    }

    class ValueReciver : IReciver
    {
        Watcher m_form;
        public ValueReciver(Watcher _form)
        {
            m_form = _form;
        }
        public void Noify(Result var)
        {
            MemberValueResult result = var as MemberValueResult;
            if (result.CommadStatus)
            {
                m_form.UpdateMemberValue(result.Value);
            }
            else
            {
                m_form.UpdateMemberValue(result.Value);
            }
        }
    }
}
