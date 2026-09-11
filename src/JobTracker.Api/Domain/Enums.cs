namespace JobTracker.Api.Domain;

public enum ApplicationStatus
{
    Saved,
    Applied,
    Screening,
    Interviewing,
    Offer,
    Accepted,
    Rejected,
    Withdrawn
}

public enum InterviewStage
{
    PhoneScreen,
    Technical,
    Behavioral,
    SystemDesign,
    Onsite,
    Final
}

public enum InterviewOutcome
{
    Pending,
    Passed,
    Failed,
    Cancelled
}
