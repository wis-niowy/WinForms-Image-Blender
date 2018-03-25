using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IV_sem___PwSG___lab3
{
    public partial class Form2 : Form
    {
        public event EventHandler OnAddToLibrary;
        public Bitmap currentPicture;
        public bool wasSaved;
        public string pathToPic;
        private int saved;

        public Form2()
        {
            InitializeComponent();
            this.Deactivate += DeactivateHandler;
            toolStripMenuSave.Click += ToolStripMenuSaveHandler;
            toolStripMenuAdd.Click += ToolStripMenuAddHandler;
        }

        public void setPicture(Bitmap pic, int sav)
        {
            pictureBox.Image = currentPicture = pic;
            saved = sav;
        }

        private void DeactivateHandler(object sender, EventArgs e) // jezeli klikniecie poza okeinko - wylacz menu kontekstowe
        {
            if (contextMenuStrip.Enabled == true)
                contextMenuStrip.Close();
        }

        protected override CreateParams CreateParams    // double buffering
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = e as MouseEventArgs;
            if (me.Button == MouseButtons.Right)
            {
                contextMenuStrip.Show(me.Location);
            }
            else
            {
                if (contextMenuStrip.Enabled == true)
                    contextMenuStrip.Close();
            }
        }

        private void ToolStripMenuSaveHandler(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.DefaultExt = ".png";
            sfd.FileName = "MyImage" + saved + ".png";
            sfd.Filter = "Image files (*.jpg, *.bmp, *.png) | *.jpg; *.bmp; *.png | All files (*.*) | *.*";
            DialogResult dr = sfd.ShowDialog();

            if (dr == DialogResult.OK)
            {
                pictureBox.Image.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
                pathToPic = sfd.FileName;
                wasSaved = true;
            }
        }

        private void ToolStripMenuAddHandler(object sender, EventArgs e)
        {
            if (!wasSaved)
                ToolStripMenuSaveHandler(null, null);
            if (OnAddToLibrary != null)
                OnAddToLibrary(this, null); // rise an event to notify main form it should update its library
        }
    }
}
