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
            ((System.ComponentModel.ISupportInitialize)(this.pboxIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // pboxIcon
            // 
            this.pboxIcon.BackColor = System.Drawing.Color.Transparent;
            this.pboxIcon.Location = new System.Drawing.Point(3, 3);
            this.pboxIcon.Name = "pboxIcon";
            this.pboxIcon.Size = new System.Drawing.Size(48, 48);
            this.pboxIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pboxIcon.TabIndex = 0;
            this.pboxIcon.TabStop = false;
            // 
            // EmailProviderWidget
            // 
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.pboxIcon);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Padding = new System.Windows.Forms.Padding(50, 0, 0, 0);
            this.Size = new System.Drawing.Size(348, 54);
            this.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.UseVisualStyleBackColor = false;
            this.Click += new System.EventHandler(this.EmailProviderWidget_Click);
            this.MouseEnter += new System.EventHandler(this.EmailProviderWidget_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.EmailProviderWidget_MouseLeave);
            ((System.ComponentModel.ISupportInitialize)(this.pboxIcon)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pboxIcon;
    }
}
