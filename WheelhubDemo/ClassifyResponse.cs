using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WheelhubDemo
{
    public class ClassifyResponse
    {
        public Result[] results { get; set; }
    }

    public class Result
    {
        public string category { get; set; }
        public string puid { get; set; }
        public float score { get; set; }
    }
}
