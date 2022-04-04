using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SFA.DAS.ApprenticeFeedback.Jobs.Infrastructure
{
    public static class AzureQueueNameShortener
    {
        private const int AzureServiceBusRuleNameMaxLength = 50;
        private const int ShortenedNameHashLength = 8;
        private const int ShortenedNameContextLength =
            AzureServiceBusRuleNameMaxLength - ShortenedNameHashLength - 1;

        /// <summary>
        /// <para>
        /// Convert event names that are too long for Azure to a shorter, but legible, representation
        /// by removing the common namespace identifiers and focussing on the interesting specifics
        /// for a particular event.
        /// </para>
        /// <para>
        /// e.g.
        ///     SFA.DAS.CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent
        ///     becomes
        ///     CommitmentsV2.ApprenticeshipCreatedEvent
        /// </para>
        /// </summary>
        /// <param name="eventType">The event type to shorten.</param>
        /// <returns></returns>
        public static string Shorten(Type eventType)
        {
            var ruleName = eventType.FullName
                ?? throw new ArgumentException("Could not find name of eventType");
            var importantName = ruleName.Replace("SFA.DAS.", "").Replace(".Messages.Events", "");

            if (importantName.Length <= AzureServiceBusRuleNameMaxLength)
                return importantName;

            var r = new Regex(@"\b(\w+)$");
            var lastWord = r.Match(importantName).Value;
            if (lastWord.Length > ShortenedNameContextLength)
                lastWord = lastWord[..ShortenedNameContextLength];

            using var md5 = new SHA512Managed();
            var hash = md5.ComputeHash(Encoding.Default.GetBytes(ruleName));
            var hashstr = BitConverter.ToString(hash).Replace("-", string.Empty);

            return $"{lastWord}.{hashstr[..ShortenedNameHashLength]}";
        }
    }
}