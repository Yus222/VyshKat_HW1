namespace Pos.Application;

public sealed record EvaluationView(string CriterionName, decimal Score,
    decimal MaxScore, string Comment);
