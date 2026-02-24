using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using SistemaPOS.Services;
using SistemaPOS.Models;
using SistemaPOS.Helpers;

namespace SistemaPOS.Forms
{
    public partial class LoginForm : Form
    {
        private ModernTextBox txtUsuario;
        private ModernTextBox txtContraseña;
        private RoundedButton btnIniciarSesion;
        private Button btnVerClave;
        private Label lblError;

        private const int FormWidth = 460;
        private const int FormHeight = 615;
        private const int PanelWidth = 400;
        private const int PanelHeight = 558;
        private const int FieldX = 30;
        private const int FieldWidth = 340;
        private const int HeaderX = FieldX + 6;
        private const int HeaderWidth = FieldWidth - 12;
        private const int CardInset = 4;
        private static readonly Color BgTopColor = Color.FromArgb(13, 23, 39);
        private static readonly Color BgBottomColor = Color.FromArgb(8, 14, 26);
        private static readonly Color CardColor = Color.FromArgb(30, 40, 58);
        private static readonly Color CardBorderColor = Color.FromArgb(56, 90, 130);
        private static readonly Color DividerColor = Color.FromArgb(58, 78, 105);
        private static readonly Color InputColor = Color.FromArgb(44, 55, 74);
        private static readonly Color InputBorderColor = Color.FromArgb(78, 98, 126);
        private static readonly Color InputFocusBorderColor = Color.FromArgb(73, 154, 239);
        private static readonly Color LabelColor = Color.FromArgb(141, 163, 191);
        private static readonly Color MutedTextColor = Color.FromArgb(111, 136, 169);
        private static readonly Color PrimaryButtonColor = Color.FromArgb(63, 137, 207);
        private static readonly Color PrimaryButtonHover = Color.FromArgb(76, 152, 224);
        private static readonly Color PrimaryButtonPress = Color.FromArgb(49, 112, 171);

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Sistema POS - Inicio de Sesión";
            this.AutoScaleMode = AutoScaleMode.None;
            this.ClientSize = new Size(FormWidth, FormHeight);
            this.MinimumSize = new Size(FormWidth, FormHeight);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = BgBottomColor;
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.DoubleBuffered = true;

            GradientBackgroundPanel fondoGradiente = new GradientBackgroundPanel
            {
                Dock = DockStyle.Fill,
                TopColor = BgTopColor,
                BottomColor = BgBottomColor
            };
            EnableDrag(fondoGradiente);

            RoundedPanel panelCentral = new RoundedPanel
            {
                Size = new Size(PanelWidth, PanelHeight),
                BackColor = CardColor,
                Radius = 16,
                BorderColor = CardBorderColor,
                BorderWidth = 1
            };

            Action centrarPanel = () =>
            {
                panelCentral.Location = new Point(
                    (this.ClientSize.Width - PanelWidth) / 2,
                    (this.ClientSize.Height - PanelHeight) / 2);
            };
            centrarPanel();
            this.Resize += (s, e) => centrarPanel();

            Panel titleBar = new Panel
            {
                Size = new Size(PanelWidth - ((CardInset + 2) * 2), 40),
                Location = new Point(CardInset + 2, CardInset + 2),
                BackColor = CardColor
            };
            EnableDrag(titleBar);

            Button btnMin = CrearBotonTitleBar("─", titleBar.Width - 78, false);
            btnMin.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            Button btnClose = CrearBotonTitleBar("✕", titleBar.Width - 40, true);
            btnClose.Click += (s, e) => this.Close();

            titleBar.Controls.Add(btnMin);
            titleBar.Controls.Add(btnClose);

