using WEBDULICH.Models;

public class Orders
{
    public int Id { get; set; }
    public string Status { get; set; }
    public int Quantity { get; set; }
    public int TotalPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public string PaymentMethod { get; set; }

    public int? UserId { get; set; }
    public User User { get; set; }

    public int? TourId { get; set; }
    public Tour Tour { get; set; }

    public int? HotelId { get; set; }      
    public Hotel Hotel { get; set; }       

    public Payment Payment { get; set; }
    public string? ConfirmedEmail { get; set; }
    public DateTime? DepartureDate { get; set; }
    
    // Navigation property for order details
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
