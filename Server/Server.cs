using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using System.Runtime.InteropServices;
using System.Text.Json;
using Assignment3TestSuite;
using System.Reflection;
using Xunit.Sdk;
using System.IO;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.Interfaces;



public class Server
{
    private readonly int _port;

    public Server(int port)
    {
        _port = port;


    }


    public void Run()
    {

        var server = new TcpListener(IPAddress.Loopback, _port); // IPv4 127.0.0.1 IPv6 ::1
        server.Start();

        Console.WriteLine($"Server started on port {_port}");

        while (true)
        {

            Console.WriteLine("Waiting for Client to connect");
            var client = server.AcceptTcpClient();
            Console.WriteLine("Client connected!!!");

            Task.Run(() => ValidateRequest(client, GetRequest(client)));


            /*Boiler plate try-catch
             * try
            {
                var stream = client.GetStream();
                string msg = ReadFromStream(stream);

                Console.WriteLine("Message from client: " + msg);
               


            }
            catch { }*/


        }




    }

    private string ReadFromStream(NetworkStream stream)
    {
        var buffer = new byte[1024];
        var readCount = stream.Read(buffer);
        return Encoding.UTF8.GetString(buffer, 0, readCount);
    }

    private void WriteToStream(NetworkStream stream, string msg)
    {
        var buffer = Encoding.UTF8.GetBytes(msg);
        stream.Write(buffer);
        stream.Flush();
    }



    public List<Category> PrintCategories()
    {
        List<Category> categories = new List<Category>()
    {
        new Category(1,"Beverages"),
        new Category(2, "Condiments"),
        new Category(3, "Confections")
    };
        return categories;

    }

    public Request GetRequest(TcpClient client)
    {
        var options = new JsonSerializerOptions // requesting case insensitivty, might move this to outside of the method for general usgae
        {
            PropertyNameCaseInsensitive = true
        };


        var stream = client.GetStream();
        var jsonMessage = ReadFromStream(stream);
        
        Request? clientRequest = clientRequest = JsonSerializer.Deserialize<Request>(jsonMessage, options);
        // Console.WriteLine("clientRequest body" + clientRequest.Body);  Proof of concept - getting body here
        return clientRequest;
    }


    
    


    void ValidateRequest(TcpClient client, Request req)
    {
        
        Response response = new Response();
        var stream = client.GetStream();
        string[] validMethods = ["create", "read", "update", "delete", "echo"];
        string[] validMethodsForBody = ["create", "update", "echo"];



        EchoChecker(req, client);
        IsJason(req, client);

        //Run the ProcessValidatedRequests
        ProcessValidatedRequests(req, client);
        

        response = RequestStructureChecker(req);


        WriteToStream(stream, JsonSerializer.Serialize(response));

    }



    /************************************HELPER METHODS****************************************/
    /******************************************************************************************/
    /******************************************************************************************/
    /******************************************************************************************/


    void EchoChecker(Request request, TcpClient client)
    {
        Response response = new Response();

        var stream = client.GetStream();

        if (request.Method == "echo" && !String.IsNullOrEmpty(request.Body))
        {

            response.ClearBody();
            response.ClearStatus();
            response.AddorAppendToBody(request.Body);
            WriteToStream(stream, JsonSerializer.Serialize(response)); 
            
            return;
        }
        else { return; }

    }


    Response IsJason(Request request, TcpClient client)
    {
        Response response = new Response();
        var stream = client.GetStream();
        
        if (!String.IsNullOrEmpty(request.Body)) { 

        try
        {

                JsonDocument.Parse(request.Body);
        }
        catch
            {

            response.AddOrAppendToStatus("illegal body");
         
            WriteToStream(stream, JsonSerializer.Serialize(response));


            return response;
        }

            return response;
        }
        return response;
    }


    /*******************************************METHODS****************************************/
    /******************************************************************************************/
    /******************************************************************************************/
    /******************************************************************************************/

    Response RequestStructureChecker(Request request)
    {
        Response response = new Response();


        Response DateResponse = new Response();
        bool dateBool;
        Response MethodResponse = new Response();
        bool methodBool;
        Response BodyResponse = new Response();
        bool bodyBool;
        Response PathResponse = new Response();
        bool pathBool;


        //DateResponse
        DateResponse.Body = ValidateDate(request, out dateBool).Body;
        DateResponse.Status = ValidateDate(request, out dateBool).Status;
        //MethodResponse
        MethodResponse.Body = ValidateMethod(request, out methodBool).Body;
        MethodResponse.Status = ValidateMethod(request, out methodBool).Status;
        //BodyResponse
        BodyResponse.Body = ValidateBody(request, out bodyBool).Body;
        BodyResponse.Status = ValidateBody(request, out bodyBool).Status;
        //PathResponse
        PathResponse.Body = ValidatePath(request, out pathBool).Body;
        PathResponse.Status = ValidatePath(request, out pathBool).Status;


        response.AddResponse(DateResponse, BodyResponse, MethodResponse, PathResponse);

        return DateResponse;
        
    }



