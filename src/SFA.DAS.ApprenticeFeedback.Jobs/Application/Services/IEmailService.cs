
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
    public interface IEmailService
    {
         Task SendFeedbackTransactionEmail(
            IMessageHandlerContext context,
            string registrationId,
            string emailAddress,
            string token1,
            string token2,
            string token3);

        Task SendFeedbackTransactionEmail(
            Func<object, Task> send,
            string registrationId,
            string emailAddress,
            string token1,
            string token2,
            string token3);

    }
}
