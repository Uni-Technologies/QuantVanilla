using ReactiveUI;
using System;
using System.Net.Http;
using System.Windows.Input;
using static System.Net.WebRequestMethods;

namespace QuantVanilla.ViewModels;

public class ConnectionWindowViewModel : ViewModelBase
{
    public string? host = Environment.GetEnvironmentVariable("Connect_Host");
    public string? Host
    {
        get => host;
        set => this.RaiseAndSetIfChanged(ref host, value);
    }
    public ICommand ConnectCommand { get; set; }
    public ICommand DisconnectCommand { get; set; }
    private string connectedText = "";
    public string ConnectedText 
    { 
        get => connectedText; 
        set => this.RaiseAndSetIfChanged(ref connectedText, value); 
    }

    public ConnectionWindowViewModel()
    {
        ConnectCommand = ReactiveCommand.Create(() =>
        {
            try
            {
                new HttpClient().Send(new HttpRequestMessage()
                {
                    Content = null,
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(Host)
                });
                ConnectedText = $"OK!";
                Environment.SetEnvironmentVariable("Connect_Host", Host);
            }
            catch
            {
                ConnectedText = $"Not OK!";
                Host = "";
                Environment.SetEnvironmentVariable("Connect_Host", null);
            }
            
        });
        DisconnectCommand = ReactiveCommand.Create(() =>
        {
            Host = null;
            Environment.SetEnvironmentVariable("Connect_Host", Host);
        });
    }
}
