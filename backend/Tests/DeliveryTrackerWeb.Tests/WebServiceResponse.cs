using System.Net;
using System.Net.Http;

namespace DeliveryTrackerWeb.Tests
{
    public class WebServiceResponse
    {
        public HttpStatusCode StatusCode => this.HttpResponse.StatusCode;
        public HttpResponseMessage HttpResponse { get; set; }
    }
}