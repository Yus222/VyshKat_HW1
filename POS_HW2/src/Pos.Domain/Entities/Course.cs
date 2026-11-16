using Pos.Domain.Factories;

namespace Pos.Domain.Entities;

// Агрегат: все оценки должны относиться к существующим студентам и критериям.
public sealed class Course
{
    private readonly Dictionary<Guid, Student> _students = new();
    private readonly Dictionary<Guid, Criterion> _criteria = new();
    private readonly Dictionary<(Guid, Guid), Evaluation> _evaluations = new();
    private readonly EvaluationFactory _factory = new();

    public IReadOnlyList<Student> Students => _students.Values.OrderBy(x => x.Name).ToList();
    public IReadOnlyList<Criterion> Criteria => _criteria.Values.OrderBy(x => x.Name).ToList();
    public IReadOnlyList<Evaluation> Evaluations => _evaluations.Values.ToList();

    public Course Copy()
    {
        var copy = new Course();
        foreach (var student in _students.Values)
            copy.AddStudent(new Student(student.Id, student.Name));
        foreach (var criterion in _criteria.Values)
            copy.AddCriterion(new Criterion(criterion.Id, criterion.Name,
                criterion.MaxScore, criterion.Weight));
        foreach (var evaluation in _evaluations.Values)
            copy.SetEvaluation(evaluation.StudentId, evaluation.CriterionId,
                evaluation.Score.Value, evaluation.Comment);
        return copy;
    }

    public void AddStudent(Student student)
    {
        if (!_students.TryAdd(student.Id, student))
            throw new ArgumentException("Студент с таким ID уже существует.");
    }

    public Student GetStudent(Guid id) => _students.TryGetValue(id, out var student)
        ? student : throw new ArgumentException("Студент не найден.");

    public void RenameStudent(Guid id, string name) => GetStudent(id).Rename(name);

    public void RemoveStudent(Guid id)
    {
        GetStudent(id);
        _students.Remove(id);
        foreach (var key in _evaluations.Keys.Where(x => x.Item1 == id).ToList())
            _evaluations.Remove(key);
    }

    public void AddCriterion(Criterion criterion)
    {
        if (!_criteria.TryAdd(criterion.Id, criterion))
            throw new ArgumentException("Критерий с таким ID уже существует.");
    }

    public Criterion GetCriterion(Guid id) => _criteria.TryGetValue(id, out var criterion)
        ? criterion : throw new ArgumentException("Критерий не найден в рубрикаторе.");

    public void UpdateCriterion(Guid id, string name, decimal maxScore, decimal weight)
    {
        var criterion = GetCriterion(id);
        if (_evaluations.Values.Any(x => x.CriterionId == id && x.Score.Value > maxScore))
            throw new ArgumentException("Новый максимум ниже уже выставленной оценки.");

        criterion.Update(name, maxScore, weight);
    }

    public void RemoveCriterion(Guid id)
    {
        GetCriterion(id);
        _criteria.Remove(id);
        foreach (var key in _evaluations.Keys.Where(x => x.Item2 == id).ToList())
            _evaluations.Remove(key);
    }

    public void SetEvaluation(Guid studentId, Guid criterionId, decimal score, string? comment = null)
    {
        var student = GetStudent(studentId);
        var criterion = GetCriterion(criterionId);
        var evaluation = _factory.Create(student, criterion, score, comment);
        _evaluations[(studentId, criterionId)] = evaluation;
    }

    public Evaluation GetEvaluation(Guid studentId, Guid criterionId)
    {
        return _evaluations.TryGetValue((studentId, criterionId), out var evaluation)
            ? evaluation : throw new ArgumentException("Оценка не найдена.");
    }

    public void RemoveEvaluation(Guid studentId, Guid criterionId)
    {
        if (!_evaluations.Remove((studentId, criterionId)))
            throw new ArgumentException("Оценка не найдена.");
    }
}
