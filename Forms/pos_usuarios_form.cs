using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using SistemaPOS.Models;
using SistemaPOS.Services;

namespace SistemaPOS.Forms
{
    public partial class UsuariosForm : Form
    {
        private DataGridView dgvUsuarios;

        public UsuariosForm()
        {
            InitializeComponent();
            CargarUsuarios();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1000, 650);
            this.BackColor = Color.FromArgb(30, 30, 30);

            // Panel superior
            Panel panelSuperior = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(970, 60),
                BackColor = Color.FromArgb(37, 37, 38)
            };

            Label lblTitulo = new Label
            {
                Text = "GESTIÓN DE USUARIOS",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 10),
                AutoSize = true
            };
            panelSuperior.Controls.Add(lblTitulo);

            Button btnNuevoUsuario = new Button
            {
                Text = "➕ Nuevo Usuario",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(800, 12),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnNuevoUsuario.FlatAppearance.BorderSize = 0;
            btnNuevoUsuario.Click += BtnNuevoUsuario_Click;
            panelSuperior.Controls.Add(btnNuevoUsuario);

            this.Controls.Add(panelSuperior);

            // DataGridView de usuarios
            dgvUsuarios = new DataGridView
            {
                Location = new Point(10, 80),
                Size = new Size(970, 480),
                BackgroundColor = Color.FromArgb(45, 45, 48),
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };
            ConfigurarEstiloGrid();
            this.Controls.Add(dgvUsuarios);

            // Botones de acción
            Button btnEditar = new Button
            {
                Text = "✏️ Editar",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(10, 570),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(255, 140, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnEditar.FlatAppearance.BorderSize = 0;
            btnEditar.Click += BtnEditar_Click;
            this.Controls.Add(btnEditar);

            Button btnCambiarPassword = new Button
            {
                Text = "🔑 Cambiar Contraseña",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(170, 570),
                Size = new Size(180, 40),
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCambiarPassword.FlatAppearance.BorderSize = 0;
            btnCambiarPassword.Click += BtnCambiarPassword_Click;
            this.Controls.Add(btnCambiarPassword);

            Button btnDesactivar = new Button
            {
                Text = "🚫 Desactivar",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(360, 570),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(139, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDesactivar.FlatAppearance.BorderSize = 0;
            btnDesactivar.Click += BtnDesactivar_Click;
            this.Controls.Add(btnDesactivar);

            Button btnActualizar = new Button
            {
                Text = "🔄 Actualizar",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(820, 570),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnActualizar.FlatAppearance.BorderSize = 0;
            btnActualizar.Click += (s, e) => CargarUsuarios();
            this.Controls.Add(btnActualizar);
        }

        private void ConfigurarEstiloGrid()
        {
            dgvUsuarios.DefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
            dgvUsuarios.DefaultCellStyle.ForeColor = Color.White;
            dgvUsuarios.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);
            dgvUsuarios.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvUsuarios.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 37, 38);
            dgvUsuarios.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvUsuarios.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvUsuarios.EnableHeadersVisualStyles = false;

            dgvUsuarios.Columns.Clear();
            dgvUsuarios.Columns.Add("IdUsuario", "ID");
            dgvUsuarios.Columns.Add("Usuario", "Usuario");
            dgvUsuarios.Columns.Add("NombreCompleto", "Nombre Completo");
            dgvUsuarios.Columns.Add("Rol", "Rol");
            dgvUsuarios.Columns.Add("FechaCreacion", "Fecha Creación");
            dgvUsuarios.Columns.Add("UltimoAcceso", "Último Acceso");
            dgvUsuarios.Columns.Add("Estado", "Estado");

            dgvUsuarios.Columns[0].Width = 50;
            dgvUsuarios.Columns[0].Visible = false;
            dgvUsuarios.Columns[1].Width = 120;
            dgvUsuarios.Columns[3].Width = 100;
            dgvUsuarios.Columns[4].Width = 130;
            dgvUsuarios.Columns[5].Width = 150;
            dgvUsuarios.Columns[6].Width = 80;
        }

        private void CargarUsuarios()
        {
            dgvUsuarios.Rows.Clear();
            List<Usuario> usuarios = UsuarioService.ObtenerTodosLosUsuarios();

            foreach (var u in usuarios)
            {
                string ultimoAcceso = u.UltimoAcceso.HasValue 
                    ? u.UltimoAcceso.Value.ToString("dd/MM/yyyy HH:mm") 
                    : "Nunca";

                string estado = u.Activo ? "Activo" : "Inactivo";
                Color colorEstado = u.Activo ? Color.Green : Color.Red;

                int index = dgvUsuarios.Rows.Add(
                    u.IdUsuario,
                    u.NombreUsuario,
                    u.NombreCompleto,
                    u.Rol,
                    u.FechaCreacion.ToString("dd/MM/yyyy"),
                    ultimoAcceso,
                    estado
                );

                dgvUsuarios.Rows[index].Cells[6].Style.ForeColor = colorEstado;
                dgvUsuarios.Rows[index].Cells[6].Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);

                // Resaltar usuario actual
                if (u.IdUsuario == SesionActual.UsuarioActivo.IdUsuario)
                {
                    dgvUsuarios.Rows[index].DefaultCellStyle.BackColor = Color.FromArgb(60, 60, 70);
                }
            }
        }

        private void BtnNuevoUsuario_Click(object sender, EventArgs e)
        {
            UsuarioEditForm editForm = new UsuarioEditForm();
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                CargarUsuarios();
                MessageBox.Show("Usuario creado exitosamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            if (dgvUsuarios.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccione un usuario.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idUsuario = Convert.ToInt32(dgvUsuarios.SelectedRows[0].Cells[0].Value);
            UsuarioEditForm editForm = new UsuarioEditForm(idUsuario);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                CargarUsuarios();
                MessageBox.Show("Usuario actualizado exitosamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnCambiarPassword_Click(object sender, EventArgs e)
        {
            if (dgvUsuarios.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccione un usuario.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idUsuario = Convert.ToInt32(dgvUsuarios.SelectedRows[0].Cells[0].Value);
            string nombreUsuario = dgvUsuarios.SelectedRows[0].Cells[2].Value.ToString();

            CambiarPasswordForm passwordForm = new CambiarPasswordForm(idUsuario, nombreUsuario);
            if (passwordForm.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("Contraseña cambiada exitosamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDesactivar_Click(object sender, EventArgs e)
        {
            if (dgvUsuarios.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccione un usuario.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idUsuario = Convert.ToInt32(dgvUsuarios.SelectedRows[0].Cells[0].Value);
            
            // No permitir desactivar al usuario actual
            if (idUsuario == SesionActual.UsuarioActivo.IdUsuario)
            {
                MessageBox.Show("No puede desactivar su propio usuario.", "Operación No Permitida",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                "¿Está seguro que desea desactivar este usuario?",
                "Confirmar Desactivación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                if (UsuarioService.EliminarUsuario(idUsuario))
                {
                    CargarUsuarios();
                    MessageBox.Show("Usuario desactivado exitosamente.", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Error al desactivar el usuario.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    // Formulario para crear/editar usuarios
    public partial class UsuarioEditForm : Form
    {
        private int? idUsuario;
        private TextBox txtUsuario, txtNombreCompleto, txtPassword, txtConfirmarPassword;
        private ComboBox cboRol;
        private CheckBox chkActivo;

        public UsuarioEditForm(int? idUsuario = null)
        {
            this.idUsuario = idUsuario;
            InitializeComponent();
            
            if (idUsuario.HasValue)
            {
                CargarUsuario();
            }
        }

        private void InitializeComponent()
        {
            this.Text = idUsuario.HasValue ? "Editar Usuario" : "Nuevo Usuario";
            this.Size = new Size(450, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(37, 37, 38);

            Panel panelPrincipal = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(390, 330),
                BackColor = Color.FromArgb(45, 45, 48)
            };

            int y = 20;

            Label lblTitulo = new Label
            {
                Text = idUsuario.HasValue ? "EDITAR USUARIO" : "NUEVO USUARIO",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, y),
                AutoSize = false,
                Size = new Size(350, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            panelPrincipal.Controls.Add(lblTitulo);
            y += 50;

            panelPrincipal.Controls.Add(CrearLabel("Usuario:", 20, y));
            txtUsuario = CrearTextBox(20, y + 25, 350);
            txtUsuario.Enabled = !idUsuario.HasValue; // No editable si es edición
            panelPrincipal.Controls.Add(txtUsuario);
            y += 60;

            panelPrincipal.Controls.Add(CrearLabel("Nombre Completo:", 20, y));
            txtNombreCompleto = CrearTextBox(20, y + 25, 350);
            panelPrincipal.Controls.Add(txtNombreCompleto);
            y += 60;

            if (!idUsuario.HasValue) // Solo para nuevos usuarios
            {
                panelPrincipal.Controls.Add(CrearLabel("Contraseña:", 20, y));
                txtPassword = CrearTextBox(20, y + 25, 350);
                txtPassword.UseSystemPasswordChar = true;
                panelPrincipal.Controls.Add(txtPassword);
                y += 50;

                panelPrincipal.Controls.Add(CrearLabel("Confirmar Contraseña:", 20, y));
                txtConfirmarPassword = CrearTextBox(20, y + 25, 350);
                txtConfirmarPassword.UseSystemPasswordChar = true;
                panelPrincipal.Controls.Add(txtConfirmarPassword);
                y += 60;
            }

            panelPrincipal.Controls.Add(CrearLabel("Rol:", 20, y));
            cboRol = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, y + 25),
                Size = new Size(170, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboRol.Items.AddRange(new string[] { "Cajero", "Supervisor" });
            cboRol.SelectedIndex = 0;
            panelPrincipal.Controls.Add(cboRol);

            if (idUsuario.HasValue)
            {
                chkActivo = new CheckBox
                {
                    Text = "Usuario Activo",
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.White,
                    Location = new Point(210, y + 28),
                    AutoSize = true,
                    Checked = true
                };
                panelPrincipal.Controls.Add(chkActivo);
            }
            y += 60;

            Button btnGuardar = new Button
            {
                Text = "💾 GUARDAR",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, y),
                Size = new Size(165, 40),
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;
            panelPrincipal.Controls.Add(btnGuardar);

            Button btnCancelar = new Button
            {
                Text = "✗ CANCELAR",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(205, y),
                Size = new Size(165, 40),
                BackColor = Color.FromArgb(139, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            panelPrincipal.Controls.Add(btnCancelar);

            this.Controls.Add(panelPrincipal);
        }

        private Label CrearLabel(string texto, int x, int y)
        {
            return new Label
            {
                Text = texto,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(x, y),
                AutoSize = true
            };
        }

        private TextBox CrearTextBox(int x, int y, int width)
        {
            return new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(x, y),
                Size = new Size(width, 25)
            };
        }

        private void CargarUsuario()
        {
            var usuarios = UsuarioService.ObtenerTodosLosUsuarios();
            var usuario = usuarios.Find(u => u.IdUsuario == idUsuario.Value);

            if (usuario != null)
            {
                txtUsuario.Text = usuario.NombreUsuario;
                txtNombreCompleto.Text = usuario.NombreCompleto;
                cboRol.SelectedItem = usuario.Rol;
                if (chkActivo != null)
                    chkActivo.Checked = usuario.Activo;
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsuario.Text))
            {
                MessageBox.Show("El nombre de usuario es obligatorio.", "Campo Requerido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNombreCompleto.Text))
            {
                MessageBox.Show("El nombre completo es obligatorio.", "Campo Requerido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!idUsuario.HasValue)
            {
                if (string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("La contraseña es obligatoria.", "Campo Requerido",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (txtPassword.Text != txtConfirmarPassword.Text)
                {
                    MessageBox.Show("Las contraseñas no coinciden.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                bool exito = UsuarioService.CrearUsuario(
                    txtUsuario.Text.Trim(),
                    txtPassword.Text,
                    txtNombreCompleto.Text.Trim(),
                    cboRol.SelectedItem.ToString()
                );

                if (exito)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Error al crear el usuario. El nombre de usuario podría estar duplicado.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                bool exito = UsuarioService.ActualizarUsuario(
                    idUsuario.Value,
                    txtNombreCompleto.Text.Trim(),
                    cboRol.SelectedItem.ToString(),
                    chkActivo.Checked
                );

                if (exito)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Error al actualizar el usuario.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    // Formulario para cambiar contraseña
    public partial class CambiarPasswordForm : Form
    {
        private int idUsuario;
        private TextBox txtNuevaPassword, txtConfirmarPassword;

        public CambiarPasswordForm(int idUsuario, string nombreUsuario)
        {
            this.idUsuario = idUsuario;
            InitializeComponent(nombreUsuario);
        }

        private void InitializeComponent(string nombreUsuario)
        {
            this.Text = "Cambiar Contraseña";
            this.Size = new Size(400, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(37, 37, 38);

            Panel panelPrincipal = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(340, 210),
                BackColor = Color.FromArgb(45, 45, 48)
            };

            Label lblTitulo = new Label
            {
                Text = $"Cambiar contraseña de:\n{nombreUsuario}",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 20),
                AutoSize = false,
                Size = new Size(300, 50),
                TextAlign = ContentAlignment.MiddleCenter
            };
            panelPrincipal.Controls.Add(lblTitulo);

            Label lblNueva = new Label
            {
                Text = "Nueva Contraseña:",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(20, 80),
                AutoSize = true
            };
            panelPrincipal.Controls.Add(lblNueva);

            txtNuevaPassword = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 105),
                Size = new Size(300, 25),
                UseSystemPasswordChar = true
            };
            panelPrincipal.Controls.Add(txtNuevaPassword);

            Label lblConfirmar = new Label
            {
                Text = "Confirmar Contraseña:",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(20, 135),
                AutoSize = true
            };
            panelPrincipal.Controls.Add(lblConfirmar);

            txtConfirmarPassword = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 160),
                Size = new Size(300, 25),
                UseSystemPasswordChar = true
            };
            panelPrincipal.Controls.Add(txtConfirmarPassword);

            Button btnGuardar = new Button
            {
                Text = "💾 CAMBIAR",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 195),
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;
            panelPrincipal.Controls.Add(btnGuardar);

            Button btnCancelar = new Button
            {
                Text = "✗ CANCELAR",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(180, 195),
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(139, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            panelPrincipal.Controls.Add(btnCancelar);

            this.Controls.Add(panelPrincipal);
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNuevaPassword.Text))
            {
                MessageBox.Show("Ingrese la nueva contraseña.", "Campo Requerido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtNuevaPassword.Text != txtConfirmarPassword.Text)
            {
                MessageBox.Show("Las contraseñas no coinciden.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (UsuarioService.ResetearContraseña(idUsuario, txtNuevaPassword.Text))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Error al cambiar la contraseña.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}