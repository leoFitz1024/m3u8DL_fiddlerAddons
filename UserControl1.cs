using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace FiddlerExtension
{
    public partial class UserControl1 : UserControl
    {
        private string filter = "";
        private string[] filterArray;
        private string m3u8DLPath = "";
        private string savePath = "";
        private string otherArg = "";
        private bool isBeginFilter = false;
        public UserControl1()
        {
            InitializeComponent();
        }
        public string Filter
        {
            get
            {
                return filter;
            }

            set
            {
                filter = value;
            }
        }
        public string SavePath
        {
            get
            {
                return savePath;
            }

            set
            {
                savePath = value;
            }
        }
        public string[] FilterArray
        {
            get
            {
                return filterArray;
            }

            set
            {
                filterArray = value;
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (!isBeginFilter) // beginFilter
            {
                isBeginFilter = !isBeginFilter;
                Filter = this.textBox2.Text;
                // 拆分成字符串数组
                FilterArray = Filter.Split('@');
                this.button1.Text = "停止捕获";
            }
            else
            {
                isBeginFilter = !isBeginFilter;
                Filter = "";
                this.button1.Text = "开始捕获";
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            // 把当前listview中的数据保存到output.txt中
            if (this.listView1.Items.Count == 0)
            {
                MessageBox.Show("列表为空!");
            }
            else
            {
                List<string> list = new List<string>();
                foreach (ListViewItem item in this.listView1.Items)
                {
                    string temp = item.SubItems[4].Text + "," + item.SubItems[2].Text + "," + item.SubItems[3].Text; // 取出url的数据进行保存
                    list.Add(temp);

                }
                Thread thexp = new Thread(() => export(list)) { IsBackground = true };
                thexp.Start();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // 把当前listview中的数据保存到output.txt中
            if (this.listView1.Items.Count == 0)
            {
                MessageBox.Show("列表为空!");
            }
            else
            {
                List<string> urlList = new List<string>();
                foreach (ListViewItem item in this.listView1.Items)
                {
                    string urlStr = item.SubItems[2].Text; // 取出url的数据进行下载
                    urlList.Add(urlStr);

                }
                Thread thdownLoad = new Thread(() => download(urlList)) { IsBackground = true };
                thdownLoad.Start();
            }
        }

        private void export(List<string> list)
        {
            string path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Fiddler" + Guid.NewGuid().ToString() + ".csv";
            StringBuilder sb = new StringBuilder();
            foreach (string urlString in list)
            {
                sb.AppendLine(urlString);
            }
            System.IO.File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            MessageBox.Show("已保存完毕，路径：" + path);
        }

        private void download(List<string> list)
        {
            StringBuilder sb = new StringBuilder();
            m3u8DLPath = this.textBox1.Text;
            savePath = this.textBox3.Text;
            otherArg = this.textBox4.Text;
            if (m3u8DLPath == "") {
                MessageBox.Show("请先选择m3u8DL路径！");
                return;
            }
            if (savePath == "")
            {
                MessageBox.Show("请选择文件保存路径！");
                return;
            }
            int index = 1;
            foreach (string urlString in list)
            {
                toolDown(urlString, m3u8DLPath, savePath, otherArg,index);
                index++;
            }
            
        }

        private void toolDown(string url,string m3u8DLPath,string savePath,string otherArg,int index) {
            try
            {
                string Arguments = url + " --workDir \""+savePath+ "\" " +otherArg;
                if (this.checkBox1.Checked)
                {
                    Arguments = Arguments + " --saveName " + index;
                }
                Process pro = Process.Start(m3u8DLPath, Arguments);//打开程序B
                pro.WaitForExit();
                int Result = pro.ExitCode;//程序B退出回传值

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string m3u8DL_path = SelectfilePath();
            this.textBox1.Text = m3u8DL_path;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string savePath = SelectDirPath();
            this.textBox3.Text = savePath;
        }




        // 选择文件：
        private string SelectfilePath()
        {
            string path = string.Empty;
            var openFileDialog = new OpenFileDialog()
            {
                Filter = "Files (*.exe*)|*.exe*"//如果需要筛选txt文件（"Files (*.txt)|*.txt"）
            };
            var result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                path = openFileDialog.FileName;
            }
            return path;
        }
        // 选择路径
        private string SelectDirPath()
        {
            string path = string.Empty;
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                path = fbd.SelectedPath;
            }
            return path;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.listView1.Items.Clear();
        }
    }
}
