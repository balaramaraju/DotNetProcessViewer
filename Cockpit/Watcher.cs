using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DebugEngine.Debugee;
using DebugEngine;
using DebugEngine.Commands;
using Cockpit.Reciver;

namespace Cockpit
{
    public partial class Watcher : Form
    {
        enum NodeTypes { 
            Nope = 0,
            FunctionLevel =1,
            ComplexValue = 2,
            Primitive = 3
        }
        class TagStoreObj {
            public NodeTypes m_Type;
            public PARAMETER m_Param;
            public bool isEval;
            public bool isResutlsAvalable;
            public bool isFailed;
            public string Description;
            public TagStoreObj(NodeTypes _type, PARAMETER _param) {
                m_Type = _type;
                m_Param = _param;
                isFailed = isEval = false;
            }
        }
        BreakPointNotificationResult m_breakInfo;
        TreeNode funNode = null;
        uint m_ProcessID;
        bool isFunArgsAvail = false;
        public Watcher(BreakPointNotificationResult _breakResult, uint _processID){
            m_breakInfo = _breakResult;
            m_ProcessID = _processID;
            InitializeComponent();
        }
        //private Dictionary<string name,
        private void Watcher_Load(object sender, EventArgs e){
            funNode  = new TreeNode(m_breakInfo.Method);
            funNode.Tag = new TagStoreObj(NodeTypes.FunctionLevel, null);
            TreeNode threadNode = new TreeNode("Thread ID :" + m_breakInfo.ThreadID.ToString());
            threadNode.Nodes.Add(funNode);
            threadNode.Tag = new TagStoreObj(NodeTypes.Nope,null);
            paramTree.Nodes.Add(threadNode);
        }
        private void paramTree_AfterSelect(object sender, TreeViewEventArgs e){
            //paramValuetxt.Text = string.Empty;
            TagStoreObj arg = e.Node.Tag as TagStoreObj;
            if(
                (arg.m_Type  == NodeTypes.ComplexValue) ||
                (arg.m_Type == NodeTypes.Primitive)){
                   if (arg.isFailed){
                       paramDesc.ForeColor = Color.Red;
                       paramDesc.Text = "Error :" + arg.Description;
                    } else {
                        paramDesc.ForeColor = Color.YellowGreen;
                        paramDesc.Text = arg.m_Param.type;
                    }
                }

            if ((arg.m_Type == NodeTypes.Primitive) && arg.isResutlsAvalable) {
                if (arg.isFailed) {
                    paramValuetxt.ForeColor = Color.Red;
                }else{
                    paramValuetxt.ForeColor = Color.YellowGreen;
                }
            }
            //if (arg != null) {
            //    paramDesc.Text = arg.type;
            //    DEBUGPARAM dArg = arg as DEBUGPARAM;
            //    if (dArg.isIndexProperty && dArg.indexValue == null) { 
            //        //Enter index range.'
            //        List<PARAMETER> vars = new List<PARAMETER>();
            //        for(int index = minvalue ; index < maxvalue; index++){
            //            DEBUGPARAM dArg1 = dArg;
            //            dArg1.indexValue = index;
            //            vars.Add(dArg1);
            //        }
            //        AddMembersToNode(e.Node, vars);
            //        return;
            //    }
            //    VARIABLE _var = debugMgr.GetParamValue(dArg,threadID);
            //    if (_var.IsComplex) {
            //        AddMembersToNode(e.Node, _var.Memebers);
            //    }else if(_var.IsArray){
            //        if (_var.Memebers != null && _var.Memebers.Count > 0){
            //            AddMembersToNode(e.Node, _var.Memebers);
            //        }else {
            //            paramValuetxt.Text = _var.Value;
            //        }
            //    }
            //    else{
            //        paramValuetxt.Text = _var.Value;
            //    }
            //}
        }
        
        private void ShowContexMenu(NodeTypes _nodeType, Point location) {
            ContextMenu cm = new ContextMenu();
            MenuItem item = null;
            switch (_nodeType) { 
                case NodeTypes.Nope:
                    break;
                case NodeTypes.FunctionLevel:
                    item = new MenuItem("Get Args", GetArgs);
                    cm.MenuItems.Add(item);
                    break;
                case NodeTypes.ComplexValue:
                    item = new MenuItem("Get Members", GetMembers);
                    cm.MenuItems.Add(item);
                    break;
                case NodeTypes.Primitive:
                    item = new MenuItem("Show Value", GetValue);
                    cm.MenuItems.Add(item);
                    break;
            }
            if (item != null) {
                cm.Show(paramTree, location);
            }
        }

        private void GetArgs(object sender, EventArgs e) {
            if (!((TagStoreObj)paramTree.SelectedNode.Tag).isEval){
                GetArgumentsCmd argumentsCmd = new GetArgumentsCmd(m_ProcessID,
                    m_breakInfo.ThreadID, new ArgumaentsReciver(this, paramTree.SelectedNode));
                MDBGService.PostCommand(argumentsCmd);
                ((TagStoreObj)paramTree.SelectedNode.Tag).isEval = true;
            }
        }
        internal void UpdateParamTree(IList<PARAMETER> args, TreeNode parentNode) {
            FormCotrolHelper.ControlInvike(paramTree, () =>
            { 
                foreach(PARAMETER param in args){
                    TreeNode item = new TreeNode(param.name);
                    item.Tag = new TagStoreObj(
                        param.isComplex ? NodeTypes.ComplexValue : NodeTypes.Primitive,
                        param);
                    parentNode.Nodes.Add(item);
                    ((TagStoreObj)parentNode.Tag).isResutlsAvalable = true;
                }
                //funNode.Nodes.Add
            });
        }
        internal void UpdateMemberValue(string value)
        {
            FormCotrolHelper.ControlInvike(paramValuetxt, () =>
            {
                paramValuetxt.Text = value;
            });
        }
        private void GetMembers(object sender, EventArgs e) {
            if (!((TagStoreObj)paramTree.SelectedNode.Tag).isEval){
                GetMembersCmd membersCmd = new GetMembersCmd(
                    m_ProcessID,
                    m_breakInfo.ThreadID,
                    ((TagStoreObj)paramTree.SelectedNode.Tag).m_Param,
                    new MembersReciver(this,paramTree.SelectedNode));
                MDBGService.PostCommand(membersCmd);
                ((TagStoreObj)paramTree.SelectedNode.Tag).isEval = true;

            }

        }
        private void GetValue(object sender, EventArgs e) {
            GetValueCmd valueCmd = new GetValueCmd(
                m_ProcessID,
                m_breakInfo.ThreadID,
                ((TagStoreObj)paramTree.SelectedNode.Tag).m_Param,
                new ValueReciver(this));
            MDBGService.PostCommand(valueCmd);
                
        }
        private void paramTree_MouseUp(object sender, MouseEventArgs e){
            if(e.Button == MouseButtons.Right){
                paramTree.SelectedNode = paramTree.GetNodeAt(e.X, e.Y);
                if(paramTree.SelectedNode != null) {
                    TagStoreObj tagStoreObj = paramTree.SelectedNode.Tag as TagStoreObj;
                    ShowContexMenu(tagStoreObj.m_Type,new Point(e.X, e.Y));
                }
            }
            
        }
        public void UpdateFieldInfo(string errorMessage,TreeNode node) {

            FormCotrolHelper.ControlInvike(paramTree, () =>
            {
                ((TagStoreObj)node.Tag).isFailed = true;
                ((TagStoreObj)node.Tag).Description = errorMessage;
            });
        }

        private void paramValuetxt_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
