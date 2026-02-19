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
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void InitializeComponent()
        {
            this.Text = "Sistema POS - Inicio de Sesión";
            this.Size = new Size(520, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UITheme.DarkBackground;
            this.ForeColor = UITheme.TextPrimary;

            // Panel de fondo con gradiente
            Panel fondoGradiente = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            fondoGradiente.Paint += (s, e) =>
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    fondoGradiente.ClientRectangle,
                    Color.FromArgb(26, 32, 44),
                    Color.FromArgb(17, 24, 39),
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, fondoGradiente.ClientRectangle);
                }
            };

            // Panel central con fondo surface
            RoundedPanel panelCentral = new RoundedPanel
            {
                Size = new Size(400, 480),
                BackColor = UITheme.SurfaceColor,
                Radius = 16,
                BorderColor = UITheme.BorderColor,
                BorderWidth = 1
            };
            panelCentral.Location = new Point((520 - 400) / 2 - 10, (620 - 480) / 2 - 20);

            // Icono del sistema
            Label lblIcono = new Label
            {
                Text = "\U0001F6D2",
                Font = new Font("Segoe UI Emoji", 36),
                AutoSize = false,
                Size = new Size(400, 65),
                Location = new Point(0, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Título principal
            Label lblTitulo = new Label
            {
                Text = "SISTEMA POS",
                AutoSize = false,
                Size = new Size(400, 35),
                Location = new Point(0, 85),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = UITheme.TextPrimary
            };

            // Subtítulo
            Label lblSubtitulo = new Label
            {
                Text = "Punto de Venta",
                AutoSize = false,
                Size = new Size(400, 22),
                Location = new Point(0, 118),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 11),
                ForeColor = UITheme.TextSecondary
            };

            // Línea separadora
            Panel lineaSeparadora = new Panel
            {
                Size = new Size(300, 1),
                Location = new Point(50, 155),
                BackColor = UITheme.BorderColor
            };

            // Label usuario
            Label lblUsuario = new Label
            {
                Text = "USUARIO",
                AutoSize = true,
                Location = new Point(40, 175),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UITheme.TextSecondary
            };

            // TextBox usuario moderno
            txtUsuario = new ModernTextBox("Ingrese su usuario", "\U0001F464")
            {
                Size = new Size(320, 42),
                Location = new Point(40, 198)
            };

            // Label contraseña
            Label lblContraseña = new Label
            {
                Text = "CONTRASEÑA",
                AutoSize = true,
                Location = new Point(40, 258),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UITheme.TextSecondary
            };

            // TextBox contraseña moderno
            txtContraseña = new ModernTextBox("Ingrese su contraseña", "\U0001F512")
            {
                Size = new Size(272, 42),
                Location = new Point(40, 281)
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

            // Botón mostrar/ocultar contraseña
            btnVerClave = new Button
            {
                Text = "\U0001F441",
                Size = new Size(42, 42),
                Location = new Point(318, 281),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Emoji", 10),
                BackColor = Color.FromArgb(55, 65, 81),
                ForeColor = UITheme.TextMuted,
                Cursor = Cursors.Hand
            };
            btnVerClave.FlatAppearance.BorderSize = 0;
            btnVerClave.Click += BtnVerClave_Click;

            // Mensaje de error
            lblError = new Label
            {
                Text = "",
                AutoSize = false,
                Size = new Size(320, 22),
                Location = new Point(40, 332),
                Font = new Font("Segoe UI", 9),
                ForeColor = UITheme.DangerColor,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };

            // Botón iniciar sesión moderno
            btnIniciarSesion = new RoundedButton
            {
                Text = "INICIAR SESIÓN",
                Size = new Size(320, 50),
                Location = new Point(40, 362),
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ButtonColor = UITheme.PrimaryColor,
                HoverColor = UITheme.PrimaryLight,
                PressColor = UITheme.PrimaryDark,
                Radius = 8
            };
            btnIniciarSesion.Click += BtnIniciarSesion_Click;

            // Versión
            Label lblVersion = new Label
            {
                Text = "v1.0.0",
                AutoSize = false,
                Size = new Size(400, 20),
                Location = new Point(0, 435),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8),
                ForeColor = UITheme.TextMuted
            };

            // Agregar controles al panel central
            panelCentral.Controls.Add(lblIcono);
            panelCentral.Controls.Add(lblTitulo);
            panelCentral.Controls.Add(lblSubtitulo);
            panelCentral.Controls.Add(lineaSeparadora);
            panelCentral.Controls.Add(lblUsuario);
            panelCentral.Controls.Add(txtUsuario);
            panelCentral.Controls.Add(lblContraseña);
            panelCentral.Controls.Add(txtContraseña);
            panelCentral.Controls.Add(btnVerClave);
            panelCentral.Controls.Add(lblError);
            panelCentral.Controls.Add(btnIniciarSesion);
            panelCentral.Controls.Add(lblVersion);

            fondoGradiente.Controls.Add(panelCentral);
            this.Controls.Add(fondoGradiente);

            this.Load += (s, e) => txtUsuario.Focus();
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
                else if (usuario.Rol == RolesPermisos.Cajera)
                {
                    formPrincipal = new MainForm();
                }
                else if (usuario.Rol == RolesPermisos.Supervisor)
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
