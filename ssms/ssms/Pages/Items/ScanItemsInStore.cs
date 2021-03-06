﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Drawing.Printing;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using ssms.DataClasses;

namespace ssms.Pages.Items
{
    public partial class ScanItemsInStore : UserControl
    {
        bool busy = false;
        List<LTS.Item> dbList = new List<LTS.Item>();
        List<inventory> theList = new List<inventory>();
        List<inventory> missing = new List<inventory>();
        List<inventory> found = new List<inventory>();
        public bool config = false;
        List<LTS.Store> listS = new List<LTS.Store>();
        List<ImpinjRevolution> impinjrev = new List<ImpinjRevolution>();
        LTS.Settings set = new LTS.Settings();
        SettingsMain sm = new SettingsMain();
        public List<string> check = new List<string>();
        
        public ScanItemsInStore()
        {
            InitializeComponent();
        }

        //Margo
        private void button2_Click(object sender, EventArgs e)
        {
            ((Main)this.Parent.Parent).ChangeView<Pages.Items.Items>();
        }

        //Margo
        private void button4_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        //Margo
        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            //Object i = lbxIn.DataSource;
            //Object imissing = lbxMissing.DataSource;

            //iTextSharp.text.Font font = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 8);
            //string folderPath = saveFileDialog1.FileName + ".pdf";


            ////Creating iTextSharp Table from the DataTable data
            //Document pdfDoc = new Document(PageSize.A4);

            //PdfPTable pdfTable = new PdfPTable(dataGridView1.ColumnCount);
            //pdfTable.DefaultCell.Padding = dataGridView1.DefaultCellStyle.Padding.All;

            //pdfTable.WidthPercentage = 100;
            //pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;
            //pdfTable.DefaultCell.BorderWidth = 0;



            ////Adding Header row
            //foreach (DataGridViewColumn column in dataGridView1.Columns)
            //{
            //    PdfPCell cell = new PdfPCell(new Phrase(column.HeaderText));
            //    cell.BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255);
            //    cell.HorizontalAlignment = Element.ALIGN_CENTER;
            //    pdfTable.AddCell(cell);
            //}

            ////Adding DataRow
            //foreach (DataGridViewRow row in dataGridView1.Rows)
            //{
            //    foreach (DataGridViewCell cell in row.Cells)
            //    {
            //        // pdfTable.AddCell(cell.Value.ToString());
            //        PdfPCell cellRows = new PdfPCell(new Phrase(cell.Value.ToString(), font));
            //        int R = cell.Style.BackColor.R;
            //        int G = cell.Style.BackColor.G;
            //        int B = cell.Style.BackColor.B;
            //        if (R == 0 && G == 0 && B == 0)
            //        {
            //            R = 255;
            //            G = 255;
            //            B = 255;
            //        }
            //        cellRows.BackgroundColor = new iTextSharp.text.BaseColor(R, G, B);
            //        cellRows.HorizontalAlignment = Element.ALIGN_CENTER;
            //        pdfTable.AddCell(cellRows);

            //    }
            //}
            //Paragraph writing = new iTextSharp.text.Paragraph("Route 66 Car Dealership " + Environment.NewLine + "Customers Information                " + DateTime.Now.ToString() + Environment.NewLine + Environment.NewLine);

            //using (FileStream stream = new FileStream(folderPath, FileMode.Create))
            //{

            //    PdfWriter.GetInstance(pdfDoc, stream);
            //    pdfDoc.Open();
            //    pdfDoc.Add(writing);
            //    pdfDoc.Add(pdfTable);
            //    pdfDoc.Close();
            //    stream.Close();
            //}
        }

        private void ScanItemsInStore_Load(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnC.Enabled = true;
            
            listS = DAT.DataAccess.GetStore().ToList();
            List<string> s = new List<string>();

            for (int i = 0; i < listS.Count; i++)
            {
                s.Add(listS[i].StoreName);
            }
            comboBoxStore.DataSource = s;
        }

