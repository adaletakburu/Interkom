namespace Interkom.Core.Application.Interfaces
{
    public interface IZenitelIntercomService
    {
        Task ClientsAsync();
        Task StateAsync();
        Task MessagesAsync();
        Task StationStateAsync();

    }
}
