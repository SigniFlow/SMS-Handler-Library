using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Moq;
using SigniFlow.SMSHandler.Exceptions;
using SigniFlow.SMSHandler.Models.Configuration;
using SigniFlow.SMSHandler.Models.HTTP;

namespace SigniFlow.SMSHandler.Tests;

[TestFixture]
[ExcludeFromCodeCoverage]
public class SmsEventDelegatorTests
{
    private Mock<ISmsEventHandler> _eventHandler;
    private SmsEventDelegator _eventDelegator;
    
    [SetUp]
    public void Setup()
    {
        _eventHandler = new Mock<ISmsEventHandler>();
        
        var authOptions = new SmsHandlerAuthOptions("Itookanarrowtotheknee");
        
        var smsEvent = new SmsEvent("Itookanarrowtotheknee","092847937222","Send OTP", "Your OTP is 12345",DateTime.Now.ToString());

        _eventDelegator = SmsEventDelegatorFactory.GetSmsEventDelegator(smsEvent, _eventHandler.Object, authOptions);
    }

    [Test(Description = "Checks that the constructor sets the event type property correctly")]
    public void Constructor_SetEventTypeProperty()
    {
        Assert.NotNull(_eventDelegator.SmsEventType);
    }
    
    [Test(Description = "Checks that the constructor sets the event handler property correctly")]
    public void Constructor_SetsEventHandlerProperty()
    {
        Assert.NotNull(this._eventDelegator.SmsEventHandler);
    }

    private static IEnumerable<CorrectEventHandlerParameter> EventHandlerMethodTestCases
    {
        get
        {
            yield return new CorrectEventHandlerParameter
            {
                MethodToCheck = handler => handler.HandleSmsOtp(),
                EventType = SmsEventType.SmsOtp
            };
            yield return new CorrectEventHandlerParameter
            {
                MethodToCheck = handler => handler.HandleSmsAccountCreated(),
                EventType = SmsEventType.SmsCreated
            };
            yield return new CorrectEventHandlerParameter
            {
                MethodToCheck = handler => handler.HandleSmsUnkown(),
                EventType = SmsEventType.Unknown
            };
        }
    }
    
    [Test]
    [TestCaseSource(nameof(EventHandlerMethodTestCases))]
    public async Task CallsCorrectEventHandler(CorrectEventHandlerParameter parameter)
    {
        _eventHandler.Setup(parameter.MethodToCheck);
        _eventDelegator.SmsEventType = parameter.EventType;
        await _eventDelegator.HandleSmsEvent();
    }

    [Test]
    [TestCase(12345)]
    public void Throw_InvalidSmsEventTypeException(SmsEventType invalidSmsEventType)
    {
        _eventDelegator.SmsEventType = invalidSmsEventType;
        Assert.ThrowsAsync<InvalidSmsEventTypeException>(async () => await _eventDelegator.HandleSmsEvent());
    }
    
    public class CorrectEventHandlerParameter
    {
        public Expression<Action<ISmsEventHandler>> MethodToCheck { get; set; }
        
        public SmsEventType EventType { get; set; }
    }
}