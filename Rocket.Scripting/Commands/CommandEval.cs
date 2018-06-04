using System;
using System.Linq;
using Rocket.API.Commands;
using Rocket.API.Plugins;
using Rocket.Core.Commands;
using Rocket.Core.User;

namespace Rocket.Scripting.Commands
{
    public class CommandEval : ICommand
    {
        private readonly ScriptingPlugin _scriptPlugin;

        public CommandEval(IPlugin plugin)
        {
            _scriptPlugin = (ScriptingPlugin) plugin;
        }

        public bool SupportsUser(Type commandUser)
        {
            return true;
        }

        public void Execute(ICommandContext commandContext)
        {
            if (commandContext.Parameters.Length == 0)
                throw new CommandWrongUsageException();

            var scriptContext = _scriptPlugin.GetScriptContext(commandContext.User);
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
        public IChildCommand[] ChildCommands => new IChildCommand[] { new CommandEvalStart(_scriptPlugin), new CommandEvalExit(_scriptPlugin) };
    }

    public class CommandEvalStart : IChildCommand
    {
        private readonly ScriptingPlugin _scriptPlugin;

        public CommandEvalStart(IPlugin plugin)
        {
            _scriptPlugin = (ScriptingPlugin)plugin;
        }

        public bool SupportsUser(Type commandUser)
        {
            return true;
        }

        public void Execute(ICommandContext commandContext)
        {
            if(commandContext.Parameters.Length != 1)
                throw new CommandWrongUsageException();

            var scriptType = commandContext.Parameters[0];
            var scriptingContext = _scriptPlugin.StartSession(commandContext.Container, commandContext.User, scriptType);
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
        private readonly ScriptingPlugin _scriptPlugin;

        public CommandEvalExit(IPlugin plugin)
        {
            _scriptPlugin = (ScriptingPlugin)plugin;
        }

        public bool SupportsUser(Type commandUser)
        {
            return true;
        }

        public void Execute(ICommandContext commandContext)
        {
            if (commandContext.Parameters.Length != 0)
                throw new CommandWrongUsageException();

            bool success = _scriptPlugin.StopSession(commandContext.User);
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