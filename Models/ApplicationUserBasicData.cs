using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace api.Models
{
    public class UserBasicData
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
    public class ApplicationUserBasicData : UserBasicData
    {
        public string Id { get; set; }
        public string Roles { get; set; }
        public string AvatarUrl { get; set; }
    }

    public class UserEditData
    {
        public string PhoneNumber { get; set; }
        public string AvatarUrl { get; set; }
        [Required]
        public string[] roles { get; set; }
    }
}