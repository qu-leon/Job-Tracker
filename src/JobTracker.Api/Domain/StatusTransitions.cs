namespace JobTracker.Api.Domain;

/// <summary>
/// Defines which status changes are legal, so an application cannot jump
/// straight from Saved to Offer or move on from a terminal state.
/// </summary>
public static class StatusTransitions
{
    private static readonly Dictionary<ApplicationStatus, ApplicationStatus[]> Allowed = new()
    {
        [ApplicationStatus.Saved] = [ApplicationStatus.Applied, ApplicationStatus.Withdrawn],
        [ApplicationStatus.Applied] = [ApplicationStatus.Screening, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn],
        [ApplicationStatus.Screening] = [ApplicationStatus.Interviewing, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn],
        [ApplicationStatus.Interviewing] = [ApplicationStatus.Offer, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn],
        [ApplicationStatus.Offer] = [ApplicationStatus.Accepted, ApplicationStatus.Rejected, ApplicationStatus.Withdrawn],
        [ApplicationStatus.Accepted] = [],
        [ApplicationStatus.Rejected] = [],
        [ApplicationStatus.Withdrawn] = []
    };

    public static bool IsTerminal(ApplicationStatus status) => Allowed[status].Length == 0;

    public static IReadOnlyList<ApplicationStatus> NextStates(ApplicationStatus from) => Allowed[from];

    public static bool CanTransition(ApplicationStatus from, ApplicationStatus to) =>
        Allowed[from].Contains(to);
}
