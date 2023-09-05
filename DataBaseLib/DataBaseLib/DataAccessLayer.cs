using NewVariant.Interfaces;
using NewVariant.Models;
namespace DataBaseLib;

/// <summary>
/// Класс, производящий обработку данных из объекта, реализующего IDataBase с помошью LINQ-запросов
/// </summary>
public class DataAccessLayer : IDataAccessLayer
{
    public DataAccessLayer()
    {
    }

    /// <summary>
    /// Возвращает список всех товаров, купленных покупателем с самым
    /// длинным именем. Если самых длинных имен несколько, то возвращается
    /// список для последнего в лексикографическом порядке имени
    /// </summary>
    /// <param name="dataBase">Обрабатываемая база данных</param>
    /// <returns>Список товаров покупателя с самым длинным именем</returns>
    public IEnumerable<Good> GetAllGoodsOfLongestNameBuyer(IDataBase dataBase)
    {
        var buyers = dataBase.GetTable<Buyer>().ToList();
        var sales = dataBase.GetTable<Sale>();
        var goods = dataBase.GetTable<Good>();
        if (buyers.Count == 0)
        {
            Console.WriteLine("Нет данных о покупателях.");
            return new List<Good>();
        }

        // Определение id покупателя с самым длинным именем
        int longestNameId = buyers.Where(x => x.Name.Length == buyers.Max(t => t.Name.Length)).OrderBy(k => k.Name).First().Id;
        
        var query = from good in goods
            where (from sale in sales
                where sale.BuyerId == longestNameId
                select sale.GoodId).Contains(good.Id)
            select good;
        
        return query;
    }

    /// <summary>
    /// Метод, определяющий категорию самого дорогого товара.
    /// </summary>
    /// <param name="dataBase">Обрабатываемая база данных</param>
    /// <returns>Название категории самого дорогого товара</returns>
    public string? GetMostExpensiveGoodCategory(IDataBase dataBase)
    {
        var goods = dataBase.GetTable<Good>().ToList();
        
        // Если база данных не содержит сведений о товарах, то возвращается соответсвующее сообщение
        if (!goods.Any())
            return "The database contains no information about goods";

        return goods.Last(x => x.Price == goods.Max(t => t.Price)).Category;
    }

    /// <summary>
    /// Метод, определяющий город, в котором было потрачено меньше всего денег
    /// </summary>
    /// <param name="dataBase">Обрабатываемая база данных</param>
    /// <returns>Название города с минимальной суммарной стоимостью всех покупок</returns>
    public string? GetMinimumSalesCity(IDataBase dataBase)
    {
        var sales = dataBase.GetTable<Sale>().ToList();
        var shops = dataBase.GetTable<Shop>().ToList();
        var goods = dataBase.GetTable<Good>();
        
        
        // Проверка, существует ли город, в котором не было совершено покупок
        var cities = (from shop in shops 
            select shop.City).ToHashSet().ToDictionary(x => x, _ => false);

        // Составление множества городов, сведения о продаже в которых содержатся в базе данных
        var presentCities = (from sale in sales
            join shop in shops on sale.ShopId equals shop.Id
            select shop.City).ToHashSet();
        
        foreach (var city in cities)
        {
            if (presentCities.Contains(city.Key))
                cities[city.Key] = true;
        }
        // Если есть город, в котором не совершалось покупок, то возвращается название этого города, т.к. покупок в нем меньше всего - 0
        if (cities.Any(x => !x.Value))
            return cities.First(x => !x.Value).Key;
        

        var query = (from sale in sales
            join shop in shops on sale.ShopId equals shop.Id
            join good in goods on sale.GoodId equals good.Id
            group new {sale, good.Price, sale.GoodCount} by shop.City).ToList();
        
        var auxQuery = (from saleGroup in query
            orderby saleGroup.Sum(x => x.Price * x.GoodCount)
            select saleGroup.Key).ToList();
        
        // Если база данных не содержит сведений о городах, то вместо названия гороода возвращается соответсвующее сообщение
        return !auxQuery.Any() ? "The database contains no information about cities" : auxQuery.First();
    }

