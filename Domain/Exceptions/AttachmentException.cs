using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMPDomain.Exceptions
{
    public class AttachmentException : Exception
    {
        public AttachmentException(string message) : base(message)
        {

        }

        public AttachmentException(string message, Exception innerException) : base(message, innerException) //also return the exception that cause this one
        {

        }
    }
}
