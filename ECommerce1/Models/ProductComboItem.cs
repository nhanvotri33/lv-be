using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models
{
    public class ProductComboItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductComboId { get; set; }
        [ForeignKey("ProductComboId")]
        public virtual ProductCombo ProductCombo { get; set; }

        [Required]
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        public bool IsMain { get; set; }

        [Required]
        [MaxLength(20)]
        public string DiscountType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; }
    }
}
