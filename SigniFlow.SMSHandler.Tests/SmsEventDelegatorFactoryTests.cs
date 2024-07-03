using System.Diagnostics.CodeAnalysis;
using Moq;
using SigniFlow.SMSHandler.Exceptions;
using SigniFlow.SMSHandler.Models.Configuration;
using SigniFlow.SMSHandler.Models.HTTP;


namespace SigniFlow.SMSHandler.Tests;

[TestFixture]
[ExcludeFromCodeCoverage]
public class SmsEventDelegatorFactoryTests
{
    private SmsEvent _smsEvent;
    private Mock<ISmsEventHandler> _smsEventHandler;
    private SmsHandlerAuthOptions _authOptions;

    [SetUp]
    public void Setup()
    {
        _smsEventHandler = new Mock<ISmsEventHandler>();
        
        _authOptions = new SmsHandlerAuthOptions("Itookanarrowtotheknee");
        
        _smsEvent = new SmsEvent("Itookanarrowtotheknee","092847937222","Send OTP", "Your OTP is 12345",DateTime.Now.ToString());
    }

    [Test]
    public void GetEventDelegator_ReturnEventdelegator()
    {
        var delegator = SmsEventDelegatorFactory.GetSmsEventDelegator(_smsEvent, _smsEventHandler.Object, _authOptions);
        Assert.That(delegator,Is.Not.EqualTo(null));
    }

    [Test]
    public void GetEventDelegator_ThrowsInvalidSmsSecret()
    {
        _smsEvent.SmsSecret = "HolyhandGrenadeofEntioch";
        
        Assert.That(
            () => SmsEventDelegatorFactory.GetSmsEventDelegator(_smsEvent,_smsEventHandler.Object,_authOptions),Throws.TypeOf<InvalidSmsSecretException>()
                .With.Message.EqualTo("Err: Sms Secret does not match")
        );
    }
    
    [Test]
    public void GeSmsEventDelegator_ReturnsSmsEventdelegator()
    {
        var eventDelegator =
            SmsEventDelegatorFactory.GetSmsEventDelegator(_smsEvent, _smsEventHandler.Object, _authOptions);
        Assert.That(eventDelegator,Is.InstanceOf<SmsEventDelegator>());
    }
    
    [Test]
    [TestCase("Send OTP", SmsEventType.SmsOtp)]
    [TestCase("Created", SmsEventType.SmsCreated)]
    [TestCase("Authenticate", SmsEventType.SmsAuthenticate)]
    [TestCase("Unknown Event", SmsEventType.Unknown)]
    public void GetSmsDelegator_SmsEventTypeGetCorretEventValue(string testSmsEvent, SmsEventType smsEventType)
    {
        _smsEvent.EventType = testSmsEvent;
        
        var eventDelegator = SmsEventDelegatorFactory.GetSmsEventDelegator(_smsEvent,_smsEventHandler.Object,_authOptions);
        
        Assert.That(eventDelegator.SmsEventType, Is.EqualTo(smsEventType));
    }
    
    [Test]
    public void SmsEventHandler_SmsEvent_PropertySet()
    {
        _smsEventHandler.SetupProperty(p => p.SMSEvent);
        
        var eventDelegator = SmsEventDelegatorFactory.GetSmsEventDelegator(_smsEvent,_smsEventHandler.Object,_authOptions);
        
        Assert.That(eventDelegator.SmsEventHandler.SMSEvent,Is.Not.Null);
    }
}