        private void comboBoxStore_SelectedIndexChanged(object sender, EventArgs e)
        {
            panel1.Visible = false;
            int index = comboBoxStore.SelectedIndex;
            int storeID = listS[index].StoreID;

            set = DAT.DataAccess.GetSettings().Where(r => r.StoreID == storeID && r.SettingsSelect == true).FirstOrDefault();
            if (set != null)
            {
                setName.Text = set.SettingsName;
                List<LTS.Reader> r = new List<LTS.Reader>();
                r = DAT.DataAccess.GetReader().Where(y => y.SettingsID == set.SettingsID).ToList();
                List<string> rs = new List<string>();
                for (int q = 0; q < r.Count; q++)
                {
                    rs.Add(r[q].IPaddress + " :  " + r[q].NumAntennas + " antenna(s)");
                }
                lbReader.DataSource = rs;
                panel1.Visible = true;

                dbList = DAT.DataAccess.GetItem().Where(t => t.ItemStatus == true && t.StoreID == storeID).ToList();
                theList.Clear();
                for(int h = 0; h < dbList.Count; h++)
                {
                    inventory i = new inventory();
                    i.itemID = dbList[h].ItemID;
                    i.EPC = dbList[h].TagEPC;
                    LTS.Product p = DAT.DataAccess.GetProduct().Where(t => t.ProductID == dbList[h].ProductID).FirstOrDefault();
                    if (p != null)
                    {
                        i.ProductName = p.ProductName;
                        i.ProductDescription = p.ProductDescription;
                        theList.Add(i);
                    }
                }
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            button4.Enabled = false;
            button2.Enabled = false;
            comboBoxStore.Enabled = false;
            lblConnect.Text = "Connecting ...";
            lblStartRead.Visible = false;
            lblStop.Visible = false;
            btnStart.Enabled = false;
            
            lblConnect.Visible = true;
            sm = null;
            sm = new SettingsMain();
            impinjrev.Clear();
            sm.SettingsID = set.SettingsID;
            sm.SettingsName = set.SettingsName;
            sm.SettingsSelect = set.SettingsSelect;
            sm.StoreID = set.StoreID;

            LTS.Store store = DAT.DataAccess.GetStore().Where(i => i.StoreID == sm.StoreID).FirstOrDefault();
            sm.StoreLocation = store.StoreLocation;
            sm.StoreName = store.StoreName;

            List<LTS.Reader> readers = new List<LTS.Reader>();
            readers = DAT.DataAccess.GetReader().Where(j => j.SettingsID == sm.SettingsID).ToList();
            for (int j = 0; j < readers.Count; j++)
            {
                ReaderMain rm = new ReaderMain();
                rm.ReaderID = readers[j].ReaderID;
                rm.IPaddress = readers[j].IPaddress;
                rm.NumAntennas = readers[j].NumAntennas;
                rm.antennas = DAT.DataAccess.GetAntenna().Where(q => q.ReaderID == rm.ReaderID).ToList();

                sm.Readers.Add(rm);

            }
            bool checks = true;

            for (int x = 0; x < sm.Readers.Count; x++)
            {

                ImpinjRevolution ir = new ImpinjRevolution();
                ir.ReaderScanMode = ScanMode.FullScan;
                ir.HostName = sm.Readers[x].IPaddress;
                ir.Antennas = sm.Readers[x].antennas;

                ir.TagRead += ir_TagRead;
                ir.Connect();

                impinjrev.Add(ir);
                if (!ir.isConnected)
                {
                    if (checks == true)
                    {
                        checks = false; 
                    }

                }
            }

            if (checks == true)
            {
                config = true;
                lblConnect.Text = "Connected";
                btnStart.Enabled = true;
                btnStop.Enabled = false;
                ((Form1)this.Parent.Parent.Parent.Parent).scan = true;



            }
            else
            {
                lblConnect.Text = "Not Connected";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            ((Form1)this.Parent.Parent.Parent.Parent).scan = true;
            button4.Enabled = false;
            comboBoxStore.Enabled = false;
            busy = true;
            missing = theList;
            populate();
            lblStartRead.Text = "Starting...";
            lblStartRead.Visible = true;
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            btnC.Enabled = false;

            ////Start
             
                btnStart.Enabled = false;
                btnStop.Enabled = true;
                

                impinjrev.ForEach(imp =>
                {
                    imp.TagRead += ir_TagRead;
                    imp.StartRead();
                });

            lblStartRead.Text = "Started and Reading ...";




        }

        private void button1_Click(object sender, EventArgs e)
        {
            button4.Enabled = true;
            comboBoxStore.Enabled = true;
            DialogResult res = MessageBox.Show("You are about to stop scanning, Click OK to Stop Scanning, or Cancel!", "", MessageBoxButtons.OKCancel);
            if (DialogResult.OK == res)
            {

                
                btnStart.Enabled = false;
                btnStop.Enabled = false;
                btnC.Enabled = true;
                lblStop.Visible = true;
                lblStop.Text = "Stopping...";

                for (int i = 0; i < impinjrev.Count; i++)
                {
                    impinjrev[i].StopRead();
                    impinjrev[i].Disconnect();

                }
                lblStop.Text = "Stopped and Disconnected...";
                lblConnect.Visible = false;
                lblStartRead.Visible = false;
                ((Form1)this.Parent.Parent.Parent.Parent).scan = false;
                busy = false;
                button2.Enabled = true;


            }
        }

        void ir_TagRead(TagInfo tag, EventArgs e)
        {
            if (tag != null)
            {
                string tagEPC = tag.TagNo;
                if (!check.Contains(tagEPC))  //add unique TIDs only
                {
                    if (tagEPC != "" && tagEPC != null)
                    {
                        inventory i = missing.Where(q => q.EPC == tagEPC).FirstOrDefault();
                        if (i != null)
                        {
                            check.Add(tagEPC);
                            found.Add(i);
                            missing.RemoveAt(missing.IndexOf(i));
                            populate();

                        }
                        

                    }
                }

            }
        }


        void populate()
        {
            this.Invoke(new MethodInvoker(delegate ()
            {
                lbxIn.DataSource = null;
                lbxIn.Items.Clear();
            for(int i = 0; i < found.Count; i++)
            {
                lbxIn.Items.Add("ItemID: " + found[i].itemID + "  Product: " + found[i].ProductName);
            }
            

            lbxMissing.DataSource = null;
            lbxMissing.Items.Clear();
            for (int i = 0; i < missing.Count; i++)
            {
                lbxMissing.Items.Add("ItemID: " + missing[i].itemID + "  Product: " + missing[i].ProductName);
            }
            }));

        }
    }

}

    


