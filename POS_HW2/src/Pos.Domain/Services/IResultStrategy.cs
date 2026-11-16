using Pos.Domain.Entities;

namespace Pos.Domain.Services;

public interface IResultStrategy
{
    string Code { get; }
    string Description { get; }
    decimal Calculate(IReadOnlyList<Criterion> criteria,
        IReadOnlyDictionary<Guid, Evaluation> evaluations);
}
