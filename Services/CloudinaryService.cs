using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace AutoGest.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<string?> SubirImagenAsync(IFormFile archivo, string carpeta = "autogest")
        {
            if (archivo == null || archivo.Length == 0) return null;

            using var stream = archivo.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(archivo.FileName, stream),
                Folder = carpeta,
                Transformation = new Transformation().Quality("auto").FetchFormat("auto")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);
            return result.SecureUrl?.ToString();
        }
    }
}