using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using Azure.AI.Vision.ImageAnalysis; // AI Namespace
using Azure;
using System.Text.Json; // For JSON serialization

namespace AzureImageProcessor
{
    public class ResizeImage
    {
        private readonly ILogger<ResizeImage> _logger;

        // Get Key/Endpoint from Environment Variables
        private readonly string _visionEndpoint = Environment.GetEnvironmentVariable("COMPUTER_VISION_ENDPOINT");
        private readonly string _visionKey = Environment.GetEnvironmentVariable("COMPUTER_VISION_KEY");

        public ResizeImage(ILogger<ResizeImage> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ResizeImage))]
        public async Task<AnalysisOutput> Run( // CHANGED: Returns a custom object instead of byte[]
            [BlobTrigger("uploads/{name}", Connection = "AzureWebJobsStorage")] byte[] imageBytes,
            string name)
        {
            _logger.LogInformation($"Processing image: {name}, Size: {imageBytes.Length} bytes");

            // 1. RESIZE LOGIC (Existing)
            byte[] resizedImage;
            using (var stream = new MemoryStream(imageBytes))
            using (var image = await Image.LoadAsync(stream))
            {
                image.Mutate(x => x.Resize(100, 0));
                using (var ms = new MemoryStream())
                {
                    await image.SaveAsJpegAsync(ms);
                    resizedImage = ms.ToArray();
                }
            }

            // 2. AI ANALYSIS LOGIC
            // Note: This requires a Computer Vision resource in your Azure Portal
            ImageAnalysisResult result = null;
            string jsonOutput = "{}";

            try 
            {
                var client = new ImageAnalysisClient(new Uri(_visionEndpoint), new AzureKeyCredential(_visionKey));
                
                // Analyze for Caption and Tags
                result = await client.AnalyzeAsync(
                    BinaryData.FromBytes(imageBytes),
                    VisualFeatures.Caption | VisualFeatures.Tags);

                // Create a simple object to save as JSON
                var analysisData = new 
                {
                    OriginalFile = name,
                    Caption = result.Caption.Text,
                    Confidence = result.Caption.Confidence,
                    Tags = result.Tags.Values
                };

                jsonOutput = JsonSerializer.Serialize(analysisData, new JsonSerializerOptions { WriteIndented = true });
                _logger.LogInformation($"AI Analysis complete: {result.Caption.Text}");
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"AI Analysis failed: {ex.Message}");
                jsonOutput = JsonSerializer.Serialize(new { Error = ex.Message });
            }

            // 3. RETURN BOTH FILES
            return new AnalysisOutput
            {
                ResizedContent = resizedImage,
                JsonContent = jsonOutput
            };
        }
    }

    // Handles writing to two different blobs at once
    public class AnalysisOutput
    {
        [BlobOutput("output/{name}", Connection = "AzureWebJobsStorage")]
        public byte[] ResizedContent { get; set; }

        [BlobOutput("output/{name}.json", Connection = "AzureWebJobsStorage")]
        public string JsonContent { get; set; }
    }
}