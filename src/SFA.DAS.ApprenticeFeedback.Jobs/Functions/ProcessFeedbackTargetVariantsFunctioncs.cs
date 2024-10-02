using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NLog.Fluent;
using SFA.DAS.ApprenticeCommitments.Jobs.Api;
using SFA.DAS.ApprenticeFeedback.Jobs.Domain.Configuration;
using SFA.DAS.ApprenticeFeedback.Jobs.Functions;
using SFA.DAS.ApprenticeFeedback.Jobs.Helpers;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

public class ProcessFeedbackTargetVariantsFunction
{
    private readonly ILogger<ProcessFeedbackTargetVariantsFunction> _log;
    private readonly IApprenticeFeedbackApi _apprenticeFeedbackApi;
    private readonly IBlobStorageHelper _blobStorageHelper;
    private readonly ApprenticeFeedbackVariantConfiguration _config;

    public ProcessFeedbackTargetVariantsFunction(
        ILogger<ProcessFeedbackTargetVariantsFunction> log, 
        IApprenticeFeedbackApi apprenticeFeedbackApi, 
        IBlobStorageHelper blobStorageHelper,
        ApprenticeFeedbackVariantConfiguration config)
    {
        _log = log;
        _apprenticeFeedbackApi = apprenticeFeedbackApi;
        _blobStorageHelper = blobStorageHelper;
        _config = config;
    }

    [FunctionName(nameof(ProcessFeedbackTargetVariantsFunction))]
    public async Task Run(
        [BlobTrigger("apprentice-feedback-template-variants/new/{name}", Connection = "AzureWebJobsStorage")] Stream blobStream,
        string name)
    {
        var processVariantsRequest = await ExtractVariantsFromFile(blobStream, name);

        if(processVariantsRequest.FeedbackVariants != null && processVariantsRequest.FeedbackVariants.Any())
        {
            await _apprenticeFeedbackApi.ProcessFeedbackTargetVariants(processVariantsRequest); 
        }
        
        await _blobStorageHelper.MoveBlobAsync(_config.BlobContainerName, _config.IncomingFolder, _config.ArchiveFolder);
    }

    private async Task<PostProcessFeedbackVariantsRequest> ExtractVariantsFromFile(Stream blobStream, string fileName)
    {
        var variantList = new List<FeedbackVariant>();

        using var reader = new StreamReader(blobStream);
        var content = await reader.ReadToEndAsync();

        _log.LogInformation($"Processing Feedback Target Variants file : {fileName}");

        var lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i];

            var columns = line.Split(',');

            if (columns.Length == 2)
            {
                if (long.TryParse(columns[0].Trim(), out long parsedId))
                {
                    if (string.IsNullOrEmpty(columns[1].Trim()))
                    {
                        _log.LogWarning($"Empty Variant in row: {line}");
                    }
                    else
                    {
                        variantList.Add(new FeedbackVariant(parsedId, columns[1].Trim()));
                    }
                }
                else
                {
                    _log.LogWarning($"Invalid ApprenticeshipId in row: {line}");
                }
            }
            else
            {
                _log.LogWarning($"Invalid row in file: {line}");
            }
        }

        return new PostProcessFeedbackVariantsRequest
        {
            FeedbackVariants = variantList
        };
    }
}

