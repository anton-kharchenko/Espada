using Espada.Billing;
using Espada.Billing.Enums;
using Espada.Billing.Models;
using Espada.Tests.Infrastructure.Billing.TestData;

namespace Espada.Tests.Infrastructure.Billing;

public sealed class BillingAccessStateTypeTests
{
    [Theory]
    [MemberData(nameof(BillingAccessStateTestData.FailedPaymentTimeline), MemberType = typeof(BillingAccessStateTestData))]
    public void GetAccessState_ShouldApplyFailedPaymentTimeline(int elapsedDays, BillingAccessStateType expected)
    {
        DateTimeOffset failedAt = new(2026, 7, 1, 0, 0, 0, TimeSpan.Zero);
        BillingCustomerSnapshot customer = new(Guid.NewGuid(), "cus_test", "sub_test", CloudBillingPlanType.Solo, "past_due", failedAt, failedAt);
        Assert.Equal(expected, customer.GetAccessState(failedAt.AddDays(elapsedDays)));
    }
}