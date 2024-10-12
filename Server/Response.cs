using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


    public class Response
    {

        public string Status { get; set; } 
        public string Body { get; set; }


        public Response(string status, string body) // constructors
        {
            Status = status;
            Body = body;

        }

  
    public void AddOrAppendToStatus(string statusMessage) //Method that adds or appends to Status 
        {
            if (Status == "")
            {
                Status = statusMessage;
            }
            else
            {
                Status = Status + " ," + statusMessage;
            }
        }


}

