﻿using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grit.CQRS
{
    public class CommandHandlerFactory : ICommandHandlerFactory
    {
        private static IKernel _kernel;
        private static IEnumerable<string> _commandAssmblies;
        private static IEnumerable<string> _handlerAssmblies;
        private static IDictionary<Type, Type> _handlers;
        private static bool _isInitialized;
        private static readonly object _lockThis = new object();

        public static void Init(IKernel kernel, 
            IEnumerable<string> commandAssmblies,
            IEnumerable<string> handlerAssmblies)
        {
            if (!_isInitialized)
            {
                lock (_lockThis)
                {
                    _commandAssmblies = commandAssmblies;
                    _handlerAssmblies = handlerAssmblies;
                    _kernel = kernel;
                    HookHandlers();
                    _isInitialized = true;
                }
            }
        }

        private static void HookHandlers()
        {
            _handlers = new Dictionary<Type, Type>();
            List<Type> commands = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies.Where(n => _commandAssmblies.Any(m => m == n.GetName().Name)))
            {
                commands.AddRange(assembly.GetExportedTypes().Where(x => x.IsSubclassOf(typeof(Command))));
            }

            foreach (var assembly in assemblies.Where(n => _handlerAssmblies.Any(m => m == n.GetName().Name)))
            {
                var allHandlers = assembly.GetExportedTypes().Where(x => x.GetInterfaces()
                        .Any(a => a.IsGenericType && a.GetGenericTypeDefinition() == typeof(ICommandHandler<>)));
                foreach(var command in commands)
                {
                    var handlers = allHandlers
                        .Where(h => h.GetInterfaces()
                            .Any(ii => ii.GetGenericArguments()
                                .Any(aa => aa == command))).ToList();
                    if (handlers.Count > 1 ||
                        (handlers.Count == 1 && _handlers.ContainsKey(command)))
                    {
                        throw new MoreThanOneDomainCommandHandlerException("more than one handler for command: " + command.Name);
                    }
                    _handlers[command] = handlers.First();
                }
            }
            foreach (var command in commands)
            {
                if(!_handlers.ContainsKey(command))
                {
                    throw new UnregisteredDomainCommandException("no handler registered for command: " + command.Name);
                }
            }
        }

        public ICommandHandler<T> GetHandler<T>() where T : Command
        {
            var handler = _handlers[typeof(T)];
            return (ICommandHandler<T>)_kernel.GetService(handler);
        }
    }
}