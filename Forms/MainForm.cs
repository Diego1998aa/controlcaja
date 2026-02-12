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

        public MainForm()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            this.Text = "Sistema POS - Control de Caja";
            IniciarMonitor();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1200, 700);
            this.BackColor = UITheme.DarkBackground;

            // Layout Principal
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(10)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70)); // Izquierda: Cola
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30)); // Derecha: KPIs

            // --- SECCIÓN IZQUIERDA: COLA DE COBRO ---
            Panel pnlCola = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(45, 45, 48), // Oscuro
                Padding = new Padding(10)
            };

            Label lblTituloCola = new Label
            {
                Text = "COLA DE COBRO (PENDIENTES)",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 40
            };

            dgvCola = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                Font = new Font("Segoe UI", 12)
            };

            // Aplicar estilo del tema
            UITheme.StyleDataGridView(dgvCola);

            dgvCola.Columns.Add("Id", "Pedido #");
            dgvCola.Columns.Add("Vendedor", "Vendedor");
            dgvCola.Columns.Add("Total", "Monto Total");
            dgvCola.Columns.Add("Tiempo", "Tiempo en espera");

            dgvCola.CellDoubleClick += DgvCola_CellDoubleClick;

            // --- PANEL DE ALERTAS DE STOCK (parte inferior izquierda) ---
            Panel pnlAlertaContenedor = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 140,
                BackColor = Color.FromArgb(35, 35, 38),
                Padding = new Padding(10, 5, 10, 5)
            };

            lblAlertaTitulo = new Label
            {
                Text = "ALERTAS DE STOCK",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = UITheme.WarningColor,
                Dock = DockStyle.Top,
                Height = 25
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

            pnlCola.Controls.Add(dgvCola);
            pnlCola.Controls.Add(pnlAlertaContenedor);
            pnlCola.Controls.Add(lblTituloCola);

            // --- SECCIÓN DERECHA: KPIs y BOTONES ---
            FlowLayoutPanel pnlDerecha = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.Transparent
            };

            // KPIs
            pnlDerecha.Controls.Add(CrearTarjetaKPI("Clientes Registrados", ref lblTotalUsuarios));
            pnlDerecha.Controls.Add(CrearTarjetaKPI("Activos", ref lblUsuariosActivos));
            pnlDerecha.Controls.Add(CrearTarjetaKPI("Nuevos del mes", ref lblNuevosMes));

            // Espaciador
            pnlDerecha.Controls.Add(new Panel { Height = 40, Width = 10 });

            // Botones Rápidos - SOLO PARA ROLES CON PERMISOS
            if (SesionActual.TienePermiso(Permiso.EditarProductos))
            {
                pnlDerecha.Controls.Add(CrearBotonAcceso("Gestión de Productos", () => AbrirForm(new InventarioForm())));
            }

            if (SesionActual.TienePermiso(Permiso.VerUsuarios))
            {
                pnlDerecha.Controls.Add(CrearBotonAcceso("Usuarios", () => AbrirForm(new UsuariosForm())));
            }

            if (SesionActual.TienePermiso(Permiso.CrearPedidos))
            {
                pnlDerecha.Controls.Add(CrearBotonAcceso("Terminal Venta", () => AbrirForm(new TerminalVentaForm())));
            }

            pnlDerecha.Controls.Add(CrearBotonAcceso("Cerrar Sesión", CerrarSesion));

            mainLayout.Controls.Add(pnlCola, 0, 0);
            mainLayout.Controls.Add(pnlDerecha, 1, 0);

            this.Controls.Add(mainLayout);
        }

        private Panel CrearTarjetaKPI(string titulo, ref Label lblValor)
        {
            Panel card = new Panel
            {
                Size = new Size(300, 100),
                BackColor = Color.White,
                Margin = new Padding(0, 0, 0, 15),
                Padding = new Padding(10)
            };

            Label lblTitle = new Label
            {
                Text = titulo.ToUpper(),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Gray,
                Dock = DockStyle.Top
            };

            lblValor = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.Black,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight
            };

            card.Controls.Add(lblValor);
            card.Controls.Add(lblTitle);
            return card;
        }

        private Button CrearBotonAcceso(string texto, Action accion)
        {
            Button btn = new Button
            {
                Text = texto,
                Size = new Size(300, 60),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 10),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => accion();
            return btn;
        }

        private void IniciarMonitor()
        {
            tmrMonitor = new Timer { Interval = 5000 }; // 5 segundos
            tmrMonitor.Tick += (s, e) => ActualizarDatos();
            tmrMonitor.Start();
            ActualizarDatos(); // Primera carga
        }

        private void ActualizarDatos()
        {
            // 1. Actualizar Cola
            var pendientes = PedidoService.ObtenerPendientes();
            dgvCola.Rows.Clear();
            foreach (var p in pendientes)
            {
                TimeSpan espera = DateTime.Now - p.Fecha;
                string tiempoTexto = string.Format("{0} min {1} seg", (int)espera.TotalMinutes, espera.Seconds);
                
                dgvCola.Rows.Add(p.IdPedido, p.NombreVendedor, p.Total.ToString("C"), tiempoTexto);
            }

            // 2. Actualizar KPIs (Usando Usuarios como proxy de Clientes según requerimiento)
            var usuarios = UsuarioService.ObtenerTodosLosUsuarios();
            lblTotalUsuarios.Text = usuarios.Count.ToString();
            lblUsuariosActivos.Text = usuarios.Count(u => u.Activo).ToString();
            lblNuevosMes.Text = usuarios.Count(u => u.FechaCreacion.Month == DateTime.Now.Month && u.FechaCreacion.Year == DateTime.Now.Year).ToString();

            // 3. Actualizar Alertas de Stock
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
                    Text = "Todos los productos tienen stock suficiente",
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
                Color colorFondo = sinStock ? Color.FromArgb(80, 220, 53, 69) : Color.FromArgb(80, 255, 193, 7);
                Color colorBorde = sinStock ? UITheme.DangerColor : UITheme.WarningColor;
                string icono = sinStock ? "AGOTADO" : "BAJO";

                Panel card = new Panel
                {
                    Size = new Size(220, 90),
                    BackColor = Color.FromArgb(50, 50, 55),
                    Margin = new Padding(5),
                    Padding = new Padding(8)
                };

                // Barra lateral de color indicador
                Panel barra = new Panel
                {
                    Dock = DockStyle.Left,
                    Width = 4,
                    BackColor = colorBorde
                };

                Label lblEstado = new Label
                {
                    Text = icono,
                    Font = new Font("Segoe UI", 8, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = colorBorde,
                    AutoSize = false,
                    Size = new Size(65, 18),
                    Location = new Point(18, 8),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                Label lblNombre = new Label
                {
                    Text = p.Nombre,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize = false,
                    Size = new Size(190, 22),
                    Location = new Point(18, 30)
                };

                Label lblDetalle = new Label
                {
                    Text = string.Format("Stock: {0}  |  Min: {1}", p.StockActual, p.StockMinimo),
                    Font = new Font("Segoe UI", 9),
                    ForeColor = UITheme.TextSecondary,
                    AutoSize = false,
                    Size = new Size(190, 20),
                    Location = new Point(18, 55)
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
                // Convertir DetallePedido a ItemVenta para el formulario de cobro
                var items = new List<ItemVenta>();
                foreach (var det in pedido.Detalles)
                {
                    items.Add(new ItemVenta(det.Producto, det.Cantidad));
                }

                // Abrir CobrarForm pasando el ID del pedido para que se actualice a PAGADO
                using (var form = new CobrarForm(items, 0.19m, idPedido))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        ActualizarDatos(); // Refrescar cola inmediatamente
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
            if (MessageBox.Show("¿Cerrar sesión?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                SesionActual.CerrarSesion();
                if (tmrMonitor != null) tmrMonitor.Stop();
                this.Close();
            }
        }
    }
}
