namespace creaCallejero
{
    partial class WinFormNombreTipoVia
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cmbBoxTipoVia = new System.Windows.Forms.ComboBox();
            this.txtBoxNombreVia = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnCrearCallejero = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cmbBoxTipoVia
            // 
            this.cmbBoxTipoVia.DropDownHeight = 100;
            this.cmbBoxTipoVia.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBoxTipoVia.FormattingEnabled = true;
            this.cmbBoxTipoVia.IntegralHeight = false;
            this.cmbBoxTipoVia.Items.AddRange(new object[] {
            "S/N",
            "AL",
            "AV",
            "BA",
            "CR",
            "CJ",
            "CA",
            "GA",
            "JR",
            "ML",
            "OV",
            "PJ",
            "PL",
            "PQ",
            "PR",
            "PZ",
            "PS",
            "CM",
            "CU",
            "SE",
            "PU",
            "BO",
            "RI",
            "AU",
            "CI",
            "VI"});
            this.cmbBoxTipoVia.Location = new System.Drawing.Point(143, 87);
            this.cmbBoxTipoVia.Name = "cmbBoxTipoVia";
            this.cmbBoxTipoVia.Size = new System.Drawing.Size(283, 24);
            this.cmbBoxTipoVia.TabIndex = 0;
            // 
            // txtBoxNombreVia
            // 
            this.txtBoxNombreVia.Location = new System.Drawing.Point(143, 41);
            this.txtBoxNombreVia.Name = "txtBoxNombreVia";
            this.txtBoxNombreVia.Size = new System.Drawing.Size(283, 22);
            this.txtBoxNombreVia.TabIndex = 1;
            this.txtBoxNombreVia.Text = "SN";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(31, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "NOMBREVIA";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // btnCrearCallejero
            // 
            this.btnCrearCallejero.BackColor = System.Drawing.SystemColors.Control;
            this.btnCrearCallejero.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCrearCallejero.Location = new System.Drawing.Point(143, 145);
            this.btnCrearCallejero.Name = "btnCrearCallejero";
            this.btnCrearCallejero.Size = new System.Drawing.Size(132, 37);
            this.btnCrearCallejero.TabIndex = 3;
            this.btnCrearCallejero.Text = "Crear Callejero";
            this.btnCrearCallejero.UseVisualStyleBackColor = false;
            this.btnCrearCallejero.Click += new System.EventHandler(this.btnCrearCallejero_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.BackColor = System.Drawing.SystemColors.Control;
            this.btnCancelar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelar.Location = new System.Drawing.Point(294, 145);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(132, 37);
            this.btnCancelar.TabIndex = 4;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = false;
            this.btnCancelar.Click += new System.EventHandler(this.button1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(59, 90);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = "TIPOVIA";
            // 
            // WinFormNombreTipoVia
            // 
            this.AcceptButton = this.btnCrearCallejero;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.btnCancelar;
            this.ClientSize = new System.Drawing.Size(482, 253);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnCancelar);
            this.Controls.Add(this.btnCrearCallejero);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtBoxNombreVia);
            this.Controls.Add(this.cmbBoxTipoVia);
            this.HelpButton = true;
            this.Name = "WinFormNombreTipoVia";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Nuevo Callejero";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.WinFormNombreTipoVia_FormClosing);
            this.Load += new System.EventHandler(this.WinFormNombreTipoVia_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbBoxTipoVia;
        private System.Windows.Forms.TextBox txtBoxNombreVia;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnCrearCallejero;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Label label2;
    }
}