using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProcessTests.Models
{
    public class vmAjaxResult
    {
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string Data { get; set; }
    }
}