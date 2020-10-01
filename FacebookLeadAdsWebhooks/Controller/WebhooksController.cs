using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using FacebookLeadAdsWebhooks.Model;
using Newtonsoft.Json;
using System.Configuration;

namespace FacebookLeadAdsWebhooks.Controller
{
    public class WebhooksController : ApiController
    {
        #region Get Request
        [HttpGet]
        public HttpResponseMessage Get()
        {
            string appAccessToken = ConfigurationManager.AppSettings["appAccessToken"];
            if (HttpContext.Current.Request.QueryString["hub.verify_token"] == appAccessToken)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(HttpContext.Current.Request.QueryString["hub.challenge"])
                };

                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                return response;
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
        }
        #endregion Get Request

        #region Post Request

        [HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody] JsonData data)
        {
            try
            {
                var entry = data.Entry.FirstOrDefault();
                var leadUrl = string.Empty;
                var formUrl = string.Empty;
                if(entry != null){
                    var change = entry.Changes.FirstOrDefault();
                    if (change == null) {
                        string token = ConfigurationManager.AppSettings["pageAccessToken"];
                        leadUrl = string.Format("https://graph.facebook.com/v8.0/{0}?access_token={1}", change.Value.LeadGenId, token);
                    }
                    else
                    {
                        return new HttpResponseMessage(HttpStatusCode.BadRequest);
                    }
                }

                using (var httpClientLead = new HttpClient())
                {
                    using (var httpClientFields = new HttpClient())
                    {
                        var responseFields = await httpClientFields.GetStringAsync(leadUrl);
                        if (!string.IsNullOrEmpty(responseFields))
                        {
                            var jsonObjFields = JsonConvert.DeserializeObject<LeadData>(responseFields);
                            //jsonObjFields.FieldData contains the field value
                        }
                    }
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("$Error-->{ex.Message}");
                Trace.WriteLine("$StackTrace-->{ex.StackTrace}");
                return new HttpResponseMessage(HttpStatusCode.BadGateway);
            }
        }

        #endregion Post Request
    }
}
