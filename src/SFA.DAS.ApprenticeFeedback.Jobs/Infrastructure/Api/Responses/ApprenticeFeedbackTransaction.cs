﻿using System;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure.Api.Responses
{
    public class ApprenticeFeedbackTransaction
    {
        public Guid ApprenticeFeedbackTransactionId { get; set; }
        public Guid ApprenticeId { get; set; }
    }
}
