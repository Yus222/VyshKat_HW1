using Pos.Domain.Entities;

namespace Pos.Domain.Services;

// Средний результат только по уже оценённым критериям.
public sealed class GradedOnlyStrategy : IResultStrategy
{
    public string Code => "graded";
    public string Description => "Только оценённые критерии";

    public decimal Calculate(IReadOnlyList<Criterion> criteria,
        IReadOnlyDictionary<Guid, Evaluation> evaluations)
    {
        decimal totalWeight = 0;
        decimal weightedScore = 0;

        foreach (var criterion in criteria)
        {
            if (!evaluations.TryGetValue(criterion.Id, out var evaluation))
                continue;

            totalWeight += criterion.Weight;
            weightedScore += evaluation.Score.Value / criterion.MaxScore * criterion.Weight;
        }

        return totalWeight == 0 ? 0 : 100 * weightedScore / totalWeight;
    }
}
