using System.Net.Http.Headers;
using System.Runtime.InteropServices;

public class Category
{

    public Category(int CategoryIdentifier, string CategoryName) //Constructor
    {

        cid = CategoryIdentifier;
        name = CategoryName;
    }

    public int cid { get; set; }
    public string name { get; set; }



}