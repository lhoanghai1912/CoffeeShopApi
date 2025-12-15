using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeShopApi.Models;

public class ProductDetail
{
    [Key]
    public int Id { get; set; }

    // Tên biến thể/Size (VD: Size M, Size L, Nóng, Đá)
    [Required]
    [MaxLength(50)]
    public string Size { get; set; } = "Standard"; 

    // Giá tiền nằm ở đây (vì mỗi size giá khác nhau)
    [Column(TypeName = "decimal(18,0)")]
    [Required]
    public decimal Price { get; set; }
    
    // --- KHÓA NGOẠI TRỎ VỀ PRODUCT ---
    public int ProductId { get; set; }

    [ForeignKey("ProductId")]
    public Product? Product { get; set; }
}