using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Configuration;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using UnitTests;


namespace ConsoleAppForTesting
{
    class Program
    {
        private static string TestCasesFile = ConfigurationSettings.AppSettings["TestCasesFile"];
        private static string ExpectedResponsesFolder = ConfigurationSettings.AppSettings["ExpectedResponsesFolder"];
        private static string RealtimeResponsesFolder = ConfigurationSettings.AppSettings["RealtimeResponsesFolder"];


        static void Main(string[] args)
        {
            try{

                Random random = new Random(6);
                Console.WriteLine((random.Next() % 20000) + 1);
                Console.ReadLine();


                return;

                using (var wc = new WebClient()) {
                    var testing = wc.DownloadString("http://hydro.keskari.webfactional.com/services/cuahsi_1_1.php?op=GetSites&site_0=a&site_1=b&authToken=c");
                    //var testing = wc.DownloadString("http://hydro.keskari.webfactional.com/services/cuahsi_1_1.php?op=GetSites&site=a&site=b&authToken=c");
                    //var testing = wc.DownloadString("http://hydro.keskari.webfactional.com/services/cuahsi_1_1.php?op=GetSites&site=a,b&authToken=c");
                    //var testing = wc.DownloadString("http://hydro.keskari.webfactional.com/services/cuahsi_1_1.php?op=GetSites&site[]=a&site[]=b&authToken=c");
                    return;
                }
            

                var listOfServiceCallsToMake = XMLHelpers.ParseTestXML(TestCasesFile);
                var populatedUrls = XMLHelpers.ConstructServiceUrl(listOfServiceCallsToMake);
                //populatedUrls.ForEach(x => { var expectedResponseXMLPath = String.Format(@"{0}\{1}", ExpectedResponsesFolder, x.TestId); });
                foreach (var p in populatedUrls)
                {
                    Console.WriteLine("***************ATTEMPTING TESTid: " + p.TestId + " ***********************");
                    var expectedResponseXMLPath = String.Empty;
                    var realtimeResponseXMLPath = String.Empty;
                    #region LOAD EXPECTED RESPONSE XML FOR THIS TEST CASE
                    try
                    {
                        expectedResponseXMLPath = String.Format(@"{0}\{1}.xml", ExpectedResponsesFolder, p.TestId);
                        var expectedDoc = new XmlDocument();
                        expectedDoc.Load(expectedResponseXMLPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ERROR IN STEP 1: " + ex.Message);
                    }
                    #endregion
                    #region DOWNLOAD REALTIME RESPONSE FOR THIS TEST CASE AND SAVE IT AS AN XML IN "RealtimeResponsesFolder"
                    try
                    {
                        realtimeResponseXMLPath = String.Format(@"{0}\{1}.xml", RealtimeResponsesFolder, p.TestId);
                        using (var wc = new WebClient())
                        {
      //                      <site>
      //  <string>string</string>
      //  <string>string</string>
      //</site>
      //<authToken>string</authToken>
                            
                            var realtimeResponse = wc.DownloadString(p.ConstructedUrl);
                            File.WriteAllText(realtimeResponseXMLPath, realtimeResponse);
                        }
                        var realtimeResponseDoc = new XmlDocument();
                        realtimeResponseDoc.Load(realtimeResponseXMLPath);
                    }
                    
                    
                    catch (Exception ex)
                    {
                        Console.WriteLine("ERROR IN STEP 2: " + ex.Message);



                    }
                    #endregion
                    #region COMPARE THE TWO LOADED XML'S AND REPORT ANY FAILURES / PASS THE TEST
                    try
                    {
                        XMLComparisons.TestForEquality3(expectedResponseXMLPath, realtimeResponseXMLPath);
                        var expectedResponseFile = File.ReadAllText(expectedResponseXMLPath);
                        var realtimeResponseFile = File.ReadAllText(realtimeResponseXMLPath);
                        //XMLComparisons.TestForEquality2(expectedResponseFile, realtimeResponseFile);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ERROR IN STEP 3: " + ex.Message);
                    }
                    #endregion
                    #region PRESENT TEST RESULTS
                    #endregion
                }
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }

        }



    }
}
