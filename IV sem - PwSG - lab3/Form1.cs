using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace IV_sem___PwSG___lab3
{
    public partial class Form1 : Form
    {
        Dictionary<Button, string> libraryDictionary = new Dictionary<Button, string>();    // przechowuje linki dla danego elementu biblioteki
        private string filename;
        private bool picLoaded1 = false;
        private bool picLoaded2 = false;
        private bool isAnyLibraryItemClicked;
        Button libraryItemClicked;
        private double trackBarValue;
        private int processesInBackground;
        private int saved;
        List<string> fileList = new List<string>();     // lista plikow w bibliotece
        BackgroundWorker bw1 = new BackgroundWorker();  // do aktywowania przycisku
        BackgroundWorker bw2 = new BackgroundWorker();  // do blendowania
        BackgroundWorker bw3 = new BackgroundWorker();  // do blendowania
        Form2 pictureWindow;

        public Form1()
        {
            InitializeComponent();
            this.MaximizeBox = false;
            //this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximumSize = new Size(Screen.PrimaryScreen.Bounds.Width, 457);
            this.MinimumSize = new Size(tableLayoutPanel1.Bounds.Width, 457);
            progressBar.Visible = progressBar2.Visible = false;
            buttonPic1.BackgroundImageLayout = ImageLayout.Stretch;
            buttonPic2.BackgroundImageLayout = ImageLayout.Stretch;
            buttonPic1.FlatStyle = FlatStyle.Flat;
            buttonPic2.FlatStyle = FlatStyle.Flat;
            buttonPerformBlending.FlatStyle = FlatStyle.Flat;
            buttonPerformBlending.Enabled = false;
            this.KeyPreview = true;
            this.KeyDown += keyEvent;
            trackBar.MouseEnter += trackBar_MouseEnter;
            trackBar.MouseLeave += trackBar_MouseLeave;
            trackBar.ValueChanged += trackBar_ValueChanged;
            trackBarValue = (double)trackBar.Value / 11;
            flowLayoutPanel.AllowDrop = true;
            flowLayoutPanel.DragEnter += DragEnterHandler;
            flowLayoutPanel.DragDrop += DragDropHandler;
            if (File.Exists("XmlData.xml")) ReadFromXml();
            bw2.WorkerReportsProgress = true;
            bw2.DoWork += bw2_DoWork;
            bw2.ProgressChanged += bw2_ProgressChanged;
            bw2.RunWorkerCompleted += bw2_RunWorkerCompleted;
            bw3.WorkerReportsProgress = true;
            bw3.DoWork += bw2_DoWork;
            bw3.ProgressChanged += bw3_ProgressChanged;
            bw3.RunWorkerCompleted += bw2_RunWorkerCompleted;
            bw1.DoWork += bw1_DoWork;
            bw1.RunWorkerCompleted += bw1_RunWorkerCompleted;
            bw1.RunWorkerAsync();
        }

        private void buttonPic_Click(object sender, EventArgs e)
        {
            Button thisButton = sender as Button;
            if (!isAnyLibraryItemClicked)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter += "Image files (*.jpg, *.bmp, *.png) | *.jpg; *.bmp; *.png | All files (*.*) | *.*";
                while (true)
                {
                    DialogResult dr = ofd.ShowDialog();
                    if (dr == DialogResult.OK)
                    {
                        filename = ofd.FileName;
                        string ext = Path.GetExtension(filename);
                        if (ext != ".jpg" && ext != ".bmp" && ext != ".png")
                        {
                            MessageBox.Show("Wrong file extension!\n You need to upload graphics.", "Error",
                                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                            continue;
                        }
                        PictureBox myPicture = new PictureBox();
                        myPicture.Image = new Bitmap(filename);
                        thisButton.BackgroundImage = myPicture.Image;
                        return;
                    }
                    else if (dr == DialogResult.Cancel)
                    {
                        return;
                    }
                }
            }
            else // jakis element biblioteki jest zaznaczony - wczytujemy ten element
            {
                thisButton.BackgroundImage = new Bitmap(libraryItemClicked.BackgroundImage);
                isAnyLibraryItemClicked = false;
                libraryItemClicked = null;
            }
        }

        private void buttonPic1_Click(object sender, EventArgs e)
        {
            picLoaded1 = true;
            buttonPic_Click(sender, e);
        }

        private void buttonPic2_Click(object sender, EventArgs e)
        {
            picLoaded2 = true;
            buttonPic_Click(sender, e);
        }

        private void keyEvent(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12)
            {
                serviceScreenshot();
            }
            else if (e.KeyCode == Keys.Delete)  // usuwamy element z biblioteki
            {
                if (isAnyLibraryItemClicked)
                {
                    fileList.Remove(libraryDictionary[libraryItemClicked]); // usun sciezke z listy sciezek do plikow obecnych w bibliotece
                    RemoveFromXml(libraryDictionary[libraryItemClicked]);
                    libraryDictionary.Remove(libraryItemClicked);           // usun pare 'button - path' z mapy
                    flowLayoutPanel.Controls.Remove(libraryItemClicked);
                }
            }
        }

        private void serviceScreenshot()
        {
            Button thisButton;
            if (picLoaded1 == true)
            {
                picLoaded2 = true;
                thisButton = buttonPic2;
            }
            else
            {
                thisButton = buttonPic1;
                picLoaded1 = true;
            }

            Bitmap bmpScreenCapture = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                                    Screen.PrimaryScreen.Bounds.Height);
            Graphics g = Graphics.FromImage(bmpScreenCapture);
            g.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
                             Screen.PrimaryScreen.Bounds.Y,
                             0, 0,
                             Screen.PrimaryScreen.Bounds.Size,
                             CopyPixelOperation.SourceCopy);
            thisButton.BackgroundImage = bmpScreenCapture;
        }

        private void bw1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker thisWorker = sender as BackgroundWorker;
            while (picLoaded1 == false || picLoaded2 == false) { };
        }

        private void bw1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true) { }
            else if (e.Error != null) { }
            else
            {
                buttonPerformBlending.Enabled = true;
            }
        }

        private void trackBar_MouseEnter(object sender, EventArgs e)
        {
            trackBar.Cursor = System.Windows.Forms.Cursors.IBeam;
        }
        private void trackBar_MouseLeave(object sender, EventArgs e)
        {
            trackBar.Cursor = System.Windows.Forms.Cursors.Default;
        }
        private void trackBar_ValueChanged(object sender, EventArgs e)
        {
            TrackBar myBar = sender as TrackBar;
            trackBarValue = (double)myBar.Value / 11;
        }

        private void buttonPerformBlending_Click(object sender, EventArgs e)
        {
            processesInBackground++;
            if (processesInBackground == 2)
                buttonPerformBlending.Enabled = false;
            labelProgress.Text = "OPERATION IN PROGRESS";
            PerformBlending();
        }

        private void PerformBlending()
        {
            if (!bw2.IsBusy)
            {
                progressBar.Visible = true;
                bw2.RunWorkerAsync(this);
                this.Activate();
            }
            else
            {
                progressBar2.Visible = true;
                bw3.RunWorkerAsync(this);
                this.Activate();
            }
        }
        private void bw2_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            Form1 mainForm = e.Argument as Form1;
            Bitmap leftBmp = new Bitmap(mainForm.buttonPic1.BackgroundImage);
            Bitmap rightBmp = new Bitmap(mainForm.buttonPic2.BackgroundImage);
            Bitmap resultBmp;
            int leftX = leftBmp.Width; int leftY = leftBmp.Height;
            int rightX = rightBmp.Width; int rightY = rightBmp.Height;
            int x = leftX < rightX ? leftX : rightX;
            int y = leftY < rightY ? leftY : rightY;
            resultBmp = new Bitmap(x, y);
            double alfa = mainForm.trackBarValue;
            for (int i = 0; i < x; ++i)
            //Parallel.For(0, x, i =>
            {
                for (int j = 0; j < y; ++j)
                { //(alfa*kolor1(i,j) + (1-alfa)*kolor2(i,j))
                    if (((int)(((double)i / (double)x + (double)j / ((double)(x * y))) * 100)) % 3 == 0)
                        bw.ReportProgress((int)(((double)i / (double)x + (double)j / ((double)x * y)) * 100));
                    int leftR = (i < leftX && j < leftY) ? (int)(alfa * leftBmp.GetPixel(i, j).R) : 0;
                    int leftG = (i < leftX && j < leftY) ? (int)(alfa * leftBmp.GetPixel(i, j).G) : 0;
                    int leftB = (i < leftX && j < leftY) ? (int)(alfa * leftBmp.GetPixel(i, j).B) : 0;

                    int rightR = (i < rightX && j < rightY) ? (int)((1 - alfa) * rightBmp.GetPixel(i, j).R) : 0;
                    int rightG = (i < rightX && j < rightY) ? (int)((1 - alfa) * rightBmp.GetPixel(i, j).G) : 0;
                    int rightB = (i < rightX && j < rightY) ? (int)((1 - alfa) * rightBmp.GetPixel(i, j).B) : 0;

                    int r = leftR + rightR;
                    int g = leftG + rightG;
                    int b = leftB + rightB;
                    resultBmp.SetPixel(i, j, Color.FromArgb(r, g, b));
                }
            }//);
            //Rectangle rectLeftBmp = new Rectangle(0, 0, leftBmp.Width, rightBmp.Height);
            //Rectangle rectRightBmp = new Rectangle(0, 0, rightBmp.Width, rightBmp.Height);
            //BitmapData leftBmpData = leftBmp.LockBits(rectLeftBmp, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);   // lockujemy bitmapy i doastajemy o nich info
            //BitmapData rightBmpData = rightBmp.LockBits(rectRightBmp, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            //IntPtr ptrLeft = leftBmpData.Scan0;     // bierzemy wskaznik na pierwszy pixel w bitmapie
            //IntPtr ptrRight = rightBmpData.Scan0;
            //int numberLeftBmp = leftBmpData.Stride * leftBmp.Height;
            //int numberRightBmp = rightBmpData.Stride * rightBmp.Height;
            //byte[] rgbLeftValues = new byte[numberLeftBmp]; // w tych tablicach zapiszemy wszystki RGB z obu bitmap
            //byte[] rgbRightValues = new byte[numberRightBmp];
            //Marshal.Copy(ptrLeft, rgbLeftValues, 0, numberLeftBmp);
            //Marshal.Copy(ptrRight, rgbRightValues, 0, numberRightBmp);

            //Rectangle rectResultBmp = new Rectangle(0, 0, resultBmp.Width, resultBmp.Height);
            //BitmapData resultBmpData = resultBmp.LockBits(rectResultBmp, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            //IntPtr ptrResult = resultBmpData.Scan0;
            //int numberResultBmp = resultBmpData.Stride * resultBmp.Height;
            //byte[] rgbResultValues = new byte[numberResultBmp];
            //for (int i = 0; i < numberResultBmp; ++i)   // tworzymy wynikowe rgb na podstawie dwoch wejsciowych
            //{
            //    if ((int)(((double)i / (double)numberResultBmp) * 100) % 5 == 0)
            //        bw.ReportProgress((int)(((double)i / (double)numberResultBmp) * 100));
            //    rgbResultValues[i] = (byte)(alfa * rgbLeftValues[i] + (1 - alfa) * rgbRightValues[i]);
            //}
            //Marshal.Copy(rgbResultValues, 0, ptrResult, numberResultBmp);   // kopiujemy zmodyfikowane rgb do wynikowej bitmapy
            //leftBmp.UnlockBits(leftBmpData);
            //rightBmp.UnlockBits(rightBmpData);
            //resultBmp.UnlockBits(resultBmpData);

            e.Result = resultBmp;

        }
        private void bw2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (progressBar.Value < 100)
                progressBar.Value = e.ProgressPercentage;
        }
        private void bw3_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (progressBar2.Value < 100)
                progressBar2.Value = e.ProgressPercentage;
        }
        private void bw2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            if (bw == bw2)
            {
                progressBar.Visible = false;
                progressBar.Value = 0;
            }
            else if (bw == bw3)
            {
                progressBar2.Visible = false;
                progressBar2.Value = 0;
            }
            buttonPerformBlending.Enabled = true;
            processesInBackground--;
            if (processesInBackground == 0)
                labelProgress.Text = " ";
            Bitmap resultBmp = e.Result as Bitmap;
            pictureWindow = new Form2();
            pictureWindow.OnAddToLibrary += AddToLibraryHandler;
            pictureWindow.setPicture(resultBmp, saved);
            saved++;
            pictureWindow.Show(this);

        }

        protected void AddToLibraryHandler(object sender, EventArgs e)
        {
            Form2 child = sender as Form2;
            if (fileList.Contains(child.pathToPic))
            {
                MessageBox.Show("File with given path is already in the library!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            fileList.Add(child.pathToPic);
            AddItemToLibrary(child.pathToPic);
            WriteToXml();
        }

        private void LibraryMemebrClickHandler(object sender, EventArgs e)
        {
            Button thisPicture = sender as Button;
            if ((string)thisPicture.Tag == "unclicked")
            {
                thisPicture.Select();
                thisPicture.Tag = "clicked";
                isAnyLibraryItemClicked = true;
                libraryItemClicked = thisPicture;
            }
            else
            {
                thisPicture.Parent.Focus();
                thisPicture.Tag = "unclicked";
                isAnyLibraryItemClicked = false;
                libraryItemClicked = null;
            }
        }

        private void DragEnterHandler(object sender, System.Windows.Forms.DragEventArgs e)
        {
            e.Effect = System.Windows.Forms.DragDropEffects.Copy;
        }

        private void DragDropHandler(object sender, System.Windows.Forms.DragEventArgs e)
        {
            List<string> extList = new List<string> {".png", ".jpg", ".bmp" };
            string[] dropped = (string[])e.Data.GetData(System.Windows.Forms.DataFormats.FileDrop);
            List<string> files = dropped.ToList();

            if (!files.Any())
                return;

            foreach (string drop in dropped)
                if (Directory.Exists(drop))
                    files.AddRange(Directory.GetFiles(drop, "*.*", SearchOption.AllDirectories).Where(s => extList.Contains(Path.GetExtension(s))) );

            foreach (string file in files)
            {
                if (fileList.Contains(file))
                {
                    MessageBox.Show("File with given path is already in the library!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    continue;
                }
                if (!fileList.Contains(file) && extList.Contains(Path.GetExtension(file.ToLower())))
                {
                    AddItemToLibrary(file);
                    fileList.Add(file);
                    WriteToXml();
                }
            }
        }

        private void AddItemToLibrary(string path)
        {
            Button newGaleryMember = new Button();
            newGaleryMember.Bounds = new Rectangle(0, 0, 150, 150);
            newGaleryMember.BackgroundImageLayout = ImageLayout.Stretch;
            try
            {
                newGaleryMember.BackgroundImage = new Bitmap(path);
            }
            catch (System.ArgumentException e)
            {
                RemoveFromXml(path);
                return;
            }
            newGaleryMember.Click += LibraryMemebrClickHandler;
            newGaleryMember.Tag = "unclicked";  // tag pomoze w sprawdzaniu czy element galerii jest zaznaczony
            flowLayoutPanel.Controls.Add(newGaleryMember);
            libraryDictionary[newGaleryMember] = path;
        }

        private void WriteToXml()
        {
            List<System.Xml.Linq.XElement> intNodeList = new List<System.Xml.Linq.XElement>();
            foreach (var link in fileList)
            {
                intNodeList.Add(new System.Xml.Linq.XElement("imageLink", link));
            }
            System.Xml.Linq.XElement externalNode = new System.Xml.Linq.XElement("libraryImages", intNodeList);
            externalNode.Save("XmlData.xml");
        }

        private void ReadFromXml()
        {

            System.Xml.Linq.XDocument xdoc = System.Xml.Linq.XDocument.Load("XmlData.xml");
            fileList = xdoc.Root.Elements("imageLink").Select(el => el.Value).ToList();         // zapisz sciezki do elementow na lsite
            foreach (var el in fileList)
            {
                AddItemToLibrary(el);   // dodaj elementy do biblioteki
            }
        }

        private void RemoveFromXml(string pathToRemove)
        {
            System.Xml.Linq.XDocument xdoc = System.Xml.Linq.XDocument.Load("XmlData.xml");
            xdoc.Root.Elements("imageLink").Select(el => el).Where(el => el.Value == pathToRemove).ToList().ForEach(el => el.Remove());
            xdoc.Save("XmlData.xml");
        }

        protected override CreateParams CreateParams        // double buffering
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }
    }
}
