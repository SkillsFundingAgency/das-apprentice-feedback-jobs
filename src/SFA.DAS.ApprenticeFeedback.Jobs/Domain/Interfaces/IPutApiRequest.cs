﻿using Newtonsoft.Json;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Domain.Interfaces
{
    public interface IPutApiRequest
    {
        [JsonIgnore]
        string PutUrl { get; }
    }
}
