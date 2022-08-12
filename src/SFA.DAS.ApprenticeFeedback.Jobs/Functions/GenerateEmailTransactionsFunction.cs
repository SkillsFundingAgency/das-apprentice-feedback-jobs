﻿
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests;


namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    public class GenerateEmailTransactionsFunction
    {
        private readonly ILogger<GenerateEmailTransactionsFunction> _log;
        private readonly IApprenticeFeedbackApi _api;

        public GenerateEmailTransactionsFunction(ILogger<GenerateEmailTransactionsFunction> log, IApprenticeFeedbackApi api)
        {
            _api = api;
            _log = log;
        }

        [FunctionName("GenerateEmailTransactions")]
        public async Task Run([TimerTrigger("%FunctionsOptions:GenerateEmailTransactionsSchedule%", RunOnStartup = false)] TimerInfo myTimer,
            ILogger log)
        {
            try
            {
                _log.LogInformation("Starting GenerateEmailTransactions");
                GetEmailTransactionsResponse response = await _api.GenerateEmailTransactions();
                _log.LogInformation($"GenerateEmailTransactions completed with {response.Count} transactions with created date {response.CreatedOn}");
                return;
            }
            catch (Exception e)
            {
                log.LogError(e, "GenerateEmailTransactions has failed");
            }
        }
    }
}
