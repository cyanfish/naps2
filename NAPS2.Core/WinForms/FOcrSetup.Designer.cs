namespace NAPS2.WinForms
{
    partial class FOcrSetup
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FOcrSetup));
            this.CheckBoxEnableOcr = new System.Windows.Forms.CheckBox();
            this.Label1 = new System.Windows.Forms.Label();
            this.comboLanguages = new System.Windows.Forms.ComboBox();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.LinkGetLanguages = new System.Windows.Forms.LinkLabel();
            this.BtnOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // CheckBoxEnableOcr
            // 
            resources.ApplyResources(this.CheckBoxEnableOcr, "CheckBoxEnableOcr");
            this.CheckBoxEnableOcr.Name = "CheckBoxEnableOcr";
            this.CheckBoxEnableOcr.UseVisualStyleBackColor = true;
            this.CheckBoxEnableOcr.CheckedChanged += new System.EventHandler(this.CheckBoxEnableOcr_CheckedChanged);
            // 
            // Label1
            // 
            resources.ApplyResources(this.Label1, "Label1");
            this.Label1.Name = "Label1";
            // 
            // comboLanguages
            // 
            this.comboLanguages.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboLanguages.FormattingEnabled = true;
            resources.ApplyResources(this.comboLanguages, "comboLanguages");
            this.comboLanguages.Name = "comboLanguages";
            // 
            // BtnCancel
            // 
            resources.ApplyResources(this.BtnCancel, "BtnCancel");
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // LinkGetLanguages
            // 
            resources.ApplyResources(this.LinkGetLanguages, "LinkGetLanguages");
            this.LinkGetLanguages.Name = "LinkGetLanguages";
            this.LinkGetLanguages.TabStop = true;
            this.LinkGetLanguages.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkGetLanguages_LinkClicked);
            // 
            // BtnOK
            // 
            resources.ApplyResources(this.BtnOK, "BtnOK");
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // FOcrSetup
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.BtnOK);
            this.Controls.Add(this.LinkGetLanguages);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.comboLanguages);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.CheckBoxEnableOcr);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FOcrSetup";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox CheckBoxEnableOcr;
        private System.Windows.Forms.Label Label1;
        private System.Windows.Forms.ComboBox comboLanguages;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.LinkLabel LinkGetLanguages;
        private System.Windows.Forms.Button BtnOK;
    }
}