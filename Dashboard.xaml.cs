using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using System.Data.SqlClient;
using System.Configuration;
using System.Windows.Markup;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Inventory_Management_System_SFML
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// 
    /// </summary>

    public partial class Dashboard : Window
    {

        private SqlConnection GetConnection()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings["InventorySystem"].ConnectionString);
        }

        private int totalProducts;
        private decimal totalSales;
        private int lowStockAlerts;

        public int TotalProducts
        {
            get => totalProducts;
            set
            {
                totalProducts = value;
                OnPropertyChanged(nameof(TotalProducts));
            }
        }

        public decimal TotalSales
        {
            get => totalSales;
            set
            {
                totalSales = value;
                OnPropertyChanged(nameof(TotalSales));
            }
        }

        public int LowStockAlerts
        {
            get => lowStockAlerts;
            set
            {
                lowStockAlerts = value;
                OnPropertyChanged(nameof(LowStockAlerts));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RefreshSalesData()
        {
            // Example logic to set the values
            TotalProducts = Products.Count; // Assuming Products is your ObservableCollection<Product>
            TotalSales = salesOrders.Sum(order => order.TotalAmount); // Assuming salesOrders is your List<SalesOrder>
            LowStockAlerts = products.Count(p => p.Quantity < 10); // Example condition for low stock
        }

        public salesOrderViewModel ViewModel { get; set; }

        private bool isSidebarVisible = true;
        public ObservableCollection<StockMovement> StockMovements { get; set; } = new ObservableCollection<StockMovement>();
        private List<Product> products = new List<Product>();
        private List<StockMovement> stockMovements = new List<StockMovement>(); // List to hold stock movements
        private List<PurchaseOrder> purchaseOrders = new List<PurchaseOrder>(); // List to hold purchase orders
        private List<SalesOrder> salesOrders = new List<SalesOrder>(); // List to hold sales orders
        private List<Supplier> suppliers = new List<Supplier>(); // List to hold suppliers
        private List<Customer> customers = new List<Customer>(); // List to hold customers
        private List<Report> reports = new List<Report>(); // List to hold reports
        private List<User> users = new List<User>(); // List to hold users
        private List<IntegrationLog> integrationLogs = new List<IntegrationLog>(); // List to hold integration logs
        private List<Notification> notifications = new List<Notification>(); // List to hold notifications
        private List<AuditLog> auditLogs = new List<AuditLog>(); // List to hold audit logs
        public ObservableCollection<Product> Products { get; set; } = new ObservableCollection<Product>();
        private List<string> userActivityLogs = new List<string>(); // List to hold user activity logs
        private int selectedCustomerIndex = -1; // To track the selected customer for editing
        private int selectedProductIndex = -1; // To track the selected product for editing
        private int selectedSupplierIndex = -1;

        public Dashboard()
        {

            InitializeComponent();
            RefreshProductList();
            RefreshUserList();
            ViewModel = new salesOrderViewModel(); // Create an instance of the ViewModel
            this.DataContext = this; // Set the DataContext for the entire window

            // Initialize and populate salesOrders for testing
            salesOrders = new List<SalesOrder>
            {
                new SalesOrder { SalesOrderID = 1, CustomerName = "John Doe", OrderDate = DateTime.Now, Status = "Pending", TotalAmount = 100.00m },
                new SalesOrder { SalesOrderID = 2, CustomerName = "Jane Smith", OrderDate = DateTime.Now.AddDays(-1), Status = "Completed", TotalAmount = 150.00m }
            };

            RefreshSalesOrderList(); // Call this method to populate the ListView
            RefreshAuditLogsList();
            RefreshSupplierList();
            RefreshOrderList();
            RefreshSalesData();
            RefreshStockMovement();
            RefreshReportsList();
            RefreshIntegrationLogsList();
        }

        private void ToggleSidebarButton_Click(object sender, RoutedEventArgs e)
        {
            if (isSidebarVisible)
            {
                // Animate sidebar out
                ThicknessAnimation slideOutAnimation = new ThicknessAnimation
                {
                    To = new Thickness(-200, 0, 0, 0), // Move out of view
                    Duration = TimeSpan.FromMilliseconds(300)
                };
                Sidebar.BeginAnimation(FrameworkElement.MarginProperty, slideOutAnimation);
                isSidebarVisible = false;
            }
            else
            {
                // Animate sidebar in
                ThicknessAnimation slideInAnimation = new ThicknessAnimation
                {
                    To = new Thickness(0, 0, 0, 0), // Move back into view
                    Duration = TimeSpan.FromMilliseconds(300)
                };
                Sidebar.BeginAnimation(FrameworkElement.MarginProperty, slideInAnimation);
                isSidebarVisible = true;
            }
        }

        private void OverviewButton_Click(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 0; // Overview tab
        }

        private void ProductManagementButton_Click(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 1; // Product Management tab
        }

        private void StockMovementButton_Click(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 2;
        }

        private void PurchaseOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 3; // Purchase Orders tab
        }

        private void SalesOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 4; // Sales Orders tab
        }

        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 5; // Reports tab
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to log out?", "Logout Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                this.Hide();
                MainWindow loginWindow = new MainWindow(); // Replace with your actual login window class
                loginWindow.Show();
            }
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ensure ProductID is provided by the user
                if (int.TryParse(txtProductID.Text, out var productId))
                {
                    using (var connection = GetConnection())
                    {
                        connection.Open();
                        var command = new SqlCommand("INSERT INTO Products (ProductID, Name, SKU, Barcode, Category, Quantity, UnitPrice, CreatedAt) VALUES (@ProductID, @Name, @SKU, @Barcode, @Category, @Quantity, @UnitPrice, @CreatedAt)", connection);
                        command.Parameters.AddWithValue("@ProductID", productId); // Add ProductID
                        command.Parameters.AddWithValue("@Name", txtProductName.Text);
                        command.Parameters.AddWithValue("@SKU", txtSKU.Text);
                        command.Parameters.AddWithValue("@Barcode", txtBarcode.Text);
                        command.Parameters.AddWithValue("@Category", (cmbCategory.SelectedItem as ComboBoxItem)?.Content.ToString());
                        command.Parameters.AddWithValue("@Quantity", int.TryParse(txtQuantity.Text, out var quantity) ? quantity : 0);
                        command.Parameters.AddWithValue("@UnitPrice", decimal.TryParse(txtUnitPrice.Text, out var price) ? price : 0);
                        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                        command.ExecuteNonQuery();
                    }
                    RefreshProductList(); // Refresh the list after adding
                    ClearProductFields();
                }
                else
                {
                    MessageBox.Show("Please enter a valid Product ID.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Database error: {sqlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProductIndex >= 0)
            {
                try
                {
                    using (var connection = GetConnection())
                    {
                        connection.Open();
                        var command = new SqlCommand("UPDATE Products SET Name = @Name, SKU = @SKU, Barcode = @Barcode, Category = @Category, Quantity = @Quantity, UnitPrice = @UnitPrice, UpdatedAt = @UpdatedAt WHERE ProductID = @ProductID", connection);
                        command.Parameters.AddWithValue("@ProductID", products[selectedProductIndex].ProductID);
                        command.Parameters.AddWithValue("@Name", txtProductName.Text);
                        command.Parameters.AddWithValue("@SKU", txtSKU.Text);
                        command.Parameters.AddWithValue("@Barcode", txtBarcode.Text);
                        command.Parameters.AddWithValue("@Category", (cmbCategory.SelectedItem as ComboBoxItem)?.Content.ToString());
                        command.Parameters.AddWithValue("@Quantity", int.TryParse(txtQuantity.Text, out var quantity) ? quantity : 0);
                        command.Parameters.AddWithValue("@UnitPrice", decimal.TryParse(txtUnitPrice.Text, out var price) ? price : 0);
                        command.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                        command.ExecuteNonQuery();
                    }
                    RefreshProductList(); // Refresh the list after editing
                    ClearProductFields();
                    selectedProductIndex = -1; // Reset selection
                }
                catch (SqlException sqlEx)
                {
                    MessageBox.Show($"Database error: {sqlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a product to edit.", "Edit Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProductIndex >= 0)
            {
                try
                {
                    using (var connection = GetConnection())
                    {
                        connection.Open();
                        var command = new SqlCommand("DELETE FROM Products WHERE ProductID = @ProductID", connection);
                        command.Parameters.AddWithValue("@ProductID", products[selectedProductIndex].ProductID);
                        command.ExecuteNonQuery();
                    }
                    RefreshProductList(); // Refresh the list after deletion
                    ClearProductFields();
                    selectedProductIndex = -1; // Reset selection
                }
                catch (SqlException sqlEx)
                {
                    MessageBox.Show($"Database error: {sqlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a product to delete.", "Delete Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RefreshProductList()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    var command = new SqlCommand("SELECT * FROM Products", connection);
                    var reader = command.ExecuteReader();

                    products.Clear(); // Clear the existing list

                    while (reader.Read())
                    {
                        var product = new Product
                        {
                            ProductID = reader.IsDBNull(0) ? 0 : reader.GetInt32(0), // Handle NULL for ProductID
                            Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1), // Handle NULL for Name
                            SKU = reader.IsDBNull(2) ? string.Empty : reader.GetString(2), // Handle NULL for SKU
                            Category = reader.IsDBNull(3) ? string.Empty : reader.GetString(3), // Handle NULL for Category
                            Quantity = reader.IsDBNull(4) ? 0 : reader.GetInt32(4), // Handle NULL for Quantity
                            UnitPrice = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5), // Handle NULL for UnitPrice
                            Barcode = reader.IsDBNull(6) ? string.Empty : reader.GetString(6), // Handle NULL for Barcode
                            CreatedAt = reader.IsDBNull(7) ? DateTime.MinValue : reader.GetDateTime(7), // Handle NULL
                            UpdatedAt = reader.IsDBNull(8) ? DateTime.MinValue : reader.GetDateTime(8)  // Handle NULL
                        };
                        products.Add(product);
                    }
                }
                lvProducts.ItemsSource = null; // Clear the current binding
                lvProducts.ItemsSource = products; // Rebind the updated product list
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Database error: {sqlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearProductFields()
        {
            txtProductName.Clear();
            txtSKU.Clear();
            txtBarcode.Clear();
            cmbCategory.SelectedIndex = -1;
            txtQuantity.Clear();
            txtUnitPrice.Clear();
        }

        private void lvProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvProducts.SelectedItem is Product selectedProduct)
            {
                selectedProductIndex = products.IndexOf(selectedProduct);
                txtProductName.Text = selectedProduct.Name;
                txtSKU.Text = selectedProduct.SKU;
                txtBarcode.Text = selectedProduct.Barcode;
                cmbCategory.SelectedItem = cmbCategory.Items.Cast<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == selectedProduct.Category);
                txtQuantity.Text = selectedProduct.Quantity.ToString();
                txtUnitPrice.Text = selectedProduct.UnitPrice.ToString("F2"); // Format for decimal
            }
            else
            {
                selectedProductIndex = -1; // Reset if no selection
            }
        }



        private void CreateOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ensure Purchase Order ID is provided by the user
                if (int.TryParse(txtPurchaseOrderID.Text, out var purchaseOrderId))
                {
                    // Ensure Supplier ID is provided
                    if (int.TryParse(txtSupplierID.Text, out var supplierId))
                    {
                        // Check if the Supplier ID exists
                        using (var connection = GetConnection())
                        {
                            connection.Open();
                            var checkSupplierCommand = new SqlCommand("SELECT COUNT(*) FROM Suppliers WHERE SupplierID = @SupplierID", connection);
                            checkSupplierCommand.Parameters.AddWithValue("@SupplierID", supplierId);
                            int supplierCount = (int)checkSupplierCommand.ExecuteScalar();

                            if (supplierCount == 0)
                            {
                                MessageBox.Show("The Supplier ID does not exist. Please enter a valid Supplier ID.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                        }

                        // Ensure Order Date is provided
                        if (dpOrderDate.SelectedDate.HasValue)
                        {
                            // Ensure Order Status is provided
                            if (cmbOrderStatus.SelectedItem != null)
                            {
                                // Ensure Total Amount is provided
                                if (decimal.TryParse(txtTotalAmount.Text, out var totalAmount))
                                {
                                    // Check if the Purchase Order ID already exists
                                    using (var connection = GetConnection())
                                    {
                                        connection.Open();
                                        var checkOrderCommand = new SqlCommand("SELECT COUNT(*) FROM PurchaseOrders WHERE PurchaseOrderID = @PurchaseOrderID", connection);
                                        checkOrderCommand.Parameters.AddWithValue("@PurchaseOrderID", purchaseOrderId);
                                        int orderCount = (int)checkOrderCommand.ExecuteScalar();

                                        if (orderCount > 0)
                                        {
                                            MessageBox.Show("The Purchase Order ID already exists. Please enter a unique Purchase Order ID.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                                            return;
                                        }
                                    }

                                    // Create a new purchase order instance
                                    var order = new PurchaseOrder
                                    {
                                        PurchaseOrderID = purchaseOrderId,
                                        SupplierID = supplierId,
                                        OrderDate = dpOrderDate.SelectedDate.Value,
                                        Status = (cmbOrderStatus.SelectedItem as ComboBoxItem)?.Content.ToString(),
                                        TotalAmount = totalAmount
                                    };

                                    // Insert the purchase order into the database
                                    using (var connection = GetConnection())
                                    {
                                        connection.Open();
                                        var command = new SqlCommand("INSERT INTO PurchaseOrders (PurchaseOrderID, SupplierID, OrderDate, Status, TotalAmount) VALUES (@PurchaseOrderID, @SupplierID, @OrderDate, @Status, @TotalAmount)", connection);
                                        command.Parameters.AddWithValue("@PurchaseOrderID", order.PurchaseOrderID);
                                        command.Parameters.AddWithValue("@SupplierID", order.SupplierID);
                                        command.Parameters.AddWithValue("@OrderDate", order.OrderDate);
                                        command.Parameters.AddWithValue("@Status", order.Status);
                                        command.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
                                        command.ExecuteNonQuery();
                                    }

                                    // Refresh the purchase orders list
                                    RefreshOrderList();
                                    ClearOrderFields();
                                }
                                else
                                {
                                    MessageBox.Show("Invalid Total Amount. Please enter a valid amount.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Please select an Order Status.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Please select an Order Date.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid Supplier ID. Please enter a valid ID.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Invalid Purchase Order ID. Please enter a valid ID.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void EditOrder_Click(object sender, RoutedEventArgs e)
        {
            if (lvPurchaseOrders.SelectedItem is PurchaseOrder selectedOrder)
            {
                try
                {
                    // Ensure Supplier ID is provided
                    if (int.TryParse(txtSupplierID.Text, out var supplierId))
                    {
                        // Ensure Order Date is provided
                        if (dpOrderDate.SelectedDate.HasValue)
                        {
                            // Ensure Order Status is provided
                            if (cmbOrderStatus.SelectedItem != null)
                            {
                                // Ensure Total Amount is provided
                                if (decimal.TryParse(txtTotalAmount.Text, out var totalAmount))
                                {
                                    // Update the selected order instance
                                    selectedOrder.SupplierID = supplierId;
                                    selectedOrder.OrderDate = dpOrderDate.SelectedDate.Value;
                                    selectedOrder.Status = (cmbOrderStatus.SelectedItem as ComboBoxItem)?.Content.ToString();
                                    selectedOrder.TotalAmount = totalAmount;

                                    // Update the purchase order in the database
                                    using (var connection = GetConnection())
                                    {
                                        connection.Open();
                                        var command = new SqlCommand("UPDATE PurchaseOrders SET SupplierID = @SupplierID, OrderDate = @OrderDate, Status = @Status, TotalAmount = @TotalAmount WHERE PurchaseOrderID = @PurchaseOrderID", connection);
                                        command.Parameters.AddWithValue("@PurchaseOrderID", selectedOrder.PurchaseOrderID);
                                        command.Parameters.AddWithValue("@SupplierID", selectedOrder.SupplierID);
                                        command.Parameters.AddWithValue("@OrderDate", selectedOrder.OrderDate);
                                        command.Parameters.AddWithValue("@Status", selectedOrder.Status);
                                        command.Parameters.AddWithValue("@TotalAmount", selectedOrder.TotalAmount);
                                        command.ExecuteNonQuery();
                                    }

                                    // Refresh the purchase orders list
                                    RefreshOrderList();
                                    ClearOrderFields();
                                }
                                else
                                {
                                    MessageBox.Show("Invalid Total Amount. Please enter a valid amount.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Please select an Order Status.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Please select an Order Date.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid Supplier ID. Please enter a valid ID.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (SqlException sqlEx)
                {
                    MessageBox.Show($"Database error: {sqlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a purchase order to edit.", "Edit Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (lvPurchaseOrders.SelectedItem is PurchaseOrder selectedOrder)
            {
                try
                {
                    // Delete the purchase order from the database
                    using (var connection = GetConnection())
                    {
                        connection.Open();
                        var command = new SqlCommand("DELETE FROM PurchaseOrders WHERE PurchaseOrderID = @PurchaseOrderID", connection);
                        command.Parameters.AddWithValue("@PurchaseOrderID", selectedOrder.PurchaseOrderID);
                        command.ExecuteNonQuery();
                    }

                    // Refresh the purchase orders list
                    RefreshOrderList();
                    ClearOrderFields();
                }
                catch (SqlException sqlEx)
                {
                    MessageBox.Show($"Database error: {sqlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a purchase order to delete.", "Delete Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void ViewOrders_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RefreshOrderList(); // Refresh the list to show current orders
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while viewing orders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshOrderList()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    var command = new SqlCommand("SELECT PurchaseOrderID, SupplierID, OrderDate, Status, TotalAmount FROM PurchaseOrders", connection);
                    var reader = command.ExecuteReader();

                    purchaseOrders.Clear(); // Clear the existing list

                    while (reader.Read())
                    {
                        var order = new PurchaseOrder
                        {
                            PurchaseOrderID = reader.IsDBNull(0) ? 0 : reader.GetInt32(0), // Handle NULL for PurchaseOrderID
                            SupplierID = reader.IsDBNull(1) ? 0 : reader.GetInt32(1), // Handle NULL for SupplierID
                            OrderDate = reader.IsDBNull(2) ? DateTime.MinValue : reader.GetDateTime(2), // Handle NULL for OrderDate
                            Status = reader.IsDBNull(3) ? string.Empty : reader.GetString(3), // Handle NULL for Status
                            TotalAmount = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4) // Handle NULL for TotalAmount
                        };
                        purchaseOrders.Add(order);
                    }
                }
                lvPurchaseOrders.ItemsSource = null; // Clear the current binding
                lvPurchaseOrders.ItemsSource = purchaseOrders; // Rebind the updated order list
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Database error: {sqlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearOrderFields()
        {
            txtPurchaseOrderID.Clear();
            txtSupplierID.Clear();
            dpOrderDate.SelectedDate = null;
            cmbOrderStatus.SelectedIndex = -1;
            txtTotalAmount.Clear();
        }

        private void CreateSalesOrder_Click(object sender, RoutedEventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(txtNewCustomerName.Text))
            {
                MessageBox.Show("Please enter a customer name.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dpOrderDate.SelectedDate == null)
            {
                MessageBox.Show("Please select an order date.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbNewSalesOrderStatus.SelectedItem == null)
            {
                MessageBox.Show("Please select an order status.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtTotalAmount.Text, out var totalAmount) || totalAmount < 0)
            {
                MessageBox.Show("Please enter a valid total amount.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create a new sales order instance
            var salesOrder = new SalesOrder
            {
                CustomerName = txtNewCustomerName.Text,
                OrderDate = dpOrderDate.SelectedDate.Value,
                Status = (cmbNewSalesOrderStatus.SelectedItem as ComboBoxItem)?.Content.ToString(),
                TotalAmount = totalAmount
            };

            // Insert the sales order into the database
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    var command = new SqlCommand("INSERT INTO SalesOrders (CustomerName, OrderDate, Status, TotalAmount) VALUES (@CustomerName, @OrderDate, @Status, @TotalAmount)", connection);
                    command.Parameters.AddWithValue("@CustomerName", salesOrder.CustomerName);
                    command.Parameters.AddWithValue("@OrderDate", salesOrder.OrderDate);
                    command.Parameters.AddWithValue("@Status", salesOrder.Status);
                    command.Parameters.AddWithValue("@TotalAmount", salesOrder.TotalAmount);
                    command.ExecuteNonQuery();
                }

                // Refresh the sales orders list
                RefreshSalesOrderList();

                // Clear the input fields
                ClearSalesOrderFields();
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Database error: {sqlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void lvPurchaseOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvPurchaseOrders.SelectedItem is PurchaseOrder selectedOrder)
            {
                // Populate fields with selected order details for editing
                txtPurchaseOrderID.Text = selectedOrder.PurchaseOrderID.ToString();
                txtSupplierID.Text = selectedOrder.SupplierID.ToString();
                dpOrderDate.SelectedDate = selectedOrder.OrderDate;
                cmbOrderStatus.SelectedItem = cmbOrderStatus.Items.Cast<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == selectedOrder.Status);
                txtTotalAmount.Text = selectedOrder.TotalAmount.ToString("F2"); // Format for decimal
            }
            else
            {
                // Clear fields if no selection
                ClearOrderFields();
            }
        }
        private void ViewSalesOrders_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Retrieve sales orders from the database
                // ...

                // Bind the retrieved sales orders to the ListView
                lvSalesOrders.ItemsSource = salesOrders;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshSalesOrderList()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    var command = new SqlCommand("SELECT SalesOrderID, CustomerName, OrderDate, Status, TotalAmount FROM SalesOrders", connection);
                    var reader = command.ExecuteReader();

                    salesOrders.Clear(); // Clear the existing list

                    while (reader.Read())
                    {
                        var salesOrder = new SalesOrder
                        {
                            SalesOrderID = reader.IsDBNull(0) ? 0 : reader.GetInt32(0), // Handle NULL for SalesOrderID
                            CustomerName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1), // Handle NULL for CustomerName
                            OrderDate = reader.IsDBNull(2) ? DateTime.MinValue : reader.GetDateTime(2), // Handle NULL for OrderDate
                            Status = reader.IsDBNull(3) ? string.Empty : reader.GetString(3), // Handle NULL for Status
                            TotalAmount = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4) // Handle NULL for TotalAmount
                        };
                        salesOrders.Add(salesOrder);
                    }
                }
                lvSalesOrders.ItemsSource = null; // Clear the current binding
                lvSalesOrders.ItemsSource = salesOrders; // Rebind the updated sales order list
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Database error: {sqlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearSalesOrderFields()
        {
            // Clear Sales Order ID field
            txtNewSalesOrderID.Clear(); // Assuming this is a TextBox for Sales Order ID

            // Clear Customer Name field
            txtNewCustomerName.Clear();

            // Clear Order Date selection
            dpOrderDate.SelectedDate = null; // Assuming this is a DatePicker for Order Date

            // Clear Order Status selection
            cmbNewSalesOrderStatus.SelectedIndex = -1; // Clear order status selection

            // Clear Total Amount field
            txtTotalAmount.Clear();
        }

        private void AddSupplier_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ensure SupplierID is provided by the user
                if (int.TryParse(txtSupplierID.Text, out var supplierId))
                {
                    // Check if the SupplierID is unique
                    using (var connection = GetConnection())
                    {
                        connection.Open();
                        var checkCommand = new SqlCommand("SELECT COUNT(*) FROM Suppliers WHERE SupplierID = @SupplierID", connection);
                        checkCommand.Parameters.AddWithValue("@SupplierID", supplierId);
                        int count = (int)checkCommand.ExecuteScalar();

                        if (count > 0)
                        {
                            // SupplierID is not unique
                            MessageBox.Show("Supplier ID must be unique. Please enter a different Supplier ID.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    // If SupplierID is unique, proceed to insert the supplier
                    using (var connection = GetConnection())
                    {
                        connection.Open();
                        var command = new SqlCommand("INSERT INTO Suppliers (SupplierID, SupplierName, ContactName, Phone, Email, Address) VALUES (@SupplierID, @SupplierName, @ContactName, @Phone, @Email, @Address)", connection);
                        command.Parameters.AddWithValue("@SupplierID", supplierId); // Add SupplierID
                        command.Parameters.AddWithValue("@SupplierName", txtSupplierName.Text);
                        command.Parameters.AddWithValue("@ContactName", txtContactName.Text);
                        command.Parameters.AddWithValue("@Phone", txtPhone.Text);
                        command.Parameters.AddWithValue("@Email", txtEmail.Text);
                        command.Parameters.AddWithValue("@Address", txtAddress.Text);
                        command.ExecuteNonQuery();
                    }

                    // Refresh the supplier list to show the newly added supplier
                    RefreshSupplierList(); // Refresh the list after adding
                    ClearSupplierFields(); // Clear input fields
                }
                else
                {
                    MessageBox.Show("Please enter a valid Supplier ID.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Database error: {sqlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditSupplier_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSupplierIndex >= 0 && selectedSupplierIndex < suppliers.Count)
            {
                var supplier = suppliers[selectedSupplierIndex];

                try
                {
                    using (var connection = GetConnection())
                    {
                        connection.Open();
                        var command = new SqlCommand("UPDATE Suppliers SET SupplierName = @SupplierName, ContactName = @ContactName, Phone = @Phone, Email = @Email, Address = @Address WHERE SupplierID = @SupplierID", connection);
                        command.Parameters.AddWithValue("@SupplierID", supplier.SupplierID);
                        command.Parameters.AddWithValue("@SupplierName", txtSupplierName.Text);
                        command.Parameters.AddWithValue("@ContactName", txtContactName.Text);
                        command.Parameters.AddWithValue("@Phone", txtPhone.Text);
                        command.Parameters.AddWithValue("@Email", txtEmail.Text);
                        command.Parameters.AddWithValue("@Address", txtAddress.Text);
                        command.ExecuteNonQuery();
                    }
                    RefreshSupplierList(); // Refresh the list after editing
                    ClearSupplierFields(); // Clear input fields
                    selectedSupplierIndex = -1; // Reset selection
                }
                catch (SqlException sqlEx)
                {
                    MessageBox.Show($"Database error: {sqlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a supplier to edit.", "Edit Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteSupplier_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSupplierIndex >= 0 && selectedSupplierIndex < suppliers.Count)
            {
                var supplier = suppliers[selectedSupplierIndex];

                try
                {
                    using (var connection = GetConnection())
                    {
                        connection.Open();
                        var command = new SqlCommand("DELETE FROM Suppliers WHERE SupplierID = @SupplierID", connection);
                        command.Parameters.AddWithValue("@SupplierID", supplier.SupplierID);
                        command.ExecuteNonQuery();
                    }
                    suppliers.RemoveAt(selectedSupplierIndex); // Remove from local list
                    RefreshSupplierList(); // Refresh the list after deletion
                    ClearSupplierFields(); // Clear input fields
                    selectedSupplierIndex = -1; // Reset selection
                }
                catch (SqlException sqlEx)
                {
                    MessageBox.Show($"Database error: {sqlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a supplier to delete.", "Delete Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void RefreshSupplierList()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    var command = new SqlCommand("SELECT SupplierID, SupplierName, ContactName, Phone, Email, Address FROM Suppliers", connection);
                    var reader = command.ExecuteReader();

                    suppliers.Clear(); // Clear the existing list

                    while (reader.Read())
                    {
                        var supplier = new Supplier
                        {
                            SupplierID = reader.IsDBNull(0) ? 0 : reader.GetInt32(0), // Handle NULL for SupplierID
                            SupplierName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1), // Handle NULL for SupplierName
                            ContactName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2), // Handle NULL for ContactName
                            Phone = reader.IsDBNull(3) ? string.Empty : reader.GetString(3), // Handle NULL for Phone
                            Email = reader.IsDBNull(4) ? string.Empty : reader.GetString(4), // Handle NULL for Email
                            Address = reader.IsDBNull(5) ? string.Empty : reader.GetString(5) // Handle NULL for Address
                        };
                        suppliers.Add(supplier);
                    }
                }
                lvSuppliers.ItemsSource = null; // Reset the ItemsSource to refresh the ListView
                lvSuppliers.ItemsSource = suppliers; // Bind the updated list to the ListView
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Database error: {sqlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearSupplierFields()
        {
            txtSupplierID.Clear(); // Clear supplier ID field (if needed)
            txtSupplierName.Clear(); // Clear supplier name field
            txtContactName.Clear(); // Clear contact name field
            txtPhone.Clear(); // Clear phone field
            txtEmail.Clear(); // Clear email field
            txtAddress.Clear(); // Clear address field
        }

        private void lvSuppliers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvSuppliers.SelectedItem is Supplier selectedSupplier)
            {
                selectedSupplierIndex = suppliers.IndexOf(selectedSupplier);
                txtSupplierID.Text = selectedSupplier.SupplierID.ToString(); // Set Supplier ID
                txtSupplierName.Text = selectedSupplier.SupplierName; // Set Supplier Name
                txtContactName.Text = selectedSupplier.ContactName; // Set Contact Name
                txtPhone.Text = selectedSupplier.Phone; // Set Phone
                txtEmail.Text = selectedSupplier.Email; // Set Email
                txtAddress.Text = selectedSupplier.Address; // Set Address
            }
            else
            {
                selectedSupplierIndex = -1; // Reset if no selection
                ClearSupplierFields(); // Optionally clear fields if no supplier is selected
            }
        }

        private void RefreshCustomerList()
        {
            //lvCustomers.ItemsSource = null; // Clear the current binding
            //lvCustomers.ItemsSource = customers; // Rebind the updated customer list
        }

        private void ClearCustomerFields()
        {
            //txtCustomerName.Clear(); // Clear customer name field txtCustomerEmail.Clear(); // Clear customer email field
            //txtCustomerPhone.Clear(); // Clear customer phone field
        }


        private void GenerateInventoryValuationReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Logic to generate inventory valuation report
                var report = new Report
                {
                    ReportType = "Inventory Valuation",
                    DateGenerated = DateTime.Now,
                    Status = "Generated"
                };

                // Fetch inventory valuation data from the database
                using (var connection = GetConnection())
                {
                    connection.Open();
                    var command = new SqlCommand("SELECT ProductID, Name, Quantity, UnitPrice FROM Products", connection);
                    var reader = command.ExecuteReader();

                    StringBuilder reportDetails = new StringBuilder();
                    while (reader.Read())
                    {
                        var productId = reader.GetInt32(0);
                        var productName = reader.GetString(1);
                        var quantity = reader.GetInt32(2);
                        var unitPrice = reader.GetDecimal(3);
                        var totalValue = quantity * unitPrice;

                        reportDetails.AppendLine($"Product ID: {productId}, Name: {productName}, Quantity: {quantity}, Unit Price: {unitPrice:C}, Total Value: {totalValue:C}");
                    }

                    // Optionally, you can store the report details in the report object
                    report.Status += "\n" + reportDetails.ToString();
                }

                reports.Add(report);
                RefreshReportsList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while generating the report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateSalesTrendsReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var report = new Report
                {
                    ReportType = "Sales Trends",
                    DateGenerated = DateTime.Now,
                    Status = "Generated"
                };

                using (var connection = GetConnection())
                {
                    connection.Open();
                    var command = new SqlCommand("SELECT OrderDate, TotalAmount FROM SalesOrders", connection);
                    var reader = command.ExecuteReader();

                    StringBuilder reportDetails = new StringBuilder();
                    while (reader.Read())
                    {
                        var orderDate = reader.GetDateTime(0);
                        var totalAmount = reader.GetDecimal(1);
                        reportDetails.AppendLine($"Order Date: {orderDate.ToShortDateString()}, Total Amount: {totalAmount:C}");
                    }

                    report.Status += "\n" + reportDetails.ToString();
                }

                reports.Add(report);
                RefreshReportsList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while generating the report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void RefreshStockMovement()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    var command = new SqlCommand("SELECT MovementID, ProductID, MovementType, Quantity, MovementDate, Description FROM StockMovements", connection);
                    var reader = command.ExecuteReader();

                    stockMovements.Clear(); // Clear the existing list

                    while (reader.Read())
                    {
                        var movement = new StockMovement
                        {
                            MovementID = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                            ProductID = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                            MovementType = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            Quantity = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                            MovementDate = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4),
                            Description = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
                        };
                        stockMovements.Add(movement);
                    }
                }

                // Bind the stock movements to the DataGrid
                dgStockMovements.ItemsSource = null; // Clear the current binding
                dgStockMovements.ItemsSource = stockMovements; // Rebind the updated stock movements list
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Database error: {sqlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void GenerateStockMovementReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var report = new Report
                {
                    ReportType = "Stock Movement",
                    DateGenerated = DateTime.Now,
                    Status = "Generated"
                };

                using (var connection = GetConnection())
                {
                    connection.Open();
                    var command = new SqlCommand("SELECT MovementDate, ProductID, MovementType, Quantity FROM StockMovements", connection);
                    var reader = command.ExecuteReader();

                    StringBuilder reportDetails = new StringBuilder();
                    while (reader.Read())
                    {
                        var movementDate = reader.GetDateTime(0);
                        var productId = reader.GetInt32(1);
                        var movementType = reader.GetString(2);
                        var quantity = reader.GetInt32(3);
                        reportDetails.AppendLine($"Movement Date: {movementDate.ToShortDateString()}, Product ID: {productId}, Movement Type: {movementType}, Quantity: {quantity}");
                    }

                    report.Status += "\n" + reportDetails.ToString();
                }

                reports.Add(report);
                RefreshReportsList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while generating the report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void RefreshReportsList()
        {
            lvReports.ItemsSource = null; // Clear the current binding
            lvReports.ItemsSource = reports; // Rebind the updated reports list
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate user input
                if (string.IsNullOrWhiteSpace(txtUserID.Text) || !int.TryParse(txtUserID.Text, out int userId))
                {
                    MessageBox.Show("User  ID must be a valid integer.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtUserName.Text))
                {
                    MessageBox.Show("User  Name cannot be empty.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPasswordHash.Text))
                {
                    MessageBox.Show("Password cannot be empty.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (cmbUserRole.SelectedItem == null)
                {
                    MessageBox.Show("Please select a user role.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check if User ID is unique
                using (var connection = GetConnection())
                {
                    connection.Open();
                    var checkCommand = new SqlCommand("SELECT COUNT(*) FROM Users WHERE UserID = @UserID", connection);
                    checkCommand.Parameters.AddWithValue("@UserID", userId);
                    int count = (int)checkCommand.ExecuteScalar();

                    if (count > 0)
                    {
                        MessageBox.Show("User  ID must be unique. Please enter a different User ID.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // Create a new user instance
                var user = new User
                {
                    UserID = userId, // Set User ID from input
                    UserName = txtUserName.Text,
                    PasswordHash = txtPasswordHash.Text, // In a real application, you should hash the password
                    Role = (cmbUserRole.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    CreatedAt = DateTime.Now
                };

                // Insert the user into the database
                using (var connection = GetConnection())
                {
                    connection.Open();
                    var command = new SqlCommand("INSERT INTO Users (UserID, Username, PasswordHash, Role, CreatedAt) VALUES (@UserID, @Username, @PasswordHash, @Role, @CreatedAt)", connection);
                    command.Parameters.AddWithValue("@UserID", user.UserID);
                    command.Parameters.AddWithValue("@Username", user.UserName);
                    command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash); // Hash the password before storing
                    command.Parameters.AddWithValue("@Role", user.Role);
                    command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
                    command.ExecuteNonQuery();
                }

                // Refresh the user list
                RefreshUserList();
                ClearUserFields();
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Database error: {sqlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveUser_Click(object sender, RoutedEventArgs e)
        {
            if (lvUsers.SelectedItem is User selectedUser)
            {
                try
                {
                    // Remove the user from the database
                    using (var connection = GetConnection())
                    {
                        connection.Open();
                        // Ensure the parameter name matches the one you are adding
                        var command = new SqlCommand("DELETE FROM Users WHERE UserID = @UserID", connection);
                        command.Parameters.AddWithValue("@UserID", selectedUser.UserID); // Correct parameter name
                        int rowsAffected = command.ExecuteNonQuery(); // Execute the command and get the number of affected rows

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("User  removed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("No user found with the specified ID.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }

                    // Remove the user from the local list and refresh the user list
                    users.Remove(selectedUser);
                    RefreshUserList();
                    ClearUserFields();
                }
                catch (SqlException sqlEx)
                {
                    MessageBox.Show($"Database error: {sqlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a user to remove.", "Selection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void RefreshUserList()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    var command = new SqlCommand("SELECT UserID, Username, PasswordHash, Role, CreatedAt FROM Users", connection);
                    var reader = command.ExecuteReader();

                    users.Clear(); // Clear the existing list

                    while (reader.Read())
                    {
                        var user = new User
                        {
                            UserID = reader.IsDBNull(0) ? 0 : reader.GetInt32(0), // Handle NULL for UserID
                            UserName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1), // Handle NULL for Username
                            PasswordHash = reader.IsDBNull(2) ? string.Empty : reader.GetString(2), // Handle NULL for PasswordHash
                            Role = reader.IsDBNull(3) ? string.Empty : reader.GetString(3), // Handle NULL for Role
                            CreatedAt = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4) // Handle NULL for CreatedAt
                        };
                        users.Add(user);
                    }
                }

                // Update the UI with the new user list
                lvUsers.ItemsSource = null;
                lvUsers.ItemsSource = users;
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Database error: {sqlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearUserFields()
        {
            txtUserID.Clear(); // Clear user ID field
            txtUserName.Clear(); // Clear user name field
            txtPasswordHash.Clear(); // Clear password field
            cmbUserRole.SelectedIndex = -1; // Clear role selection
        }

        private void lvUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvUsers.SelectedItem is User selectedUser)
            {
                txtUserName.Text = selectedUser.UserName ?? string.Empty; // Use null-coalescing operator
                txtPasswordHash.Text = selectedUser.PasswordHash ?? string.Empty; // Use null-coalescing operator
                cmbUserRole.SelectedItem = cmbUserRole.Items.Cast<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == selectedUser.Role);
            }
            else
            {
                ClearUserFields(); // Clear fields if no user is selected
            }
        }
        private void RefreshAuditLogsList()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    var command = new SqlCommand("SELECT LogID, UserID, Action, TableAffected, ActionTime, Description FROM AuditLogs", connection);
                    var reader = command.ExecuteReader();

                    auditLogs.Clear(); // Clear the existing list

                    while (reader.Read())
                    {
                        var log = new AuditLog
                        {
                            LogID = reader.GetInt32(0), // LogID
                            UserID = reader.GetInt32(1), // UserID
                            Action = reader.GetString(2), // Action
                            TableAffected = reader.GetString(3), // TableAffected
                            ActionTime = reader.GetDateTime(4), // ActionTime
                            Description = reader.IsDBNull(5) ? string.Empty : reader.GetString(5) // Description
                        };
                        auditLogs.Add(log);
                    }
                }

                lvAuditLogs.ItemsSource = null; // Clear the current binding
                lvAuditLogs.ItemsSource = auditLogs; // Rebind the updated audit logs list
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Database error: {sqlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LookupProduct_Click(object sender, RoutedEventArgs e)
        {
            string barcode = txtBarcodeScan.Text.Trim();
            var product = products.FirstOrDefault(p => p.Barcode == barcode);

            if (product != null)
            {
                // Display product details
                txtProductDetails.Text = $"Name: {product.Name}\nSKU: {product.SKU}\nCategory: {product.Category}\nQuantity: {product.Quantity}\nPrice: {product.UnitPrice:C}";
                ProductDetailsPanel.Visibility = Visibility.Visible;

                // Show quick stock entry panel
                QuickStockEntryPanel.Visibility = Visibility.Visible; // Ensure this panel is visible for stock entry
            }
            else
            {
                MessageBox.Show("Product not found.", "Lookup Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                ProductDetailsPanel.Visibility = Visibility.Collapsed; // Hide product details if not found
                QuickStockEntryPanel.Visibility = Visibility.Collapsed; // Hide quick stock entry if product not found
            }
            ClearBarcodeInput(); // Clear input after lookup
        }
        private void UpdateStock_Click(object sender, RoutedEventArgs e)
        {
            if (products.FirstOrDefault(p => p.Barcode == txtBarcodeScan.Text.Trim()) is Product product)
            {
                if (int.TryParse(txtStockQuantity.Text, out var quantity) && quantity > 0)
                {
                    // Update the product's quantity
                    product.Quantity += quantity;

                    // Update the product in the database
                    using (var connection = GetConnection())
                    {
                        connection.Open();
                        var command = new SqlCommand("UPDATE Products SET Quantity = @Quantity WHERE Barcode = @Barcode", connection); // Corrected SQL command
                        command.Parameters.AddWithValue("@Quantity", product.Quantity);
                        command.Parameters.AddWithValue("@Barcode", product.Barcode); // Ensure this parameter is correctly defined
                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Stock updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearBarcodeInput(); // Clear input after updating stock
                }
                else
                {
                    MessageBox.Show("Please enter a valid quantity.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("No product found for the entered barcode.", "Update Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ClearBarcodeInput()
        {
            txtBarcodeScan.Clear();
            ProductDetailsPanel.Visibility = Visibility.Collapsed;
        }



        private void SaveRoleChanges_Click(object sender, RoutedEventArgs e)
        {
            string selectedRole = (cmbAdjustUserRole.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (!string.IsNullOrEmpty(selectedRole))
            {
                // Here you would typically save the role changes to a database or configuration file
                MessageBox.Show($"Role '{selectedRole}' has been saved.", "Role Changes", MessageBoxButton.OK, MessageBoxImage.Information);
                // LogUser  Action($"Changed user role to: {selectedRole}", "Settings");
            }
            else
            {
                MessageBox.Show("Please select a role to save.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SaveReorderThreshold_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtReorderThreshold.Text, out int newThreshold))
            {
                // Here you would typically save the new threshold to a database or configuration file
                MessageBox.Show($"Reorder threshold set to: {newThreshold}", "Reorder Threshold", MessageBoxButton.OK, MessageBoxImage.Information);
                // LogUser  Action($"Set reorder threshold to: {newThreshold}", "Settings");
            }
            else
            {
                MessageBox.Show("Please enter a valid number for the reorder threshold.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SaveIntegrationSettings_Click(object sender, RoutedEventArgs e)
        {
            string apiKey = txtApiKey.Text.Trim();
            if (!string.IsNullOrEmpty(apiKey))
            {
                // Here you would typically save the API key to a database or configuration file
                MessageBox.Show("API Key saved successfully.", "Integration Settings", MessageBoxButton.OK, MessageBoxImage.Information);
                // LogUser  Action($"API Key updated.", "Settings");
            }
            else
            {
                MessageBox.Show("Please enter a valid API Key.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CreateBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Define the backup file path
                string backupFilePath = @"C:\Backups\InventorySystemBackup.bak"; // Change this path as needed

                using (var connection = GetConnection())
                {
                    connection.Open();
                    var command = new SqlCommand($"BACKUP DATABASE [InventorySystem] TO DISK = '{backupFilePath}'", connection);
                    command.ExecuteNonQuery();
                }

                MessageBox.Show("Backup created successfully!", "Backup", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Database error: {sqlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetBackupSchedule_Click(object sender, RoutedEventArgs e)
        {
            if (chkAutomaticBackup.IsChecked == true)
            {
                // Logic to enable automatic backups
                MessageBox.Show("Automatic backups have been enabled.", "Backup Schedule", MessageBoxButton.OK, MessageBoxImage.Information);
                // You can implement a timer or a scheduled task here for automatic backups
            }
            else
            {
                MessageBox.Show("Automatic backups have been disabled.", "Backup Schedule", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RestoreBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Define the backup file path
                string backupFilePath = @"C:\Backups\InventorySystemBackup.bak"; // Change this path as needed

                using (var connection = GetConnection())
                {
                    connection.Open();
                    var command = new SqlCommand($"USE master; RESTORE DATABASE [InventorySystem] FROM DISK = '{backupFilePath}' WITH REPLACE", connection);
                    command.ExecuteNonQuery();
                }

                MessageBox.Show("Data restored from backup successfully!", "Restore", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Database error: {sqlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SyncNow_Click(object sender, RoutedEventArgs e)
        {
            txtSyncStatus.Text = "Syncing...";

            // Simulate a sync operation
            Task.Run(() =>
            {
                System.Threading.Thread.Sleep(2000); // Simulate time taken for sync
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    txtSyncStatus.Text = "Synced Successfully";
                    AddIntegrationLog("Synchronization completed successfully.");
                });
            });
        }
        private void AddIntegrationLog(string message)
        {
            var log = new IntegrationLog
            {
                LogMessage = message,
                Timestamp = DateTime.Now
            };
            integrationLogs.Add(log);
            RefreshIntegrationLogsList();
        }

        private void RefreshIntegrationLogsList()
        {
            lvIntegrationLogs.ItemsSource = null; // Clear the current binding
            lvIntegrationLogs.ItemsSource = integrationLogs; // Rebind the updated integration logs list
        }

        private void OpenUserManual_Click(object sender, RoutedEventArgs e)
        {
            UserManual userManual = new UserManual();
            Window manualWindow = new Window
            {
                Title = "User  Manual",
                Content = userManual,
                Width = 600,
                Height = 400
            };
            manualWindow.Show();
        }

        private void ViewFAQs_Click(object sender, RoutedEventArgs e)
        {
            FAQs faqs = new FAQs();
            Window faqsWindow = new Window
            {
                Title = "Frequently Asked Questions",
                Content = faqs,
                Width = 600,
                Height = 400
            };
            faqsWindow.Show();
        }

        private void ContactSupport_Click(object sender, RoutedEventArgs e)
        {
            ContactSupport contactSupport = new ContactSupport();
            Window supportWindow = new Window
            {
                Title = "Contact Support",
                Content = contactSupport,
                Width = 400,
                Height = 400
            };
            supportWindow.Show();
        }
        private void lvSalesOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvSalesOrders.SelectedItem is SalesOrder selectedOrder)
            {
                // Populate fields with selected order details for editing
                txtNewSalesOrderID.Text = selectedOrder.SalesOrderID.ToString();
                txtNewCustomerName.Text = selectedOrder.CustomerName;
                dpOrderDate.SelectedDate = selectedOrder.OrderDate;
                cmbNewSalesOrderStatus.SelectedItem = cmbNewSalesOrderStatus.Items.Cast<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == selectedOrder.Status);
                txtTotalAmount.Text = selectedOrder.TotalAmount.ToString("F2"); // Format for decimal
            }
            else
            {
                // Clear fields if no selection
                ClearSalesOrderFields();
            }
        }



        private void SupplierManagementButton_Click(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 5; // Supplier Management tab
        }

        private void CustomerManagementButton_Click(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 6; // Customer Management tab
        }

        private void UserManagementButton_Click(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 7; // User Management tab
        }

        private void AuditLogsButton_Click(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 8; // Audit Logs tab
        }

        private void BarcodeScanningButton_Click(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 9; // Barcode Scanning tab
        }

        private void NotificationsCenterButton_Click(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 10; // Notifications Center tab
        }

        private void SettingsConfigurationButton_Click(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 11; // Settings and Configuration tab
        }

        private void BackupRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 12; // Backup and Restore tab
        }

        private void ReportsAnalyticsButton_Click(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 13; // Reports and Analytics tab
        }

        private void HelpSupportButton_Click(object sender, RoutedEventArgs e)
        {
            TabControl.SelectedIndex = 14; // Help and Support tab
        }

        private void LoadStockMovements()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    var command = new SqlCommand("SELECT MovementID, ProductID, MovementType, Quantity, MovementDate, Description FROM StockMovements", connection);
                    var reader = command.ExecuteReader();

                    stockMovements.Clear(); // Clear the existing list

                    while (reader.Read())
                    {
                        var movement = new StockMovement
                        {
                            MovementID = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                            ProductID = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                            MovementType = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                            Quantity = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                            MovementDate = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4),
                            Description = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
                        };
                        stockMovements.Add(movement);
                    }
                }

            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Database error: {sqlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void AddStockMovement(StockMovement movement)
        {
            try
            {
                // Check if Movement ID and Product ID are unique
                using (var connection = GetConnection())
                {
                    connection.Open();

                    // Check for unique Movement ID
                    var checkMovementIdCommand = new SqlCommand("SELECT COUNT(*) FROM StockMovements WHERE MovementID = @MovementID", connection);
                    checkMovementIdCommand.Parameters.AddWithValue("@MovementID", movement.MovementID);
                    int movementIdCount = (int)checkMovementIdCommand.ExecuteScalar();

                    if (movementIdCount > 0)
                    {
                        MessageBox.Show("Movement ID must be unique. Please enter a different Movement ID.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Check for unique Product ID
                    var checkProductIdCommand = new SqlCommand("SELECT COUNT(*) FROM StockMovements WHERE ProductID = @ProductID", connection);
                    checkProductIdCommand.Parameters.AddWithValue("@ProductID", movement.ProductID);
                    int productIdCount = (int)checkProductIdCommand.ExecuteScalar();

                    if (productIdCount > 0)
                    {
                        MessageBox.Show("Product ID must be unique. Please enter a different Product ID.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Insert the new stock movement
                    var command = new SqlCommand("INSERT INTO StockMovements (MovementID, ProductID, MovementType, Quantity, MovementDate, Description) VALUES (@MovementID, @ProductID, @MovementType, @Quantity, @MovementDate, @Description)", connection);
                    command.Parameters.AddWithValue("@MovementID", movement.MovementID);
                    command.Parameters.AddWithValue("@ProductID", movement.ProductID);
                    command.Parameters.AddWithValue("@MovementType", movement.MovementType);
                    command.Parameters.AddWithValue("@Quantity", movement.Quantity);
                    command.Parameters.AddWithValue("@MovementDate", movement.MovementDate);
                    command.Parameters.AddWithValue("@Description", movement.Description);
                    command.ExecuteNonQuery();
                }

                // Add the new movement to the ObservableCollection
                StockMovements.Add(movement); // This will automatically update the DataGrid
                RefreshStockMovement();

                // Update the Movement ID TextBox
                txtNewMovementID.Text = movement.MovementID.ToString();
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Database error: {sqlEx.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void AddStockMovement_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate inputs
                if (int.TryParse(txtNewMovementID.Text, out int movementId) &&
                    int.TryParse(txtNewProductID.Text, out int productId) &&
                    !string.IsNullOrWhiteSpace(txtNewMovementType.Text) &&
                    int.TryParse(txtNewQuantity.Text, out int quantity) &&
                    dpNewMovementDate.SelectedDate.HasValue)
                {
                    var movement = new StockMovement
                    {
                        MovementID = movementId,
                        ProductID = productId,
                        MovementType = txtNewMovementType.Text,
                        Quantity = quantity,
                        MovementDate = dpNewMovementDate.SelectedDate.Value,
                        Description = txtNewDescription.Text
                    };

                    AddStockMovement(movement);
                }
                else
                {
                    MessageBox.Show("Please ensure all fields are filled out correctly.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearStockMovementFields()
        {
            txtNewProductID.Clear();
            txtNewMovementType.Clear();
            txtNewQuantity.Clear();
            dpNewMovementDate.SelectedDate = null;
            txtNewDescription.Clear();
        }


    }
    public class IntegrationLog
    {
        public string LogMessage { get; set; } // The log message
        public DateTime Timestamp { get; set; } // When the log was created
    }

    public class PurchaseOrder
    {
        public int PurchaseOrderID { get; set; } // Unique identifier for the purchase order
        public int SupplierID { get; set; } // ID of the supplier
        public DateTime OrderDate { get; set; } // Date of the order
        public string Status { get; set; } // Status of the order
        public decimal TotalAmount { get; set; } // Total amount for the order
    }

    public class SalesOrder
    {
        public int SalesOrderID { get; set; } // Unique identifier for the sales order
        public string CustomerName { get; set; } // Customer name
        public DateTime OrderDate { get; set; } // Date of the order
        public string Status { get; set; } // Order status
        public decimal TotalAmount { get; set; } // Total amount for the order
    }
    public class Supplier
    {
        public int SupplierID { get; set; } // Unique identifier for the supplier
        public string SupplierName { get; set; } // Supplier name
        public string ContactName { get; set; } // Contact name
        public string Phone { get; set; } // Phone number
        public string Email { get; set; } // Email address
        public string Address { get; set; } // Address
    }

    public class AuditLog
    {

        public int LogID { get; set; } // Unique identifier for the log entry
        public int UserID { get; set; } // ID of the user who performed the action
        public string Action { get; set; } // Description of the action performed
        public string TableAffected { get; set; } // The table affected by the action
        public DateTime ActionTime { get; set; } // When the action was performed
        public string Description { get; set; } // Additional description of the action
        public DateTime Timestamp { get; set; } // When the log entry was created
        public string AffectedModule { get; set; } // The module affected by the action
    }

    public class Report
    {
        public string ReportType { get; set; } // Type of the report
        public DateTime DateGenerated { get; set; } // Date the report was generated
        public string Status { get; set; } // Status of the report
    }
    public class Customer
    {
        public string Name { get; set; } // Customer name
        public string Email { get; set; } // Customer email
        public string Phone { get; set; } // Customer phone
    }
    public class DateTimeToStringConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                return dateTime.ToString("g"); // General date/time pattern
            }
            return string.Empty; // Return empty string if value is not DateTime
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class Notification
    {
        public string Type { get; set; } // Type of notification (e.g., "Alert", "Info")
        public string Message { get; set; } // Notification message
        public DateTime Timestamp { get; set; } // When the notification was created
    }
    public class User
    {
        public int UserID { get; set; } // Unique identifier for the user, generated by the database
        public string UserName { get; set; } // User's name
        public string PasswordHash { get; set; } // Hashed password
        public string Role { get; set; } // User's role
        public DateTime CreatedAt { get; set; } // Date the user was created
    }
    public class StockMovement
    {
        public int MovementID { get; set; } // Unique identifier for the stock movement
        public int ProductID { get; set; } // ID of the product
        public string MovementType { get; set; } // Type of movement (e.g., "In", "Out")
        public int Quantity { get; set; } // Quantity of stock moved
        public DateTime MovementDate { get; set; } // Date of the movement
        public string Description { get; set; } // Description of the movement
    }
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                return string.IsNullOrEmpty(strValue) ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
