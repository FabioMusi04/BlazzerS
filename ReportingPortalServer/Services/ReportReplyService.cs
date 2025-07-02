using Microsoft.EntityFrameworkCore;
using Models;
using Models.enums;
using Models.http;
using ReportingPortalServer.Services.AppwriteIO;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace ReportingPortalServer.Services
{
    public interface IReportReplyService
    {
        public Task<ReportReplyResponse> CreateReportReply(CreateReportReplyRequest request, ApplicationDbContext context, string jwt, IUploadFileService uploadFileService, IAppwriteClient appwriteClient);
        public ReportReplyResponse DeleteReportReply(int idRep, string jwt, ApplicationDbContext _context);
        public ReportReplyResponse UpdateReportReply(int idRep, string mess, string jwt, ApplicationDbContext _context);
        public Task<ReportRepliesPaginatedResponse> GetPaginatedReportsReplies(string jwt, ReportsReplyPaginatedRequest request, ApplicationDbContext context);
    }

    public class ReportReplyService : IReportReplyService
    {
        public async Task<ReportReplyResponse> CreateReportReply(CreateReportReplyRequest request, ApplicationDbContext context, string jwt, IUploadFileService uploadFileService, IAppwriteClient appwriteClient)
        {
            if (request == null)
            {
                return new ReportReplyResponse
                {
                    StatusCode = 400,
                    Message = "Request cannot be null."
                };
            }
            if (string.IsNullOrEmpty(jwt))
            {
                return new ReportReplyResponse
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }
            if (request.ReportId <= 0)
            {
                return new ReportReplyResponse
                {
                    StatusCode = 400,
                    Message = "Invalid report reply data."
                };
            }

            Report? report = context.Reports.FirstOrDefault(r => r.Id == request.ReportId);
            if (report == null)
            {
                return new ReportReplyResponse
                {
                    StatusCode = 404,
                    Message = "Report not found."
                };
            }

            List<int> fileIds = [];
            if (request.Attachments != null && request.Attachments.Count != 0)
            {
                var response = await uploadFileService.CreateUploadFilesAsync(request, context, jwt, appwriteClient);
                if (response.StatusCode >= 200 && response.StatusCode < 300)
                {
                    if (response.Files.Count != 0)
                    {
                        fileIds = [.. response.Files.Select(f => f.Id)];
                    }
                }
                else
                {
                    return new ReportReplyResponse
                    {
                        StatusCode = response.StatusCode,
                        Message = response.Message
                    };
                }
            }

            var reply = new ReportReply
            {
                Id = request.Id,
                ReportId = request.ReportId,
                UserId = request.UserId,
                Message = request.Message,
                NewStatus = request.NewStatus ?? report.Status,
                Attachment1Id = fileIds.Count > 0 ? fileIds[0] : (int?)null,
                Attachment2Id = fileIds.Count > 1 ? fileIds[1] : (int?)null,
                Attachment3Id = fileIds.Count > 2 ? fileIds[2] : (int?)null,
            };


            if (request.NewStatus != null && request.NewStatus != report.Status)
            {
                Notification emailNotificaiton = new()
                {
                    Title = "Report Status Updated",
                    Message = $"Your report '{report.Title}' has been updated to {reply.NewStatus}.",
                    UserId = report.UserId,
                    Status = NotificationStatusEnum.Unread,
                    Channel = NotificationChannelEnum.Email
                };
                context.Notifications.Add(emailNotificaiton);

                report.Status = request.NewStatus ?? report.Status;

                context.Reports.Update(report);
            }

            context.ReportReplies.Add(reply);

            context.SaveChanges();

            return new ReportReplyResponse
            {
                StatusCode = 201,
                Message = "Report reply created successfully.",
            };
        }
        public ReportReplyResponse DeleteReportReply(int idRep, string jwt, ApplicationDbContext _context)
        {
            if (string.IsNullOrEmpty(jwt))
            {
                return new ReportReplyResponse
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }
            if (idRep <= 0)
            {
                return new ReportReplyResponse
                {
                    StatusCode = 400,
                    Message = "Invalid report reply ID."
                };
            }

            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(jwt))
            {
                return new ReportReplyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }
            var token = handler.ReadJwtToken(jwt);
            var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int parsedUserId))
            {
                return new ReportReplyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }
            var currentUser = _context.Users.FirstOrDefault(u => u.Id == parsedUserId);
            if (currentUser == null)
            {
                return new ReportReplyResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }

            ReportReply? reportReply = currentUser.Role == UserRoleEnum.Admin
                ? _context.ReportReplies.FirstOrDefault(r => r.Id == idRep)
                : _context.ReportReplies.FirstOrDefault(r => r.Id == idRep && r.UserId == currentUser.Id);

            if (reportReply == null)
            {
                return new ReportReplyResponse
                {
                    StatusCode = 404,
                    Message = "Report reply not found or you do not have permission."
                };
            }

            _context.ReportReplies.Remove(reportReply);
            _context.SaveChanges();
            return new ReportReplyResponse
            {
                reportReply = reportReply,
                StatusCode = 200,
                Message = "Report reply deleted successfully."
            };
        }
        public ReportReplyResponse UpdateReportReply(int idRep, string mess, string jwt, ApplicationDbContext _context)
        {
            if (string.IsNullOrEmpty(jwt))
            {
                return new ReportReplyResponse
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                };
            }
            if (idRep <= 0)
            {
                return new ReportReplyResponse
                {
                    StatusCode = 400,
                    Message = "Invalid report reply ID."
                };
            }

            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(jwt))
            {
                return new ReportReplyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                };
            }
            var token = handler.ReadJwtToken(jwt);
            var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int parsedUserId))
            {
                return new ReportReplyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                };
            }
            var currentUser = _context.Users.FirstOrDefault(u => u.Id == parsedUserId);
            if (currentUser == null)
            {
                return new ReportReplyResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                };
            }

            ReportReply? reportReply = currentUser.Role == UserRoleEnum.Admin
                ? _context.ReportReplies.FirstOrDefault(r => r.Id == idRep)
                : _context.ReportReplies.FirstOrDefault(r => r.Id == idRep && r.UserId == currentUser.Id);

            if (reportReply == null)
            {
                return new ReportReplyResponse
                {
                    StatusCode = 404,
                    Message = "Report reply not found or you do not have permission."
                };
            }
            reportReply.Message = mess;
            _context.SaveChanges();
            return new ReportReplyResponse
            {
                reportReply = reportReply,
                StatusCode = 200,
                Message = "Report reply updated successfully."
            };
        }
        public Task<ReportRepliesPaginatedResponse> GetPaginatedReportsReplies(string jwt, ReportsReplyPaginatedRequest request, ApplicationDbContext context)
        {
            if (string.IsNullOrEmpty(jwt))
            {
                return Task.FromResult(new ReportRepliesPaginatedResponse
                {
                    StatusCode = 401,
                    Message = "Authorization header is missing or invalid."
                });
            }

            if (request.PageSize <= 0 || request.Page < 0)
            {
                return Task.FromResult(new ReportRepliesPaginatedResponse
                {
                    StatusCode = 400,
                    Message = "Invalid pagination parameters."
                });
            }

            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(jwt))
            {
                return Task.FromResult(new ReportRepliesPaginatedResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT not valid."
                });

            }
            var token = handler.ReadJwtToken(jwt);
            var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int parsedUserId))
            {
                return Task.FromResult(new ReportRepliesPaginatedResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "JWT does not contain user ID."
                });
            }

            var currentUser = context.Users.FirstOrDefault(u => u.Id == parsedUserId);
            if (currentUser == null)
            {
                return Task.FromResult(new ReportRepliesPaginatedResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Authenticated user not found."
                });
            }


            IQueryable<ReportReply> query = context.ReportReplies;

            query = query.Where(r => r.ReportId == request.ReportId);

            int totalCount = query.Count();
            List<ReportReply> repliesList = [.. query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(n => n.Attachment1)
                .Include(n => n.Attachment2)
                .Include(n => n.Attachment3)
                .Include(n => n.User)
            ];

            return Task.FromResult(new ReportRepliesPaginatedResponse
            {
                StatusCode = 200,
                Message = "Report replies retrieved successfully.",
                TotalCount = totalCount,
                Items = repliesList
            });
        }
    }
}
