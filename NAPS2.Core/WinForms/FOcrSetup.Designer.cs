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
            this.checkBoxEnableOcr = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboLanguages = new System.Windows.Forms.ComboBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.linkGetLanguages = new System.Windows.Forms.LinkLabel();
            this.btnOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // checkBoxEnableOcr
            // 
            resources.ApplyResources(this.checkBoxEnableOcr, "checkBoxEnableOcr");
            this.checkBoxEnableOcr.Name = "checkBoxEnableOcr";
            this.checkBoxEnableOcr.UseVisualStyleBackColor = true;
            this.checkBoxEnableOcr.CheckedChanged += new System.EventHandler(this.checkBoxEnableOcr_CheckedChanged);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // comboLanguages
            // 
            this.comboLanguages.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboLanguages.FormattingEnabled = true;
            resources.ApplyResources(this.comboLanguages, "comboLanguages");
            this.comboLanguages.Name = "comboLanguages";
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // linkGetLanguages
            // 
            resources.ApplyResources(this.linkGetLanguages, "linkGetLanguages");
            this.linkGetLanguages.Name = "linkGetLanguages";
            this.linkGetLanguages.TabStop = true;
            this.linkGetLanguages.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkGetLanguages_LinkClicked);
            // 
            // btnOK
            // 
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // FOcrSetup
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.linkGetLanguages);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.comboLanguages);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkBoxEnableOcr);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FOcrSetup";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxEnableOcr;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboLanguages;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.LinkLabel linkGetLanguages;
        private System.Windows.Forms.Button btnOK;
    }
}