namespace SliceA.Infrastructure;

public class Contact
{
    public long ContactId { get; set; }
    public long CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
}