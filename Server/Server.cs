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
using System.Text.RegularExpressions;



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



        EchoChecker(req, client);
        IsJason(req, client);

        //Run the ProcessValidatedRequests

        response = ProcessValidatedRequests(req, client);

        response = RequestValidator(req);



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

        if (!String.IsNullOrEmpty(request.Body))
        {

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

    Response RequestValidator(Request request)
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

        DateResponse.Status = ValidateDate(request, out dateBool).Status;
        //MethodResponse

        MethodResponse.Status = ValidateMethod(request, out methodBool).Status;
        //BodyResponse

        BodyResponse.Status = ValidateBody(request, out bodyBool).Status;
        //PathResponse

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


        foreach (var method in validMethods)
        {
            if (!method.Any(request.Method.Contains))
            {

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

        if (String.IsNullOrEmpty(request.Body) /*&& request.Method != "create"*/)
        {
            Console.WriteLine("Body contains nothing");
            response.AddOrAppendToStatus("missing body");

            return response;
        }
        BodyValidated = true;
        return response;
    }

    Response ValidatePath(Request request, out bool PathValidated)
    {

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

        CategoryList categoryList = new CategoryList();

        const string smallerPartialPath = "/api/"; // hardcoded smallerpartial path here

        const string partialPath = "/api/categories"; //We hardcode the partial path here



        var regDoesPathContainNumber = new Regex(@"/api/categories/\d+");



        switch (request.Method)
        {
            case "create":
                if (categoryList.DoesCategoryExist(4)
                    break;
        }

        if (request.Path == testvar)
        {
            Console.WriteLine($"Path  was accessed");
            if (request.Method.Equals("create"))
            {
                Console.WriteLine("Trying to create on something that exists");
                response.ClearStatus();
                response.SetBodyToNull();
                response.AddOrAppendToStatus("4 bad request");
                WriteToStream(stream, JsonSerializer.Serialize(response));
                return response;
            }

            if (request.Method.Equals("read"))
            {
                Console.WriteLine($"trying to read CID:");
                response.ClearStatus();
                response.ClearBody();
                response.AddOrAppendToStatus("1 Ok");
                response.AddorAppendToBody(JsonSerializer.Serialize(categories[cid - 1]));
                WriteToStream(stream, JsonSerializer.Serialize(response));
                return response;
            }

            return response;
        }
        if (request.Path == partialPath)
        {
            Console.WriteLine("you accessed the whole categories table");
            if (request.Method.Equals("update"))
            {
                Console.WriteLine("trying to update the whole table");
                response.ClearStatus();
                response.SetBodyToNull();
                response.AddOrAppendToStatus("4 bad request");
                WriteToStream(stream, JsonSerializer.Serialize(response));
                return response;
            }

            if (request.Method.Equals("delete"))
            {
                Console.WriteLine("trying to delete the whole table incorrectly");
                response.ClearStatus();
                response.SetBodyToNull();
                response.AddOrAppendToStatus("4 bad request");
                WriteToStream(stream, JsonSerializer.Serialize(response));
            }
            if (request.Method.Equals("read"))
            {
                Console.WriteLine($"trying to read the whole table");
                response.ClearStatus();
                response.ClearBody();
                response.AddOrAppendToStatus("1 Ok");
                response.AddorAppendToBody(JsonSerializer.Serialize(categories));
                WriteToStream(stream, JsonSerializer.Serialize(response));
                return response;
            }

            return response;
        }

        if (String.IsNullOrEmpty(request.Path))
        {
            Console.WriteLine("is null");// if null, return back to ValidateRequest()
        }

        else if (request.Path.Contains(smallerPartialPath) && !request.Path.Equals(partialPath) && request.Path.Contains("1"))

        //  bool isIntString = "your string".All(char.IsDigit)
        //source: https://stackoverflow.com/questions/18251875/in-c-how-to-check-whether-a-string-contains-an-integer
        {
            Console.WriteLine("Path does not exist");
            response.ClearStatus();
            response.SetBodyToNull();
            response.AddOrAppendToStatus("4 bad request");
            WriteToStream(stream, JsonSerializer.Serialize(response));
            return response;

        }
        else
        {
            Console.WriteLine("You entered a number in your path but it does not exist");
            response.ClearStatus();
            response.SetBodyToNull();
            response.AddOrAppendToStatus("5 Not found");
            WriteToStream(stream, JsonSerializer.Serialize(response));
            return response;
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
