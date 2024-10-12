using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using System.Runtime.InteropServices;
using System.Text.Json;
using Assignment3TestSuite;
using System.Net.Http.Headers;


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



    public void PrintCategories()
    {

        List<Category> categories = new List<Category>()
    {
        new Category(1,"Beverages"),
        new Category(2, "Condiments"),
        new Category(3, "Confections")
    };
        foreach (Category category in categories)
        {
            Console.WriteLine($"cid: {category.cid}, name: {category.name}");
            Console.WriteLine(JsonSerializer.Serialize<Category>(category));
        }

    }

    public Request GetRequest(TcpClient client)
    {
        var options = new JsonSerializerOptions // requesting case insensitivty, might move this to outside of the method for general usgae
        {
            PropertyNameCaseInsensitive = true
        };


        var stream = client.GetStream();
        var jsonMessage = ReadFromStream(stream);
        Console.WriteLine(jsonMessage);
        Request? clientRequest = clientRequest = JsonSerializer.Deserialize<Request>(jsonMessage, options);
        // Console.WriteLine("clientRequest body" + clientRequest.Body);  Proof of concept - getting body here
        return clientRequest;
    }


    void ValidateRequest(TcpClient client, Request req)
    {
        Response response = new Response("", "");
        var stream = client.GetStream();
        string[] validMethods = ["create", "read", "update", "delete", "echo"];
        string[] validMethodsForBody = ["create", "update", "echo"];

        string partialPath = "API/categories";

        Console.WriteLine(req.Body);
        //if any element in request missing
        if (req.Body == null && req.Date == null && req.Path == null && req.Method == null)
        {
            ;
            if (req.Body == null)
            {
                response.AddOrAppendToStatus("missing body");
            }
            if (req.Date == null)
            {
                response.AddOrAppendToStatus("missing date");
            }
            if (req.Method == null)
            {
                response.AddOrAppendToStatus("missing method");
            }
            if (req.Path == null)
            {
                response.AddOrAppendToStatus("missing resource");

            }

            //WriteToStream(stream, ToJson(response)); //Dont know why this does not work
            WriteToStream(stream, JsonSerializer.Serialize(response)); //temp solution Writes to stream
        }


        foreach (var method in validMethods)
        {
            if (req.Method != method)
            {
                response.AddOrAppendToStatus("illegal method");

                if (req.Body == null && req.Path == null)
                {
                    response.AddOrAppendToStatus("missing resource");

                }

                if (req.Body == null)
                {
                    response.AddOrAppendToStatus("missing body");

                }

                if (req.Date.Contains("/"))
                {
                    response.clearStatus("");
                    response.AddOrAppendToStatus("illegal date");
                    WriteToStream(stream, JsonSerializer.Serialize(response));

                }

                else
                {

                    WriteToStream(stream, JsonSerializer.Serialize(response));
                }


            }


        }
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
