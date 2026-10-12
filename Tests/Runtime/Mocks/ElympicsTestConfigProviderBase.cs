using Elympics;
using NUnit.Framework;

namespace ElympicsPlayPad.Tests.Runtime.Mocks
{
    public abstract class ElympicsTestConfigProviderBase
    {
        private const string PathInResources = "Elympics/TestConfig";

        protected abstract bool RequiresTestConfig { get; }

        [OneTimeSetUp]
        protected virtual void OneTimeSetUp()
        {
            if (RequiresTestConfig)
                ElympicsConfig.PathInResources = PathInResources;
        }

        [OneTimeTearDown]
        protected virtual void OneTimeTearDown()
        {
            if (RequiresTestConfig)
                ElympicsConfig.PathInResources = ElympicsConfig.PathInResourcesDefault;
        }
    }
}
