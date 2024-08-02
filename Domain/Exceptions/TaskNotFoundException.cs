using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMPDomain.Exceptions
{
    public class TaskNotFoundException : Exception
    {
        public TaskNotFoundException(int taskId)
            : base($"Task with ID {taskId} was not found.")
        {

        }
    }
}
