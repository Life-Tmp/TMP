using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMPDomain.HelperModels
{
    public class LoginResponse
    {
        public string AccessToken {  get; set; }
        public string Message { get; set; }
        public string Error {  get; set; }
    }
}
