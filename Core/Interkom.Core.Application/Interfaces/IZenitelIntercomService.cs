using System;

namespace Interkom.Core.Application.Interfaces
{
    public interface IZenitelIntercomService
    {
        List<string> GetFullStationList();

        void MakeAnnounce(string command);

        bool SendAudioToIntercom(string filePath, string rtpAddress);

        Task<bool> UploadAudioFileAsync(string ip, string username, string password, string filePath, string group, int index);
    }
}
