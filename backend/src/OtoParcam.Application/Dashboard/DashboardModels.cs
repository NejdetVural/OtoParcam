namespace OtoParcam.Application.Dashboard;

public class DashboardStatsDto
{
    public int TotalProducts { get; set; }
    public int TotalCustomers { get; set; }
    public int PendingPurchaseRequests { get; set; }
    public int ProductsAwaitingAttention { get; set; }
}
