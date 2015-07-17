using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using DebugEngine;

namespace Cockpit
{
    public partial class CockPitLauncher : Form
    {
        List<int> processes = new List<int>();
        DebugFacade debugMgr = new DebugFacade();
        public CockPitLauncher()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (processList.SelectedIndex >= 0) {
                int pid = processes[processList.SelectedIndex];
                try
                {
                    Process ps = Process.GetProcessById(pid);
                    if (!ps.HasExited)
                    {
                        CockPit cockPit = new CockPit((uint)pid, debugMgr);
                        cockPit.ShowDialog();
                    }
                }
                catch (Exception ex){
                    MessageBox.Show(ex.Message);
                 /*Ignore*/
                }
                reload();
            }
        }

        private static bool IsWow64Process(Process process)
        {
            if ((Environment.OSVersion.Version.Major > 5)
                || ((Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor >= 1)))
            {
                try
                {
                    bool retVal;

                    return NativeMethods.IsWow64Process(process.Handle, out retVal) && retVal;
                }
                catch
                {
                    return false; // access is denied to the process
                }
            }

            return false; // not on 64-bit Windows
        }

        private void CockPitLauncher_Load(object sender, EventArgs e){
            reload();
        }

        void reload() {
            processes.Clear();
            processList.Items.Clear();

            int myProcessID = Process.GetCurrentProcess().Id;
            foreach (Process process in Process.GetProcesses()) {
                if (myProcessID == process.Id) { continue; }
                if ((IntPtr.Size == 4) && IsWow64Process(process)){
                    if (DebugEng.IsManagedProcess(process.Handle)){
                        processes.Add(process.Id);
                        processList.Items.Add(process.ProcessName + "<" + process.Id.ToString() + ">");
                        
                    }
                }
                else if ((IntPtr.Size == 8) && !IsWow64Process(process))
                {
                    
                        processes.Add(process.Id);
                        processList.Items.Add(process.ProcessName + "<" + process.Id.ToString() + ">");

                }
            }
            //Need to sort the list base on process names
           // processList.Items.So
        }
        private void Refresh_Click(object sender, EventArgs e){
            reload();
        }
        
    }

    internal static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);


    }
}
