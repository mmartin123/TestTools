using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ProcessTests.WaterML11;

using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

using ProcessTests.DataAccess;


namespace ProcessTests {
    //Google username: waterdatacenter@gmail.com password: waterdatacenter123


    public class MultiEndPoint {
        public static List<String> AdminUsers {
            get {
                var adminUsers = new List<string>();
                //Keep adding any admin users here
                adminUsers.Add("cccc");
                adminUsers.Add("newspring400@gmail.com");
                adminUsers.Add("jpollak@cuahsi.org");
                return adminUsers;
            }
        }

        public class GeneratedTest {
            public string confirmationNo { get; set; }
            public string wsdl { get; set; }
            public string seed { get; set; }
            public Site randomSite { get; set; }
            public Variable randomVariable { get; set; }
            public SiteInfo randomSiteInfo { get; set; }
        }
        public class Site {
            public string SiteName { get; set; }
            public string SiteCode { get; set; }
            public string Network { get; set; }
        }
        public class Variable {
            public string VariableId { get; set; }
            public string VariableCode { get; set; }
            public string VariableNetwork { get; set; }
        }
        public class SiteInfo {
            public bool traversalError = false;
            public string BeginDateTime { get; set; }
            public string EndDateTime { get; set; }
            public string BeginDateTimeUTC { get; set; }
            public string EndDateTimeUTC { get; set; }
            public List<Variable> SiteVariables = new List<Variable>();
        }
        private static void AssertIfValidXML(string dataReceived) {
            //If invalid XML has been returned by a HydroServer, an exception will automatically be thrown here
            var xDoc = XDocument.Parse(dataReceived);
        }
        private static int GetRandomCursor(int seed, int listCount) {
            //using the seed, give me an index or cursor value that does not exceed the number of items in my list
            var cursor = (seed > listCount) ? seed % listCount : seed;
            return cursor;
        }

        public static GeneratedTest GenerateTest_1_1(string url, int seed) {
            var result = new GeneratedTest();
            result.wsdl = url;
            result.seed = seed.ToString();
            var listSiteInfo = GetSites(url);
            if (!listSiteInfo.Any()) {
                throw new Exception("GetSites returned 0 sites. Cannot proceed!");
            }
            //next pick a random site from the list of sites we obtained
            var selectedSite = listSiteInfo[GetRandomCursor(seed, listSiteInfo.Count)];
            result.randomSite = selectedSite;
            var listVariableInfo = GetVariableInfo(url, selectedSite.Network);
            if (!listVariableInfo.Any()) {
                listVariableInfo = VariableNotFoundWorkaround(url, selectedSite);
            }
            if (!listVariableInfo.Any()) {
                throw new Exception("GetVariableInfo returned 0 variables. Cannot proceed!");
            }
            var selectedVariable = listVariableInfo[GetRandomCursor(seed, listVariableInfo.Count)];
            result.randomVariable = selectedVariable;
            var siteInfo = GetSiteInfo(url, selectedSite, selectedVariable, false);
            if (siteInfo.traversalError) {
                throw new Exception(String.Format("Variable [{0}:{1}] not found in SiteInfo() for [{2}]", selectedVariable.VariableNetwork, selectedVariable.VariableCode, selectedSite.SiteCode));
            }
            result.randomSiteInfo = siteInfo;
            //GetValues(url, seed, result.randomSite, result.randomVariable, result.randomSiteInfo);
            return result;

        }
        public static string GetValuesWrapper(string summaryObject, string username) {
            var js = new System.Web.Script.Serialization.JavaScriptSerializer();
            GeneratedTest summary = js.Deserialize<GeneratedTest>(summaryObject);
            summary.confirmationNo = String.Empty;
            var getValues = GetValues(summary.wsdl, Convert.ToInt32(summary.seed), summary.randomSite, summary.randomVariable, summary.randomSiteInfo);
            //Dump the values into the db
            return DataAccess.DataAccess.InsertTestResult(new TestArchive { Id = -1, Timestamp = DateTime.Now, WSDL = summary.wsdl, Seed = Convert.ToInt32(summary.seed), GetValuesResult = getValues, Username = username });
            //If confirmation is blank, something went wrong!
        }

