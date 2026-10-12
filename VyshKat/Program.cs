using Microsoft.Extensions.DependencyInjection;
using VyshKat;
using VyshKat.Services;

var services = new ServiceCollection();
services.AddSingleton<IServiceCenter, ServiceCenter>();
services.AddSingleton<Park>();
services.AddSingleton<TextReader>(Console.In);
services.AddSingleton<TextWriter>(Console.Out);
services.AddTransient<ConsoleApp>();

using var provider = services.BuildServiceProvider();
provider.GetRequiredService<ConsoleApp>().Run();
