using Xunit;
using VyshKat.Services;

namespace VyshKat.Tests;

public class ConsoleAppTests
{
    [Fact]
    public void MenuAllowsAddingTransportAndShowingReports()
    {
        string input = "1\n1\nСамокат\nS-1\n6\n1\n2,5\n3\n4\n5\n0\n";
        var output = RunApp(input);

        Assert.Contains("Техника принята в парк", output);
        Assert.Contains("Транспортных средств: 1", output);
        Assert.Contains("Суточное энергопотребление: 2.5 кВт·ч", output);
        Assert.Contains("Самокат (№ S-1), простота: 6", output);
        Assert.Contains("Самокат (№ S-1)", output);
    }

    [Fact]
    public void MenuAllowsAddingHelmetAndChargingStation()
    {
        string input = "2\n1\nШлем\nH-1\n2\n2\nЗарядка\nC-1\n1.2\n3\n5\n0\n";
        var output = RunApp(input);

        Assert.Contains("Всего объектов на балансе: 2", output);
        Assert.Contains("Суточное энергопотребление: 1.2 кВт·ч", output);
        Assert.Contains("Шлем (№ H-1)", output);
        Assert.Contains("Зарядка (№ C-1)", output);
    }

    [Fact]
    public void MenuRejectsBrokenTransportAndShowsEmptyResults()
    {
        string input = "1\n2\nВелосипед\nB-1\n10\n0\n3\n4\n5\n0\n";
        var output = RunApp(input);

        Assert.Contains("Техосмотр не пройден", output);
        Assert.Contains("Транспортных средств: 0", output);
        Assert.Contains("Подходящего транспорта нет", output);
        Assert.Contains("На балансе пока ничего нет", output);
    }

    [Fact]
    public void MenuRejectsDuplicateInventoryNumber()
    {
        string input = "2\n1\nШлем\n100\n2\n1\nВторой шлем\n100\n3\n0\n";
        var output = RunApp(input);

        Assert.Contains("Этот инвентарный номер уже используется", output);
        Assert.Contains("Всего объектов на балансе: 1", output);
    }

    [Fact]
    public void MenuChecksInputAndKeepsRunning()
    {
        string input = "9\n1\n9\n3\n\nЭлектровелосипед\n\nE-1\n11\n8\n3\n1\n-2\n1\n3\n0\n";
        var output = RunApp(input);

        Assert.Contains("Неизвестная команда", output);
        Assert.Contains("Введите число от 1 до 3", output);
        Assert.Contains("Значение не должно быть пустым", output);
        Assert.Contains("Введите число от 1 до 10", output);
        Assert.Contains("Введите число от 0 до 1", output);
        Assert.Contains("Введите неотрицательное число", output);
        Assert.Contains("Транспортных средств: 1", output);
    }

    [Fact]
    public void MenuExitsOnEndOfInput()
    {
        var output = RunApp("");

        Assert.Contains("=== ВышКат ===", output);
    }

    private static string RunApp(string input)
    {
        using var reader = new StringReader(input);
        using var writer = new StringWriter();
        var app = new ConsoleApp(new Park(new ServiceCenter()), reader, writer);
        app.Run();
        return writer.ToString();
    }
}
