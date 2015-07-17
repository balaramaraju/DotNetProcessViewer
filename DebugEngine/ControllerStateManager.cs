using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DebugEngine.Interfaces;

namespace DebugEngine.Utilities
{
    internal enum ControllerState{
        Running,
        Paused
    }
    class ControllerStateObject {
        ICorDebugController m_Controller;
        bool inCallBack;
        ControllerState currentstate;
        internal void SetState(ControllerState cs) {
            lock (this){
                if (!inCallBack)
                {
                    switch (cs)
                    {
                        case ControllerState.Running:
                            if (currentstate == ControllerState.Running)
                            {
                                return;
                            }
                            if (currentstate == ControllerState.Paused)
                            {
                                m_Controller.Continue(0);
                                currentstate = ControllerState.Running;
                            }
                            break;
                        case ControllerState.Paused:
                            if (currentstate == ControllerState.Paused)
                            {
                                return;
                            }
                            if (currentstate == ControllerState.Running)
                            {
                                m_Controller.Stop(0);
                                currentstate = ControllerState.Paused;
                            }
                            break;
                    }
                }
            }
        }
        internal void RaiseCallBackFlag() {
            lock (this){
                //if (currentstate == ControllerState.Paused) {
                //    //throw new Exception("Not in Valid state");
                //}
                currentstate = ControllerState.Paused;
                inCallBack = true;
            }
        }
        internal void ReleaseCallBackFlag() {
            lock (this){
                if (inCallBack != true){
                    throw new Exception("Not in Valid callBack state");
                }
                inCallBack = false;
                currentstate = ControllerState.Running;
                m_Controller.Continue(0);
            }
        }
        internal ControllerStateObject(ICorDebugController contoller) {
            m_Controller = contoller;
        }
    }
    public static class ControllerStateMgr{
        static ControllerStateObject stateObj;
        //This need to be called in Process Creaded notification
        public static void SetStateObj(ICorDebugController value) {
            if (stateObj != null) { throw new Exception("Already initialized"); }
            stateObj = new ControllerStateObject(value);
        }
        private static void CheckStateObject() {
            if (stateObj == null) {
                throw new Exception("State Object in not initialized");
            }
        }
        internal static void SetState(ControllerState cs) {
            CheckStateObject();
            stateObj.SetState(cs);
        }
        internal static void RaiseCallBackFlag()
        {
            CheckStateObject();
            stateObj.RaiseCallBackFlag();
        }
        internal static void ReleaseCallBackFlag()
        {
            CheckStateObject();
            stateObj.ReleaseCallBackFlag();
        }
        public static void ReleaseStateObj() {
            stateObj = null;
        }
    }
}
