
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeFeedback.Jobs.Application.Services;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests;


namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    public class GenerateEmailTransactionsFunction
    {
        private readonly ILogger<GenerateEmailTransactionsFunction> _log;
        private readonly IApprenticeFeedbackEmailTransactionApi _api;
        //private readonly IEmailService _service;

        public GenerateEmailTransactionsFunction(ILogger<GenerateEmailTransactionsFunction> log, IApprenticeFeedbackEmailTransactionApi api) //, IEmailService service)
        {
            _log = log;
            _api = api;
            //_service = service;
        }

        [FunctionName("GenerateEmailTransactions")]
        public async Task Run([TimerTrigger("%FunctionsOptions:GenerateEmailTransactionsOptions:Schedule%", RunOnStartup = false)] TimerInfo myTimer,
            ILogger log)
        {
            try
            {
                _log.LogInformation("Starting GenerateEmailTransactions");
                GetEmailTransactionsResponse response = await _api.GenerateEmailTransactions(new GenerateEmailTransactionsRequest());
                _log.LogInformation($"GenerateEmailTransactions completed with {response.Count} transactions with created date {response.CreatedOn}");

                //_service.SendFeedbackTransactionEmail()

                return;
            }
            catch (Exception e)
            {
                log.LogError(e, "GenerateEmailTransactions has failed");
            }
        }
    }
}
