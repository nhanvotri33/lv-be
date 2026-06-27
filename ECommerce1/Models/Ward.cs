using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models
{
    public class Ward
    {
        [Key]
        [MaxLength(20)]
        public string Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string FullName { get; set; }

        [MaxLength(255)]
        public string CodeName { get; set; }

        [MaxLength(20)]
        public string ProvinceId { get; set; }
        
        [ForeignKey("ProvinceId")]
        public virtual Province Province { get; set; }

        public virtual ICollection<ShippingInfo> ShippingInfos { get; set; }
    }
}
