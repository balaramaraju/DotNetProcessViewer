using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DebugEngine;
using DebugEngine.Debugee;
using System.Diagnostics;
using DebugEngine.Utilities;
using DebugEngine.Commands;
using Cockpit.Reciver;
using DebugEngine.Debugee.Wrappers;
using System.Collections.Concurrent;

namespace Cockpit
{
    public partial class CockPit : Form
    {
        static DebugFacade debugMgr;
        uint ProcessID;
        bool isProcessExited;
        IDictionary<string, bool> bpState = new Dictionary<string, bool>();
        public CockPit(uint processID , DebugFacade facade){
            ProcessID = processID;
            debugMgr = facade;
            AttachCmd attach = new AttachCmd(processID, new AttachReciver(this));
            InitializeComponent();
            MDBGService.PostCommand(attach);
        }

        private bool isDebugMode = false;
        private void Monitor_Click(object sender, EventArgs e){
            
            Debug(!isDebugMode);
            isDebugMode = !isDebugMode;
            //Monitor.Text =  isDebugMode ? "Detach" : "Monitor";
        }
        private void Debug(bool isEnable){
            if (isEnable){
                threadList.Items.Clear();
                assembliesList.Items.Clear();
                exceptionWindow.Text = string.Empty;
                threadStackTrace.Text = string.Empty;
                //debugMgr.RegisterForAssemblyLoad_Unload(AssemblyNotifiaction);
                //debugMgr.RegisterForTreadCreate_TerminateNotification(ThreadNotification);
                //debugMgr.RegisterForExcpetionCatcher(ExceptionCatcher);
                //debugMgr.RegiesterForProcessTermintatedNotification(ProcessTerminated);
                //debugMgr.RegisterForBreakPointNotification(BreakPoint);
                debugMgr.OpenDebugSession(ProcessID);
                
            }else {
                debugMgr.UnRegisterForAssemblyLoad_Unload(AssemblyNotifiaction);
                debugMgr.UnRegisterForTreadCreate_TerminateNotification(ThreadNotification);
                //debugMgr.UnRegisterForExcpetionCatcher(ExceptionCatcher);
                debugMgr.UnregiesterForProcessTermintatedNotification(ProcessTerminated);
                debugMgr.CloseDebugSession();
                
            }
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            assembliesList.Columns.Add("Name");
            assembliesList.Columns.Add("Path");
            //breakPointText.Enabled = false;
            //CreateBreakPointbt.Enabled = false;
            assembliesList.Columns[0].Width = 300;
            assembliesList.Columns[1].Width = 500;
            threadList.Columns.Add("ID");
            threadList.Columns.Add("State");
            threadList.Columns[0].Width = 95;
            threadList.Columns[1].Width = 70;
            BreakPointList.Columns.Add("Active Break Points");
            //BreakPointList.Columns.Add("State");
            BreakPointList.Columns[0].Width = 350;
            //BreakPointList.Columns[1].Width = 100;
            GoButton.Enabled = false;
            ShowValButton.Enabled = false;
            Process proces = Process.GetProcessById((int)ProcessID);
            this.Text = "Cockpit {" + proces.ProcessName + " <"+proces.Id + "> }";
        }

        private void threadList_MouseDoubleClick(object sender, MouseEventArgs e){
            StringBuilder strBuffer = new StringBuilder();
            uint threadid = Convert.ToUInt32(threadList.SelectedItems[0].Text);
            CallStackCmd stackCommand = new CallStackCmd(ProcessID, threadid, new CallStackReciver(this));
            MDBGService.PostCommand(stackCommand);
        }
        //private void threadList_MouseDoubleClick(object sender, MouseEventArgs e)
        //{
        //    StringBuilder strBuffer = new StringBuilder();
        //    uint threadid = Convert.ToUInt32(threadList.SelectedItems[0].Text);
        //    if (isProcessExited || !debugMgr.IsAttached)
        //    {
        //        strBuffer.AppendLine("Process is Terminated/Detached. Below is last seen StackTrace.");
        //        string stkTrace = null;
        //        if (CockPitCache.stackTraceInfo.TryGetValue(threadid, out stkTrace))
        //        {
        //            strBuffer.AppendLine(stkTrace);
        //        }
        //        threadStackTrace.ForeColor = Color.Pink;
        //    }
        //    else
        //    {
        //        strBuffer.AppendLine("Stacktrace for thread : " + threadid + " @ " + DateTime.Now);
        //        strBuffer.Append("\n_________________________________________________\n");
        //        List<string> stackTrace = debugMgr.GetStackTrace(threadid);
        //        if (stackTrace != null)
        //        {
        //            foreach (string fnTrace in stackTrace)
        //            {
        //                strBuffer.AppendLine(fnTrace);
        //            }
        //        }
        //        threadStackTrace.ForeColor = Color.LightGreen;
        //        if (CockPitCache.stackTraceInfo.ContainsKey(threadid))
        //        {
        //            CockPitCache.stackTraceInfo[threadid] = threadStackTrace.Text;
        //        }
        //        else
        //        {
        //            CockPitCache.stackTraceInfo.Add(threadid, threadStackTrace.Text);
        //        }
        //    }