        public static string GetValuesForComparison(string summaryObject) {
            var js = new System.Web.Script.Serialization.JavaScriptSerializer();
            GeneratedTest summary = js.Deserialize<GeneratedTest>(summaryObject);
            var getValues = GetValues(summary.wsdl, Convert.ToInt32(summary.seed), summary.randomSite, summary.randomVariable, summary.randomSiteInfo);
            return getValues;
        }

        private static List<Site> GetSites(string url) {
            //connect to web service
            //WaterML10.WaterOneFlowSoapClient Wof10Client = new WaterML10.WaterOneFlowSoapClient("WaterOneFlowSoap", url);
            //var getSites10 = Wof10Client.GetSites(new string[] { }, String.Empty);
            //var testxxx = getSites10.site[0];
            //AssertIfValidXML(getSites10);
            //XmlDocument doc10 = new XmlDocument();
            //doc10.LoadXml(getSites10);
            WaterML11.WaterOneFlowClient WofClient = new WaterOneFlowClient("WaterOneFlow", url);
            WaterML11.SiteInfoResponseType sitesAll = null;
            //call the getSites method on this service
            var getSites = WofClient.GetSites(new string[] { }, String.Empty);
            //ensure the service returns valid XML
            AssertIfValidXML(getSites);
            XmlDocument doc = new XmlDocument();
            //convert that string into an xml object
            doc.LoadXml(getSites);
            var listSites = new List<Site>();
            //traverse the xml using nested loops to obtain desired fields and values
            foreach (XmlNode node in doc.DocumentElement.ChildNodes) {
                if (node.HasChildNodes) {
                    foreach (XmlNode childNode in node.ChildNodes) {
                        if (childNode.Name == "siteInfo" && childNode.HasChildNodes) {
                            var tempSite = new Site();
                            foreach (XmlNode siteNodes in childNode.ChildNodes) {
                                var nodeName = siteNodes.Name;
                                var nodeValue = siteNodes.InnerText;
                                if (nodeName == "siteName") {
                                    tempSite.SiteName = nodeValue;
                                }
                                if (nodeName == "siteCode") {
                                    tempSite.SiteCode = nodeValue;
                                    if (siteNodes.Attributes["network"] != null) {
                                        tempSite.Network = siteNodes.Attributes["network"].Value;
                                    }
                                }
                            }
                            listSites.Add(tempSite);    //add each iteration of a site that getSites() returns into a list
                        }
                    }
                }
            }
            return listSites;
        }
        private static List<Variable> GetVariableInfo(string url, string network) {
            var listVariable = new List<Variable>();
            try {
                WaterML11.WaterOneFlowClient WofClient = new WaterOneFlowClient("WaterOneFlow", url);
                WaterML11.SiteInfoResponseType sitesAll = null;
                var getVariableInfo = WofClient.GetVariableInfo(network + ":", String.Empty);
                //I want to get a collection of all variables associated to this 'network' via this call. This seems to work for some URL's, but returns no variables for some other URS.
                //Question: Instead of doing this, is it okay to do the following:
                //When we call getSiteInfo() for a randomly selected site, that object already contains a bunch of variables. Is it okay to randomly pick a variable from that collection instead, and 
                //completely bypass calling GetVariableInfo(network + ":", String.Empty) and then picking a random variable from the result... ?
                //Instead, we would then skip calling GetVariableInfo() and proceed to calling GetValues() using a randomly selected variable as returned in GetSiteInfo()...

                //Finally, if it IS ok to simply rely on the output from GetSiteInfo() for picking our variables, then what do we do if no variables are present in that packet?
                //Is it ok to throw an error then?

                //
                AssertIfValidXML(getVariableInfo);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(getVariableInfo);

                foreach (XmlNode node in doc.DocumentElement.ChildNodes) {
                    if (node.HasChildNodes) {
                        foreach (XmlNode childNode in node.ChildNodes) {
                            if (childNode.Name == "variable" && childNode.HasChildNodes) {
                                var tempVariable = new Variable();
                                foreach (XmlNode variableNodes in childNode.ChildNodes) {
                                    var nodeName = variableNodes.Name;
                                    var nodeValue = variableNodes.InnerText;
                                    if (nodeName == "variableCode") {
                                        tempVariable.VariableCode = nodeValue;
                                        if (variableNodes.Attributes["vocabulary"] != null) {
                                            tempVariable.VariableNetwork = variableNodes.Attributes["vocabulary"].Value;
                                        }
                                        if (variableNodes.Attributes["variableID"] != null) {
                                            tempVariable.VariableId = variableNodes.Attributes["variableID"].Value;
                                        }
                                    }
                                }
                                listVariable.Add(tempVariable);
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                if (ex.Message == "Variable Not Found") {
                    return listVariable;
                }
            }
            return listVariable;
        }

        private static List<Variable> VariableNotFoundWorkaround(string url, Site s) {
            var workaroundResult = new List<Variable>();
            workaroundResult = GetSiteInfo(url, s, null, true).SiteVariables;
            return workaroundResult;

        }

        private static SiteInfo GetSiteInfo(string url, Site site, Variable randomVariable, bool isWorkAroundMode) {
            WaterML11.WaterOneFlowClient WofClient = new WaterOneFlowClient("WaterOneFlow", url);
            WaterML11.SiteInfoResponseType sitesAll = null;
            var getSiteInfo = WofClient.GetSiteInfo(site.Network + ":" + site.SiteCode, String.Empty);
            AssertIfValidXML(getSiteInfo);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(getSiteInfo);
            var siteInfo = new SiteInfo();
            foreach (XmlNode node in doc.DocumentElement.ChildNodes) {
                if (node.HasChildNodes) {
                    foreach (XmlNode childNode in node.ChildNodes) {
                        if (childNode.Name == "seriesCatalog" && childNode.HasChildNodes) {
                            foreach (XmlNode series in childNode.ChildNodes) {
                                var isMatchFound = false;
                                foreach (XmlNode seriesChild in series) {
                                    if (seriesChild.Name == "variable" && series.HasChildNodes) {
                                        foreach (XmlNode variableChild in seriesChild) {
                                            if (variableChild.Name == "variableCode") {
                                                var tempVariable = new Variable();
                                                if (variableChild.Attributes["vocabulary"] != null) {
                                                    tempVariable.VariableNetwork = variableChild.Attributes["vocabulary"].Value;
                                                }
                                                if (variableChild.Attributes["variableID"] != null) {
                                                    tempVariable.VariableId = variableChild.Attributes["variableID"].Value;
                                                }

                                                if (isWorkAroundMode) {
                                                    //Stuff the variables into list property for workaround
                                                    tempVariable.VariableCode = variableChild.InnerText;
                                                    siteInfo.SiteVariables.Add(tempVariable);
                                                } else {
                                                    //Now compare tempVariable against randomVariable
                                                    if (tempVariable.VariableNetwork == randomVariable.VariableNetwork &&
                                                        tempVariable.VariableId == randomVariable.VariableId) {
                                                        isMatchFound = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    if (!isWorkAroundMode && isMatchFound) {
                                        if (seriesChild.Name == "variableTimeInterval" && series.HasChildNodes) {
                                            foreach (XmlNode variableTimeIntervalChild in seriesChild) {
                                                if (variableTimeIntervalChild.Name == "beginDateTime") {
                                                    siteInfo.BeginDateTime = variableTimeIntervalChild.InnerText;
                                                }
                                                if (variableTimeIntervalChild.Name == "endDateTime") {
                                                    siteInfo.EndDateTime = variableTimeIntervalChild.InnerText;
                                                }
                                                if (variableTimeIntervalChild.Name == "beginDateTimeUTC") {
                                                    siteInfo.BeginDateTimeUTC = variableTimeIntervalChild.InnerText;
                                                }
                                                if (variableTimeIntervalChild.Name == "endDateTimeUTC") {
                                                    siteInfo.EndDateTimeUTC = variableTimeIntervalChild.InnerText;
                                                }
                                            }
                                            return siteInfo;
                                        }
                                    }
                                }

                            }
                        }
                    }   //main foreach ends
                }
            }
            if (!isWorkAroundMode) siteInfo.traversalError = true;
            return siteInfo;
        }

        public static string GetValues(string url, int seed, Site site, Variable randomVariable, SiteInfo siteInfo) {
            WaterML11.WaterOneFlowClient WofClient = new WaterOneFlowClient("WaterOneFlow", url);
            WaterML11.SiteInfoResponseType sitesAll = null;
            var getValues = WofClient.GetValues(site.Network + ":" + site.SiteCode, randomVariable.VariableNetwork + ":" + randomVariable.VariableCode, siteInfo.BeginDateTime, siteInfo.EndDateTime, String.Empty);
            AssertIfValidXML(getValues);
            return getValues;
        }
    }
}
