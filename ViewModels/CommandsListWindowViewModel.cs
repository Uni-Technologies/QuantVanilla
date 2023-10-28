using Quant.cs;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace QuantVanilla.ViewModels;

public class CommandsListWindowViewModel : ViewModelBase
{
    public ObservableCollection<IQuantCommand> Commands { get; set; }
    public CommandsListWindowViewModel()
    {
        Commands = new ObservableCollection<IQuantCommand>();
    }
    public CommandsListWindowViewModel(List<IQuantCommand> commands)
    {
        Commands = new ObservableCollection<IQuantCommand>();
        foreach(var command in commands)
        {
            Commands.Add(command);
        }
    }
}