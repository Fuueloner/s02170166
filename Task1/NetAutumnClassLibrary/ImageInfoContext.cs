using System.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetAutumnClassLibrary
{
    public class ImageInfoContext : DbContext
    {
        public ImageInfoContext() : base("DbConnection") { }
        public DbSet<ImageInfo> ImageInfos { get; set; }
    }
}
