using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProcessTests.DataAccess {
    public partial class TestArchive {
        public String TimestampString { get { return Timestamp.ToShortDateString() + " " + Timestamp.ToLongTimeString(); } }
    }
}