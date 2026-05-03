using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace InventoryApp;

public partial class MainWindow : Window
{
    private readonly InventoryService _inventoryService;
    private Item? _itemBeingEdited;
    public MainWindow()
    {
        _inventoryService = new InventoryService(new FileRepository());
        InitializeComponent();
        ShowItems(_inventoryService.GetAll());
        UpdateStats();
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!TryReadInputs(out string name,
                               out decimal purchasePrice, out decimal sellingPrice,
                               out decimal vat,
                               out int quantity, out int minStock))
            {
                return;
            }

            Item item = new(name, purchasePrice, sellingPrice, vat, minStock)
            {
                Quantity = quantity
            };

            _inventoryService.AddItem(item);
            _inventoryService.Save();

            ShowItems(_inventoryService.GetAll());
            UpdateStats();
            ResetInputs();
            SetStatus("Položka byla přidána.");
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message);
        }
    }

    private bool TryReadInputs(out string name,
                               out decimal purchasePrice, out decimal sellingPrice,
                               out decimal vat,
                               out int quantity, out int minStock)
    {
        name = NameTextBox.Text;
        purchasePrice = 0m;
        sellingPrice = 0m;
        vat = 0m;
        quantity = 0;
        minStock = 0;

        if (string.IsNullOrWhiteSpace(name))
        {
            SetStatus("Zadej název.", warning: true);
            return false;
        }

        if (!TryParseDecimal(PurchasePriceTextBox.Text, out purchasePrice))
        {
            SetStatus("Cena nákupu musí být číslo.", warning: true);
            return false;
        }

        if (!TryParseDecimal(SellingPriceTextBox.Text, out sellingPrice))
        {
            SetStatus("Cena prodeje musí být číslo.", warning: true);
            return false;
        }

        if (purchasePrice > sellingPrice)
        {
            SetStatus("Cena nákupu nemůže být vyšší než cena prodeje.", warning: true);
            return false;
        }

        if (!TryParseDecimal(VatTextBox.Text, out vat))
        {
            SetStatus("DPH musí být číslo (např. 0 nebo 21).", warning: true);
            return false;
        }

        if (!int.TryParse(QuantityTextBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out quantity))
        {
            SetStatus("Počet ks na skladě musí být celé číslo.", warning: true);
            return false;
        }

        if (!int.TryParse(MinStockTextBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out minStock))
        {
            SetStatus("Minimum ks musí být celé číslo.", warning: true);
            return false;
        }

        return true;
    }

    private static bool TryParseDecimal(string text, out decimal value)
    {
        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out value))
            return true;
        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }

    private void UpdateButton_Click(object sender, RoutedEventArgs e)
    {
        if (_itemBeingEdited == null)
        {
            if (ItemsDataGrid.SelectedItem is not Item selectedItem)
            {
                SetStatus("Vyber položku k úpravě.", warning: true);
                return;
            }

            _itemBeingEdited = selectedItem;

            NameTextBox.Text = selectedItem.Name;
            PurchasePriceTextBox.Text = selectedItem.PurchasePrice.ToString(CultureInfo.CurrentCulture);
            SellingPriceTextBox.Text = selectedItem.SellingPrice.ToString(CultureInfo.CurrentCulture);
            VatTextBox.Text = selectedItem.Vat.ToString(CultureInfo.CurrentCulture);
            QuantityTextBox.Text = selectedItem.Quantity.ToString(CultureInfo.CurrentCulture);
            MinStockTextBox.Text = selectedItem.MinStock.ToString(CultureInfo.CurrentCulture);

            SetEditMode(true);

            SetStatus("Uprav hodnoty a znovu klikni na Potvrdit úpravu.");
            return;
        }

        try
        {
            if (!TryReadInputs(out _,
                               out decimal purchasePrice, out decimal sellingPrice,
                               out decimal vat,
                               out int quantity, out int minStock))
            {
                return;
            }

            _inventoryService.UpdateItem(_itemBeingEdited.Id,
                                         purchasePrice: purchasePrice,
                                         sellingPrice: sellingPrice,
                                         vat: vat,
                                         quantity: quantity,
                                         minStock: minStock);
            _inventoryService.Save();

            ShowItems(_inventoryService.GetAll());
            UpdateStats();

            _itemBeingEdited = null;
            ItemsDataGrid.SelectedItem = null;

            ResetInputs();
            SetEditMode(false);

            SetStatus("Položka byla upravena.");
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message);
        }
    }

    private void SetEditMode(bool editing)
    {
        AddButton.IsEnabled = !editing;
        ShowAllButton.IsEnabled = !editing;
        LowStockButton.IsEnabled = !editing;
        DeleteButton.IsEnabled = !editing;
        SellButton.IsEnabled = !editing;

        NameTextBox.IsEnabled = !editing;
        SearchTextBox.IsEnabled = !editing;
        ItemsDataGrid.IsEnabled = !editing;

        UpdateButton.Content = editing ? "Potvrdit úpravu" : "Upravit vybranou";
        CancelEditButton.Visibility = editing ? Visibility.Visible : Visibility.Collapsed;

        if (editing && _itemBeingEdited != null)
        {
            SectionTitleTextBlock.Text = $"Upravit položku: {_itemBeingEdited.Name}";
            SectionTitleTextBlock.Foreground = (Brush)FindResource("Primary");
        }
        else
        {
            SectionTitleTextBlock.Text = "Přidat položku";
            SectionTitleTextBlock.Foreground = (Brush)FindResource("Muted");
        }
    }

    private void CancelEditButton_Click(object sender, RoutedEventArgs e)
    {
        _itemBeingEdited = null;
        ItemsDataGrid.SelectedItem = null;

        ResetInputs();
        SetEditMode(false);

        SetStatus("Editace zrušena.");
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (_itemBeingEdited != null)
            {
                UpdateButton_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (Keyboard.FocusedElement is TextBox box
                     && (box == NameTextBox || box == PurchasePriceTextBox
                         || box == SellingPriceTextBox || box == VatTextBox
                         || box == QuantityTextBox || box == MinStockTextBox))
            {
                AddButton_Click(this, new RoutedEventArgs());
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Escape && _itemBeingEdited != null)
        {
            CancelEditButton_Click(this, new RoutedEventArgs());
            e.Handled = true;
        }
        else if (e.Key == Key.Delete
                 && _itemBeingEdited == null
                 && Keyboard.FocusedElement is not TextBox)
        {
            DeleteButton_Click(this, new RoutedEventArgs());
            e.Handled = true;
        }
    }

    private void SellButton_Click(object sender, RoutedEventArgs e)
    {
        if (ItemsDataGrid.SelectedItem is not Item selectedItem)
        {
            SetStatus("Vyber položku k prodeji.", warning: true);
            return;
        }

        if (selectedItem.Quantity <= 0)
        {
            SetStatus($"Položka \"{selectedItem.Name}\" nemá žádný kus na skladě.", warning: true);
            return;
        }

        SellDialog dialog = new(selectedItem.Name, selectedItem.Quantity)
        {
            Owner = this
        };

        if (dialog.ShowDialog() != true)
        {
            SetStatus("Prodej zrušen.");
            return;
        }

        try
        {
            _inventoryService.SellItem(selectedItem.Id, dialog.Count);
            _inventoryService.Save();

            ShowItems(_inventoryService.GetAll());
            UpdateStats();
            SetStatus($"Prodáno {dialog.Count} ks položky \"{selectedItem.Name}\".");
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message, warning: true);
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (ItemsDataGrid.SelectedItem is not Item selectedItem)
        {
            SetStatus("Vyber položku ke smazání.", warning: true);
            return;
        }

        MessageBoxResult result = MessageBox.Show(
            $"Opravdu chceš smazat položku \"{selectedItem.Name}\"?",
            "Potvrzení smazání",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);

        if (result != MessageBoxResult.Yes)
        {
            SetStatus("Smazání zrušeno.");
            return;
        }

        _inventoryService.RemoveItem(selectedItem.Id);
        _inventoryService.Save();

        ItemsDataGrid.SelectedItem = null;
        ShowItems(_inventoryService.GetAll());
        UpdateStats();
        SetStatus("Položka byla smazána.");
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (ItemsDataGrid == null) return;

        if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
        {
            ShowItems(_inventoryService.GetAll());
        }
        else
        {
            ShowItems(_inventoryService.Search(SearchTextBox.Text));
        }
        UpdateStats();
    }

    private void ShowAllButton_Click(object sender, RoutedEventArgs e)
    {
        ShowItems(_inventoryService.GetAll());
        UpdateStats();
        SetStatus("Zobrazeny všechny položky.");
    }

    private void LowStockButton_Click(object sender, RoutedEventArgs e)
    {
        ShowItems(_inventoryService.GetLowStock());
        UpdateStats();
        SetStatus("Zobrazeny položky pod minimálním stavem.");
    }

    private void ShowItems(IEnumerable<Item> items)
    {
        ItemsDataGrid.ItemsSource = items.ToList();
    }

    private void ResetInputs()
    {
        NameTextBox.Text = "";
        PurchasePriceTextBox.Text = "";
        SellingPriceTextBox.Text = "";
        VatTextBox.Text = "";
        QuantityTextBox.Text = "";
        MinStockTextBox.Text = "";

        Keyboard.ClearFocus();
    }

    private void SetStatus(string message, bool warning = false)
    {
        StatusTextBlock.Text = message;
        StatusTextBlock.Foreground = (Brush)FindResource(warning ? "Danger" : "Muted");
    }

    private static string CzechItemPlural(int count)
    {
        if (count == 1) return "položka";
        if (count >= 2 && count <= 4) return "položky";
        return "položek";
    }

    private void UpdateStats()
    {
        TotalPurchaseValueTextBlock.Text =
            $"{_inventoryService.TotalPurchaseValue():N2} Kč";

        TotalRevenueTextBlock.Text =
            $"{_inventoryService.TotalRevenue():N2} Kč";

        TotalRevenueWithVatTextBlock.Text =
            $"{_inventoryService.TotalRevenueWithVat():N2} Kč";

        TotalItemCountTextBlock.Text =
            $"{_inventoryService.TotalItemCount()} ks";

        int lowStockCount = _inventoryService.LowStockCount();
        LowStockCountTextBlock.Text = $"{lowStockCount} {CzechItemPlural(lowStockCount)}";

        Brush lowStockBrush = (Brush)FindResource(lowStockCount > 0 ? "Danger" : "Muted");
        FontWeight lowStockWeight = lowStockCount > 0 ? FontWeights.Bold : FontWeights.Normal;
        LowStockLabelTextBlock.Foreground = lowStockBrush;
        LowStockCountTextBlock.Foreground = lowStockBrush;
        LowStockLabelTextBlock.FontWeight = lowStockWeight;
        LowStockCountTextBlock.FontWeight = lowStockWeight;
    }
}