using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace api.Models
{// con esta clase podemos agregar propiedades a la tabla users podemos hacer lo mismo con cualquier otra tabla creada por identity
 // solo debe heredar de la tabla que corresponda
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<IdentityUserRole<string>> Roles { get; set; }

        public virtual ICollection<IdentityUserClaim<string>> Claims { get; set; }
    }
}