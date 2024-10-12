using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class Request
{
   // [JsonProperty("method")]
    public string? Method { get; set; }
   // [JsonProperty("path")]
    public string? Path { get; set; }
   // [JsonProperty("date")]
    public string? Date { get; set; }
  //  [JsonProperty("body")]
    public string? Body { get; set; }
}