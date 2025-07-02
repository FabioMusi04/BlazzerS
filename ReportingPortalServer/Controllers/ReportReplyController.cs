using Microsoft.AspNetCore.Mvc;
using Models.http;
using ReportingPortalServer.Services;
using ReportingPortalServer.Services.AppwriteIO;
using ReportingPortalServer.Services.Helpers;

namespace ReportingPortalServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class ReportReplyController(ILogger<ReportReplyController> logger, ApplicationDbContext context, IReportReplyService reportReplyService, IUploadFileService uploadFileService, IAppwriteClient appwriteClient) : Controller
    {

        private readonly ILogger<ReportReplyController> _logger = logger;
        private readonly ApplicationDbContext _context = context;
        private readonly IReportReplyService _reportReplyService = reportReplyService;
        private readonly IUploadFileService _uploadFileService = uploadFileService;
        private readonly IAppwriteClient _appwriteClient = appwriteClient;

        [HttpPost]
        public async Task<ReportReplyResponse> CreateReportReply([FromForm] CreateReportReplyRequest req)
        {
            _logger.LogInformation("CreateReportReply request received");

            if (req == null || string.IsNullOrEmpty(req.Message))
            {
                return new ReportReplyResponse
                {
                    StatusCode = 400,
                    Message = "Invalid request data."
                };
            }

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new ReportReplyResponse
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return await _reportReplyService.CreateReportReply(req, _context, jwt, _uploadFileService, _appwriteClient);
        }

        [HttpDelete("{id}")]
        public ReportReplyResponse DeleteReportReply(int id)
        {
            _logger.LogInformation($"DeleteReportReply request received for id: {id}");
            if (id <= 0)
            {
                return new ReportReplyResponse
                {
                    StatusCode = 400,
                    Message = "Invalid report reply ID."
                };
            }
            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new ReportReplyResponse
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }
            return _reportReplyService.DeleteReportReply(id, jwt, _context);
        }

        [HttpPut("{id}")]
        public ReportReplyResponse UpdateReportReply(int id, [FromForm] UpdateReportReplyRequest req)
        {
            _logger.LogInformation($"UpdateReportReply request received for id: {id}");
            if (id <= 0 || string.IsNullOrEmpty(req.Message))
            {
                return new ReportReplyResponse
                {
                    StatusCode = 400,
                    Message = "Invalid report reply data."
                };
            }
            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new ReportReplyResponse
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }
            return _reportReplyService.UpdateReportReply(id, req.Message, jwt, _context);
        }

        [HttpGet]
        public async Task<ReportRepliesPaginatedResponse> GetPaginatedReportReply([FromQuery] ReportsReplyPaginatedRequest req)
        {
            _logger.LogInformation($"GetPaginatedReportReply request received with Page={req.Page}, PageSize={req.PageSize}");
            if (req.PageSize <= 0 || req.Page < 0)
            {
                return new ReportRepliesPaginatedResponse
                {
                    StatusCode = 400,
                    Message = "Invalid pagination parameters."
                };
            }
            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new ReportRepliesPaginatedResponse
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }
            return await _reportReplyService.GetPaginatedReportsReplies(jwt, req, _context);
        }
    }
}
