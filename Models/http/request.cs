using Microsoft.AspNetCore.Http;
using Models.enums;
using System.ComponentModel.DataAnnotations;

namespace Models.http
{
    public class Request
    {

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

    public class UsersPaginatedRequest : PagedRequest
    {
        public UserRoleEnum? Role { get; set; } = default!;
        public bool? EmailConfirmed { get; set; } = default!;
    }

    public class NotificationsPaginatedRequest : PagedRequest
    {
        public NotificationStatusEnum? Status { get; set; } = default!;
    }

    public class UserSessionsPaginatedRequest : PagedRequest
    {
        public int? UserId { get; set; } = default!;
    }

    public class PostPaginatedRequest : PagedRequest
    {
        public int? UserId { get; set; } = default!;
    }

    public class TwoFACodeRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }

    public class Disable2FARequest
    {
        public string Email { get; set; } = string.Empty;
    }
}