namespace DeliveryTrackerWeb.Tests
{
    public class WebServiceResponse <T> : WebServiceResponse
    {
        public T Result { get; set; }
    }
}