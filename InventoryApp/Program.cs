class Program
{
    public static void Main(string[] args)
    {
        InventoryService inventoryService = new InventoryService(new FileRepository());

        // Inventar se ulozi do ./bin/Debug/saves/

        Console.WriteLine("Nactene polozky ze souboru:");
        foreach (Item item in inventoryService.GetAll())
        {
            Console.Write(item.Name + ", id: ");
            Console.Write(item.Id + ", vytvoreno dne: ");
            Console.WriteLine(item.CreatedAt);
        }

        Item test = new Item($"Balicek_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}", new Random().Next(), 5);

        inventoryService.AddItem(test);
        inventoryService.Save();
    }
}