using Xunit;
using Silksong.Services;


namespace Silksong.Tests.Services
{
    public class SharedLinkServiceTest
    {
        [Fact]
        public void ReceivedSharedLink_WhenSuscribed_InvokesEvent()
        {
            var service = new SharedLinkService();
            string? receivedLink = null;

            service.Subscribe(link => receivedLink = link);
            service.ReceiveSharedLink("http://example.com");


            Assert.Equal("http://example.com", receivedLink);
        }

        [Fact]
        public void ReceiveSharedLink_WhenNoSubscriber_StoresLink()
        {
            var service = new SharedLinkService();
            service.ReceiveSharedLink("http://example.com");

            Assert.Equal("http://example.com", service.ConsumePendingLink());
        }

        [Fact]

        public void ReceivedSharedLink_WhenNoSuscriber_NotStoringLink()
        {
            var service = new SharedLinkService();
            ;
            service.ReceiveSharedLink(null);
            service.ReceiveSharedLink("");
            service.ConsumePendingLink();
        }


        [Fact]
        public void ConsumePendingLink_ReturnsValue_AndClearsIt()
        {
            var service = new SharedLinkService();
            service.ReceiveSharedLink("http://example.com");

            var firstCall = service.ConsumePendingLink();
            var secondCall = service.ConsumePendingLink();

            Assert.Equal("http://example.com", firstCall);
            Assert.Null(secondCall);
        }

        [Fact]
        public void Suscribe_WhenPendingLinkExists_CallsCallbackInmediately()
        {
            var service = new SharedLinkService();
            service.ReceiveSharedLink("http://example.com");
            string? receivedLink = null;

            service.Subscribe(link => receivedLink = link);

            Assert.Equal("http://example.com", receivedLink); ; ;
            Assert.Null(service.ConsumePendingLink());
        }

        //covered cases when:

        //receiving link with suscribe event, invokes event
        //receiving link without suscribe event, stores link
        //receiving invalid link (null or empty), it doesn't works
        //suscribe when there are pending link, calls to the callback and pending links gets clear
        //consume pending link, returns link and clears _pendingLink


    }
}