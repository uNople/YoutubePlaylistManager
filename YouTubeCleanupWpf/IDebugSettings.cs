namespace YouTubeCleanupWpf;

public interface IDebugSettings
{
    public delegate void ChangedEventHandler(bool showIds); 
    public event ChangedEventHandler ShowIdsChanged;
    public bool ShowIds { get; set; }
}