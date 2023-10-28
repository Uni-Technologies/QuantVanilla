using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Quant.cs;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Windows.Input;

namespace QuantVanilla.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<QuantVanillaOut> outs { get; set; }
    public ObservableCollection<ImprovedNotification> notifications { get; set; }



    private string pathFormat = $"{Environment.UserName}@{Environment.CurrentDirectory} > ";
    public string BindOfWatermark
    {
        get => pathFormat;
        set => this.RaiseAndSetIfChanged(ref pathFormat, $"{Environment.UserName}@{Environment.CurrentDirectory} > ");
    }
    private string cmdLine;
    public string CmdLine { get => cmdLine;
        set => this.RaiseAndSetIfChanged(ref cmdLine, value);
    }
    public ICommand enterCommand { get; set; }
    public ICommand cleanNotifications { get; set; }
    public QuantCore core { get; set; }


    private string SetPath()
    {
        string result;
        switch (Environment.OSVersion.Platform)
        {
            case PlatformID.Win32NT:
                result = $"{Environment.GetLogicalDrives()[0]}";
                break;
            case PlatformID.Unix:
                result = Environment.UserName != "root" ? $"/home/{Environment.UserName}" : "/"; // If root, path will be '/', if not path will be '/home/{user}'
                break;
            default:
                result = "/";
                break;
        }
        Environment.CurrentDirectory = result;
        return result;
    }

    public MainWindowViewModel() 
    {
        SetPath();
        BindOfWatermark = "";

        cmdLine = "";
        outs = new ObservableCollection<QuantVanillaOut>();
        notifications = new ObservableCollection<ImprovedNotification>() 
        {
            new ImprovedNotification(new Notification("Example notification", "Description of notification", "No Data", "No sender"))
        };
        var commands = new List<IQuantCommand>() 
        {


            new QuantCommand()
            {
                Name = "echo",
                Description = "Displays the entered text",
                OnCommandExecution = new OnCommandExecutionHandler((ref QuantCore core, string args) =>
                {
                    return args;
                })
            },
            new QuantCommand()
            {
                Name = "cd",
                Description = "Directory navigation command",
                OnCommandExecution = new OnCommandExecutionHandler((ref QuantCore core, string args) =>
                {
                    if(Directory.Exists(args))
                    {
                        Environment.CurrentDirectory = args;
                    }
                    else
                    {
                        return "Exception: Directory does not exist";
                    }
                    return "";
                })
            },
            new QuantCommand()
            {
                Name = "dir",
                Description = "View directory content",
                OnCommandExecution = new OnCommandExecutionHandler((ref QuantCore core, string args) =>
                {
                    foreach(var item in Directory.GetDirectories(Environment.CurrentDirectory))
                    {
                        core.InvokeOutput($"DIR {new DirectoryInfo(item).Name}");
                    }
                    foreach(var item in Directory.GetFiles(Environment.CurrentDirectory))
                    {
                        core.InvokeOutput($"FILE {new FileInfo(item).Name}");
                    }

                    return "";
                })
            },
            new QuantCommand()
            {
                Name = "rm",
                Description = "Removes file",
                OnCommandExecution = new OnCommandExecutionHandler((ref QuantCore core, string args) =>
                {
                    if(File.Exists(args))
                    {
                        try
                        {
                            File.Delete(args);
                            return "File removed successfully";
                        }
                        catch
                        {
                            return "Exception: Unknown";
                        }
                    }
                    else 
                    {
                        return "Exception: File does not exist";
                    }
                    
                })
            },
            new QuantCommand()
            {
                Name = "clear",
                Description = "Clear log",
                OnCommandExecution = new OnCommandExecutionHandler((ref QuantCore core, string args) =>
                {
                    outs.Clear();
                    return "";
                })
            },
            new QuantCommand()
            {
                Name = "date",
                Description = "Shows current date",
                OnCommandExecution = new OnCommandExecutionHandler((ref QuantCore core, string args) =>
                {
                    string minutes = $"{DateTime.Now.Minute}";
                    if(minutes.Length == 1) {minutes = $"0{minutes}"; }
                    return $"{DateTime.Now.Day} {DateTimeFormatInfo.CurrentInfo.MonthNames[DateTime.Now.Month-1]}, {DateTime.Now.Hour}:{minutes}";
                })
            },
            new QuantCommand()
            {
                Name = "getd",
                Description = "Gets all logical drives/mounted drives",
                OnCommandExecution = new OnCommandExecutionHandler((ref QuantCore core, string args) =>
                {
                    for(int i = 0; i < Environment.GetLogicalDrives().Length; i++)
                    {
                        core.InvokeOutput($"[{i}] {Environment.GetLogicalDrives()[i]}");
                    }
                    return "";
                })
            },
            new QuantCommand()
            {
                Name = "touch",
                Description = "Creates file",
                OnCommandExecution = new OnCommandExecutionHandler((ref QuantCore core, string args) =>
                {
                    try 
                    {
                        File.Create(args).Dispose();
                        return "Successful";
                    }
                    catch
                    {
                        return "Exception: Unknown";
                    }
                })
            },
            new QuantCommand()
            {
                Name = "read",
                Description = "Reads file's content",
                OnCommandExecution = new OnCommandExecutionHandler((ref QuantCore core, string args) =>
                {
                    if(File.Exists(args))
                    {
                        return File.ReadAllText("args");
                    }
                    else
                    {
                        return "Exception: File does not exist";
                    }
                })
            },
            new QuantCommand()
            {
                Name = "fget",
                Description = "Downloads file from uri",
                OnCommandExecution = new OnCommandExecutionHandler((ref QuantCore icore, string args) =>
                {
                    new Thread(DownloadProcess).Start();
                    void DownloadProcess()
                    {
                        try
                        {
                            new WebClient().DownloadFile(args.Split()[0], args.Split()[1]);
                            Dispatcher.UIThread.Invoke(() =>
                            core.GetNotification(new Notification("Downloaded", $"File {args.Split()[1]} downloaded", null, "FGet")));
                        }
                        catch
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            core.GetNotification(new Notification("Error", $"Error while downloading file {args.Split()[1]} from {args.Split()[0]}", null, "FGet")));
                        }
                    }
                    return "";
                })
            },
            new QuantCommand()
            {
                Name = "write",
                Description = "Writes content to file",
                OnCommandExecution = new OnCommandExecutionHandler((ref QuantCore core, string args) =>
                {
                    try
                    {
                        File.WriteAllText(args.Split()[0], args.Replace($"{args.Split()[0]} ", ""));
                        return "Successfully";
                    }
                    catch
                    {
                        return "Exception: Unknown";
                    }
                })
            },
            new QuantCommand()
            {
                Name = "mkdir",
                Description = "Creates directory",
                OnCommandExecution = new OnCommandExecutionHandler((ref QuantCore core, string args) =>
                {
                    try
                    {
                        Directory.CreateDirectory(args);
                        return "Successfully";
                    }
                    catch
                    {
                        return "Exception: Unknown";
                    }
                })
            },
            new QuantCommand()
            {
                Name = "search",
                Description = "Search file by name",
                OnCommandExecution = new OnCommandExecutionHandler((ref QuantCore core, string args) =>
                {
                    core.InvokeOutput("Results:");
                    foreach(var item in Directory.GetFiles(Environment.CurrentDirectory))
                    {
                        if(new FileInfo(item).Name.ToLowerInvariant().Contains(args.ToLowerInvariant()))
                        {
                            core.InvokeOutput(new FileInfo(item).Name);
                        }
                    } 
                    return "";
                })
            },
            new QuantCommand()
            {
                Name = "ping",
                Description = "Ping IP address",
                OnCommandExecution = new OnCommandExecutionHandler((ref QuantCore icore, string args) =>
                {
                    if(args.Split().Length <= 1)
                    {
                        icore.InvokeOutput("Not enough arguments");
                        return "";
                    }
                    new Thread(Ping).Start();
                    void Ping() {
                        
                        string host = args.Split()[0];
                        int count = 4;
                        if (int.TryParse(args.Split()[1], out int parsedCount))
                        {
                            count = parsedCount;
                        }
                        Ping pingSender = new Ping();
                        PingOptions options = new PingOptions()
                        {
                            DontFragment = true
                        };
                        for (int i = 0; i < count; i++)
                        {
                            PingReply reply = pingSender.Send(host);

                            if (reply.Status == IPStatus.Success)
                            {
                                Dispatcher.UIThread.Invoke(() => {
                                core.InvokeOutput($"Reply from {reply.Address}: bytes={reply.Buffer.Length} time={reply.RoundtripTime}ms"); });
                            }
                            else
                            {
                                Dispatcher.UIThread.Invoke(() => {
                                core.InvokeOutput($"Status: {reply.Status}"); });
                            }
                        };
                    }
                    return "";
                })
            },
            new QuantCommand()
            {
                Name = "execute",
                Description = "Executes a file",
                OnCommandExecution = new OnCommandExecutionHandler((ref QuantCore core, string args) =>
                {
                    if(File.Exists(args.Split()[0]))
                    {
                        try 
                        {
                            Process.Start(args.Split()[0], args.Replace($"{args.Split()[0]} ", ""));
                            return "Successfully"; 
                        }
                        catch
                        {
                            return "Exception: Unknown";
                        }
                    }
                    else
                    {
                        return "Exception: File does not exist";
                    }
                    
                })
            },
            new QuantCommand()
            {
                Name = "mkzip",
                Description = "Makes zip archive",
                OnCommandExecution = new OnCommandExecutionHandler((ref QuantCore core, string args) =>
                {
                    try
                    {
                        if (Directory.Exists(args.Split()[0]))
                        {
                            ZipFile.CreateFromDirectory(args.Split()[0], args.Split()[1], CompressionLevel.SmallestSize, true);
                            core.InvokeOutput("Successfully created zip");
                        }
                        else
                        {
                            core.InvokeOutput("Exception: Directory does not exist");
                        }
                    }
                    catch
                    {
                        core.InvokeOutput("Exception: Unknown");
                    }
                    return "";
                })
            },
            new QuantCommand()
            {
                Name = "unzip",
                Description = "Unzips the file",
                OnCommandExecution = new OnCommandExecutionHandler((ref QuantCore core, string args) =>
                {
                    try
                    {
                        if (File.Exists(args.Split()[0]))
                        {
                            if (!Directory.Exists(args.Split()[1]))
                            {
                                Directory.CreateDirectory(args.Split()[1]);
                            }
                            ZipFile.ExtractToDirectory(args.Split()[0], $"{args.Split()[1]}", true);
                            core.InvokeOutput($"Successfully unzipped");
                        }
                        else
                        {
                            core.InvokeOutput("Exception: File does not exist");
                        }

                    }
                    catch
                    {
                        core.InvokeOutput("Exception: Unknown");
                    }
                    return "";
                })
            },
            new QuantCommand()
            {
                Name = "help",
                Description = "Help command",
                OnCommandExecution = new OnCommandExecutionHandler((ref QuantCore core, string args) =>
                {
                    var result = new List<string>()
                    {
                        "Commands usages:",
                        "help",
                        "cd (directory)",
                        "dir",
                        "clear",
                        "ping (domain) [iterations]",
                        "fget (link to file)",
                        "echo (text)",
                        "getd",
                        "mkzip (source directory name) (output zip file name)",
                        "unzip (file name)",
                        "rm (file name)",
                        "read (file name)",
                        "write (file name)",
                        "mkdir (directory name)",
                        "touch (file name)",
                        "date",
                        "execute (file name)",
                    };
                    foreach(var item in result)
                    {
                        core.InvokeOutput(item);
                    }
                    return "";
                })
            }


        };


        core = new QuantCore(commands, "Quant Vanilla");
        core.OnOutputEvent = new OnOutputEventHandler((string output) =>
        {
            if (output != "")
            {
                if (Environment.GetEnvironmentVariable("Connect_Host") != null)
                {
                    outs.Add(new QuantVanillaOut(output, WhereIGetOut.Connection));
                }
                else
                {
                    outs.Add(new QuantVanillaOut(output, WhereIGetOut.Local));
                }
            }
        });
        core.OnNotificationEvent = new OnNotificationEventHandler((ref QuantCore core, ref Notification notification) =>
        {
            if(notification.Sender == null)
            {
                notification.Sender = "Unknown sender";
            }
            notifications.Add(new ImprovedNotification(notification));
        });

        enterCommand = ReactiveCommand.Create(() =>
        {
            try
            {
                var host = Environment.GetEnvironmentVariable("Connect_Host");
                if (host == null)
                {
                    core.ExecuteCommand(CmdLine);
                    BindOfWatermark = $"{Environment.UserName}@{Environment.CurrentDirectory} > ";
                }
                else
                {
                    try
                    {
                        core.SendRequest(CmdLine, host);
                    }
                    catch
                    {
                        Environment.SetEnvironmentVariable("Connect_Host", null);
                        core.InvokeOutput("Exception: Host not found");
                    }
                }
                CmdLine = "";
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
            }
        });
        cleanNotifications = ReactiveCommand.Create(() =>
        {
            notifications.Clear();
        });
        outs.Add(new QuantVanillaOut("Quant Vanilla started", WhereIGetOut.Local));
        

        
    }
}

public class QuantVanillaOut
{
    public string Content { get; set; }
    public Bitmap? Bitmap { get; set; } = null;

    public QuantVanillaOut(string content, WhereIGetOut whereIGet)
    {
        Content = content;
        switch(whereIGet)
        {
            case WhereIGetOut.Connection:
                Bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://QuantVanilla/Assets/icon_connect.png")));
                break;
            case WhereIGetOut.Local:
                if(Content.Contains("Exception") || Content.Contains("Information:"))
                {
                    Bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://QuantVanilla/Assets/warning.png")));
                }
                else if(Content.Contains("List with commands changed to previous one"))
                {
                    Bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://QuantVanilla/Assets/previous.png")));
                }
                else
                {
                    Bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://QuantVanilla/Assets/ok.png")));
                }
                break;
        }
    }
}
public enum WhereIGetOut
{
    Connection,
    Local
}

public class ImprovedNotification
{
    public Notification notification { get; set; }
    public bool isChecked { get; set; } = false;
    public ImprovedNotification(Notification notification)
    {
        this.notification = notification;
    }
}