using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

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
        
        public async Task<string> UploadAsync (IFormFile file) 
        {
            await _containerClient!.CreateIfNotExistsAsync(PublicAccessType.Blob);
            
            var blobClient = _containerClient.GetBlobClient(Guid.NewGuid() + Path.GetExtension(file.FileName));
            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);
            
            return blobClient.Uri.ToString();
        }
        
        public async Task DeleteAsync(string fileUrl) 
        {
            var fileName = Path.GetFileName(new Uri(fileUrl).AbsolutePath);
            var blobClient = _containerClient!.GetBlobClient(fileName);
            await blobClient.DeleteAsync();
        }
    }
}