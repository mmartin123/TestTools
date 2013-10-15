using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UnitTests;

namespace ProcessTests.Models
{
    public class vmMain
    {
        public vmAjaxResult vmAjaxResult { get; set; }
        public List<vmTestCase> TestCases { get; set; }
    }

    public class vmTestCase
    {
        public GenericTestSuiteObject TestSuiteObject { get; set; }
        public DateTime InitializationTime { get; set; }
        public DateTime DurationRun { get; set; }
        public bool TestPassed { get; set; }
        
    }
}