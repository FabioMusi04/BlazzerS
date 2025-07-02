using Microsoft.AspNetCore.Mvc;
using Models.http;
using ReportingPortalServer.Services;
using ReportingPortalServer.Services.AppwriteIO;
using ReportingPortalServer.Services.Helpers;

namespace ReportingPortalServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController(ILogger<ReportController> logger, ApplicationDbContext context, IReportService reportService, IUploadFileService uploadFileService, IAppwriteClient appwriteClient) : Controller
    {
        private readonly ILogger<ReportController> _logger = logger;
        private readonly ApplicationDbContext _context = context;
        private readonly IReportService _reportService = reportService;
        private readonly IUploadFileService _uploadFileService = uploadFileService;
        private readonly IAppwriteClient _appwriteClient = appwriteClient;

        [HttpGet("{id}")]
        public ReportResponse GetReportById(int id)
        {
            _logger.LogInformation($"GetReportById request received for id: {id}");

            if (id <= 0)
            {
                return new ReportResponse
                {
                    StatusCode = 400,
                    Message = "Invalid report ID."
                };
            }

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new ReportResponse
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return _reportService.GetReportById(jwt, id, _context);
        }

        [HttpGet]
        public async Task<ReportsPaginatedResponse> GetPaginated([FromQuery] ReportsPaginatedRequest req)
        {
            _logger.LogInformation($"GetPaginated request received with Page={req.Page}, PageSize={req.PageSize}");

            if (req.PageSize <= 0 || req.Page < 0)
            {
                return new ReportsPaginatedResponse
                {
                    StatusCode = 400,
                    Message = "Invalid pagination parameters."
                };
            }

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new ReportsPaginatedResponse
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return await _reportService.GetPaginatedReports(jwt, req, _context);
        }

        [HttpPost]
        public async Task<ReportResponse> CreateReport([FromForm] CreateReportRequest reportRequest)
        {
            try
            {
                _logger.LogInformation("CreateReport request received");

                if (reportRequest == null || string.IsNullOrEmpty(reportRequest.Title) || string.IsNullOrEmpty(reportRequest.Description))
                {
                    return new ReportResponse
                    {
                        StatusCode = 400,
                        Message = "Invalid request data."
                    };
                }

                string? jwt = Utils.GetJwt(HttpContext);
                if (string.IsNullOrEmpty(jwt))
                {
                    return new ReportResponse
                    {
                        StatusCode = 401,
                        Message = "Authorization header is missing or invalid."
                    };
                }

                return await _reportService.CreateReport(reportRequest, jwt, _context, _uploadFileService, _appwriteClient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating report");
                return new ReportResponse
                {
                    StatusCode = 500,
                    Message = $"An error occurred while processing your request. {ex.Message}"
                };
            }
        }

        [HttpDelete("{id}")]
        public ReportResponse DeleteReport(int id)
        {
            _logger.LogInformation($"DeleteReport request received for idRep: {id}");

            if (id <= 0)
            {
                return new ReportResponse
                {
                    StatusCode = 400,
                    Message = "Invalid report ID."
                };
            }

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new ReportResponse
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return _reportService.DeleteReport(id, jwt, _context);
        }

        [HttpPut("{id}")]
        public ReportResponse UpdateReport(int id, [FromForm] CreateReportRequest updateRequest)
        {
            _logger.LogInformation($"UpdateReport request received for idRep: {id}");

            if (id <= 0)
            {
                return new ReportResponse
                {
                    StatusCode = 400,
                    Message = "Invalid report ID."
                };
            }

            if (updateRequest == null)
            {
                return new ReportResponse
                {
                    StatusCode = 400,
                    Message = "Invalid update data."
                };
            }

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new ReportResponse
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return _reportService.UpdateReport(id, updateRequest, jwt, _context);
        }

        [HttpGet("cluster")]
        public async Task<ClusterResponse> GetCluster([FromQuery] ClusterRequest req)
        {
            _logger.LogInformation($"GetCluster request received with MinLat={req.MinLat}, MinLng={req.MinLng}, MaxLat={req.MaxLat}, MaxLng={req.MaxLng}, Zoom={req.Zoom}");
            if (req.MinLat < -90 || req.MaxLat > 90 || req.MinLng < -180 || req.MaxLng > 180)
            {
                return new ClusterResponse
                {
                    StatusCode = 400,
                    Message = "Invalid latitude or longitude values."
                };
            }
            //string? jwt = Utils.GetJwt(HttpContext);
            //if (string.IsNullOrEmpty(jwt))
            //{
            //    return new ClusterResponse
            //    {
            //        StatusCode = 401,
            //        Message = "Authorization header is missing or invalid."
            //    };
            //}
            return await _reportService.GetClusteredReports(/*jwt,*/ req, _context);
        }

        [HttpGet("analytics")]
        public async Task<ReportAnalyticsResponse> GetReportAnalytics([FromQuery] bool IsPersonal)
        {
            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new ReportAnalyticsResponse
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }
            return await _reportService.GetReportAnalytics(jwt, IsPersonal, _context);
        }

        [HttpGet("reportPdf")]
        public async Task<IActionResult> GetReportPdf()
        {
            _logger.LogInformation($"GetReportPdf request received");
            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return Unauthorized("Authorization header is missing or invalid.");
            }

            string randomFileName = $"{Guid.NewGuid()}.pdf";

            return File(await _reportService.GenerateReportSummaryPdfAsync(jwt, _context), "application/pdf", randomFileName);
        }
    }
}
