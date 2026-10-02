using Microsoft.IdentityModel.Tokens;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
namespace BusinessManagement.App
{
    public partial class MainDashboardForm : Form
    {
    private List<CartItem> currentCart = new List<CartItem>();
private DataGridView dgvCart;
private Label lblCartTotalVal;
private TextBox txtCustomerName;
        private Panel pnlSidebar;
        private Panel pnlContentArea;
        private Label lblHeaderTitle;
        private InventoryRepository repository;
        private Button btnInventoryNav;

        public MainDashboardForm()
        {
            repository = new InventoryRepository();
            InitializeCustomLayout();
            SwitchView("Dashboard"); // Load dashboard by default
        }
        private void UpdateSidebarBadges()
        {
            if (btnInventoryNav == null) return;

            try
            {
                int lowStockCount = repository.GetLowStockCount();
                if (lowStockCount > 0)
                {
                    btnInventoryNav.Text = $"📦  Inventory Catalog  [{lowStockCount}!]";
                    btnInventoryNav.ForeColor = Color.FromArgb(248, 113, 113); // Soft red alert
                }
                else
                {
                    btnInventoryNav.Text = "📦  Inventory Catalog";
                    btnInventoryNav.ForeColor = Color.FromArgb(209, 213, 219); // Default grey
                }
            }
            catch
            {
                // Fallback if table doesn't exist yet
            }
        }

        private void InitializeCustomLayout()
        {
            this.WindowState = FormWindowState.Maximized;
            this.Text = "Nexus Enterprise ERP & POS Dashboard";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(15, 13, 25);

            // 1. CREATE THE SIDEBAR PANEL FIRST
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 240,
                BackColor = Color.FromArgb(20, 16, 32)
            };
            this.Controls.Add(pnlSidebar); // <-- Must be added to form first so it exists!

            // 2. NOW ADD BRANDING & NAV BUTTONS TO IT
            Label lblBrand = new Label
            {
                Location = new Point(20, 25),
                AutoSize = true,
                Text = "⚡ NEXUS ERP",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White
            };
            pnlSidebar.Controls.Add(lblBrand);

            AddNavButton("📊  Dashboard", 80, (s, e) => SwitchView("Dashboard"));
            AddNavButton("📦  Inventory Catalog", 135, (s, e) => SwitchView("Inventory"));
            AddNavButton("🛒  POS Checkout", 190, (s, e) => SwitchView("Checkout"));
            AddNavButton("📈  Z-Report & Financials", 245, (s, e) => SwitchView("ZReport"));
            AddNavButton("📄  Invoice Manager", 300, (s, e) => SwitchView("Invoices"));
            AddNavButton("🚚  Suppliers & Orders", 355, (s, e) => SwitchView("Suppliers"));

            // --- 2. Top Header Bar ---
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(24, 20, 37)
            };

            lblHeaderTitle = new Label
            {
                Location = new Point(30, 20),
                AutoSize = true,
                Text = "Dashboard Overview",
                Font = new Font("Segoe UI Semibold", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(243, 244, 246)
            };
            pnlHeader.Controls.Add(lblHeaderTitle);

            // --- 3. Right Content Area ---
            pnlContentArea = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(15, 13, 25)
            };

