﻿using System.Threading.Tasks;

namespace Calamari.Commands.Support
{
    /// <summary>
    /// A command that requires the command line arguments passed to it. We are transitioning away from this interface
    /// </summary>
    public interface ICommand
    {
        int Execute();
    }

    public interface ICommandAsync
    {
        Task Execute();
    }
}