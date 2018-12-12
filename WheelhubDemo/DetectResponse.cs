using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WheelhubDemo
{
    public class DetectResponse
    {
        public Box[] boxes_detected { get; set; }

        public string request_id { get; set; }
    }

    public class Box
    {
        public float[] box { get; set; }
        public string puid { get; set; }
        public float score { get; set; }
        public string type { get; set; }

        public System.Drawing.RectangleF RectangleF
        {
            get
            {
                return new System.Drawing.RectangleF
                {
                    X = this.Size.Width * box[0],
                    Y = this.Size.Height * box[1],
                    Width = this.Size.Width * box[2],
                    Height = this.Size.Height * box[3]
                };
            }
        }

        public System.Drawing.Size Size
        {
            get; set;
        }
    }
}
