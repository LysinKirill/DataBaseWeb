
using DataBaseLib;
using DataBaseWeb.Data;
using NewVariant.Models;
using Microsoft.EntityFrameworkCore;

class Program
{
    

    public static void Main(string[] args)
    {
        DataBase temp = new DataBase();
        try
        {
            temp.Deserialize<Sale>("Sales.txt");
            temp.Deserialize<Shop>("Shops.txt");
            temp.Deserialize<Buyer>("Buyers.txt");
            temp.Deserialize<Good>("Goods.txt");
        }
        catch (Exception e)
        {
            Console.WriteLine("Ошибка при десериализации тестовой базы данных.");
            
            temp.CreateTable<Sale>();
            temp.CreateTable<Shop>();
            temp.CreateTable<Buyer>();
            temp.CreateTable<Good>();
        }
        temp.Serialize<Sale>("SalesTemp.txt");
        temp.Serialize<Shop>("ShopsTemp.txt");
        temp.Serialize<Buyer>("BuyersTemp.txt");
        temp.Serialize<Good>("GoodsTemp.txt");
        
        var builder = WebApplication.CreateBuilder(args);
        

//builder.Services.AddDbContext<SalesContext>(options =>
//    options.UseSqlite(builder.Configuration.GetConnectionString("SalesContext")));



// Add services to the container.
        builder.Services.AddControllersWithViews();

        var app = builder.Build();

// Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}



