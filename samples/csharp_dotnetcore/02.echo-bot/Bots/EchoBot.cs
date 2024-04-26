// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Graph;
using Microsoft.Graph.Communications.Common;
using Microsoft.Graph.Models;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class EchoBot : ActivityHandler
    {
        private readonly IHttpClientFactory _clientFactory;

        public EchoBot(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var message = turnContext.Activity;

            if (message.Attachments != null && message.Attachments.Any())
            {
                var attachment = message.Attachments.First();

                // Baixar o arquivo do anexo (implementar a lógica conforme necessário)
                var fileContent = await DownloadFile(attachment.ContentUrl);

                // Obter o token de acesso
                var accessToken = await GraphHelper.GetAccessToken();

                // IDs do site e lista
                string siteId = "your-site-id";
                string listId = "your-list-id";
                string itemId = "your-item-id";  // Suponha que já existe

                // Upload do arquivo
                await GraphHelper.UploadFileToSharePoint(accessToken, siteId, listId, itemId, attachment.Name, fileContent);

                await turnContext.SendActivityAsync(MessageFactory.Text("Arquivo recebido e salvo no SharePoint!"), cancellationToken);
            }
            else
            {
                // Resposta padrão do EchoBot
                await turnContext.SendActivityAsync(MessageFactory.Text($"Você disse: {message.Text}"), cancellationToken);
            }
        }


        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}
