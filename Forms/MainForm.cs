using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using SistemaPOS.Models;
using SistemaPOS.Data;
using SistemaPOS.Services;
using SistemaPOS.Helpers;

namespace SistemaPOS.Forms
{
    public partial class MainForm : Form
    {
        private DataGridView dgvCola;
        private Timer tmrMonitor;
        private Label lblTotalUsuarios;
        private Label lblUsuariosActivos;
        private Label lblNuevosMes;
        private FlowLayoutPanel pnlAlertas;
        private Label lblAlertaTitulo;
        private Label lblReloj;

        public MainForm()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            this.Text = "Sistema POS - Control de Caja";
            IniciarMonitor();
        }

        private void InitializeComponent()
        {
            this.AutoScaleMode = AutoScaleMode.None;
            this.Size = new Size(1200, 700);
            this.BackColor = UITheme.DarkBackground;

            // ==========================================
            // SIDEBAR IZQUIERDA
            // ==========================================
            Panel sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = UITheme.SidebarBackground
            };

            // Logo en sidebar
            Panel logoPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.Transparent
            };

            Label lblLogo = new Label
            {
                Text = "\U0001F6D2  POS",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = UITheme.AccentColor,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            logoPanel.Controls.Add(lblLogo);

            Panel logoSep = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = UITheme.BorderColor
            };
            logoPanel.Controls.Add(logoSep);

            sidebar.Controls.Add(logoPanel);

            // Sidebar items - se agregan en orden inverso porque Dock=Top los apila
            Panel sidebarItems = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 10, 0, 0)
            };

            // Botón cerrar sesión al fondo
            Panel logoutPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 58,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 10, 0, 0)
            };
            SidebarItem btnLogout = new SidebarItem("\U0001F6AA", "Cerrar Sesión", CerrarSesion);
            logoutPanel.Controls.Add(btnLogout);
            sidebar.Controls.Add(logoutPanel);

            // Items del menú
            var menuItems = new List<SidebarItem>();
            menuItems.Add(new SidebarItem("\U0001F4CB", "Cola de Cobro", () => { }, true));

            if (SesionActual.TienePermiso(Permiso.EditarProductos))
                menuItems.Add(new SidebarItem("\U0001F4E6", "Productos", () => AbrirForm(new InventarioForm())));

            if (SesionActual.TienePermiso(Permiso.VerUsuarios))
                menuItems.Add(new SidebarItem("\U0001F465", "Usuarios", () => AbrirForm(new UsuariosForm())));

            if (SesionActual.TienePermiso(Permiso.CrearPedidos))
                menuItems.Add(new SidebarItem("\U0001F4B3", "Terminal Venta", () => AbrirForm(new TerminalVentaForm())));

            if (SesionActual.TienePermiso(Permiso.VerReportes))
                menuItems.Add(new SidebarItem("\U0001F4CA", "Reportes", () => AbrirForm(new ReportesForm())));

            if (SesionActual.TienePermiso(Permiso.ConfigurarSistema))
                menuItems.Add(new SidebarItem("\u2699", "Configuración", () => AbrirForm(new ConfiguracionForm())));

            // Agregar items en orden inverso para Dock.Top
            for (int i = menuItems.Count - 1; i >= 0; i--)
                sidebarItems.Controls.Add(menuItems[i]);

            sidebar.Controls.Add(sidebarItems);

            // ==========================================
            // HEADER BAR
            // ==========================================
            string rolTexto = SesionActual.UsuarioActivo != null ? SesionActual.UsuarioActivo.Rol : "";
            string nombreUsuario = SesionActual.UsuarioActivo != null ? SesionActual.UsuarioActivo.NombreCompleto : "";
            Panel header = UITheme.CrearHeaderBar("Cola de Cobro", string.Format("Bienvenido, {0} ({1})", nombreUsuario, rolTexto));

            // Reloj en el header
            lblReloj = new Label
            {
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = UITheme.TextSecondary,
                AutoSize = true,
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Location = new Point(900, 18)
            };
            header.Controls.Add(lblReloj);
            header.Resize += (s, e) =>
            {
                lblReloj.Location = new Point(header.Width - lblReloj.Width - 30, 18);
            };

            // ==========================================
            // CONTENIDO PRINCIPAL
            // ==========================================
            Panel contenido = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UITheme.DarkBackground,
                Padding = new Padding(20)
            };

            // KPIs horizontales en la parte superior
            FlowLayoutPanel pnlKPIs = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 100,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 0, 0, 10)
            };

            pnlKPIs.Controls.Add(UITheme.CrearTarjetaKPI("Clientes Registrados", "\U0001F465", UITheme.PrimaryColor, ref lblTotalUsuarios));
            pnlKPIs.Controls.Add(UITheme.CrearTarjetaKPI("Activos", "\u2705", UITheme.SuccessColor, ref lblUsuariosActivos));
            pnlKPIs.Controls.Add(UITheme.CrearTarjetaKPI("Nuevos del Mes", "\U0001F195", UITheme.AccentColor, ref lblNuevosMes));

            // Grid de cola
            Panel pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UITheme.SurfaceColor,
                Padding = new Padding(15)
            };

            Label lblTituloCola = new Label
            {
                Text = "PEDIDOS PENDIENTES DE COBRO",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = UITheme.TextPrimary,
                Dock = DockStyle.Top,
                Height = 35
            };

            Label lblInstruccion = new Label
            {
                Text = "Doble clic en un pedido para procesar el cobro",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.TextMuted,
                Dock = DockStyle.Top,
                Height = 20
            };

            dgvCola = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                Font = new Font("Segoe UI", 11)
            };
            UITheme.StyleDataGridView(dgvCola);

            dgvCola.Columns.Add("Id", "Pedido #");
            dgvCola.Columns.Add("Vendedor", "Vendedor");
            dgvCola.Columns.Add("Total", "Monto Total");
            dgvCola.Columns.Add("Tiempo", "Tiempo en espera");
            dgvCola.Columns[0].FillWeight = 15;
            dgvCola.Columns[1].FillWeight = 35;
            dgvCola.Columns[2].FillWeight = 25;
            dgvCola.Columns[3].FillWeight = 25;

            dgvCola.CellDoubleClick += DgvCola_CellDoubleClick;

            pnlGrid.Controls.Add(dgvCola);
            pnlGrid.Controls.Add(lblInstruccion);
            pnlGrid.Controls.Add(lblTituloCola);

            // Panel de alertas de stock
            Panel pnlAlertaContenedor = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 130,
                BackColor = UITheme.SurfaceColor,
                Padding = new Padding(15, 8, 15, 8)
            };

            Panel alertaSep = new Panel
            {
                Dock = DockStyle.Top,
                Height = 1,
                BackColor = UITheme.BorderColor
            };

            lblAlertaTitulo = new Label
            {
                Text = "ALERTAS DE STOCK",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = UITheme.WarningColor,
                Dock = DockStyle.Top,
                Height = 28
            };

            pnlAlertas = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.Transparent
            };

            pnlAlertaContenedor.Controls.Add(pnlAlertas);
            pnlAlertaContenedor.Controls.Add(lblAlertaTitulo);
            pnlAlertaContenedor.Controls.Add(alertaSep);

            contenido.Controls.Add(pnlGrid);
            contenido.Controls.Add(pnlAlertaContenedor);
            contenido.Controls.Add(pnlKPIs);

            // ==========================================
            // ENSAMBLAJE FINAL
            // ==========================================
            this.Controls.Add(contenido);
            this.Controls.Add(header);
            this.Controls.Add(sidebar);
        }

        private void IniciarMonitor()
        {
            tmrMonitor = new Timer { Interval = 5000 };
            tmrMonitor.Tick += (s, e) =>
            {
                ActualizarDatos();
                if (lblReloj != null)
                    lblReloj.Text = DateTime.Now.ToString("HH:mm:ss");
            };
            tmrMonitor.Start();
            ActualizarDatos();
            if (lblReloj != null)
                lblReloj.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void ActualizarDatos()
        {
            var pendientes = PedidoService.ObtenerPendientes();
            dgvCola.Rows.Clear();
            foreach (var p in pendientes)
            {
                TimeSpan espera = DateTime.Now - p.Fecha;
                string tiempoTexto = string.Format("{0} min {1} seg", (int)espera.TotalMinutes, espera.Seconds);

                int idx = dgvCola.Rows.Add(p.IdPedido, p.NombreVendedor, p.Total.ToString("C"), tiempoTexto);

                // Colorear según tiempo de espera
                if (espera.TotalMinutes > 10)
                    dgvCola.Rows[idx].DefaultCellStyle.ForeColor = UITheme.DangerColor;
                else if (espera.TotalMinutes > 5)
                    dgvCola.Rows[idx].DefaultCellStyle.ForeColor = UITheme.WarningColor;
            }

            var usuarios = UsuarioService.ObtenerTodosLosUsuarios();
            lblTotalUsuarios.Text = usuarios.Count.ToString();
            lblUsuariosActivos.Text = usuarios.Count(u => u.Activo).ToString();
            lblNuevosMes.Text = usuarios.Count(u => u.FechaCreacion.Month == DateTime.Now.Month && u.FechaCreacion.Year == DateTime.Now.Year).ToString();

            ActualizarAlertas();
        }

        private void ActualizarAlertas()
        {
            var productosBajoStock = ProductoService.ObtenerConBajoStock();
            pnlAlertas.Controls.Clear();

            if (productosBajoStock.Count == 0)
            {
                lblAlertaTitulo.Text = "SIN ALERTAS DE STOCK";
                lblAlertaTitulo.ForeColor = UITheme.SuccessColor;
                pnlAlertas.Controls.Add(new Label
                {
                    Text = "\u2705  Todos los productos tienen stock suficiente",
                    Font = new Font("Segoe UI", 10),
                    ForeColor = UITheme.TextSecondary,
                    AutoSize = true,
                    Margin = new Padding(5)
                });
                return;
            }

            lblAlertaTitulo.Text = string.Format("ALERTAS DE STOCK ({0})", productosBajoStock.Count);
            lblAlertaTitulo.ForeColor = UITheme.WarningColor;

            foreach (var p in productosBajoStock)
            {
                bool sinStock = p.SinStock();
                Color colorBorde = sinStock ? UITheme.DangerColor : UITheme.WarningColor;
                string icono = sinStock ? "AGOTADO" : "BAJO";

                Panel card = new Panel
                {
                    Size = new Size(200, 80),
                    BackColor = Color.FromArgb(40, 50, 65),
                    Margin = new Padding(5),
                    Padding = new Padding(8)
                };

                Panel barra = new Panel
                {
                    Dock = DockStyle.Left,
                    Width = 4,
                    BackColor = colorBorde
                };

                Label lblEstado = new Label
                {
                    Text = icono,
                    Font = new Font("Segoe UI", 7, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = colorBorde,
                    AutoSize = false,
                    Size = new Size(60, 16),
                    Location = new Point(14, 8),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                Label lblNombre = new Label
                {
                    Text = p.Nombre,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize = false,
                    Size = new Size(170, 20),
                    Location = new Point(14, 28)
                };

                Label lblDetalle = new Label
                {
                    Text = string.Format("Stock: {0}  |  Min: {1}", p.StockActual, p.StockMinimo),
                    Font = new Font("Segoe UI", 8),
                    ForeColor = UITheme.TextSecondary,
                    AutoSize = false,
                    Size = new Size(170, 18),
                    Location = new Point(14, 50)
                };

                card.Controls.Add(lblEstado);
                card.Controls.Add(lblNombre);
                card.Controls.Add(lblDetalle);
                card.Controls.Add(barra);

                pnlAlertas.Controls.Add(card);
            }
        }

        private void DgvCola_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int idPedido = Convert.ToInt32(dgvCola.Rows[e.RowIndex].Cells[0].Value);
                ProcesarCobro(idPedido);
            }
        }

        private void ProcesarCobro(int idPedido)
        {
            var pedido = PedidoService.ObtenerPedidoPorId(idPedido);
            if (pedido != null)
            {
                var items = new List<ItemVenta>();
                foreach (var det in pedido.Detalles)
                {
                    items.Add(new ItemVenta(det.Producto, det.Cantidad));
                }

                using (var form = new CobrarForm(items, 0.19m, idPedido))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        ActualizarDatos();
                    }
                }
            }
        }

        private void AbrirForm(Form form)
        {
            form.StartPosition = FormStartPosition.CenterScreen;
            form.ShowDialog();
            ActualizarDatos();
        }

        private void CerrarSesion()
        {
            if (MessageBox.Show("¿Cerrar sesión?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                SesionActual.CerrarSesion();
                if (tmrMonitor != null) tmrMonitor.Stop();
                this.Close();
            }
        }
    }
}
