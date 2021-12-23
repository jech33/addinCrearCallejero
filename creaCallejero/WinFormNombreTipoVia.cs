using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace creaCallejero
{
    public partial class WinFormNombreTipoVia : Form
    {
        public static System.Windows.Forms.Button s_btnCreaCallejero;
        public static System.Windows.Forms.Button s_btnCancelar;
        private static System.Windows.Forms.TextBox s_txtNombreVia;
        private static System.Windows.Forms.ComboBox s_comboTipoVia;
        private bool xClicked = true;

        public WinFormNombreTipoVia()
        {
            InitializeComponent();
            s_btnCreaCallejero = btnCrearCallejero;
            s_btnCancelar = btnCancelar;
            s_txtNombreVia = txtBoxNombreVia;
            s_comboTipoVia = cmbBoxTipoVia;
            creaCallejero tiposDeVia = new creaCallejero();
            cmbBoxTipoVia.Items.AddRange(tiposDeVia.siglasComboBox());
            xClicked = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            creaCallejero cancelar = new creaCallejero();
            cancelar.deleteFeature();
            xClicked = false;
            this.Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnCrearCallejero_Click(object sender, EventArgs e)
        {
            creaCallejero calcular = new creaCallejero();
            try
            {
                Global.creationStatus = true;
                calcular.calculateAllFields(Global.viaACalcular ,s_txtNombreVia.Text, s_comboTipoVia.Text);
                Global.creationStatus = false;
                Global.viaACalcular = null;
            }
            catch
            {
                MessageBox.Show("Verifique que está editando sobre una capa 'MALLAVIAL' valida");
                calcular.deleteFeature();
            }
            xClicked = false;
            this.Close();
        }

        private void WinFormNombreTipoVia_Load(object sender, EventArgs e)
        {

        }

        private void WinFormNombreTipoVia_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (xClicked == true)            
            {
                // Then assume that X has been clicked and act accordingly.
                creaCallejero cancelar = new creaCallejero();
                cancelar.deleteFeature();
            }
        }
    }
}
