using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using SistemaPOS.Models;
using SistemaPOS.Services;
using SistemaPOS.Helpers;

namespace SistemaPOS.Forms
{
    public partial class VendedorMainForm : Form
    {
        private DataGridView dgvPedidos;
        private Timer tmrMonitor;
        private Label lblTotalPedidos;
        private Label lblPendientes;
        private Label lblTotalVendido;
        private FlowLayoutPanel pnlAlertas;
        private Label lblAlertaTitulo;
        private Label lblReloj;

        public VendedorMainForm()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            this.Text = "Sistema POS - Panel Vendedor";
            IniciarMonitor();
        }

        private void InitializeComponent()
        {
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
            menuItems.Add(new SidebarItem("\U0001F4CB", "Mis Pedidos", () => { }, true));
            menuItems.Add(new SidebarItem("\U0001F4B3", "Terminal de Venta", () => AbrirForm(new TerminalVentaForm())));
            menuItems.Add(new SidebarItem("\U0001F4E6", "Consultar Stock", () => AbrirForm(new InventarioForm())));

            for (int i = menuItems.Count - 1; i >= 0; i--)
                sidebarItems.Controls.Add(menuItems[i]);

            sidebar.Controls.Add(sidebarItems);

            // ==========================================
            // HEADER BAR
            // ==========================================
            string nombreUsuario = SesionActual.UsuarioActivo != null ? SesionActual.UsuarioActivo.NombreCompleto : "";
            Panel header = UITheme.CrearHeaderBar("Panel Vendedor", string.Format("Bienvenido, {0}", nombreUsuario));

            lblReloj = new Label
            {
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = UITheme.TextSecondary,
                AutoSize = true,
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

            // KPIs
            FlowLayoutPanel pnlKPIs = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 100,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 0, 0, 10)
            };

            pnlKPIs.Controls.Add(UITheme.CrearTarjetaKPI("Pedidos Hoy", "\U0001F4CB", UITheme.PrimaryColor, ref lblTotalPedidos));
            pnlKPIs.Controls.Add(UITheme.CrearTarjetaKPI("Pendientes", "\u23F3", UITheme.WarningColor, ref lblPendientes));
            pnlKPIs.Controls.Add(UITheme.CrearTarjetaKPI("Total Vendido", "\U0001F4B0", UITheme.SuccessColor, ref lblTotalVendido));

            // Grid de pedidos
            Panel pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UITheme.SurfaceColor,
                Padding = new Padding(15)
            };

            Label lblTituloCola = new Label
            {
                Text = "MIS PEDIDOS DE HOY",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = UITheme.TextPrimary,
                Dock = DockStyle.Top,
                Height = 35
            };

            dgvPedidos = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                Font = new Font("Segoe UI", 11)
            };
            UITheme.StyleDataGridView(dgvPedidos);

            dgvPedidos.Columns.Add("Id", "Pedido #");
            dgvPedidos.Columns.Add("Total", "Monto Total");
            dgvPedidos.Columns.Add("Estado", "Estado");
            dgvPedidos.Columns.Add("Hora", "Hora");
            dgvPedidos.Columns[0].FillWeight = 20;
            dgvPedidos.Columns[1].FillWeight = 30;
            dgvPedidos.Columns[2].FillWeight = 25;
            dgvPedidos.Columns[3].FillWeight = 25;

            dgvPedidos.CellFormatting += DgvPedidos_CellFormatting;

            pnlGrid.Controls.Add(dgvPedidos);
            pnlGrid.Controls.Add(lblTituloCola);

            // Alertas de stock
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

            // Ensamblaje
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
            int idVendedor = SesionActual.UsuarioActivo.IdUsuario;

            var pedidos = PedidoService.ObtenerPedidosPorVendedor(idVendedor);
            dgvPedidos.Rows.Clear();
            foreach (var p in pedidos)
            {
                dgvPedidos.Rows.Add(
                    p.IdPedido,
                    p.Total.ToString("C"),
                    p.Estado,
                    p.Fecha.ToString("HH:mm:ss")
                );
            }

            int totalPedidos, pendientes;
            decimal totalVendido;
            PedidoService.ObtenerEstadisticasVendedor(idVendedor, out totalPedidos, out pendientes, out totalVendido);
            lblTotalPedidos.Text = totalPedidos.ToString();
            lblPendientes.Text = pendientes.ToString();
            lblTotalVendido.Text = totalVendido.ToString("C");

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

        private void DgvPedidos_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == 2 && e.Value != null)
            {
                string estado = e.Value.ToString();
                switch (estado)
                {
                    case "PENDIENTE":
                        e.CellStyle.ForeColor = UITheme.WarningColor;
                        break;
                    case "PAGADO":
                        e.CellStyle.ForeColor = UITheme.SuccessColor;
                        break;
                    case "CANCELADO":
                        e.CellStyle.ForeColor = UITheme.DangerColor;
                        break;
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
