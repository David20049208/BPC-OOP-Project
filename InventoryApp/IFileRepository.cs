
namespace InventoryApp;

public interface IFileRepository
{

    public const string SAVE_FILE_NAME = "inventory.json";
    public const string SAVE_DIRECTORY_NAME = "saves";

    List<Item> Load();
    void Save(List<Item> items);
}
