using System.Windows;

namespace InventoryApp;

public partial class SellDialog : Window
{
    private readonly int _availableStock;

    public int Count { get; private set; }

    public SellDialog(string itemName, int availableStock)
    {
        InitializeComponent();
        _availableStock = availableStock;
        ItemNameTextBlock.Text = itemName;
        StockInfoTextBlock.Text = $"Na skladě: {availableStock} ks";
        Loaded += (_, _) => CountTextBox.Focus();
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(CountTextBox.Text, out int count))
        {
            ErrorTextBlock.Text = "Zadej celé číslo.";
            return;
        }
        if (count <= 0)
        {
            ErrorTextBlock.Text = "Počet musí být kladný.";
            return;
        }
        if (count > _availableStock)
        {
            ErrorTextBlock.Text = $"Nemůžeš prodat víc než {_availableStock} ks.";
            return;
        }
        Count = count;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
