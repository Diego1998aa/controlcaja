using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
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

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Sistema POS - Inicio de Sesión";
            this.Size = new Size(460, 580);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(15, 20, 30);
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;

            // ─── FONDO GRADIENTE ───
            Panel fondoGradiente = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            fondoGradiente.Paint += (s, e) =>
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    fondoGradiente.ClientRectangle,
                    Color.FromArgb(22, 28, 42),
                    Color.FromArgb(12, 17, 28),
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, fondoGradiente.ClientRectangle);
                }
            };
            EnableDrag(fondoGradiente);

            // ─── PANEL CENTRAL ───
            const int panelW = 400;
            const int panelH = 524;
            RoundedPanel panelCentral = new RoundedPanel
            {
                Size = new Size(panelW, panelH),
                Location = new Point((460 - panelW) / 2, (580 - panelH) / 2),
                BackColor = UITheme.SurfaceColor,
                Radius = 20,
                BorderColor = UITheme.BorderColor,
                BorderWidth = 1
            };

            // ─── BARRA DE TÍTULO PERSONALIZADA ───
            Panel titleBar = new Panel
            {
                Size = new Size(panelW, 44),
                Location = new Point(0, 0),
                BackColor = UITheme.SurfaceColor
            };
            EnableDrag(titleBar);

            // Botón minimizar
            Button btnMin = CrearBotonTitleBar("─", panelW - 78, false);
            btnMin.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            // Botón cerrar
            Button btnClose = CrearBotonTitleBar("✕", panelW - 40, true);
            btnClose.Click += (s, e) => Application.Exit();

            titleBar.Controls.Add(btnMin);
            titleBar.Controls.Add(btnClose);

            // ─── CABECERA: icono + títulos ───
            Label lblIcono = new Label
            {
                Text = "\U0001F6D2",
                Font = new Font("Segoe UI Emoji", 32),
                AutoSize = false,
                Size = new Size(panelW, 58),
                Location = new Point(0, 44),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblTitulo = new Label
            {
                Text = "SISTEMA POS",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = UITheme.TextPrimary,
                AutoSize = false,
                Size = new Size(panelW, 34),
                Location = new Point(0, 102),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblSubtitulo = new Label
            {
                Text = "Punto de Venta",
                Font = new Font("Segoe UI", 10),
                ForeColor = UITheme.TextSecondary,
                AutoSize = false,
                Size = new Size(panelW, 20),
                Location = new Point(0, 134),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Panel separador = new Panel
            {
                Size = new Size(panelW - 80, 1),
                Location = new Point(40, 166),
                BackColor = UITheme.BorderColor
            };

            // ─── CAMPO USUARIO ───
            const int fieldX = 30;
            const int fieldW = 340;

            Label lblUsuario = new Label
            {
                Text = "USUARIO",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = UITheme.TextMuted,
                AutoSize = true,
                Location = new Point(fieldX, 184)
            };

            RoundedPanel wrapUsuario = CrearWrapperCampo(fieldX, 202, fieldW, 46);
            txtUsuario = new ModernTextBox("Ingrese su usuario", "\U0001F464")
            {
                Size = new Size(fieldW, 46),
                Location = Point.Empty
            };
            wrapUsuario.Controls.Add(txtUsuario);
            HookFocusBorder(txtUsuario.InnerTextBox, wrapUsuario);

            // ─── CAMPO CONTRASEÑA ───
            Label lblContraseña = new Label
            {
                Text = "CONTRASEÑA",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = UITheme.TextMuted,
                AutoSize = true,
                Location = new Point(fieldX, 264)
            };

            RoundedPanel wrapContraseña = CrearWrapperCampo(fieldX, 282, fieldW, 46);
            txtContraseña = new ModernTextBox("Ingrese su contraseña", "\U0001F512")
            {
                Size = new Size(fieldW - 46, 46),
                Location = Point.Empty
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
                Size = new Size(44, 42),
                Location = new Point(fieldW - 45, 2),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 10),
                BackColor = Color.Transparent,
                ForeColor = UITheme.TextMuted,
                Cursor = Cursors.Hand
            };
            btnVerClave.FlatAppearance.BorderSize = 0;
            btnVerClave.FlatAppearance.MouseOverBackColor = Color.FromArgb(65, 75, 91);
            btnVerClave.Click += BtnVerClave_Click;

            wrapContraseña.Controls.Add(txtContraseña);
            wrapContraseña.Controls.Add(btnVerClave);
            HookFocusBorder(txtContraseña.InnerTextBox, wrapContraseña);

            // ─── MENSAJE DE ERROR ───
            lblError = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = UITheme.DangerColor,
                AutoSize = false,
                Size = new Size(fieldW, 22),
                Location = new Point(fieldX, 340),
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };

            // ─── BOTÓN INICIAR SESIÓN ───
            btnIniciarSesion = new RoundedButton
            {
                Text = "INICIAR SESIÓN",
                Size = new Size(fieldW, 50),
                Location = new Point(fieldX, 370),
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ButtonColor = UITheme.PrimaryColor,
                HoverColor = UITheme.PrimaryLight,
                PressColor = UITheme.PrimaryDark,
                Radius = 10
            };
            btnIniciarSesion.Click += BtnIniciarSesion_Click;

            // ─── VERSIÓN ───
            Label lblVersion = new Label
            {
                Text = "v1.0.0",
                Font = new Font("Segoe UI", 8),
                ForeColor = UITheme.TextMuted,
                AutoSize = false,
                Size = new Size(panelW, 20),
                Location = new Point(0, 468),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // ─── ENSAMBLAR PANEL CENTRAL ───
            panelCentral.Controls.Add(titleBar);
            panelCentral.Controls.Add(lblIcono);
            panelCentral.Controls.Add(lblTitulo);
            panelCentral.Controls.Add(lblSubtitulo);
            panelCentral.Controls.Add(separador);
            panelCentral.Controls.Add(lblUsuario);
            panelCentral.Controls.Add(wrapUsuario);
            panelCentral.Controls.Add(lblContraseña);
            panelCentral.Controls.Add(wrapContraseña);
            panelCentral.Controls.Add(lblError);
            panelCentral.Controls.Add(btnIniciarSesion);
            panelCentral.Controls.Add(lblVersion);

            fondoGradiente.Controls.Add(panelCentral);
            this.Controls.Add(fondoGradiente);

            this.Load += (s, e) => txtUsuario.Focus();
        }

        // ─── HELPERS DE CONSTRUCCIÓN ───

        private Button CrearBotonTitleBar(string texto, int x, bool esPeligroso)
        {
            Button btn = new Button
            {
                Text = texto,
                Size = new Size(36, 28),
                Location = new Point(x, 8),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = UITheme.SurfaceColor,
                ForeColor = UITheme.TextMuted,
                Cursor = Cursors.Hand,
                TabStop = false
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = esPeligroso
                ? UITheme.DangerColor
                : Color.FromArgb(55, 65, 81);
            btn.MouseEnter += (s, e) => btn.ForeColor = Color.White;
            btn.MouseLeave += (s, e) => btn.ForeColor = UITheme.TextMuted;
            return btn;
        }

        private RoundedPanel CrearWrapperCampo(int x, int y, int w, int h)
        {
            return new RoundedPanel
            {
                Size = new Size(w, h),
                Location = new Point(x, y),
                BackColor = Color.FromArgb(55, 65, 81),
                Radius = 8,
                BorderColor = UITheme.BorderColor,
                BorderWidth = 1
            };
        }

        private void HookFocusBorder(TextBox inner, RoundedPanel wrapper)
        {
            inner.GotFocus += (s, e) =>
            {
                wrapper.BorderColor = UITheme.PrimaryColor;
                wrapper.Invalidate();
            };
            inner.LostFocus += (s, e) =>
            {
                wrapper.BorderColor = UITheme.BorderColor;
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

        // ─── LÓGICA DE NEGOCIO ───

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

            var usuario = UsuarioService.Autenticar(txtUsuario.Text, txtContraseña.Text);
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
