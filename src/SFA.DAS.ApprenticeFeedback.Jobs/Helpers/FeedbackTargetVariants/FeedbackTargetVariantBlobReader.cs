using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Requests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Helpers.FeedbackTargetVariants
{
    public interface IFeedbackTargetVariantBlobReader
    {
        Task<IEnumerable<FeedbackVariant>> ReadVariantsFromBlob(BlobClient blobClient);
    }

    public class FeedbackTargetVariantBlobReader : IFeedbackTargetVariantBlobReader
    {
        private readonly ILogger<FeedbackTargetVariantBlobReader> _logger;

        public FeedbackTargetVariantBlobReader(ILogger<FeedbackTargetVariantBlobReader> logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<FeedbackVariant>> ReadVariantsFromBlob(BlobClient blobClient)
        {
            using (Stream blobStream = await blobClient.OpenReadAsync())
            {
                var variantList = new List<FeedbackVariant>();
                using var reader = new StreamReader(blobStream);
                _logger.LogInformation($"Processing Feedback Target Variants from blob: {blobClient.Name}");

                // Skip the header row
                await reader.ReadLineAsync();

                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var columns = line.Split(',');

                    if (columns.Length == 2)
                    {
                        if (long.TryParse(columns[0].Trim(), out long parsedId))
                        {
                            if (string.IsNullOrEmpty(columns[1].Trim()))
                            {
                                _logger.LogWarning($"Empty Variant in row: {line}");
                            }
                            else
                            {
                                variantList.Add(new FeedbackVariant(parsedId, columns[1].Trim()));
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"Invalid ApprenticeshipId in row: {line}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Invalid row in file: {line}");
                    }
                }
                return variantList;
            }
        }
    
    
    
    }

}
