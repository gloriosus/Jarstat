namespace Jarstat.Client.Services;

public class NotifyStateService
{
    public event EventHandler? EventClick;

    public void NotifyEventClick(object sender)
    {
        if (EventClick != null)
            EventClick(sender, EventArgs.Empty);
    }
}
