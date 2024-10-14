using System.Net.Http.Headers;
using System.Runtime.InteropServices;

public class CategoryList
{
    List<Category> categories = new List<Category>()
        {
        new Category(1,"Beverages"),
        new Category(2, "Condiments"),
        new Category(3, "Confections")
        };

    public bool DoesCategoryExist(int CategoryIdentifier) //Method that checks if a category exists
    {



        bool categoryExists = false;
        foreach (Category category in categories)
        {
            if (category.cid == CategoryIdentifier)
            {
                categoryExists = true;
            }
        }
        return categoryExists;
    }
}

public class Category
{

    public Category()
    {

    }

    public Category(int CategoryIdentifier, string CategoryName) //Constructor
    {

        cid = CategoryIdentifier;
        name = CategoryName;
    }


    public int cid { get; set; }
    public string name { get; set; }



}