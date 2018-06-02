using System;
using System.Collections.Generic;
using System.Linq;
using Rocket.API.Commands;
using Rocket.API.DependencyInjection;
using Rocket.API.Permissions;
using Rocket.API.User;
using Rocket.Core.Commands;
using Rocket.Core.User;

namespace Rocket.Scripting.Commands
{
    public class CommandEval : ICommand
    {
        public static Dictionary<string, IScriptContext> ScriptContexts { get; } = new Dictionary<string, IScriptContext>();

        public static IScriptContext StartSession(IDependencyContainer container, IUser user, string providerName)
        {
            var context = GetScriptContext(user);
            if (context != null)
                return context;

            var providers = container.ResolveAll<IScriptingProvider>();
            foreach (var provider in providers)
            {
                if (provider.ScriptName.Equals(providerName, StringComparison.OrdinalIgnoreCase)
                    || provider.FileTypes.Any(c => c.Equals(providerName, StringComparison.OrdinalIgnoreCase)))
                {
                    context = provider.CreateScriptContext(container);
                    ScriptContexts.Add(user.Id, context);
                    return context;
                }
            }

            return null;
        }

        public static IScriptContext GetScriptContext(IUser user)
        {
            foreach (var scriptContext in ScriptContexts)
            {
                if (scriptContext.Key.Equals(user.Id, StringComparison.OrdinalIgnoreCase))
                    return scriptContext.Value;
            }

            return null;
        }

        public static bool StopSession(IUser user)
        {
            return ScriptContexts.Keys
                .ToList()
                .Where(c => c.Equals(user.Id, StringComparison.OrdinalIgnoreCase))
                .All(c => ScriptContexts.Remove(c));
        }
        public bool SupportsUser(Type commandUser)
        {
            return true;
        }

        public void Execute(ICommandContext commandContext)
        {
            if (commandContext.Parameters.Length == 0)
                throw new CommandWrongUsageException();

            var scriptContext = GetScriptContext(commandContext.User);
            if (scriptContext == null)
            {
                commandContext.User.SendMessage("Session was not started yet. Use " + commandContext.CommandPrefix + commandContext.CommandAlias + " start <type> to start a new session.", ConsoleColor.Red);
                return;
            }

            var result = scriptContext.Eval(commandContext.Parameters.GetArgumentLine(0));
            if (result.HasReturn)
                commandContext.User.SendMessage("> " + result.Return, ConsoleColor.Gray);
        }

        public string Name => "Eval";
        public string[] Aliases => new[] { "e" };
        public string Summary => "Evaluates an expression.";
        public string Description => null;
        public string Permission => "Rocket.Scripting";
        public string Syntax => "<expression>";
        public IChildCommand[] ChildCommands => new IChildCommand[] { new CommandEvalStart(), new CommandEvalExit() };
    }

    public class CommandEvalStart : IChildCommand
    {
        public bool SupportsUser(Type commandUser)
        {
            return true;
        }

        public void Execute(ICommandContext commandContext)
        {
            if(commandContext.Parameters.Length != 1)
                throw new CommandWrongUsageException();

            var scriptType = commandContext.Parameters[0];
            var scriptingContext = CommandEval.StartSession(commandContext.Container, commandContext.User, scriptType);
            if (scriptingContext == null)
            {
                commandContext.User.SendMessage("Script provider not found: " + scriptType, ConsoleColor.Red);
                var providers= commandContext.Container.ResolveAll<IScriptingProvider>();
                var providerNames = string.Join(", ", providers.Select(c => c.ScriptName).ToArray());
                commandContext.User.SendMessage("Available script providers: " + providerNames);
                return;
            }

            commandContext.User.SendMessage("Scripting session has started.", ConsoleColor.Green);
        }

        public string Name => "Start";
        public string[] Aliases => null;
        public string Summary => "Starts a scripting session.";
        public string Description => null;
        public string Permission => "Rocket.Scripting";
        public string Syntax => "<script provider>";
        public IChildCommand[] ChildCommands => null;
    }

    public class CommandEvalExit : IChildCommand
    {
        public bool SupportsUser(Type commandUser)
        {
            return true;
        }

        public void Execute(ICommandContext commandContext)
        {
            if (commandContext.Parameters.Length != 0)
                throw new CommandWrongUsageException();

            bool success = CommandEval.StopSession(commandContext.User);
            if (!success)
            {
                commandContext.User.SendMessage("Session is not running.", ConsoleColor.Red);
                return;
            }

            commandContext.User.SendMessage("Session was stopped.", ConsoleColor.Green);
        }

        public string Name => "Exit";
        public string[] Aliases => null;
        public string Summary => "Stops a scripting session and exits.";
        public string Description => null;
        public string Permission => "Rocket.Scripting";
        public string Syntax => "";
        public IChildCommand[] ChildCommands => null;
    }
}