using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiThreadingApp
{
    public partial class Form1 : Form
    {
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
            //var dt = ConvertCSVtoDataTable(@"C:\Users\RobTad\Documents\KoU\YAZLAB-1\P2\sample.csv");
            var dt = Csv_to_table(@"C:\Users\RobTad\Documents\KoU\YAZILIMLAB-1\YL-PROJECT-2\small_sample.csv");
            //dataGridView1.DataSource = dt;
            
        }


        public DataTable dt = Csv_to_table(@"C:\Users\RobTad\Documents\KoU\YAZILIMLAB-1\YL-PROJECT-2\small_sample.csv");
        public DataTable dtNew;
        //public DataTable dtDisplay =  new DataTable();
        

        public HashSet<string> splitter(int i,int column, DataTable dt)
        {
            string target;
            target = (string)dt.Rows[i][column];
            string[] splitted_target;
            splitted_target = target.Split(' ');
            var target_set = new HashSet<string>(splitted_target);
            //tBox.AppendText(string.Join(", ", splitted_target));
            //tBox.AppendText(string.Join(", ", target_set));
            
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
        
       

        public void check_similarity(int column, int inequality_flag, float similarity_percentage)
        {
            dtNew = dt.Clone();//datatable for the outputs of similarity check
                               //add columns for dtDisplay
            dtNew.Clear();
            DataTable dtDisplay =  new DataTable();

            dtDisplay.Columns.Add("KAYIT 1", typeof(String));
            dtDisplay.Columns.Add("KAYIT 2", typeof(String));
            dtDisplay.Columns.Add("BENZERLİK ORANI", typeof(String));

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //column designates the column to be checked for similarity(product or issue)
                
                var set1 = splitter(i, column, dt);

                for (int j = i+1; j < dt.Rows.Count; j++)
                {
                    var set2 = splitter(j, column, dt);
                    //check intersection between set1 and set2
                    HashSet<string> intersection = new HashSet<string>(set1);
                    intersection.IntersectWith(set2);
                    //calculate percentage depending on intersection
                    if (intersection.Count>0)
                    {
                        float len1_n_len2 = intersection.Count();
                        tBox.AppendText(string.Join(", ", intersection));
                        tBox.AppendText(Environment.NewLine);
                        //tBox.AppendText("intersection len = " + len1_n_len2);

                        float len1 = ListLen(i, column, dt);
                        float len2 = ListLen(j, column, dt);
                        float longer = len1 >= len2 ? len1 : len2;
                        float percentage = (len1_n_len2 / longer)*100;
                        percentage = (float)Math.Round(percentage, 1);

                        if (percentage >= similarity_percentage)
                        {
                            
                            tBox.AppendText("intersection len = " + len1_n_len2 + "-> " + len1 + ", " + len2);
                            tBox.AppendText(Environment.NewLine);
                            tBox.AppendText("intersection percentage = " + percentage + "%");
                            tBox.AppendText(Environment.NewLine);
                            tBox.AppendText("(i,j) = (" + i + "," + j + ")");
                            tBox.AppendText(Environment.NewLine);
                            string percent = percentage + "%";

                            //MyTable.Rows.Add(MyTable2.Rows[0]["Id"], MyTable2.Rows[0]["Name"]);
                            dtDisplay.Rows.Add(dt.Rows[i][column], dt.Rows[j][column], percentage + "%");

                            //write the following at the end of outer for loop
                           
                            dtNew.ImportRow(dt.Rows[i]);
                            dtNew.ImportRow(dt.Rows[j]);

                            //dataGridView1.DataSource = dtNew;
                             
                        }



                    }


                }


            }
            //dataGridView1.DataSource = dtNew;
            dataGridView2.DataSource = dtDisplay;


        }
        //DataTable containing all the csv records (dt)

        private void button1_Click(object sender, EventArgs e)
        {
            tBox.Clear();
            
            //dataGridView2.DataSource = dtNew;
            //getting column to be checked from a user (using combobox's dropdown list)
            if (!float.TryParse(textBox2.Text, out float similarity_percentage))
            {
                MessageBox.Show("Please enter a number for similarity percentage");
                return;
            }
            if (comboBox1.SelectedItem == null || comboBox2.SelectedItem == null)
            {
                MessageBox.Show("Wrong Column or Inequality Operator!\nTry Again!");
                return;
            }
            //float similarity_percentage = float.Parse(textBox2.Text);
            int column = comboBox1.SelectedIndex;
            int inequality = comboBox2.SelectedIndex;

            check_similarity(column, inequality, similarity_percentage);

             
        }

       

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            int index = e.RowIndex;
            //
            DataTable dtTemp;
            dtTemp = dtNew.Clone();
            dtTemp.ImportRow(dtNew.Rows[2*index]);
            dtTemp.ImportRow(dtNew.Rows[2*index+1]);

            dataGridView1.DataSource = dtTemp;
            //dataGridView1.DataSource = dtNew;


            ///
            //tBox.AppendText("DATA GRID VIEW CELL CLICK. INDEX = " + index + " and " + (index+1));

        }
    }
}
