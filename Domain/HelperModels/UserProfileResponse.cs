using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMPDomain.HelperModels
{
    public class UserProfileResponse
    {
        
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Birthdate { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfilePicture { get; set; }
    }
}
