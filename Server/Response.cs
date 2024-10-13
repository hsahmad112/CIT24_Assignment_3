using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit.Sdk;


public class Response
    {

        public string? Status { get; set; } 
        public string? Body { get; set; }


        public Response(string status, string body) // constructors
        {
            Status = status;
            Body = body;

        }
    public Response() // constructors
    {


    }

    public void AddOrAppendToStatus(string statusMessage) //Method that adds or appends to Status 
        {
            if (Status == "" || Status == null)
            {
                Status = statusMessage;
            }
            else
            {
                Status = Status + " , " + statusMessage;
            }
        }

    public void AddorAppendToBody(string bodyMessage)
    {
        if (Body == "")
        {
            Body = bodyMessage;
        }
        else
        {
            Body = Body + " , " + bodyMessage;
        }
    }


    public Response AddResponse(Response oldResponse, Response newResponse)
    {
        string delimiter = " , ";
        Response combinedResponse = new Response();
        combinedResponse.Status = oldResponse.Status += delimiter  += newResponse.Status;
        combinedResponse.Body = oldResponse.Body += delimiter += newResponse.Body;

        return combinedResponse;
    }

    public void ClearStatus()
    {
        Status = "";
    }


    public void ClearBody()
    {
        Body = "";
    }
}

