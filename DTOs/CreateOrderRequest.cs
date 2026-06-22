namespace EmployeeManagement.DTOs
{
    public record CreateOrderRequest(
        string ProductName,
        int Quantity,
        decimal TotalAmount,
        string Street,
        string City,
        string State,
        string PostalCode
    );
}
