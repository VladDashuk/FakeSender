using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using InfluxDB.Client;
using InfluxDB.Client.Writes;
using InfluxDB.Client.Api.Domain;

public class DataRecord
{
    public float Wattage { get; set; }
    public string Timestamp { get; set; } 
}

class Program
{
    static async System.Threading.Tasks.Task Main()
    {
        string filePath = "data.json";

        if (System.IO.File.Exists(filePath))
        {
            string json = System.IO.File.ReadAllText(filePath);
            List<DataRecord> dataRecords = JsonConvert.DeserializeObject<List<DataRecord>>(json);

            string url = "host";
            string token = "token";
            string bucket = "bucket";
            string org = "org";

            using (var client = InfluxDBClientFactory.Create(url, token))
            {
                foreach (var record in dataRecords)
                {
                    DateTime fullDateTime = DateTime.Parse(record.Timestamp);
                    TimeSpan timeOnly = fullDateTime.TimeOfDay;

                    var point = PointData
                        .Measurement("power_usage")
                        .Field("wattage", record.Wattage)
                        .Field("time_only", timeOnly.ToString()) // Не знаю, как иначе
                        .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

                    var writeApi = client.GetWriteApiAsync();
                    await writeApi.WritePointAsync(point, bucket, org);

                }

                Console.WriteLine("Success");
            }
        }
        else
        {
            Console.WriteLine($"File not found: {filePath}");
        }
    }
}
