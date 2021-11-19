using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using Oracle.DataAccess.Client;
using System.Configuration;

namespace creaCallejero
{
    public partial class Login : Form
    {

        public Login()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Login_Load(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            String usuario = txtBoxUser.Text;
            String password = txtBoxPassword.Text;

            if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("El usuario y el password son datos requeridos");
            }
            else
            {
                Validar(usuario, password);
            }
        }

        private void Validar(string username, string password)
        {
            // Cadena de conexion en config
            string CadenaConexion = Global.connectionString;
            CadenaConexion = string.Format(CadenaConexion, username, password);

            try
            {
                Form1 formulario = new Form1();
                formulario.cnn = CadenaConexion;
                formulario.ShowDialog();
                this.Hide();
                Global.nombreUsuario = username;
                Global.dbStatusConnected = true;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }

        }


    }
}
