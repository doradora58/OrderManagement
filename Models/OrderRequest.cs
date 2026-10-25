namespace OrderManagement.Models;
public class OrderRequest
{
    public FromInfo From { get; set; } = new();
    public ToInfo To { get; set; } = new();
}

public class FromInfo
{
    public string SenderName { get; set; } = string.Empty;
    public string PostalCode { get; set; } =string.Empty;
    public string Address { get; set; } = string.Empty;
}

public class ToInfo
{
    public string RecipientName { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}



