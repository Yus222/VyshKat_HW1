using Pos.Application;
using Pos.Domain.Entities;
using Pos.Domain.Ports;
using Pos.Domain.Services;
using Pos.Infrastructure;

var tests = new (string Name, Action Test)[]
{
    ("Пустое имя студента запрещено", EmptyStudentName),
    ("Неверный максимум критерия запрещён", InvalidCriterion),
    ("Отрицательный балл запрещён", NegativeScore),
    ("Балл выше максимума запрещён", ExcessScore),
    ("Несуществующий критерий запрещён", UnknownCriterion),
    ("Несуществующий студент запрещён", UnknownStudent),
    ("Изменение оценки заменяет старую", ReplaceScore),
    ("Нельзя снизить максимум ниже оценки", ReduceMaximum),
    ("Удаление критерия удаляет оценки", DeleteCriterion),
    ("Удаление студента удаляет оценки", DeleteStudent),
    ("Взвешенный итог рассчитан верно", WeightedTotal),
    ("Пропуски учитываются по выбранной стратегии", MissingScoreStrategies),
    ("Пустой рубрикатор даёт 0", EmptyRubric),
    ("JSON сохраняет и загружает данные", JsonRoundTrip),
    ("CSV содержит правильный итог и экранирует имя", CsvExport),
    ("Facade: сценарий от создания до экспорта", FacadeScenario),
    ("Ошибка записи не меняет данные в памяти", SaveFailureRollback),
    ("Демонстрация не меняет основную ведомость", DemoIsSeparate),
    ("Некорректные изменения критерия не сохраняются", InvalidCriterionUpdate)
};

int failed = 0;
foreach (var (name, test) in tests)
{
    try
    {
        test();
        Console.WriteLine("OK: " + name);
    }
    catch (Exception e)
    {
        failed++;
        Console.WriteLine("FAIL: " + name + " — " + e.Message);
    }
}

Console.WriteLine($"Итого: {tests.Length - failed}/{tests.Length}");
return failed == 0 ? 0 : 1;

static Course NewCourse()
{
    return new Course();
}

static (Course Course, Student Student, Criterion Criterion) Sample()
{
    var course = NewCourse();
    var student = new Student(Guid.NewGuid(), "Студент");
    var criterion = new Criterion(Guid.NewGuid(), "Контрольная", 10, 2);
    course.AddStudent(student);
    course.AddCriterion(criterion);
    return (course, student, criterion);
}

static TotalCalculator Calculator()
{
    return new TotalCalculator(new IResultStrategy[]
    {
        new AllCriteriaStrategy(), new GradedOnlyStrategy()
    });
}

static void Check(bool condition, string message = "Проверка не прошла")
{
    if (!condition)
        throw new Exception(message);
}

static void Throws<T>(Action action) where T : Exception
{
    try
    {
        action();
    }
    catch (T)
    {
        return;
    }
    throw new Exception("Ожидалось исключение " + typeof(T).Name);
}

static void EmptyStudentName()
    => Throws<ArgumentException>(() => new Student(Guid.NewGuid(), " "));

static void InvalidCriterion()
{
    Throws<ArgumentException>(() => new Criterion(Guid.NewGuid(), "А", 0, 1));
    Throws<ArgumentException>(() => new Criterion(Guid.NewGuid(), "А", 10, 0));
}

static void NegativeScore()
{
    var (course, student, criterion) = Sample();
    Throws<ArgumentException>(() => course.SetEvaluation(student.Id, criterion.Id, -1));
}

static void ExcessScore()
{
    var (course, student, criterion) = Sample();
    Throws<ArgumentException>(() => course.SetEvaluation(student.Id, criterion.Id, 11));
}

static void UnknownCriterion()
{
    var (course, student, _) = Sample();
    Throws<ArgumentException>(() => course.SetEvaluation(student.Id, Guid.NewGuid(), 5));
}

static void UnknownStudent()
{
    var (course, _, criterion) = Sample();
    Throws<ArgumentException>(() => course.SetEvaluation(Guid.NewGuid(), criterion.Id, 5));
}

static void ReplaceScore()
{
    var (course, student, criterion) = Sample();
    course.SetEvaluation(student.Id, criterion.Id, 4);
    course.SetEvaluation(student.Id, criterion.Id, 7);
    Check(course.Evaluations.Count == 1);
    Check(course.GetEvaluation(student.Id, criterion.Id).Score.Value == 7);
    course.RemoveEvaluation(student.Id, criterion.Id);
    Check(course.Evaluations.Count == 0);
}

static void ReduceMaximum()
{
    var (course, student, criterion) = Sample();
    course.SetEvaluation(student.Id, criterion.Id, 8);
    Throws<ArgumentException>(() => course.UpdateCriterion(criterion.Id, "Контрольная", 5, 1));
    Check(criterion.MaxScore == 10);
}

static void DeleteCriterion()
{
    var (course, student, criterion) = Sample();
    course.SetEvaluation(student.Id, criterion.Id, 8);
    course.RemoveCriterion(criterion.Id);
    Check(course.Evaluations.Count == 0 && course.Criteria.Count == 0);
}

static void DeleteStudent()
{
    var (course, student, criterion) = Sample();
    course.SetEvaluation(student.Id, criterion.Id, 8);
    course.RemoveStudent(student.Id);
    Check(course.Evaluations.Count == 0 && course.Students.Count == 0);
}

