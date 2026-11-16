using Pos.Domain.ValueObjects;

namespace Pos.Domain.Entities;

public sealed class Evaluation
{
    public Guid StudentId { get; }
    public Guid CriterionId { get; }
    public Score Score { get; }
    public string Comment { get; }

    internal Evaluation(Guid studentId, Guid criterionId, Score score, string? comment)
    {
        StudentId = studentId;
        CriterionId = criterionId;
        Score = score;
        Comment = comment?.Trim() ?? "";
    }
}
