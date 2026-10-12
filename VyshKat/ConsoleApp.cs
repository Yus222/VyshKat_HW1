using System.Globalization;
using VyshKat.Models;
using VyshKat.Services;

namespace VyshKat;

public sealed class ConsoleApp
{
    private readonly Park _park;
    private readonly TextReader _input;
    private readonly TextWriter _output;

    public ConsoleApp(Park park, TextReader input, TextWriter output)
    {
        _park = park;
        _input = input;
        _output = output;
    }

    public void Run()
    {
        while (true)
        {
            _output.WriteLine("\n=== ВышКат ===");
            _output.WriteLine("1. Добавить транспорт");
            _output.WriteLine("2. Добавить вещь");
            _output.WriteLine("3. Отчёт по парку");
            _output.WriteLine("4. Транспорт для новичков");
            _output.WriteLine("5. Список имущества");
            _output.WriteLine("0. Выход");
            _output.Write("Ваш выбор: ");

            string? command = _input.ReadLine();
            if (command is null or "0")
            {
                return;
            }

            switch (command)
            {
                case "1":
                    AddTransport();
                    break;
                case "2":
                    AddItem();
                    break;
                case "3":
                    ShowReport();
                    break;
                case "4":
                    ShowBeginners();
                    break;
                case "5":
                    ShowInventory();
                    break;
                default:
                    _output.WriteLine("Неизвестная команда");
                    break;
            }
        }
    }

    private void AddTransport()
    {
        int type = ReadInt("Тип (1 - электросамокат, 2 - велосипед, 3 - электровелосипед): ", 1, 3);
        string name = ReadText("Название: ");
        string number = ReadText("Инвентарный номер: ");
        int ease = ReadInt("Простота для новичка (1-10): ", 1, 10);
        bool isWorking = ReadInt("Техника исправна? (1 - да, 0 - нет): ", 0, 1) == 1;

        Transport transport = type switch
        {
            1 => new ElectricScooter(name, number, ease, isWorking, ReadEnergy()),
            2 => new Bicycle(name, number, ease, isWorking),
            _ => new ElectricBike(name, number, ease, isWorking, ReadEnergy())
        };

        try
        {
            bool approved = _park.AddTransport(transport);
            _output.WriteLine(approved ? "Техника принята в парк" : "Техосмотр не пройден. Техника отклонена");
        }
        catch (InvalidOperationException exception)
        {
            _output.WriteLine(exception.Message);
        }
    }

    private void AddItem()
    {
        int type = ReadInt("Тип (1 - шлем, 2 - зарядная станция): ", 1, 2);
        string name = ReadText("Название: ");
        string number = ReadText("Инвентарный номер: ");

        IInventoryItem item = type == 1
            ? new Helmet(name, number)
            : new ChargingStation(name, number, ReadEnergy());

        try
        {
            _park.AddItem(item);
            _output.WriteLine("Вещь добавлена на баланс");
        }
        catch (InvalidOperationException exception)
        {
            _output.WriteLine(exception.Message);
        }
    }

    private void ShowReport()
    {
        _output.WriteLine($"Транспортных средств: {_park.TransportCount}");
        _output.WriteLine($"Всего объектов на балансе: {_park.InventoryCount}");
        _output.WriteLine($"Суточное энергопотребление: {_park.TotalDailyEnergy().ToString(CultureInfo.InvariantCulture)} кВт·ч");
    }

    private void ShowBeginners()
    {
        var transport = _park.ForBeginners();
        if (transport.Count == 0)
        {
            _output.WriteLine("Подходящего транспорта нет");
            return;
        }

        foreach (var item in transport)
        {
            _output.WriteLine($"{item.Name} (№ {item.InventoryNumber}), простота: {item.EaseOfUse}");
        }
    }

    private void ShowInventory()
    {
        if (_park.InventoryCount == 0)
        {
            _output.WriteLine("На балансе пока ничего нет");
            return;
        }

        foreach (var item in _park.Items)
        {
            _output.WriteLine($"{item.Name} (№ {item.InventoryNumber})");
        }
    }

    private string ReadText(string prompt)
    {
        while (true)
        {
            _output.Write(prompt);
            string? value = _input.ReadLine();
            if (value is null)
            {
                throw new EndOfStreamException("Ввод завершён");
            }

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }

            _output.WriteLine("Значение не должно быть пустым");
        }
    }

    private int ReadInt(string prompt, int minimum, int maximum)
    {
        while (true)
        {
            _output.Write(prompt);
            string? value = _input.ReadLine();
            if (value is null)
            {
                throw new EndOfStreamException("Ввод завершён");
            }

            if (int.TryParse(value, out int result) && result >= minimum && result <= maximum)
            {
                return result;
            }

            _output.WriteLine($"Введите число от {minimum} до {maximum}");
        }
    }

    private decimal ReadEnergy()
    {
        while (true)
        {
            _output.Write("Суточное энергопотребление (кВт·ч): ");
            string? value = _input.ReadLine();
            if (value is null)
            {
                throw new EndOfStreamException("Ввод завершён");
            }

            value = value.Replace(',', '.');
            if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal result)
                && result >= 0)
            {
                return result;
            }

            _output.WriteLine("Введите неотрицательное число");
        }
    }
}
