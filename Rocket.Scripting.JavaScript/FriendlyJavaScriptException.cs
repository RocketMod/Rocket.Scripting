using System;
using Jint.Runtime;
using Rocket.API.Commands;
using Rocket.Core.User;

namespace Rocket.Scripting.JavaScript
{
    public class FriendlyJavaScriptException : Exception, ICommandFriendlyException
    {
        public FriendlyJavaScriptException(JavaScriptException ex) : base(ex.Message)
        {

        }

        public void SendErrorMessage(ICommandContext context)
        {
            context.User.SendMessage("> " + Message, ConsoleColor.Red);
        }
    }
}