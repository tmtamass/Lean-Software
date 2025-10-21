using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class MainForm : Form
{
    private Panel card;
    private Label titleLabel, selectLabel, gameLabel, infoHeader;
    private Button leftBtn, rightBtn, closeBtn, generateBtn;
    private Panel divider;
    private Label regUsersLabel, regUsersValue, genAccountsLabel, genAccountsValue, subscriptionLabel, subscriptionValue;

    private readonly string[] games = new[] { "FIVEM", "STEAM", "DISCORD" };
    private int gameIndex = 0;

    public MainForm()
    {
        Text = "Lean Generator";
        StartPosition = FormStartPosition.CenterScreen;
        // Purple theme background
        BackColor = Color.FromArgb(18, 14, 30);
        ClientSize = new Size(480, 600);
        DoubleBuffered = true;
        Font = new Font("Segoe UI", 10f, FontStyle.Regular);
        FormBorderStyle = FormBorderStyle.None; // borderless per request
        ControlBox = false;

        // Apply rounded corners to the window
        ApplyWindowRounding(12);

        card = new Panel
        {
            BackColor = Color.FromArgb(34, 28, 56),
            Size = new Size(420, 520),
            Location = new Point(30, 40)
        };
        // Rounded corners for the card
        card.SizeChanged += (s, e) =>
        {
            card.Region = new Region(CreateRoundedRect(new Rectangle(Point.Empty, card.Size), 12));
        };
        card.Paint += (s, e) =>
        {
            using (var p = new Pen(Color.FromArgb(100, 85, 160)))
            {
                e.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1);
            }
        };
        Controls.Add(card);

        closeBtn = new Button
        {
            Text = "✕",
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.Gainsboro,
            BackColor = Color.Transparent,
            Location = new Point(card.Width - 40, 10),
            Size = new Size(30, 30),
            TabStop = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        closeBtn.FlatAppearance.BorderSize = 0;
        // Replace garbled text with a drawn X icon
        closeBtn.Text = string.Empty;
        closeBtn.Image = CreateXBitmap(12, 12, Color.Red);
        closeBtn.ImageAlign = ContentAlignment.MiddleCenter;
        closeBtn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 255, 0, 0);
        closeBtn.Click += (s, e) => Close();
        card.Controls.Add(closeBtn);

        titleLabel = new Label
        {
            Text = "LEAN GENERATOR",
            ForeColor = Color.FromArgb(170, 150, 255),
            Font = new Font("Segoe UI Semibold", 12f, FontStyle.Bold),
            Location = new Point(20, 16),
            AutoSize = true
        };
        card.Controls.Add(titleLabel);

        selectLabel = new Label
        {
            Text = "Select the Process.",
            ForeColor = Color.Gainsboro,
            Location = new Point(20, 70),
            AutoSize = true
        };
        card.Controls.Add(selectLabel);

        leftBtn = MakeArrowButton(new Point(40, 110), false);
        leftBtn.Click += (s, e) => ChangeGame(-1);
        rightBtn = MakeArrowButton(new Point(card.Width - 80, 110), true);
        rightBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        rightBtn.Click += (s, e) => ChangeGame(1);
        card.Controls.Add(leftBtn);
        card.Controls.Add(rightBtn);

        gameLabel = new Label
        {
            Text = games[0],
            ForeColor = Color.White,
            Font = new Font("Segoe UI Black", 28f, FontStyle.Bold),
            AutoSize = true
        };
        card.Controls.Add(gameLabel);
        PositionGameLabel();

        divider = new Panel
        {
            BackColor = Color.FromArgb(90, 75, 140),
            Size = new Size(card.Width - 40, 1),
            Location = new Point(20, 190)
        };
        card.Controls.Add(divider);

        infoHeader = new Label
        {
            Text = "Information about your GEN",
            ForeColor = Color.Gainsboro,
            Location = new Point(20, 210),
            AutoSize = true
        };
        card.Controls.Add(infoHeader);

        var y = 250;
        AddInfoRow(ref y, "Registered Users", "0", out regUsersLabel, out regUsersValue);
        AddInfoRow(ref y, "Generated Accounts", "0", out genAccountsLabel, out genAccountsValue);
        AddInfoRow(ref y, "Subscription", "Lifetime", out subscriptionLabel, out subscriptionValue);

        generateBtn = new Button
        {
            Text = "Generate    →",
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            BackColor = Color.FromArgb(113, 97, 255),
            Location = new Point(20, card.Height - 70),
            Size = new Size(card.Width - 40, 46),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };
        generateBtn.FlatAppearance.BorderSize = 0;
        // Ensure no stray glyphs in label
        generateBtn.Text = "Generate";
        #if false
        generateBtn.Click += (s, e) => MessageBox.Show(this, "UI only — no generation implemented.", "Generate", MessageBoxButtons.OK, MessageBoxIcon.Information);
        // Rounded button region
        generateBtn.SizeChanged += (s, e) =>
        {
            generateBtn.Region = new Region(CreateRoundedRect(new Rectangle(Point.Empty, generateBtn.Size), 8));
        };
        #endif
        // Increment stats and show confirmation
        generateBtn.Click += (s, e) =>
        {
            IncrementLabel(regUsersValue, 1);
            IncrementLabel(genAccountsValue, 1);
            MessageBox.Show(this, "Done.", "Generate", MessageBoxButtons.OK, MessageBoxIcon.Information);
        };
        card.Controls.Add(generateBtn);

        // Drag support for borderless window
        card.MouseDown += BeginDrag;
        card.MouseMove += DoDrag;
        MouseDown += BeginDrag;
        MouseMove += DoDrag;
    }

    private Button MakeArrowButton(Point location, bool right)
    {
        var b = new Button
        {
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.Gainsboro,
            BackColor = Color.FromArgb(48, 40, 72),
            Location = location,
            Size = new Size(40, 36),
            TabStop = false,
            ImageAlign = ContentAlignment.MiddleCenter,
        };
        b.FlatAppearance.BorderColor = Color.FromArgb(110, 90, 160);
        b.FlatAppearance.BorderSize = 1;
        b.Image = CreateArrowBitmap(18, 18, right);
        // Rounded arrow buttons
        b.SizeChanged += (s, e) =>
        {
            b.Region = new Region(CreateRoundedRect(new Rectangle(Point.Empty, b.Size), 6));
        };
        return b;
    }

    private Bitmap CreateArrowBitmap(int w, int h, bool right)
    {
        var bmp = new Bitmap(w, h);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            PointF p1, p2, p3;
            if (right)
            {
                p1 = new PointF(w * 0.72f, h * 0.50f); // tip
                p2 = new PointF(w * 0.35f, h * 0.20f);
                p3 = new PointF(w * 0.35f, h * 0.80f);
            }
            else
            {
                p1 = new PointF(w * 0.28f, h * 0.50f); // tip
                p2 = new PointF(w * 0.65f, h * 0.20f);
                p3 = new PointF(w * 0.65f, h * 0.80f);
            }

            using (var brush = new SolidBrush(Color.Gainsboro))
            using (var pen = new Pen(Color.Gainsboro, 1f))
            {
                g.FillPolygon(brush, new[] { p1, p2, p3 });
                g.DrawPolygon(pen, new[] { p1, p2, p3 });
            }
        }
        return bmp;
    }

    private Bitmap CreateXBitmap(int w, int h, Color color)
    {
        var bmp = new Bitmap(w, h);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            using (var pen = new Pen(color, 2f))
            {
                g.DrawLine(pen, 2, 2, w - 3, h - 3);
                g.DrawLine(pen, w - 3, 2, 2, h - 3);
            }
        }
        return bmp;
    }

    private void PositionGameLabel()
    {
        gameLabel.Location = new Point((card.Width - gameLabel.Width) / 2, 105);
    }

    private void ChangeGame(int delta)
    {
        gameIndex = (gameIndex + delta + games.Length) % games.Length;
        gameLabel.Text = games[gameIndex];
        PositionGameLabel();
    }

    private Point dragStart;
    private bool dragging;

    private void BeginDrag(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            dragging = true;
            dragStart = e.Location;
        }
    }

    private void DoDrag(object sender, MouseEventArgs e)
    {
        if (dragging && e.Button == MouseButtons.Left)
        {
            var screen = PointToScreen(e.Location);
            Location = new Point(screen.X - dragStart.X, screen.Y - dragStart.Y);
        }
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        // Keep form corners rounded on resize
        ApplyWindowRounding(12);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        dragging = false;
        base.OnMouseUp(e);
    }

    private void AddInfoRow(ref int y, string label, string value, out Label keyLabel, out Label valLabel)
    {
        keyLabel = new Label
        {
            Text = "  " + label,
            ForeColor = Color.Gainsboro,
            Location = new Point(20, y),
            AutoSize = true
        };

        valLabel = new Label
        {
            Text = value,
            ForeColor = Color.White,
            Location = new Point(card.Width - 20 - 60, y),
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleRight,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };

        var rowUnderline = new Panel
        {
            BackColor = Color.FromArgb(70, 55, 110),
            Size = new Size(card.Width - 40, 1),
            Location = new Point(20, y + 28)
        };

        card.Controls.Add(keyLabel);
        card.Controls.Add(valLabel);
        card.Controls.Add(rowUnderline);
        y += 44;
    }

    private void IncrementLabel(Label label, int delta)
    {
        if (label == null) return;
        int n;
        if (!int.TryParse(label.Text, out n)) n = 0;
        n += delta;
        label.Text = n.ToString();
    }

    // Helpers for rounded corners
    private void ApplyWindowRounding(int radius)
    {
        var rect = new Rectangle(Point.Empty, this.Size);
        if (rect.Width > 0 && rect.Height > 0)
        {
            this.Region = new Region(CreateRoundedRect(rect, radius));
        }
    }

    private GraphicsPath CreateRoundedRect(Rectangle bounds, int radius)
    {
        int d = radius * 2;
        var path = new GraphicsPath();
        if (radius <= 0)
        {
            path.AddRectangle(bounds);
            path.CloseFigure();
            return path;
        }

        var arc = new Rectangle(bounds.Location, new Size(d, d));
        // Top-left
        path.AddArc(arc, 180, 90);
        // Top-right
        arc.X = bounds.Right - d;
        path.AddArc(arc, 270, 90);
        // Bottom-right
        arc.Y = bounds.Bottom - d;
        path.AddArc(arc, 0, 90);
        // Bottom-left
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }
}

public static class Program
{
    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}
