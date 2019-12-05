using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.Models
{
     public class UserInfoCreate : UserInfo
    {
        public string RoleName { get; set; }
        public string AvatarUrl { get; set; }
        public string PhoneNumber { get; set; }
    }
}