using NewVariant.Interfaces;
using NewVariant.Models;
using System.Linq;
namespace DataBaseLib;

public class DataAccessLayer : IDataAccessLayer
{
    public DataAccessLayer()
    {
    }

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

        int longestNameId = buyers.Where(x => x.Name.Length == buyers.Max(t => t.Name.Length)).OrderBy(k => k.Name).First().Id;
        
        var query = from good in goods
            where (from sale in sales
                where sale.BuyerId == longestNameId
                select sale.GoodId).Contains(good.Id)
            select good;
        
        return query;
    }

    public string? GetMostExpensiveGoodCategory(IDataBase dataBase)
    {
        var goods = dataBase.GetTable<Good>().ToList();
        if (goods.Count == 0)
            return "The database contains no information about goods";

        return goods.Last(x => x.Price == goods.Max(t => t.Price)).Category;
    }

    public string? GetMinimumSalesCity(IDataBase dataBase)
    {
        // Если в городе ничего не было куплено, то он не учитывается. Переделать!
        var sales = dataBase.GetTable<Sale>().ToList();
        var shops = dataBase.GetTable<Shop>().ToList();
        var goods = dataBase.GetTable<Good>();
        
        
        // Проверка, существует ли город, в котором не было совершено покупок
        var cities = shops.ToDictionary(x => x.City, x => false);

        var presentCities = (from sale in sales
            join shop in shops on sale.ShopId equals shop.Id
            select shop.City).ToHashSet();
        
        foreach (var city in cities)
        {
            if (presentCities.Contains(city.Key))
                cities[city.Key] = true;
        }

        if (cities.Any(x => !x.Value))
            return cities.First(x => !x.Value).Key;

        foreach (var pv in cities)
        {
            Console.WriteLine(pv);
        }

        var query = (from sale in sales
            join shop in shops on sale.ShopId equals shop.Id
            join good in goods on sale.GoodId equals good.Id
            group new {sale, good.Price, sale.GoodCount} by shop.City).ToList();
        var auxQuery = (from saleGroup in query
            orderby saleGroup.Sum(x => x.Price * x.GoodCount)
            select saleGroup.Key).ToList();
        
        return !auxQuery.Any() ? "The database contains no information about cities" : auxQuery.First();
    }

    public IEnumerable<Buyer> GetMostPopularGoodBuyers(IDataBase dataBase)
    {
        var sales = dataBase.GetTable<Sale>().ToList();
        var buyers = dataBase.GetTable<Buyer>();

        var query = (from goodsGroup in
                (from sale in sales
                    group sale.GoodCount by sale.GoodId)
            orderby goodsGroup.Sum()
            select goodsGroup.Key);
        
        if (query.Count() == 0)
            return new List<Buyer>();
        
        int mostPopularGoodId = 
            query.Last();

        var buyerIds = (from sale in sales
            where sale.GoodId == mostPopularGoodId
            select sale.BuyerId).ToHashSet();

        return from buyer in buyers
            where buyerIds.Contains(buyer.Id)
            select buyer;
    }

    public int GetMinimumNumberOfShopsInCountry(IDataBase dataBase)
    {
        var shops = dataBase.GetTable<Shop>();

        var query =
            (from shopsByCountry in
                    (from shop in shops
                        group shop by shop.Country)
                orderby shopsByCountry.Count()
                select shopsByCountry.Count());
        
        var enumerable = query.ToList();
        return !enumerable.Any() ? 0 : enumerable.First();


    }

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