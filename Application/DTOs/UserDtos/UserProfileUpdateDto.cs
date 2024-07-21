using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMPApplication.DTOs.UserDtos
{
    public class UserProfileUpdateDto
    {
        
        
        public string FirstName { get; set; } //GivenName
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Picture { get; set; }
        public DateTime Birthday {  get; set; } 

        public DateTime UpdatedAt {  get; set; }
      
    }
}
