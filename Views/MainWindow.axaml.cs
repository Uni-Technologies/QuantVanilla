using Avalonia.Controls;
using QuantVanilla.ViewModels;
using System;

namespace QuantVanilla.Views;

public partial class MainWindow : Window
{
    public Window connect = new ConnectionWindow()
    {
        DataContext = new ConnectionWindowViewModel()
    };
    public Window cmdList = new CommandsListWindow()
    {
        DataContext = new CommandsListWindowViewModel(),

    };


    public MainWindow()
    {
        InitializeComponent();
    }

    private void InfoClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var getInfo = new Flyout()
        {
            Content = new StackPanel()
            {
                Children =
                {
                    new TextBlock()
                    {
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        Text = $"Operation System:"
                    },
                    new TextBlock()
                    {
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        Text = $"{Environment.OSVersion}"
                    },
                    new TextBlock()
                    {
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        Text = $"User: {Environment.UserName}"
                    },
                    new TextBlock()
                    {
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        Text = $"Processor Count: {Environment.ProcessorCount}"
                    },
                    new Separator(),
                    new TextBlock()
                    {
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        Text = $"Quant Vanilla version: 1.0"
                    },
                    new TextBlock()
                    {
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        Text = $"Quant.cs version: 1.1"
                    },
                    new TextBlock()
                    {
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        Text = $"Avalonia version: 11.0.2"
                    },
                    new TextBlock()
                    {
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                        Text = $".NET {Environment.Version}"
                    }
                }
            }
        };

        getInfo.ShowAt(InfoButton);
    }

    private void ConnectButton(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if(connect.ShowActivated)
        {
            connect.Show(this);
            connect.Closed += Connect_Closed;
        }
    }

    private void Connect_Closed(object? sender, EventArgs e)
    {
        connect = new ConnectionWindow()
        {
            DataContext = new ConnectionWindowViewModel()
        };
    }

    private void CmdList_Button(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (cmdList.ShowActivated)
        {
            cmdList.DataContext = new CommandsListWindowViewModel((DataContext as MainWindowViewModel).core.Commands);
            cmdList.Show(this);
            cmdList.Closed += CmdList_Closed;
        }
    }

    private void CmdList_Closed(object? sender, EventArgs e)
    {
        cmdList = new CommandsListWindow()
        {
            DataContext = new CommandsListWindowViewModel((DataContext as MainWindowViewModel).core.Commands)
        };
    }



    private void OnEnter(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Enter)
        {
            (DataContext as MainWindowViewModel).enterCommand.Execute(null);
        }
    }
}