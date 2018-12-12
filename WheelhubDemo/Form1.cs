using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace WheelhubDemo
{
    public partial class Form1 : Form
    {
        private FileInfo _imageFile;
        private FileInfo ImageFile
        {

            get { return _imageFile; }
            set
            {
                _imageFile = value;

                using (var img = Image.FromFile(value.FullName))
                {
                    this.labelRawImageFileInfo.Text = string.Format("{0}: {1} X {2}", value.Name, img.Width, img.Height);
                }
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var dir = new DirectoryInfo(@".\images");
            foreach(var f in dir.GetFiles())
            {
                var ext = f.Extension.ToLower();
                if (ext == ".jpg" || ext == ".png")
                {
                    var p = new PictureBoxEx
                    {
                        Name = Guid.NewGuid().ToString(),
                        File = f,
                        Size = new Size(160, 160),
                        Image = Image.FromFile(f.FullName),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Cursor = Cursors.Hand
                    };

                    p.Click += P_Click;

                    this.flowLayoutPanel1.Controls.Add(p);
                }
            }
        }

        private void P_Click(object sender, EventArgs e)
        {
            var p = sender as PictureBoxEx;

            p.BorderStyle = BorderStyle.FixedSingle;
            
            foreach(var c in this.flowLayoutPanel1.Controls)
            {
                var a = c as PictureBoxEx;
                if (a == null)
                    continue;
                if (a.Name == p.Name)
                    continue;

                if (a.BorderStyle != BorderStyle.None)
                    a.BorderStyle = BorderStyle.None;
            }

            this.ImageFile = p.File;

            ShowLoading();

            Thread t = new Thread(new ThreadStart(Process));
            t.IsBackground = true;
            t.Start();
        }

        private void buttonSelectPic_Click(object sender, EventArgs e)
        {
            using(OpenFileDialog of = new OpenFileDialog())
            {
                of.Filter = "jpg|*.jpg|png|*.png";
                if (of.ShowDialog() == DialogResult.OK)
                {
                    this.buttonSelectPic.Enabled = false;

                    this.ImageFile = new FileInfo(of.FileName);

                    ShowLoading();

                    Thread t = new Thread(new ThreadStart(Process));
                    t.IsBackground = true;
                    t.Start();
                }
            }
        }

        private void Process()
        {
            //var results = Classify();
            var boxes = Detect();

            if (IsDetectAlgoNG(boxes, AppConfig.Threshold))
                ShowResult(boxes);
            else
                ShowOKResult();

            this.Invoke(new Action(() =>
            {
                this.buttonSelectPic.Enabled = true;
            }));
        }

        double cost
        {
            set
            {
                this.Invoke(new Action(() =>
                {
                    this.Text = string.Format("轮毂缺陷监测Demo - {0} ms", value);
                }));
            }
        }

        private Box[] Detect()
        {
            // http://wheel-hub-1-0.c6e0a93c1c8b344af83a179e13dd91164.cn-hangzhou.alicontainer.com/service/detect/wheel-hub-1-0

            int count = 0;

            var api = new Malong.Common.Api.ApiHelper
            {
                ApiUrl = AppConfig.ApiUrl,
                ContentType = Malong.Common.Api.HttpContentType.FILE,
                Method = Malong.Common.Api.HttpMethod.POST,
                RequestBody = new Malong.Common.Api.HttpRequestBody("search")
                {
                    File = this.ImageFile,
                    QueryForms = new Dictionary<string, string>
                    {
                        {"service_id", "19bcu2lc" },
                        {"service_type", "classify" }
                    }
                }
            };

            DateTime start = DateTime.Now;

            DetectResponse response = null;
            try
            {
                response= api.GetResponse<DetectResponse>();
            }
            catch
            {

            }

            while (response == null || string.IsNullOrWhiteSpace(response.request_id))
            {
                if (count > 10)
                    break;

                try
                {
                    response = api.GetResponse<DetectResponse>();
                }
                catch
                {

                }
                count++;
            }

            cost = DateTime.Now.Subtract(start).TotalMilliseconds;

            if (response == null || string.IsNullOrWhiteSpace(response.request_id))
            {
                this.buttonSelectPic.Enabled = true;
                return null;
            }

            var raw_img = Image.FromFile(this.ImageFile.FullName);

            var size = new Size(raw_img.Width, raw_img.Height);

            foreach(var b in response.boxes_detected)
            {
                b.Size = size;
            }

            return response.boxes_detected;
        }

        private void ShowLoading()
        {
            this.Invoke(new Action(() =>
            {
                var p = new PictureBox
                {
                    Width = this.groupBox2.Width - 40,
                    Height = this.groupBox2.Height - 40,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                    Left = 20,
                    Top = 20,
                    Image = Image.FromFile(this.ImageFile.FullName),
                    SizeMode = PictureBoxSizeMode.Zoom
                };
                var p1 = new PictureBox
                {
                    Width = 200,
                    Height = 200,
                    Anchor = AnchorStyles.None,
                    Left = (p.Width - 200) / 2,
                    Top = (p.Height - 200) / 2,
                    Image = global::WheelhubDemo.Properties.Resources.loader,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Parent = p,
                    BackColor = Color.Transparent
                };
                this.groupBox2.Controls.Clear();
                this.groupBox2.Controls.Add(p);
            }));
        }

        private void ShowOKResult()
        {
            this.Invoke(new Action(() =>
            {
                var p = new PictureBox
                {
                    Width = this.groupBox2.Width - 40,
                    Height = this.groupBox2.Height - 40,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                    Left = 20,
                    Top = 20,
                    Image = Image.FromFile(this.ImageFile.FullName),
                    SizeMode = PictureBoxSizeMode.Zoom
                };
                var p1 = new PictureBox
                {
                    Width = 200,
                    Height = 200,
                    Anchor = AnchorStyles.None,
                    Left = (p.Width - 200) / 2,
                    Top = (p.Height - 200) / 2,
                    Image = global::WheelhubDemo.Properties.Resources.ok_wheel,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Parent = p,
                    BackColor = Color.Transparent
                };
                this.groupBox2.Controls.Clear();
                this.groupBox2.Controls.Add(p);
            }));
        }

        private void ShowResult(Box[] boxes)
        {
            this.Invoke(new Action(() =>
            {
                var p = new PictureBox
                {
                    Width = this.groupBox2.Width - 40,
                    Height = this.groupBox2.Height - 40,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                    Left = 20,
                    Top = 20,
                    Image = Helper.DrawRectangle(Image.FromFile(this.ImageFile.FullName), boxes),
                    SizeMode = PictureBoxSizeMode.Zoom
                };
                this.groupBox2.Controls.Clear();
                this.groupBox2.Controls.Add(p);
            }));
        }

        static bool IsDetectAlgoNG(Box[] boxes, float f = 0f)
        {
            if (boxes == null || boxes.Length == 0)
                return false;

            return boxes.OrderByDescending(p => p.score).FirstOrDefault().score >= f;
        }
    }

    public class PictureBoxEx : PictureBox
    {
        public FileInfo File { get; set; }
    }
}
