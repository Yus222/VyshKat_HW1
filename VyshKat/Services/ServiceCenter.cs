using VyshKat.Models;

namespace VyshKat.Services;

public sealed class ServiceCenter : IServiceCenter
{
    public bool Approve(Transport transport)
    {
        return transport.IsWorking;
    }
}
