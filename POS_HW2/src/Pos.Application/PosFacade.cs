using Pos.Application.Ports;
using Pos.Domain.Entities;
using Pos.Domain.Ports;
using Pos.Domain.Services;

namespace Pos.Application;

public sealed class PosFacade
{
    private readonly ICourseRepository _repository;
    private readonly IReportExporter _exporter;
    private readonly TotalCalculator _calculator;
    private Course _course;

    public PosFacade(ICourseRepository repository, IReportExporter exporter,
        TotalCalculator calculator)
    {
        _repository = repository;
        _exporter = exporter;
        _calculator = calculator;
        _course = repository.Load();
    }

    public IReadOnlyList<Student> Students => _course.Students;
    public IReadOnlyList<Criterion> Criteria => _course.Criteria;

    public Student AddStudent(string name)
    {
        var student = new Student(Guid.NewGuid(), name);
        Change(course => course.AddStudent(student));
        return student;
    }

    public void RenameStudent(Guid id, string name)
    {
        Change(course => course.RenameStudent(id, name));
    }

    public void RemoveStudent(Guid id)
    {
        Change(course => course.RemoveStudent(id));
    }

    public Criterion AddCriterion(string name, decimal maxScore, decimal weight)
    {
        var criterion = new Criterion(Guid.NewGuid(), name, maxScore, weight);
        Change(course => course.AddCriterion(criterion));
        return criterion;
    }

    public void UpdateCriterion(Guid id, string name, decimal maxScore, decimal weight)
    {
        Change(course => course.UpdateCriterion(id, name, maxScore, weight));
    }

    public void RemoveCriterion(Guid id)
    {
        Change(course => course.RemoveCriterion(id));
    }

    public void SetEvaluation(Guid studentId, Guid criterionId, decimal score, string? comment)
    {
        Change(course => course.SetEvaluation(studentId, criterionId, score, comment));
    }

    public void RemoveEvaluation(Guid studentId, Guid criterionId)
    {
        Change(course => course.RemoveEvaluation(studentId, criterionId));
    }

    public IReadOnlyList<EvaluationView> GetEvaluations(Guid studentId)
    {
        _course.GetStudent(studentId);
        return _course.Evaluations.Where(x => x.StudentId == studentId)
            .Select(x =>
            {
                var criterion = _course.GetCriterion(x.CriterionId);
                return new EvaluationView(criterion.Name, x.Score.Value,
                    criterion.MaxScore, x.Comment);
            })
            .OrderBy(x => x.CriterionName)
            .ToList();
    }

    public StudentTotal GetTotal(Guid studentId, string mode)
        => _calculator.Calculate(_course, studentId, mode);

    public void ExportTotals(string path, string mode)
    {
        var totals = Students.Select(x => GetTotal(x.Id, mode)).ToList();
        _exporter.Export(path, totals);
    }

    public IReadOnlyList<StudentTotal> RunDemo(string path)
    {
        var demo = new Course();
        var first = new Student(Guid.NewGuid(), "Иван Петров");
        var second = new Student(Guid.NewGuid(), "Анна Сидорова");
        var homework = new Criterion(Guid.NewGuid(), "Домашние задания", 10, 2);
        var exam = new Criterion(Guid.NewGuid(), "Контрольная", 20, 3);

        demo.AddStudent(first);
        demo.AddStudent(second);
        demo.AddCriterion(homework);
        demo.AddCriterion(exam);
        demo.SetEvaluation(first.Id, homework.Id, 8, "Хорошо");
        demo.SetEvaluation(first.Id, exam.Id, 15, "Зачтено");
        demo.SetEvaluation(second.Id, homework.Id, 10, "Отлично");
        demo.SetEvaluation(second.Id, exam.Id, 18, "Отлично");

        var totals = demo.Students.Select(x => _calculator.Calculate(demo, x.Id, "all"))
            .ToList();
        _exporter.Export(path, totals);
        return totals;
    }

    private void Change(Action<Course> action)
    {
        var updated = _course.Copy();
        action(updated);
        _repository.Save(updated);
        _course = updated;
    }
}
