using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace AzureImageProcessor
{
    public class ResizeImage
    {
        private readonly ILogger<ResizeImage> _logger;

        public ResizeImage(ILogger<ResizeImage> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ResizeImage))]
        [BlobOutput("output/{name}", Connection = "AzureWebJobsStorage")]
        public async Task<byte[]> Run(
            // CHANGE 1: We ask for byte[] (raw data) instead of Stream
            [BlobTrigger("uploads/{name}", Connection = "AzureWebJobsStorage")] byte[] imageBytes, 
            string name)
        {
            _logger.LogInformation($"Processing image: {name}, Size: {imageBytes.Length} bytes");

            // CHANGE 2: We wrap the bytes in a stream manually so ImageSharp can read it
            using (var stream = new MemoryStream(imageBytes))
            using (var image = await Image.LoadAsync(stream))
            {
                // Resize to 100px wide
                image.Mutate(x => x.Resize(100, 0));
                
                using (var ms = new MemoryStream())
                {
                    await image.SaveAsJpegAsync(ms);
                    _logger.LogInformation($"Saved resized version of {name}!");
                    
                    // Return the result
                    return ms.ToArray(); 
                }
            }
        }
    }
}