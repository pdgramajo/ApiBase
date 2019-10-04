using System.ComponentModel.DataAnnotations;
namespace api.Models
{
    public class RoleId
    {
        [Required]
        public string Id { get; set; }
    }
}