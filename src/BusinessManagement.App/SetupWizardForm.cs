using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BusinessManagement.App
{
    public partial class SetupWizardForm : Form
    {
        private Panel mainCardPanel;
        private Label lblTitle, lblSubtitle, lblPreset, lblPrompt, lblStatus, lblVerticalInfo;
        private ComboBox cmbPresets;
        private TextBox txtDescription;
        private Button btnMatch, btnInitialize, btnClose;
        private System.Windows.Forms.Timer animationTimer;
        private List<StarNode> stars = new List<StarNode>();
        private List<DataPulse> pulses = new List<DataPulse>();
        private Random rand = new Random();
        private float rippleRadius = 0f;
        private int rippleWaitTicks = 0;

        public string SelectedProfile => cmbPresets?.SelectedItem?.ToString() ?? string.Empty;

        public SetupWizardForm()
        {
            // Maximum double-buffering performance to completely eradicate flicker
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.DoubleBuffered = true;
            this.UpdateStyles();

            SetupFullScreenUI();
            InitializeConstellation();

            // High-framerate ~65 FPS animation clock
            animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 15;
            animationTimer.Tick += (s, e) =>
            {
                UpdateConstellation();
                this.Invalidate();
            };
            animationTimer.Start();

            this.FormClosed += (s, e) => animationTimer.Stop();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Suppress default background clearing to prevent flashing
        }

        private class StarNode
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float VX { get; set; }
            public float VY { get; set; }
            public float Size { get; set; }
            public float Alpha { get; set; }
            public float AlphaSpeed { get; set; }
        }

        private class DataPulse
        {
            public StarNode StartStar { get; set; }
            public StarNode EndStar { get; set; }
            public float Progress { get; set; }
            public float Speed { get; set; }
        }

        private void InitializeConstellation()
        {
            stars.Clear();
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            // Rich starfield with 250 stars
            for (int i = 0; i < 250; i++)
            {
                stars.Add(new StarNode
                {
                    X = rand.Next(0, Math.Max(screenWidth, 1920)),
                    Y = rand.Next(0, Math.Max(screenHeight, 1080)),
                    VX = (float)(rand.NextDouble() * 0.18 - 0.09), // Gentle, slow floating drift
                    VY = (float)(rand.NextDouble() * 0.18 - 0.09),
                    Size = (float)(rand.NextDouble() * 2.2 + 1.2),
                    Alpha = (float)(rand.NextDouble() * 0.5 + 0.5),
                    AlphaSpeed = (float)(rand.NextDouble() * 0.012 + 0.004)
                });
            }
        }

        private void UpdateConstellation()
        {
            int maxW = Math.Max(this.ClientSize.Width, 800);
            int maxH = Math.Max(this.ClientSize.Height, 600);

            foreach (var star in stars)
            {
                star.X += star.VX;
                star.Y += star.VY;

                if (star.X < 0) star.X = maxW;
                if (star.X > maxW) star.X = 0;
                if (star.Y < 0) star.Y = maxH;
                if (star.Y > maxH) star.Y = 0;

                star.Alpha += star.AlphaSpeed;
                if (star.Alpha > 1.0f || star.Alpha < 0.3f)
                {
                    star.AlphaSpeed = -star.AlphaSpeed;
                }
            }

            // Spawn dynamic shooting data beams between stars
            if (pulses.Count < 24 && stars.Count > 1 && rand.Next(100) < 60)
            {
                StarNode s1 = stars[rand.Next(stars.Count)];
                StarNode s2 = stars[rand.Next(stars.Count)];
                if (s1 != s2)
                {
                    float dx = s1.X - s2.X;
                    float dy = s1.Y - s2.Y;
                    float distSq = (dx * dx) + (dy * dy);
                    if (distSq < 35000f && distSq > 3000f)
                    {
                        pulses.Add(new DataPulse
                        {
                            StartStar = s1,
                            EndStar = s2,
                            Progress = 0f,
                            // Decreased speed for a slower, more graceful transition
                            Speed = (float)(rand.NextDouble() * 0.008 + 0.004)
                        });
                    }
                }
            }

            // Update active pulses
            for (int i = pulses.Count - 1; i >= 0; i--)
            {
                pulses[i].Progress += pulses[i].Speed;
                if (pulses[i].Progress >= 1.0f)
                {
                    pulses.RemoveAt(i);
                }
            }

            // Sequential ripple wave handling (waits for previous to finish before triggering next)
            float maxRippleDist = Math.Max(maxW, maxH) * 1.3f;
            if (rippleRadius > 0f)
            {
                rippleRadius += 2.2f;
                if (rippleRadius > maxRippleDist)
                {
                    rippleRadius = 0f;
                    rippleWaitTicks = 0;
                }
            }
            else
            {
                rippleWaitTicks++;
                if (rippleWaitTicks > 75) // ~1.2 second pause between sequential ripples
                {
                    rippleRadius = 1f;
                }
            }
        }

        private void SetupFullScreenUI()
        {
            this.Text = "Nexus Enterprise Setup";
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;

            // Top Control Header Bar
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(6, 8, 14)
            };

            Label lblAppTitle = new Label
            {
                Text = "NEXUS CORE // ARCHITECTURAL ENVIRONMENT MATRIX v4.8",
                ForeColor = Color.FromArgb(129, 140, 248),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(28, 17)
            };
            headerPanel.Controls.Add(lblAppTitle);

            // Functional Close Button
            btnClose = new Button
            {
                Text = "✕",
                ForeColor = Color.FromArgb(203, 213, 225),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Width = 56,
                Height = 50,
                Dock = DockStyle.Right,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(239, 68, 68);
            btnClose.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };
            headerPanel.Controls.Add(btnClose);
            this.Controls.Add(headerPanel);

            // Solid high-tech obsidian card container (guarantees zero rendering glitches or font artifacts)
            mainCardPanel = new Panel
            {
                Width = 800,
                Height = 620,
                BackColor = Color.FromArgb(18, 22, 36)
            };

            CenterCard();
            this.Resize += (s, e) => CenterCard();

            // Immersive Title
            lblTitle = new Label
            {
                Text = "Initialize Ecosystem Matrix",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 26, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(50, 38)
            };
            mainCardPanel.Controls.Add(lblTitle);

            // Immersive Subtitle
            lblSubtitle = new Label
            {
                Text = "Deploying secure database schemas, high-speed inventory telemetry, and automated compliance guardrails tailored to your commercial vertical.",
                ForeColor = Color.FromArgb(156, 163, 175),
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                AutoSize = false,
                Width = 700,
                Height = 45,
                Location = new Point(52, 94)
            };
            mainCardPanel.Controls.Add(lblSubtitle);

            // Preset Section Label
            lblPreset = new Label
            {
                Text = "SELECT DEPLOYMENT VERTICAL",
                ForeColor = Color.FromArgb(129, 140, 248),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(52, 154)
            };
            mainCardPanel.Controls.Add(lblPreset);

            // Presets Dropdown
            cmbPresets = new ComboBox
            {
                Location = new Point(52, 182),
                Width = 696,
                Height = 38,
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(24, 32, 48),
                ForeColor = Color.White
            };
            cmbPresets.Items.Add("💊 Pharmacy & Healthcare (Batch tracking & Expiry guardrails)");
            cmbPresets.Items.Add("🛒 Retail & General Supermarket POS");
            cmbPresets.Items.Add("💻 Tech Hardware & Electronics Store");
            cmbPresets.Items.Add("🛋️ Furniture & Showroom Retail POS");
            cmbPresets.Items.Add("🍔 F&B, Grocery & Convenience Store POS");
            cmbPresets.SelectedIndex = 0;
            cmbPresets.SelectedIndexChanged += (s, e) => UpdateVerticalDescription();
            mainCardPanel.Controls.Add(cmbPresets);

            // Dynamic Vertical Sub-Description Label
            lblVerticalInfo = new Label
            {
                Text = "Configures automated batch tracking, strict expiry date warnings, and healthcare compliance records.",
                ForeColor = Color.FromArgb(203, 213, 225),
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                AutoSize = false,
                Width = 696,
                Height = 32,
                Location = new Point(52, 224)
            };
            mainCardPanel.Controls.Add(lblVerticalInfo);

            // Description Prompt Label
            lblPrompt = new Label
            {
                Text = "AI-POWERED SEMANTIC CATALOG MATCHER",
                ForeColor = Color.FromArgb(129, 140, 248),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(52, 270)
            };
            mainCardPanel.Controls.Add(lblPrompt);

            // Description Input Box
            txtDescription = new TextBox
            {
                Location = new Point(52, 298),
                Width = 564,
                Height = 42,
                Multiline = true,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(24, 32, 48),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "e.g., selling furniture or sodas and food"
            };
            mainCardPanel.Controls.Add(txtDescription);

            // Match Button
            btnMatch = new Button
            {
                Text = "⚡ Auto-Tune",
                Location = new Point(624, 297),
                Width = 124,
                Height = 40,
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnMatch.FlatAppearance.BorderSize = 0;
            btnMatch.Click += (s, e) => PerformSmartMatch();
            mainCardPanel.Controls.Add(btnMatch);

            // Status message label
            lblStatus = new Label
            {
                Text = "System standing by for core initialization sequence...",
                ForeColor = Color.FromArgb(148, 163, 184),
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                AutoSize = true,
                Location = new Point(52, 352)
            };
            mainCardPanel.Controls.Add(lblStatus);

            // Initialize Button (Emerald CTA)
            btnInitialize = new Button
            {
                Text = "🚀 Deploy & Initialize Secure Workspace",
                Location = new Point(52, 440),
                Width = 696,
                Height = 54,
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.OK
            };
            btnInitialize.FlatAppearance.BorderSize = 0;
            btnInitialize.Click += (s, e) => this.Close();
            mainCardPanel.Controls.Add(btnInitialize);

            this.Controls.Add(mainCardPanel);
        }

        private void UpdateVerticalDescription()
        {
            if (cmbPresets.SelectedIndex == 0)
                lblVerticalInfo.Text = "Configures automated batch tracking, strict expiry date warnings, and healthcare compliance records.";
            else if (cmbPresets.SelectedIndex == 1)
                lblVerticalInfo.Text = "Optimized for fast barcode scanning, multi-category inventory, and everyday supermarket checkout flows.";
            else if (cmbPresets.SelectedIndex == 2)
                lblVerticalInfo.Text = "Enables serial number tracking, warranty logging, and electronic parts specifications.";
            else if (cmbPresets.SelectedIndex == 3)
                lblVerticalInfo.Text = "Tailored for large item showrooms, custom delivery scheduling, and inventory deposit management.";
            else if (cmbPresets.SelectedIndex == 4)
                lblVerticalInfo.Text = "Built for high-velocity item sales, beverage inventory management, and quick barcode processing.";
        }

        private void PerformSmartMatch()
        {
            string input = txtDescription.Text.ToLower();
            if (input.Contains("furniture") || input.Contains("chair") || input.Contains("table") || input.Contains("sofa"))
            {
                cmbPresets.SelectedIndex = 3;
            }
            else if (input.Contains("soda") || input.Contains("food") || input.Contains("grocery") || input.Contains("drink") || input.Contains("snack"))
            {
                cmbPresets.SelectedIndex = 4;
            }
            else if (input.Contains("pharmacy") || input.Contains("medicine") || input.Contains("pill") || input.Contains("health"))
            {
                cmbPresets.SelectedIndex = 0;
            }
            else if (input.Contains("tech") || input.Contains("computer") || input.Contains("phone") || input.Contains("electronic"))
            {
                cmbPresets.SelectedIndex = 2;
            }
            else
            {
                cmbPresets.SelectedIndex = 1;
            }

            lblStatus.Text = $"✨ AI Matched vertical to: {cmbPresets.SelectedItem.ToString().Split(' ')[1]}!";
            lblStatus.ForeColor = Color.FromArgb(52, 211, 153);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Deep background gradient
            Rectangle renderRect = new Rectangle(0, 0, this.ClientSize.Width + 2, this.ClientSize.Height + 2);
            using (LinearGradientBrush bgBrush = new LinearGradientBrush(
                renderRect, Color.FromArgb(8, 10, 18), Color.FromArgb(2, 3, 6), 65f))
            {
                g.FillRectangle(bgBrush, renderRect);
            }

            // Draw sequential expanding ripple ring with smooth alpha fade
            if (rippleRadius > 0)
            {
                float maxDist = Math.Max(this.ClientSize.Width, 800) * 1.3f;
                float rippleRatio = rippleRadius / maxDist;
                int alphaRipple = Math.Max(0, (int)(80 * (1.0f - rippleRatio)));
                using (Pen ripplePen = new Pen(Color.FromArgb(alphaRipple, 129, 140, 248), 1.5f))
                {
                    Point center = new Point(this.ClientSize.Width / 2, this.ClientSize.Height / 2);
                    g.DrawEllipse(ripplePen, center.X - rippleRadius, center.Y - rippleRadius, rippleRadius * 2, rippleRadius * 2);
                }
            }

            // Render shooting data lines with smooth fade-in and fade-out animations
            foreach (var pulse in pulses)
            {
                float px = pulse.StartStar.X + (pulse.EndStar.X - pulse.StartStar.X) * pulse.Progress;
                float py = pulse.StartStar.Y + (pulse.EndStar.Y - pulse.StartStar.Y) * pulse.Progress;

                // Smooth fade-in and fade-out envelope
                float fadeFactor = 1.0f;
                if (pulse.Progress < 0.2f) fadeFactor = pulse.Progress / 0.2f;
                else if (pulse.Progress > 0.8f) fadeFactor = (1.0f - pulse.Progress) / 0.2f;

                int lineAlpha = (int)(165 * fadeFactor);
                int nodeAlpha = (int)(240 * fadeFactor);

                using (Pen pulseTailPen = new Pen(Color.FromArgb(lineAlpha, 147, 197, 253), 1.5f))
                {
                    g.DrawLine(pulseTailPen, pulse.StartStar.X, pulse.StartStar.Y, px, py);
                }

                using (Brush pulseHeadBrush = new SolidBrush(Color.FromArgb(nodeAlpha, 235, 243, 255)))
                {
                    g.FillEllipse(pulseHeadBrush, px - 2.75f, py - 2.75f, 5.5f, 5.5f);
                }
            }

            // Render twinkling stars
            foreach (var star in stars)
            {
                int cAlpha = Math.Min(200, Math.Max(70, (int)(star.Alpha * 160)));

                // Outer glow halo
                using (Brush haloBrush = new SolidBrush(Color.FromArgb(cAlpha / 6, 129, 140, 248)))
                {
                    g.FillEllipse(haloBrush, star.X - star.Size * 1.5f, star.Y - star.Size * 1.5f, star.Size * 3f, star.Size * 3f);
                }

                // Core
                using (Brush starBrush = new SolidBrush(Color.FromArgb(cAlpha, 200, 215, 240)))
                {
                    g.FillEllipse(starBrush, star.X - star.Size / 2, star.Y - star.Size / 2, star.Size, star.Size);
                }
            }

            // Draw vibrant neon border around the solid card container
            if (mainCardPanel != null)
            {
                Rectangle cardRect = mainCardPanel.Bounds;
                cardRect.Inflate(1, 1);
                using (Pen cardBorderPen = new Pen(Color.FromArgb(129, 140, 248), 1.75f))
                {
                    g.DrawRectangle(cardBorderPen, cardRect);
                }
            }
        }

        private void CenterCard()
        {
            if (mainCardPanel != null)
            {
                mainCardPanel.Location = new Point(
                    (this.ClientSize.Width - mainCardPanel.Width) / 2,
                    (this.ClientSize.Height - mainCardPanel.Height) / 2 + 20
                );
            }
        }
    }
}