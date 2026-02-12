using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using SistemaPOS.Models;
using SistemaPOS.Services;
using SistemaPOS.Helpers;

namespace SistemaPOS.Forms
{
    public partial class UsuariosForm : Form
    {
        private DataGridView dgvUsuarios;
        private Button btnDesactivarReactivar;

        public UsuariosForm()
        {
            // VALIDACIÓN DE PERMISOS
            if (!SesionActual.TienePermiso(Permiso.VerUsuarios))
            {
                MessageBox.Show("No tiene permisos para acceder a este módulo", "Acceso Denegado",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
                return;
            }

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
                Text = "ADMINISTRACIÓN DE PERSONAL",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 10),
                AutoSize = true
            };
            panelSuperior.Controls.Add(lblTitulo);

            Button btnNuevoUsuario = new Button
            {
                Text = "Nuevo Usuario",
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
                Text = "Editar",
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
                Text = "Cambiar Contraseña",
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

            btnDesactivarReactivar = new Button
            {
                Text = "Desactivar",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(360, 570),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(139, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDesactivarReactivar.FlatAppearance.BorderSize = 0;
            btnDesactivarReactivar.Click += BtnDesactivarReactivar_Click;
            this.Controls.Add(btnDesactivarReactivar);
            dgvUsuarios.SelectionChanged += (s, e) => ActualizarBotonDesactivarReactivar();

            Button btnActualizar = new Button
            {
                Text = "Actualizar",
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
            dgvUsuarios.DoubleClick += DgvUsuarios_DoubleClick;
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
            ActualizarBotonDesactivarReactivar();
        }

        private void BtnNuevoUsuario_Click(object sender, EventArgs e)
        {
            if (!SesionActual.TienePermiso(Permiso.CrearUsuarios))
            {
                MessageBox.Show("No tiene permisos para crear usuarios", "Acceso Denegado",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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
            bool esPropioUsuario = SesionActual.UsuarioActivo != null && idUsuario == SesionActual.UsuarioActivo.IdUsuario;

            // Validar permisos
            if (esPropioUsuario && !SesionActual.TienePermiso(Permiso.CambiarPropiaContraseña))
            {
                MessageBox.Show("No tiene permisos para cambiar contraseñas", "Acceso Denegado",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!esPropioUsuario && !SesionActual.TienePermiso(Permiso.CambiarContraseñas))
            {
                MessageBox.Show("No tiene permisos para cambiar contraseñas de otros usuarios", "Acceso Denegado",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string nombreUsuario = dgvUsuarios.SelectedRows[0].Cells[2].Value.ToString();

            CambiarPasswordForm passwordForm = new CambiarPasswordForm(idUsuario, nombreUsuario);
            if (passwordForm.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("Contraseña cambiada exitosamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DgvUsuarios_DoubleClick(object sender, EventArgs e)
        {
            if (dgvUsuarios.SelectedRows.Count > 0)
                BtnEditar_Click(sender, e);
        }

        private void ActualizarBotonDesactivarReactivar()
        {
            if (btnDesactivarReactivar == null) return;
            if (dgvUsuarios.SelectedRows.Count == 0)
            {
                btnDesactivarReactivar.Text = "Desactivar";
                btnDesactivarReactivar.BackColor = Color.FromArgb(139, 0, 0);
                btnDesactivarReactivar.Enabled = false;
                return;
            }
            object valEstado = dgvUsuarios.SelectedRows[0].Cells[6].Value;
            bool activo = valEstado != null && valEstado.ToString() == "Activo";
            int idUsuario = Convert.ToInt32(dgvUsuarios.SelectedRows[0].Cells[0].Value);
            bool esUsuarioActual = SesionActual.UsuarioActivo != null && idUsuario == SesionActual.UsuarioActivo.IdUsuario;

            if (activo)
            {
                btnDesactivarReactivar.Text = "Desactivar";
                btnDesactivarReactivar.BackColor = Color.FromArgb(139, 0, 0);
                btnDesactivarReactivar.Enabled = !esUsuarioActual;
            }
            else
            {
                btnDesactivarReactivar.Text = "Reactivar";
                btnDesactivarReactivar.BackColor = Color.FromArgb(34, 139, 34);
                btnDesactivarReactivar.Enabled = true;
            }
        }

        private void BtnDesactivarReactivar_Click(object sender, EventArgs e)
        {
            if (dgvUsuarios.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccione un usuario.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idUsuario = Convert.ToInt32(dgvUsuarios.SelectedRows[0].Cells[0].Value);
            object valEstado2 = dgvUsuarios.SelectedRows[0].Cells[6].Value;
            bool activo = valEstado2 != null && valEstado2.ToString() == "Activo";

            if (activo)
            {
                if (SesionActual.UsuarioActivo != null && idUsuario == SesionActual.UsuarioActivo.IdUsuario)
                {
                    MessageBox.Show("No puede desactivar su propio usuario.", "Operación no permitida",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                DialogResult result = MessageBox.Show("¿Desactivar este usuario? No podrá iniciar sesión.", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result != DialogResult.Yes) return;
                try
                {
                    if (UsuarioService.EliminarUsuario(idUsuario))
                    {
                        CargarUsuarios();
                        MessageBox.Show("Usuario desactivado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                        MessageBox.Show("Error al desactivar.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                object valNombre = dgvUsuarios.SelectedRows[0].Cells[2].Value;
                object valRol = dgvUsuarios.SelectedRows[0].Cells[3].Value;
                string nombre = valNombre != null ? valNombre.ToString() : "";
                string rol = valRol != null ? valRol.ToString() : RolesPermisos.Vendedor;
                try
                {
                    UsuarioService.ActualizarUsuario(idUsuario, nombre, rol, true);
                    CargarUsuarios();
                    MessageBox.Show("Usuario reactivado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    // Formulario para crear/editar usuarios
    public partial class UsuarioEditForm : Form
    {
        private const int MinLongitudUsuario = 3;
        private const int MaxLongitudUsuario = 50;
        private const int MinLongitudPassword = 6;

        private int? idUsuario;
        private TextBox txtUsuario, txtNombreCompleto, txtPassword, txtConfirmarPassword;
        private ComboBox cboRol;
        private CheckBox chkActivo;
        private Label lblFechaCreacion, lblUltimoAcceso;
        private Button btnCambiarPassword;

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
            this.Size = new Size(520, 620);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            UITheme.ApplyTheme(this);

            Panel panelPrincipal = new Panel
            {
                Location = new Point(16, 16),
                Size = new Size(472, 560),
                BackColor = UITheme.PanelBackground
            };

            int y = 12;

            // Título
            Label lblTitulo = new Label
            {
                Text = idUsuario.HasValue ? "Editar usuario" : "Nuevo usuario",
                Font = UITheme.FontTitle,
                ForeColor = UITheme.TextPrimary,
                Location = new Point(16, y),
                AutoSize = true
            };
            panelPrincipal.Controls.Add(lblTitulo);
            y += 44;

            // Grupo: Datos de cuenta
            GroupBox grpCuenta = new GroupBox
            {
                Text = "  Datos de cuenta  ",
                Font = UITheme.FontSubtitle,
                ForeColor = UITheme.TextPrimary,
                Location = new Point(16, y),
                Size = new Size(440, idUsuario.HasValue ? 100 : 200),
                BackColor = UITheme.PanelBackground
            };
            int yGrp = 28;
            grpCuenta.Controls.Add(CrearLabel("Usuario (inicio de sesión):", 12, yGrp));
            txtUsuario = CrearTextBox(12, yGrp + 24, 416);
            txtUsuario.Enabled = !idUsuario.HasValue;
            txtUsuario.MaxLength = MaxLongitudUsuario;
            if (!idUsuario.HasValue)
                txtUsuario.ForeColor = Color.Silver;
            grpCuenta.Controls.Add(txtUsuario);
            yGrp += 64;

            if (!idUsuario.HasValue)
            {
                grpCuenta.Controls.Add(CrearLabel("Contraseña (mín. " + MinLongitudPassword + " caracteres):", 12, yGrp));
                txtPassword = CrearTextBox(12, yGrp + 24, 416);
                txtPassword.UseSystemPasswordChar = true;
                grpCuenta.Controls.Add(txtPassword);
                yGrp += 64;
                grpCuenta.Controls.Add(CrearLabel("Confirmar contraseña:", 12, yGrp));
                txtConfirmarPassword = CrearTextBox(12, yGrp + 24, 416);
                txtConfirmarPassword.UseSystemPasswordChar = true;
                grpCuenta.Controls.Add(txtConfirmarPassword);
                yGrp += 56;
            }
            panelPrincipal.Controls.Add(grpCuenta);
            y += grpCuenta.Height + 12;

            // Grupo: Datos personales y rol
            GroupBox grpPersonal = new GroupBox
            {
                Text = "  Datos personales y rol  ",
                Font = UITheme.FontSubtitle,
                ForeColor = UITheme.TextPrimary,
                Location = new Point(16, y),
                Size = new Size(440, 182),
                BackColor = UITheme.PanelBackground
            };
            yGrp = 28;
            grpPersonal.Controls.Add(CrearLabel("Nombre completo:", 12, yGrp));
            txtNombreCompleto = CrearTextBox(12, yGrp + 24, 416);
            grpPersonal.Controls.Add(txtNombreCompleto);
            yGrp += 64;
            grpPersonal.Controls.Add(CrearLabel("Rol:", 12, yGrp));
            cboRol = new ComboBox
            {
                Font = UITheme.FontRegular,
                Location = new Point(12, yGrp + 24),
                Size = new Size(416, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = UITheme.InputBackground,
                ForeColor = UITheme.TextPrimary,
                FlatStyle = FlatStyle.Flat,
                IntegralHeight = true
            };
            cboRol.Items.AddRange(RolesPermisos.ObtenerRolesDisponibles().ToArray());
            cboRol.SelectedIndex = 0;
            grpPersonal.Controls.Add(cboRol);

            if (idUsuario.HasValue)
            {
                chkActivo = new CheckBox
                {
                    Text = "Usuario activo",
                    Font = UITheme.FontRegular,
                    ForeColor = UITheme.TextPrimary,
                    Location = new Point(12, yGrp + 58),
                    AutoSize = true,
                    Checked = true
                };
                grpPersonal.Controls.Add(chkActivo);
            }
            panelPrincipal.Controls.Add(grpPersonal);
            y += grpPersonal.Height + 12;

            // En edición: información extra y botón cambiar contraseña
            if (idUsuario.HasValue)
            {
                lblFechaCreacion = new Label
                {
                    Font = UITheme.FontSmall,
                    ForeColor = UITheme.TextSecondary,
                    Location = new Point(16, y),
                    AutoSize = true
                };
                lblUltimoAcceso = new Label
                {
                    Font = UITheme.FontSmall,
                    ForeColor = UITheme.TextSecondary,
                    Location = new Point(16, y + 20),
                    AutoSize = true
                };
                panelPrincipal.Controls.Add(lblFechaCreacion);
                panelPrincipal.Controls.Add(lblUltimoAcceso);
                y += 48;

                btnCambiarPassword = new Button
                {
                    Text = "Cambiar contraseña",
                    Font = UITheme.FontBold,
                    Location = new Point(16, y),
                    Size = new Size(180, 36),
                    BackColor = UITheme.InfoColor,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                btnCambiarPassword.FlatAppearance.BorderSize = 0;
                btnCambiarPassword.Click += BtnCambiarPasswordEnEditor_Click;
                panelPrincipal.Controls.Add(btnCambiarPassword);
                y += 48;
            }
            else
            {
                y += 8;
            }

            // Botones Guardar / Cancelar
            Button btnCancelar = new Button
            {
                Text = "Cancelar",
                Font = UITheme.FontBold,
                Location = new Point(248, y),
                Size = new Size(120, 40),
                BackColor = UITheme.DangerColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            panelPrincipal.Controls.Add(btnCancelar);

            Button btnGuardar = new Button
            {
                Text = "Guardar",
                Font = UITheme.FontBold,
                Location = new Point(116, y),
                Size = new Size(120, 40),
                BackColor = UITheme.SuccessColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;
            panelPrincipal.Controls.Add(btnGuardar);

            this.AcceptButton = btnGuardar;
            this.CancelButton = btnCancelar;
            this.Controls.Add(panelPrincipal);
        }

        private Label CrearLabel(string texto, int x, int y)
        {
            return new Label
            {
                Text = texto,
                Font = UITheme.FontBold,
                ForeColor = UITheme.TextPrimary,
                Location = new Point(x, y),
                AutoSize = true
            };
        }

        private TextBox CrearTextBox(int x, int y, int width)
        {
            var txt = new TextBox
            {
                Font = UITheme.FontRegular,
                Location = new Point(x, y),
                Size = new Size(width, 28)
            };
            UITheme.StyleTextBox(txt);
            return txt;
        }

        private void CargarUsuario()
        {
            var usuarios = UsuarioService.ObtenerTodosLosUsuarios();
            var usuario = usuarios.Find(u => u.IdUsuario == idUsuario.Value);

            if (usuario != null)
            {
                txtUsuario.Text = usuario.NombreUsuario;
                txtNombreCompleto.Text = usuario.NombreCompleto;
                var idx = cboRol.Items.IndexOf(usuario.Rol);
                cboRol.SelectedIndex = idx >= 0 ? idx : 0;
                if (chkActivo != null)
                    chkActivo.Checked = usuario.Activo;
                if (lblFechaCreacion != null)
                    lblFechaCreacion.Text = "Creado: " + usuario.FechaCreacion.ToString("dd/MM/yyyy HH:mm");
                if (lblUltimoAcceso != null)
                    lblUltimoAcceso.Text = "Último acceso: " + (usuario.UltimoAcceso.HasValue ? usuario.UltimoAcceso.Value.ToString("dd/MM/yyyy HH:mm") : "Nunca");
            }
        }

        private void BtnCambiarPasswordEnEditor_Click(object sender, EventArgs e)
        {
            if (!idUsuario.HasValue) return;
            bool esPropio = SesionActual.UsuarioActivo != null && SesionActual.UsuarioActivo.IdUsuario == idUsuario.Value;
            if (esPropio && !SesionActual.TienePermiso(Permiso.CambiarPropiaContraseña))
            {
                MessageBox.Show("No tiene permisos para cambiar su contraseña.", "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!esPropio && !SesionActual.TienePermiso(Permiso.CambiarContraseñas))
            {
                MessageBox.Show("No tiene permisos para cambiar contraseñas de otros usuarios.", "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string nombreDisplay = txtNombreCompleto.Text.Trim();
            if (string.IsNullOrEmpty(nombreDisplay)) nombreDisplay = txtUsuario.Text;
            var frm = new CambiarPasswordForm(idUsuario.Value, nombreDisplay);
            if (frm.ShowDialog() == DialogResult.OK)
                MessageBox.Show("Contraseña actualizada correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string nombreCompleto = txtNombreCompleto.Text.Trim();

            if (string.IsNullOrWhiteSpace(usuario))
            {
                MessageBox.Show("El nombre de usuario es obligatorio.", "Campo requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsuario.Focus();
                return;
            }
            if (usuario.Length < MinLongitudUsuario)
            {
                MessageBox.Show(string.Format("El usuario debe tener al menos {0} caracteres.", MinLongitudUsuario), "Datos inválidos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsuario.Focus();
                return;
            }
            if (usuario.Contains(" "))
            {
                MessageBox.Show("El nombre de usuario no puede contener espacios.", "Datos inválidos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsuario.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(nombreCompleto))
            {
                MessageBox.Show("El nombre completo es obligatorio.", "Campo requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombreCompleto.Focus();
                return;
            }

            if (!idUsuario.HasValue)
            {
                if (string.IsNullOrWhiteSpace(txtPassword.Text))
                {
                    MessageBox.Show("La contraseña es obligatoria.", "Campo requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return;
                }
                if (txtPassword.Text.Length < MinLongitudPassword)
                {
                    MessageBox.Show(string.Format("La contraseña debe tener al menos {0} caracteres.", MinLongitudPassword), "Datos inválidos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return;
                }
                if (txtPassword.Text != txtConfirmarPassword.Text)
                {
                    MessageBox.Show("Las contraseñas no coinciden.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtConfirmarPassword.Focus();
                    return;
                }

                try
                {
                    bool exito = UsuarioService.CrearUsuario(usuario, txtPassword.Text, nombreCompleto, cboRol.SelectedItem.ToString());
                    if (exito)
                    {
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("No se pudo crear el usuario. El nombre de usuario podría estar en uso.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }

            try
            {
                bool exito = UsuarioService.ActualizarUsuario(idUsuario.Value, nombreCompleto, cboRol.SelectedItem.ToString(), chkActivo.Checked);
                if (exito)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("No se pudo actualizar el usuario.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            UITheme.ApplyTheme(this);

            Panel panelPrincipal = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(440, 360),
                BackColor = Color.FromArgb(45, 45, 48)
            };

            Label lblTitulo = new Label
            {
                Text = string.Format("Cambiar contraseña de:\n{0}", nombreUsuario),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 20),
                AutoSize = false,
                Size = new Size(400, 60),
                TextAlign = ContentAlignment.MiddleCenter
            };
            panelPrincipal.Controls.Add(lblTitulo);

            Label lblNueva = new Label
            {
                Text = "Nueva Contraseña:",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 100),
                AutoSize = true
            };
            panelPrincipal.Controls.Add(lblNueva);

            txtNuevaPassword = new TextBox
            {
                Font = new Font("Segoe UI", 12),
                Location = new Point(20, 130),
                Size = new Size(400, 30),
                UseSystemPasswordChar = true
            };
            panelPrincipal.Controls.Add(txtNuevaPassword);

            Label lblConfirmar = new Label
            {
                Text = "Confirmar Contraseña:",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 180),
                AutoSize = true
            };
            panelPrincipal.Controls.Add(lblConfirmar);

            txtConfirmarPassword = new TextBox
            {
                Font = new Font("Segoe UI", 12),
                Location = new Point(20, 210),
                Size = new Size(400, 30),
                UseSystemPasswordChar = true
            };
            panelPrincipal.Controls.Add(txtConfirmarPassword);

            Button btnGuardar = new Button
            {
                Text = "💾 CAMBIAR",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(20, 280),
                Size = new Size(180, 45),
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
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(240, 280),
                Size = new Size(180, 45),
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