        //    threadStackTrace.Text = strBuffer.ToString();
        //}
        private void threadList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void AssemblyNotifiaction(ASSEMBLY assm) {
            if (assm.Activity == ASSEMBLYACTION.LOAD){
                FormCotrolHelper.ControlInvike(assembliesList, () =>
                {
                    ListViewItem[] items = assembliesList.Items.Find(assm.Name, false);
                    if (items == null || items.Length == 0 ){
                        ListViewItem item = new ListViewItem(assm.Name);
                        item.Name = assm.Name;
                        item.Text = assm.Name;
                        item.SubItems.Add(assm.Path);

                        item.ForeColor = Color.GreenYellow;
                        assembliesList.Items.Add(item);
                    }else{
                        items[0].ForeColor = Color.GreenYellow;
                    }
                    
                });
            }else{
                FormCotrolHelper.ControlInvike(threadList, () =>
                {
                    ListViewItem item = assembliesList.Items[assm.Name];
                    item.ForeColor = Color.Red;
                    //item.SubItems[3].Text = assm.UnloadedTime.ToString();
                });
        }
    }
        private void ThreadNotification(THREAD thread){
            if (thread.State != THREADSTATE.TERMINATED){
                FormCotrolHelper.ControlInvike(threadList, () => {
                    ListViewItem item = new ListViewItem(thread.Id.ToString().Trim());
                    item.Name = thread.Id.ToString().Trim();
                    item.Text = thread.Id.ToString().Trim();
                    item.SubItems.Add("Alive");
                    item.ForeColor = Color.GreenYellow;
                    threadList.Items.Add(item);
                });
            }else{
                FormCotrolHelper.ControlInvike(threadList, () => {
                    ListViewItem[] item = threadList.Items.Find(thread.Id.ToString().Trim(), true);
                    if (item != null)
                    {
                        item[0].ForeColor = Color.Red;
                        item[0].SubItems[1].Text = "Dead";
                    }
                });
            }
        }
        
        private void ProcessTerminated(PROCESS process) {
            isProcessExited = process.terminated;
            //FormCotrolHelper.ControlInvike(Monitor, () =>
            //{ 
            //    Monitor.Enabled = false;
            //    this.Text += "Process Termintated";
            //});
            
        }
        private void BreakPoint(BREAKPOINT bp) {
            //debugMgr.FreezeDebugee();
            isFreezed = true;
            
            FormCotrolHelper.ControlInvike(breakpointStatus, () =>
            {
                breakpointStatus.Text = "Break point on thread id " + bp.Threadid + "\n" + bp.frame;
            });
            //FormCotrolHelper.ControlInvike(FreezeBtn, () =>
            //{
            //    isFreezed = true;
            //    FreezeBtn.Text = "GO";
            //});
            FormCotrolHelper.ControlInvike(threadStackTrace, () =>
            {
                threadStackTrace.Text = string.Empty;
            });
        }
        bool isFreezed = false;
        

        

       

        private void CreateBreakPointbt_Click(object sender, EventArgs e)
        {
            try
            {
                string bkTest = breakPointText.Text.Trim();
                Command setBP = CreateBPCommand(bkTest, true);
                MDBGService.PostCommand(setBP);
            }
            catch (Exception) { 
            
            }
  
        }

