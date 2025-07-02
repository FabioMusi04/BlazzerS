namespace Models.enums
{
    public enum UserRoleEnum
    {
        User,
        Admin
    }

    public enum ReportStatusEnum
    {
        Pending,
        InProgress,
        Resolved,
        Rejected
    }

    public enum NotificationStatusEnum
    {
        Unread,
        Read
    }

    public enum NotificationChannelEnum
    {
        App,
        Email,
        Push
    }

    public enum AlertTypeEnum
    {
        Information,
        Warning,
        Danger,
        Normal
    }

    public enum AlertStylePositionEnum
    {
        BottomRight,
        Center,
    }

    public enum FormatEnum
    {
        Png,
        Jpeg,
        Gif,
        Webp,
        Pdf,
        Unknown
    }
}

