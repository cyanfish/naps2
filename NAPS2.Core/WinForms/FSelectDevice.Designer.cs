namespace NAPS2.WinForms
{
    partial class FSelectDevice
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FSelectDevice));
            this.BtnCancel = new System.Windows.Forms.Button();
            this.BtnSelect = new System.Windows.Forms.Button();
            this.ListboxDevices = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // BtnCancel
            // 
            resources.ApplyResources(this.BtnCancel, "BtnCancel");
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // BtnSelect
            // 
            resources.ApplyResources(this.BtnSelect, "BtnSelect");
            this.BtnSelect.Name = "BtnSelect";
            this.BtnSelect.UseVisualStyleBackColor = true;
            this.BtnSelect.Click += new System.EventHandler(this.BtnSelect_Click);
            // 
            // ListboxDevices
            // 
            this.ListboxDevices.FormattingEnabled = true;
            resources.ApplyResources(this.ListboxDevices, "ListboxDevices");
            this.ListboxDevices.Name = "ListboxDevices";
            this.ListboxDevices.Format += new System.Windows.Forms.ListControlConvertEventHandler(this.ListboxDevices_Format);
            // 
            // FSelectDevice
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ListboxDevices);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnSelect);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FSelectDevice";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Button BtnSelect;
        private System.Windows.Forms.ListBox ListboxDevices;
    }
}