using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Collections.Specialized;
using System.Configuration;
using Wesson.Win.Dev.GenStoredProcedures;

namespace Wesson.Win.Dev.StoreProc
{
    public partial class Form1 : Form
    {
        protected SQLDMOHelper dmoMain = new SQLDMOHelper();

        public Form1()
        {
            try
            {
                InitializeComponent();

                object[] objServers = (object[])dmoMain.RegisteredServers;
                selServers.Items.Add("local");

                // Default connection details, if provided

                NameValueCollection settingsAppSettings = (NameValueCollection)ConfigurationSettings.AppSettings;

                dmoMain.UserName = "sa";
                dmoMain.Password = "Starwolf";
               
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "Error");
            }
        }


        public void Process()
        {
            // First ensure connection details are valid
            if (dmoMain.ServerName == "" || dmoMain.UserName == "")
            {
                MessageBox.Show("Please enter in valid connection details.", this.Text);
            }
            else
            {
                this.Cursor = Cursors.WaitCursor;
                //statbarpnlMain.Text = "Connecting to SQL Server...";

                //Valid connection details				
                tvwServerExplorer.Nodes.Clear();

                // List Databases
                try
                {
                    dmoMain.Connect();
                    Array aDatabases = (Array)dmoMain.Databases;
                    dmoMain.DisConnect();

                    for (int i = 0; i < aDatabases.Length; i++)
                    {
                        TreeNode treenodeDatabase = new TreeNode(aDatabases.GetValue(i).ToString(), 0, 0);
                        treenodeDatabase.Nodes.Add("");
                        tvwServerExplorer.Nodes.Add(treenodeDatabase);
                    }

                    this.Cursor = Cursors.Default;
                    //statbarpnlMain.Text = "Connectiong successful, databases listed...";
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message, "Error");
                    this.Cursor = Cursors.Default;
                    //statbarpnlMain.Text = "Connectiong un-successful...";
                    MessageBox.Show("Connection to database failed. Please check your Server Name, User and Password.", this.Text);
                }
            }


        }

        private void cmdConnect_Click(object sender, EventArgs e)
        {
            Process();
        }

        private void tvwServerExplorer_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            // List all Tables for selected Database

            if (e.Node.ImageIndex == 0)
            {
                this.Cursor = Cursors.WaitCursor;
                //statbarpnlMain.Text = "Listing Tables...";

                // Set database to get tables from						

                dmoMain.Database = e.Node.Text;

                // Clear dummy node

                e.Node.Nodes.Clear();

                try
                {
                    // List Tables
                    dmoMain.Connect();
                    Array aTables = (Array)dmoMain.Tables;
                    dmoMain.DisConnect();

                    for (int i = 0; i < aTables.Length; i++)
                    {
                        TreeNode treenodeTable = new TreeNode(aTables.GetValue(i).ToString(), 1, 1);
                        TreeNode treenodeTableALL = new TreeNode("ALL", 2, 2);
                        TreeNode treenodeTableSELECT = new TreeNode("SELECT", 2, 2);
                        TreeNode treenodeTableSELECTALL = new TreeNode("SELECTALL", 2, 2);
                        TreeNode treenodeTableSELECTDISTINCT = new TreeNode("SELECTDISTINCT", 2, 2);
                        TreeNode treenodeTableSELECTMULTI = new TreeNode("SELECTMULTI", 2, 2);
                        TreeNode treenodeTableSELECT2KEYS = new TreeNode("SELECT2KEYS", 2, 2);
                        TreeNode treenodeTableSELECT3KEYS = new TreeNode("SELECT3KEYS", 2, 2);
                        TreeNode treenodeTableSELECT4KEYS = new TreeNode("SELECT4KEYS", 2, 2);
                        TreeNode treenodeTableDELETE = new TreeNode("DELETE", 2, 2);
                        TreeNode treenodeTableINSERT = new TreeNode("INSERT", 2, 2);
                        TreeNode treenodeTableUPDATE = new TreeNode("UPDATE", 2, 2);
                        TreeNode treenodeTableIU = new TreeNode("IU", 2, 2);

                        treenodeTable.Nodes.Add(treenodeTableALL);
                        treenodeTable.Nodes.Add(treenodeTableSELECT);
                        treenodeTable.Nodes.Add(treenodeTableSELECTALL);
                        treenodeTable.Nodes.Add(treenodeTableSELECTDISTINCT);
                        treenodeTable.Nodes.Add(treenodeTableSELECTMULTI);
                        treenodeTable.Nodes.Add(treenodeTableSELECT2KEYS);
                        treenodeTable.Nodes.Add(treenodeTableSELECT3KEYS);
                        treenodeTable.Nodes.Add(treenodeTableSELECT4KEYS);
                        treenodeTable.Nodes.Add(treenodeTableDELETE);
                        treenodeTable.Nodes.Add(treenodeTableINSERT);
                        treenodeTable.Nodes.Add(treenodeTableUPDATE);
                        treenodeTable.Nodes.Add(treenodeTableIU);

                        e.Node.Nodes.Add(treenodeTable);
                    }

                    this.Cursor = Cursors.Default;
                    //statbarpnlMain.Text = "Tables listed...";
                }
                catch
                {
                    this.Cursor = Cursors.Default;
                    //statbarpnlMain.Text = "Problem listing Tables...";

                    MessageBox.Show("Problem connecting to database. Cannot list tables, reconnect advised.", this.Text);
                }
            }
        }

        private void tvwServerExplorer_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode tnodeSelected = (TreeNode)e.Node;

            if (tnodeSelected.ImageIndex == 2)
            {
                this.Cursor = Cursors.WaitCursor;
                //statbarpnlMain.Text = "Generating Stored Procedure, please wait...";
                // SP selected, generate SP
                TreeNode tnodeTable = (TreeNode)tnodeSelected.Parent;
                TreeNode tSelection = (TreeNode)tnodeSelected;
                TreeNode tDatabase = (TreeNode)tnodeTable.Parent;

                txtDatabaseName.Text = tDatabase.Text;

                dmoMain.Table = tnodeTable.Text;

                dmoMain.Connect();



                if (tSelection.Text == "ALL")
                {
                    txtGeneratedCode.Text = new StoredProcedure().GenerateSELECT(dmoMain.Fields, dmoMain.Table, txtDatabaseName.Text.ToString());
                    txtGeneratedCode.Text = txtGeneratedCode.Text + Environment.NewLine.ToString() + Environment.NewLine.ToString() + new StoredProcedure().GenerateSELECTALL(dmoMain.Fields, dmoMain.Table, txtDatabaseName.Text.ToString());
                    txtGeneratedCode.Text = txtGeneratedCode.Text + Environment.NewLine.ToString() + Environment.NewLine.ToString() + new StoredProcedure().GenerateSELECTDistinct(dmoMain.Fields, dmoMain.Table, txtDatabaseName.Text.ToString());
                    txtGeneratedCode.Text = txtGeneratedCode.Text + Environment.NewLine.ToString() + Environment.NewLine.ToString() + new StoredProcedure().GenerateSELECTINDIVIUALS(dmoMain.Fields, dmoMain.Table, txtDatabaseName.Text.ToString());
                    txtGeneratedCode.Text = txtGeneratedCode.Text + Environment.NewLine.ToString() + Environment.NewLine.ToString() + new StoredProcedure().GenerateSELECTPairIndividuals(dmoMain.Fields, dmoMain.Table, txtDatabaseName.Text.ToString());
                    txtGeneratedCode.Text = txtGeneratedCode.Text + Environment.NewLine.ToString() + Environment.NewLine.ToString() + new Using3Keys().GenerateSELECT3Keys(dmoMain.Fields, dmoMain.Table, txtDatabaseName.Text.ToString());
                    txtGeneratedCode.Text = txtGeneratedCode.Text + Environment.NewLine.ToString() + Environment.NewLine.ToString() + new Using4Keys().GenerateSELECT4Keys(dmoMain.Fields, dmoMain.Table, txtDatabaseName.Text.ToString());
                    txtGeneratedCode.Text = txtGeneratedCode.Text + Environment.NewLine.ToString() + Environment.NewLine.ToString() + new StoredProcedure().GenerateDELETE(dmoMain.Fields, dmoMain.Table, txtDatabaseName.Text.ToString());
                    txtGeneratedCode.Text = txtGeneratedCode.Text + Environment.NewLine.ToString() + Environment.NewLine.ToString() + new StoredProcedure().GenerateINSERT(dmoMain.Fields, dmoMain.Table, txtDatabaseName.Text.ToString());
                    txtGeneratedCode.Text = txtGeneratedCode.Text + Environment.NewLine.ToString() + Environment.NewLine.ToString() + new StoredProcedure().GenerateUPDATE(dmoMain.Fields, dmoMain.Table, txtDatabaseName.Text.ToString());
                    txtGeneratedCode.Text = txtGeneratedCode.Text + Environment.NewLine.ToString() + Environment.NewLine.ToString() + new StoredProcedure().GenerateIU(dmoMain.Fields, dmoMain.Table, txtDatabaseName.Text.ToString());
                } 
                if (tSelection.Text == "SELECT")
                {
                    txtGeneratedCode.Text = new StoredProcedure().GenerateSELECT(dmoMain.Fields, dmoMain.Table, txtDatabaseName.Text.ToString());
                }
                if (tSelection.Text == "SELECTALL")
                {
                    txtGeneratedCode.Text = new StoredProcedure().GenerateSELECTALL(dmoMain.Fields, dmoMain.Table, txtDatabaseName.Text.ToString());
                }
                if (tSelection.Text == "SELECTDISTINCT")
                {
                    txtGeneratedCode.Text = new StoredProcedure().GenerateSELECTDistinct(dmoMain.Fields, dmoMain.Table, txtDatabaseName.Text.ToString());
                }
                if (tSelection.Text == "SELECTMULTI")
                {
                    txtGeneratedCode.Text = new StoredProcedure().GenerateSELECTINDIVIUALS(dmoMain.Fields, dmoMain.Table, txtDatabaseName.Text.ToString());
                }
                if (tSelection.Text == "SELECT2KEYS")
                {
                    txtGeneratedCode.Text = new StoredProcedure().GenerateSELECTPairIndividuals(dmoMain.Fields, dmoMain.Table, txtDatabaseName.Text.ToString());
                }
                if (tSelection.Text == "SELECT3KEYS")
                {
                    txtGeneratedCode.Text = new Using3Keys().GenerateSELECT3Keys(dmoMain.Fields, dmoMain.Table, txtDatabaseName.Text.ToString());
                }
                if (tSelection.Text == "SELECT4KEYS")
                {
                    txtGeneratedCode.Text = new Using4Keys().GenerateSELECT4Keys(dmoMain.Fields, dmoMain.Table, txtDatabaseName.Text.ToString());
                }
                if (tSelection.Text == "DELETE")
                {
                    txtGeneratedCode.Text = new StoredProcedure().GenerateDELETE(dmoMain.Fields, dmoMain.Table, txtDatabaseName.Text.ToString());
                }
                if (tSelection.Text == "INSERT")
                {
                    txtGeneratedCode.Text = new StoredProcedure().GenerateINSERT(dmoMain.Fields, dmoMain.Table, txtDatabaseName.Text.ToString());
                }
                if (tSelection.Text == "UPDATE")
                {
                    txtGeneratedCode.Text = new StoredProcedure().GenerateUPDATE(dmoMain.Fields, dmoMain.Table, txtDatabaseName.Text.ToString());
                }
                if (tSelection.Text == "IU")
                {
                    txtGeneratedCode.Text = new StoredProcedure().GenerateIU(dmoMain.Fields, dmoMain.Table, txtDatabaseName.Text.ToString());
                }

                dmoMain.DisConnect();

                this.Cursor = Cursors.Default;
                //statbarpnlMain.Text = "Stored Procedure generated...";

                txtGeneratedCode.Focus();
                txtGeneratedCode.SelectAll();
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }




    }
}