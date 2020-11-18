using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAutumnClassLibrary
{
    public class ImageInfo
    {
        public int Id { get; set; }
        public string FullPath { get; set; }
        public string ClassName { get; set; }
        public float Confidence { get; set; }
        public byte[] Image { get; set; }
    }
}
