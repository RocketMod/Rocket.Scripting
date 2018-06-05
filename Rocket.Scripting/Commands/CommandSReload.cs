using System;
using System.Drawing;
using Rocket.API.Commands;
using Rocket.API.Plugins;
using Rocket.Core.User;

namespace Rocket.Scripting.Commands
{
    public class CommandSReload : ICommand
    {
        private readonly ScriptingPlugin _scriptingPlugin;

        public CommandSReload(IPlugin plugin)
        {
            _scriptingPlugin = (ScriptingPlugin)plugin;
        }
        public bool SupportsUser(Type user)
        {
            return true;
        }

        public void Execute(ICommandContext context)
        {
            if (context.Parameters.Length > 0)
            {
                var providerName = context.Parameters.Get<string>(0);
                var provider = _scriptingPlugin.GetScriptingProvider(context.Container, providerName);

                if (provider == null)
                {
                    context.User.SendMessage("ScriptProvider not found: " + providerName);
                    return;
                }

                context.User.SendMessage("Reloading: " + provider.ScriptName, Color.Green);

                provider.UnloadPlugins();
                provider.LoadPlugins();
            }
            else
            {
                foreach (var provider in context.Container.ResolveAll<IScriptingProvider>())
                {
                    context.User.SendMessage("Reloading: " + provider.ScriptName, Color.Green);
                    provider.UnloadPlugins();
                    provider.LoadPlugins();
                }
            }

            context.User.SendMessage("Scripts have been reloaded!", Color.DarkGreen);
        }

        public string Name => "sreload";
        public string[] Aliases => null;
        public string Summary => "Reloads scripts";
        public string Description => null;
        public string Permission => null;
        public string Syntax => "[script provider]";
        public IChildCommand[] ChildCommands => null;
    }
}