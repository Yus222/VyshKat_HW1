using Pos.Domain.Entities;
using Pos.Domain.ValueObjects;

namespace Pos.Domain.Factories;

public sealed class EvaluationFactory
{
    public Evaluation Create(Student student, Criterion criterion, decimal value, string? comment)
    {
        var score = new Score(value);
        if (score.Value > criterion.MaxScore)
            throw new ArgumentException($"Балл не может превышать {criterion.MaxScore}.");

        return new Evaluation(student.Id, criterion.Id, score, comment);
    }
}
