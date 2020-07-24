using Fiddler;
using System;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;

namespace FiddlerExtension
{
    public class Class1 : IAutoTamper
    {
        private TabPage tabPage; //创建插件的选项卡页
        private UserControl1 myCtrl; //MyControl自定义控件
        private string filePath;
        //private FileStream fs;
        //private StreamWriter sw;
        private StringBuilder contentSBTrue;
        private StringBuilder contentSBFalse;
        private Boolean isUseSBFlag;
        private int currentCount;
        public const int WRITE_FILE_THRESHOLD = 1;

        public Class1()
        {
            //构造函数中实例化对象
            this.tabPage = new TabPage("视频下载工具");//选项卡的名字为Test
            this.myCtrl = new UserControl1();
            filePath = this.myCtrl.SavePath + "output" + ".csv";
            //StringBuilder sb = new StringBuilder();
            //fs = new FileStream(path, FileMode.Create);
            //sw = new StreamWriter(fs);
            contentSBTrue = new StringBuilder();
            contentSBFalse = new StringBuilder();
            isUseSBFlag = true;
            currentCount = 0;
        }

        public void OnLoad()
        {
            //将用户控件添加到选项卡中
            this.tabPage.Controls.Add(this.myCtrl);
            //为选项卡添加icon图标，这里使用Fiddler 自带的
            this.tabPage.ImageIndex = (int)Fiddler.SessionIcons.Timeline;
            //将tabTage选项卡添加到Fidder UI的Tab 页集合中
            FiddlerApplication.UI.tabsViews.TabPages.Add(this.tabPage);
        }

        public void OnBeforeUnload()
        {

        }

        // Called before the user can edit a request using the Fiddler Inspectors
        public void AutoTamperRequestBefore(Session oSession)
        {
            // this.myCtrl.textBox1.Text += oSession.fullUrl + "\r\n";
            // 过滤满足条件的host
            if (this.myCtrl.Filter != "")
            {
                string url = oSession.fullUrl;
                foreach (string regexString in this.myCtrl.FilterArray)
                {
                    Regex regex = new Regex(regexString);
                    if (regex.IsMatch(url))
                    {
                        ListViewItem item = new ListViewItem(this.myCtrl.listView1.Items.Count + "");
                        item.SubItems.Add(oSession.host); // 加入host
                        item.SubItems.Add(oSession.fullUrl); // 加入fullUrl
                        item.SubItems.Add(DateTime.Now.ToString()); // 加入time
                        item.SubItems.Add(currentCount.ToString()); // 加入packetNo
                                                                    //item.SubItems.Add(""); // 加入request参数
                                                                    //item.SubItems.Add(""); // 加入response参数
                        this.myCtrl.listView1.Items.Add(item);
                        string temp = oSession.id + "," + oSession.fullUrl + "," + DateTime.Now.ToString(); // 取出url的数据进行保存
                        currentCount++;
                        //sw.WriteLine(temp);
                        //sw.Flush();
                        if (isUseSBFlag) // 写出到contentSBTrue中
                        {
                            AppendSB(contentSBTrue, temp);
                        }
                        else  // 写出到contentSBFalse中
                        {
                            AppendSB(contentSBFalse, temp);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 写入到StringBuilder中，满足一定条件后写出到文件中
        /// </summary>
        /// <param name="contentSB">StringBuilder</param>
        /// <param name="temp">追加到contentSB的string</param>
        /// <returns></returns>
        private void AppendSB(StringBuilder contentSB, string temp)
        {
            contentSB.AppendLine(temp);
            if (currentCount >= WRITE_FILE_THRESHOLD)
            {
                isUseSBFlag = !isUseSBFlag;
                // 线程统一写出到文件中
                currentCount = 0;
                Thread thexp = new Thread(() => WriteToFile(ref contentSB, this.filePath)) { IsBackground = true };
                thexp.Start();
            }
        }


        /// <summary>
        /// 写入记事本
        /// </summary>
        /// <param name="contentSB">写出的内容</param>
        /// <param name="filePath">文件路径（含文件名）</param>
        /// <returns></returns>
        private static void WriteToFile(ref StringBuilder contentSB, string filePath)
        {
            try
            {
                File.AppendAllText(filePath, contentSB.ToString(), Encoding.UTF8);
                //MessageBox.Show("已保存完毕，路径：" + filePath);
                contentSB.Remove(0, contentSB.Length); // 清空
            }
            catch
            {
                //MessageBox.Show("遇到问题，未保存到文件中");
            }
        }

        // Called after the user has had the chance to edit the request using the Fiddler Inspectors, but before the request is sent
        public void AutoTamperRequestAfter(Session oSession) { }


        // Called before the user can edit a response using the Fiddler Inspectors, unless streaming.
        public void AutoTamperResponseBefore(Session oSession) { }


        // Called after the user edited a response using the Fiddler Inspectors.  Not called when streaming.
        public void AutoTamperResponseAfter(Session oSession) { }


        // Called Fiddler returns a self-generated HTTP error (for instance DNS lookup failed, etc)
        public void OnBeforeReturningError(Session oSession) { }
    }
}
