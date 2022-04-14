using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ApiTester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnSign_Click(object sender, EventArgs e)
        {

             txtOut.Text= txtInput.Text+ GetSign(txtInput.Text);
          
        }
        public  string GetSign(string url)
        {
            string result;
            string str = url.Substring(url.IndexOf("?", 4) + 1);
            List<string> list = str.Split('&').ToList();
            list.Sort();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string str1 in list)
            {
                stringBuilder.Append((stringBuilder.Length > 0 ? "&" : string.Empty));
                stringBuilder.Append(str1);
            }
            stringBuilder.Append(txtAppSecret.Text);
            result = ToMD5(stringBuilder.ToString()).ToLower();
            return "&sign=" + result;
        }
        public string ToMD5(string input)
        {
            MD5 md5 = MD5.Create();
           var comBytes= md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            var output= "";
            foreach (var item in comBytes)
            {
                output += item.ToString("x2");
            }
            return output;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void btnEq_Click(object sender, EventArgs e)
        {
            dataGridView1.AutoGenerateColumns = false;
            dataGridView2.AutoGenerateColumns = false;

            var ls1 = GetPar(txtUri1.Text);
            var ls2= GetPar(txtUri2.Text);
            foreach (var item in ls1)
            {
                var data = ls2.FirstOrDefault(x => x.Key == item.Key);
                if (data != null)
                {
                    if (data.Value!=item.Value)
                    {
                        item.Different = true;
                        data.Different = true;
                    }
                }
                else
                {
                    item.Different = true;
                }
            }
            dataGridView1.DataSource= ls1;
            dataGridView2.DataSource = ls2;
            foreach (DataGridViewRow item in dataGridView1.Rows)
            {
                var value= item.DataBoundItem as ParModel;
                if (value.Different)
                {
                    item.DefaultCellStyle.BackColor = Color.Red;
                }
            }
            foreach (DataGridViewRow item in dataGridView2.Rows)
            {
                var value = item.DataBoundItem as ParModel;
                if (value.Different)
                {
                    item.DefaultCellStyle.BackColor = Color.Red;
                }
            }
        }

        private List<ParModel> GetPar(string uri)
        {
            var u = new Uri(uri);
            var strs = u.Query.Replace("?", "").Split('&');
            List<ParModel> ls = new List<ParModel>();
            foreach (var item in strs.OrderBy(x=>x))
            { 
                var v = item.Split('=');
                if (v[0]!="sign"&& v[0] != "ts")
                {
                    ls.Add(new ParModel(v[0], v[1]));
                }
            }
            return ls;
        }

    }
    public class ParModel
    {
        public ParModel(string key,string value)
        {
            Key = key;
            Value = value;
        }
        public string Key { get; set; }
        public string Value { get; set; }
        public bool Different { get; set; } = false;
    }
}
