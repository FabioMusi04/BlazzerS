using Microsoft.AspNetCore.Http;
using Models.enums;
using System.ComponentModel.DataAnnotations;

namespace Models.http
{
    public class Request
    {

    }

    public class ClusterRequest : Request
    {
        public double MinLat { get; set; }
        public double MinLng { get; set; }
        public double MaxLat { get; set; }
        public double MaxLng { get; set; }
        public int Zoom { get; set; }
        public int? CategoryId { get; set; } = null;
        public ReportStatusEnum? Status { get; set; } = null;
    }

    public class PagedRequest
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; } = default!;
        public string? SortField { get; set; } = default!;
        public bool? SortAscending { get; set; } = null;
    }

    public class LoginRequest : Request
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string Password { get; set; } = default!;
    }

    public class RegisterRequest : Request
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string Password { get; set; } = default!;

        [Required]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = default!;

        [Required]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string Name { get; set; } = default!;
        [Required]
        [StringLength(100, ErrorMessage = "Surname cannot be longer than 100 characters.")]
        public string Surname { get; set; } = default!;
    }

    public class ReportRequest : Request
    {
        [Required]
        public string Title { get; set; } = default!;
        [Required]
        public string Description { get; set; } = default!;
        [Required]
        public int CategoryId { get; set; } = default!;
        [Required]
        public string Location { get; set; } = default!;
        public string LocationDetail { get; set; } = default!;
    }

    public class NotificationConnectRequest : Request
    {
        [Required]
        public string Client { get; set; } = default!;

        [Required]
        public string Endpoint { get; set; } = default!;

        [Required]
        public string P256dh { get; set; } = default!;

        [Required]
        public string Auth { get; set; } = default!;
    }

    public class ReadNotificationRequest : Request
    {
        public int UserId { get; set; } = default!;
        public int NotificationId { get; set; } = default!;
    }

    public class VerificationTokenRequest : Request
    {
        public string Token { get; set; } = default!;
    }

    public class VerificationTokenRetryRequest : Request
    {
        public int UserId { get; set; } = default!;
    }

    public class ResetPasswordRequest : Request
    {
        public string Email { get; set; } = default!;
    }

    public class CreateNotificationRequest : Request
    {
        [Required]
        public int UserId { get; set; } = default!;

        [Required]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters.")]
        public string Title { get; set; } = default!;

        [Required]
        [StringLength(500, ErrorMessage = "Message cannot be longer than 500 characters.")]
        public string Message { get; set; } = default!;

        public NotificationStatusEnum Status { get; set; } = NotificationStatusEnum.Unread;
        public NotificationChannelEnum Channel { get; set; } = NotificationChannelEnum.App;
    }

    public class CreateReportRequest
    {
        public string? Id { get; set; } = null;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? LocationDetail { get; set; } = string.Empty;
        public string Latitude { get; set; } = string.Empty;
        public string Longitude { get; set; } = string.Empty;
        public IFormFile? File { get; set; } = default!;
    }

    public class UploadFileRequest
    {
        public IFormFile? File { get; set; } = default!;
    }

    public class UpdateNotificationRequest : Request
    {
        public int NotificationId { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string Message { get; set; } = default!;
    }

    public class ReportsPaginatedRequest : PagedRequest
    {
        public ReportStatusEnum? Status { get; set; } = default!;
        public bool IsPersonal { get; set; } = false;
    }

    public class UsersPaginatedRequest : PagedRequest
    {
        public UserRoleEnum? Role { get; set; } = default!;
        public bool? EmailConfirmed { get; set; } = default!;
    }

    public class NotificationsPaginatedRequest : PagedRequest
    {
        public NotificationStatusEnum? Status { get; set; } = default!;
    }

    public class CategoriesPaginatedRequest : PagedRequest
    {

    }

    public class CreateReportReplyRequest
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; } = string.Empty;
        public ReportStatusEnum? NewStatus { get; set; } = null;
        public List<IFormFile>? Attachments { get; set; } = default!;
    }

    public class UpdateReportReplyRequest : Request
    {
        public string Message { get; set; } = string.Empty;
    }

    public class ReportsReplyPaginatedRequest : PagedRequest
    {
        public int ReportId { get; set; }
    }
}