        private Command CreateBPCommand(string identifier, bool set) {
            int moduledelimit = identifier.IndexOf('!');
            int classdelimit = identifier.IndexOf('#');
            string Module = identifier.Substring(0, moduledelimit);
            string Class = identifier.Substring(moduledelimit + 1, classdelimit - (moduledelimit + 1));
            string Function = identifier.Substring(classdelimit + 1);
            if (set)
            {
                return new SetBreakPointCmd(
                    ProcessID,
                    Module,
                    Class,
                    Function, 0,
                    new SetBPReciver(this),
                    new BreakPointHitNotifacation(this));
            }
            else {
                return new RemoveBreakPointCmd(
                    ProcessID,
                    Module,
                    Class,
                    Function, 0,
                    new RemoveBPReciver(this));
            }
        }
        private void BreakPointList_DoubleClick(object sender, EventArgs e){
            
            if (BreakPointList.Items != null && BreakPointList.Items.Count > 0){
                string bpstring = BreakPointList.SelectedItems[0].Text;
                //int moduledelimit = bpTest.IndexOf('!');
                //int classdelimit = bpTest.IndexOf('#');
                //string Module = bpTest.Substring(0, moduledelimit);
                //string Class = bpTest.Substring(moduledelimit + 1, classdelimit - (moduledelimit + 1));
                //string Function = bpTest.Substring(classdelimit + 1);
                Command cmd = null;
                bool isCreate = Convert.ToBoolean(BreakPointList.SelectedItems[0].Tag);
                cmd = CreateBPCommand(bpstring,!isCreate);
                MDBGService.PostCommand(cmd);
                //

            }
            
        }

        private bool IsBPActivated() {
            return true;
        }

        public void AvailableAppDomains(IList<string> appDomains) {
            FormCotrolHelper.ControlInvike(appDomainCb, () =>
            {
                appDomainCb.Items.Clear();
                foreach (string appdomain in appDomains) {
                    appDomainCb.Items.Add(appdomain);
                }
                //item.SubItems[3].Text = assm.UnloadedTime.ToString();
            });
        }
        public void AvailbleThreads(IList<uint> ThreadIDs) {
            FormCotrolHelper.ControlInvike(threadList, () =>
            {
                threadList.Items.Clear();
                foreach (uint tid in ThreadIDs)
                {
                    ListViewItem item = new ListViewItem(tid.ToString().Trim());
                    item.Name = tid.ToString().Trim();
                    item.Text = tid.ToString().Trim();
                    item.SubItems.Add("Alive");
                    item.ForeColor = Color.GreenYellow;
                    threadList.Items.Add(item);
                }
            });
        }
        public void UpdateThreadStatus(uint threadID, bool isCreated) {
            FormCotrolHelper.ControlInvike(threadList, () =>
            {
                if (!isCreated){
                    foreach (ListViewItem item in threadList.Items){
                        if (item.Name == threadID.ToString()){
                            item.SubItems[1].Text = "Dead";
                            item.ForeColor = Color.Red;
                            break;
                        }
                    }
                }else{
                    ListViewItem item = new ListViewItem(threadID.ToString().Trim());
                    item.Name = threadID.ToString().Trim();
                    item.Text = threadID.ToString().Trim();
                    item.SubItems.Add("Alive");
                    item.ForeColor = Color.GreenYellow;
                    threadList.Items.Add(item);
                }
            });
        }
        public void LoadedAssemblies(IList<string> assmblies)
        {
            FormCotrolHelper.ControlInvike(assembliesList, () =>
            {
                foreach (string assm in assmblies)
                {
                    int index = assm.LastIndexOf("\\");
                    string moduleName = assm.Substring(index+1);
                    string modulePath = assm.Substring(0,index+1);
                    ListViewItem item = new ListViewItem(moduleName);
                    item.Name = moduleName;
                    item.Text = moduleName;
                    item.SubItems.Add(modulePath);
                    item.ForeColor = Color.GreenYellow;
                    assembliesList.Items.Add(item);
                }
            });
        }
        private void loadedAssmbt_Click(object sender, EventArgs e)
        {
            LoadedAssembliesCmd loadAssm = new LoadedAssembliesCmd(
                        ProcessID,
                        appDomainCb.SelectedItem.ToString().Trim(),
                        new LoadedAssmbliesReciver(this)
                        );
            MDBGService.PostCommand(loadAssm);

            
        }
        public void DisplayStackTrace(string str) {
            FormCotrolHelper.ControlInvike(threadStackTrace, () => {
                //threadStackTrace.Text = string.Empty;
                threadStackTrace.Text = str;
            });
        }
        private string breakLine = "********************************";
        public void DisplayException(string str){
            FormCotrolHelper.ControlInvike(exceptionWindow, () =>  {
                exceptionWindow.Text += breakLine + str;
            });
        }
        IDictionary<string, List<string>> pendingBreakPoints = new ConcurrentDictionary<string, List<string>>();
        public void SetBreakPoint(string module, string bpLocation, bool isPending) {
            FormCotrolHelper.ControlInvike(breakPointText, () => breakPointText.Clear());
            FormCotrolHelper.ControlInvike(BreakPointList, () =>
            {
                if (BreakPointList.Items.ContainsKey(bpLocation)) {
                    BreakPointList.Items.RemoveByKey(bpLocation);
                }
                ListViewItem item = new ListViewItem(bpLocation);
                item.Name = bpLocation;
                item.Text = bpLocation;
                item.Tag = true;
                item.ForeColor = (isPending) ? Color.Pink : Color.GreenYellow;
                BreakPointList.Items.Add(item);
                
            });
            if (isPending) {
                List<string> bps = null;
                if (pendingBreakPoints.TryGetValue(module, out bps)) {
                    bps.Add(bpLocation);
                }else{
                    bps = new List<string>();
                    bps.Add(bpLocation);
                    pendingBreakPoints.Add(module, bps);
                }
            }
        }