static void WeightedTotal()
{
    var (course, student, first) = Sample();
    var second = new Criterion(Guid.NewGuid(), "Экзамен", 20, 3);
    course.AddCriterion(second);
    course.SetEvaluation(student.Id, first.Id, 8);
    course.SetEvaluation(student.Id, second.Id, 15);
    // 100 * ((8/10)*2 + (15/20)*3) / (2+3) = 77.
    Check(Calculator().Calculate(course, student.Id, "all").Percent == 77);
}

static void MissingScoreStrategies()
{
    var (course, student, first) = Sample();
    course.AddCriterion(new Criterion(Guid.NewGuid(), "Экзамен", 10, 2));
    course.SetEvaluation(student.Id, first.Id, 8);
    Check(Calculator().Calculate(course, student.Id, "all").Percent == 40);
    Check(Calculator().Calculate(course, student.Id, "graded").Percent == 80);
}

static void EmptyRubric()
{
    var course = NewCourse();
    var student = new Student(Guid.NewGuid(), "А");
    course.AddStudent(student);
    Check(Calculator().Calculate(course, student.Id, "all").Percent == 0);
}

static void JsonRoundTrip()
{
    var path = Path.Combine(Path.GetTempPath(), "pos-test-" + Guid.NewGuid() + ".json");
    try
    {
        var (course, student, criterion) = Sample();
        course.SetEvaluation(student.Id, criterion.Id, 9, "Комментарий");
        var repo = new JsonCourseRepository(path);
        repo.Save(course);
        var loaded = repo.Load();
        Check(loaded.Students.Single().Id == student.Id);
        Check(loaded.Criteria.Single().MaxScore == 10);
        Check(loaded.GetEvaluation(student.Id, criterion.Id).Score.Value == 9);
        Check(loaded.GetEvaluation(student.Id, criterion.Id).Comment == "Комментарий");
    }
    finally
    {
        if (File.Exists(path)) File.Delete(path);
    }
}

static void CsvExport()
{
    var path = Path.Combine(Path.GetTempPath(), "pos-test-" + Guid.NewGuid() + ".csv");
    try
    {
        new CsvReportExporter().Export(path, new[]
        {
            new StudentTotal(Guid.NewGuid(), "Иван; \"тест\"", 77.5m, 2, 2, "all")
        });
        var text = File.ReadAllText(path);
        Check(text.Contains("77.50"));
        Check(text.Contains("\"Иван; \"\"тест\"\"\""));
    }
    finally
    {
        if (File.Exists(path)) File.Delete(path);
    }
}

static void FacadeScenario()
{
    var dir = Path.Combine(Path.GetTempPath(), "pos-scenario-" + Guid.NewGuid());
    var dataPath = Path.Combine(dir, "course.json");
    var csvPath = Path.Combine(dir, "totals.csv");
    try
    {
        ICourseRepository repo = new JsonCourseRepository(dataPath);
        var app = new PosFacade(repo, new CsvReportExporter(), Calculator());
        var student = app.AddStudent("Аня");
        var criterion = app.AddCriterion("Проект", 10, 1);
        app.SetEvaluation(student.Id, criterion.Id, 9, "ОК");
        Check(app.GetTotal(student.Id, "all").Percent == 90);
        app.ExportTotals(csvPath, "all");
        Check(File.Exists(csvPath));
        var reopened = new PosFacade(repo, new CsvReportExporter(), Calculator());
        Check(reopened.GetTotal(student.Id, "all").Percent == 90);
    }
    finally
    {
        if (Directory.Exists(dir)) Directory.Delete(dir, true);
    }
}

static void SaveFailureRollback()
{
    var repo = new FailingRepository();
    var app = new PosFacade(repo, new CsvReportExporter(), Calculator());
    Throws<IOException>(() => app.AddStudent("Студент"));
    Check(app.Students.Count == 0);
}

static void DemoIsSeparate()
{
    var dir = Path.Combine(Path.GetTempPath(), "pos-demo-" + Guid.NewGuid());
    var dataPath = Path.Combine(dir, "data.json");
    var csvPath = Path.Combine(dir, "demo.csv");
    try
    {
        var repo = new JsonCourseRepository(dataPath);
        var app = new PosFacade(repo, new CsvReportExporter(), Calculator());
        app.AddStudent("Настоящий студент");
        var saved = File.ReadAllText(dataPath);
        var totals = app.RunDemo(csvPath);
        Check(totals.Count == 2);
        Check(File.Exists(csvPath));
        Check(app.Students.Count == 1);
        Check(File.ReadAllText(dataPath) == saved);
        app.RunDemo(csvPath);
        Check(app.Students.Count == 1);
    }
    finally
    {
        if (Directory.Exists(dir)) Directory.Delete(dir, true);
    }
}

static void InvalidCriterionUpdate()
{
    var (course, _, criterion) = Sample();
    Throws<ArgumentException>(() => course.UpdateCriterion(criterion.Id, "", 15, 1));
    Throws<ArgumentException>(() => course.UpdateCriterion(criterion.Id, "Название", -1, 1));
    Check(criterion.MaxScore == 10);
    Check(criterion.Name == "Контрольная");
}

sealed class FailingRepository : ICourseRepository
{
    public Course Load() => new();

    public void Save(Course course) => throw new IOException("Сохранение недоступно.");
}
