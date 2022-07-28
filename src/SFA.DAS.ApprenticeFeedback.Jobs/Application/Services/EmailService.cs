
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Configuration;
using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Interfaces;
using SFA.DAS.Notifications.Messages.Commands;


namespace SFA.DAS.ApprenticeFeedback.Jobs.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly ApplicationConfiguration _config;

        public EmailService(ApplicationConfiguration config, ILogger<EmailService> logger)
        {
            _logger = logger;
            _config = config;
        }

        public async Task SendFeedbackTransactionEmail(
            IMessageHandlerContext context,
            string registrationId,
            string emailAddress,
            string token1,
            string token2,
            string token3)
        {
            await SendFeedbackTransactionEmail(
                o => context.Send(o),
                registrationId,
                emailAddress,
                token1,
                token2,
                token3);
        }

        public async Task SendFeedbackTransactionEmail(
            Func<object, Task> send,
            string registrationId,
            string emailAddress,
            string token1,
            string token2,
            string token3)
        {
            //var link = $"{_settings.ApprenticeWeb.StartPageUrl}?Register={registrationId}";

            //_logger.LogInformation($"Send ApprenticeSignUpInvitation ({{templateId}}) with {{link}}",
            //    _settings.Notifications.ApprenticeSignUp, link);

            await SendEmail(send, emailAddress,
                _config.Notifications.FeedbackTransaction,
                new Dictionary<string, string>
                {
                    { "This", token1 },
                    { "That", token2 },
                    { "Other", token3 },
                });
        }

        //internal async Task SendApprenticeshipChanged(
        //    IMessageHandlerContext context,
        //    string emailAddress,
        //    string firstName,
        //    string lastName)
        //{
        //    await SendEmail(o => context.Send(o), emailAddress,
        //        _settings.Notifications.ApprenticeshipChanged,
        //        new Dictionary<string, string>
        //        {
        //            { "GivenName", firstName },
        //            { "FamilyName", lastName },
        //            { "ConfirmApprenticeshipUrl", _settings.ApprenticeWeb.ConfirmApprenticeshipUrl.ToString() },
        //        });
        //}

        //internal async Task SendApprenticeshipConfirmed(
        //    IMessageHandlerContext context,
        //    string emailAddress,
        //    string firstName)
        //{
        //    await SendEmail(o => context.Send(o), emailAddress,
        //        _settings.Notifications.ApprenticeshipConfirmed,
        //        new Dictionary<string, string>
        //        {
        //            { "FirstName", firstName },
        //            { "SurveyLink", _settings.SurveyLink.ToString() },
        //            { "MyApprenticeshipUrl", _settings.ApprenticeWeb.StartPageUrl.ToString() },
        //        });
        //}

        //internal async Task SendApprenticeshipStopped(
        //    IMessageHandlerContext context,
        //    string emailAddress,
        //    string firstName,
        //    string employerName,
        //    string apprenticeshipName)
        //{
        //    var link = new Uri(_settings.ApprenticeWeb.StartPageUrl, "home");

        //    await SendApprenticeshipStopped(context, emailAddress, firstName
        //        , employerName, apprenticeshipName, link);
        //}

        //internal async Task SendUnmatchedApprenticeshipStopped(
        //    IMessageHandlerContext context,
        //    string emailAddress,
        //    string firstName,
        //    string employerName,
        //    string apprenticeshipName,
        //    Guid? registrationId)
        //{
        //    var link = new Uri(_settings.ApprenticeWeb.StartPageUrl, $"?Register={registrationId}");

        //    await SendApprenticeshipStopped(context, emailAddress, firstName,
        //        employerName, apprenticeshipName, link);
        //}

        //private async Task SendApprenticeshipStopped(
        //    IMessageHandlerContext context,
        //    string emailAddress,
        //    string firstName,
        //    string employerName,
        //    string apprenticeshipName,
        //    Uri link)
        //{
        //    await SendEmail(o => context.Send(o), emailAddress,
        //        _settings.Notifications.ApprenticeshipStopped,
        //        new Dictionary<string, string>
        //        {
        //            { "FirstName", firstName },
        //            { "EmployerName", employerName },
        //            { "CourseName", apprenticeshipName },
        //            { "ConfirmApprenticeshipUrl", link.ToString() },
        //        });
        //}

        private async Task SendEmail(Func<object, Task> send, string emailAddress, Guid templateId, Dictionary<string, string> tokens)
        {
            var message = new SendEmailCommand(templateId.ToString(), emailAddress, tokens);
            await send(message);
        }
    }
}
