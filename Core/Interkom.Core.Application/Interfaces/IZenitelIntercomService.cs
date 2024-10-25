namespace Interkom.Core.Application.Interfaces
{
    public interface IZenitelIntercomService
    {
        Task CallAsync();
        Task ClientsAsync();
        Task StateAsync();
        Task MessagesAsync();
        Task StationStateAsync();

        Task InviteAsync();
        Task AckAsync();
        Task CancelAsync();
        Task OptionsAsync();
        Task ByeAsync();

    }
}
