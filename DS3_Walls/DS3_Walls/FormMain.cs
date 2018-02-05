using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DS3_Walls {
    public partial class FormMain : Form {

        FormOverlay overlay = new FormOverlay();

        public FormMain() {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e) {

        }

        private void CheckBoxOverlay_CheckedChanged(object sender, EventArgs e) {
            //System.Console.WriteLine("Hello world!");
            if (checkBoxOverlay.Checked) {
                overlay.Show();
            } else {
                overlay.Hide();
            }
        }
    }
}
