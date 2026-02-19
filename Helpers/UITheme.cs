using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

// Requiere referencias a System, System.Drawing y System.Windows.Forms (SistemaPOS.csproj).

namespace SistemaPOS.Helpers
{
    // ==========================================
    // PALETA DE COLORES MODERNA
    // ==========================================
    public static class UITheme
    {
        // Fondos principales
        public static readonly Color DarkBackground = Color.FromArgb(26, 32, 44);      // #1A202C
        public static readonly Color PanelBackground = Color.FromArgb(45, 55, 72);      // #2D3748
        public static readonly Color LightPanel = Color.FromArgb(74, 85, 104);          // #4A5568
        public static readonly Color InputBackground = Color.FromArgb(45, 55, 72);      // #2D3748
        public static readonly Color SurfaceColor = Color.FromArgb(35, 43, 58);         // superficie intermedia

        // Texto
        public static readonly Color TextPrimary = Color.FromArgb(247, 250, 252);       // #F7FAFC
        public static readonly Color TextSecondary = Color.FromArgb(160, 174, 192);     // #A0AEC0
        public static readonly Color TextMuted = Color.FromArgb(113, 128, 150);         // #718096

        // Colores de Acción
        public static readonly Color PrimaryColor = Color.FromArgb(49, 130, 206);       // Azul vibrante
        public static readonly Color PrimaryDark = Color.FromArgb(36, 100, 163);
        public static readonly Color PrimaryLight = Color.FromArgb(66, 153, 225);
        public static readonly Color SecondaryColor = Color.FromArgb(74, 85, 104);
        public static readonly Color AccentColor = Color.FromArgb(56, 178, 172);        // Teal
        public static readonly Color SuccessColor = Color.FromArgb(72, 187, 120);       // Verde suave
        public static readonly Color DangerColor = Color.FromArgb(245, 101, 101);       // Rojo suave
        public static readonly Color WarningColor = Color.FromArgb(236, 201, 75);       // Amarillo cálido
        public static readonly Color InfoColor = Color.FromArgb(66, 153, 225);          // Azul info
        public static readonly Color ErrorColor = DangerColor;

        // Bordes
        public static readonly Color BorderColor = Color.FromArgb(74, 85, 104);
        public static readonly Color BorderLight = Color.FromArgb(100, 116, 139);

        // Sidebar
        public static readonly Color SidebarBackground = Color.FromArgb(23, 25, 35);
        public static readonly Color SidebarHover = Color.FromArgb(45, 55, 72);
        public static readonly Color SidebarActive = Color.FromArgb(49, 130, 206);

        // Fuentes
        public static readonly Font FontTitle = new Font("Segoe UI", 18, FontStyle.Bold);
        public static readonly Font FontSubtitle = new Font("Segoe UI", 13, FontStyle.Bold);
        public static readonly Font FontRegular = new Font("Segoe UI", 10, FontStyle.Regular);
        public static readonly Font FontText = FontRegular;
        public static readonly Font FontBold = new Font("Segoe UI", 10, FontStyle.Bold);
        public static readonly Font FontSmall = new Font("Segoe UI", 9, FontStyle.Regular);
        public static readonly Font FontLarge = new Font("Segoe UI", 14, FontStyle.Regular);
        public static readonly Font FontIcon = new Font("Segoe UI", 16, FontStyle.Regular);

        // Radio de bordes
        public const int BorderRadius = 10;
        public const int BorderRadiusSmall = 6;

        // ==========================================
        // MÉTODOS DE ESTILO
        // ==========================================

        public static void ApplyTheme(Form form)
        {
            form.BackColor = DarkBackground;
            form.ForeColor = TextPrimary;
            form.Font = FontRegular;
        }

        public static void StyleButton(Button btn, Color backColor)
        {
            btn.BackColor = backColor;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            Color hoverColor = LightenColor(backColor, 25);
            Color pressColor = DarkenColor(backColor, 15);

            btn.MouseEnter += (s, e) => btn.BackColor = hoverColor;
            btn.MouseLeave += (s, e) => btn.BackColor = backColor;
            btn.MouseDown += (s, e) => btn.BackColor = pressColor;
            btn.MouseUp += (s, e) => btn.BackColor = hoverColor;
        }

