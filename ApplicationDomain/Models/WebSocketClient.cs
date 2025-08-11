using System.ComponentModel;

public class WebSocketClient : INotifyPropertyChanged
{
    private DateTime _lastActivity;
    
    public string ClientId { get; set; }
    public DateTime ConnectedTime { get; set; }
    
    public DateTime LastActivity 
    { 
        get => _lastActivity;
        set
        {
            _lastActivity = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    
    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}