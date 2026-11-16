using System.Text.Json;
using Pos.Domain.Entities;
using Pos.Domain.Ports;

namespace Pos.Infrastructure;

// Адаптер: JSON — деталь инфраструктуры, которой не существует в домене.
public sealed class JsonCourseRepository : ICourseRepository
{
    private readonly string _path;
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public JsonCourseRepository(string path)
    {
        _path = path;
    }

    public Course Load()
    {
        var course = new Course();
        if (!File.Exists(_path))
            return course;

        var data = JsonSerializer.Deserialize<StoredCourse>(File.ReadAllText(_path), _options)
            ?? throw new InvalidDataException("Не удалось прочитать данные курса.");

        foreach (var x in data.Students)
            course.AddStudent(new Student(x.Id, x.Name));
        foreach (var x in data.Criteria)
            course.AddCriterion(new Criterion(x.Id, x.Name, x.MaxScore, x.Weight));
        foreach (var x in data.Evaluations)
            course.SetEvaluation(x.StudentId, x.CriterionId, x.Score, x.Comment);

        return course;
    }

    public void Save(Course course)
    {
        var data = new StoredCourse
        {
            Students = course.Students.Select(x => new StoredStudent(x.Id, x.Name)).ToList(),
            Criteria = course.Criteria.Select(x =>
                new StoredCriterion(x.Id, x.Name, x.MaxScore, x.Weight)).ToList(),
            Evaluations = course.Evaluations.Select(x =>
                new StoredEvaluation(x.StudentId, x.CriterionId, x.Score.Value, x.Comment)).ToList()
        };

        var fullPath = Path.GetFullPath(_path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var tempPath = fullPath + ".tmp";
        try
        {
            File.WriteAllText(tempPath, JsonSerializer.Serialize(data, _options));
            File.Move(tempPath, fullPath, true);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    public sealed class StoredCourse
    {
        public List<StoredStudent> Students { get; set; } = new();
        public List<StoredCriterion> Criteria { get; set; } = new();
        public List<StoredEvaluation> Evaluations { get; set; } = new();
    }

    public sealed record StoredStudent(Guid Id, string Name);
    public sealed record StoredCriterion(Guid Id, string Name, decimal MaxScore, decimal Weight);
    public sealed record StoredEvaluation(Guid StudentId, Guid CriterionId,
        decimal Score, string Comment);
}
