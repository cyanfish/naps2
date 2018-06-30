namespace NAPS2.WinForms
{
    partial class FOcrLanguageDownload
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FOcrLanguageDownload));
            this.Label3 = new System.Windows.Forms.Label();
            this.BtnDownload = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.LvLanguages = new System.Windows.Forms.ListView();
            this.Label1 = new System.Windows.Forms.Label();
            this.LabelSizeEstimate = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Label3
            // 
            resources.ApplyResources(this.Label3, "Label3");
            this.Label3.Name = "Label3";
            // 
            // BtnDownload
            // 
            resources.ApplyResources(this.BtnDownload, "BtnDownload");
            this.BtnDownload.Name = "BtnDownload";
            this.BtnDownload.UseVisualStyleBackColor = true;
            this.BtnDownload.Click += new System.EventHandler(this.BtnDownload_Click);
            // 
            // BtnCancel
            // 
            resources.ApplyResources(this.BtnCancel, "BtnCancel");
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // LvLanguages
            // 
            this.LvLanguages.CheckBoxes = true;
            resources.ApplyResources(this.LvLanguages, "LvLanguages");
            this.LvLanguages.Name = "LvLanguages";
            this.LvLanguages.UseCompatibleStateImageBehavior = false;
            this.LvLanguages.View = System.Windows.Forms.View.List;
            this.LvLanguages.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.LvLanguages_ItemChecked);
            // 
            // Label1
            // 
            resources.ApplyResources(this.Label1, "Label1");
            this.Label1.Name = "Label1";
            // 
            // LabelSizeEstimate
            // 
            resources.ApplyResources(this.LabelSizeEstimate, "LabelSizeEstimate");
            this.LabelSizeEstimate.Name = "LabelSizeEstimate";
            // 
            // FOcrLanguageDownload
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.LabelSizeEstimate);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.LvLanguages);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnDownload);
            this.Controls.Add(this.Label3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FOcrLanguageDownload";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label Label3;
        private System.Windows.Forms.Button BtnDownload;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.ListView LvLanguages;
        private System.Windows.Forms.Label Label1;
        private System.Windows.Forms.Label LabelSizeEstimate;

    }
}