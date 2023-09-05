using System.Text.Json;
using NewVariant.Exceptions;
using NewVariant.Interfaces;

namespace DataBaseLib;

public class DataBase : IDataBase
{

    private Dictionary<Type, List<IEntity>?> _tables;

    public DataBase()
    {
        _tables = new Dictionary<Type, List<IEntity>?>();
    }


    public void CreateTable<T>() where T : IEntity
    {
        if(_tables.ContainsKey(typeof(T)))
            throw new DataBaseException("Такая таблица уже существует в базе данных");
        _tables[typeof(T)] = new List<IEntity>();
    }

    public void InsertInto<T>(Func<T> getEntity) where T : IEntity
    {
        if (!_tables.ContainsKey(typeof(T)))
            throw new DataBaseException("Такая таблица не существует");
        _tables[typeof(T)]!.Add(getEntity());
    }

    public IEnumerable<T> GetTable<T>() where T : IEntity
    {
        if (!_tables.ContainsKey(typeof(T)))
            throw new DataBaseException("Такая таблица не существует");
        return _tables[typeof(T)]!.Select(x => (T)x);
    }

    public void Serialize<T>(string path) where T : IEntity
    {
        if (!_tables.ContainsKey(typeof(T)))
            throw new DataBaseException("Такая таблица не существует");
        string jsonString = JsonSerializer.Serialize(_tables[typeof(T)]!.Select(x => (T)x));
        File.WriteAllText(path, jsonString);
    }

    public void Deserialize<T>(string path) where T : IEntity
    {
        string serializationData = File.ReadAllText(path);
        List<T>? table = JsonSerializer.Deserialize<List<T>>(serializationData);
        _tables[typeof(T)] = table is null ? new List<IEntity>() : table.Select(x => (IEntity)x).ToList();
    }
}