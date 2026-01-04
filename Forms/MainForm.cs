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
            };:oUs.Rol}",
 = new Font("Segoe UI", 11, FontStyle.Bold),
                              Location = new Point(20, 20),

            };

               reColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                              Anchor = AnchorStyles.Top | AnchorStyles.Right

            p
r = Color.FromArgb(45, 45, 48)
            };ck = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 30)
            };

            this.Controls. 

Menu("🛒 PUNTO DE VENTA", y);
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
            Cra= 30;
       
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
             ━━ ForeColor = Color.Gray,           AutoSize = false,
       (200, 20),
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
                Text = texto, Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(200, 45), Location = new Point(10, y),
                BackColor = Color.FromArgb(63, 63, 70), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, TextAlign = ContentAlignment.MiddleLeft,
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
                Text = $"¡Bienvenido, {SesionActual.UsuarioActivo.NombreCompl Si
                Font = new Font("Segoe UI", 16),
                ForeColor = Color.White,
             600, 150),
                TextAlign = ContentAlignment.MiddleCenter
            };
            lblBienvenida.Location = new Point(
                (panelContenido.Width - lblBienvenida.Width) / 2,
                (panelContenido.Height - lblBienvenida.Height) / 2
            );
            panelContenido.Con

        private void BtnCerrarSesion_CsgeBoxButtons.YesNo,
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
            t
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "Database files (*.db)|*.db",
                    FileName = $"backup_pos_{DateTime.Now:yyyyMMdd_HHmmss}.db",
                    Title = "Guardar Backup de la Base de Datos"
                };
.ShowDialog() == DialogResult.OK)
                {
                    DatabaseHelper.Bacexo   MessageBoxIcon.Information
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