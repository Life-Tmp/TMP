using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMPDomain.Exceptions
{
    public class TaskAlreadyExistsException : Exception
    {
        public TaskAlreadyExistsException(string taskName)
            : base($"A task with the name '{taskName}' already exists.")
        {
        }
    }
}
