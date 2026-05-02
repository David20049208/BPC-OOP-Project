using System;
using System.Text.Json.Serialization;

namespace InventoryApp;

public class Item
{
    private string _name;
    private decimal _price;
    private int _quantity;
    private int _minStock;
    private readonly DateTime _createdAt;
    private DateTime _updatedAt;

    public Guid Id { get; } = Guid.NewGuid();

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

    public decimal Price
    {
        get => _price;
        set
        {
            if (value < 0)
                throw new ArgumentException("Cena nesmí být záporná.");
            _price = value;
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

    public Item(string name, decimal price, int minStock)
    {
        _createdAt = DateTime.UtcNow;

        Name = name;
        Price = price;
        Quantity = 0;
        MinStock = minStock;
    }

    [JsonConstructor]
    public Item(Guid id, string name, decimal price, int quantity, int minStock,
               DateTime createdAt, DateTime updatedAt)
    {
        Id = id;
        _createdAt = createdAt;
        _updatedAt = updatedAt;

        _name = name;
        _price = price;
        _quantity = quantity;
        _minStock = minStock;
    }
}
