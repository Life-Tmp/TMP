using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMPDomain.Exceptions
{
    public class ProjectNotFoundException : Exception
    {
        public ProjectNotFoundException(int projectId)
            : base($"Project with ID {projectId} not found.")
        {

        }
    }
}
