namespace EmployeeManagement.DTOs
{
    public record OrderResponse(
        int Id,
        string ProductName,
        int Quantity,
        decimal TotalAmount,
        string Status,
        DateTime OrderDate,
        string EmployeeName,
        string ShipCity
    );
}
