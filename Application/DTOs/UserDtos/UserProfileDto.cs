using Newtonsoft.Json;

namespace TMPApplication.DTOs.UserDtos
{
    public class UserProfileDto
    {
        [JsonProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")]
        public string Id { get; set; }

        [JsonProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")]
        public string FirstName { get; set; }

        [JsonProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname")]
        public string LastName { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("picture")]
        public string ProfilePicture { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }

        [JsonProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")]
        public string Email { get; set; }

        [JsonProperty("email_verified")]
        public bool IsEmailVerified { get; set; }

        [JsonProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dateofbirth")]
        public string Birthdate { get; set; }

        [JsonProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/roles")]
        public IEnumerable<string> Roles { get; set; }

        [JsonProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone")]
        public string PhoneNumber { get; set; }
    }
}