        public static void StyleTextBox(TextBox txt)
        {
            txt.BackColor = Color.FromArgb(55, 65, 81);
            txt.ForeColor = TextPrimary;
            txt.BorderStyle = BorderStyle.FixedSingle;
            txt.Font = FontRegular;
        }

        public static void StyleLabel(Label lbl, Font font = null, Color? color = null)
        {
            lbl.Font = font ?? FontRegular;
            lbl.ForeColor = color ?? TextPrimary;
        }

        public static void StyleDataGridView(DataGridView dgv)
        {
            dgv.BackgroundColor = DarkBackground;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.GridColor = Color.FromArgb(55, 65, 81);

            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(35, 43, 58);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextSecondary;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(35, 43, 58);
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 6, 8, 6);
            dgv.ColumnHeadersHeight = 42;

            dgv.DefaultCellStyle.BackColor = Color.FromArgb(30, 38, 52);
            dgv.DefaultCellStyle.ForeColor = TextPrimary;
            dgv.DefaultCellStyle.SelectionBackColor = PrimaryColor;   // Azul opaco - DataGridView no soporta alpha
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            dgv.DefaultCellStyle.Font = FontRegular;
            dgv.DefaultCellStyle.Padding = new Padding(8, 4, 8, 4);

            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(35, 45, 60);
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = PrimaryColor;
            dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.White;

            dgv.RowTemplate.Height = 38;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        public static void StyleComboBox(ComboBox cbo)
        {
            cbo.BackColor = InputBackground;
            cbo.ForeColor = TextPrimary;
            cbo.FlatStyle = FlatStyle.Flat;
            cbo.Font = FontRegular;
        }

        public static void StyleDateTimePicker(DateTimePicker dtp)
        {
            dtp.BackColor = InputBackground;
            dtp.ForeColor = TextPrimary;
            dtp.Font = FontRegular;
        }

        // ==========================================
        // UTILIDADES DE COLOR
        // ==========================================

        public static Color LightenColor(Color color, int amount)
        {
            int r = Math.Min(255, color.R + amount);
            int g = Math.Min(255, color.G + amount);
            int b = Math.Min(255, color.B + amount);
            return Color.FromArgb(color.A, r, g, b);
        }

        public static Color DarkenColor(Color color, int amount)
        {
            int r = Math.Max(0, color.R - amount);
            int g = Math.Max(0, color.G - amount);
            int b = Math.Max(0, color.B - amount);
            return Color.FromArgb(color.A, r, g, b);
        }

        public static Color WithAlpha(Color color, int alpha)
        {
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }

        // ==========================================
        // DIÁLOGOS
        // ==========================================

        public static void MostrarNotificacion(string mensaje, string titulo = "Notificación", MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            MessageBox.Show(mensaje, titulo, MessageBoxButtons.OK, icon);
        }

        public static DialogResult MostrarConfirmacion(string mensaje, string titulo = "Confirmar")
        {
            return MessageBox.Show(mensaje, titulo, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        // ==========================================
        // CREACIÓN DE COMPONENTES REUTILIZABLES
        // ==========================================

        public static Panel CrearPanel(int x, int y, int width, int height, Color? backgroundColor = null)
        {
            return new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = backgroundColor ?? PanelBackground,
                Padding = new Padding(10)
            };
        }

        public static Label CrearBadge(string texto, Color color, int x, int y)
        {
            Label badge = new Label
            {
                Text = texto,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = color,
                AutoSize = false,
                Size = new Size(80, 22),
                Location = new Point(x, y),
                TextAlign = ContentAlignment.MiddleCenter
            };
            return badge;
        }

        /// <summary>
        /// Crea un header bar para formularios principales
        /// </summary>
        public static Panel CrearHeaderBar(string titulo, string subtitulo = null)
        {
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = SurfaceColor,
                Padding = new Padding(20, 0, 20, 0)
            };

            Label lblTitulo = new Label
            {
                Text = titulo,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = TextPrimary,
                AutoSize = true,
                Location = new Point(20, subtitulo != null ? 8 : 16)
            };
            header.Controls.Add(lblTitulo);

            if (subtitulo != null)
            {
                Label lblSub = new Label
                {
                    Text = subtitulo,
                    Font = FontSmall,
                    ForeColor = TextSecondary,
                    AutoSize = true,
                    Location = new Point(20, 34)
                };
                header.Controls.Add(lblSub);
            }

            // Línea inferior accent
            Panel lineaAccent = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 2,
                BackColor = PrimaryColor
            };
            header.Controls.Add(lineaAccent);

