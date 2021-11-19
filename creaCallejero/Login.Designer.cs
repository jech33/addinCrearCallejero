namespace creaCallejero
{
    partial class Login
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
            this.txtBoxUser = new System.Windows.Forms.TextBox();
            this.txtBoxPassword = new System.Windows.Forms.TextBox();
            this.btnLogin = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lblPass = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtBoxUser
            // 
            this.txtBoxUser.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtBoxUser.Location = new System.Drawing.Point(132, 46);
            this.txtBoxUser.Name = "txtBoxUser";
            this.txtBoxUser.Size = new System.Drawing.Size(230, 22);
            this.txtBoxUser.TabIndex = 0;
            this.txtBoxUser.Text = "Usuario";
            // 
            // txtBoxPassword
            // 
            this.txtBoxPassword.Location = new System.Drawing.Point(132, 117);
            this.txtBoxPassword.Name = "txtBoxPassword";
            this.txtBoxPassword.PasswordChar = '●';
            this.txtBoxPassword.Size = new System.Drawing.Size(230, 22);
            this.txtBoxPassword.TabIndex = 1;
            this.txtBoxPassword.Text = "Contraseña";
            // 
            // btnLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(197, 181);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(91, 31);
            this.btnLogin.TabIndex = 2;
            this.btnLogin.Text = "LOGIN";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(128, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "Usuario";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // lblPass
            // 
            this.lblPass.AutoSize = true;
            this.lblPass.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPass.Location = new System.Drawing.Point(128, 91);
            this.lblPass.Name = "lblPass";
            this.lblPass.Size = new System.Drawing.Size(95, 20);
            this.lblPass.TabIndex = 4;
            this.lblPass.Text = "Contraseña";
            // 
            // Login
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(482, 253);
            this.Controls.Add(this.txtBoxUser);
            this.Controls.Add(this.txtBoxPassword);
            this.Controls.Add(this.lblPass);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnLogin);
            this.Name = "Login";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login";
            this.Load += new System.EventHandler(this.Login_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtBoxUser;
        private System.Windows.Forms.TextBox txtBoxPassword;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblPass;
    }
}