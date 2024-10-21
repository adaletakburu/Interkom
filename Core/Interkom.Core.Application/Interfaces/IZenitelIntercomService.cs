namespace Interkom.Core.Application.Interfaces
{
    public interface IZenitelIntercomService
    {
        Task CallAsync();
        Task ClientsAsync();
        Task StateAsync();
        Task MessagesAsync();
        Task StationStateAsync();

    }
}
