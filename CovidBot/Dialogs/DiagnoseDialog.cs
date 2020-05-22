// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.6.2

using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CovidBot.CognitiveModels;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace CovidBot.Dialogs
{
    public class DiagnoseDialog : CancelAndHelpDialog
    {
        private const string DestinationStepMsgText = "Where would you like to travel to?";
        private const string OriginStepMsgText = "Where are you traveling from?";
        private static string attachmentPromptId = $"{nameof(DiagnoseDialog)}_attachmentPrompt";
        private readonly CovidRecognizer _luisRecognizer;

        public DiagnoseDialog(CovidRecognizer luisRecognizer,UserDetailsDialog userDetailsDialog)
            : base(nameof(DiagnoseDialog))
        {
            _luisRecognizer = luisRecognizer;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new AttachmentPrompt(attachmentPromptId));
            AddDialog(userDetailsDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                AskSymptomsStepAsync,
                GetSymptomsStepAsync,
                XRayStepAsync,
                ConfirmStepAsync,
                FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> AskSymptomsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //var bookingDetails = (BookingDetails)stepContext.Options;

            //if (bookingDetails.Destination == null)
            //{
            //    var promptMessage = MessageFactory.Text(DestinationStepMsgText, DestinationStepMsgText, InputHints.ExpectingInput);
            //    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            //}

            //  return await stepContext.NextAsync(bookingDetails.Destination, cancellationToken);

            var messageText = "Please mention some of the symptoms you encountered?";
            var messageTextE = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
            // await stepContext.Context.SendActivityAsync(messageTextE, cancellationToken);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = messageTextE }, cancellationToken);
            // return await stepContext.NextAsync(null,cancellationToken);

        }

        private async Task<DialogTurnResult> GetSymptomsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //var bookingDetails = (BookingDetails)stepContext.Options;

            //bookingDetails.Destination = (string)stepContext.Result;

            //if (bookingDetails.Origin == null)
            //{
            //    var promptMessage = MessageFactory.Text(OriginStepMsgText, OriginStepMsgText, InputHints.ExpectingInput);
            //    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            //}
            var luisResult = await _luisRecognizer.RecognizeAsync<CovidHealthBot>(stepContext.Context, cancellationToken);
            switch (luisResult.TopIntent().intent)
            {
                case CovidHealthBot.Intent.Symptoms_At_Risk:
                     if(luisResult.Entities.symptoms.Length > 0)
                    {
                        var messageText = "Sorry you might be at risk, do you want to continue with furthur investigation?";
                        var messageTextE = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                        // await stepContext.Context.SendActivityAsync(messageTextE, cancellationToken);
                        return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = messageTextE }, cancellationToken);
                    }
                    break;

                default:
                    return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            return await stepContext.NextAsync(cancellationToken);
        }

        private async Task<DialogTurnResult> XRayStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                var messageText = "Please attach your xray as attachment";
                var messageTextE = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                return await stepContext.PromptAsync(attachmentPromptId, new PromptOptions { Prompt = messageTextE }, cancellationToken);
            }
            else
            {
                var messageText = "Be careful Maintain social distance";
                var messageTextE = MessageFactory.Text(messageText, messageText, InputHints.IgnoringInput);
                var c = await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = messageTextE }, cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

                //var bookingDetails = (BookingDetails)stepContext.Options;

                //bookingDetails.Origin = (string)stepContext.Result;

                //if (bookingDetails.TravelDate == null || IsAmbiguous(bookingDetails.TravelDate))
                //{
                //    return await stepContext.BeginDialogAsync(nameof(DateResolverDialog), bookingDetails.TravelDate, cancellationToken);
                //}

                //return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            List<Attachment> attachments = (List<Attachment>)stepContext.Result;
            string replyText = string.Empty;
            foreach (var file in attachments)
            {
                // Determine where the file is hosted.
                var remoteFileUrl = file.ContentUrl;

                // Save the attachment to the system temp directory.
                var localFileName = Path.Combine(Path.GetTempPath(), file.Name);

                // Download the actual attachment
                using (var webClient = new WebClient())
                {
                    webClient.DownloadFile(remoteFileUrl, localFileName);
                }

                replyText += $"Attachment \"{file.Name}\"" +
                             $" has been received and saved to \"{localFileName}\"\r\n";
            }
            // pingu api call here
            if (true)
            {
                return await stepContext.BeginDialogAsync(nameof(UserDetailsDialog), null, cancellationToken);
            }
            else
            {
                var ac = stepContext.Context.Activity;
                var messageText = "Be careful Maintain social distance";
                var messageTextE = MessageFactory.Text(replyText, replyText, InputHints.IgnoringInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = messageTextE }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                var bookingDetails = (UserDetailsDialog)stepContext.Options;

                return await stepContext.EndDialogAsync(bookingDetails, cancellationToken);
            }

            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private static bool IsAmbiguous(string timex)
        {
            var timexProperty = new TimexProperty(timex);
            return !timexProperty.Types.Contains(Constants.TimexTypes.Definite);
        }
    }
}
