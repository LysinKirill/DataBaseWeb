using DataBaseLib;
using DataBaseWeb.Models;
using Microsoft.AspNetCore.Mvc;
using NewVariant.Exceptions;
using NewVariant.Models;

namespace DataBaseWeb.Controllers;

/// <summary>
/// Контроллер, отвечающий за вывод базы данных: страниц по каждой таблице
/// </summary>
public class DBController : Controller
{
    private DataBase _dataBase = new DataBase();

    //Свойства предствалений таблиц из базы данных
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
        // Загрузка текущего содержимого базы данных
        LoadDataBase();
    }

    // Главная страница сайта
    public IActionResult Index()
    {
        return View("../Home/Index");
    }
    
    // Страница с данными о продажах
    public IActionResult Sales()
    {
        return View(SalesVM);
    }

    // Страница с данными о товарах
    public IActionResult Goods()
    {
        return View(GoodsVM);
    }

    // Страница с данными о магазинах
    public IActionResult Shops()
    {
        return View(ShopsVM);
    }
    
    // Страница с данными о покупателях
    public IActionResult Buyers()
    {
        return View(BuyersVM);
    }

    /// <summary>
    /// Перенаправление пользователя на страницу с подтверждением удаления
    /// </summary>
    /// <param name="tableName">Имя таблицы, которую необходимо удалить</param>
    public IActionResult DeleteTable(string? tableName)
    {
        HashSet<string> availableTables = new HashSet<string>{ "Sales", "Shops", "Buyers", "Goods" };
        if (tableName is null || !availableTables.Contains(tableName))
            return View("../Home/Index");
        ViewData["DataBase"] = _dataBase;
        ViewData["TableToDelete"] = tableName;
        return View();
    }
    
    /// <summary>
    /// Удаление информации из таблицы в базе данных
    /// </summary>
    /// <param name="tableName">Имя таблицы, которую необходимо удалить</param>
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

    /// <summary>
    /// Добавление новой строки в таблицу в базе данных
    /// </summary>
    /// <param name="tableName">Имя таблицы, в которую нужно добавить новое значение</param>
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

    
    /// <summary>
    /// Добавление в базу данных информации о новой продаже
    /// </summary>
    /// <param name="form">Форма, из которой получаются данные об объекте</param>
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
                    ViewData["Information"] = "The identifier should be a non-negative integer number";
                    ViewData["PreviousPage"] = "Sales";
                    return View("InformationPage");
                }

                // Проверка на наличие сведений об объекте с совпадающим id в таблице
                foreach (var x in _dataBase.GetTable<Sale>())
                {
                    if (sale.Id == x.Id)
                    {
                        ViewData["Information"] =
                            $"Unable to add the entry to the table. Id {sale.Id} is already present";
                        ViewData["PreviousPage"] = "Sales";
                        return View("InformationPage");
                    }
                }

                _dataBase.InsertInto<Sale>(() => sale);
                _dataBase.Serialize<Sale>("SalesTemp.txt");
            }
            catch (DataBaseException e)
            {
                ViewData["Information"] = "Error while attempting to add the entry";
                ViewData["PreviousPage"] = "Sales";
                return View("InformationPage");
            }
            catch
            {
                ViewData["Information"] = "Unable to create a new sale";
                ViewData["PreviousPage"] = "Sales";
                return View("InformationPage");
            }
        }
        
        return View("Sales", SalesVM);
    }
    
    /// <summary>
    /// Добавление в базу данных информации о новом товаре
    /// </summary>
    /// <param name="form">Форма, из которой получаются данные об объекте</param>
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
                    ViewData["Information"] = "The identifier should be a non-negative integer number";
                    ViewData["PreviousPage"] = "Goods";
                    return View("InformationPage");
                }
                // Проверка на наличие сведений об объекте с совпадающим id в таблице
                foreach (var x in _dataBase.GetTable<Good>())
                {
                    if (good.Id == x.Id)
                    {
                        ViewData["Information"] =
                            $"Unable to add the entry to the table. Id {good.Id} is already present";
                        ViewData["PreviousPage"] = "Goods";
                        return View("InformationPage");
                    }
                }

                _dataBase.InsertInto<Good>(() => good);
                _dataBase.Serialize<Good>("GoodsTemp.txt");
            }
            catch (DataBaseException e)
            {
                ViewData["Information"] = "Error while attempting to add the entry";
                ViewData["PreviousPage"] = "Goods";
                return View("InformationPage");
            }
            catch
            {
                ViewData["Information"] = "Unable to create a new good";
                ViewData["PreviousPage"] = "Goods";
                return View("InformationPage");
            }
        }
        
        return View("Goods", GoodsVM);
    }
    
    /// <summary>
    /// Добавление в базу данных информации о новом покупателе
    /// </summary>
    /// <param name="form">Форма, из которой получаются данные об объекте</param>
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
                    ViewData["Information"] = "The identifier should be a non-negative integer number";
                    ViewData["PreviousPage"] = "Buyers";
                    return View("InformationPage");
                }

                // Проверка на наличие сведений об объекте с совпадающим id в таблице
                foreach (var x in _dataBase.GetTable<Buyer>())
                {
                    if (buyer.Id == x.Id)
                    {
                        ViewData["Information"] =
                            $"Unable to add the entry to the table. Id {buyer.Id} is already present";
                        ViewData["PreviousPage"] = "Buyers";
                        return View("InformationPage");
                    }
                }

                _dataBase.InsertInto<Buyer>(() => buyer);
                _dataBase.Serialize<Buyer>("BuyersTemp.txt");
            }
            catch (DataBaseException e)
            {
                ViewData["Information"] = "Error while attempting to add the entry";
                ViewData["PreviousPage"] = "Buyers";
                return View("InformationPage");
            }
            catch
            {
                ViewData["Information"] = "Unable to create a new Buyer";
                ViewData["PreviousPage"] = "Sales";
                return View("InformationPage");
            }
        }
        
        return View("Buyers", BuyersVM);
    }
    
    /// <summary>
    /// Добавление в базу данных информации о новом магазине
    /// </summary>
    /// <param name="form">Форма, из которой получаются данные об объекте</param>
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
                    ViewData["Information"] = "The identifier should be a non-negative integer number";
                    ViewData["PreviousPage"] = "Shops";
                    return View("InformationPage");
                }
                // Проверка на наличие сведений об объекте с совпадающим id в таблице
                foreach (var x in _dataBase.GetTable<Shop>())
                {
                    if (shop.Id == x.Id)
                    {
                        ViewData["Information"] =
                            $"Unable to add the entry to the table. Id {shop.Id} is already present";
                        ViewData["PreviousPage"] = "Shops";
                        return View("InformationPage");
                    }
                }

                _dataBase.InsertInto<Shop>(() => shop);
                _dataBase.Serialize<Shop>("ShopsTemp.txt");
            }
            catch (DataBaseException e)
            {
                ViewData["Information"] = "Error while attempting to add the entry";
                ViewData["PreviousPage"] = "Shops";
                return View("InformationPage");
            }
            catch
            {
                ViewData["Information"] = "Unable to create a new shop";
                ViewData["PreviousPage"] = "Shops";
                return View("InformationPage");
            }
        }
        
        return View("Shops", ShopsVM);
    }

    /// <summary>
    /// Метод, производящий загрузку данных в базу данных
    /// </summary>
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