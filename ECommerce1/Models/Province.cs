using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models
{
    public class Province
    {
        [Key]
        [MaxLength(20)]
        public string Id { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string FullName { get; set; }
        
        [MaxLength(255)]
        public string CodeName { get; set; }

        public virtual ICollection<Ward> Wards { get; set; }
    }
}
