using System;
using System.Runtime.Serialization;

namespace TemperatureSensor
{
	[DataContract]
	public class TemperatureData
	{
		public TemperatureData(string city, double temperature):this(city, temperature, DateTime.UtcNow)
		{
		}

		public TemperatureData(string city, double temperature, DateTime utcNow) 
		{
			Time = utcNow;
			City = city;
			Temperature = temperature;
		}

		[DataMember]
		public DateTime Time { get; set; }
		[DataMember]
		public String City { get; set; }
		[DataMember]
		public double Temperature { get; set; }
	}
}
