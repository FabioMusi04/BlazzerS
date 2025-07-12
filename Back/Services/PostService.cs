using Back.Services.AppwriteIO;
using Back.Services.Helpers;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.front;
using Models.http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Back.Services
{
    public interface IPostService
    {
        Task<PostResponse> CreatePostAsync(string JWT, PostForm model, ApplicationDbContext context, IUploadFileService uploadFileService, IAppwriteClient appwriteClient);
        Task<PostResponse> UpdatePostAsync(string JWT, int postId, PostForm model, ApplicationDbContext context);
        Response DeletePostAsync(string JWT, int postId, ApplicationDbContext context);
        PostsPaginatedResponse GetMyPostsAsync(string JWT, PostPaginatedRequest request, ApplicationDbContext context);
        PostResponse GetPostAsync(string JWT, int postId, ApplicationDbContext context);
        PostsPaginatedResponse GetAllPostsAsync(string JWT, PostPaginatedRequest request, ApplicationDbContext context);
    }

    public class PostService : IPostService
    {
        public async Task<PostResponse> CreatePostAsync(string JWT, PostForm model, ApplicationDbContext context, IUploadFileService uploadFileService, IAppwriteClient appwriteClient)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(JWT))
            {
                return new PostResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }

            JwtSecurityToken token = handler.ReadJwtToken(JWT);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return new PostResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }

            User? user = context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return new PostResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "User not found."
                };
            }

            if (model.Image == null || model.Image.Length == 0)
            {
                var post = new Post
                {
                    Content = Utils.SanitizeHtml(model.Content),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    UserId = userId
                };

                context.Posts.Add(post);
                await context.SaveChangesAsync();

                return new PostResponse
                {
                    Post = post,
                    StatusCode = 200
                };
            }

            UploadFileResponse uploadedFile = await uploadFileService.CreateUploadFile(
                new UploadFileRequest { File = model.Image }, context, JWT, appwriteClient);

            if (uploadedFile.StatusCode < 200 || uploadedFile.StatusCode >= 300)
            {
                return new PostResponse
                {
                    StatusCode = uploadedFile.StatusCode,
                    Message = uploadedFile.Message
                };
            }

            var postWithImage = new Post
            {
                Content = model.Content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = userId,
                ImageId = uploadedFile.File?.Id
            };

            context.Posts.Add(postWithImage);
            await context.SaveChangesAsync();

            return new PostResponse
            {
                Post = postWithImage,
                StatusCode = 200
            };

        }

        public async Task<PostResponse> UpdatePostAsync(string JWT, int postId, PostForm model, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(JWT))
            {
                return new PostResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }

            JwtSecurityToken token = handler.ReadJwtToken(JWT);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return new PostResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }

            User? user = context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return new PostResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "User not found."
                };
            }

            var post = await context.Posts.FirstOrDefaultAsync(p => p.Id == postId && p.UserId == userId);
            if (post == null)
            {
                return new PostResponse
                {
                    StatusCode = 404,
                    Message = "Post not found or not owned by user."
                };
            }

            post.Content = Utils.SanitizeHtml(model.Content);
            post.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return new PostResponse
            {
                Post = post,
                StatusCode = 200
            };
        }

        public Response DeletePostAsync(string JWT, int postId, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(JWT))
            {
                return new PostResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }

            JwtSecurityToken token = handler.ReadJwtToken(JWT);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return new PostResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }

            User? user = context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return new PostResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "User not found."
                };
            }

            var post = context.Posts.FirstOrDefault(p => p.Id == postId && p.UserId == userId);
            if (post == null)
            {
                return new Response
                {
                    StatusCode = 404,
                    Message = "Post not found or not owned by user."
                };
            }

            context.Posts.Remove(post);
            context.SaveChanges();

            return new Response
            {
                StatusCode = 200,
                Message = "Post deleted successfully."
            };
        }

        public PostsPaginatedResponse GetMyPostsAsync(string JWT, PostPaginatedRequest request, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(JWT))
            {
                return new PostsPaginatedResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }

            JwtSecurityToken token = handler.ReadJwtToken(JWT);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return new PostsPaginatedResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }

            User? user = context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return new PostsPaginatedResponse
                {
                    StatusCode = (int)System.Net.HttpStatusCode.NotFound,
                    Message = "User not found."
                };
            }

            IQueryable<Post> query = context.Posts.Include(p => p.Image).Include(p => p.User).ThenInclude(u => u.ProfileImage)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new Post()
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Content = p.Content,
                    Likes = p.Likes,
                    ImageId = p.ImageId,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    User = new User
                    {
                        Id = p.User.Id,
                        Email = p.User.Email,
                        ProfileImage = p.User.ProfileImage,
                        ProfileImageId = p.User.ProfileImageId,
                    },
                    Image = p.Image
                });

            var posts = query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PostsPaginatedResponse
            {
                Items = posts,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = query.Count(),
                StatusCode = 200
            };
        }

        public PostResponse GetPostAsync(string JWT, int postId, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(JWT))
            {
                return new PostResponse
                {
                    StatusCode = 400,
                    Message = "JWT not valid."
                };
            }

            JwtSecurityToken token = handler.ReadJwtToken(JWT);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return new PostResponse
                {
                    StatusCode = 400,
                    Message = "JWT does not contain user ID."
                };
            }

            var post = context.Posts
                .Include(p => p.Image)
                .Include(p => p.User)
                    .ThenInclude(u => u.ProfileImage)
                .Where(p => p.Id == postId)
                .Select(p => new Post()
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    Content = p.Content,
                    Likes = p.Likes,
                    ImageId = p.ImageId,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    User = new User
                    {
                        Id = p.User.Id,
                        Email = p.User.Email,
                        ProfileImage = new UploadFile
                        {
                            Id = p.User.ProfileImage.Id,
                            FilePath = p.User.ProfileImage.FilePath,
                            // include other needed fields
                        },
                        ProfileImageId = p.User.ProfileImageId,
                    },
                    Image = p.Image
                })
                .FirstOrDefault();


            if (post == null)
            {
                return new PostResponse
                {
                    StatusCode = 404,
                    Message = "Post not found."
                };
            }

            return new PostResponse
            {
                StatusCode = 200,
                Post = post
            };
        }

        public PostsPaginatedResponse GetAllPostsAsync(string JWT, PostPaginatedRequest request, ApplicationDbContext context)
        {
            JwtSecurityTokenHandler handler = new();
            if (!handler.CanReadToken(JWT))
            {
                return new PostsPaginatedResponse
                {
                    StatusCode = 400,
                    Message = "JWT not valid."
                };
            }

            JwtSecurityToken token = handler.ReadJwtToken(JWT);
            Claim? userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return new PostsPaginatedResponse
                {
                    StatusCode = 400,
                    Message = "JWT does not contain user ID."
                };
            }

            IQueryable<Post> query = context.Posts.Include(p => p.Image).Include(p => p.User).ThenInclude(u => u.ProfileImage).OrderByDescending(p => p.CreatedAt).Select(p => new Post()
            {
                Id = p.Id,
                UserId = p.UserId,
                Content = p.Content,
                Likes = p.Likes,
                ImageId = p.ImageId,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                User = new User
                {
                    Id = p.User.Id,
                    Email = p.User.Email,
                    ProfileImage = p.User.ProfileImage,
                    ProfileImageId = p.User.ProfileImageId,
                },
                Image = p.Image
            });

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(p => p.Content.Contains(request.Search));
            }

            var total = query.Count();
            var posts = query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PostsPaginatedResponse
            {
                StatusCode = 200,
                Message = "Posts fetched successfully.",
                Items = posts,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = total
            };
        }

    }
}
