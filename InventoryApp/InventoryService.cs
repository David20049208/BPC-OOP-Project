using System;

namespace InventoryApp;

public class InventoryService
{
    private readonly IFileRepository _repository;
    private List<Item> _items;

    public InventoryService(IFileRepository repository)
    {
        _repository = repository;
        _items = _repository.Load();
    }

    public int Count => _items.Count;

    public void AddItem(Item item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));
        if (_items.Any(i => i.Id == item.Id))
            throw new InvalidOperationException($"Zboží s ID {item.Id} již existuje.");
        if (_items.Any(i => i.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Zboží s názvem '{item.Name}' již existuje.");
        _items.Add(item);
    }

    public List<Item> GetAll()
    {
        return _items;
    }

    public Item? GetById(Guid id)
    {
        return _items.FirstOrDefault(i => i.Id == id);
    }

    public void UpdateItem(Guid id, decimal? price = null, int? quantity = null, int? minStock = null)
    {
        Item item = GetById(id) ?? throw new KeyNotFoundException($"Zboží {id} nenalezeno.");
        if (price.HasValue) item.Price = price.Value;
        if (quantity.HasValue) item.Quantity = quantity.Value;
        if (minStock.HasValue) item.MinStock = minStock.Value;
    }

    public bool RemoveItem(Guid id)
    {
        Item? item = GetById(id);
        if (item is null) return false;
        _items.Remove(item);
        return true;
    }

    public IEnumerable<Item> GetLowStock()
    {
        return _items.Where(i => i.Quantity < i.MinStock);
    }

    public IEnumerable<Item> Search(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return Enumerable.Empty<Item>();
        return _items.Where(i => i.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    public decimal TotalInventoryValue()
    {
        return _items.Sum(i => i.Price * i.Quantity);
    }

    public int TotalItemCount()
    {
        return _items.Sum(i => i.Quantity);
    }

    public int LowStockCount()
    {
        return _items.Count(i => i.Quantity < i.MinStock);
    }

    public void Save()
    {
        _repository.Save(_items);
    }
}
