namespace Interkom.Core.Application.Interfaces
{
    public interface IZenitelIntercomService
    {
        Task InviteAsync();
        Task AckAsync();
        Task CancelAsync();
        Task OptionsAsync();
        Task ByeAsync();

    }
}
