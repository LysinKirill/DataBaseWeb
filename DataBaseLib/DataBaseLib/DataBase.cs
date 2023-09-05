using System.Text.Json;
using NewVariant.Exceptions;
using NewVariant.Interfaces;

namespace DataBaseLib;
/// <summary>
/// Класс, представляющий базу данных, которая включает (может включать) в себя таблицы, содержащие сведения о
/// продажах, покупателях, товарах и магазинах
/// </summary>
public class DataBase : IDataBase
{
    
    private Dictionary<Type, List<IEntity>?> _tables;

    public DataBase()
    {
        _tables = new Dictionary<Type, List<IEntity>?>();
    }


    /// <summary>
    /// Создание таблицы продаж / покупателей / товаров / магазинов
    /// </summary>
    /// <typeparam name="T">Тип объекта, по которому создается таблица</typeparam>
    /// <exception cref="DataBaseException">Исключение, сигнализирующее о том, что таблица типа T уже существует в базе данных</exception>
    public void CreateTable<T>() where T : IEntity
    {
        if(_tables.ContainsKey(typeof(T)))
            throw new DataBaseException("Такая таблица уже существует в базе данных");
        _tables[typeof(T)] = new List<IEntity>();
    }

    /// <summary>
    /// Добавление в таблицу базы данных новой строки
    /// </summary>
    /// <param name="getEntity">делегат, возвращающий обект типа T</param>
    /// <typeparam name="T">Параметр, определяющий, в какую таблицу должна быть добавлена информация</typeparam>
    /// <exception cref="DataBaseException">исключение, возникающее при попытке добавления строки в несуществующиую таблицу</exception>
    public void InsertInto<T>(Func<T> getEntity) where T : IEntity
    {
        if (!_tables.ContainsKey(typeof(T)))
            throw new DataBaseException("Такая таблица не существует");
        _tables[typeof(T)]!.Add(getEntity());
    }

    /// <summary>
    /// получает таблицу из базы данных по заданному типу
    /// </summary>
    /// <typeparam name="T">Тип таблицы, которую необходимо получить из базы данных</typeparam>
    /// <returns>Перечислитель, содержащий в себе объекты нужо</returns>
    /// <exception cref="DataBaseException">Ошибка, возникающая при попытке получения таблицы, данных о которой нет в базе данных</exception>
    public IEnumerable<T> GetTable<T>() where T : IEntity
    {
        if (!_tables.ContainsKey(typeof(T)))
            throw new DataBaseException("Такая таблица не существует");
        return _tables[typeof(T)]!.Cast<T>();
    }

    /// <summary>
    /// Метод, сериализующий указанную таблицу из базы данны и записывающий результат в файл
    /// </summary>
    /// <param name="path">путь к файлу, в который будет производиться запись</param>
    /// <typeparam name="T">Тип таблицы, которую необходимо сериализовать</typeparam>
    /// <exception cref="DataBaseException">Исключение, возникающее при попытке сериализации несуществу</exception>
    public void Serialize<T>(string path) where T : IEntity
    {
        if (!_tables.ContainsKey(typeof(T)))
            throw new DataBaseException("Такая таблица не существует");
        
        string jsonString = JsonSerializer.Serialize(_tables[typeof(T)]!.Select(x => (T)x));
        File.WriteAllText(path, jsonString);
    }

    /// <summary>
    /// Метод, десериализующий таблицу из заданного файла
    /// </summary>
    /// <param name="path">Путь к файлу, в котором сериализованная таблица</param>
    /// <typeparam name="T">Тип десериализуемой таблицы</typeparam>
    public void Deserialize<T>(string path) where T : IEntity
    {
        string serializationData = File.ReadAllText(path);
        List<T>? table = JsonSerializer.Deserialize<List<T>>(serializationData);
        _tables[typeof(T)] = table is null ? new List<IEntity>() : table.Select(x => (IEntity)x).ToList();
    }
}