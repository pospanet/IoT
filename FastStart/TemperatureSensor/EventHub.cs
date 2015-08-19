using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace TemperatureSensor
{
	public static class EventHub
	{
		private const string ContentType = "application/atom+xml;type=entry;charset=utf-8";
		private const string Method = "POST";
		private const string AuthorizationHeaderKey = "Authorization";
		private const string UnderlyingTypeKey = "UnderlyingType";

		private const string SAS = "SharedAccessSignature sr=https%3a%2f%2fbuildprague.servicebus.windows.net%2ftemperature%2fpublishers%2fdatasender&sig=rKyl%2bT61BxbJeBTRogDNp0TCOIiPUKFSNGFr5omO3xI%3d&se=1533364167&skn=Sender";
		//private const string ServiceUri = "https://buildprague.servicebus.windows.net:443/temperature/publishers/DataSender/messages/";
		private const string ServiceUri = "http://buildprague.servicebus.windows.net/temperature/publishers/DataSender/messages/";
		private const string BaseUri = "http://buildprague.servicebus.windows.net";

		public static async Task SendEventHubEventAsync(TemperatureData temperatureData)
		{
			HttpClient client = new HttpClient();

			client.DefaultRequestHeaders.Clear();

			var authHeader = new System.Net.Http.Headers.AuthenticationHeaderValue(
				"SharedAccessSignature", SAS);
			client.DefaultRequestHeaders.Authorization = authHeader;

			DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(TemperatureData));
			string body = string.Empty;
			using (MemoryStream ms = new MemoryStream())
			using (StreamReader sr = new StreamReader(ms))
			{
				serializer.WriteObject(ms, temperatureData);
				ms.Seek(0, SeekOrigin.Begin);
				body = sr.ReadToEnd();
			}
			StringContent content = new StringContent(body, Encoding.UTF8);
			client.BaseAddress = new Uri(BaseUri);

			//await client.PostAsync(ServiceUri, content);

			client.Dispose();
		}
	}
}
