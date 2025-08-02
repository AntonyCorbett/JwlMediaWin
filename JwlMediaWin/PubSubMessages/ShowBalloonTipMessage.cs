namespace JwlMediaWin.PubSubMessages
{
    using Hardcodet.Wpf.TaskbarNotification;

    internal sealed class ShowBalloonTipMessage
    {
        public string Title { get; set; }

        public string Message { get; set; }

        public BalloonIcon IconType { get; set; }
    }
}
