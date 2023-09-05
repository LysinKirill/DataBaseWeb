using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DataBaseWeb.Models;

namespace DataBaseWeb.Controllers;

/// <summary>
/// Контроллер, отвечающий за главную страницу сайта
/// </summary>
public class HomeController : Controller
{
    public HomeController(ILogger<HomeController> logger)
    {
    }

    public IActionResult Index()
    {
        return View();
    }
    

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}