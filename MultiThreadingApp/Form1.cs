using System;
using System.IO;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
//using System.Drawing;
using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace MultiThreadingApp
{
    public partial class Form1 : Form
    {
        public DataTable dt = Csv_to_table(@"C:\Users\RobTad\Documents\KoU\YAZILIMLAB-1\YL-PROJECT-2\sample.csv");
        public DataTable dtNew;
        //public DataTable dtDisplay =  new DataTable();
        int threadNum;
        int maxNum = 1000;
        int column, column2, inequality, benchmarkCol, display_column;
        string benchmarkVal;
        float similarity_percentage;
        DataTable dtDisplay = new DataTable();

        public static DataTable Csv_to_table(string CSVFilePathName)
        {
            string[] Lines = File.ReadAllLines(CSVFilePathName);
            string[] Fields;
            Fields = Lines[0].Split(new char[] { ',' });
            int Cols = Fields.GetLength(0);
            DataTable dt = new DataTable();
            //1st row must be column names; force lower case to ensure matching later on.
            for (int i = 0; i < Cols; i++)
                dt.Columns.Add(Fields[i].ToLower(), typeof(string));
            DataRow Row;
            for (int i = 1; i < Lines.GetLength(0); i++)
            {
                Fields = Lines[i].Split(new char[] { ',' });
                Row = dt.NewRow();
                for (int f = 0; f < Cols; f++)
                    Row[f] = Fields[f];
                dt.Rows.Add(Row);
            }
            return dt;
        }
        
        public Form1()
        {
            InitializeComponent();
            
            //dataGridView1.DataSource = dt;
            CheckForIllegalCrossThreadCalls = false;
            /*
            dataGridView2.Columns.Add("KAYIT 1", "KAYIT 1");
            dataGridView2.Columns.Add("KAYIT 2", "KAYIT 2");
            dataGridView2.Columns.Add("BENZERLİK ORANI", "BENZERLİK ORANI");
            */
            dtDisplay.Columns.Add("KAYIT 1", typeof(String));
            dtDisplay.Columns.Add("KAYIT 2", typeof(String));
            dtDisplay.Columns.Add("BENZERLİK ORANI", typeof(String));
        }

        public HashSet<string> splitter(int i,int column, DataTable dt)
        {
            string target;
            target = (string)dt.Rows[i][column];
            string[] splitted_target;
            splitted_target = target.Split(' ');
            var target_set = new HashSet<string>(splitted_target);
            
            return target_set;
        }

        public int ListLen(int i, int column, DataTable dt)
        {
            string target;
            target = (string)dt.Rows[i][column];
            string[] splitted_target;
            splitted_target = target.Split(' ');
            return splitted_target.Length;
        }
       
        public void check_similarity(int column, int column2, int inequality_flag, float similarity_percentage, int benchmarkCol, string benchmarkVal, int display_column)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            // dtNew = dt.Clone();//datatable for the outputs of similarity check
            //add columns for dtDisplay
            // dtNew.Clear();
            //for dataGridView2
            int num = int.Parse(Thread.CurrentThread.Name);
            /*
            DataTable dtDisplay =  new DataTable();

            dtDisplay.Columns.Add("KAYIT 1", typeof(String));
            dtDisplay.Columns.Add("KAYIT 2", typeof(String));
            dtDisplay.Columns.Add("BENZERLİK ORANI", typeof(String));
            */

            //for dataGridView3
            DataTable dt3 = new DataTable();

            int flag = 0;//to check if benchmark column is compared
                
            
                for (int i = num * (dt.Rows.Count / threadNum); i < (num + 1) * (200 / threadNum); i++)
                {
                    //column designates the column to be checked for similarity(product or issue)

                    var set1 = splitter(i, column, dt);
                    int start_index = i + 1;

                    if (flag == 1)
                    {
                        break;
                    }
                    if(benchmarkCol != -1) 
                    {
                        if (!(dt.Rows[i][benchmarkCol].ToString().Equals(benchmarkVal)))
                        {
                            continue;
                        }
                        else
                        {
                            flag = 1;
                            start_index = 0;
                        }

                    }
                    for (int j = start_index; j < dt.Rows.Count; j++)
                    {
                        if(i==j)
                        {
                            continue;
                        }
                        var set2 = splitter(j, column, dt);
                        //check intersection between set1 and set2
                        HashSet<string> intersection = new HashSet<string>(set1);
                        intersection.IntersectWith(set2);
                        //calculate percentage depending on intersection
                        if (intersection.Count > 0)
                        {
                            float len1_n_len2 = intersection.Count();
                            
                            float len1 = ListLen(i, column, dt);
                            float len2 = ListLen(j, column, dt);
                            float longer = len1 >= len2 ? len1 : len2;
                            float percentage = (len1_n_len2 / longer) * 100;
                            percentage = (float)Math.Round(percentage, 1);
                        if (inequality_flag == 0)
                        {
                            if (percentage >= similarity_percentage)
                            {
                                string percent = percentage + "%";

                                if (column2 != -1)//for the same selected column
                                {

                                    if (dt.Rows[i][column2].ToString().Equals(dt.Rows[j][column2].ToString()))
                                    {
                                        lock (this)
                                        {
                                            dtDisplay.Rows.Add(dt.Rows[i][column], dt.Rows[j][column], percent);

                                            dtNew.ImportRow(dt.Rows[i]);
                                            dtNew.ImportRow(dt.Rows[j]);
                                        }
                                    }
                                }
                                else
                                {
                                    lock (this)
                                    {
                                        dtDisplay.Rows.Add(dt.Rows[i][column], dt.Rows[j][column], percent);
                                    }
                                    lock (this)
                                    {
                                        dtNew.ImportRow(dt.Rows[i]);
                                        dtNew.ImportRow(dt.Rows[j]);
                                    }
                                }
                                //dataGridView1.DataSource = dtNew;
                            }

                        }
                        else if(inequality_flag == 1)
                        {
                            if (percentage <= similarity_percentage)
                            {
                                string percent = percentage + "%";

                                if (column2 != -1)//for the same selected column
                                {

                                    if (dt.Rows[i][column2].ToString().Equals(dt.Rows[j][column2].ToString()))
                                    {
                                        lock (this)
                                        {
                                            dtDisplay.Rows.Add(dt.Rows[i][column], dt.Rows[j][column], percent);

                                            dtNew.ImportRow(dt.Rows[i]);
                                            dtNew.ImportRow(dt.Rows[j]);
                                        }
                                    }
                                }
                                else
                                {
                                    lock (this)
                                    {
                                        dtDisplay.Rows.Add(dt.Rows[i][column], dt.Rows[j][column], percent);
                                    }
                                    lock (this)
                                    {
                                        dtNew.ImportRow(dt.Rows[i]);
                                        dtNew.ImportRow(dt.Rows[j]);
                                    }
                                }
                                //dataGridView1.DataSource = dtNew;
                            }
                        }
                        /*
                        else
                        {
                            if (percentage == similarity_percentage)
                            {
                                string percent = percentage + "%";

                                if (column2 != -1)//for the same selected column
                                {

                                    if (dt.Rows[i][column2].ToString().Equals(dt.Rows[j][column2].ToString()))
                                    {
                                        lock (this)
                                        {
                                            dtDisplay.Rows.Add(dt.Rows[i][column], dt.Rows[j][column], percent);

                                            dtNew.ImportRow(dt.Rows[i]);
                                            dtNew.ImportRow(dt.Rows[j]);
                                        }
                                    }
                                }
                                else
                                {
                                    lock (this)
                                    {
                                        dtDisplay.Rows.Add(dt.Rows[i][column], dt.Rows[j][column], percent);
                                    }
                                    lock (this)
                                    {
                                        dtNew.ImportRow(dt.Rows[i]);
                                        dtNew.ImportRow(dt.Rows[j]);
                                    }
                                }
                                //dataGridView1.DataSource = dtNew;
                            }
                        }
                         */   
                        }
                    }
                }

            //dataGridView1.DataSource = dtNew;
            
            //dataGridView2.DataSource = dtDisplay;
                        
            //add column and distinct rows from dtNew to datagridview3 for display_column
            /*
            if(display_column != -1)
            {
                //dt3.Columns.Add(dt.Columns[display_column].ToString(), typeof(String));
                //dt.DefaultView.ToTable(true, new String[] { "columnName" });

                DataView view = new DataView(dtNew);
                DataTable dtDistinct = view.ToTable(true, dtNew.Columns[display_column].ToString());
                dataGridView3.DataSource = dtDistinct;

                //dt3.Rows.Add(dtNew.Rows[i][display_column]); //use select and distinct

                //dtNew.ImportRow(dt.Rows[0]);
            }
            */
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            //Console.WriteLine("All time {0}. Thread - {1}", Thread.CurrentThread.Name, ts);
            tBox.AppendText(Environment.NewLine);
            tBox.AppendText(Thread.CurrentThread.Name + ". Thread - " + ts.TotalSeconds);
            tBox.AppendText(Environment.NewLine);
        }

        //DataTable containing all the csv records (dt)

        private void button1_Click(object sender, EventArgs e)
        {
            dataGridView2.DataSource = null;
            dtNew = dt.Clone();//datatable for the outputs of similarity check
                               //add columns for dtDisplay
            dtNew.Clear();
            dtDisplay.Clear();
            
            tBox.Clear();

            //getting column to be checked from a user (using combobox's dropdown list)
            //float similarity_percentage = (float)Convert.ToDouble(textBox2.Text);
            //float similarity_percentage2 = (float)Convert.ToDouble(textBox1.Text);

            if (!float.TryParse(textBox2.Text, out similarity_percentage))
            {
                //MessageBox.Show("Please enter a number for similarity percentage");
                //return;
            }
           
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                similarity_percentage = -1;
            }
            
            /*
            if (comboBox1.SelectedItem == null || comboBox2.SelectedItem == null)
            {
                MessageBox.Show("Wrong Column or Inequality Operator!\nTry Again!");
                return;
            }
            */

            //float similarity_percentage = float.Parse(textBox2.Text);
            column = comboBox1.SelectedIndex;
            inequality = comboBox2.SelectedIndex;
            column2 = comboBox4.SelectedIndex;

            benchmarkCol = comboBox3.SelectedIndex;
            benchmarkVal = textBox1.Text;

            threadNum = int.Parse(textBox3.Text);

            display_column = comboBox5.SelectedIndex;

            myThreads();
            //backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            myThreads();
        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int index = e.RowIndex;
            
            DataTable dtTemp;
            dtTemp = dtNew.Clone();
            dtTemp.ImportRow(dtNew.Rows[2*index]);
            dtTemp.ImportRow(dtNew.Rows[2*index+1]);

            dataGridView1.DataSource = dtTemp;
            //dataGridView1.DataSource = dtNew;

            ///
            //tBox.AppendText("DATA GRID VIEW CELL CLICK. INDEX = " + index + " and " + (index+1));
        }

        private void callCalculator()
        {
            check_similarity(column, column2, inequality, similarity_percentage, benchmarkCol, benchmarkVal, display_column);
        }

        private void myThreads()
        {
            Thread[] threads = new Thread[threadNum];

            // CurrentThread gets you the current
            // executing thread
            //Thread.CurrentThread.Name = "main";

            // Create 15 threads that will call for 
            // IssueWithdraw to execute
            for (int i = 0; i < threadNum; i++)
            {
                // You can only point at methods
                // without arguments and that return 
                // nothing
                Thread t = new Thread(new
                    ThreadStart(callCalculator));
                t.Name = i.ToString();
                threads[i] = t;
            }

            // Have threads try to execute
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < threadNum; i++)
            {
                // Check if thread has started
                //Console.WriteLine("Thread {0} Alive : {1}",
                //    threads[i].Name, threads[i].IsAlive);
                tBox.AppendText("Thread " + threads[i].Name + " Alive : " + threads[i].IsAlive);
                tBox.AppendText(Environment.NewLine);
                // Start thread
                threads[i].Start();

                // Check if thread has started
                //Console.WriteLine("Thread {0} Alive : {1}",
                //    threads[i].Name, threads[i].IsAlive);
                tBox.AppendText("Thread " + threads[i].Name + " Alive : " + threads[i].IsAlive);
                tBox.AppendText(Environment.NewLine);
            }
            for (int i = 0; i < threadNum; i++)
            {
                threads[i].Join();
            }
            stopWatch.Stop();
            dataGridView2.DataSource = dtDisplay;
            TimeSpan ts = stopWatch.Elapsed;
            //Console.WriteLine("All time {0}. Thread - {1}", Thread.CurrentThread.Name, ts);
            tBox.AppendText(Environment.NewLine);
            tBox.AppendText("Total time " + Thread.CurrentThread.Name + " Threads - " + ts);
            tBox.AppendText(Environment.NewLine);

            if (display_column != -1)
            {
                DataView view = new DataView(dtNew);
                DataTable dtDistinct = view.ToTable(true, dtNew.Columns[display_column].ToString());
                dataGridView3.DataSource = dtDistinct;
            }
        }
    }
}
