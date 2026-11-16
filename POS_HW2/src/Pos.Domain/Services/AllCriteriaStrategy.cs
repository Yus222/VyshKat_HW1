using Pos.Domain.Entities;

namespace Pos.Domain.Services;

// Не выставленная оценка считается нулём.
public sealed class AllCriteriaStrategy : IResultStrategy
{
    public string Code => "all";
    public string Description => "Все критерии (пропуски = 0)";

    public decimal Calculate(IReadOnlyList<Criterion> criteria,
        IReadOnlyDictionary<Guid, Evaluation> evaluations)
    {
        decimal totalWeight = criteria.Sum(x => x.Weight);
        if (totalWeight == 0)
            return 0;

        decimal weightedScore = 0;
        foreach (var criterion in criteria)
        {
            if (evaluations.TryGetValue(criterion.Id, out var evaluation))
                weightedScore += evaluation.Score.Value / criterion.MaxScore * criterion.Weight;
        }

        return 100 * weightedScore / totalWeight;
    }
}
