using Microsoft.EntityFrameworkCore;
using Models;
using Models.enums;
using Models.http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ReportingPortalServer.Services
{
    public interface ICategoryService
    {
        CategoryResponse GetCategory(int id, ApplicationDbContext context);
        CategoriesPaginatedResponse GetPaginatedCategories(CategoriesPaginatedRequest request, ApplicationDbContext context);
        Response CreateCategory(Category category, ApplicationDbContext context, string JWT);
        Response UpdateCategory(Category category, ApplicationDbContext context, string JWT);
        Response DeleteCategory(int id, ApplicationDbContext context, string JWT);
    }

    public class CategoryService : ICategoryService
    {
        public CategoryResponse GetCategory(int id, ApplicationDbContext context)
        {
            Category? category = context.Categories.FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return new CategoryResponse
                {
                    StatusCode = 404,
                    Message = "Category not found."
                };
            }

            return new CategoryResponse
            {
                Category = category,
                StatusCode = 200
            };
        }

        public CategoriesPaginatedResponse GetPaginatedCategories(CategoriesPaginatedRequest request, ApplicationDbContext context)
        {
            IQueryable<Category> query = context.Categories.AsQueryable();
            bool asc = request.SortAscending ?? true;

            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(u =>
                    u.Name.ToLower().Contains(request.Search.ToLower())
                );
            }

            if (!string.IsNullOrEmpty(request.SortField))
            {
                if (asc)
                {
                    query = query.OrderBy(u => EF.Property<object>(u, request.SortField));
                }
                else
                {
                    query = query.OrderByDescending(u => EF.Property<object>(u, request.SortField));
                }
            }

            List<Category> items = [.. query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)];

            return new CategoriesPaginatedResponse
            {
                Items = items,
                TotalCount = query.Count(),
                Page = request.Page,
                PageSize = request.PageSize,
                StatusCode = 200
            };
        }

        public Response CreateCategory(Category category, ApplicationDbContext context, string JWT)
        {
            Response? authCheck = CheckAdminAuthorization(context, JWT);
            if (authCheck != null) return authCheck;

            context.Categories.Add(category);
            context.SaveChanges();

            return new Response
            {
                StatusCode = 201,
                Message = "Category created successfully."
            };
        }

        public Response UpdateCategory(Category category, ApplicationDbContext context, string JWT)
        {
            Response? authCheck = CheckAdminAuthorization(context, JWT);
            if (authCheck != null) return authCheck;

            Category? existing = context.Categories.FirstOrDefault(c => c.Id == category.Id);
            if (existing == null)
            {
                return new Response
                {
                    StatusCode = 404,
                    Message = "Category not found."
                };
            }

            existing.Name = category.Name;
            existing.UpdatedAt = DateTime.UtcNow;

            context.SaveChanges();

            return new Response
            {
                StatusCode = 200,
                Message = "Category updated successfully."
            };
        }

        public Response DeleteCategory(int id, ApplicationDbContext context, string JWT)
        {
            Response? authCheck = CheckAdminAuthorization(context, JWT);
            if (authCheck != null) return authCheck;

            Category? category = context.Categories.FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return new Response
                {
                    StatusCode = 404,
                    Message = "Category not found."
                };
            }

            context.Categories.Remove(category);
            context.SaveChanges();

            return new Response
            {
                StatusCode = 200,
                Message = "Category deleted successfully."
            };
        }

        private Response? CheckAdminAuthorization(ApplicationDbContext context, string JWT)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(JWT))
            {
                return new Response
                {
                    StatusCode = 400,
                    Message = "JWT not valid."
                };
            }

            JwtSecurityToken token = handler.ReadJwtToken(JWT);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return new Response
                {
                    StatusCode = 400,
                    Message = "JWT does not contain user ID."
                };
            }

            User? currentUser = context.Users.FirstOrDefault(u => u.Id == userId);
            if (currentUser == null)
            {
                return new Response
                {
                    StatusCode = 404,
                    Message = "Authenticated user not found."
                };
            }

            if (currentUser.Role != UserRoleEnum.Admin)
            {
                return new Response
                {
                    StatusCode = 403,
                    Message = "Only administrators can access this resource."
                };
            }

            return null;
        }
    }
}