    /// <summary>
    /// Метод, возвращающий списко покупателей, которые приобрели товар, чьих единиц было приоберетено максимальное количество
    /// </summary>
    /// <param name="dataBase">Обрабатываемая база данных</param>
    /// <returns>Список покупателей</returns>
    public IEnumerable<Buyer> GetMostPopularGoodBuyers(IDataBase dataBase)
    {
        var sales = dataBase.GetTable<Sale>().ToList();
        var buyers = dataBase.GetTable<Buyer>();

        // Нахождение количетсва проибретенных единиц каждого товара
        var query = 
            (from goodsGroup in
                (from sale in sales
                    group sale.GoodCount by sale.GoodId)
            orderby goodsGroup.Sum()
            select goodsGroup.Key).ToList();
        
        // Если никакой товар не был приобретён, то возвращаем пустой список покупателей
        if (!query.Any())
            return new List<Buyer>();
        
        int mostPopularGoodId = 
            query.Last();
        
        // Определение идентификаторов искомых покупателей
        var buyerIds = (from sale in sales
            where sale.GoodId == mostPopularGoodId
            select sale.BuyerId).ToHashSet();

        return from buyer in buyers
            where buyerIds.Contains(buyer.Id)
            select buyer;
    }

    /// <summary>
    /// Определяет наименьшее число магазинов по всем странам, представленным в базе данных
    /// </summary>
    /// <param name="dataBase">Обрабатываемая база данных</param>
    /// <returns>Минимальное число по странам</returns>
    public int GetMinimumNumberOfShopsInCountry(IDataBase dataBase)
    {
        var shops = dataBase.GetTable<Shop>();

        // Группировка магазинов по странам и определение количетсва магазинов в каждой группе
        var query = 
            (from shopsByCountry in
                    (from shop in shops
                        group shop by shop.Country)
                orderby shopsByCountry.Count()
                select shopsByCountry.Count()).ToList();
        
        // Если база данных не содержит сведений о магазнах, то минимальное количество магазинов в стране равно 0
        return !query.Any() ? 0 : query.First();
    }

    /// <summary>
    /// Составляет список покупок, совершённых покукпателями, живущими в городе отличном от города, в котором находится магазин
    /// </summary>
    /// <param name="dataBase">Обрабатываемая база данных</param>
    /// <returns>Список покупок, совершённых покупателями, не проживающими в городе, где расположен магазин</returns>
    public IEnumerable<Sale> GetOtherCitySales(IDataBase dataBase)
    {
        var buyers = dataBase.GetTable<Buyer>().ToList();
        var sales = dataBase.GetTable<Sale>().ToList();
        var shops = dataBase.GetTable<Shop>().ToList();

        var salesByCities = from sale in sales
            join shop in shops on sale.ShopId equals shop.Id
            select new { sale, shop.City };
        
        var query = from buyer in buyers
            from sale in salesByCities
            where (sale.City != buyer.City) & (sale.sale.BuyerId == buyer.Id)
            select sale.sale;
        
        return query;

    }

    /// <summary>
    /// Определяет общую стоимость покупок, совершённых всеми покупателями
    /// </summary>
    /// <param name="dataBase">Обрабатываемая база данных</param>
    /// <returns>Суммарная стоимость всех покупок</returns>
    public long GetTotalSalesValue(IDataBase dataBase)
    {
        var sales = dataBase.GetTable<Sale>().ToList();
        var goods = dataBase.GetTable<Good>().ToList();

        long totalSum = (from sale in sales
            join good in goods on sale.GoodId equals good.Id
            select (sale.GoodCount * good.Price)).Sum();
        
        return totalSum;
    }
}