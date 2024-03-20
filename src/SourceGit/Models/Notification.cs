using System;

namespace SourceGit.Models
{
    public class Notification
    {
        public bool IsError { get; set; } = false;
        public string Message { get; set; } = string.Empty;

        public string Action { get; set; }
        public Action<string> Callback { get; set; }

        public event EventHandler Dismiss;
        public event EventHandler<string> Activate;

        public virtual void OnDismiss()
        {
            Dismiss?.Invoke(this, EventArgs.Empty);
            Callback?.Invoke(string.Empty);
        }

        public void OnActivate(string action)
        {
            Activate?.Invoke(this, action);
            Callback?.Invoke(action);
        }
    }

    public interface INotificationReceiver
    {
        void OnReceiveNotification(string ctx, Notification notice);
    }
}