using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using KylieBot.Models;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Internals;
using KylieBot.Dialogs;
using System.Collections.Generic;
using System;
using Autofac;

namespace KylieBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        User u = new Models.User();
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new RootDialog(u));
            }
            else
            {
                await HandleSystemMessageAsync(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task<Activity> HandleSystemMessageAsync(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData) { }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                List<User> memberList = new List<User>();

                using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
                {
                    var client = scope.Resolve<IConnectorClient>();
                    var activityMembers = await client.Conversations.GetConversationMembersAsync(message.Conversation.Id);

                    foreach (var member in activityMembers)
                    {
                        memberList.Add(new User() { Id = member.Id, Name = member.Name });
                    }

                    if (message.MembersAdded != null && message.MembersAdded.Any(o => o.Id == message.Recipient.Id))
                    {
                        u.Id = message.From.Id;
                        u.Name = message.From.Name;
                        var intro = message.CreateReply("Hello **" + message.From.Name + "**! I am **Kylie Bot (KB)**. \n\n What can I assist you with?");
                        await connector.Conversations.ReplyToActivityAsync(intro);
                    }
                }

                if (message.MembersAdded != null && message.MembersAdded.Any() && memberList.Count > 2)
                {
                    var added = message.CreateReply(message.MembersAdded[0].Name + " joined the conversation");
                    await connector.Conversations.ReplyToActivityAsync(added);
                }

                if (message.MembersRemoved != null && message.MembersRemoved.Any())
                {
                    var removed = message.CreateReply(message.MembersRemoved[0].Name + " left the conversation");
                    await connector.Conversations.ReplyToActivityAsync(removed);
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate) { }
            else if (message.Type == ActivityTypes.Typing) { }
            else if (message.Type == ActivityTypes.Ping)
            {
                Activity reply = message.CreateReply();
                reply.Type = ActivityTypes.Ping;
                return reply;
            }
            return null;
        }
    }
}