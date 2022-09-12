using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using YgoProgressionDuels.Shared;

namespace YgoProgressionDuels.Services
{
    public class FileUploader : IFileUploader
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileUploader(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> UploadFileAsync(IBrowserFile file, string desiredSubdirectory = "")
        {
            string fileDirectory = Path.Combine(_webHostEnvironment.WebRootPath, desiredSubdirectory.Trim());

            // Ensure that the fileDirectory exists
            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }

            // Give this file a guid name
            string fileName = Path.ChangeExtension(Guid.NewGuid().ToString(), Path.GetExtension(file.Name));
            string filePath = Path.Combine(fileDirectory, fileName);

            // Store the file
            using MemoryStream memStream = new();
            await file.OpenReadStream().CopyToAsync(memStream);
            using FileStream fileStream = new(filePath, FileMode.CreateNew, FileAccess.Write);
            memStream.WriteTo(fileStream);

            // Return the path to the file, without the root
            return Path.Combine(Path.DirectorySeparatorChar.ToString(), desiredSubdirectory.Trim(), fileName);
        }

        public async Task<string> UploadImageAsync(IBrowserFile file, string desiredSubdirectory = "")
        {
            // Ensure that this is an accepted image format
            if (!FileIsAcceptedImageFormat(file.Name))
            {
                throw new InvalidDataException($"File {file.Name} is not in an accepted image format");
            }

            return await UploadFileAsync(file, Path.Combine("Images", desiredSubdirectory.Trim()));
        }

        public async Task<string> UploadUserAvatarAsync(IBrowserFile file)
        {
            return await UploadImageAsync(file, "Avatars");
        }

        public void DeleteFile(string filePath)
        {
            File.Delete(Path.Combine(_webHostEnvironment.WebRootPath, filePath));
        }

        public bool FileIsAcceptedImageFormat(string fileName)
        {
            string extension = Path.GetExtension(fileName);

            if (string.IsNullOrWhiteSpace(extension))
            {
                return false;
            }

            return Constants.AllowedImageExtensions.Any(ext => ext.Equals(extension, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
