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
        ArgumentNullException.ThrowIfNull(item);
        if (_items.Any(i => i.Id == item.Id))
            throw new InvalidOperationException($"Zboží s ID {item.Id} již existuje.");
        if (_items.Any(i => i.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Zboží s názvem '{item.Name}' již existuje.");
        _items.Add(item);
    }

    public IReadOnlyList<Item> GetAll()
    {
        return _items;
    }

    public Item? GetById(Guid id)
    {
        return _items.FirstOrDefault(i => i.Id == id);
    }

    public void UpdateItem(Guid id, decimal? purchasePrice = null, decimal? sellingPrice = null,
                           decimal? vat = null,
                           int? quantity = null, int? soldCount = null, int? minStock = null)
    {
        Item item = GetById(id) ?? throw new KeyNotFoundException($"Zboží {id} nenalezeno.");
        if (purchasePrice.HasValue) item.PurchasePrice = purchasePrice.Value;
        if (sellingPrice.HasValue) item.SellingPrice = sellingPrice.Value;
        if (vat.HasValue) item.Vat = vat.Value;
        if (quantity.HasValue) item.Quantity = quantity.Value;
        if (soldCount.HasValue) item.SoldCount = soldCount.Value;
        if (minStock.HasValue) item.MinStock = minStock.Value;
    }

    public bool RemoveItem(Guid id)
    {
        Item? item = GetById(id);
        if (item is null) return false;
        _items.Remove(item);
        return true;
    }

    public void SellItem(Guid id, int count)
    {
        Item item = GetById(id) ?? throw new KeyNotFoundException($"Zboží {id} nenalezeno.");
        if (count <= 0)
            throw new ArgumentException("Počet k prodeji musí být kladný.");
        if (count > item.Quantity)
            throw new InvalidOperationException($"Nedostatek na skladě (k dispozici: {item.Quantity}).");
        item.Quantity -= count;
        item.SoldCount += count;
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

    public decimal TotalPurchaseValue()
    {
        return _items.Sum(i => i.PurchasePrice * (i.Quantity + i.SoldCount));
    }

    public decimal TotalRevenue()
    {
        return _items.Sum(i => i.SellingPrice * i.SoldCount);
    }

    public decimal TotalRevenueWithVat()
    {
        return _items.Sum(i => i.SellingPriceWithVat * i.SoldCount);
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
