using Microsoft.AspNetCore.Mvc;
using Models;
using Models.http;
using ReportingPortalServer.Services;
using ReportingPortalServer.Services.Helpers;

namespace ReportingPortalServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController(ILogger<CategoryController> logger, ICategoryService categoryService, ApplicationDbContext context) : Controller
    {
        private readonly ILogger<CategoryController> _logger = logger;
        private readonly ICategoryService _categoryService = categoryService;
        private readonly ApplicationDbContext context = context;

        [HttpGet("{id}")]
        public CategoryResponse GetCategory(int id)
        {
            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new CategoryResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }

            _logger.LogInformation($"GetCategory request received for ID: {id}");
            return _categoryService.GetCategory(id, context);
        }

        [HttpGet]
        public CategoriesPaginatedResponse GetPaginatedCategories([FromQuery] CategoriesPaginatedRequest request)
        {
            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new CategoriesPaginatedResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.Unauthorized,
                    Message = "Authorization header is missing or invalid."
                };
            }

            _logger.LogInformation($"GetPaginatedCategories request received: Page={request.Page}, PageSize={request.PageSize}");
            return _categoryService.GetPaginatedCategories(request, context);
        }

        [HttpPost]
        public Response CreateCategory([FromBody] Category category)
        {
            _logger.LogInformation("CreateCategory request received");

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new Response
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return _categoryService.CreateCategory(category, context, jwt);
        }

        [HttpPut("{id}")]
        public Response UpdateCategory(int id, [FromBody] Category category)
        {
            _logger.LogInformation($"UpdateCategory request received for ID: {id}");

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new Response
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }

            category.Id = id;
            return _categoryService.UpdateCategory(category, context, jwt);
        }

        [HttpDelete("{id}")]
        public Response DeleteCategory(int id)
        {
            _logger.LogInformation($"DeleteCategory request received for ID: {id}");

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return new Response
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }

            return _categoryService.DeleteCategory(id, context, jwt);
        }
    }
}