            return header;
        }

        /// <summary>
        /// Crea una tarjeta KPI moderna
        /// </summary>
        public static Panel CrearTarjetaKPI(string titulo, string icono, Color colorAccent, ref Label lblValor, int width = 220)
        {
            Panel card = new Panel
            {
                Size = new Size(width, 90),
                BackColor = SurfaceColor,
                Margin = new Padding(0, 0, 12, 12),
                Padding = new Padding(0)
            };

            // Barra lateral de color
            Panel barra = new Panel
            {
                Dock = DockStyle.Left,
                Width = 4,
                BackColor = colorAccent
            };

            // Icono
            Label lblIcono = new Label
            {
                Text = icono,
                Font = new Font("Segoe UI", 22, FontStyle.Regular),
                ForeColor = colorAccent,
                Location = new Point(14, 18),
                AutoSize = true
            };

            // Título
            Label lblTitle = new Label
            {
                Text = titulo.ToUpper(),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = TextSecondary,
                Location = new Point(56, 14),
                AutoSize = true
            };

            // Valor
            lblValor = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = TextPrimary,
                Location = new Point(56, 36),
                AutoSize = true
            };

            card.Controls.Add(lblValor);
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblIcono);
            card.Controls.Add(barra);

            // Hover effect
            Color normalBg = SurfaceColor;
            Color hoverBg = LightenColor(SurfaceColor, 8);
            card.MouseEnter += (s, e) => card.BackColor = hoverBg;
            card.MouseLeave += (s, e) => card.BackColor = normalBg;

            return card;
        }
    }

    // ==========================================
    // CONTROLES CUSTOM
    // ==========================================

    /// <summary>
    /// Panel con bordes redondeados
    /// </summary>
    public class RoundedPanel : Panel
    {
        public int Radius { get; set; }
        public Color BorderColor { get; set; }
        public int BorderWidth { get; set; }

        public RoundedPanel()
        {
            Radius = UITheme.BorderRadius;
            BorderColor = Color.Transparent;
            BorderWidth = 0;
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath path = CreateRoundedPath(this.ClientRectangle, Radius))
            {
                this.Region = new Region(path);

                using (SolidBrush brush = new SolidBrush(this.BackColor))
                {
                    e.Graphics.FillPath(brush, path);
                }

                if (BorderWidth > 0 && BorderColor != Color.Transparent)
                {
                    using (Pen pen = new Pen(BorderColor, BorderWidth))
                    {
                        Rectangle rect = new Rectangle(
                            BorderWidth / 2,
                            BorderWidth / 2,
                            this.Width - BorderWidth,
                            this.Height - BorderWidth);
                        using (GraphicsPath borderPath = CreateRoundedPath(rect, Radius))
                        {
                            e.Graphics.DrawPath(pen, borderPath);
                        }
                    }
                }
            }
        }

        private GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    /// <summary>
    /// Botón con bordes redondeados y efecto hover
    /// </summary>
    public class RoundedButton : Control
    {
        public int Radius { get; set; }
        public Color ButtonColor { get; set; }
        public Color HoverColor { get; set; }
        public Color PressColor { get; set; }
        public string IconText { get; set; }

        private bool _isHovering = false;
        private bool _isPressed = false;

        public RoundedButton()
        {
            Radius = UITheme.BorderRadiusSmall;
            ButtonColor = UITheme.PrimaryColor;
            HoverColor = UITheme.PrimaryLight;
            PressColor = UITheme.PrimaryDark;
            IconText = "";

            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.Cursor = Cursors.Hand;
            this.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            this.ForeColor = Color.White;
            this.Size = new Size(200, 45);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            Color bgColor = _isPressed ? PressColor : (_isHovering ? HoverColor : ButtonColor);

            using (GraphicsPath path = CreateRoundedPath(new Rectangle(0, 0, Width, Height), Radius))
            {
                this.Region = new Region(path);
                using (SolidBrush brush = new SolidBrush(bgColor))
                {
                    e.Graphics.FillPath(brush, path);
                }
            }

            // Texto centrado
            string fullText = string.IsNullOrEmpty(IconText) ? Text : IconText + "  " + Text;
            TextRenderer.DrawText(e.Graphics, fullText, this.Font,
                new Rectangle(0, 0, Width, Height), this.ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _isHovering = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _isHovering = false;
            _isPressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _isPressed = true;
            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _isPressed = false;
            Invalidate();
            base.OnMouseUp(e);
        }

        private GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    /// <summary>
    /// TextBox moderno con placeholder text
    /// </summary>
    public class ModernTextBox : Panel
    {
        private TextBox _innerTextBox;
        private Label _placeholderLabel;
        private Panel _borderBottom;
        public string PlaceholderText { get; set; }

        public new string Text
        {
            get { return _innerTextBox.Text; }
            set { _innerTextBox.Text = value; UpdatePlaceholder(); }
        }

        public bool UseSystemPasswordChar
        {
            get { return _innerTextBox.UseSystemPasswordChar; }
            set { _innerTextBox.UseSystemPasswordChar = value; }
        }

        public bool ReadOnly
        {
            get { return _innerTextBox.ReadOnly; }
            set { _innerTextBox.ReadOnly = value; }
        }

        public int MaxLength
        {
            get { return _innerTextBox.MaxLength; }
            set { _innerTextBox.MaxLength = value; }
        }

        public TextBox InnerTextBox { get { return _innerTextBox; } }

        public new event EventHandler TextChanged
        {
            add { _innerTextBox.TextChanged += value; }
            remove { _innerTextBox.TextChanged -= value; }
        }

        public event KeyEventHandler InnerKeyDown
        {
            add { _innerTextBox.KeyDown += value; }
            remove { _innerTextBox.KeyDown -= value; }
        }

        public ModernTextBox(string placeholder = "", string iconText = "")
        {
            PlaceholderText = placeholder;
            this.Height = 42;
            this.BackColor = Color.FromArgb(55, 65, 81);

            int leftMargin = iconText != "" ? 38 : 12;
            int topMargin = 11;

            if (iconText != "")
            {
                Label lblIcon = new Label
                {
                    Text = iconText,
                    Font = new Font("Segoe UI", 12),
                    ForeColor = UITheme.TextMuted,
                    Location = new Point(10, topMargin),
                    AutoSize = true
                };
                this.Controls.Add(lblIcon);
            }

            _borderBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 2,
                BackColor = UITheme.BorderColor
            };

            _innerTextBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(55, 65, 81),
                ForeColor = UITheme.TextPrimary,
                Font = new Font("Segoe UI", 11),
                Location = new Point(leftMargin, topMargin),
                Width = 250,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };
            _innerTextBox.GotFocus += (s, e) => { OnFocusChanged(true); UpdatePlaceholder(); };
            _innerTextBox.LostFocus += (s, e) => { OnFocusChanged(false); UpdatePlaceholder(); };
            _innerTextBox.TextChanged += (s, e) => UpdatePlaceholder();

            _placeholderLabel = new Label
            {
                Text = placeholder,
                Font = new Font("Segoe UI", 11),
                ForeColor = UITheme.TextMuted,
                BackColor = Color.Transparent,
                AutoSize = false,
                Location = new Point(leftMargin, topMargin - 1),
                Size = new Size(250, 20),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.IBeam,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };
            _placeholderLabel.Click += (s, e) => _innerTextBox.Focus();

            this.Controls.Add(_borderBottom);
            this.Controls.Add(_innerTextBox);
            this.Controls.Add(_placeholderLabel);
        }

        private void OnFocusChanged(bool focused)
        {
            _borderBottom.BackColor = focused ? UITheme.PrimaryColor : UITheme.BorderColor;
        }

        private void UpdatePlaceholder()
        {
            _placeholderLabel.Visible = string.IsNullOrEmpty(_innerTextBox.Text) && !_innerTextBox.Focused;
        }

        public new bool Focus()
        {
            return _innerTextBox.Focus();
        }

        public void Clear()
        {
            _innerTextBox.Clear();
        }

        public void SelectAll()
        {
            _innerTextBox.SelectAll();
        }
    }

    /// <summary>
    /// Botón de sidebar con icono y efecto hover
    /// </summary>
    public class SidebarItem : Panel
    {
        public string IconText { get; set; }
        public string LabelText { get; set; }
        public bool IsActive { get; set; }
        public new Action OnClick { get; set; }

        private Label _iconLabel;
        private Label _textLabel;
        private Panel _accentBar;

        public SidebarItem(string icon, string text, Action onClick, bool isActive = false)
        {
            IconText = icon;
            LabelText = text;
            OnClick = onClick;
            IsActive = isActive;

            this.Height = 48;
            this.Dock = DockStyle.Top;
            this.BackColor = isActive ? UITheme.SidebarHover : Color.Transparent;
            this.Cursor = Cursors.Hand;

            _accentBar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 3,
                BackColor = isActive ? UITheme.AccentColor : Color.Transparent
            };

            _iconLabel = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 14),
                ForeColor = isActive ? UITheme.AccentColor : UITheme.TextSecondary,
                AutoSize = false,
                Size = new Size(44, 48),
                Location = new Point(10, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };

            _textLabel = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10, isActive ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = isActive ? UITheme.TextPrimary : UITheme.TextSecondary,
                AutoSize = false,
                Size = new Size(160, 48),
                Location = new Point(54, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand
            };

            this.Controls.Add(_textLabel);
            this.Controls.Add(_iconLabel);
            this.Controls.Add(_accentBar);

            // Click en cualquier parte
            this.Click += (s, e) => onClick();
            _iconLabel.Click += (s, e) => onClick();
            _textLabel.Click += (s, e) => onClick();

            // Hover
            Color hoverBg = UITheme.SidebarHover;
            Color normalBg = isActive ? UITheme.SidebarHover : Color.Transparent;

            Action<object, EventArgs> onEnter = (s, e) => { if (!IsActive) this.BackColor = hoverBg; };
            Action<object, EventArgs> onLeave = (s, e) => { if (!IsActive) this.BackColor = normalBg; };

            this.MouseEnter += new EventHandler(onEnter);
            this.MouseLeave += new EventHandler(onLeave);
            _iconLabel.MouseEnter += new EventHandler(onEnter);
            _iconLabel.MouseLeave += new EventHandler(onLeave);
            _textLabel.MouseEnter += new EventHandler(onEnter);
            _textLabel.MouseLeave += new EventHandler(onLeave);
        }

        public void SetActive(bool active)
        {
            IsActive = active;
            _accentBar.BackColor = active ? UITheme.AccentColor : Color.Transparent;
            _iconLabel.ForeColor = active ? UITheme.AccentColor : UITheme.TextSecondary;
            _textLabel.ForeColor = active ? UITheme.TextPrimary : UITheme.TextSecondary;
            _textLabel.Font = new Font("Segoe UI", 10, active ? FontStyle.Bold : FontStyle.Regular);
            this.BackColor = active ? UITheme.SidebarHover : Color.Transparent;
        }
    }
}
