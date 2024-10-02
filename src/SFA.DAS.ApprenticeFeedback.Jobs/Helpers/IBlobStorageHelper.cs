using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Helpers
{
    public interface IBlobStorageHelper
    {
        Task MoveBlobAsync(string containerName, string sourceBlobPath, string destinationBlobPath);
    }
}
