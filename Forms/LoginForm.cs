using System;
using System.Windows.Forms;
using SistemaPOS.Services;
using SistemaPOS.Models;
using SistemaPOS.Helpers;

namespace SistemaPOS.Forms
{
    public partial class LoginForm : Form
    {
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
            this.Size = new System.Drawing.Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = UITheme.PanelBackground;
            this.ForeColor = UITheme.TextPrimary;

            // Panel principal centrado
            int pWidth = 450;
            int pHeight = 350;
            Panel panelPrincipal = new Panel
            {
                Size = new System.Drawing.Size(pWidth, pHeight),
                Location = new System.Drawing.Point((600 - pWidth)/2 - 10, (500 - pHeight)/2 - 30),
                BackColor = UITheme.DarkBackground
            };

            // Label título
            Label lblTitulo = new Label
            {
                Text = "SISTEMA POS",
                AutoSize = false,
                Size = new System.Drawing.Size(pWidth, 50),
                Location = new System.Drawing.Point(0, 20),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Segoe UI", 18, System.Drawing.FontStyle.Bold)
            };
            UITheme.StyleLabel(lblTitulo, UITheme.FontTitle);

            // Label usuario
            Label lblUsuario = new Label
            {
                Text = "Usuario:",
                AutoSize = true,
                Location = new System.Drawing.Point(50, 80),
                Font = new System.Drawing.Font("Segoe UI", 12)
            };
            UITheme.StyleLabel(lblUsuario, UITheme.FontRegular, UITheme.TextSecondary);

            // TextBox usuario
            txtUsuario = new TextBox
            {
                Size = new System.Drawing.Size(350, 35),
                Location = new System.Drawing.Point(50, 105),
                Font = new System.Drawing.Font("Segoe UI", 12)
            };
            UITheme.StyleTextBox(txtUsuario);

            // Label contraseña
            Label lblContraseña = new Label
            {
                Text = "Contraseña:",
                AutoSize = true,
                Location = new System.Drawing.Point(50, 155),
                Font = new System.Drawing.Font("Segoe UI", 12)
            };
            UITheme.StyleLabel(lblContraseña, UITheme.FontRegular, UITheme.TextSecondary);

            // TextBox contraseña
            txtContraseña = new TextBox
            {
                Size = new System.Drawing.Size(310, 35),
                Location = new System.Drawing.Point(50, 180),
                UseSystemPasswordChar = true,
                Font = new System.Drawing.Font("Segoe UI", 12)
            };
            UITheme.StyleTextBox(txtContraseña);

            // Botón mostrar/ocultar contraseña
            btnVerClave = new Button
            {
                Text = "👁",
                Size = new System.Drawing.Size(35, txtContraseña.Height),
                Location = new System.Drawing.Point(365, 180),
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 7),
                BackColor = System.Drawing.Color.FromArgb(80, 80, 80),
                ForeColor = System.Drawing.Color.White,
                Cursor = Cursors.Hand
            };
            btnVerClave.FlatAppearance.BorderSize = 0;
            btnVerClave.Click += BtnVerClave_Click;

            // Botón iniciar sesión
            btnIniciarSesion = new Button
            {
                Text = "INICIAR SESIÓN",
                Size = new System.Drawing.Size(350, 50),
                Location = new System.Drawing.Point(50, 250),
                Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold)
            };
            UITheme.StyleButton(btnIniciarSesion, UITheme.PrimaryColor);
            btnIniciarSesion.Click += BtnIniciarSesion_Click;

            // Agregar controles
            panelPrincipal.Controls.Add(lblTitulo);
            panelPrincipal.Controls.Add(lblUsuario);
            panelPrincipal.Controls.Add(txtUsuario);
            panelPrincipal.Controls.Add(lblContraseña);
            panelPrincipal.Controls.Add(txtContraseña);
            panelPrincipal.Controls.Add(btnVerClave);
            panelPrincipal.Controls.Add(btnIniciarSesion);
            
            this.Controls.Add(panelPrincipal);
            this.AcceptButton = btnIniciarSesion;
        }

        private TextBox txtUsuario;
        private TextBox txtContraseña;
        private Button btnIniciarSesion;
        private Button btnVerClave;

        private void BtnVerClave_Click(object sender, EventArgs e)
        {
            txtContraseña.UseSystemPasswordChar = !txtContraseña.UseSystemPasswordChar;
            btnVerClave.Text = txtContraseña.UseSystemPasswordChar ? "👁" : "🔒";
        }

        private void BtnIniciarSesion_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsuario.Text) || string.IsNullOrWhiteSpace(txtContraseña.Text))
            {
                MessageBox.Show("Por favor ingrese usuario y contraseña", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var usuario = UsuarioService.Autenticar(txtUsuario.Text, txtContraseña.Text);
            if (usuario != null)
            {
                SesionActual.UsuarioActivo = usuario;
                this.Hide();

                // FLUJO SEGÚN ROL
                Form formPrincipal = null;

                if (usuario.Rol == RolesPermisos.Vendedor)
                {
                    // Vendedor va directo a Terminal de Venta
                    formPrincipal = new TerminalVentaForm();
                }
                else if (usuario.Rol == RolesPermisos.Cajera)
                {
                    // Cajera va a MainForm (cola de cobro)
                    formPrincipal = new MainForm();
                }
                else if (usuario.Rol == RolesPermisos.Supervisor)
                {
                    // Supervisor va a MainForm con acceso completo
                    formPrincipal = new MainForm();
                }
                else
                {
                    MessageBox.Show("Rol de usuario no reconocido", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Show();
                    return;
                }

                formPrincipal.FormClosed += (s, args) =>
                {
                    if (SesionActual.UsuarioActivo == null)
                    {
                        // Cerrar sesión: volver a mostrar login
                        txtUsuario.Clear();
                        txtContraseña.Clear();
                        this.Show();
                    }
                    else
                    {
                        // Cierre normal de la app
                        this.Close();
                    }
                };
                formPrincipal.Show();
            }
            else
            {
                MessageBox.Show("Usuario o contraseña incorrectos", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
