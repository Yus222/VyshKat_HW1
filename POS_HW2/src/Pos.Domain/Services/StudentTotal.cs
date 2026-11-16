namespace Pos.Domain.Services;

public sealed record StudentTotal(
    Guid StudentId,
    string StudentName,
    decimal Percent,
    int GradedCount,
    int CriteriaCount,
    string Mode);
