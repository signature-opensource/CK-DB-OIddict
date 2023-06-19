using System;
using System.Threading.Tasks;
using CK.Core;
using CK.Cris;
using Microsoft.Extensions.DependencyInjection;

namespace CK.DB.OIddict.DefaultServer.App
{
    public class CommandAdapter<TCommand, TResult> : IAutoService
    where TCommand : ICommand<TResult> where TResult : class, IPoco
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly PocoDirectory _pocoDirectory;
        private readonly CommandExecutor _commandExecutor;

        public CommandAdapter
        (
            IServiceScopeFactory serviceScopeFactory,
            PocoDirectory pocoDirectory,
            CommandExecutor commandExecutor
        )
        {
            _serviceScopeFactory = serviceScopeFactory;
            _pocoDirectory = pocoDirectory;
            _commandExecutor = commandExecutor;
        }

        public async Task<TResult?> HandleAsync
        (
            ActivityMonitor activityMonitor,
            Action<TCommand>? input = null
        )
        {
            input ??= c => { };

            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var services = scope.ServiceProvider;
            var command = _pocoDirectory.Create<TCommand>( input );

            var result = await _commandExecutor.ExecuteCommandAsync
            (
                activityMonitor,
                services,
                command
            );

            var tResult = result.Result as TResult;

            return tResult;
        }
    }
}
