using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;

namespace SFA.DAS.ApprenticeFeedback.Jobs.UnitTests
{
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute()
            : base(() => CreateFixture())
        {
        }

        private static IFixture CreateFixture()
        {
            var fixture = new Fixture();
            fixture.Customize(new AutoMoqCustomization { ConfigureMembers = true });
            return fixture;
        }
    }
}