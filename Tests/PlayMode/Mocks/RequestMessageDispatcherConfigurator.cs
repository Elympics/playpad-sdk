using System;
using System.Linq;
using System.Reflection;
using ElympicsPlayPad.ExternalCommunicators.WebCommunication;
using NUnit.Framework;

namespace ElympicsPlayPad.Tests.PlayMode.Mocks
{
    internal static class RequestMessageDispatcherConfigurator
    {
        public static RequestMessageDispatcher SetTimeoutLenght(this RequestMessageDispatcher sut, TimeSpan newLenght)
        {
            var timeOutInSec = sut.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(x => x.Name == RequestMessageDispatcher.RequestTimeOutSecFieldName);
            Assert.NotNull(timeOutInSec);
            timeOutInSec.SetValue(sut,newLenght);
            return sut;
        }
    }
}
