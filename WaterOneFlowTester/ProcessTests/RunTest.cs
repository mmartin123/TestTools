using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml;
using ConsoleAppForTesting;
using ProcessTests.Models;
using System.Text;

namespace ProcessTests
{
    public static class Tester
    {
        private static StringBuilder _sb = new StringBuilder();
        public static vmAjaxResult RunTest(string ExpectedResponsesFolder, string RealtimeResponsesFolder, string TestId, string ConstructedUrl)
        {
            _sb.Clear();
            var result = new vmAjaxResult { ErrorCode = 0 };
            var expectedResponseXMLPath = String.Empty;
            var realtimeResponseXMLPath = String.Empty;
            #region LOAD EXPECTED RESPONSE XML FOR THIS TEST CASE
            try
            {
                expectedResponseXMLPath = String.Format(@"{0}\{1}.xml", ExpectedResponsesFolder, TestId);
                var expectedDoc = new XmlDocument();
                expectedDoc.Load(expectedResponseXMLPath);
            }
            catch (Exception ex)
            {
                _sb.AppendLine("ERROR IN STEP 1: " + ex.Message);
                result.ErrorCode = 9001;
            }
            #endregion
            #region DOWNLOAD REALTIME RESPONSE FOR THIS TEST CASE AND SAVE IT AS AN XML IN "RealtimeResponsesFolder"
            try
            {
                realtimeResponseXMLPath = String.Format(@"{0}\{1}.xml", RealtimeResponsesFolder, TestId);
                using (var wc = new WebClient())
                {
                    var realtimeResponse = wc.DownloadString(ConstructedUrl);
                    File.WriteAllText(realtimeResponseXMLPath, realtimeResponse);
                }
                var realtimeResponseDoc = new XmlDocument();
                realtimeResponseDoc.Load(realtimeResponseXMLPath);
            }
            catch (Exception ex)
            {
                _sb.AppendLine("ERROR IN STEP 2: " + ex.Message);
                result.ErrorCode = 9002;
            }
            #endregion
            #region COMPARE THE TWO LOADED XML'S AND REPORT ANY FAILURES / PASS THE TEST
            try
            {
                //TODO: Call TestForEquality() before TestForEquality3 since that is faster for successful tests. 
                //We need to proceed to TestForEquality3 only if TestForEquality FAILS.
                var compareResult = XMLComparisons.TestForEquality3(expectedResponseXMLPath, realtimeResponseXMLPath);
                //[ERROR#9***] if some error ( where * could be any digit), or [ERROR#0] if not error
                if (compareResult[7] == '0')
                {
                    result.ErrorCode = 0;
                }
                else
                {
                    result.ErrorCode = Convert.ToInt32(compareResult.Substring(7, 4));
                    
                }
                result.ErrorMessage = compareResult;
                //var expectedResponseFile = File.ReadAllText(expectedResponseXMLPath);
                //var realtimeResponseFile = File.ReadAllText(realtimeResponseXMLPath);
                //XMLComparisons.TestForEquality2(expectedResponseFile, realtimeResponseFile);
            }
            catch (Exception ex)
            {
                _sb.AppendLine("ERROR IN STEP 3: " + ex.Message);
                result.ErrorCode = 9003;
                result.ErrorMessage = _sb.ToString();
            }
            #endregion
            #region PRESENT TEST RESULTS
            return result;
            #endregion
        }
    }
}