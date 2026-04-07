using System;

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
            throw new InvalidOperationException($"Item with id {item.Id} already exists.");
        if (_items.Any(i => i.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Item with name '{item.Name}' already exists.");
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

    public Item? GetByName(string name)
    {
        return _items.FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public bool Exists(Guid id)
    {
        return _items.Any(i => i.Id == id);
    }

    public void UpdateItem(Guid id, decimal? price = null, int? quantity = null, int? minStock = null)
    {
        Item item = GetById(id) ?? throw new KeyNotFoundException($"Item {id} not found.");
        if (price.HasValue) item.Price = price.Value;
        if (quantity.HasValue) item.Quantity = quantity.Value;
        if (minStock.HasValue) item.MinStock = minStock.Value;
    }

    public void IncreaseStock(Guid id, int amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive.", nameof(amount));
        Item item = GetById(id) ?? throw new KeyNotFoundException($"Item {id} not found.");
        item.Quantity += amount;
    }

    public void DecreaseStock(Guid id, int amount)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive.", nameof(amount));
        Item item = GetById(id) ?? throw new KeyNotFoundException($"Item {id} not found.");
        if (item.Quantity < amount)
            throw new InvalidOperationException($"Not enough stock for '{item.Name}' (have {item.Quantity}, need {amount}).");
        item.Quantity -= amount;
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

    public IEnumerable<Item> SortBy(string property, bool ascending = true)
    {
        IEnumerable<Item> sorted = property.ToLowerInvariant() switch
        {
            "name" => _items.OrderBy(i => i.Name),
            "price" => _items.OrderBy(i => i.Price),
            "quantity" => _items.OrderBy(i => i.Quantity),
            "minstock" => _items.OrderBy(i => i.MinStock),
            "createdat" => _items.OrderBy(i => i.CreatedAt),
            "updatedat" => _items.OrderBy(i => i.UpdatedAt),
            _ => throw new ArgumentException($"Unknown property '{property}'.", nameof(property))
        };
        return ascending ? sorted : sorted.Reverse();
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