            Label lblCarrito = new Label
            {
                Text = "\uE7BF",
                Font = new Font("Segoe MDL2 Assets", 18),
                ForeColor = Color.FromArgb(120, 185, 245),
                BackColor = CardColor,
                AutoSize = false,
                Size = new Size(HeaderWidth, 28),
                Location = new Point(HeaderX, 46),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblTitulo = new Label
            {
                Text = "Sistema POS",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(240, 247, 255),
                BackColor = CardColor,
                AutoSize = false,
                Size = new Size(HeaderWidth, 36),
                Location = new Point(HeaderX, 74),
                TextAlign = ContentAlignment.MiddleCenter
            };
            lblTitulo.UseCompatibleTextRendering = true;

            Label lblSubtitulo = new Label
            {
                Text = "Punto de Venta",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(163, 184, 209),
                BackColor = CardColor,
                AutoSize = false,
                Size = new Size(HeaderWidth, 20),
                Location = new Point(HeaderX, 110),
                TextAlign = ContentAlignment.MiddleCenter
            };
            lblSubtitulo.UseCompatibleTextRendering = true;

            Panel separador = new Panel
            {
                Size = new Size(HeaderWidth, 1),
                Location = new Point(HeaderX, 140),
                BackColor = DividerColor
            };

            Label lblUsuario = new Label
            {
                Text = "USUARIO",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = LabelColor,
                BackColor = CardColor,
                AutoSize = true,
                Location = new Point(FieldX, 156)
            };

            RoundedPanel wrapUsuario = CrearWrapperCampo(FieldX, 174, FieldWidth, 46);
            txtUsuario = new ModernTextBox("Ingrese su usuario", "👤")
            {
                Size = new Size(FieldWidth, 46),
                Location = Point.Empty,
                Margin = Padding.Empty
            };
            txtUsuario.InnerKeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    txtContraseña.Focus();
                    e.SuppressKeyPress = true;
                }
            };
            wrapUsuario.Controls.Add(txtUsuario);
            HookFocusBorder(txtUsuario.InnerTextBox, wrapUsuario);

            Label lblContraseña = new Label
            {
                Text = "CONTRASEÑA",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = LabelColor,
                BackColor = CardColor,
                AutoSize = true,
                Location = new Point(FieldX, 238)
            };

