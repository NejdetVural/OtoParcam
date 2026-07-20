namespace OtoParcam.Domain.Enums;

public enum PurchaseRequestStatus
{
    Pending = 1,
    WaitingForCustomerConfirmation = 2,
    Approved = 3,
    Rejected = 4,
    Cancelled = 5
}