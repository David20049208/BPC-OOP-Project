using System;
using System.Text.Json.Serialization;

namespace InventoryApp;

public class Item
{
    private string _name;
    private decimal _purchasePrice;
    private decimal _sellingPrice;
    private decimal _vat;
    private int _quantity;
    private int _soldCount;
    private int _minStock;
    private readonly DateTime _createdAt;
    private DateTime _updatedAt;

    public Guid Id { get; }

    public string Name
    {
        get => _name;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Název nesmí být prázdný.");
            _name = value;
            _updatedAt = DateTime.UtcNow;
        }
    }

    public decimal PurchasePrice
    {
        get => _purchasePrice;
        set
        {
            if (value < 0)
                throw new ArgumentException("Nákupní cena nesmí být záporná.");
            _purchasePrice = value;
            _updatedAt = DateTime.UtcNow;
        }
    }

    public decimal SellingPrice
    {
        get => _sellingPrice;
        set
        {
            if (value < 0)
                throw new ArgumentException("Prodejní cena nesmí být záporná.");
            _sellingPrice = value;
            _updatedAt = DateTime.UtcNow;
        }
    }

    public decimal Vat
    {
        get => _vat;
        set
        {
            if (value < 0)
                throw new ArgumentException("DPH nesmí být záporné.");
            _vat = value;
            _updatedAt = DateTime.UtcNow;
        }
    }

    public int Quantity
    {
        get => _quantity;
        set
        {
            if (value < 0)
                throw new ArgumentException("Množství nesmí být záporné.");
            _quantity = value;
            _updatedAt = DateTime.UtcNow;
        }
    }

    public int SoldCount
    {
        get => _soldCount;
        set
        {
            if (value < 0)
                throw new ArgumentException("Počet prodaných nesmí být záporný.");
            _soldCount = value;
            _updatedAt = DateTime.UtcNow;
        }
    }

    public int MinStock
    {
        get => _minStock;
        set
        {
            if (value < 0)
                throw new ArgumentException("Minimum nesmí být záporné.");
            _minStock = value;
            _updatedAt = DateTime.UtcNow;
        }
    }

    public DateTime CreatedAt => _createdAt.ToLocalTime();

    public DateTime UpdatedAt => _updatedAt.ToLocalTime();

    [JsonIgnore]
    public bool IsLowStock => _quantity < _minStock;

    [JsonIgnore]
    public decimal SellingPriceWithVat => _sellingPrice * (1 + _vat / 100m);

    public Item(string name, decimal purchasePrice, decimal sellingPrice, decimal vat, int minStock)
    {
        Id = Guid.NewGuid();
        _createdAt = DateTime.UtcNow;

        Name = name;
        PurchasePrice = purchasePrice;
        SellingPrice = sellingPrice;
        Vat = vat;
        Quantity = 0;
        SoldCount = 0;
        MinStock = minStock;
    }

    [JsonConstructor]
    public Item(Guid id, string name, decimal purchasePrice, decimal sellingPrice, decimal vat,
                int quantity, int soldCount, int minStock,
                DateTime createdAt, DateTime updatedAt)
    {
        Id = id;
        _createdAt = createdAt;
        _updatedAt = updatedAt;

        _name = name;
        _purchasePrice = purchasePrice;
        _sellingPrice = sellingPrice;
        _vat = vat;
        _quantity = quantity;
        _soldCount = soldCount;
        _minStock = minStock;
    }
}
