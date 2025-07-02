using Microsoft.AspNetCore.Components;
using Models.enums;

namespace Models.front
{
    public class AlertModel
    {
        public string Title { get; set; } = "Alert";
        public string Message { get; set; } = "Something happened.";
        public AlertStylePositionEnum Position { get; set; } = AlertStylePositionEnum.Center;
        public AlertTypeEnum Style { get; set; } = AlertTypeEnum.Normal;
        public bool IsVisible { get; set; } = false;
        public bool HasActions { get; set; } = false;
        public string OkText { get; set; } = "OK";
        public string CancelText { get; set; } = "Cancel";
        public EventCallback OnOk { get; set; }
        public EventCallback OnCancel { get; set; }
    }
}
