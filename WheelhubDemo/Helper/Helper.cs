using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Security.Cryptography;
using System.IO;
using System.Linq;

namespace WheelhubDemo
{
    public static class Helper
    {
public static double OverlapAreaRate(this Rectangle rectangle1, Rectangle rectangle2)
{
    int startX = Math.Min(rectangle1.X, rectangle2.X);
    int endX = Math.Max(rectangle1.X + rectangle1.Width, rectangle2.X + rectangle2.Width);
    int overlapWidth = rectangle1.Width + rectangle2.Width - (endX - startX);

    int startY = Math.Min(rectangle1.Y, rectangle2.Y);
    int endY = Math.Max(rectangle1.Y + rectangle1.Height, rectangle2.Y + rectangle2.Height);
    int overlapHeight = rectangle1.Height + rectangle2.Height - (endY - startY);

    if (overlapWidth <= 0 || overlapHeight <= 0)
        return 0d;

    var overLapArea = overlapWidth * overlapHeight;

    return overLapArea * 1.0d / (rectangle1.Width * rectangle1.Height + rectangle2.Width * rectangle2.Height - overLapArea);
}

        public static double OverlapAreaRate(this RectangleF rectangle1, RectangleF rectangle2)
        {
            var startX = Math.Min(rectangle1.X, rectangle2.X);
            var endX = Math.Max(rectangle1.X + rectangle1.Width, rectangle2.X + rectangle2.Width);
            var overlapWidth = rectangle1.Width + rectangle2.Width - (endX - startX);

            var startY = Math.Min(rectangle1.Y, rectangle2.Y);
            var endY = Math.Max(rectangle1.Y + rectangle1.Height, rectangle2.Y + rectangle2.Height);
            var overlapHeight = rectangle1.Height + rectangle2.Height - (endY - startY);

            if (overlapWidth <= 0 || overlapHeight <= 0)
                return 0d;

            var overLapArea = overlapWidth * overlapHeight;

            return overLapArea * 1.0d / (rectangle1.Width * rectangle1.Height + rectangle2.Width * rectangle2.Height - overLapArea);
        }

        public static string ComputMd5(this string file)
        {
            if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
                return "";

            var bytes = File.ReadAllBytes(file);
            var md5 = MD5.Create();
            var md5Bytes = md5.ComputeHash(bytes);
            var _md5 = "";
            foreach (var b in md5Bytes)
            {
                _md5 += b.ToString("X2");
            }
            return _md5;
        }

        public static string ComputMd5(this FileInfo file)
        {
            if (!file.Exists)
                return "";

            var bytes = File.ReadAllBytes(file.FullName);
            var md5 = MD5.Create();
            var md5Bytes = md5.ComputeHash(bytes);
            var _md5 = "";
            foreach (var b in md5Bytes)
            {
                _md5 += b.ToString("X2");
            }
            return _md5;
        }

        public static Image DrawRectangle(this Image img, Box[] boxes)
        {
            if (boxes == null || boxes.Length == 0)
                return img;

            // 对矩形框进行合并
            var _boxes = MergeBox(boxes);

            using (Graphics g = Graphics.FromImage(img))
            {
                Color? color = null;
                foreach (var b in _boxes)
                {
                    var x = img.Width * b.box[0];
                    var y = img.Height * b.box[1];
                    var w = img.Width * b.box[2];
                    var h = img.Height * b.box[3];

                    color = GetColor(color);

                    g.DrawRectangle(new Pen(new SolidBrush(color.Value), 4.0f), x, y, w, h);

                    g.DrawString(b.score.ToString(), new Font("微软雅黑", 10.0f, FontStyle.Bold), new SolidBrush(Color.Red), new PointF(x, y - 20));
                }
            }

            return img;
        }

        private static Color[] Colors = new Color[] { Color.Red, Color.Yellow, Color.Green, Color.Blue, Color.Purple };
        private static Color GetColor(Color? color)
        {
            if (!color.HasValue)
                return Colors[0];

            for (int i = 0; i < Colors.Length; i++)
            {
                if (Colors[i] == color.Value)
                {
                    if (i == Colors.Length - 1)
                        return Colors[0];
                    else
                        return Colors[i + 1];
                }
            }

            return Colors[0];
        }

        private static Box[] MergeBox(Box[] boxes)
        {
            if (boxes == null || boxes.Length < 2)
                return boxes;

            List<Box> _boxes = boxes.ToList();

            // 两两对比，进行框的合并
            for(int i = 0; i < _boxes.Count; i++)
            {
                for(int j = i + 1; j < _boxes.Count; j++)
                {
                    var rec1 = _boxes[i].RectangleF;
                    var rec2 = _boxes[j].RectangleF;

                    var d = OverlapAreaRate(rec1, rec2);
                    if (d > 0)
                    {
                        var x = Math.Min(rec1.X, rec2.X);
                        var y = Math.Min(rec1.Y, rec2.Y);

                        var startX = Math.Min(rec1.X, rec2.X);
                        var endX = Math.Max(rec1.X + rec1.Width, rec2.X + rec2.Width);

                        var startY = Math.Min(rec1.Y, rec2.Y);
                        var endY = Math.Max(rec1.Y + rec1.Height, rec2.Y + rec2.Height);

                        var w = (endX - startX);
                        var h = (endY - startY);

                        _boxes[i] = new Box
                        {
                            box = new float[] { x / _boxes[i].Size.Width, y / _boxes[i].Size.Height, w / _boxes[i].Size.Width, h / _boxes[i].Size.Height },
                            puid = _boxes[i].puid,
                            score = Math.Max(_boxes[i].score, _boxes[j].score),
                            Size = _boxes[i].Size,
                            type = _boxes[i].type
                        };


                        _boxes.RemoveAt(j);
                        j--;
                    }
                }
            }

            return _boxes.ToArray();
        }
    }
}
