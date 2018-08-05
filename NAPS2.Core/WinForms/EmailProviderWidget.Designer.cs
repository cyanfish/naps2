namespace NAPS2.WinForms
{
    partial class EmailProviderWidget
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pboxIcon = new System.Windows.Forms.PictureBox();
            this.lblName = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pboxIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // pboxIcon
            // 
            this.pboxIcon.Location = new System.Drawing.Point(3, 3);
            this.pboxIcon.Name = "pboxIcon";
            this.pboxIcon.Size = new System.Drawing.Size(50, 50);
            this.pboxIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pboxIcon.TabIndex = 0;
            this.pboxIcon.TabStop = false;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblName.Location = new System.Drawing.Point(59, 18);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(0, 20);
            this.lblName.TabIndex = 1;
            // 
            // EmailProviderWidget
            // 
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.pboxIcon);
            this.Name = "EmailProviderWidget";
            this.Size = new System.Drawing.Size(348, 54);
            ((System.ComponentModel.ISupportInitialize)(this.pboxIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pboxIcon;
        private System.Windows.Forms.Label lblName;
    }
}
