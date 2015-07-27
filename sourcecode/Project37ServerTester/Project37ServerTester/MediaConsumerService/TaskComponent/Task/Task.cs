using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Project37ServerTester.MediaConsumerService.TaskComponent
{
    public interface ITaskDelegate
    {
        void HandleTaskCompleted(Task task);
    }

    public class Task 
    {
        ITaskDelegate TaskDelegate{get; set;}

        protected void NotifyCompleted(Task task)
        {
            TaskDelegate.HandleTaskCompleted(task);
        }

        public virtual void Exectute(){}
    }
}
