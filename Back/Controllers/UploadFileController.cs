using Microsoft.AspNetCore.Mvc;
using Models.http;
using ReportingPortalServer.Services;
using ReportingPortalServer.Services.AppwriteIO;
using ReportingPortalServer.Services.Helpers;

namespace ReportingPortalServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadFileController(ILogger<UploadFileController> logger, IUploadFileService uploadFileService, IAppwriteClient appwriteClient, ApplicationDbContext context) : Controller
    {
        private readonly ILogger<UploadFileController> _logger = logger;
        private readonly IUploadFileService _uploadFileService = uploadFileService;
        private readonly IAppwriteClient _appwriteClient = appwriteClient;
        private readonly ApplicationDbContext context = context;

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<UploadFileResponse> CreateUploadFile([FromForm] UploadFileRequest request)
        {
            _logger.LogInformation("CreateUploadFile request received");

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new UploadFileResponse
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return await _uploadFileService.CreateUploadFile(request, context, jwt, _appwriteClient);
        }

        [HttpDelete("{id}")]
        public async Task<Response> DeleteUploadFile(int id)
        {
            _logger.LogInformation($"DeleteUploadFile request received for ID: {id}");

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new Response
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return await _uploadFileService.DeleteUploadFile(id, context, jwt, _appwriteClient);
        }
    }
}
