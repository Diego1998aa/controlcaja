using System;
using System.Windows.Forms;
using System.Drawing;
using SistemaPOS.Models;

namespace SistemaPOS.Forms
{
    public partial class MainForm : Form
    {
        private Panel panelMenu;
        private Panel panelContenido;
        private Label lblUsuario;

        public MainForm()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            this.Text = "Sistema POS - Menú Principal";
            ConfigurarMenuSegunRol();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Panel superior con info de usuario
            Panel panelSuperior = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(37, 37, 38)
            };

            lblUsuario = new Label
            {
                Text = $"Usuario: {SesionActual.UsuarioActivo.NombreCompleto} | Rol: {SesionActual.UsuarioActivo.Rol}",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 20),
                AutoSize = true
            };

            Button btnCerrarSesion = new Button
            {
                Text = "Cerrar Sesión",
                Font = new Font("Segoe UI", 10),
                Size = new Size(130, 35),
                Location = new Point(this.Width - 160, 12),
                BackColor = Color.FromArgb(204, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnCerrarSesion.FlatAppearance.BorderSize = 0;
            btnCerrarSesion.Click += BtnCerrarSesion_Click;

            panelSuperior.Controls.Add(lblUsuario);
            panelSuperior.Controls.Add(btnCerrarSesion);

            // Panel menú lateral
            panelMenu = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = Color.FromArgb(45, 45, 48)
            };

            // Panel de contenido
            panelContenido = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 30)
            };

            this.Controls.Add(panelContenido);
            this.Controls.Add(panelMenu);
            this.Controls.Add(panelSuperior);
        }

        private void ConfigurarMenuSegunRol()
        {
            int y = 20;

            // Botón Punto de Venta (todos)
            Button btnPuntoVenta = CrearBotonMenu("🛒 PUNTO DE VENTA", y);
            btnPuntoVenta.Click += (s, e) => AbrirFormulario(new PuntoVentaForm());
            panelMenu.Controls.Add(btnPuntoVenta);
            y += 55;

            if (SesionActual.EsSupervisor())
            {
                // Separador
                Label separador1 = new Label
                {
                    Text = "━━━━━━━━━━━━━━━━",
                    ForeColor = Color.Gray,
                    AutoSize = false,
                    Size = new Size(200, 20),
                    Location = new Point(10, y),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                panelMenu.Controls.Add(separador1);
                y += 30;

                // Botón Inventario
                Button btnInventario = CrearBotonMenu("📦 INVENTARIO", y);
                btnInventario.Click += (s, e) => AbrirFormulario(new InventarioForm());
                panelMenu.Controls.Add(btnInventario);
                y += 55;

                // Botón Reportes
                Button btnReportes = CrearBotonMenu("📊 REPORTES", y);
                btnReportes.Click += (s, e) => AbrirFormulario(new ReportesForm());
                panelMenu.Controls.Add(btnReportes);
                y += 55;

                // Botón Usuarios
                Button btnUsuarios = CrearBotonMenu("👥 USUARIOS", y);
                btnUsuarios.Click += (s, e) => AbrirFormulario(new UsuariosForm());
                panelMenu.Controls.Add(btnUsuarios);
                y += 55;

                // Separador
                Label separador2 = new Label
                {
                    Text = "━━━━━━━━━━━━━━━━",
                    ForeColor = Color.Gray,
                    AutoSize = false,
                    Size = new Size(200, 20),
                    Location = new Point(10, y),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                panelMenu.Controls.Add(separador2);
                y += 30;

                // Botón Backup
                Button btnBackup = CrearBotonMenu("💾 BACKUP", y);
                btnBackup.Click += BtnBackup_Click;
                panelMenu.Controls.Add(btnBackup);
            }

            // Mostrar mensaje de bienvenida
            MostrarBienvenida();
        }

        private Button CrearBotonMenu(string texto, int y)
        {
            Button btn = new Button
            {
                Text = texto,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(200, 45),
                Location = new Point(10, y),
                BackColor = Color.FromArgb(63, 63, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(0, 122, 204);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(63, 63, 70);
            return btn;
        }

        private void AbrirFormulario(Form formulario)
        {
            panelContenido.Controls.Clear();
            formulario.TopLevel = false;
            formulario.FormBorderStyle = FormBorderStyle.None;
            formulario.Dock = DockStyle.Fill;
            panelContenido.Controls.Add(formulario);
            formulario.Show();
        }

        private void MostrarBienvenida()
        {
            Label lblBienvenida = new Label
            {
                Text = $"¡Bienvenido, {SesionActual.UsuarioActivo.NombreCompleto}!\n\n" +
                       "Seleccione una opción del menú lateral para comenzar.",
                Font = new Font("Segoe UI", 16),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(600, 150),
                TextAlign = ContentAlignment.MiddleCenter
            };
            lblBienvenida.Location = new Point(
                (panelContenido.Width - lblBienvenida.Width) / 2,
                (panelContenido.Height - lblBienvenida.Height) / 2
            );
            panelContenido.Controls.Add(lblBienvenida);
        }

        private void BtnCerrarSesion_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "¿Está seguro que desea cerrar sesión?",
                "Cerrar Sesión",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                SesionActual.CerrarSesion();
                this.Hide();
                LoginForm loginForm = new LoginForm();
                loginForm.FormClosed += (s, args) => this.Close();
                loginForm.Show();
            }
        }

        private void BtnBackup_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Database files (*.db)|*.db",
                    FileName = $"backup_pos_{DateTime.Now:yyyyMMdd_HHmmss}.db",
                    Title = "Guardar Backup de la Base de Datos"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    DatabaseHelper.BackupDatabase(saveDialog.FileName);
                    MessageBox.Show(
                        "Backup creado exitosamente.",
                        "Backup Completado",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al crear backup: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}