using Microsoft.Extensions.DependencyInjection;
using Pos.Application;
using Pos.Application.Ports;
using Pos.Domain.Ports;
using Pos.Domain.Services;
using Pos.Infrastructure;
using Pos.Cli;

var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "pos.json");

var services = new ServiceCollection();
services.AddSingleton<ICourseRepository>(_ => new JsonCourseRepository(dataPath));
services.AddSingleton<IReportExporter, CsvReportExporter>();
services.AddSingleton<IResultStrategy, AllCriteriaStrategy>();
services.AddSingleton<IResultStrategy, GradedOnlyStrategy>();
services.AddSingleton<TotalCalculator>();
services.AddSingleton<PosFacade>();
services.AddSingleton<ConsoleMenu>();

using var provider = services.BuildServiceProvider();

try
{
    provider.GetRequiredService<ConsoleMenu>().Run();
}
catch (Exception e)
{
    System.Console.WriteLine("Ошибка запуска: " + e.Message);
    Environment.ExitCode = 1;
}
