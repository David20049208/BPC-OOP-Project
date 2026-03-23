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

    public void AddItem(Item item)
    {
        _items.Add(item);
    }

    public List<Item> GetAll()
    {
        return _items;
    }

    public Item? GetByName(string name)
    {
        return _items.FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}
