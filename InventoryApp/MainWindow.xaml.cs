using System;
using System.Collections.Generic;
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
        InitializeComponent();
        _inventoryService = new InventoryService(new FileRepository());
        ShowItems(_inventoryService.GetAll());
        UpdateStats();
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (NameTextBox.Foreground == Brushes.Gray || string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                SetStatus("Zadej název.");
                return;
            }

            string name = NameTextBox.Text;

            if (!decimal.TryParse(PriceTextBox.Text, out decimal price))
            {
                SetStatus("Cena musí být číslo.");
                return;
            }

            if (!int.TryParse(QuantityTextBox.Text, out int quantity))
            {
                SetStatus("Množství musí být celé číslo.");
                return;
            }

            if (!int.TryParse(MinStockTextBox.Text, out int minStock))
            {
                SetStatus("Minimum musí být celé číslo.");
                return;
            }

            Item item = new Item(name, price, minStock)
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

    private void UpdateButton_Click(object sender, RoutedEventArgs e)
    {
        if (_itemBeingEdited == null)
        {
            if (ItemsDataGrid.SelectedItem is not Item selectedItem)
            {
                SetStatus("Vyber položku k úpravě.");
                return;
            }

            _itemBeingEdited = selectedItem;

            NameTextBox.Text = selectedItem.Name;
            PriceTextBox.Text = selectedItem.Price.ToString();
            QuantityTextBox.Text = selectedItem.Quantity.ToString();
            MinStockTextBox.Text = selectedItem.MinStock.ToString();

            NameTextBox.IsEnabled = false;

            NameTextBox.Foreground = Brushes.Black;
            PriceTextBox.Foreground = Brushes.Black;
            QuantityTextBox.Foreground = Brushes.Black;
            MinStockTextBox.Foreground = Brushes.Black;

            SetStatus("Uprav hodnoty a znovu klikni na Upravit vybranou.");
            return;
        }

        try
        {
            decimal price = decimal.Parse(PriceTextBox.Text);
            int quantity = int.Parse(QuantityTextBox.Text);
            int minStock = int.Parse(MinStockTextBox.Text);

            _inventoryService.UpdateItem(_itemBeingEdited.Id, price, quantity, minStock);
            _inventoryService.Save();

            ShowItems(_inventoryService.GetAll());
            UpdateStats();

            _itemBeingEdited = null;
            ItemsDataGrid.SelectedItem = null;

            ResetInputs();
            NameTextBox.IsEnabled = true; 

            SetStatus("Položka byla upravena.");
        }
        catch (Exception ex)
        {
            SetStatus(ex.Message);
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (ItemsDataGrid.SelectedItem is not Item selectedItem)
        {
            SetStatus("Vyber položku ke smazání.");
            return;
        }

        _inventoryService.RemoveItem(selectedItem.Id);
        _inventoryService.Save();

        ShowItems(_inventoryService.GetAll());
        UpdateStats();
        SetStatus("Položka byla smazána.");
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        ShowItems(_inventoryService.Search(SearchTextBox.Text));
        UpdateStats();
        SetStatus("Výsledky vyhledávání.");
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
        PriceTextBox.Text = "";
        QuantityTextBox.Text = "";
        MinStockTextBox.Text = "";

        Placeholder_LostFocus(NameTextBox, new RoutedEventArgs());
        Placeholder_LostFocus(PriceTextBox, new RoutedEventArgs());
        Placeholder_LostFocus(QuantityTextBox, new RoutedEventArgs());
        Placeholder_LostFocus(MinStockTextBox, new RoutedEventArgs());

        Keyboard.ClearFocus();
    }

    private void SetStatus(string message)
    {
        StatusTextBlock.Text = message;
    }

    private void Placeholder_GotFocus(object sender, RoutedEventArgs e)
    {
        TextBox box = (TextBox)sender;

        if (box.Foreground == Brushes.Gray)
        {
            box.Text = "";
            box.Foreground = Brushes.Black;
        }
    }

    private void Placeholder_LostFocus(object sender, RoutedEventArgs e)
    {
        TextBox box = (TextBox)sender;

        if (string.IsNullOrWhiteSpace(box.Text))
        {
            box.Text = box.Tag?.ToString();
            box.Foreground = Brushes.Gray;
        }
    }
    private void UpdateStats()
    {
        TotalValueTextBlock.Text =
            $"Celkem: {_inventoryService.TotalInventoryValue():N2} Kč";

        TotalItemCountTextBlock.Text =
            $"Kusů celkem: {_inventoryService.TotalItemCount()}";

        LowStockCountTextBlock.Text =
            $"Pod minimem: {_inventoryService.LowStockCount()}";
    }
}