using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace api.Models
{
   public class ApplicationRole : IdentityRole
	{
		public virtual ICollection<IdentityUserRole<string>> Users { get; set; }

		public virtual ICollection<IdentityRoleClaim<string>> Claims { get; set; }
	}
}