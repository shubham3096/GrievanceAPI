using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrievanceService.Models
{
    public class MediaFileModel
    {
        public List<InputFile> files { get; set; }
    }
    public class InputFile
    {
        public string filename { get; set; }

        public int? filesize { get; set; }

        public string filetype { get; set; }

        public string base64 { get; set; }

        public string doc_id { get; set; }
    }

    public class MediaFileModelForeSewa
    {
        public List<InputFileForeSewa> files { get; set; }
    }
    public class InputFileForeSewa
    {
        public string filename { get; set; }

        public int? filesize { get; set; }

        public string filetype { get; set; }

        public string base64 { get; set; }

        public string doc_id { get; set; }
        public string userid { get; set; }
    }
}