            RoundedPanel wrapContraseña = CrearWrapperCampo(FieldX, 256, FieldWidth, 46);
            txtContraseña = new ModernTextBox("Ingrese su contraseña", "🔒")
            {
                Size = new Size(FieldWidth - 44, 46),
                Location = Point.Empty,
                Margin = Padding.Empty
            };
            txtContraseña.UseSystemPasswordChar = true;
            txtContraseña.InnerKeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    BtnIniciarSesion_Click(s, e);
                    e.SuppressKeyPress = true;
                }
            };

            btnVerClave = new Button
            {
                Text = "\U0001F441",
                Size = new Size(44, 46),
                Location = new Point(FieldWidth - 44, 0),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 10),
                BackColor = InputColor,
                ForeColor = MutedTextColor,
                Cursor = Cursors.Hand,
                TabStop = false,
                Margin = Padding.Empty
            };
            btnVerClave.FlatAppearance.BorderSize = 0;
            btnVerClave.FlatAppearance.MouseOverBackColor = Color.FromArgb(56, 71, 94);
            btnVerClave.Click += BtnVerClave_Click;

            wrapContraseña.Controls.Add(txtContraseña);
            wrapContraseña.Controls.Add(btnVerClave);
            HookFocusBorder(txtContraseña.InnerTextBox, wrapContraseña);

            wrapContraseña.Resize += (s, e) =>
            {
                txtContraseña.Size = new Size(Math.Max(120, wrapContraseña.Width - 44), wrapContraseña.Height);
                btnVerClave.Location = new Point(wrapContraseña.Width - 44, 0);
                btnVerClave.Height = wrapContraseña.Height;
            };

            lblError = new Label
            {
                Text = string.Empty,
                Font = new Font("Segoe UI", 9),
                ForeColor = UITheme.DangerColor,
                BackColor = CardColor,
                AutoSize = false,
                Size = new Size(FieldWidth, 22),
                Location = new Point(FieldX, 310),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };

            btnIniciarSesion = new RoundedButton
            {
                Text = "INICIAR SESIÓN",
                Size = new Size(FieldWidth, 50),
                Location = new Point(FieldX, 338),
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ButtonColor = PrimaryButtonColor,
                HoverColor = PrimaryButtonHover,
                PressColor = PrimaryButtonPress,
                Radius = 9
            };
            btnIniciarSesion.Click += BtnIniciarSesion_Click;

            Panel sepBottom = new Panel
            {
                Size = new Size(PanelWidth - 60, 1),
                Location = new Point(30, 400),
                BackColor = DividerColor
            };

            PictureBox picLogo = new PictureBox
            {
                Size = new Size(160, 58),
                Location = new Point((PanelWidth - 160) / 2, 410),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = CardColor
            };
            picLogo.Image = CargarLogoSeguro();

            Label lblLogoNoDisponible = new Label
            {
                Text = "Logo no disponible",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = MutedTextColor,
                BackColor = CardColor,
                AutoSize = false,
                Size = new Size(PanelWidth - ((CardInset + 2) * 2), 16),
                Location = new Point(CardInset + 2, 470),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = picLogo.Image == null
            };

            Label lblDesarrollado = new Label
            {
                Text = "Desarrollado por Ubanzync SpA",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.FromArgb(109, 136, 171),
                BackColor = CardColor,
                AutoSize = false,
                Size = new Size(PanelWidth - ((CardInset + 2) * 2), 18),
                Location = new Point(CardInset + 2, 492),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblVersion = new Label
            {
                Text = "v1.0.0",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(88, 110, 141),
                BackColor = CardColor,
                AutoSize = false,
                Size = new Size(PanelWidth - ((CardInset + 2) * 2), 18),
                Location = new Point(CardInset + 2, 512),
                TextAlign = ContentAlignment.MiddleCenter
            };

            panelCentral.Controls.Add(titleBar);
            panelCentral.Controls.Add(lblCarrito);
            panelCentral.Controls.Add(lblTitulo);
            panelCentral.Controls.Add(lblSubtitulo);
            panelCentral.Controls.Add(separador);
            panelCentral.Controls.Add(lblUsuario);
            panelCentral.Controls.Add(wrapUsuario);
            panelCentral.Controls.Add(lblContraseña);
            panelCentral.Controls.Add(wrapContraseña);
            panelCentral.Controls.Add(lblError);
            panelCentral.Controls.Add(btnIniciarSesion);
            panelCentral.Controls.Add(sepBottom);
            panelCentral.Controls.Add(picLogo);
            panelCentral.Controls.Add(lblLogoNoDisponible);
            panelCentral.Controls.Add(lblDesarrollado);
            panelCentral.Controls.Add(lblVersion);

            fondoGradiente.Controls.Add(panelCentral);
            this.Controls.Add(fondoGradiente);

            this.Load += (s, e) => txtUsuario.Focus();
        }

        private Button CrearBotonTitleBar(string texto, int x, bool esPeligroso)
        {
            Button btn = new Button
            {
                Text = texto,
                Size = new Size(36, 28),
                Location = new Point(x, 8),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = CardColor,
                ForeColor = MutedTextColor,
                Cursor = Cursors.Hand,
                TabStop = false
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = esPeligroso
                ? UITheme.DangerColor
                : InputColor;
            btn.MouseEnter += (s, e) => btn.ForeColor = Color.FromArgb(230, 237, 245);
            btn.MouseLeave += (s, e) => btn.ForeColor = MutedTextColor;
            return btn;
        }

        private RoundedPanel CrearWrapperCampo(int x, int y, int w, int h)
        {
            return new RoundedPanel
            {
                Size = new Size(w, h),
                Location = new Point(x, y),
                BackColor = InputColor,
                Padding = Padding.Empty,
                Margin = Padding.Empty,
                Radius = 9,
                BorderColor = InputBorderColor,
                BorderWidth = 1
            };
        }

        private Image CargarLogoSeguro()
        {
            foreach (string ruta in ObtenerRutasLogo())
            {
                if (!File.Exists(ruta))
                {
                    continue;
                }

                try
                {
                    if (EsArchivoWebP(ruta))
                    {
                        Image webp = CargarWebPConWic(ruta);
                        if (webp != null)
                        {
                            return webp;
                        }

                        continue;
                    }

                    using (FileStream fs = new FileStream(ruta, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (Image img = Image.FromStream(fs, false, false))
                    {
                        return new Bitmap(img);
                    }
                }
                catch
                {
                    // Intentar con la siguiente ruta disponible.
                }
            }

            return null;
        }

        private Image CargarWebPConWic(string ruta)
        {
            try
            {
                Uri uri = new Uri(ruta, UriKind.Absolute);
                BitmapDecoder decoder = BitmapDecoder.Create(
                    uri,
                    BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.OnLoad);

                if (decoder.Frames.Count == 0)
                {
                    return null;
                }

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(decoder.Frames[0]));

                using (MemoryStream ms = new MemoryStream())
                {
                    encoder.Save(ms);
                    ms.Position = 0;
                    using (Image img = Image.FromStream(ms, false, false))
                    {
                        return new Bitmap(img);
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private IEnumerable<string> ObtenerRutasLogo()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string startup = Application.StartupPath;
            string up1 = Path.GetFullPath(Path.Combine(baseDir, ".."));
            string up2 = Path.GetFullPath(Path.Combine(baseDir, "..", ".."));
            string up3 = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));

            return new[]
            {
                Path.Combine(baseDir, "Resources", "logo_real.png"),
                Path.Combine(baseDir, "Resources", "logo.png"),
                Path.Combine(startup, "Resources", "logo_real.png"),
                Path.Combine(startup, "Resources", "logo.png"),
                Path.Combine(baseDir, "logo_real.png"),
                Path.Combine(baseDir, "logo.png"),
                Path.Combine(startup, "logo_real.png"),
                Path.Combine(startup, "logo.png"),
                Path.Combine(up1, "Resources", "logo.png"),
                Path.Combine(up1, "Resources", "logo_real.png"),
                Path.Combine(up2, "Resources", "logo.png"),
                Path.Combine(up2, "Resources", "logo_real.png"),
                Path.Combine(up3, "Resources", "logo.png"),
                Path.Combine(up3, "Resources", "logo_real.png")
            };
        }

        private bool EsArchivoWebP(string ruta)
        {
            try
            {
                byte[] header = new byte[12];
                using (FileStream fs = new FileStream(ruta, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    if (fs.Read(header, 0, header.Length) < header.Length)
                    {
                        return false;
                    }
                }

                bool esRiff = header[0] == (byte)'R' && header[1] == (byte)'I' &&
                              header[2] == (byte)'F' && header[3] == (byte)'F';
                bool esWebp = header[8] == (byte)'W' && header[9] == (byte)'E' &&
                              header[10] == (byte)'B' && header[11] == (byte)'P';

                return esRiff && esWebp;
            }
            catch
            {
                return false;
            }
        }

        private sealed class GradientBackgroundPanel : Panel
        {
            public Color TopColor { get; set; }
            public Color BottomColor { get; set; }

            public GradientBackgroundPanel()
            {
                TopColor = Color.FromArgb(18, 25, 40);
                BottomColor = Color.FromArgb(8, 12, 22);
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                UpdateStyles();
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    ClientRectangle,
                    TopColor,
                    BottomColor,
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, ClientRectangle);
                }
            }
        }

        private void HookFocusBorder(TextBox inner, RoundedPanel wrapper)
        {
            inner.GotFocus += (s, e) =>
            {
                wrapper.BorderColor = InputFocusBorderColor;
                wrapper.Invalidate();
            };
            inner.LostFocus += (s, e) =>
            {
                wrapper.BorderColor = InputBorderColor;
                wrapper.Invalidate();
            };
        }

        private void EnableDrag(Control control)
        {
            Point dragStartScreen = Point.Empty;
            Point formStartPos = Point.Empty;
            bool dragging = false;

            control.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    dragging = true;
                    dragStartScreen = control.PointToScreen(e.Location);
                    formStartPos = this.Location;
                }
            };
            control.MouseMove += (s, e) =>
            {
                if (dragging)
                {
                    Point cur = control.PointToScreen(e.Location);
                    this.Location = new Point(
                        formStartPos.X + cur.X - dragStartScreen.X,
                        formStartPos.Y + cur.Y - dragStartScreen.Y);
                }
            };
            control.MouseUp += (s, e) => { dragging = false; };
        }

        private void BtnVerClave_Click(object sender, EventArgs e)
        {
            txtContraseña.UseSystemPasswordChar = !txtContraseña.UseSystemPasswordChar;
            btnVerClave.Text = txtContraseña.UseSystemPasswordChar ? "\U0001F441" : "\U0001F512";
        }

        private void BtnIniciarSesion_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;

            if (string.IsNullOrWhiteSpace(txtUsuario.Text) || string.IsNullOrWhiteSpace(txtContraseña.Text))
            {
                lblError.Text = "Por favor ingrese usuario y contraseña";
                lblError.Visible = true;
                return;
            }

            var usuario = UsuarioService.Autenticar(txtUsuario.Text.Trim(), txtContraseña.Text);
            if (usuario != null)
            {
                SesionActual.UsuarioActivo = usuario;
                this.Hide();

                Form formPrincipal = null;

                if (usuario.Rol == RolesPermisos.Vendedor)
                {
                    formPrincipal = new VendedorMainForm();
                }
                else if (usuario.Rol == RolesPermisos.Cajera || usuario.Rol == RolesPermisos.Supervisor)
                {
                    formPrincipal = new MainForm();
                }
                else
                {
                    lblError.Text = "Rol de usuario no reconocido";
                    lblError.Visible = true;
                    this.Show();
                    return;
                }

                formPrincipal.FormClosed += (s, args) =>
                {
                    if (SesionActual.UsuarioActivo == null)
                    {
                        txtUsuario.Clear();
                        txtContraseña.Clear();
                        lblError.Visible = false;
                        this.Show();
                    }
                    else
                    {
                        this.Close();
                    }
                };
                formPrincipal.Show();
            }
            else
            {
                lblError.Text = "Usuario o contraseña incorrectos";
                lblError.Visible = true;
                txtContraseña.Clear();
                txtContraseña.Focus();
            }
        }
    }
}
