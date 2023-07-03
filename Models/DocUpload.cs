using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewDepot.Models
{
    public class DocUpload
    {
        public int Id { get; set; }
        //public HttpPostedFileBase File { get; set; }
    }

    public class DocuploadResult
    {
        public int DocTypId { get; set; }
        public int FileId { get; set; }
    }
}