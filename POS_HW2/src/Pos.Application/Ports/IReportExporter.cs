using Pos.Domain.Services;

namespace Pos.Application.Ports;

public interface IReportExporter
{
    void Export(string path, IReadOnlyList<StudentTotal> totals);
}
