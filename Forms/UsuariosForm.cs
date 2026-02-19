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
        private RoundedButton btnDesactivarReactivar;

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
            UITheme.ApplyTheme(this);

            // HEADER BAR
            var header = UITheme.CrearHeaderBar("Administración de Personal", "Gestión de usuarios del sistema");

            // Panel de botones superiores (Nuevo Usuario)
            Panel panelTop = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = UITheme.DarkBackground };

            RoundedButton btnNuevo = new RoundedButton
            {
                Text = "➕ NUEVO USUARIO",
                Location = new Point(820, 10),
                Size = new Size(160, 40),
                BackColor = UITheme.PrimaryColor,
                ForeColor = Color.White
            };
            btnNuevo.Click += BtnNuevoUsuario_Click;
            panelTop.Controls.Add(btnNuevo);

            // DataGridView de usuarios
            dgvUsuarios = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = UITheme.DarkBackground,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };
            UITheme.StyleDataGridView(dgvUsuarios);
            ConfigurarColumnasGrid();

            // Panel inferior con botones de acción
            Panel panelBottom = new Panel { Dock = DockStyle.Bottom, Height = 70, BackColor = UITheme.PanelBackground };

            RoundedButton btnEditar = new RoundedButton
            {
                Text = "✏ EDITAR",
                Location = new Point(20, 15),
                Size = new Size(140, 40),
                BackColor = UITheme.WarningColor
            };
            btnEditar.Click += BtnEditar_Click;
            panelBottom.Controls.Add(btnEditar);

            RoundedButton btnPassword = new RoundedButton
            {
                Text = "🔑 CAMBIAR CONTRASEÑA",
                Location = new Point(170, 15),
                Size = new Size(220, 40),
                BackColor = UITheme.InfoColor
            };
            btnPassword.Click += BtnCambiarPassword_Click;
            panelBottom.Controls.Add(btnPassword);

            btnDesactivarReactivar = new RoundedButton
            {
                Text = "DESACTIVAR",
                Location = new Point(400, 15),
                Size = new Size(160, 40),
                BackColor = UITheme.ErrorColor
            };
            btnDesactivarReactivar.Click += BtnDesactivarReactivar_Click;
            panelBottom.Controls.Add(btnDesactivarReactivar);

            RoundedButton btnActualizar = new RoundedButton
            {
                Text = "↻ ACTUALIZAR",
                Location = new Point(820, 15),
                Size = new Size(160, 40),
                BackColor = UITheme.AccentColor
            };
            btnActualizar.Click += (s, e) => CargarUsuarios();
            panelBottom.Controls.Add(btnActualizar);

            // Orden correcto para WinForms Docking: Fill primero, luego Bottom, luego Top
            this.Controls.Add(dgvUsuarios);
            this.Controls.Add(panelBottom);
            this.Controls.Add(panelTop);
            this.Controls.Add(header);
            dgvUsuarios.SelectionChanged += (s, e) => ActualizarBotonDesactivarReactivar();
        }

        private void ConfigurarColumnasGrid()
        {
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
                Color colorEstado = u.Activo ? UITheme.SuccessColor : UITheme.ErrorColor;

                bool esSupervisor = u.Rol == RolesPermisos.Supervisor;
                bool esUsuarioActual = SesionActual.UsuarioActivo != null && u.IdUsuario == SesionActual.UsuarioActivo.IdUsuario;

                // Solo se modifica la columna visual "Usuario" (nunca se escribe de vuelta a la DB)
                string prefijo = esSupervisor ? "👑 " : "";
                string sufijo = esUsuarioActual ? "  (activo)" : "";
                string usuarioDisplay = prefijo + u.NombreUsuario + sufijo;

                int index = dgvUsuarios.Rows.Add(
                    u.IdUsuario,
                    usuarioDisplay,
                    u.NombreCompleto,
                    u.Rol,
                    u.FechaCreacion.ToString("dd/MM/yyyy"),
                    ultimoAcceso,
                    estado
                );

                dgvUsuarios.Rows[index].Cells[6].Style.ForeColor = colorEstado;
                dgvUsuarios.Rows[index].Cells[6].Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);

                // Filas de Supervisor: fondo dorado oscuro + texto dorado
                if (esSupervisor)
                {
                    dgvUsuarios.Rows[index].DefaultCellStyle.BackColor = Color.FromArgb(50, 42, 12);
                    dgvUsuarios.Rows[index].DefaultCellStyle.ForeColor = UITheme.WarningColor;
                    dgvUsuarios.Rows[index].DefaultCellStyle.SelectionBackColor = Color.FromArgb(100, 80, 20);
                    dgvUsuarios.Rows[index].DefaultCellStyle.SelectionForeColor = UITheme.WarningColor;
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
                btnDesactivarReactivar.Text = "DESACTIVAR";
                btnDesactivarReactivar.BackColor = UITheme.ErrorColor;
                btnDesactivarReactivar.Enabled = false;
                return;
            }
            object valEstado = dgvUsuarios.SelectedRows[0].Cells[6].Value;
            bool activo = valEstado != null && valEstado.ToString() == "Activo";
            int idUsuario = Convert.ToInt32(dgvUsuarios.SelectedRows[0].Cells[0].Value);
            bool esUsuarioActual = SesionActual.UsuarioActivo != null && idUsuario == SesionActual.UsuarioActivo.IdUsuario;

            if (activo)
            {
                btnDesactivarReactivar.Text = "DESACTIVAR";
                btnDesactivarReactivar.BackColor = UITheme.ErrorColor;
                btnDesactivarReactivar.Enabled = !esUsuarioActual;
            }
            else
            {
                btnDesactivarReactivar.Text = "REACTIVAR";
                btnDesactivarReactivar.BackColor = UITheme.SuccessColor;
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
            this.Size = new Size(540, idUsuario.HasValue ? 650 : 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            UITheme.ApplyTheme(this);

            // HEADER BAR
            var header = UITheme.CrearHeaderBar(
                idUsuario.HasValue ? "Editar Usuario" : "Nuevo Usuario",
                idUsuario.HasValue ? "Modificar información del usuario" : "Crear nuevo usuario del sistema"
            );
            this.Controls.Add(header);

            // Panel con scroll
            Panel panelContenido = new Panel
            {
                Location = new Point(20, 90),
                Size = new Size(480, idUsuario.HasValue ? 480 : 420),
                BackColor = UITheme.DarkBackground,
                AutoScroll = true
            };

            int y = 10;

            // SECCIÓN: DATOS DE CUENTA
            Label lblSeccionCuenta = new Label
            {
                Text = "— DATOS DE CUENTA",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UITheme.AccentColor,
                Location = new Point(0, y),
                AutoSize = true
            };
            panelContenido.Controls.Add(lblSeccionCuenta);
            y += 30;

            panelContenido.Controls.Add(CrearLabel("Usuario (inicio de sesión):", 0, y));
            txtUsuario = CrearTextBox(0, y + 22, 460);
            txtUsuario.Enabled = !idUsuario.HasValue;
            txtUsuario.MaxLength = MaxLongitudUsuario;
            panelContenido.Controls.Add(txtUsuario);
            y += 60;

            if (!idUsuario.HasValue)
            {
                panelContenido.Controls.Add(CrearLabel(string.Format("Contraseña (mínimo {0} caracteres):", MinLongitudPassword), 0, y));
                txtPassword = CrearTextBox(0, y + 22, 460);
                txtPassword.UseSystemPasswordChar = true;
                panelContenido.Controls.Add(txtPassword);
                y += 60;

                panelContenido.Controls.Add(CrearLabel("Confirmar contraseña:", 0, y));
                txtConfirmarPassword = CrearTextBox(0, y + 22, 460);
                txtConfirmarPassword.UseSystemPasswordChar = true;
                panelContenido.Controls.Add(txtConfirmarPassword);
                y += 60;
            }

            y += 10;

            // SECCIÓN: DATOS PERSONALES
            Label lblSeccionPersonal = new Label
            {
                Text = "— DATOS PERSONALES Y ROL",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UITheme.AccentColor,
                Location = new Point(0, y),
                AutoSize = true
            };
            panelContenido.Controls.Add(lblSeccionPersonal);
            y += 30;

            panelContenido.Controls.Add(CrearLabel("Nombre completo:", 0, y));
            txtNombreCompleto = CrearTextBox(0, y + 22, 460);
            panelContenido.Controls.Add(txtNombreCompleto);
            y += 60;

            panelContenido.Controls.Add(CrearLabel("Rol:", 0, y));
            cboRol = new ComboBox
            {
                Font = UITheme.FontRegular,
                Location = new Point(0, y + 22),
                Size = new Size(460, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            UITheme.StyleComboBox(cboRol);
            cboRol.Items.AddRange(RolesPermisos.ObtenerRolesDisponibles().ToArray());
            cboRol.SelectedIndex = 0;
            panelContenido.Controls.Add(cboRol);
            y += 60;

            if (idUsuario.HasValue)
            {
                chkActivo = new CheckBox
                {
                    Text = "Usuario activo",
                    Font = UITheme.FontRegular,
                    ForeColor = UITheme.TextPrimary,
                    Location = new Point(0, y),
                    AutoSize = true,
                    Checked = true
                };
                panelContenido.Controls.Add(chkActivo);
                y += 40;

                lblFechaCreacion = new Label
                {
                    Font = UITheme.FontSmall,
                    ForeColor = UITheme.TextMuted,
                    Location = new Point(0, y),
                    AutoSize = true
                };
                panelContenido.Controls.Add(lblFechaCreacion);
                y += 22;

                lblUltimoAcceso = new Label
                {
                    Font = UITheme.FontSmall,
                    ForeColor = UITheme.TextMuted,
                    Location = new Point(0, y),
                    AutoSize = true
                };
                panelContenido.Controls.Add(lblUltimoAcceso);
                y += 35;

                RoundedButton btnCambiarPass = new RoundedButton
                {
                    Text = "🔑 CAMBIAR CONTRASEÑA",
                    Location = new Point(0, y),
                    Size = new Size(220, 36),
                    BackColor = UITheme.InfoColor
                };
                btnCambiarPass.Click += BtnCambiarPasswordEnEditor_Click;
                panelContenido.Controls.Add(btnCambiarPass);
            }

            this.Controls.Add(panelContenido);

            // Botones Guardar / Cancelar
            RoundedButton btnGuardar = new RoundedButton
            {
                Text = "💾 GUARDAR",
                Location = new Point(250, this.ClientSize.Height - 60),
                Size = new Size(120, 40),
                BackColor = UITheme.SuccessColor
            };
            btnGuardar.Click += BtnGuardar_Click;
            this.Controls.Add(btnGuardar);

            RoundedButton btnCancelar = new RoundedButton
            {
                Text = "CANCELAR",
                Location = new Point(380, this.ClientSize.Height - 60),
                Size = new Size(120, 40),
                BackColor = UITheme.ErrorColor
            };
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancelar);

            // AcceptButton y CancelButton requieren IButtonControl, no compatible con RoundedButton
            // this.AcceptButton = btnGuardar;
            // this.CancelButton = btnCancelar;
        }

        private Label CrearLabel(string texto, int x, int y)
        {
            return new Label
            {
                Text = texto,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UITheme.TextSecondary,
                Location = new Point(x, y),
                AutoSize = true
            };
        }

        private TextBox CrearTextBox(int x, int y, int width)
        {
            var txt = new TextBox
            {
                Font = new Font("Segoe UI", 11),
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
            this.Size = new Size(500, 380);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            UITheme.ApplyTheme(this);

            // HEADER BAR
            var header = UITheme.CrearHeaderBar("Cambiar Contraseña", nombreUsuario);
            this.Controls.Add(header);

            RoundedPanel panel = new RoundedPanel
            {
                Location = new Point(30, 100),
                Size = new Size(420, 200),
                BackColor = UITheme.PanelBackground,
                Radius = 12
            };

            int y = 20;

            Label lblNueva = new Label
            {
                Text = "NUEVA CONTRASEÑA",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UITheme.TextSecondary,
                Location = new Point(20, y),
                AutoSize = true
            };
            panel.Controls.Add(lblNueva);

            txtNuevaPassword = new TextBox
            {
                Font = new Font("Segoe UI", 11),
                Location = new Point(20, y + 22),
                Size = new Size(380, 28),
                UseSystemPasswordChar = true
            };
            UITheme.StyleTextBox(txtNuevaPassword);
            panel.Controls.Add(txtNuevaPassword);
            y += 70;

            Label lblConfirmar = new Label
            {
                Text = "CONFIRMAR CONTRASEÑA",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UITheme.TextSecondary,
                Location = new Point(20, y),
                AutoSize = true
            };
            panel.Controls.Add(lblConfirmar);

            txtConfirmarPassword = new TextBox
            {
                Font = new Font("Segoe UI", 11),
                Location = new Point(20, y + 22),
                Size = new Size(380, 28),
                UseSystemPasswordChar = true
            };
            UITheme.StyleTextBox(txtConfirmarPassword);
            panel.Controls.Add(txtConfirmarPassword);

            this.Controls.Add(panel);

            // Botones
            RoundedButton btnGuardar = new RoundedButton
            {
                Text = "💾 CAMBIAR",
                Location = new Point(200, 320),
                Size = new Size(140, 40),
                BackColor = UITheme.SuccessColor
            };
            btnGuardar.Click += BtnGuardar_Click;
            this.Controls.Add(btnGuardar);

            RoundedButton btnCancelar = new RoundedButton
            {
                Text = "CANCELAR",
                Location = new Point(350, 320),
                Size = new Size(120, 40),
                BackColor = UITheme.ErrorColor
            };
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(btnCancelar);
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
