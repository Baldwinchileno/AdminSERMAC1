using System;

namespace AdminSERMAC.Models
{
    public class NotificationEventArgs : EventArgs
    {
        public NotificationType Type { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }

    public enum NotificationType
    {
        StockBajo,
        PagoPendiente
    }
}