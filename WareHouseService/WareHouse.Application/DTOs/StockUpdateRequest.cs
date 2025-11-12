using System.ComponentModel.DataAnnotations;

namespace WareHouse.Application.DTOs;

public class StockUpdateRequest
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public int Quantity { get; set; }

    [Required]
    [StringLength(20, ErrorMessage = "Location cannot exceed 20 characters")]
    public string Location { get; set; } = string.Empty;

    [Required]
    public string Operation { get; set; } = string.Empty; // "restock" or "adjust"
}