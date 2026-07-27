using Espada.Billing.Enums;

namespace Espada.Tests.Infrastructure.Billing.TestData;

public static class BillingAccessStateTestData
{
    public static TheoryData<int, BillingAccessStateType> FailedPaymentTimeline => new()
    {
        { 1, BillingAccessStateType.Grace },
        { 7, BillingAccessStateType.ReadOnly },
        { 30, BillingAccessStateType.SyncDisabled }
    };
}