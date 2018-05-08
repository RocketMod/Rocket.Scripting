using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Rocket.API.DependencyInjection;
using Rocket.Core.Plugins;

namespace Rocket.Scripting
{
    class ScriptingPlugin : Plugin
    {
        public ScriptingPlugin(IDependencyContainer container) : base("ScriptingPlugin", container)
        {
        }
    }
}