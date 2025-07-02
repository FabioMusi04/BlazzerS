using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.http;
using ReportingPortalServer.Services.AppwriteIO;
using ReportingPortalServer.Services.Helpers;

namespace ReportingPortalServer.Services
{
    public interface IUploadFileService
    {
        Task<UploadFileResponse> CreateUploadFile(dynamic request, ApplicationDbContext context, string jwt, IAppwriteClient appwriteClient);
        Task<UploadFilesResponse> CreateUploadFilesAsync(dynamic request, ApplicationDbContext context, string jwt, IAppwriteClient appwriteClient);
        Task<Response> DeleteUploadFile(int id, ApplicationDbContext context, string jwt, IAppwriteClient appwriteClient);
    }

    public class UploadFileService : IUploadFileService
    {
        public async Task<UploadFileResponse> CreateUploadFile(dynamic request, ApplicationDbContext context, string jwt, IAppwriteClient appwriteClient)
        {
            if (request.File == null || request.File.Length == 0)
            {
                return new UploadFileResponse
                {
                    StatusCode = 400,
                    Message = "No file uploaded."
                };
            }

            string tempDir = Path.Combine(Path.GetTempPath(), "uploads");
            Directory.CreateDirectory(tempDir);

            string tempFilePath = Path.Combine(tempDir, Guid.NewGuid() + Path.GetExtension(request.File.FileName));

            using (FileStream stream = new(tempFilePath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream);
            }

            string randomFileName = $"{Guid.NewGuid()}{Path.GetExtension(request.File.FileName)}";

            string appwriteFileId = await appwriteClient.UploadFileAsync("68376d5cec9c85d2b5d3", tempFilePath, randomFileName);

            UploadFile uploadFile = new()
            {
                FileName = randomFileName,
                FilePath = $"https://fra.cloud.appwrite.io/v1/storage/buckets/68376d5cec9c85d2b5d3/files/{appwriteFileId}/view?project=683724270008f8aac069",
                ContentType = request.File.ContentType,
                Format = Utils.GetFormatFromFileExtension(Path.GetExtension(randomFileName)),
                Size = request.File.Length,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                if (System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }
            }
            catch (Exception ex)
            {
                return new UploadFileResponse
                {
                    StatusCode = 500,
                    Message = $"Error deleting temporary file: {ex.Message}"
                };
            }

            context.UploadFiles.Add(uploadFile);
            context.SaveChanges();

            return new UploadFileResponse
            {
                StatusCode = 201,
                Message = "File uploaded successfully.",
                File = uploadFile
            };
        }

        public async Task<UploadFilesResponse> CreateUploadFilesAsync(dynamic request, ApplicationDbContext context, string jwt, IAppwriteClient appwriteClient)
        {
            if (request.Attachments == null || request.Attachments.Count == 0)
            {
                return new UploadFilesResponse
                {
                    StatusCode = 400,
                    Message = "No files uploaded."
                };
            }

            var uploadedFiles = new List<UploadFile>();
            int uploadLimit = Math.Min(3, request.Attachments.Count);

            for (int i = 0; i < uploadLimit; i++)
            {
                if (request.Attachments[i] is not IFormFile file)
                {
                    continue;
                }

                Console.WriteLine($"Processing file: {file.FileName}, Size: {file.Length} bytes");

                var result = await Utils.HandleSingleUploadAsync(file, appwriteClient, context);

                if (result.StatusCode == 201 && result.File != null)
                {
                    uploadedFiles.Add(result.File);
                }
                else
                {
                    Console.WriteLine($"Upload failed for file: {file.FileName}, message: {result.Message}");
                }
            }

            if (uploadedFiles.Count > 0)
            {
                await context.SaveChangesAsync();

                return new UploadFilesResponse
                {
                    StatusCode = 201,
                    Message = "Files uploaded successfully.",
                    Files = uploadedFiles
                };
            }

            return new UploadFilesResponse
            {
                StatusCode = 500,
                Message = "Failed to upload files.",
                Files = uploadedFiles
            };
        }


        public async Task<Response> DeleteUploadFile(int id, ApplicationDbContext context, string jwt, IAppwriteClient appwriteClient)
        {
            UploadFile? file = context.UploadFiles.Find(id);
            if (file == null)
            {
                return new Response
                {
                    StatusCode = 404,
                    Message = "File not found."
                };
            }

            string path = file.FilePath;
            string idAppwrite = path.Split("files/")[1].Split("/view")[0];

            await appwriteClient.DeleteFileAsync("68376d5cec9c85d2b5d3", idAppwrite);

            context.UploadFiles.Remove(file);
            context.SaveChanges();

            return new Response
            {
                StatusCode = 200,
                Message = "File deleted successfully."
            };
        }
    }
}
