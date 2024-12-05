namespace Interkom.Core.Application.Interfaces
{
    public interface IZenitelIntercomService
    {
        List<string> GetFullStationList();

        void MakeAnnounce(string command);

        bool SendAudioToIntercom(string filePath, string rtpAddress);
    }
}
