using System.Text.Json;
using System.IO;

namespace InventoryApp;

public class FileRepository : IFileRepository
{
    private readonly string _filePath;
    private readonly string _directory;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public FileRepository()
    {
        _directory = Path.Combine(Directory.GetCurrentDirectory(), IFileRepository.SAVE_DIRECTORY_NAME);
        Directory.CreateDirectory(_directory);
        _filePath = Path.Combine(_directory, IFileRepository.SAVE_FILE_NAME);

        _jsonSerializerOptions = new JsonSerializerOptions{WriteIndented = true};
    }

    public List<Item> Load()
    {
        if (!File.Exists(_filePath))
            return new List<Item>();

        string json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<List<Item>>(json) ?? new List<Item>();
    }

    public void Save(List<Item> items)
    {
        string json = JsonSerializer.Serialize(items, _jsonSerializerOptions);

        File.WriteAllText(_filePath, json);
    }
}
