using DataBaseLib;
using DataBaseWeb.Models;
using Microsoft.AspNetCore.Mvc;
using NewVariant.Models;

namespace DataBaseWeb.Controllers;

public class DataAccessController : Controller
{
    private DataBase _dataBase = new DataBase();
    private DataAccessLayer _dataAccessLayer = new DataAccessLayer();
    
    public DataAccessController()
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


    public IActionResult LongestNameBuyer()
    {

        var goodsViewModel = new GoodsViewModel
        {
            Goods = _dataAccessLayer.GetAllGoodsOfLongestNameBuyer(_dataBase).ToList()
        };
        return View(goodsViewModel);
    }

    public IActionResult MostExpensiveCategory()
    {
        ViewData["MostExpensiveCategory"] = _dataAccessLayer.GetMostExpensiveGoodCategory(_dataBase);
        return View();
    }

    public IActionResult MinimumSalesCity()
    {
        ViewData["MinimumSalesCity"] = _dataAccessLayer.GetMinimumSalesCity(_dataBase);
        return View();
    }

    public IActionResult MostPopularGoodBuyers()
    {
        var buyersViewModel = new BuyersViewModel()
        {
            Buyers = _dataAccessLayer.GetMostPopularGoodBuyers(_dataBase).ToList()
        };
        return View(buyersViewModel);
    }

    public IActionResult MinimumNumberOfShopsInCountry()
    {
        ViewData["MinimumNumberOfShops"] = _dataAccessLayer.GetMinimumNumberOfShopsInCountry(_dataBase);
        return View();
    }

    public IActionResult OtherCitySales()
    {
        var salesViewModel = new SalesViewModel()
        {
            Sales = _dataAccessLayer.GetOtherCitySales(_dataBase).ToList()
        };
        return View(salesViewModel);
    }

    public IActionResult TotalSalesValue()
    {
        ViewData["TotalSalesValue"] = _dataAccessLayer.GetTotalSalesValue(_dataBase);
        return View();
    }

}