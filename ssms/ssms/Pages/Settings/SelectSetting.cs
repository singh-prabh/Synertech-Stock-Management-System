﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ssms.DataClasses;

namespace ssms.Pages.Settings
{
    public partial class SelectSetting : UserControl
    {
        //Margo
        List<LTS.Store> listS = new List<LTS.Store>();
        List<LTS.Settings> listSet = new List<LTS.Settings>();
        int SelectedStore;
        int SelectedSetting;

        public SelectSetting()
        {
            InitializeComponent();
        }

        //Margo
        private void buttonBack_Click(object sender, EventArgs e)
        {
            ((Main)this.Parent.Parent).ChangeView<Settings>();
        }

        //Margo
        private void SelectSetting_Load(object sender, EventArgs e)
        {
            //load store names into combo box from db
            listS = DAT.DataAccess.GetStore().ToList();
            List<string> S = new List<string>();

            for (int x = 0; x < listS.Count; x++)
            {
                S.Add(listS[x].StoreName);
            }
            comboBoxStore.DataSource = S;
        }

        //Margo
        private void comboBoxStore_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataGridViewReaders.DataSource = null;
            dataGridViewReaders.Rows.Clear();

            int storeIndex = comboBoxStore.SelectedIndex;
            int storeID = listS[storeIndex].StoreID;
            SelectedStore = storeID;

           listSet = DAT.DataAccess.GetSettings().Where(i => i.StoreID == storeID).ToList();

            List<String> setName = new List<string>();
            for(int q=0; q < listSet.Count; q++)
            {
                setName.Add(listSet[q].SettingsName);
            }

            comboBox1.DataSource = setName;
        }

        //Margo
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataGridViewReaders.DataSource = null;
            dataGridViewReaders.Rows.Clear();

            int settingsIndex = comboBox1.SelectedIndex;
            int settingsID = listSet[settingsIndex].SettingsID;
            SelectedSetting = settingsID;

            SettingsMain sm = new SettingsMain();
            sm.SettingsID = settingsID;
            sm.SettingsName = listSet[settingsIndex].SettingsName;
            sm.SettingsSelect = listSet[settingsIndex].SettingsSelect;
            sm.StoreID = listSet[settingsIndex].StoreID;

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

            for (int i = 0; i < sm.Readers.Count; i++)
            {
                for(int y = 0; y < sm.Readers[i].antennas.Count; y++)
                {
                    dataGridViewReaders.Rows.Add(sm.Readers[i].IPaddress, sm.Readers[i].antennas[y].AntennaNumber, sm.Readers[i].antennas[y].TxPower, sm.Readers[i].antennas[y].RxPower);
                }
                
            }


        }

        //Margo
        private void btnAdd_Click(object sender, EventArgs e)
        {
            label6.Visible = false;
            if (dataGridViewReaders.Rows.Count != 0)
            {
                LTS.Settings old = DAT.DataAccess.GetSettings().Where(i => i.StoreID == SelectedStore && i.SettingsSelect == true).FirstOrDefault();
                bool oldchanged;
                if (old != null)
                {
                    old.SettingsSelect = false;
                    oldchanged = DAT.DataAccess.UpdateSettings(old);
                }
                else
                {
                    oldchanged = true;
                }
                
                
                if (oldchanged)
                {
                    LTS.Settings newSelect = DAT.DataAccess.GetSettings().Where(i => i.SettingsID == SelectedSetting).FirstOrDefault();
                    newSelect.SettingsSelect = true;
                    bool newchanged = DAT.DataAccess.UpdateSettings(newSelect);
                    if (newchanged)
                    {
                        MessageBox.Show("Setting Selected Successfully!");
                        ((Main)this.Parent.Parent).ChangeView<Settings>();

                    }
                    else
                    {
                        MessageBox.Show("Sorry, something went wrong, the setting was not selected!");
                        ((Main)this.Parent.Parent).ChangeView<Settings>();
                    }
                }
                else
                {
                    MessageBox.Show("Sorry, something went wrong, the setting was not selected!");
                    ((Main)this.Parent.Parent).ChangeView<Settings>();
                }
            }
            else
            {
                label6.Visible = true;
            }
        }
    }
}
