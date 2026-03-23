class Program
{
    public static void Main(string[] args)
    {
        FileRepository repository = new FileRepository();
        InventoryService inventoryService = new InventoryService(repository);

        // Inventar se ulozi do ./bin/Debug/saves/

        Console.WriteLine("Nactene polozky ze souboru:");
        foreach (Item item in inventoryService.GetAll())
        {
            Console.Write(item.Name + ", id: ");
            Console.Write(item.Id + ", vytvoreno dne: ");
            Console.WriteLine(item.CreatedAt);
        }

        Item test = new Item("Balicek", new Random().Next(), 5);
        inventoryService.AddItem(test);

        Console.Write("\nNastavuji cenu prvni polozky s nazvem balicek: ");
        Console.WriteLine(inventoryService.GetByName("balicek")?.Price = 42);

        repository.Save(inventoryService.GetAll());
    }
}