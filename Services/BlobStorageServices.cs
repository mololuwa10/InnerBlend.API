using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MetadataExtractor;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace InnerBlend.API.Services
{
    public class BlobStorageServices
    {
        public readonly BlobContainerClient? _containerClient;
        
        public BlobStorageServices(IConfiguration configuration) 
        {
            var connectionString = configuration["AzureBlobStorage:ConnectionString"];
            var containerName = configuration["AzureBlobStorage:ContainerName"];
            _containerClient = new BlobContainerClient(connectionString, containerName);
        }
        
        public async Task<string> UploadAsync (Stream stream, string fileName) 
        {
            await _containerClient!.CreateIfNotExistsAsync(PublicAccessType.Blob);
            
            var blobClient = _containerClient.GetBlobClient(Guid.NewGuid() + Path.GetExtension(fileName));
            await blobClient.UploadAsync(stream, overwrite: true);

            return blobClient.Uri.ToString();
        }
        
        public async Task DeleteAsync(string fileUrl) 
        {
            var fileName = Path.GetFileName(new Uri(fileUrl).AbsolutePath);
            var blobClient = _containerClient!.GetBlobClient(fileName);
            await blobClient.DeleteAsync();
        }
        
        // Implementing this to reduces storage cost, bandwidth, and improves app speed.
        public async Task<Stream>  CompressAndResizeImageAsync(IFormFile file) 
        {
            using var image = Image.Load(file.OpenReadStream());

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(800, 0), // Resize width to 800px, keep aspect ratio
                Mode = ResizeMode.Max
            }));

            var output = new MemoryStream();
            await image.SaveAsJpegAsync(output); // or SaveAsPngAsync
            output.Position = 0;
            return output;
        }
        
        public Dictionary<string, string> GetImageMetadata(IFormFile file) 
        {
            var metadata = ImageMetadataReader.ReadMetadata(file.OpenReadStream());
            var result = new Dictionary<string, string>();
            
            foreach (var directory in metadata) 
            {
                foreach (var tag in directory.Tags) 
                {
                    if (!result.ContainsKey(tag.Name)) 
                    {
                        result[tag.Name] = tag.Description ?? string.Empty;
                    }
                }
            }
            
            return result;
        }
    }
}