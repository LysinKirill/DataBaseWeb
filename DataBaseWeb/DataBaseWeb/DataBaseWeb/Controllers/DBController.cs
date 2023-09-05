using DataBaseLib;
using DataBaseWeb.Models;
using Microsoft.AspNetCore.Mvc;
using NewVariant.Exceptions;
using NewVariant.Models;

namespace DataBaseWeb.Controllers;

public class DBController : Controller
{
    private DataBase _dataBase = new DataBase();

    private SalesViewModel SalesVM => new SalesViewModel
    {
        Sales = _dataBase.GetTable<Sale>().ToList()
    };
    
    private GoodsViewModel GoodsVM => new GoodsViewModel
    {
        Goods = _dataBase.GetTable<Good>().ToList()
    };
    
    private ShopsViewModel ShopsVM => new ShopsViewModel
    {
        Shops = _dataBase.GetTable<Shop>().ToList()
    };
    
    private BuyersViewModel BuyersVM => new BuyersViewModel
    {
        Buyers = _dataBase.GetTable<Buyer>().ToList()
    };

    public DBController()
    {
        LoadDataBase();
    }

    // GET
    public IActionResult Index()
    {

        return View("../Home/Index");
    }


    public IActionResult Sales()
    {
        return View(SalesVM);
    }

    public IActionResult Goods()
    {
        return View(GoodsVM);
    }

    public IActionResult Shops()
    {
        return View(ShopsVM);
    }

    public IActionResult Buyers()
    {
        return View(BuyersVM);
    }

    public IActionResult DeleteTable(string? tableName)
    {
        HashSet<string> availableTables = new HashSet<string>{ "Sales", "Shops", "Buyers", "Goods" };
        if (tableName is null || !availableTables.Contains(tableName))
            return View("../Home/Index");
        ViewData["DataBase"] = _dataBase;
        ViewData["TableToDelete"] = tableName;
        return View();
    }
    
    public IActionResult RemoveTable(string? tableName)
    {
        switch (tableName)
        {
            case "Shops":
                System.IO.File.WriteAllText("ShopsTemp.txt", "[]");
                LoadDataBase();
                return View("Shops", ShopsVM);
            case "Sales":
                System.IO.File.WriteAllText("SalesTemp.txt", "[]");
                LoadDataBase();
                return View("Sales", SalesVM);
            case "Buyers":
                System.IO.File.WriteAllText("BuyersTemp.txt", "[]");
                LoadDataBase();
                return View("Buyers", BuyersVM);
            case "Goods":
                System.IO.File.WriteAllText("GoodsTemp.txt", "[]");
                LoadDataBase();
                return View("Goods", GoodsVM);
            default:
                return View("../Home/Index");
        }
    }

    public IActionResult AddEntry(string? tableName)
    {
        switch (tableName)
        {
            case "Sales":
                return View("AddSaleEntry");
            case "Goods":
                return View("AddGoodEntry");
            case "Buyers":
                return View("AddBuyerEntry");
            case "Shops":
                return View("AddShopEntry");
            default:
                return View("../Home/Index");
        }
    }

    
    
    [HttpPost]
    public IActionResult AddSale(IFormCollection form)
    {
        if (ModelState.IsValid)
        {
            try
            {
                Sale sale = new Sale(int.Parse(form["Id"].ToString()), int.Parse(form["BuyerId"].ToString()), int.Parse(form["ShopId"].ToString()),
                    int.Parse(form["GoodId"].ToString()), long.Parse(form["GoodCount"].ToString()));
                if (sale.Id <= 0)
                {
                    Console.WriteLine("Идентификатор должен быть натуральным числом.");
                    return View("Sales", SalesVM);
                }

                // Проверка на наличие сведений об объекте с совпадающим id в таблице
                foreach (var x in _dataBase.GetTable<Sale>())
                {
                    if (sale.Id == x.Id)
                    {
                        Console.WriteLine($"Невозможно добавить данные в таблицу, идентификатор (Id = {sale.Id}) уже присутсвует в таблице.");
                        return View("Sales", SalesVM);
                    }
                }

                _dataBase.InsertInto<Sale>(() => sale);
                _dataBase.Serialize<Sale>("SalesTemp.txt");
            }
            catch (DataBaseException e)
            {
                Console.WriteLine("Ошибка при добавлении информации в таблицу...");
            }
            catch
            {
                Console.WriteLine("Ошибка создания объекта");
            }
        }
        
        return View("Sales", SalesVM);
    }
    