        public void DisableBreakPoint( string bpLocation){
            FormCotrolHelper.ControlInvike(BreakPointList, () =>{
                if (BreakPointList.Items.ContainsKey(bpLocation)){
                    ListViewItem item = BreakPointList.Items[bpLocation];
                    item.Tag = false;
                    item.ForeColor = Color.RosyBrown;
                }
            });
        }
        
        
        public void SetBreakPointInformation(BreakPointNotificationResult result) {
            
            FormCotrolHelper.ControlInvike(breakpointStatus, () =>
            {
                breakpointStatus.Text += "Break happend on Thread " + result.ThreadID + "\n" +
                    "@ " + Utils.getBPuid(
                    result.Module,
                    result.Class, 
                    result.Method) + ";";
                breakpointStatus.Tag = result;
            });
            FormCotrolHelper.ControlInvike(GoButton, () =>
            {
                GoButton.Enabled = true;
                ShowValButton.Enabled = true;
            });
        }
        public void DisableGoButton()
        {
            FormCotrolHelper.ControlInvike(breakpointStatus, () =>
            {
                breakpointStatus.Text = string.Empty;
            });
            FormCotrolHelper.ControlInvike(GoButton, () =>
            {
                GoButton.Enabled = false;
                ShowValButton.Enabled = false;
            });
        }
        private void CockPit_FormClosing(object sender, FormClosingEventArgs e){
            DetachCmd detach = new DetachCmd(ProcessID, new DetachReciver());
            MDBGService.PostCommand(detach);
        }

        private void GoButton_Click(object sender, EventArgs e){
            GoCmd go = new GoCmd(ProcessID, new GoCmdReciver(this));
            MDBGService.PostCommand(go);
        }

        internal void UpdateLoadedAssembly(string _assmname,string _appDomain) {
            int lastIndex = _assmname.LastIndexOf('\\');
            string assmLocation = _assmname.Substring(0,lastIndex);
            string assmName = _assmname.Substring(lastIndex + 1);
            string activeAppDomain = string.Empty;
            
            
            FormCotrolHelper.ControlInvike(appDomainCb, () =>
            {
                if (appDomainCb.SelectedIndex > 0) {
                    activeAppDomain = appDomainCb.SelectedItem.ToString().Trim();
                }
            });
            if (activeAppDomain == _appDomain){
                FormCotrolHelper.ControlInvike(assembliesList, () =>
                {
                    ListViewItem item = new ListViewItem(assmName);
                    item.Name = assmName;
                    item.Text = assmName;
                    item.SubItems.Add(assmLocation);
                    item.ForeColor = Color.GreenYellow;
                    assembliesList.Items.Add(item);
                    
                });
            }

            //Check for pending breakpoints
            List<string> pendingBreakPoint = null;
            string moduleName = assmName.Substring(0, assmName.LastIndexOf('.'));
            if (pendingBreakPoints.TryGetValue(moduleName, out pendingBreakPoint)){
                foreach (string bpString in pendingBreakPoint) {
                    Command setBP = CreateBPCommand(bpString,true);
                    MDBGService.PostCommand(setBP);
                }
            }
        }
        private void ShowValButton_Click(object sender, EventArgs e){
            BreakPointNotificationResult bpRes 
                = breakpointStatus.Tag as BreakPointNotificationResult;
            Watcher wth = new Watcher(bpRes,ProcessID);
            wth.ShowDialog();
        }
    }

    class FormCotrolHelper
    {
        delegate void UniversalVoidDelegate();

        /// <summary>
        /// Call form controll action from different thread
        /// </summary>
        public static void ControlInvike(Control control, Action function)
        {
            if (control.IsDisposed || control.Disposing)
                return;

            if (control.InvokeRequired)
            {
                control.Invoke(new UniversalVoidDelegate(() => ControlInvike(control, function)));
                return;
            }
            function();
        }
    }
}
