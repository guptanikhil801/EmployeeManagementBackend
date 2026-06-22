namespace EmployeeManagement.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }

        // Navigation back
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }

    // Explicit join entity — use this when the join table
    // has extra columns (Quantity, UnitPrice at time of order)
    public class OrderProduct
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }   // price at time of purchase

        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