    [HttpPost]
    public IActionResult AddGood(IFormCollection form)
    {
        if (ModelState.IsValid)
        {
            try
            {
                Good good = new Good(int.Parse(form["Id"].ToString()), form["Name"].ToString(), int.Parse(form["ShopId"].ToString()),
                    form["Category"].ToString(), long.Parse(form["Price"].ToString()));
                if (good.Id <= 0)
                {
                    Console.WriteLine("Идентификатор должен быть натуральным числом.");
                    return View("Goods", GoodsVM);
                }
                // Проверка на наличие сведений об объекте с совпадающим id в таблице
                foreach (var x in _dataBase.GetTable<Good>())
                {
                    if (good.Id == x.Id)
                    {
                        Console.WriteLine($"Невозможно добавить данные в таблицу, идентификатор (Id = {good.Id}) уже присутсвует в таблице.");
                        return View("Goods", GoodsVM);
                    }
                }

                _dataBase.InsertInto<Good>(() => good);
                _dataBase.Serialize<Good>("GoodsTemp.txt");
            }
            catch (DataBaseException e)
            {
                Console.WriteLine("Ошибка при добавлении информации в таблицу...");
            }
            catch
            {
                Console.WriteLine("Ошибка создания объекта");
            }
        }
        
        return View("Goods", GoodsVM);
    }
    
    [HttpPost]
    public IActionResult AddBuyer(IFormCollection form)
    {
        if (ModelState.IsValid)
        {
            try
            {
                Buyer buyer = new Buyer(int.Parse(form["Id"].ToString()), form["Name"].ToString(),
                    form["Surname"].ToString(), form["City"].ToString(), form["Country"].ToString());

                if (buyer.Id <= 0)
                {
                    Console.WriteLine("Идентификатор должен быть натуральным числом.");
                    return View("Buyers", BuyersVM);
                }

                // Проверка на наличие сведений об объекте с совпадающим id в таблице
                foreach (var x in _dataBase.GetTable<Buyer>())
                {
                    if (buyer.Id == x.Id)
                    {
                        Console.WriteLine(
                            $"Невозможно добавить данные в таблицу, идентификатор (Id = {buyer.Id}) уже присутсвует в таблице.");
                        return View("Buyers", BuyersVM);
                    }
                }

                _dataBase.InsertInto<Buyer>(() => buyer);
                _dataBase.Serialize<Buyer>("BuyersTemp.txt");
            }
            catch (DataBaseException e)
            {
                Console.WriteLine("Ошибка при добавлении информации в таблицу...");
            }
            catch
            {
                Console.WriteLine("Ошибка создания объекта");
            }
        }
        
        return View("Buyers", BuyersVM);
    }
    
    [HttpPost]
    public IActionResult AddShop(IFormCollection form)
    {
        if (ModelState.IsValid)
        {
            try
            {
                Shop shop = new Shop(int.Parse(form["Id"].ToString()), form["Name"].ToString(), form["City"].ToString(),
                    form["Country"].ToString());
                
                if (shop.Id <= 0)
                {
                    Console.WriteLine("Идентификатор должен быть натуральным числом.");
                    return View("Shops", ShopsVM);
                }
                // Проверка на наличие сведений об объекте с совпадающим id в таблице
                foreach (var x in _dataBase.GetTable<Shop>())
                {
                    if (shop.Id == x.Id)
                    {
                        Console.WriteLine($"Невозможно добавить данные в таблицу, идентификатор (Id = {shop.Id}) уже присутсвует в таблице.");
                        return View("Shops", ShopsVM);
                    }
                }

                _dataBase.InsertInto<Shop>(() => shop);
                _dataBase.Serialize<Shop>("ShopsTemp.txt");
            }
            catch (DataBaseException e)
            {
                Console.WriteLine("Ошибка при добавлении информации в таблицу...");
            }
            catch
            {
                Console.WriteLine("Ошибка создания объекта");
            }
        }
        
        return View("Shops", ShopsVM);
    }

    private void LoadDataBase()
    {
        try
        {
            _dataBase.Deserialize<Sale>("SalesTemp.txt");
            _dataBase.Deserialize<Shop>("ShopsTemp.txt");
            _dataBase.Deserialize<Buyer>("BuyersTemp.txt");
            _dataBase.Deserialize<Good>("GoodsTemp.txt");
        }
        catch
        {
            Console.WriteLine("Произошла ошибка при десериализации базы данных.");
            _dataBase.CreateTable<Sale>();
            _dataBase.CreateTable<Shop>();
            _dataBase.CreateTable<Buyer>();
            _dataBase.CreateTable<Good>();
        }
    }
}