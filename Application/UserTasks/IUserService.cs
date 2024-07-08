using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPDomain;

namespace TMPApplication.UserTasks
{
    internal interface IUserService
    {
        Task<string> UserLogin(string email, string password);
        Task<string> UserRegister(UserRegisterDto userRegister);
    }
}
