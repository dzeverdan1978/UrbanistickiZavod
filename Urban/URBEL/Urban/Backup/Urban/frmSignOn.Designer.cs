namespace Urban
{
    partial class frmSignOn
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.unos_un = new System.Windows.Forms.TextBox();
            this.unos_pwd = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(49, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Korisničko ime:";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(49, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Lozinka:";
            // 
            // unos_un
            // 
            this.unos_un.Location = new System.Drawing.Point(148, 35);
            this.unos_un.Name = "unos_un";
            this.unos_un.Size = new System.Drawing.Size(251, 20);
            this.unos_un.TabIndex = 2;
            this.unos_un.KeyDown += new System.Windows.Forms.KeyEventHandler(this.unos_un_KeyDown);
            // 
            // unos_pwd
            // 
            this.unos_pwd.Location = new System.Drawing.Point(148, 72);
            this.unos_pwd.Name = "unos_pwd";
            this.unos_pwd.PasswordChar = '*';
            this.unos_pwd.Size = new System.Drawing.Size(251, 20);
            this.unos_pwd.TabIndex = 3;
            this.unos_pwd.KeyDown += new System.Windows.Forms.KeyEventHandler(this.unos_pwd_KeyDown);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(84, 138);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(102, 35);
            this.button1.TabIndex = 4;
            this.button1.Text = "Potvrdi";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(246, 138);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(102, 35);
            this.button2.TabIndex = 5;
            this.button2.Text = "Odustani";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // frmSignOn
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(434, 185);
            this.ControlBox = false;
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.unos_pwd);
            this.Controls.Add(this.unos_un);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "frmSignOn";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "                                          Prijava korisnika";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmSignOn_FormClosed);
            this.VisibleChanged += new System.EventHandler(this.frmSignOn_VisibleChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox unos_un;
        private System.Windows.Forms.TextBox unos_pwd;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}