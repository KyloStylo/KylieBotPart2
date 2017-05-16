using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using KylieBot.Models;
using AuthBot;
using AuthBot.Models;
using AuthBot.Dialogs;
using System.Threading;

namespace KylieBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private User user;

        public RootDialog(User user)
        {
            this.user = user;
        }

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            if (string.IsNullOrEmpty(await context.GetAccessToken(AuthSettings.Scopes)))
            {
                await context.Forward(new AzureAuthDialog(AuthSettings.Scopes), this.ResumeAfterAuth, activity, CancellationToken.None);
            }
            else
            {
                context.Wait(MessageReceivedAsync);
            }

            if (!string.IsNullOrEmpty(user.Token) && activity.Text == "logout")
            {
                await context.Logout();
            }
        }

        private async Task ResumeAfterAuth(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;
            user.Token = await context.GetAccessToken(AuthSettings.Scopes);
            await context.PostAsync(message);
            context.Wait(MessageReceivedAsync);
        }
    }
}