namespace Dsp.Areas.Edu.Models
{
    using System.Collections.Generic;
    using System.Web;

    public class FileInfoModel
    {
        public HttpPostedFileBase File { get; set; }
        public IEnumerable<string> ExistingFiles { get; set; }
    }
}
