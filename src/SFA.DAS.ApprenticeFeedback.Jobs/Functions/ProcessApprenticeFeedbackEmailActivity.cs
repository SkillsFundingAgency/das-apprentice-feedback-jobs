
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using System;
using System.Threading.Tasks;


namespace SFA.DAS.ApprenticeFeedback.Jobs.Functions
{
    public class ProcessApprenticeFeedbackEmailActivity
    {
        private readonly ILogger<ProcessApprenticeFeedbackEmailActivity> _logger;
        private readonly IApprenticeFeedbackApi _apprenticeFeedbackApi;
        
        public ProcessApprenticeFeedbackEmailActivity(IApprenticeFeedbackApi apprenticeFeedbackApi, ILogger<ProcessApprenticeFeedbackEmailActivity> logger)
        {
            _logger = logger;
            _apprenticeFeedbackApi = apprenticeFeedbackApi;
        }

        [FunctionName("ProcessApprenticeFeedbackEmailActivity")]
        public async Task Run([ActivityTrigger] ApprenticeFeedbackTransaction emailTarget)
        {
            try
            {
                _logger.LogInformation($"Starting Send Apprentice Email Activity with Id {emailTarget.ApprenticeFeedbackTransactionId}");
                var response = await _apprenticeFeedbackApi.ProcessEmailTransaction(emailTarget.ApprenticeFeedbackTransactionId);

                // This Outer Api Call will
                // 1. Get the Apprentice Feedback Target associated with the transaction from the inner api
                //    This will include the apprentice sign in id
                // 2. It will make a call to the Apprentice Accounts Api which will in turn query for
                //    the Apprentice, name, and email, along with their contact preferences.
                // 3. This information will be pushed down to the apprentice feedback api for it to update the
                //    feedback transaction ( e.g. preference not allowed set sent date in future )
                //    It will update the feedback transaction, and set template id if not already set.
                // 4. The apprentice feedback Api will send the message to Notifications via NService bus which means the api
                //    will need configuring to send nservice bus messages
                // 5. Once the command is set, the sent date can be set on the transaction preventing it from being sent again 
                //    should also check it's not already been set to deal with potential paralell processing issues.
                // 6. May also need to account for unit of work whereby the command isn't sent or changes are saved until the request
                //    is completed. Seen similiarly in apprentice commitments jobs.
                _logger.LogInformation($"Finished Send Apprentice Email Activity with Id {emailTarget.ApprenticeFeedbackTransactionId}");

            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unable to send email for Apprentice Feedback Transaction {emailTarget.ApprenticeFeedbackTransactionId}");
            }
        }
    }
}
