using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMPDomain.Exceptions
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string userId)
            : base($"User with ID {userId} was not found.")
        {

        }
    }
}
