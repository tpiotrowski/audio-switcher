﻿// -----------------------------------------------------------------------
// Copyright (c) David Kean. All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Linq;
using System.ComponentModel.Composition;
using System.Collections.Generic;

namespace AudioSwitcher.Presentation.CommandModel
{
    [Export(typeof(CommandManager))]
    internal class CommandManager : IDisposable
    {
        private readonly ExportFactory<ICommand, ICommandMetadata>[] _commands;
        private readonly Dictionary<string, Lifetime<ICommand>> _commandCache = new Dictionary<string, Lifetime<ICommand>>();

        [ImportingConstructor]
        public CommandManager([ImportMany]ExportFactory<ICommand, ICommandMetadata>[] commands)
        {
            _commands = commands;
        }

        public Lifetime<ICommand> FindCommand(string id)
        {
            if (id == null)
                throw new ArgumentNullException("id");

            Lifetime<ICommand> command;
            if (!_commandCache.TryGetValue(id, out command))
            {
                bool cache;
                command = CreateCommand(id, out cache);

                if (cache)
                {   
                    _commandCache.Add(id, command);
                }
            }

            return command;
        }

        public void Dispose()
        {
            // Clean up the commands that live for the lifetime of the application
            foreach (Lifetime<ICommand> command in _commandCache.Values)
            {
                command.Dispose();
            }
        }

        private Lifetime<ICommand> CreateCommand(string id, out bool cache)
        {
            ExportFactory<ICommand, ICommandMetadata> factory =  _commands.Where(c => c.Metadata.Id == id)
                                                                          .SingleOrDefault();
            cache = false;
            if (factory == null)
                return null;

            ExportLifetimeContext<ICommand> context = factory.CreateExport();
            if (factory.Metadata.IsDynamic)
            {
                // Dynamic commands are created on the fly and are not cached, we need to make 
                // sure that we clean them up when we're finished with them.
                return new Lifetime<ICommand>(() => context.Value, () => context.Dispose());
            }

            cache = true;
            return new Lifetime<ICommand>(() => context.Value, () => { });
        }
    }
}
