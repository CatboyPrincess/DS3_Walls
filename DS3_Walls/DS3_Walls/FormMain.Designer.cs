﻿namespace DS3_Walls {
    partial class FormMain {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.checkBoxOverlay = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // checkBoxOverlay
            // 
            this.checkBoxOverlay.AutoSize = true;
            this.checkBoxOverlay.Location = new System.Drawing.Point(13, 13);
            this.checkBoxOverlay.Name = "checkBoxOverlay";
            this.checkBoxOverlay.Size = new System.Drawing.Size(98, 17);
            this.checkBoxOverlay.TabIndex = 0;
            this.checkBoxOverlay.Text = "Enable Overlay";
            this.checkBoxOverlay.UseVisualStyleBackColor = true;
            this.checkBoxOverlay.CheckedChanged += new System.EventHandler(this.CheckBoxOverlay_CheckedChanged);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.checkBoxOverlay);
            this.Name = "FormMain";
            this.Text = "DS3 Walls";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxOverlay;
    }
}

