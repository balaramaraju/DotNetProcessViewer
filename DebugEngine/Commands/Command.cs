using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DebugEngine.Commands
{

    public interface IReciver {
        void Noify(Result var);
    }
    
    public abstract class Command{
        protected string m_Name;
        protected uint m_ProcessID;

        private IReciver m_Reciver { get ; set; }
        protected Command(uint _pid,string _name, IReciver _reciver) {
            m_ProcessID = _pid;
            m_Name = _name;
            m_Reciver = _reciver;
        }
        public void Execute(){
            Result result = ResultFactory.CreateResultObject(m_Name);
            result.ProcessID = m_ProcessID;
            try{
               CoreExecute(ref result);
            }catch (Exception ex){
                result.CommadStatus = false;
                result.Description = ex.Message;
                result.FailureException = ex;
            } finally {
                ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(NotifyClient), result);
            }

        }
        private void NotifyClient(Object result) {
            try{
                m_Reciver.Noify(result as Result);
            }catch (Exception) { 
                //failed to notify. yet to implement
            }
        }
        protected abstract void CoreExecute(ref Result result);
        
    }
    
 
}