    Response ValidateDate(Request request, out bool DateValidated)
    {
        DateValidated = false;
        Response response = new Response();

        if (request.Date == null)
        {
            response.AddOrAppendToStatus("missing date");
            DateValidated = false;
            return response;
        }

        if (request.Date.Contains("/")) //uh not ideal but... works? if time (haha) allows do something smarter
        {
            response.AddOrAppendToStatus("illegal date");
            DateValidated = false;
            return response;
        }
            DateValidated = true;
        return response;
    }

    Response ValidateMethod(Request request, out bool MethodValidated)
    {
        MethodValidated = false;
        Response response = new Response();
        string[] validMethods = ["create", "read", "update", "delete", "echo"];
        string[] validMethodsForBody = ["create", "update", "echo"];


        if (request.Method == null)
        {
            Console.WriteLine("Method not found, thus missing");
            response.AddOrAppendToStatus("missing method");
            MethodValidated = false;
            return response;
        }


        foreach (var method in validMethods) {
            if (!method.Any(request.Method.Contains)) { 
            
                Console.WriteLine("Method does not exist thus illegal");
                response.AddOrAppendToStatus("illegal method");
                MethodValidated = false;
                return response;
            }
        }
        MethodValidated = true;
        return response;
    }

    Response ValidateBody(Request request, out bool BodyValidated)
    { 
        BodyValidated = false;
        Response response = new Response();

        if (String.IsNullOrEmpty(request.Body)) {
            Console.WriteLine("Body contains nothing");
            response.AddOrAppendToStatus("missing body");

            return response;
        }
        BodyValidated = true;
        return response;
    }

     Response ValidatePath(Request request, out bool PathValidated) {

        Response response = new Response();


        List<Category> categories = new List<Category>()
        {
        new Category(1,"Beverages"),
        new Category(2, "Condiments"),
        new Category(3, "Confections")
        };
        const string partialPath = "/api/"; //We hardcode the correct path here, as we only have one path.

        foreach (Category category in categories)
        {
            if (String.IsNullOrEmpty(request.Path))
            {
                Console.WriteLine("Path contains nothing OR nothing approved");
                response.AddOrAppendToStatus("missing resource");
                PathValidated = false;
                return response;
            }
            if (request.Path.Contains(partialPath))
            {
                   

                PathValidated = true;
                return response; 
            }
            
        }
        PathValidated = true;
        return response;
    }

    
    Response ProcessValidatedRequests(Request request, TcpClient client)
    {
        Response response = new Response();
        var stream = client.GetStream();


        List<Category> categories = new List<Category>()
        {
        new Category(1,"Beverages"),
        new Category(2, "Condiments"),
        new Category(3, "Confections")
        };
        const string smallerPartialPath = "/api/"; // hardcoded smallerpartial path here

        const string partialPath = "/api/categories"; //We hardcode the partial path here
        
        IEnumerable<int> query = from Category category in categories
                                 select category.cid;
        foreach (int cid in query)
        {
            string testvar = partialPath + $"/{cid}";

            if (request.Path == testvar)
            {
                Console.WriteLine($"Path {cid} was accessed");
                return response;
            }
            /*else if (request.Path == partialPath)
            {
                Console.WriteLine("you accessed the whole categories table");
                return response;
            }*/

            if ( String.IsNullOrEmpty(request.Path)) {
                Console.WriteLine("is null");// if null, return back to ValidateRequest()
            }

            else if(request.Path.Contains(smallerPartialPath) && !request.Path.Equals(partialPath))
            {
                Console.WriteLine("Path does not exist");
                response.ClearStatus();
                response.SetBodyToNull();
                response.AddOrAppendToStatus("4 bad request");
                WriteToStream(stream, JsonSerializer.Serialize(response));
                return response;

            }

        }
        
        
        return response;
    }



    public static string ToJson(Response response)
    {
        return JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public static Request? FromJson(string element)
    {
        return JsonSerializer.Deserialize<Request>(element, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    


}
