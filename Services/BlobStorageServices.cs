using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
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

            Console.WriteLine($"Blob Connection String: {connectionString}");
            Console.WriteLine($"Blob Container Name: {containerName}");
        }

        public async Task<string> UploadAsync(Stream stream, string fileName)
        {
            await _containerClient!.CreateIfNotExistsAsync(PublicAccessType.None);

            var blobClient = _containerClient.GetBlobClient(
                Guid.NewGuid() + Path.GetExtension(fileName)
            );
            await blobClient.UploadAsync(stream, overwrite: true);

            // Generating a SAS token
            var sasUri = GenerateSasUriForBlob(blobClient, TimeSpan.FromHours(1));

            return sasUri.ToString();
        }

        public async Task DeleteAsync(string fileUrl)
        {
            var fileName = Path.GetFileName(new Uri(fileUrl).AbsolutePath);
            var blobClient = _containerClient!.GetBlobClient(fileName);
            await blobClient.DeleteAsync();
        }

        // Implementing this to reduces storage cost, bandwidth, and improves app speed.
        public async Task<Stream> CompressAndResizeImageAsync(IFormFile file)
        {
            using var image = Image.Load(file.OpenReadStream());

            image.Mutate(x =>
                x.Resize(
                    new ResizeOptions
                    {
                        Size = new Size(800, 0), // Resize width to 800px, keep aspect ratio
                        Mode = ResizeMode.Max,
                    }
                )
            );

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

        private Uri GenerateSasUriForBlob(BlobClient blobClient, TimeSpan duration)
        {
            if (!_containerClient!.CanGenerateSasUri)
            {
                throw new InvalidOperationException(
                    "The container client does not support generating SAS URIs."
                );
            }

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = blobClient.BlobContainerName,
                BlobName = blobClient.Name,
                Resource = "b", // Indicates it's a blob, not a container
                ExpiresOn = DateTimeOffset.UtcNow.Add(duration),
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read); // Only allow reading the image

            // Generate the full URI with SAS token
            return blobClient.GenerateSasUri(sasBuilder);
        }
    }
}
