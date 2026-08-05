namespace Espada.Tests.Domain.Aggregates
{
    public sealed class AgentSessionTests
    {
        [Fact]
        public void ApprovalFlow_ShouldRequireRunningSessionAndResumeExplicitly()
        {
            DateTimeOffset createdAtUtc = DateTimeOffset.UtcNow;
            AgentSession session = AgentSession.Create(AgentSessionId.New(), WorkspaceId.New(), ProjectId.New(),
                AgentProfileId.New(), DeviceId.New(), "Review the repository", "feature/review",
                "C:\\worktrees\\review", createdAtUtc).ShouldSucceed();

            session.Start(createdAtUtc.AddSeconds(1)).ShouldSucceed();
            session.WaitForApproval(createdAtUtc.AddSeconds(2)).ShouldSucceed();
            session.Status.Should().Be(AgentSessionStatusType.WaitingForApproval);
            session.Complete(createdAtUtc.AddSeconds(3)).IsFailure.Should().BeTrue();
            session.ResumeAfterApproval(createdAtUtc.AddSeconds(4)).ShouldSucceed();
            session.Complete(createdAtUtc.AddSeconds(5)).ShouldSucceed();
            session.Status.Should().Be(AgentSessionStatusType.Completed);
        }
    }
}