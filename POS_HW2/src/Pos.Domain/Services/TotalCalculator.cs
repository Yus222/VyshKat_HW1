using Pos.Domain.Entities;

namespace Pos.Domain.Services;

// Domain Service: итог вычисляется из текущего рубрикатора и оценок, не хранится отдельно.
public sealed class TotalCalculator
{
    private readonly Dictionary<string, IResultStrategy> _strategies;

    public TotalCalculator(IEnumerable<IResultStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(x => x.Code);
    }

    public StudentTotal Calculate(Course course, Guid studentId, string mode)
    {
        var student = course.GetStudent(studentId);
        if (!_strategies.TryGetValue(mode, out var strategy))
            throw new ArgumentException("Неизвестный способ расчёта итога.");

        var criteria = course.Criteria;
        var evaluations = course.Evaluations
            .Where(x => x.StudentId == studentId)
            .ToDictionary(x => x.CriterionId);
        var percent = strategy.Calculate(criteria, evaluations);

        return new StudentTotal(student.Id, student.Name, percent,
            evaluations.Count, criteria.Count, strategy.Code);
    }
}
