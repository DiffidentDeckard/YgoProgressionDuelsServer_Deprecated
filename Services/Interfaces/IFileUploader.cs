using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;

namespace YgoProgressionDuels.Services
{
    public interface IFileUploader
    {
        /// <summary>
        /// Uploads a file to the specified subdirectory inside wwwroot.
        /// Returns the the relative filepath (from wwwroot) to the stored file.
        /// </summary>
        public Task<string> UploadFileAsync(IBrowserFile file, string desiredSubdirectory = "");

        /// <summary>
        /// Uploads an image to the specified subdirectory inside wwwroot/Images.
        /// If the file is not an accepted image type, this method will throw an InvalidDataException.
        /// Returns the the relative filepath (from wwwroot) to the stored file.
        /// </summary>
        public Task<string> UploadImageAsync(IBrowserFile file, string desiredSubdirectory = "");

        /// <summary>
        /// Uploads an image to wwwroot/Images/Avatars.
        /// If the file is not an accepted image type, this method will throw an InvalidDataException.
        /// Returns the the relative filepath (from wwwroot) to the stored file.
        /// </summary>
        public Task<string> UploadUserAvatarAsync(IBrowserFile file);

        /// <summary>
        /// Deletes a file at the specified filePath inside wwwroot.
        /// If the directory does not exist, throws an error.
        /// If the file does not exist, no error is thrown.
        /// </summary>
        public void DeleteFile(string filePath);

        /// <summary>
        /// Returns true if the file has an accept image extension
        /// </summary>
        public bool FileIsAcceptedImageFormat(string fileName);
    }
}
