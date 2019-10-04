using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace api.Models
{
    public class ApplicationUserBasicData
    {

        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Roles { get; set; }
    }
}