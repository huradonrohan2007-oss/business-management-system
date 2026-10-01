using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
namespace BusinessManagement.App
{
    public partial class MainDashboardForm : Form
    {
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
            Label lblInfo = new Label
            {
                Location = new Point(30, 40),
                AutoSize = true,
                Text = "POS Checkout Terminal integrated here. (You can port your Form1 checkout controls here!)",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(209, 213, 219)
            };
            pnlContentArea.Controls.Add(lblInfo);
        }

        private void LoadZReportView()
        {
            DataGridView dgvZReport = new DataGridView
            {
                Location = new Point(30, 80),
                Size = new Size(1100, 450),
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
                dgvZReport.DataSource = repository.GetDailySalesSummary(DateTime.Today);
            }
            catch { }

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
    // --- Section 1: Registered Suppliers Grid ---
    Label lblSupTitle = new Label
    {
        Location = new Point(30, 20),
        AutoSize = true,
        Text = "🚚 Approved Supply Partners",
        Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold),
        ForeColor = Color.FromArgb(243, 244, 246)
    };
    pnlContentArea.Controls.Add(lblSupTitle);

    DataGridView dgvSuppliers = new DataGridView
    {
        Location = new Point(30, 55),
        Size = new Size(1100, 200),
        BackgroundColor = Color.FromArgb(24, 20, 37),
        BorderStyle = BorderStyle.None,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        ReadOnly = true,
        RowHeadersVisible = false
    };

    dgvSuppliers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
    dgvSuppliers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
    dgvSuppliers.EnableHeadersVisualStyles = false;
    dgvSuppliers.DefaultCellStyle.BackColor = Color.FromArgb(24, 20, 37);
    dgvSuppliers.DefaultCellStyle.ForeColor = Color.FromArgb(229, 231, 235);

    try
    {
        dgvSuppliers.DataSource = repository.GetAllSuppliers();
    }
    catch { }
    pnlContentArea.Controls.Add(dgvSuppliers);

    // --- Section 2: Restock Purchase Order Panel ---
    Panel pnlOrderBox = new Panel
    {
        Location = new Point(30, 275),
        Size = new Size(1100, 260),
        BackColor = Color.FromArgb(24, 20, 37)
    };

