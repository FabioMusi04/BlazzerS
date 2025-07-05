namespace Models.http
{
    public class Response
    {
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
    }

    public class PagedResponse<T> : Response
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public List<T> Items { get; set; } = [];
    }

    public class RegisterResponse : Response
    {
        public int UserId { get; set; }
    }

    public class LoginResponse : Response
    {
        public string? Token { get; set; } = default!;
        public User User { get; set; } = default!;
    }

    public class UserResponse : Response
    {
        public User User { get; set; } = default!;
    }

    public class UploadFileResponse : Response
    {
        public UploadFile File { get; set; } = default!;
    }

    public class UploadFilesResponse : Response
    {
        public List<UploadFile> Files { get; set; } = [];
    }

    public class NotificationResponse : Response
    {
        public Notification Notification { get; set; } = default!;
    }
    public class VerificationTokenResponse : Response
    {
        public bool IsValid { get; set; } = false;
    }

    public class NotificationConnectResponse : Response
    {

    }

    public class VerificationTokenRetryReponse : Response
    {
    }

    public class UsersPaginatedResponse : PagedResponse<User>
    {
    }

    public class NotificationsPaginatedResponse : PagedResponse<Notification>
    {
    }
}
