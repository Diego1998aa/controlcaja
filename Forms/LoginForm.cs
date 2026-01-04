using System;
using System.Windows.Forms;
using SistemaPOS.Services;
using SistemaPOS.Models;

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
            this.Size = new System.Drawing.Size(400, 300);
            this.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);

            // Panel principal
            Panel panelPrincipal = new Panel
            {
                Size = new System.Drawing.Size(350, 220),
                Location = new System.Drawing.Point(25, 20),
                BackColor = System.Drawing.Color.FromArgb(37, 37, 38)
            };

            // Label título
            Label lblTitulo = new Label
            {
                Text = "SISTEMA POS",
                Font = new System.Drawing.Font("Segoe UI", 18, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.White,
                AutoSize = false,
                Size = new System.Drawing.Size(350, 40),
                Location = new System.Drawing.Point(0, 20),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };

            // Label usuario
            Label lblUsuario = new Label
            {
                Text = "Usuario:",
                Font = new System.Drawing.Font("Segoe UI", 10),
                ForeColor = System.Drawing.Color.White,
                AutoSize = true,
                Location = new System.Drawing.Point(30, 80)
            };

            // TextBox usuario
            txtUsuario = new TextBox
            {
                Font = new System.Drawing.Font("Segoe UI", 11),
                Size = new System.Drawing.Size(290, 30),
                Location = new System.Drawing.Point(30, 105)
            };

            // Label contraseña
            Label lblContraseña = new Label
            {
                Text = "Contraseña:",
                Font = new System.Drawing.Font("Segoe UI", 10),
                ForeColor = System.Drawing.Color.White,
                AutoSize = true,
                Location = new System.Drawing.Point(30, 140)
            };

            // TextBox contraseña
            txtContraseña = new TextBox
            {
                Font = new System.Drawing.Font("Segoe UI", 11),
                Size = new System.Drawing.Size(290, 30),
                Location = new System.Drawing.Point(30, 165),
                UseSystemPasswordChar = true
            };

            // Botón iniciar sesión
            btnIniciarSesion = new Button
            {
                Text = "INICIAR SESIÓN",
                Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold),
                Size = new System.Drawing.Size(290, 35),
                Location = new System.Drawing.Point(30, 210),
                BackColor = System.Drawing.Color.FromArgb(0, 122, 204),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnIniciarSesion.FlatAppearance.BorderSize = 0;
            btnIniciarSesion.Click += BtnIniciarSesion_Click;

            // Agregar controles
            panelPrincipal.Controls.Add(lblTitulo);
            panelPrincipal.Controls.Add(lblUsuario);
            panelPrincipal.Controls.Add(txtUsuario);
            panelPrincipal.Controls.Add(lblContraseña);
            panelPrincipal.Controls.Add(txtContraseña);
            panelPrincipal.Controls.Add(btnIniciarSesion);

            this.Controls.Add(panelPrincipal);

            // Enter para login
            txtContraseña.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    BtnIniciarSesion_Click(s, e);
                }
            };
        }

        private TextBox txtUsuario;
        private TextBox txtContraseña;
        private Button btnIniciarSesion;

        private void BtnIniciarSesion_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string contraseña = txtContraseña.Text;

            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(contraseña))
            {
                MessageBox.Show("Por favor ingrese usuario y contraseña.", "Campos Vacíos",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Usuario usuarioAutenticado = UsuarioService.Autenticar(usuario, contraseña);

            if (usuarioAutenticado != null)
            {
                SesionActual.UsuarioActivo = usuarioAutenticado;
                // SesionActual.FechaHoraInicio = DateTime.Now; // Si tienes esta propiedad en Models

                MessageBox.Show($"Bienvenido, {usuarioAutenticado.NombreCompleto}!", "Inicio de Sesión Exitoso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Abrir formulario principal
                this.Hide();
                MainForm mainForm = new MainForm();
                mainForm.FormClosed += (s, args) => this.Close();
                mainForm.Show();
            }
            else
            {
                MessageBox.Show("Usuario o contraseña incorrectos.", "Error de Autenticación",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtContraseña.Clear();
                txtContraseña.Focus();
            }
        }
    }
}