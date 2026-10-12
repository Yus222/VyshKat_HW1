using VyshKat.Models;

namespace VyshKat.Services;

public interface IServiceCenter
{
    bool Approve(Transport transport);
}