    Label lblOrderTitle = new Label
    {
        Location = new Point(20, 20),
        AutoSize = true,
        Text = "📦 Dispatch Stock Restock Order",
        Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold),
        ForeColor = Color.FromArgb(243, 244, 246)
    };
    pnlOrderBox.Controls.Add(lblOrderTitle);

            // Supplier Dropdown
            Label lblSelSup = new Label { Location = new Point(20, 65), Text = "Select Supplier:", ForeColor = Color.FromArgb(156, 163, 175), AutoSize = true };
            ComboBox cmbSuppliers = new ComboBox { Location = new Point(20, 90), Size = new Size(320, 30), DropDownStyle = ComboBoxStyle.DropDownList };
    try
    {
        cmbSuppliers.DataSource = repository.GetAllSuppliers();
        cmbSuppliers.DisplayMember = "SupplierName";
        cmbSuppliers.ValueMember = "SupplierID";
    }
    catch { }
    pnlOrderBox.Controls.Add(lblSelSup);
    pnlOrderBox.Controls.Add(cmbSuppliers);

    // Product Dropdown
    Label lblSelProd = new Label { Location = new Point(370, 65), Text = "Select Product to Restock:", ForeColor = Color.FromArgb(156, 163, 175), AutoSize = true };
    ComboBox cmbProducts = new ComboBox { Location = new Point(370, 90), Size = new Size(320, 30), DropDownStyle = ComboBoxStyle.DropDownList };
    try
    {
        cmbProducts.DataSource = repository.GetAllProducts();
        cmbProducts.DisplayMember = "ProductName";
        cmbProducts.ValueMember = "ProductID";
    }
    catch { }
    pnlOrderBox.Controls.Add(lblSelProd);
    pnlOrderBox.Controls.Add(cmbProducts);

    // Quantity Input
    Label lblQty = new Label { Location = new Point(720, 65), Text = "Order Quantity:", ForeColor = Color.FromArgb(156, 163, 175), AutoSize = true };
    NumericUpDown numQuantity = new NumericUpDown { Location = new Point(720, 90), Size = new Size(150, 30), Minimum = 1, Maximum = 1000, Value = 20 };
    pnlOrderBox.Controls.Add(lblQty);
    pnlOrderBox.Controls.Add(numQuantity);

    // Submit Order Button
    Button btnOrder = new Button
    {
        Location = new Point(20, 150),
        Size = new Size(220, 45),
        Text = "🚀 Submit Purchase Order",
        BackColor = Color.FromArgb(79, 70, 229),
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        Cursor = Cursors.Hand,
        Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold)
    };
    btnOrder.FlatAppearance.BorderSize = 0;

    btnOrder.Click += (s, e) =>
    {
        try
        {
            if (cmbSuppliers.SelectedValue != null && cmbProducts.SelectedValue != null)
            {
                long supplierId = Convert.ToInt64(cmbSuppliers.SelectedValue);
                string supplierName = cmbSuppliers.Text;
                int productId = Convert.ToInt32(cmbProducts.SelectedValue);
                int qty = (int)numQuantity.Value;

                // Calculate estimated cost or let user pass value
                decimal unitCost = 25.00m; // Default estimate
                decimal totalCost = qty * unitCost;

                repository.PlaceRestockOrder(supplierId, supplierName, productId, qty, totalCost);
                MessageBox.Show("Purchase order successfully dispatched and inventory stock updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error placing order: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    };

    pnlOrderBox.Controls.Add(btnOrder);
    pnlContentArea.Controls.Add(pnlOrderBox);
}

        private void LoadDashboardView()
        {
            // --- 1. TOP TIER: Sales Trend Chart with Gradients & Hover Effects ---
            Chart salesChart = new Chart
            {
                Location = new Point(30, 20),
                Size = new Size(1100, 220),
                BackColor = Color.FromArgb(24, 20, 37)
            };

            ChartArea chartArea = new ChartArea("SalesArea");
            chartArea.BackColor = Color.FromArgb(24, 20, 37);
            chartArea.AxisX.LabelStyle.ForeColor = Color.FromArgb(156, 163, 175);
            chartArea.AxisY.LabelStyle.ForeColor = Color.FromArgb(156, 163, 175);
            chartArea.AxisX.LineColor = Color.FromArgb(55, 65, 81);
            chartArea.AxisY.LineColor = Color.FromArgb(55, 65, 81);
            chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(30, 27, 46);
            chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(30, 27, 46);
            salesChart.ChartAreas.Add(chartArea);

            Series series = new Series("Revenue")
            {
                ChartType = SeriesChartType.Area,
                Color = Color.FromArgb(79, 70, 229), // Rich Indigo
                BackSecondaryColor = Color.FromArgb(15, 13, 25), // Gradient fade
                BackGradientStyle = GradientStyle.TopBottom,
                BorderWidth = 3,
                BorderColor = Color.FromArgb(129, 140, 248)
            };

            // Sample data points for sales trend over the week
            series.Points.AddXY("Mon", 4200);
            series.Points.AddXY("Tue", 6800);
            series.Points.AddXY("Wed", 5100);
            series.Points.AddXY("Thu", 9400);
            series.Points.AddXY("Fri", 12300);
            series.Points.AddXY("Sat", 14850);
            series.Points.AddXY("Sun", 11200);

            salesChart.Series.Add(series);

            // Hover tooltip interactivity
            ToolTip chartTooltip = new ToolTip();
            salesChart.MouseMove += (s, e) =>
            {
                HitTestResult result = salesChart.HitTest(e.X, e.Y);
                if (result.ChartElementType == ChartElementType.DataPoint)
                {
                    DataPoint pt = series.Points[result.PointIndex];
                    chartTooltip.SetToolTip(salesChart, $"Day: {pt.AxisLabel}\nRevenue: Rs. {pt.YValues[0]:N2}");
                }
            };

            pnlContentArea.Controls.Add(salesChart);

            // --- 2. MIDDLE TIER: Top-Selling Products & "See More" Toggle ---
            Label lblProdHeader = new Label
            {
                Location = new Point(30, 255),
                AutoSize = true,
                Text = "🔥 Top-Selling Products",
                Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(243, 244, 246)
            };
            pnlContentArea.Controls.Add(lblProdHeader);

            Button btnSeeMore = new Button
            {
                Location = new Point(980, 250),
                Size = new Size(150, 30),
                Text = "View All Catalog ➔",
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.FromArgb(209, 213, 219),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnSeeMore.FlatAppearance.BorderSize = 0;
            btnSeeMore.Click += (s, e) => SwitchView("Inventory"); // Seamlessly jumps to full catalog view!
            pnlContentArea.Controls.Add(btnSeeMore);

            DataGridView dgvTopProducts = new DataGridView
            {
                Location = new Point(30, 290),
                Size = new Size(1100, 140),
                BackgroundColor = Color.FromArgb(24, 20, 37),
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                RowHeadersVisible = false
            };

            dgvTopProducts.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            dgvTopProducts.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvTopProducts.EnableHeadersVisualStyles = false;
            dgvTopProducts.DefaultCellStyle.BackColor = Color.FromArgb(24, 20, 37);
            dgvTopProducts.DefaultCellStyle.ForeColor = Color.FromArgb(229, 231, 235);

            try
            {
                dgvTopProducts.DataSource = repository.GetItemizedSalesSummary(DateTime.Today);
            }
            catch { }
            pnlContentArea.Controls.Add(dgvTopProducts);

            // --- 3. BOTTOM TIER: Quick Supply Checkout Strip ---
            Panel pnlBottomCheckout = new Panel
            {
                Location = new Point(30, 445),
                Size = new Size(1100, 95),
                BackColor = Color.FromArgb(24, 20, 37)
            };

            Label lblBottomTitle = new Label
            {
                Location = new Point(20, 15),
                AutoSize = true,
                Text = "⚡ Quick Supply Restock & Supplier Dispatch",
                Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(243, 244, 246)
            };
            pnlBottomCheckout.Controls.Add(lblBottomTitle);

            Button btnJumpSuppliers = new Button
            {
                Location = new Point(20, 45),
                Size = new Size(240, 35),
                Text = "Open Supplier Hub 🚚",
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold)
            };
            btnJumpSuppliers.FlatAppearance.BorderSize = 0;
            btnJumpSuppliers.Click += (s, e) => SwitchView("Suppliers");
            pnlBottomCheckout.Controls.Add(btnJumpSuppliers);

            pnlContentArea.Controls.Add(pnlBottomCheckout);
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
    }
}