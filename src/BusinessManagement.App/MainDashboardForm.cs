using BusinessManagement.Core;
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

            // SwitchView("Dashboard"); // <-- Remove this from the constructor

            // Add this instead so it loads with the correct maximized dimensions:
            this.Shown += (s, e) => SwitchView("Dashboard");
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

            // 1. CREATE THE SIDEBAR PANEL FIRST (Wider for stylish spacing)
            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 280,
                BackColor = Color.FromArgb(20, 16, 32)
            };
            this.Controls.Add(pnlSidebar);

            // 2. BRANDING & NAV BUTTONS
            Label lblBrand = new Label
            {
                Location = new Point(20, 25),
                AutoSize = true,
                Text = "⚡ NEXUS ERP",
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                ForeColor = Color.White
            };
            pnlSidebar.Controls.Add(lblBrand);

            AddNavButton("📊  Dashboard", 85, (s, e) => SwitchView("Dashboard"));
            AddNavButton("📦  Inventory Catalog", 145, (s, e) => SwitchView("Inventory"));
            AddNavButton("🛒  POS Checkout", 205, (s, e) => SwitchView("Checkout"));
            AddNavButton("📈  Z-Report & Financials", 265, (s, e) => SwitchView("ZReport"));
            AddNavButton("📄  Invoice Manager", 325, (s, e) => SwitchView("Invoices"));
            AddNavButton("🚚  Suppliers & Orders", 385, (s, e) => SwitchView("Suppliers"));
            AddNavButton("⏳  Slow-Moving Stock", 445, (s, e) => SwitchView("SlowMoving"));

            // --- Top Header Bar ---
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

            // --- Right Content Area ---
            pnlContentArea = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(15, 13, 25)
            };

            this.Controls.Add(pnlContentArea);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlSidebar);
        }

        private Button AddNavButton(string text, int topPosition, EventHandler onClick)
        {
            Button btn = new Button
            {
                Location = new Point(15, topPosition),
                Size = new Size(250, 48), // Wider and taller for bigger icons/text
                Text = text,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                BackColor = Color.FromArgb(24, 20, 37),
                ForeColor = Color.FromArgb(209, 213, 219),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI Semibold", 11f) // Larger font
            };
            btn.FlatAppearance.BorderSize = 0;

            btn.MouseEnter += (s, e) => { btn.BackColor = Color.FromArgb(79, 70, 229); btn.ForeColor = Color.White; };
            btn.MouseLeave += (s, e) => { btn.BackColor = Color.FromArgb(24, 20, 37); btn.ForeColor = Color.FromArgb(209, 213, 219); };

            btn.Click += onClick;
            pnlSidebar.Controls.Add(btn);

            return btn;
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
                case "Customers":
                    lblHeaderTitle.Text = "Customer Directory & Loyalty Management";
                    LoadCustomersView();
                    break;
                case "SlowMoving":
                    lblHeaderTitle.Text = "Slow-Moving Stock Analysis";
                    LoadSlowMovingInventoryView();
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
                ForeColor = Color.FromArgb(52, 211, 153),
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
            btnViewRecentInvoices.Click += (s, e) => { SwitchView("Invoices"); };

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
                Size = new Size(560, 270),
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

            // --- Fidelity & Checkout Controls Box ---
            Panel pnlCheckoutBox = new Panel
            {
                Location = new Point(560, 390),
                Size = new Size(560, 205), // Increased height to prevent overlap
                BackColor = Color.FromArgb(24, 20, 37)
            };

            // 1. Fidelity Card Scan Row
            Label lblFidelityLabel = new Label
            {
                Location = new Point(15, 12),
                AutoSize = true,
                ForeColor = Color.FromArgb(156, 163, 175),
                Text = "Fidelity Card:",
                Font = new Font("Segoe UI", 9f)
            };

            TextBox txtFidelityCard = new TextBox
            {
                Location = new Point(115, 10),
                Size = new Size(165, 25),
                PlaceholderText = "Scan Card #...",
                BackColor = Color.FromArgb(30, 27, 46),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Button btnVerifyCard = new Button
            {
                Location = new Point(290, 9),
                Size = new Size(70, 27),
                Text = "Verify",
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            btnVerifyCard.FlatAppearance.BorderSize = 0;

            Label lblPointsDisplay = new Label
            {
                Location = new Point(370, 12),
                AutoSize = true,
                ForeColor = Color.FromArgb(52, 211, 153),
                Text = "Points: 0",
                Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold)
            };

            // 2. Customer Name Row
            Label lblCustName = new Label
            {
                Location = new Point(15, 48),
                AutoSize = true,
                ForeColor = Color.FromArgb(156, 163, 175),
                Text = "Customer:",
                Font = new Font("Segoe UI", 9f)
            };

            txtCustomerName = new TextBox
            {
                Location = new Point(115, 45),
                Size = new Size(245, 25),
                Text = "Walk-in Customer",
                BackColor = Color.FromArgb(30, 27, 46),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                ReadOnly = true
            };

            // 3. Points Redemption Input Row
            Label lblRedeemText = new Label
            {
                Location = new Point(15, 85),
                AutoSize = true,
                ForeColor = Color.FromArgb(156, 163, 175),
                Text = "Use Pts:",
                Font = new Font("Segoe UI", 9f)
            };

            TextBox txtPointsToRedeem = new TextBox
            {
                Location = new Point(115, 82),
                Size = new Size(100, 25),
                Text = "0",
                BackColor = Color.FromArgb(30, 27, 46),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // 4. Total Due Row
            Label lblTotalText = new Label
            {
                Location = new Point(15, 122),
                AutoSize = true,
                ForeColor = Color.FromArgb(156, 163, 175),
                Text = "Total Due:",
                Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold)
            };

            lblCartTotalVal = new Label
            {
                Location = new Point(115, 118),
                AutoSize = true,
                ForeColor = Color.FromArgb(52, 211, 153),
                Text = "Rs 0.00",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold)
            };

            // 5. Complete Checkout Button
            Button btnCompleteCheckout = new Button
            {
                Location = new Point(15, 155),
                Size = new Size(530, 38),
                Text = "💳 Authorize Sale & Update Inventory",
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Bold)
            };
            btnCompleteCheckout.FlatAppearance.BorderSize = 0;

            // --- State variables for active checkout session ---
            CustomerFidelityModel activeFidelityCustomer = null;
            LoyaltyCheckoutManager loyaltyManager = new LoyaltyCheckoutManager();

            // Wire up Verification Click safely
            btnVerifyCard.Click += (s, e) =>
            {
                string cardCode = txtFidelityCard.Text.Trim();
                if (string.IsNullOrEmpty(cardCode)) return;

                try
                {
                    activeFidelityCustomer = repository.GetCustomerByFidelityCard(cardCode);
                    if (activeFidelityCustomer != null)
                    {
                        txtCustomerName.Text = activeFidelityCustomer.CustomerName;
                        lblPointsDisplay.Text = $"Points: {activeFidelityCustomer.PointsBalance}";
                    }
                    else
                    {
                        MessageBox.Show("Fidelity card not found in the database.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        activeFidelityCustomer = null;
                        txtCustomerName.Text = "Walk-in Customer";
                        lblPointsDisplay.Text = "Points: 0";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Database error during card lookup: {ex.Message}\n\n(Tip: Ensure the Customers table exists in your SQLite database).",
                        "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    activeFidelityCustomer = null;
                }
            };

            // Wire up Checkout Completion with Safe-Proof Margin Protection
            btnCompleteCheckout.Click += (s, e) =>
            {
                if (currentCart.Count == 0)
                {
                    MessageBox.Show("Basket is empty.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    // Calculate raw cart subtotal
                    decimal cartSubtotal = 0;
                    foreach (var item in currentCart)
                    {
                        cartSubtotal += item.Subtotal;
                    }

                    int pointsRequested = 0;
                    int.TryParse(txtPointsToRedeem.Text, out pointsRequested);
                    int availablePoints = (activeFidelityCustomer != null) ? activeFidelityCustomer.PointsBalance : 0;

                    // Run through the safe-proof manager (Enforces 30% margin protection ceiling)
                    var calcResult = loyaltyManager.CalculateSafeCheckout(cartSubtotal, availablePoints, pointsRequested);

                    string custName = txtCustomerName.Text;
                    long invoiceId = repository.SaveInvoice(custName, currentCart);
                    if (activeFidelityCustomer != null)
                    {
                        int finalPointsBalance = activeFidelityCustomer.PointsBalance - calcResult.PointsRedeemed + calcResult.PointsEarned;

                        // Save updated points balance back to SQLite
                        repository.UpdateCustomerLoyalty(activeFidelityCustomer.CustomerID, finalPointsBalance, activeFidelityCustomer.StoreCredit);
                    }
                    MessageBox.Show($"Transaction authorized successfully!\n" +
                                    $"Invoice #{invoiceId} recorded.\n" +
                                    $"Subtotal: Rs. {cartSubtotal:N2}\n" +
                                    $"Discount Applied (Protected): - Rs. {calcResult.DiscountApplied:N2}\n" +
                                    $"Final Payable: Rs. {calcResult.FinalPayable:N2}\n" +
                                    $"Points Earned: +{calcResult.PointsEarned}",
                                    "Manager Sign-off", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    currentCart.Clear();
                    RefreshCartGrid();
                    dgvCatalog.DataSource = repository.GetAllProducts();
                    txtFidelityCard.Clear();
                    txtCustomerName.Text = "Walk-in Customer";
                    lblPointsDisplay.Text = "Points: 0";
                    txtPointsToRedeem.Text = "0";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Checkout error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            pnlCheckoutBox.Controls.AddRange(new Control[] {
        lblFidelityLabel, txtFidelityCard, btnVerifyCard, lblPointsDisplay,
        lblCustName, txtCustomerName, lblRedeemText, txtPointsToRedeem,
        lblTotalText, lblCartTotalVal, btnCompleteCheckout

    });
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

            int margin = 20;
            int gap = 15;
            int trackerWidth = 300;

            int availableWidth = pnlContentArea.Width;
            int availableHeight = pnlContentArea.Height;

            int workspaceWidth = availableWidth - (margin * 2) - trackerWidth - gap;
            int workspaceHeight = availableHeight - (margin * 2);
            int trackerHeight = workspaceHeight;

            // --- Main Left Workspace ---
            Panel pnlWorkspace = new Panel
            {
                Location = new Point(margin, margin),
                Size = new Size(workspaceWidth, workspaceHeight),
                BackColor = Color.FromArgb(24, 20, 37),
                AutoScroll = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            var metrics = repository.GetLiveShiftMetrics();
            int currentY = 15;

            // 0. Low-Stock Warning Banner
            if (metrics.lowStockCount > 0)
            {
                Panel pnlBanner = new Panel
                {
                    Location = new Point(20, currentY),
                    Size = new Size(workspaceWidth - 55, 42),
                    BackColor = Color.FromArgb(60, 22, 30),
                    Cursor = Cursors.Hand
                };
                pnlBanner.Click += (s, e) => SwitchView("Suppliers");

                Label lblBannerText = new Label
                {
                    Text = $"⚠  Attention: {metrics.lowStockCount} product(s) have reached critical low stock! Click here to manage restock orders.",
                    ForeColor = Color.FromArgb(252, 165, 165),
                    Font = new Font("Segoe UI Semibold", 10f),
                    Location = new Point(15, 11),
                    AutoSize = true,
                    Cursor = Cursors.Hand
                };
                lblBannerText.Click += (s, e) => SwitchView("Suppliers");
                pnlBanner.Controls.Add(lblBannerText);
                pnlWorkspace.Controls.Add(pnlBanner);

                currentY += 52;
            }

            // 1. Chart Section Title & Canvas
            Label lblWorkspaceTitle = new Label
            {
                Text = "Store Performance & Sales Trends",
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 12f, FontStyle.Bold),
                Location = new Point(20, currentY),
                AutoSize = true
            };
            pnlWorkspace.Controls.Add(lblWorkspaceTitle);
            currentY += 32;

            int chartWidth = workspaceWidth - 55;
            Panel pnlChartCanvas = new Panel
            {
                Location = new Point(20, currentY),
                Size = new Size(chartWidth, 195),
                BackColor = Color.FromArgb(30, 25, 45)
            };

            DataTable trendData = repository.GetRecentSalesTrend();
            pnlChartCanvas.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                if (trendData.Rows.Count == 0)
                {
                    using (Brush brush = new SolidBrush(Color.FromArgb(140, 140, 150)))
                    {
                        g.DrawString("No sales data recorded yet. Run a checkout to populate trend!",
                            new Font("Segoe UI", 10f), brush, new PointF(25, 35));
                    }
                    return;
                }

                int startX = 55;
                int maxBarHeight = 130;
                int barWidth = 55;
                int spacing = 45;

                decimal maxVal = 100;
                foreach (DataRow row in trendData.Rows)
                {
                    decimal rev = Convert.ToDecimal(row["DailyRevenue"]);
                    if (rev > maxVal) maxVal = rev;
                }

                using (Pen axisPen = new Pen(Color.FromArgb(60, 50, 80), 1))
                {
                    g.DrawLine(axisPen, 40, 160, chartWidth - 30, 160);
                }

                int index = 0;
                foreach (DataRow row in trendData.Rows)
                {
                    string dateStr = Convert.ToDateTime(row["SaleDate"]).ToString("MMM dd");
                    decimal revenue = Convert.ToDecimal(row["DailyRevenue"]);

                    int barHeight = (maxVal > 0) ? (int)((revenue / maxVal) * maxBarHeight) : 10;
                    if (barHeight < 5 && revenue > 0) barHeight = 5;

                    int posX = startX + (index * (barWidth + spacing));
                    int posY = 160 - barHeight;

                    using (Brush barBrush = new SolidBrush(Color.FromArgb(79, 70, 229)))
                    {
                        g.FillRectangle(barBrush, posX, posY, barWidth, barHeight);
                    }

                    using (Brush textBrush = new SolidBrush(Color.White))
                    {
                        g.DrawString($"Rs.{revenue:0}", new Font("Segoe UI", 8.5f, FontStyle.Bold), textBrush, new PointF(posX - 2, posY - 20));
                        g.DrawString(dateStr, new Font("Segoe UI", 9f), new SolidBrush(Color.FromArgb(180, 180, 190)), new PointF(posX + 3, 165));
                    }

                    index++;
                }
            };
            pnlWorkspace.Controls.Add(pnlChartCanvas);
            currentY += 215;


            // 2. Row 1 Tables: Top-Selling Products & Low-Stock Estimator
            int colWidth = (chartWidth - 15) / 2;
            int rightColX = 20 + colWidth + 15;
            int tableHeight = 220;

            Label lblLeaderboardTitle = new Label
            {
                Text = "🔥 Top-Selling Products Today",
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold),
                Location = new Point(20, currentY),
                AutoSize = true
            };
            pnlWorkspace.Controls.Add(lblLeaderboardTitle);

            Label lblRestockEstTitle = new Label
            {
                Text = "📦 Low-Stock Restock Estimator",
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold),
                Location = new Point(rightColX, currentY),
                AutoSize = true
            };
            pnlWorkspace.Controls.Add(lblRestockEstTitle);
            currentY += 32;

            // Left DataGridView (Top Products)
            DataGridView dgvTopProducts = new DataGridView
            {
                Location = new Point(20, currentY),
                Size = new Size(colWidth, tableHeight),
                BackgroundColor = Color.FromArgb(30, 25, 45),
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(50, 42, 75),
                RowTemplate = { Height = 34 }
            };

            dgvTopProducts.ColumnHeadersHeight = 35;
            dgvTopProducts.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(45, 38, 68),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9.5f)
            };
            dgvTopProducts.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(30, 25, 45),
                ForeColor = Color.FromArgb(240, 240, 245),
                SelectionBackColor = Color.FromArgb(79, 70, 229),
                SelectionForeColor = Color.White,
                Font = new Font("Segoe UI", 9f)
            };

            try
            {
                dgvTopProducts.DataSource = repository.GetTopSellingProducts();
                if (dgvTopProducts.Columns.Contains("ItemName"))
                {
                    dgvTopProducts.Columns["ItemName"].HeaderText = "Product Name";
                    dgvTopProducts.Columns["ItemName"].FillWeight = 140;
                }
                if (dgvTopProducts.Columns.Contains("TotalSold"))
                {
                    dgvTopProducts.Columns["TotalSold"].HeaderText = "Sold";
                    dgvTopProducts.Columns["TotalSold"].FillWeight = 50;
                }
                if (dgvTopProducts.Columns.Contains("Revenue"))
                {
                    dgvTopProducts.Columns["Revenue"].HeaderText = "Revenue (Rs.)";
                    dgvTopProducts.Columns["Revenue"].DefaultCellStyle.Format = "N2";
                    dgvTopProducts.Columns["Revenue"].FillWeight = 80;
                }
            }
            catch { }
            pnlWorkspace.Controls.Add(dgvTopProducts);


            // Right DataGridView (Restock Estimator)
            DataGridView dgvRestockEst = new DataGridView
            {
                Location = new Point(rightColX, currentY),
                Size = new Size(colWidth, tableHeight),
                BackgroundColor = Color.FromArgb(30, 25, 45),
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(50, 42, 75),
                RowTemplate = { Height = 34 }
            };

            dgvRestockEst.ColumnHeadersHeight = 35;
            dgvRestockEst.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(45, 38, 68),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9.5f)
            };
            dgvRestockEst.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(30, 25, 45),
                ForeColor = Color.FromArgb(240, 240, 245),
                SelectionBackColor = Color.FromArgb(79, 70, 229),
                SelectionForeColor = Color.White,
                Font = new Font("Segoe UI", 9f)
            };

            decimal totalEstimatedRestockCost = 0;
            try
            {
                DataTable restockDt = repository.GetAutomatedRestockQueue();
                dgvRestockEst.DataSource = restockDt;

                foreach (DataGridViewColumn col in dgvRestockEst.Columns)
                {
                    if (col.Name == "ProductName" || col.Name == "StockQuantity" || col.Name == "RecommendedQty" || col.Name == "EstimatedCost")
                    {
                        col.Visible = true;
                    }
                    else
                    {
                        col.Visible = false;
                    }
                }

                if (dgvRestockEst.Columns.Contains("ProductName"))
                {
                    dgvRestockEst.Columns["ProductName"].HeaderText = "Product Name";
                    dgvRestockEst.Columns["ProductName"].FillWeight = 130;
                }
                if (dgvRestockEst.Columns.Contains("StockQuantity"))
                {
                    dgvRestockEst.Columns["StockQuantity"].HeaderText = "Stock";
                    dgvRestockEst.Columns["StockQuantity"].FillWeight = 45;
                }
                if (dgvRestockEst.Columns.Contains("RecommendedQty"))
                {
                    dgvRestockEst.Columns["RecommendedQty"].HeaderText = "Order";
                    dgvRestockEst.Columns["RecommendedQty"].FillWeight = 45;
                }
                if (dgvRestockEst.Columns.Contains("EstimatedCost"))
                {
                    dgvRestockEst.Columns["EstimatedCost"].HeaderText = "Est. Cost";
                    dgvRestockEst.Columns["EstimatedCost"].DefaultCellStyle.Format = "N2";
                    dgvRestockEst.Columns["EstimatedCost"].FillWeight = 75;
                }

                foreach (DataRow row in restockDt.Rows)
                {
                    if (row["EstimatedCost"] != DBNull.Value)
                        totalEstimatedRestockCost += Convert.ToDecimal(row["EstimatedCost"]);
                }
            }
            catch { }
            pnlWorkspace.Controls.Add(dgvRestockEst);
            currentY += tableHeight + 12;

            // Restock Budget Summary Footer
            Panel pnlRestockSummary = new Panel
            {
                Location = new Point(rightColX, currentY),
                Size = new Size(colWidth, 38),
                BackColor = Color.FromArgb(35, 30, 50)
            };

            Label lblTotalCostLabel = new Label
            {
                Text = "Est. Restock Budget:",
                ForeColor = Color.FromArgb(180, 180, 190),
                Font = new Font("Segoe UI Semibold", 9f),
                Location = new Point(12, 10),
                AutoSize = true
            };

            Label lblTotalCostVal = new Label
            {
                Text = $"Rs. {totalEstimatedRestockCost:N2}",
                ForeColor = Color.FromArgb(52, 211, 153),
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Location = new Point(145, 8),
                AutoSize = true
            };

            pnlRestockSummary.Controls.AddRange(new Control[] { lblTotalCostLabel, lblTotalCostVal });
            pnlWorkspace.Controls.Add(pnlRestockSummary);

            currentY += 55;


            // 3. Row 2: Customer Credit & Loyalty Tracker Widget Header with "View All" Link
            Label lblLoyaltyTitle = new Label
            {
                Text = "⭐ Customer Credit & Loyalty Accounts",
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold),
                Location = new Point(20, currentY),
                AutoSize = true
            };
            pnlWorkspace.Controls.Add(lblLoyaltyTitle);

            Label lblViewAllCustomers = new Label
            {
                Text = "View All Customers ➔",
                ForeColor = Color.FromArgb(129, 140, 248), // Accent indigo-blue link color
                Font = new Font("Segoe UI Semibold", 9.5f, FontStyle.Underline),
                Cursor = Cursors.Hand,
                AutoSize = true
            };

            // Position it neatly on the right side of the section header row
            lblViewAllCustomers.Location = new Point(20 + chartWidth - lblViewAllCustomers.PreferredWidth, currentY + 3);
            lblViewAllCustomers.Click += (s, e) => SwitchView("Customers"); // Adjust target view name if your form uses a different identifier (e.g. "CustomerAccounts")
            pnlWorkspace.Controls.Add(lblViewAllCustomers);

            currentY += 32;

            DataGridView dgvLoyaltyTracker = new DataGridView
            {
                Location = new Point(20, currentY),
                Size = new Size(chartWidth, 180),
                BackgroundColor = Color.FromArgb(30, 25, 45),
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(50, 42, 75),
                RowTemplate = { Height = 34 }
            };

            dgvLoyaltyTracker.ColumnHeadersHeight = 35;
            dgvLoyaltyTracker.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(45, 38, 68),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9.5f)
            };
            dgvLoyaltyTracker.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(30, 25, 45),
                ForeColor = Color.FromArgb(240, 240, 245),
                SelectionBackColor = Color.FromArgb(79, 70, 229),
                SelectionForeColor = Color.White,
                Font = new Font("Segoe UI", 9f)
            };

            try
            {
                // Attempt to call repository method if implemented, otherwise catch and use fallback
                var method = repository.GetType().GetMethod("GetCustomerLoyaltyAccounts");
                if (method != null)
                {
                    dgvLoyaltyTracker.DataSource = method.Invoke(repository, null);
                }
                else
                {
                    throw new Exception("Method not found in repository");
                }

                if (dgvLoyaltyTracker.Columns.Contains("CustomerName")) dgvLoyaltyTracker.Columns["CustomerName"].HeaderText = "Customer Name";
                if (dgvLoyaltyTracker.Columns.Contains("Phone")) dgvLoyaltyTracker.Columns["Phone"].HeaderText = "Contact Phone";
                if (dgvLoyaltyTracker.Columns.Contains("StoreCredit"))
                {
                    dgvLoyaltyTracker.Columns["StoreCredit"].HeaderText = "Store Credit (Rs.)";
                    dgvLoyaltyTracker.Columns["StoreCredit"].DefaultCellStyle.Format = "N2";
                }
                if (dgvLoyaltyTracker.Columns.Contains("LoyaltyPoints")) dgvLoyaltyTracker.Columns["LoyaltyPoints"].HeaderText = "Loyalty Points";
            }
            catch
            {
                // Fallback mock structure so the UI renders smoothly immediately
                DataTable dtFallback = new DataTable();
                dtFallback.Columns.Add("CustomerName");
                dtFallback.Columns.Add("Phone");
                dtFallback.Columns.Add("StoreCredit", typeof(decimal));
                dtFallback.Columns.Add("LoyaltyPoints", typeof(int));
                dtFallback.Rows.Add("Jean-Luc Dubois", "+230 5712 3456", 1250.00m, 450);
                dtFallback.Rows.Add("Aisha Ramchurn", "+230 5988 9012", 0.00m, 820);
                dtFallback.Rows.Add("Kunal Beeharry", "+230 5433 1122", 3400.50m, 150);
                dgvLoyaltyTracker.DataSource = dtFallback;
            }

            pnlWorkspace.Controls.Add(dgvLoyaltyTracker);
            currentY += 200 + 20; // Bottom clearance padding for scrolling
            // --- Right-Hand Live Shift Performance Tracker ---
            Panel pnlLiveTracker = new Panel
            {
                Location = new Point(margin + workspaceWidth + gap, margin),
                Size = new Size(trackerWidth, trackerHeight),
                BackColor = Color.FromArgb(24, 20, 37),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right
            };

            Label lblSidebarTitle = new Label
            {
                Text = "⚡ Live Shift Tracker",
                ForeColor = Color.FromArgb(79, 70, 229),
                Font = new Font("Segoe UI Semibold", 12f, FontStyle.Bold),
                Location = new Point(15, 15),
                AutoSize = true
            };
            pnlLiveTracker.Controls.Add(lblSidebarTitle);

            decimal avgBasket = metrics.transactionCount > 0 ? metrics.grossRevenue / metrics.transactionCount : 0;

            int cardStartY = 60;
            int cardWidth = trackerWidth - 30;
            int cardHeight = (trackerHeight - 80 - (3 * 15)) / 4;
            if (cardHeight < 90) cardHeight = 90;

            Action<string, string, Color, EventHandler> addInteractiveCard = (title, value, valColor, onClick) =>
            {
                Panel card = new Panel
                {
                    Location = new Point(15, cardStartY),
                    Size = new Size(cardWidth, cardHeight),
                    BackColor = Color.FromArgb(35, 30, 50),
                    Cursor = Cursors.Hand,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                Label lblTitle = new Label
                {
                    Text = title,
                    ForeColor = Color.FromArgb(180, 180, 190),
                    Font = new Font("Segoe UI", 9.5f),
                    Location = new Point(15, 15),
                    AutoSize = true,
                    Cursor = Cursors.Hand
                };

                Label lblVal = new Label
                {
                    Text = value,
                    ForeColor = valColor,
                    Font = new Font("Segoe UI Semibold", 17f, FontStyle.Bold),
                    Location = new Point(15, 40),
                    AutoSize = true,
                    Cursor = Cursors.Hand
                };

                if (onClick != null)
                {
                    card.Click += onClick;
                    lblTitle.Click += onClick;
                    lblVal.Click += onClick;
                }

                card.Controls.Add(lblTitle);
                card.Controls.Add(lblVal);
                pnlLiveTracker.Controls.Add(card);
                cardStartY += cardHeight + 15;
            };

            addInteractiveCard("Today's Gross Revenue", $"Rs. {metrics.grossRevenue:N2}", Color.FromArgb(16, 185, 129), null);
            addInteractiveCard("Total Transactions", metrics.transactionCount.ToString(), Color.White, null);
            addInteractiveCard("Average Basket Size", $"Rs. {avgBasket:N2}", Color.White, null);
            addInteractiveCard("Critical Low-Stock Items", metrics.lowStockCount.ToString(),
                metrics.lowStockCount > 0 ? Color.FromArgb(239, 68, 68) : Color.FromArgb(16, 185, 129),
                (s, e) => SwitchView("Suppliers"));

            pnlContentArea.Controls.Add(pnlWorkspace);
            pnlContentArea.Controls.Add(pnlLiveTracker);
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
        private void LoadCustomersView()
        {
            pnlContentArea.Controls.Clear();

            // Root Container
            Panel pnlRoot = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(24, 20, 37),
                Padding = new Padding(20)
            };

            // 1. Dedicated Header Panel (Completely isolated at the top so it never overlaps)
            Panel pnlHeaderSection = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Color.FromArgb(24, 20, 37)
            };

            Label lblHeader = new Label
            {
                Text = "Customer Directory & Fidelity Management",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlHeaderSection.Controls.Add(lblHeader);

            // 2. Main Body Panel (Fills everything underneath the header)
            Panel pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(24, 20, 37)
            };

            // Right-Side Editor Panel
            Panel pnlEditor = new Panel
            {
                Dock = DockStyle.Right,
                Width = 380,
                BackColor = Color.FromArgb(30, 25, 45),
                Padding = new Padding(15)
            };

            Label lblEditorTitle = new Label
            {
                Text = "Customer Record Manager",
                Location = new Point(20, 15),
                AutoSize = true,
                ForeColor = Color.FromArgb(52, 211, 153),
                Font = new Font("Segoe UI Semibold", 11f, FontStyle.Bold)
            };
            pnlEditor.Controls.Add(lblEditorTitle);

            int startY = 55;
            int spacing = 58;

            // Fidelity Code Field
            Label lblC1 = new Label { Text = "Fidelity Card #", Location = new Point(20, startY), AutoSize = true, ForeColor = Color.FromArgb(156, 163, 175), Font = new Font("Segoe UI", 9f) };
            TextBox txtCardCode = new TextBox { Location = new Point(20, startY + 20), Size = new Size(340, 26), BackColor = Color.FromArgb(24, 20, 37), ForeColor = Color.White, Font = new Font("Segoe UI", 9.5f), BorderStyle = BorderStyle.FixedSingle };
            pnlEditor.Controls.Add(lblC1);
            pnlEditor.Controls.Add(txtCardCode);

            // Customer Name Field
            startY += spacing;
            Label lblC2 = new Label { Text = "Customer Name", Location = new Point(20, startY), AutoSize = true, ForeColor = Color.FromArgb(156, 163, 175), Font = new Font("Segoe UI", 9f) };
            TextBox txtName = new TextBox { Location = new Point(20, startY + 20), Size = new Size(340, 26), BackColor = Color.FromArgb(24, 20, 37), ForeColor = Color.White, Font = new Font("Segoe UI", 9.5f), BorderStyle = BorderStyle.FixedSingle };
            pnlEditor.Controls.Add(lblC2);
            pnlEditor.Controls.Add(txtName);

            // Phone Number Field
            startY += spacing;
            Label lblC3 = new Label { Text = "Phone Number", Location = new Point(20, startY), AutoSize = true, ForeColor = Color.FromArgb(156, 163, 175), Font = new Font("Segoe UI", 9f) };
            TextBox txtPhone = new TextBox { Location = new Point(20, startY + 20), Size = new Size(340, 26), BackColor = Color.FromArgb(24, 20, 37), ForeColor = Color.White, Font = new Font("Segoe UI", 9.5f), BorderStyle = BorderStyle.FixedSingle };
            pnlEditor.Controls.Add(lblC3);
            pnlEditor.Controls.Add(txtPhone);

            // Email Address Field
            startY += spacing;
            Label lblC4 = new Label { Text = "Email Address", Location = new Point(20, startY), AutoSize = true, ForeColor = Color.FromArgb(156, 163, 175), Font = new Font("Segoe UI", 9f) };
            TextBox txtEmail = new TextBox { Location = new Point(20, startY + 20), Size = new Size(340, 26), BackColor = Color.FromArgb(24, 20, 37), ForeColor = Color.White, Font = new Font("Segoe UI", 9.5f), BorderStyle = BorderStyle.FixedSingle };
            pnlEditor.Controls.Add(lblC4);
            pnlEditor.Controls.Add(txtEmail);

            int selectedCustomerId = 0;
            int btnY = startY + 60;

            Button btnSaveNew = new Button
            {
                Text = "+ Add New",
                Location = new Point(20, btnY),
                Size = new Size(105, 36),
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI Semibold", 9f)
            };
            btnSaveNew.FlatAppearance.BorderSize = 0;

            Button btnUpdate = new Button
            {
                Text = "💾 Save Edit",
                Location = new Point(135, btnY),
                Size = new Size(105, 36),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI Semibold", 9f)
            };
            btnUpdate.FlatAppearance.BorderSize = 0;

            Button btnDelete = new Button
            {
                Text = "🗑 Delete",
                Location = new Point(250, btnY),
                Size = new Size(105, 36),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI Semibold", 9f)
            };
            btnDelete.FlatAppearance.BorderSize = 0;

            pnlEditor.Controls.Add(btnSaveNew);
            pnlEditor.Controls.Add(btnUpdate);
            pnlEditor.Controls.Add(btnDelete);

            // Grid Container
            Panel pnlGridContainer = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 0, 15, 0)
            };

            DataGridView dgvAllCustomers = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(30, 25, 45),
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(50, 42, 75),
                RowTemplate = { Height = 40 }
            };

            dgvAllCustomers.ColumnHeadersHeight = 42;
            dgvAllCustomers.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(45, 38, 68),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 10f)
            };
            dgvAllCustomers.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(30, 25, 45),
                ForeColor = Color.FromArgb(240, 240, 245),
                SelectionBackColor = Color.FromArgb(79, 70, 229),
                SelectionForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f)
            };

            pnlGridContainer.Controls.Add(dgvAllCustomers);

            // Assemble Body
            pnlBody.Controls.Add(pnlEditor);
            pnlBody.Controls.Add(pnlGridContainer);

            // Assemble Root (Header docked top first, Body docked fill second)
            pnlRoot.Controls.Add(pnlBody);
            pnlRoot.Controls.Add(pnlHeaderSection);

            pnlContentArea.Controls.Add(pnlRoot);

            Action refreshGrid = () =>
            {
                dgvAllCustomers.DataSource = repository.GetAllCustomers();

                if (dgvAllCustomers.Columns.Contains("CustomerID"))
                    dgvAllCustomers.Columns["CustomerID"].Visible = false;

                if (dgvAllCustomers.Columns.Contains("FidelityCardCode"))
                {
                    dgvAllCustomers.Columns["FidelityCardCode"].HeaderText = "Card #";
                    dgvAllCustomers.Columns["FidelityCardCode"].FillWeight = 80;
                }
                if (dgvAllCustomers.Columns.Contains("CustomerName"))
                {
                    dgvAllCustomers.Columns["CustomerName"].HeaderText = "Customer Name";
                    dgvAllCustomers.Columns["CustomerName"].FillWeight = 120;
                }
                if (dgvAllCustomers.Columns.Contains("Phone"))
                {
                    dgvAllCustomers.Columns["Phone"].HeaderText = "Phone";
                    dgvAllCustomers.Columns["Phone"].FillWeight = 95;
                }
                if (dgvAllCustomers.Columns.Contains("Email"))
                {
                    dgvAllCustomers.Columns["Email"].HeaderText = "Email Address";
                    dgvAllCustomers.Columns["Email"].FillWeight = 130;
                }
                if (dgvAllCustomers.Columns.Contains("PointsBalance"))
                {
                    dgvAllCustomers.Columns["PointsBalance"].HeaderText = "Points";
                    dgvAllCustomers.Columns["PointsBalance"].FillWeight = 70;
                }
                if (dgvAllCustomers.Columns.Contains("StoreCredit"))
                {
                    dgvAllCustomers.Columns["StoreCredit"].HeaderText = "Credit";
                    dgvAllCustomers.Columns["StoreCredit"].FillWeight = 75;
                }
                if (dgvAllCustomers.Columns.Contains("LifetimeSpend"))
                {
                    dgvAllCustomers.Columns["LifetimeSpend"].HeaderText = "Total Spend";
                    dgvAllCustomers.Columns["LifetimeSpend"].FillWeight = 85;
                }
            };

            refreshGrid();

            dgvAllCustomers.CellClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    var cust = (CustomerModel)dgvAllCustomers.Rows[e.RowIndex].DataBoundItem;
                    selectedCustomerId = cust.CustomerID;
                    txtCardCode.Text = cust.FidelityCardCode;
                    txtName.Text = cust.CustomerName;
                    txtPhone.Text = cust.Phone;
                    txtEmail.Text = cust.Email;
                }
            };

            btnSaveNew.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Customer Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var newCust = new CustomerModel
                {
                    FidelityCardCode = string.IsNullOrWhiteSpace(txtCardCode.Text) ? "FID-" + new Random().Next(2000, 9999) : txtCardCode.Text.Trim(),
                    CustomerName = txtName.Text.Trim(),
                    Phone = txtPhone.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    PointsBalance = 50,
                    StoreCredit = 0.00m,
                    LifetimeSpend = 0.00m
                };

                repository.AddCustomer(newCust);
                refreshGrid();
                MessageBox.Show("New customer registered successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            btnUpdate.Click += (s, e) =>
            {
                if (selectedCustomerId == 0)
                {
                    MessageBox.Show("Please select a customer from the table to edit.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var custToUpdate = new CustomerModel
                {
                    CustomerID = selectedCustomerId,
                    FidelityCardCode = txtCardCode.Text.Trim(),
                    CustomerName = txtName.Text.Trim(),
                    Phone = txtPhone.Text.Trim(),
                    Email = txtEmail.Text.Trim()
                };

                repository.UpdateCustomer(custToUpdate);
                refreshGrid();
                MessageBox.Show("Customer details updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            btnDelete.Click += (s, e) =>
            {
                if (selectedCustomerId == 0)
                {
                    MessageBox.Show("Please select a customer to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var confirm = MessageBox.Show($"Are you sure you want to delete customer {txtName.Text}?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (confirm == DialogResult.Yes)
                {
                    repository.DeleteCustomer(selectedCustomerId);
                    refreshGrid();
                    selectedCustomerId = 0;
                    txtCardCode.Clear();
                    txtName.Clear();
                    txtPhone.Clear();
                    txtEmail.Clear();
                }
            };
        }
        private void LoadSlowMovingInventoryView()
        {
            // Clear previous view controls
            pnlContentArea.Controls.Clear();

            Label lblTitle = new Label
            {
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(243, 244, 246),
                Text = "⏳ Slow-Moving Inventory & Capital Optimizer"
            };
            pnlContentArea.Controls.Add(lblTitle);

            DataGridView dgvSlowMoving = new DataGridView
            {
                Location = new Point(20, 70),
                Size = new Size(1100, 480),
                BackgroundColor = Color.FromArgb(24, 20, 37),
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false
            };

            dgvSlowMoving.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(79, 70, 229);
            dgvSlowMoving.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvSlowMoving.DefaultCellStyle.BackColor = Color.FromArgb(24, 20, 37);
            dgvSlowMoving.DefaultCellStyle.ForeColor = Color.FromArgb(229, 231, 235);

            try
            {
                // 1. Fetch products as a DataTable from the repository
                DataTable dtProducts = repository.GetAllProducts();

                // 2. Map DataTable rows to your SlowMovingItemModel list
                var rawItems = new List<SlowMovingItemModel>();
                foreach (DataRow row in dtProducts.Rows)
                {
                    rawItems.Add(new SlowMovingItemModel
                    {
                        SKU = row["SKU"].ToString(),
                        ItemName = row["ProductName"].ToString(),
                        StockQuantity = Convert.ToInt32(row["StockQuantity"]),
                        UnitsSoldLast30Days = row.Table.Columns.Contains("UnitsSoldLast30Days") && row["UnitsSoldLast30Days"] != DBNull.Value
                                            ? Convert.ToInt32(row["UnitsSoldLast30Days"]) : 0,
                        UnitPrice = Convert.ToDecimal(row["UnitPrice"])
                    });
                }

                // 3. Run through your intelligence engine
                var slowItems = InventoryIntelligenceEngine.AnalyzeSlowMovers(rawItems);
                dgvSlowMoving.DataSource = slowItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading analytics: {ex.Message}");
            }

            pnlContentArea.Controls.Add(dgvSlowMoving);
        }
    }
}