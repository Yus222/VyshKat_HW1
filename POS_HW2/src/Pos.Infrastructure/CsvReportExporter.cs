using System.Globalization;
using System.Text;
using Pos.Application.Ports;
using Pos.Domain.Services;

namespace Pos.Infrastructure;

public sealed class CsvReportExporter : IReportExporter
{
    public void Export(string path, IReadOnlyList<StudentTotal> totals)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(path))!;
        Directory.CreateDirectory(directory);

        using var writer = new StreamWriter(path, false, new UTF8Encoding(true));
        writer.WriteLine("Студент;Итог (%);Оценено;Всего критериев;Режим");
        foreach (var x in totals)
        {
            writer.WriteLine($"{Escape(x.StudentName)};" +
                $"{x.Percent.ToString("F2", CultureInfo.InvariantCulture)};" +
                $"{x.GradedCount};{x.CriteriaCount};{Escape(x.Mode)}");
        }
    }

    private static string Escape(string value)
        => "\"" + value.Replace("\"", "\"\"") + "\"";
}