            this.Controls.Add(pnlContentArea);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlSidebar);
        }

        private Button AddNavButton(string text, int topPosition, EventHandler onClick) // Changed from void to Button
        {
            Button btn = new Button
            {
                Location = new Point(15, topPosition),
                Size = new Size(210, 42),
                Text = text,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                BackColor = Color.FromArgb(24, 20, 37),
                ForeColor = Color.FromArgb(209, 213, 219),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI Semibold", 10f)
            };
            btn.FlatAppearance.BorderSize = 0;

            btn.MouseEnter += (s, e) => { btn.BackColor = Color.FromArgb(79, 70, 229); btn.ForeColor = Color.White; };
            btn.MouseLeave += (s, e) => { btn.BackColor = Color.FromArgb(24, 20, 37); btn.ForeColor = Color.FromArgb(209, 213, 219); };

            btn.Click += onClick;
            pnlSidebar.Controls.Add(btn);

            return btn; // <-- Add this line so it returns the button reference
        }

        private void SwitchView(string viewName)
        {
            pnlContentArea.Controls.Clear(); // Clear previous view controls

            switch (viewName)
            {
                case "Dashboard":
                    lblHeaderTitle.Text = "Dashboard Overview";
                    LoadDashboardView();
                    break;
                case "Inventory":
                    lblHeaderTitle.Text = "Product Inventory Catalog";
                    LoadInventoryView();
                    break;
                case "Checkout":
                    lblHeaderTitle.Text = "Active POS Checkout Terminal";
                    LoadCheckoutView();
                    break;
                case "ZReport":
                    lblHeaderTitle.Text = "Daily Financial Z-Report & Summaries";
                    LoadZReportView();
                    break;
                case "Invoices":
                    lblHeaderTitle.Text = "Invoice History & Receipts";
                    LoadInvoiceView();
                    break;
                case "Suppliers":
                    lblHeaderTitle.Text = "Supplier & Supply Chain Management";
                    LoadSuppliersView();
                    break;
            }
        }

        private void LoadInventoryView()
        {
            DataGridView dgvInventory = new DataGridView
            {
                Location = new Point(30, 30),
                Size = new Size(1100, 520),
                BackgroundColor = Color.FromArgb(24, 20, 37),
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            dgvInventory.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            dgvInventory.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvInventory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvInventory.EnableHeadersVisualStyles = false;
            dgvInventory.DefaultCellStyle.BackColor = Color.FromArgb(24, 20, 37);
            dgvInventory.DefaultCellStyle.ForeColor = Color.FromArgb(229, 231, 235);
            dgvInventory.DefaultCellStyle.SelectionBackColor = Color.FromArgb(79, 70, 229);

            try
            {
                dgvInventory.DataSource = repository.GetAllProducts();
            }
            catch { }

            pnlContentArea.Controls.Add(dgvInventory);
        }

        private void LoadCheckoutView()
        {
            currentCart.Clear();

            // --- Top Managerial Overview Strip ---
            Panel pnlManagerHUD = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(1100, 50),
                BackColor = Color.FromArgb(24, 20, 37)
            };

            Label lblHudTitle = new Label
            {
                Location = new Point(15, 15),
                AutoSize = true,
                ForeColor = Color.FromArgb(156, 163, 175),
                Text = "👔 MANAGER POS HUD:",
                Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold)
            };

            // Pull quick today metrics from repository
            decimal todayRevenue = 0;
            int todayTxCount = 0;
            try
            {
                var summaryDt = repository.GetDailySalesSummary(DateTime.Today);
                if (summaryDt != null && summaryDt.Rows.Count > 0)
                {
                    if (summaryDt.Rows[0]["GrossRevenue"] != DBNull.Value)
                        todayRevenue = Convert.ToDecimal(summaryDt.Rows[0]["GrossRevenue"]);
                    if (summaryDt.Rows[0]["TotalTransactions"] != DBNull.Value)
                        todayTxCount = Convert.ToInt32(summaryDt.Rows[0]["TotalTransactions"]);
                }
            }
            catch { }

            Label lblHudMetrics = new Label
            {
                Location = new Point(180, 15),
                AutoSize = true,
                ForeColor = Color.FromArgb(52, 211, 153), // Emerald
                Text = $"Today's Register Drawer: Rs.{todayRevenue:N2}  |  Total Sales Processed: {todayTxCount}",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };

            Button btnViewRecentInvoices = new Button
            {
                Location = new Point(910, 9),
                Size = new Size(175, 32),
                Text = "🔍 Audit Recent Receipts",
                BackColor = Color.FromArgb(55, 48, 107),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            btnViewRecentInvoices.FlatAppearance.BorderSize = 0;
            btnViewRecentInvoices.Click += (s, e) => {
                SwitchView("Invoices"); // Jump straight to invoice audit trail
            };

            pnlManagerHUD.Controls.AddRange(new Control[] { lblHudTitle, lblHudMetrics, btnViewRecentInvoices });
            pnlContentArea.Controls.Add(pnlManagerHUD);

            // --- Left Pane: Product Catalog ---
            Label lblCatalogTitle = new Label
            {
                Location = new Point(20, 85),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(243, 244, 246),
                Text = "Product Catalog & Stock Lookup"
            };
            pnlContentArea.Controls.Add(lblCatalogTitle);

            DataGridView dgvCatalog = new DataGridView
            {
                Location = new Point(20, 115),
                Size = new Size(520, 395),
                BackgroundColor = Color.FromArgb(24, 20, 37),
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false
            };
            dgvCatalog.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(79, 70, 229);
            dgvCatalog.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCatalog.DefaultCellStyle.BackColor = Color.FromArgb(24, 20, 37);
            dgvCatalog.DefaultCellStyle.ForeColor = Color.FromArgb(229, 231, 235);
            dgvCatalog.DefaultCellStyle.SelectionBackColor = Color.FromArgb(99, 102, 241);

            try
            {
                dgvCatalog.DataSource = repository.GetAllProducts();
                if (dgvCatalog.Columns.Contains("ProductID")) dgvCatalog.Columns["ProductID"].Visible = false;
                if (dgvCatalog.Columns.Contains("ReorderLevel")) dgvCatalog.Columns["ReorderLevel"].Visible = false;
            }
            catch { }
            pnlContentArea.Controls.Add(dgvCatalog);

            // --- Right Pane: Active Cart ---
            Label lblCartTitle = new Label
            {
                Location = new Point(560, 85),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(243, 244, 246),
                Text = "Active Basket & Manager Checkout"
            };
            pnlContentArea.Controls.Add(lblCartTitle);

            dgvCart = new DataGridView
            {
                Location = new Point(560, 115),
                Size = new Size(560, 290),
                BackgroundColor = Color.FromArgb(24, 20, 37),
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false
            };
            dgvCart.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            dgvCart.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCart.DefaultCellStyle.BackColor = Color.FromArgb(24, 20, 37);
            dgvCart.DefaultCellStyle.ForeColor = Color.FromArgb(229, 231, 235);
            pnlContentArea.Controls.Add(dgvCart);

            // --- Add to Cart Button ---
            Button btnAddToCart = new Button
            {
                Location = new Point(20, 522),
                Size = new Size(520, 36),
                Text = "➕ Add Item to Basket",
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold)
            };
            btnAddToCart.FlatAppearance.BorderSize = 0;
            btnAddToCart.Click += (s, e) =>
            {
                if (dgvCatalog.SelectedRows.Count > 0)
                {
                    var row = dgvCatalog.SelectedRows[0];
                    int productId = Convert.ToInt32(row.Cells["ProductID"].Value);
                    string productName = row.Cells["ProductName"].Value.ToString();
                    decimal unitPrice = Convert.ToDecimal(row.Cells["UnitPrice"].Value);
                    int stock = Convert.ToInt32(row.Cells["StockQuantity"].Value);

                    if (stock <= 0)
                    {
                        MessageBox.Show("Item is out of stock!", "Stock Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var existing = currentCart.Find(c => c.ProductID == productId);
                    if (existing != null)
                    {
                        if (existing.Quantity + 1 > stock)
                        {
                            MessageBox.Show("Cannot exceed available inventory stock.", "Limit Reached", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        existing.Quantity++;
                    }
                    else
                    {
                        currentCart.Add(new CartItem { ProductID = productId, ProductName = productName, UnitPrice = unitPrice, Quantity = 1 });
                    }
                    RefreshCartGrid();
                }
            };
            pnlContentArea.Controls.Add(btnAddToCart);

            // --- Checkout Controls Box ---
            Panel pnlCheckoutBox = new Panel
            {
                Location = new Point(560, 415),
                Size = new Size(560, 143),
                BackColor = Color.FromArgb(24, 20, 37)
            };

            Label lblCustName = new Label
            {
                Location = new Point(15, 15),
                AutoSize = true,
                ForeColor = Color.FromArgb(156, 163, 175),
                Text = "Customer / Account:",
                Font = new Font("Segoe UI", 9f)
            };
            txtCustomerName = new TextBox
            {
                Location = new Point(145, 13),
                Size = new Size(395, 27),
                Text = "Walk-in Customer",
                BackColor = Color.FromArgb(30, 27, 46),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblTotalText = new Label
            {
                Location = new Point(15, 57),
                AutoSize = true,
                ForeColor = Color.FromArgb(156, 163, 175),
                Text = "Total Due:",
                Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold)
            };
            lblCartTotalVal = new Label
            {
                Location = new Point(145, 53),
                AutoSize = true,
                ForeColor = Color.FromArgb(52, 211, 153),
                Text = "Rs.0.00",
                Font = new Font("Segoe UI", 14, FontStyle.Bold)
            };

            Button btnCompleteCheckout = new Button
            {
                Location = new Point(15, 93),
                Size = new Size(525, 36),
                Text = "💳 Authorize Sale & Update Inventory",
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold)
            };
            btnCompleteCheckout.FlatAppearance.BorderSize = 0;
            btnCompleteCheckout.Click += (s, e) =>
            {
                if (currentCart.Count == 0)
                {
                    MessageBox.Show("Basket is empty.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    string custName = string.IsNullOrWhiteSpace(txtCustomerName.Text) ? "Walk-in Customer" : txtCustomerName.Text;
                    long invoiceId = repository.SaveInvoice(custName, currentCart);

                    MessageBox.Show($"Transaction authorized successfully!\nInvoice #{invoiceId} recorded and stock decremented.", "Manager Sign-off", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    currentCart.Clear();
                    RefreshCartGrid();
                    dgvCatalog.DataSource = repository.GetAllProducts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Checkout error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            pnlCheckoutBox.Controls.AddRange(new Control[] { lblCustName, txtCustomerName, lblTotalText, lblCartTotalVal, btnCompleteCheckout });
            pnlContentArea.Controls.Add(pnlCheckoutBox);
        }

        private void RefreshCartGrid()
        {
            dgvCart.DataSource = null;
            dgvCart.DataSource = currentCart;

            decimal total = 0;
            foreach (var item in currentCart) total += item.Subtotal;
            if (lblCartTotalVal != null) lblCartTotalVal.Text = $"${total:N2}";
        }

        private void LoadZReportView()
        {
            // --- Export Button ---
            Button btnExport = new Button
            {
                Location = new Point(30, 20),
                Size = new Size(160, 35),
                Text = "📥 Export to CSV",
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold)
            };
            btnExport.FlatAppearance.BorderSize = 0;
            pnlContentArea.Controls.Add(btnExport);
            DataGridView dgvZReport = new DataGridView
            {
                Location = new Point(30, 70), // Moved down to fit button
                Size = new Size(1100, 480),
                BackgroundColor = Color.FromArgb(24, 20, 37),
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                RowHeadersVisible = false
            };

            dgvZReport.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            dgvZReport.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvZReport.EnableHeadersVisualStyles = false;
            dgvZReport.DefaultCellStyle.BackColor = Color.FromArgb(24, 20, 37);
            dgvZReport.DefaultCellStyle.ForeColor = Color.FromArgb(229, 231, 235);

            try
            {
                dgvZReport.DataSource = repository.GetItemizedSalesSummary(DateTime.Today);
            }
            catch { }

            // Wire up export click event
            btnExport.Click += (s, e) => ExportDataGridViewToCSV(dgvZReport, $"Z-Report_{DateTime.Now:yyyyMMdd}.csv");

            pnlContentArea.Controls.Add(dgvZReport);
        }

        private void LoadInvoiceView()
        {
            DataGridView dgvInvoices = new DataGridView
            {
                Location = new Point(30, 30),
                Size = new Size(1100, 520),
                BackgroundColor = Color.FromArgb(24, 20, 37),
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                RowHeadersVisible = false
            };

            dgvInvoices.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            dgvInvoices.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvInvoices.EnableHeadersVisualStyles = false;
            dgvInvoices.DefaultCellStyle.BackColor = Color.FromArgb(24, 20, 37);
            dgvInvoices.DefaultCellStyle.ForeColor = Color.FromArgb(229, 231, 235);

            try
            {
                dgvInvoices.DataSource = repository.GetAllInvoices();
            }
            catch { }

            pnlContentArea.Controls.Add(dgvInvoices);
        }

        private void LoadSuppliersView()
{
            // --- Header Title ---
            Label lblTitle = new Label
            {
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(243, 244, 246),
                Text = "⚡ Automated Supply Chain & Purchase Order Hub"
            };
            pnlContentArea.Controls.Add(lblTitle);

            // --- Subtitle / Status ---
            Label lblSubtitle = new Label
            {
                Location = new Point(20, 50),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(156, 163, 175),
                Text = "System continuously monitors stock thresholds and auto-drafts restock purchase orders."
            };
            pnlContentArea.Controls.Add(lblSubtitle);

            // --- Automated Restock Queue Grid ---
            Label lblGridTitle = new Label
            {
                Location = new Point(20, 90),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(243, 244, 246),
                Text = "Auto-Drafted Restock Purchase Orders (Low Stock Triggered)"
            };
            pnlContentArea.Controls.Add(lblGridTitle);

            DataGridView dgvRestockQueue = new DataGridView
            {
                Location = new Point(20, 120),
                Size = new Size(1100, 340),
                BackgroundColor = Color.FromArgb(24, 20, 37),
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false
            };
            dgvRestockQueue.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(79, 70, 229);
            dgvRestockQueue.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvRestockQueue.DefaultCellStyle.BackColor = Color.FromArgb(24, 20, 37);
            dgvRestockQueue.DefaultCellStyle.ForeColor = Color.FromArgb(229, 231, 235);
            dgvRestockQueue.DefaultCellStyle.SelectionBackColor = Color.FromArgb(99, 102, 241);

            try
            {
                dgvRestockQueue.DataSource = repository.GetAutomatedRestockQueue();
                if (dgvRestockQueue.Columns.Contains("ProductID")) dgvRestockQueue.Columns["ProductID"].Visible = false;
                if (dgvRestockQueue.Columns.Contains("SupplierID")) dgvRestockQueue.Columns["SupplierID"].Visible = false;
            }
            catch { }
            pnlContentArea.Controls.Add(dgvRestockQueue);

            // --- Executive Action Button Panel ---
            Panel pnlActionBox = new Panel
            {
                Location = new Point(20, 475),
                Size = new Size(1100, 70),
                BackColor = Color.FromArgb(24, 20, 37)
            };

            Button btnExecuteAutoOrder = new Button
            {
                Location = new Point(15, 15),
                Size = new Size(350, 40),
                Text = "🚀 Dispatch Selected Purchase Order",
                BackColor = Color.FromArgb(16, 185, 129), // Emerald
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold)
            };
            btnExecuteAutoOrder.FlatAppearance.BorderSize = 0;
            btnExecuteAutoOrder.Click += (s, e) =>
            {
                if (dgvRestockQueue.SelectedRows.Count > 0)
                {
                    var row = dgvRestockQueue.SelectedRows[0];
                    int productId = Convert.ToInt32(row.Cells["ProductID"].Value);
                    long supplierId = Convert.ToInt64(row.Cells["SupplierID"].Value);
                    string supplierName = row.Cells["SupplierName"].Value.ToString();
                    int qty = Convert.ToInt32(row.Cells["RecommendedQty"].Value);
                    decimal cost = Convert.ToDecimal(row.Cells["EstimatedCost"].Value);
                    string productName = row.Cells["ProductName"].Value.ToString();

                    try
                    {
                        // Execute restock transaction and increment stock automatically
                        repository.PlaceRestockOrder(supplierId, supplierName, productId, qty, cost);

                        MessageBox.Show($"Purchase order successfully dispatched to {supplierName}!\nRestocked {qty} units of '{productName}'. Inventory levels updated.", "Automation Executed", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Refresh queue
                        dgvRestockQueue.DataSource = repository.GetAutomatedRestockQueue();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to dispatch order: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Please select an auto-drafted purchase order from the queue.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            Label lblInfoNote = new Label
            {
                Location = new Point(380, 24),
                AutoSize = true,
                ForeColor = Color.FromArgb(156, 163, 175),
                Text = "💡 Selecting an order and clicking dispatch automatically restocks inventory and logs vendor expenditure.",
                Font = new Font("Segoe UI", 9f)
            };

            pnlActionBox.Controls.AddRange(new Control[] { btnExecuteAutoOrder, lblInfoNote });
            pnlContentArea.Controls.Add(pnlActionBox);
        }

        private void LoadDashboardView()
        {
            pnlContentArea.Controls.Clear();

            // --- Main Left Workspace (Sales Trend Chart) ---
            Panel pnlWorkspace = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(740, 520),
                BackColor = Color.FromArgb(24, 20, 37)
            };

            Label lblWorkspaceTitle = new Label
            {
                Text = "Store Performance & Sales Trends",
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 12f, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            pnlWorkspace.Controls.Add(lblWorkspaceTitle);

            // Chart Canvas Panel
            Panel pnlChartCanvas = new Panel
            {
                Location = new Point(20, 65),
                Size = new Size(700, 435),
                BackColor = Color.FromArgb(30, 25, 45)
            };

            // Fetch sales data for the chart
            DataTable trendData = repository.GetRecentSalesTrend();

            // Paint event to draw a sleek bar chart dynamically
            pnlChartCanvas.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                if (trendData.Rows.Count == 0)
                {
                    using (Brush brush = new SolidBrush(Color.FromArgb(140, 140, 150)))
                    {
                        g.DrawString("No sales data recorded yet. Run a checkout to populate trend!",
                            new Font("Segoe UI", 10f), brush, new PointF(20, 40));
                    }
                    return;
                }

                int startX = 60;
                int maxBarHeight = 300;
                int barWidth = 50;
                int spacing = 40;

                // Find max revenue for scaling bars
                decimal maxVal = 100; // default ceiling
                foreach (DataRow row in trendData.Rows)
                {
                    decimal rev = Convert.ToDecimal(row["DailyRevenue"]);
                    if (rev > maxVal) maxVal = rev;
                }

                // Draw axes lines
                using (Pen axisPen = new Pen(Color.FromArgb(60, 50, 80), 1))
                {
                    g.DrawLine(axisPen, 50, 380, 660, 380); // X Axis
                    g.DrawLine(axisPen, 50, 50, 50, 380);   // Y Axis
                }

                int index = 0;
                foreach (DataRow row in trendData.Rows)
                {
                    string dateStr = Convert.ToDateTime(row["SaleDate"]).ToString("MMM dd");
                    decimal revenue = Convert.ToDecimal(row["DailyRevenue"]);

                    int barHeight = (maxVal > 0) ? (int)((revenue / maxVal) * maxBarHeight) : 10;
                    if (barHeight < 5 && revenue > 0) barHeight = 5;

                    int posX = startX + (index * (barWidth + spacing));
                    int posY = 380 - barHeight;

                    // Draw Bar with Jewel-tone gradient/solid color
                    using (Brush barBrush = new SolidBrush(Color.FromArgb(79, 78, 229))) // Indigo accent
                    {
                        g.FillRectangle(barBrush, posX, posY, barWidth, barHeight);
                    }

                    // Draw Value on top of bar
                    using (Brush textBrush = new SolidBrush(Color.White))
                    {
                        g.DrawString($"Rs.{revenue:0}", new Font("Segoe UI", 8f), textBrush, new PointF(posX - 5, posY - 18));
                        // Draw Date label underneath
                        g.DrawString(dateStr, new Font("Segoe UI", 8f), new SolidBrush(Color.FromArgb(180, 180, 190)), new PointF(posX, 390));
                    }

                    index++;
                }
            };

            pnlWorkspace.Controls.Add(pnlChartCanvas);


            // --- Right-Hand Live Shift Performance Tracker Sidebar ---
            Panel pnlSidebar = new Panel
            {
                Location = new Point(780, 20),
                Size = new Size(330, 520),
                BackColor = Color.FromArgb(24, 20, 37)
            };

            Label lblSidebarTitle = new Label
            {
                Text = "⚡ Live Shift Tracker",
                ForeColor = Color.FromArgb(79, 78, 229),
                Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            pnlSidebar.Controls.Add(lblSidebarTitle);

            var metrics = repository.GetLiveShiftMetrics();
            decimal avgBasket = metrics.transactionCount > 0 ? metrics.grossRevenue / metrics.transactionCount : 0;

            int startY = 65;
            Action<string, string, Color> addMetricCard = (title, value, valColor) =>
            {
                Panel card = new Panel
                {
                    Location = new Point(20, startY),
                    Size = new Size(290, 75),
                    BackColor = Color.FromArgb(35, 30, 50)
                };

                Label lblTitle = new Label
                {
                    Text = title,
                    ForeColor = Color.FromArgb(180, 180, 190),
                    Font = new Font("Segoe UI", 9f),
                    Location = new Point(15, 12),
                    AutoSize = true
                };

                Label lblVal = new Label
                {
                    Text = value,
                    ForeColor = valColor,
                    Font = new Font("Segoe UI Semibold", 14f, FontStyle.Bold),
                    Location = new Point(15, 35),
                    AutoSize = true
                };

                card.Controls.Add(lblTitle);
                card.Controls.Add(lblVal);
                pnlSidebar.Controls.Add(card);
                startY += 90;
            };

            addMetricCard("Today's Gross Revenue", $"Rs. {metrics.grossRevenue:N2}", Color.FromArgb(16, 185, 129));
            addMetricCard("Total Transactions", metrics.transactionCount.ToString(), Color.White);
            addMetricCard("Average Basket Size", $"Rs. {avgBasket:N2}", Color.White);
            addMetricCard("Critical Low-Stock Items", metrics.lowStockCount.ToString(), metrics.lowStockCount > 0 ? Color.FromArgb(239, 68, 68) : Color.FromArgb(16, 185, 129));

            pnlContentArea.Controls.Add(pnlWorkspace);
            pnlContentArea.Controls.Add(pnlSidebar);
        }

        private void CreateStatCard(Panel parent, string title, string value, Color bgColor, int leftOffset)
        {
            Panel card = new Panel
            {
                Location = new Point(leftOffset, 0),
                Size = new Size(260, 100),
                BackColor = bgColor
            };

            Label lblTitle = new Label
            {
                Location = new Point(15, 15),
                AutoSize = true,
                Text = title,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(156, 163, 175)
            };

            Label lblValue = new Label
            {
                Location = new Point(15, 45),
                AutoSize = true,
                Text = value,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White
            };

            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);
            parent.Controls.Add(card);
        }
        private void ExportDataGridViewToCSV(DataGridView dgv, string defaultFilename)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV file (*.csv)|*.csv";
                sfd.FileName = defaultFilename;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
                        {
                            // Write Column Headers
                            for (int i = 0; i < dgv.Columns.Count; i++)
                            {
                                sw.Write(dgv.Columns[i].HeaderText + (i == dgv.Columns.Count - 1 ? "" : ","));
                            }
                            sw.WriteLine();

                            // Write Row Data
                            foreach (DataGridViewRow row in dgv.Rows)
                            {
                                if (row.IsNewRow) continue;
                                for (int i = 0; i < dgv.Columns.Count; i++)
                                {
                                    string value = row.Cells[i].Value?.ToString() ?? "";
                                    if (value.Contains(",")) value = $"\"{value}\"";
                                    sw.Write(value + (i == dgv.Columns.Count - 1 ? "" : ","));
                                }
                                sw.WriteLine();
                            }
                        }
                        MessageBox.Show("Report exported successfully as CSV!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}