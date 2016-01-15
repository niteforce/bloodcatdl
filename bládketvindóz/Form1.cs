using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;


namespace bládketvindóz
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        JsonTools j = new JsonTools();
        Stopwatch s = new Stopwatch();
        RootObject[] h;
        ListViewItem lvi;

        int oldalid = 1;

        List<string> idk = new List<string>();

        public void WhichMapsDoIHave(ref List<string> ídék)
        {
            List<string> folders = new List<string>();
            folders = Directory.GetDirectories(textBox1.Text + "\\Songs\\").ToList<string>();
            ídék = folders;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button3_Click(this, EventArgs.Empty);
            threadselected = 10;}

        private void ListFill(float minAr = 0, int p = 0)
        {

            for (int i = p; i < oldalid * 40; i++)
            {




                if (!idk.Contains(pagemaps[i].id))
                    if (pagemaps[i].maximumAr >= minAr)
                    {
                        {
                            lvi = new ListViewItem();
                            lvi.Text = pagemaps[i].id; //ID hozzáadása

                            ListViewItem.ListViewSubItem subItem = new ListViewItem.ListViewSubItem(lvi, "Title");      //Címek hozzáadása   
                            subItem.Text = pagemaps[i].title;
                            lvi.SubItems.Add(subItem);

                            ListViewItem.ListViewSubItem subItem2 = new ListViewItem.ListViewSubItem(lvi, "AR");


                            string arek = "";

                            arek += pagemaps[i].minimumAr + "  -  ";

                            arek += pagemaps[i].maximumAr;



                            subItem2.Text = arek;
                            lvi.SubItems.Add(subItem2);


                            ListViewItem.ListViewSubItem subItem3 = new ListViewItem.ListViewSubItem(lvi, "Length");

                            string hossz = "";
                            if (double.Parse(pagemaps[i].beatmaps[0].length) / 60 >= 1)
                                hossz = (double.Parse(pagemaps[i].beatmaps[0].length) / 60).ToString("##.##") + " mins";
                            else
                                hossz = pagemaps[i].beatmaps[0].length + " s ";


                            subItem3.Text = hossz;
                            lvi.SubItems.Add(subItem3);


                            listView1.Items.Add(lvi);

                        }

                    }

            }



        }



        private void button1_Click(object sender, EventArgs e)
        {
            button2.Enabled = true;
            loadpage();
            while(lvi == null)
                  loadpage();
        }

        private void loadpage()
        {
            string jsonData = j.GET("http://bloodcat.com/osu/?mod=json&p=" + oldalid.ToString());
            h = JsonConvert.DeserializeObject<RootObject[]>(jsonData);
            Savepage();

            for (int i = 0; i < h.Length - 1; i++)
            {
                h[i].maximumAr = 0;
                h[i].minimumAr = 11;
                foreach (Beatmap t in h[i].beatmaps)
                {
                    try
                    {
                        if (h[i].minimumAr > (float.Parse(t.ar, CultureInfo.InvariantCulture)) && (float.Parse(t.ar, CultureInfo.InvariantCulture) != 0))
                            h[i].minimumAr = Math.Round((double)float.Parse(t.ar, CultureInfo.InvariantCulture), 2);
                        
                        if (h[i].maximumAr < float.Parse(t.ar, CultureInfo.InvariantCulture))
                            h[i].maximumAr = Math.Round((double)float.Parse(t.ar, CultureInfo.InvariantCulture), 2);
                        
                    }
                    catch (FormatException) { }
                }
            }
            ListFill((float)numericUpDown1.Value, ((oldalid - 1) * 40));
            oldalid++;
        }

        RootObject[] pagemaps = new RootObject[65535];


        private void Savepage()      //félreteszi a mapokat egy tömbbe // kezdő vagyok pls
        {
            for (int i = ((oldalid - 1) * 40); i < h.Length + ((oldalid - 1) * 40); i++)
                pagemaps[i] = new RootObject();
            for (int i = 0; i < h.Length; i++)
                pagemaps[i + ((oldalid - 1) * 40)] = h[i];
        }


        WebClient[] wc;
        NetSpeedCounter[] o;

        int q;
        int threads;
        string[] a = new string[10];

        private bool isOn = false;

        private void button2_Click(object sender, EventArgs e)
        {
            if (!isOn)
            {

                radioButton1.Enabled = false;
                radioButton2.Enabled = false;
                radioButton3.Enabled = false;

                isOn = !isOn;
                button2.Text = "Stop";

                Directory.CreateDirectory(textBox1.Text + "\\Songs\\");
                Button clickedButton = (Button)sender;
                threads = threadselected;
                wc = new WebClient[10];
                o = new NetSpeedCounter[10];
                WebRequest.DefaultWebProxy = null;
                refillmaps();


                


                start:
                for (q = 0; q < threadselected; q++)
                {

                    wc[q] = new WebClient();
                    o[q] = new NetSpeedCounter(wc[q], 5);
                    a[q] = listView1.Items[q].SubItems[0].Text;

                    if (File.Exists(textBox1.Text + "\\Songs\\" + a[q] + ".oszx"))
                    {
                        File.Delete(textBox1.Text + "\\Songs\\" + a[q] + ".oszx");
                    }

                    //if (File.Exists(textBox1.Text + "\\Songs\\" + a[q] + ".osz"))
                    //{
                    //    idk.Add(a[q]);
                    //    lvi.ListView.Items.Clear();
                    //    oldalid--;
                    //    ListFill((float)numericUpDown1.Value);
                    //    oldalid++;
                    //    refillmaps();
                    //    q++;

                    //    goto start;

                    //}

                    using (wc[q])
                    {
                        wc[q].DownloadFileAsync(new System.Uri("http://bloodcat.com/osu/s/" + a[q]),
                        textBox1.Text + "\\Songs\\" + a[q] + ".oszx");
                        o[q].Start();
                    }

                    idk.Add(a[q]);
                    lvi.ListView.Items.Clear();
                    oldalid--;
                    ListFill((float)numericUpDown1.Value);
                    oldalid++;
                    refillmaps();
                }

                #region wcEvents
                for (int c = 0; c < threads; c++)
                {
                    if (c == 0)
                        wc[c].DownloadFileCompleted += add;
                    if (c == 1)
                        wc[c].DownloadFileCompleted += add1;
                    if (c == 2)
                        wc[c].DownloadFileCompleted += add2;
                    if (c == 3)
                        wc[c].DownloadFileCompleted += add3;
                    if (c == 4)
                        wc[c].DownloadFileCompleted += add4;
                    if (c == 5)
                        wc[c].DownloadFileCompleted += add5;
                    if (c == 6)
                        wc[c].DownloadFileCompleted += add6;
                    if (c == 7)
                        wc[c].DownloadFileCompleted += add7;
                    if (c == 8)
                        wc[c].DownloadFileCompleted += add8;
                    if (c == 9)
                        wc[c].DownloadFileCompleted += add9;

                }

                for (int c = 0; c < threads; c++)
                {
                    if (c == 0)
                        wc[c].DownloadProgressChanged += wc_DownloadProgressChanged;
                    if (c == 1)
                        wc[c].DownloadProgressChanged += wc1_DownloadProgressChanged;
                    if (c == 2)
                        wc[c].DownloadProgressChanged += wc2_DownloadProgressChanged;
                    if (c == 3)
                        wc[c].DownloadProgressChanged += wc3_DownloadProgressChanged;
                    if (c == 4)
                        wc[c].DownloadProgressChanged += wc4_DownloadProgressChanged;
                    if (c == 5)
                        wc[c].DownloadProgressChanged += wc5_DownloadProgressChanged;
                    if (c == 6)
                        wc[c].DownloadProgressChanged += wc6_DownloadProgressChanged;
                    if (c == 7)
                        wc[c].DownloadProgressChanged += wc7_DownloadProgressChanged;
                    if (c == 8)
                        wc[c].DownloadProgressChanged += wc8_DownloadProgressChanged;
                    if (c == 9)
                        wc[c].DownloadProgressChanged += wc9_DownloadProgressChanged;

                }
                #endregion
                
            }
            else
            {
                for (int i = 0; i < threadselected; i++)
                    using (wc[i])
                           wc[i].CancelAsync();

                radioButton1.Enabled = true;
                radioButton2.Enabled = true;
                radioButton3.Enabled = true;

                isOn = !isOn;
                button2.Text = "Start DL";
            }


        }
        #region RefillMaps
        private void add9(object sender, AsyncCompletedEventArgs e)
        {
            int l = 9;
            if (e.Cancelled)
            {

                File.Delete(textBox1.Text + "\\Songs\\" + a[l] + ".oszx");
                wc[l].Dispose();
                return;
            }


            int y = 10;
            if (y <= threadselected)
            {
                int x = y - 1;
                wc[x].Dispose();
                wc[x] = new WebClient();
                o[x] = new NetSpeedCounter(wc[x], 5);
                File.Move(textBox1.Text + "\\Songs\\" + a[9] + ".oszx", textBox1.Text + "\\Songs\\" + a[9] + ".osz");
                a[x] = listView1.Items[x].SubItems[0].Text;
                idk.Add(a[9]);
                lvi.ListView.Items.Clear();
                oldalid--;
                ListFill((float)numericUpDown1.Value);
                oldalid++;
                refillmaps();
                
                using (wc[x])
                {
                    wc[x].DownloadFileAsync(new System.Uri("http://bloodcat.com/osu/s/" + a[x]),
                    textBox1.Text + "\\Songs\\" + a[x] + ".oszx");
                    wc[x].DownloadProgressChanged += wc9_DownloadProgressChanged;
                    wc[x].DownloadFileCompleted += add9;
                }
                o[x].Reset();
                o[x].Start();

            }
        }

        private void add8(object sender, AsyncCompletedEventArgs e)
        {

            int l = 8;
            if (e.Cancelled)
            {
                File.Delete(textBox1.Text + "\\Songs\\" + a[l] + ".oszx");
                wc[l].Dispose();
                return;
            }

            int y = 9;
            if (y <= threadselected)
            {
                int x = y - 1;
                wc[x].Dispose();
                wc[x] = new WebClient();
                o[x] = new NetSpeedCounter(wc[x], 5);
                File.Move(textBox1.Text + "\\Songs\\" + a[8] + ".oszx", textBox1.Text + "\\Songs\\" + a[8] + ".osz");
                a[x] = listView1.Items[x].SubItems[0].Text;
                idk.Add(a[8]);
                lvi.ListView.Items.Clear();
                oldalid--;
                ListFill((float)numericUpDown1.Value);
                oldalid++;
                refillmaps();
                
                using (wc[x])
                {
                    wc[x].DownloadFileAsync(new System.Uri("http://bloodcat.com/osu/s/" + a[x]),
                    textBox1.Text + "\\Songs\\" + a[x] + ".oszx");
                    wc[x].DownloadProgressChanged += wc8_DownloadProgressChanged;
                    wc[x].DownloadFileCompleted += add8;
                }
                o[x].Reset();
                o[x].Start();

            }
        }

        private void add7(object sender, AsyncCompletedEventArgs e)
        {
            int l = 7;
            if (e.Cancelled)
            {
                File.Delete(textBox1.Text + "\\Songs\\" + a[l] + ".oszx");
                wc[l].Dispose();
                return;
            }

            int y = 8;
            if (y <= threadselected)
            {
                int x = y - 1;
                wc[x].Dispose();
                wc[x] = new WebClient();
                o[x] = new NetSpeedCounter(wc[x], 5);
                File.Move(textBox1.Text + "\\Songs\\" + a[7] + ".oszx", textBox1.Text + "\\Songs\\" + a[7] + ".osz");
                a[x] = listView1.Items[x].SubItems[0].Text;
                idk.Add(a[7]);
                lvi.ListView.Items.Clear();
                oldalid--;
                ListFill((float)numericUpDown1.Value);
                oldalid++;
                refillmaps();
                
                using (wc[x])
                {
                    wc[x].DownloadFileAsync(new System.Uri("http://bloodcat.com/osu/s/" + a[x]),
                    textBox1.Text + "\\Songs\\" + a[x] + ".oszx");
                    wc[x].DownloadProgressChanged += wc7_DownloadProgressChanged;
                    wc[x].DownloadFileCompleted += add7;
                }
                o[x].Reset();
                o[x].Start();

            }
        }

        private void add6(object sender, AsyncCompletedEventArgs e)
        {
            int l = 6;
            if (e.Cancelled)
            {
                File.Delete(textBox1.Text + "\\Songs\\" + a[l] + ".oszx");
                wc[l].Dispose();
                return;
            }

            int y = 7;
            if (y <= threadselected)
            {
                int x = y - 1;
                wc[x].Dispose();
                wc[x] = new WebClient();
                o[x] = new NetSpeedCounter(wc[x], 5);
                File.Move(textBox1.Text + "\\Songs\\" + a[6] + ".oszx", textBox1.Text + "\\Songs\\" + a[6] + ".osz");
                a[x] = listView1.Items[x].SubItems[0].Text;
                idk.Add(a[6]);
                lvi.ListView.Items.Clear();
                oldalid--;
                ListFill((float)numericUpDown1.Value);
                oldalid++;
                refillmaps();
                
                using (wc[x])
                {
                    wc[x].DownloadFileAsync(new System.Uri("http://bloodcat.com/osu/s/" + a[x]),
                    textBox1.Text + "\\Songs\\" + a[x] + ".oszx");
                    wc[x].DownloadProgressChanged += wc6_DownloadProgressChanged;
                    wc[x].DownloadFileCompleted += add6;
                }
                o[x].Reset();
                o[x].Start();

            }
        }

        private void add5(object sender, AsyncCompletedEventArgs e)
        {

            int l = 5;
            if (e.Cancelled)
            {
                File.Delete(textBox1.Text + "\\Songs\\" + a[l] + ".oszx");
                wc[l].Dispose();
                return;
            }

            int y = 6;
            if (y <= threadselected)
            {
                int x = y - 1;
                wc[x].Dispose();
                wc[x] = new WebClient();
                o[x] = new NetSpeedCounter(wc[x], 5);
                File.Move(textBox1.Text + "\\Songs\\" + a[5] + ".oszx", textBox1.Text + "\\Songs\\" + a[5] + ".osz");
                a[x] = listView1.Items[x].SubItems[0].Text;
                idk.Add(a[5]);
                lvi.ListView.Items.Clear();
                oldalid--;
                ListFill((float)numericUpDown1.Value);
                oldalid++;
                refillmaps();
                
                using (wc[x])
                {
                    wc[x].DownloadFileAsync(new System.Uri("http://bloodcat.com/osu/s/" + a[x]),
                    textBox1.Text + "\\Songs\\" + a[x] + ".oszx");
                    wc[x].DownloadProgressChanged += wc5_DownloadProgressChanged;
                    wc[x].DownloadFileCompleted += add5;
                }
                o[x].Reset();
                o[x].Start();

            }
        }

        private void add4(object sender, AsyncCompletedEventArgs e)
        {


            int l = 4;
            if (e.Cancelled)
            {
                File.Delete(textBox1.Text + "\\Songs\\" + a[l] + ".oszx");
                wc[l].Dispose();
                return;
            }

            int y = 5;
            if (y <= threadselected)
            {
                int x = y - 1;
                wc[x].Dispose();
                wc[x] = new WebClient();
                o[x] = new NetSpeedCounter(wc[x], 5);
                File.Move(textBox1.Text + "\\Songs\\" + a[4] + ".oszx", textBox1.Text + "\\Songs\\" + a[4] + ".osz");

                a[x] = listView1.Items[x].SubItems[0].Text;
                idk.Add(a[4]);
                lvi.ListView.Items.Clear();
                oldalid--;
                ListFill((float)numericUpDown1.Value);
                oldalid++;
                refillmaps();

                
                using (wc[x])
                {
                    wc[x].DownloadFileAsync(new System.Uri("http://bloodcat.com/osu/s/" + a[x]),
                    textBox1.Text + "\\Songs\\" + a[x] + ".oszx");
                    wc[x].DownloadProgressChanged += wc4_DownloadProgressChanged;
                    wc[x].DownloadFileCompleted += add4;
                }
                o[x].Reset();
                o[x].Start();

            }
        }

        private void add3(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                File.Delete(textBox1.Text + "\\Songs\\" + a[3] + ".oszx");
                wc[3].Dispose();
                return;
            }

            int y = 4;
            if (y <= threadselected)
            {
                int x = y - 1;
                wc[x].Dispose();

                wc[x] = new WebClient();
                o[x] = new NetSpeedCounter(wc[x], 5);
                File.Move(textBox1.Text + "\\Songs\\" + a[3] + ".oszx", textBox1.Text + "\\Songs\\" + a[3] + ".osz");
                a[x] = listView1.Items[x].SubItems[0].Text;
                idk.Add(a[3]);
                lvi.ListView.Items.Clear();
                oldalid--;
                ListFill((float)numericUpDown1.Value);
                oldalid++;
                refillmaps();
                
                using (wc[x])
                {
                    wc[x].DownloadFileAsync(new System.Uri("http://bloodcat.com/osu/s/" + a[x]),
                    textBox1.Text + "\\Songs\\" + a[x] + ".oszx");
                    wc[x].DownloadProgressChanged += wc3_DownloadProgressChanged;
                    wc[x].DownloadFileCompleted += add3;
                }
                o[x].Reset();
                o[x].Start();

            }
        }

        private void add2(object sender, AsyncCompletedEventArgs e)
        {
            
            if (e.Cancelled)
            {
                File.Delete(textBox1.Text + "\\Songs\\" + a[2] + ".oszx");
                wc[2].Dispose();
                return;
            }


                int y = 3;
                if (y <= threadselected)
                {
                    int x = y - 1;
                    wc[x].Dispose();
                    wc[x] = new WebClient();
                    o[x] = new NetSpeedCounter(wc[x], 5);
                     File.Move(textBox1.Text + "\\Songs\\" + a[2] + ".oszx", textBox1.Text + "\\Songs\\" + a[2] + ".osz");
                   
                    a[x] = listView1.Items[x].SubItems[0].Text;
                    idk.Add(a[2]);
                    lvi.ListView.Items.Clear();
                    oldalid--;
                    ListFill((float)numericUpDown1.Value);
                    oldalid++;
                    refillmaps();
                   
                    using (wc[x])
                    {
                        wc[x].DownloadFileAsync(new System.Uri("http://bloodcat.com/osu/s/" + a[x]),
                        textBox1.Text + "\\Songs\\" + a[x] + ".oszx");
                        wc[x].DownloadProgressChanged += wc2_DownloadProgressChanged;
                        wc[x].DownloadFileCompleted += add2;
                    }
                    o[x].Reset();
                    o[x].Start();
                }
    
        }

        private void add1(object sender, AsyncCompletedEventArgs e)
        {
            int l = 1;

            if (e.Cancelled)
            {

                File.Delete(textBox1.Text + "\\Songs\\" + a[l] + ".oszx");
                wc[l].Dispose();
                return;
            }
            int y = 2;
            if (y <= threadselected)
            {
                int x = y - 1;
                wc[x].Dispose();
                wc[x] = new WebClient();
                o[x] = new NetSpeedCounter(wc[x], 5);
                File.Move(textBox1.Text + "\\Songs\\" + a[1] + ".oszx", textBox1.Text + "\\Songs\\" + a[1] + ".osz");
                a[x] = listView1.Items[x].SubItems[0].Text;
                idk.Add(a[1]);
                lvi.ListView.Items.Clear();
                oldalid--;
                ListFill((float)numericUpDown1.Value);
                oldalid++;
                refillmaps();
                
                using (wc[x])
                {
                    wc[x].DownloadFileAsync(new System.Uri("http://bloodcat.com/osu/s/" + a[x]),
                    textBox1.Text + "\\Songs\\" + a[x] + ".oszx");
                    wc[x].DownloadProgressChanged += wc1_DownloadProgressChanged;
                    wc[x].DownloadFileCompleted += add1;
                }
                o[x].Reset();
                o[x].Start();

            }
        }

        #endregion


        private void add(object sender, AsyncCompletedEventArgs e)
        {
            int l = 0;

            if (e.Cancelled)
            {

                File.Delete(textBox1.Text + "\\Songs\\" + a[l] + ".oszx");
                wc[l].Dispose();
                return;
            }
            int y = 1;
            if (y <= threadselected)
            {
                int x = y - 1;
                wc[x].Dispose();
                wc[x] = new WebClient();
                o[x] = new NetSpeedCounter(wc[x], 5);
                File.Move(textBox1.Text + "\\Songs\\" + a[0] + ".oszx", textBox1.Text + "\\Songs\\" + a[0] + ".osz");

                a[x] = listView1.Items[x].SubItems[0].Text;
                idk.Add(a[0]);
                lvi.ListView.Items.Clear();
                oldalid--;
                ListFill((float)numericUpDown1.Value);
                oldalid++;
                refillmaps();
                
                using (wc[x])
                {
                    wc[x].DownloadFileAsync(new System.Uri("http://bloodcat.com/osu/s/" + a[x]),
                    textBox1.Text + "\\Songs\\" + a[x] + ".oszx");
                    wc[x].DownloadProgressChanged += wc_DownloadProgressChanged;
                    wc[x].DownloadFileCompleted += add;
                }
                o[x].Reset();
                o[x].Start();

            }
        }

        #region BarCHange
        private void wc9_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
           progressBar10.Value = e.ProgressPercentage;
        }

        private void wc8_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
           progressBar9.Value = e.ProgressPercentage;
        }

        private void wc7_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
           progressBar8.Value = e.ProgressPercentage;
        }

        private void wc6_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
           progressBar7.Value = e.ProgressPercentage;
        }

        private void wc5_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
           progressBar6.Value = e.ProgressPercentage;
        }

        private void wc4_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
           progressBar5.Value = e.ProgressPercentage;
        }

        private void wc3_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
           progressBar4.Value = e.ProgressPercentage;
        }

        private void wc2_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
           progressBar3.Value = e.ProgressPercentage;
   
        }

        private void wc1_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
           progressBar2.Value = e.ProgressPercentage;            
        }

        #endregion

        double speedsum;

        public void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            //string[] seb = new string[threads];
            string[] downloadedbytes = new string[threads];
            for (int i = 0; i < threads; i++)
            {

                speedsum += o[i].Speed;
                //seb[i] = (o[i].Speed.ToString()) + " kb/s";
                downloadedbytes[i] = ((o[i].ReceivedBytes/1024).ToString("##")) + " kb" ;

            }
            

        //    toolStripStatusLabel1.Text = "Download speed: " + ((speedsum).ToString("##")) + " kb /s"; //javítani majd :D


           progressBar1.Value = e.ProgressPercentage;    

        }

        private void button3_Click(object sender, EventArgs e)
        {
            var dialog = new Ookii.Dialogs.VistaFolderBrowserDialog();
            dialog.ShowDialog();     
            textBox1.Text = dialog.SelectedPath;
            
            try
            {
                WhichMapsDoIHave(ref idk);
                button2.Enabled = true;
            }
            catch (DirectoryNotFoundException) { }
            for (int i = 0; i < idk.Count; i++)
            {
                idk[i] = idk[i].Split(' ')[0];
                idk[i] = idk[i].Split('\\')[idk[i].Split('\\').Length - 1];
            }
            button1.Enabled = true;
        }

        private void selectAR_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                lvi.ListView.Items.Clear();
            }
            catch (NullReferenceException) { }

            oldalid--;          
            ListFill((float)numericUpDown1.Value);
            oldalid++;
        }
        

        private void refillmaps()
        {
            try
            {
                while (lvi.ListView.Items.Count < threadselected)
                  {
                      button1_Click(this, EventArgs.Empty);       //request new page
                  }
            }
            catch (NullReferenceException)
            {
                while(lvi == null)
                {
                    button1_Click(this, EventArgs.Empty);
                }
                refillmaps();
            }
            
        }

        int threadselected;
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            threadselected = 1;

            refillmaps();

           progressBar2.Visible = false;
           progressBar3.Visible = false;
           progressBar4.Visible = false;
           progressBar5.Visible = false;
           progressBar6.Visible = false;
           progressBar7.Visible = false;
           progressBar8.Visible = false;
           progressBar9.Visible = false;
           progressBar10.Visible = false;
           progressBar1.Visible = true;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            threadselected = 5;
            refillmaps();

           progressBar6.Visible = false;
           progressBar7.Visible = false;
           progressBar8.Visible = false;
           progressBar9.Visible = false;
           progressBar10.Visible = false;
           progressBar5.Visible = true;
           progressBar4.Visible = true;
           progressBar3.Visible = true;
           progressBar2.Visible = true;
           progressBar1.Visible = true;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            threadselected = 10;
            refillmaps();

           progressBar10.Visible = true;
           progressBar9.Visible = true;
           progressBar8.Visible = true;
           progressBar7.Visible = true;
           progressBar6.Visible = true;
           progressBar5.Visible = true;
           progressBar4.Visible = true;
           progressBar3.Visible = true;
           progressBar2.Visible = true;
           progressBar1.Visible = true;
        }
    }
    
}
