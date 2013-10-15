using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XmlUnit;
using System.Xml;
using System.Xml.Linq;
//using System.Xml.Schema;

namespace ConsoleAppForTesting {
    public class TestNode {
        public string NodeName { get; set; }
        public string NodeValue { get; set; }
        public int NodeCounter { get; set; }
    }

    public static class XMLComparisons {
        private static string _expectedResponsePath = String.Empty;
        private static string _realtimeResponsePath = String.Empty;
        private static XmlDocument _realtimeResponseXML = new XmlDocument();

        private static string _tempExpectedNodeName = String.Empty;
        private static string _tempExpectedNodeValue = String.Empty;
        private static bool _successFlag = false;   //to detect if the given node on expected xml is present in realtime xml or not
        private static bool _successNodeValuesMatchFlag = false;
        private static int _tempOccuranceCtr = 0;
        private static StringBuilder _sb = new StringBuilder();

        private static List<string> TestForEqualityExclusionList {
            get {
                //Add any nodes you wish to exclude from being compared in old database dumps and today's getValues in here.
                var exclusions = new List<string>();
                exclusions.Add("creationTime"); //Excluded because the timestamps will always be different but that doesn't mean the actual data is different
                return exclusions;
            }
        }

        public static void RecursiveNodeProcessing(XmlNode someNode) {
            var result = new List<TestNode>();
            if (someNode.HasChildNodes) {
                foreach (XmlNode node in someNode.ChildNodes) {
                    if (node.HasChildNodes && node.Name != "#text") //DO NOT CHANGE 
                    {
                        var nodeName = node.Name;
                        var nodeValue = (node.ChildNodes[0].Name == "#text") ? node.InnerText : String.Empty;
                        //Console.WriteLine(String.Format("[Name: {0}] [Value: {1}]", nodeName, nodeValue));
                        CheckIfNodeExistsInRealtimeResponse(nodeName, nodeValue);
                        if (!_successFlag || !_successNodeValuesMatchFlag) {
                            return;
                        }
                        RecursiveNodeProcessing(node);
                    }
                    //return sb.ToString();
                }
            }
        }

        public static void RecursiveNodeProcessingForRealtimeResponse(XmlNode someNode) {
            var result = new List<TestNode>();
            if (someNode.HasChildNodes) {
                foreach (XmlNode node in someNode.ChildNodes) {
                    if (node.HasChildNodes && node.Name != "#text") //DO NOT CHANGE 
                    {
                        //Console.WriteLine("Scanning occurance # " + (_tempOccuranceCtr + 1) + " of " + node.Name + " in realtimeResponse XML...");
                        var realtimeNodeName = node.Name;
                        
                        var realtimeNodeValue = (node.ChildNodes[0].Name == "#text") ? node.InnerText : String.Empty;
                        //Console.WriteLine(String.Format("[Name: {0}] [Value: {1}]", realtimeNodeName, realtimeNodeValue));
                        if (_tempExpectedNodeName == realtimeNodeName) {
                            //Console.WriteLine(String.Format("SUCCESS: {0} was found in both documents!", realtimeNodeName));
                            _successFlag = true;
                            //Next, also check that since the nodes match, if they're VALUES match as well or not?
                            //Except the exclusion cases, where it is OKAY to encounter different NodeValues
                            
                            if (_tempExpectedNodeValue == realtimeNodeValue) {
                                _successNodeValuesMatchFlag = true;
                            } else {
                                _sb.AppendLine(String.Format("Expected Node Value: [{0}] Encountered Node Value: [{1}]", _tempExpectedNodeValue, realtimeNodeValue));
                                return;
                            }
                            //return;
                        } else {
                            //TODO: Complete this section !!!
                            //_sb.AppendLine(String.Format("Expected Node Name: [{0}] Encountered Node Name: [{1}]", _tempExpectedNodeName, realtimeNodeName));
                            //return;
                        }

                        RecursiveNodeProcessingForRealtimeResponse(node);
                    }
                }
                if (!_successFlag || !_successNodeValuesMatchFlag) {
                    return;
                }
            }
        }

        public static void CheckIfNodeExistsInRealtimeResponse(string nodeName, string nodeValue) {
            _tempExpectedNodeName = nodeName;
            _tempExpectedNodeValue = nodeValue;
            _successFlag = false;
            _successNodeValuesMatchFlag = false;
            //Console.WriteLine(String.Format("Searching for node [{0}] in realtime XML", _tempExpectedNodeName));
            int i = 0;
            foreach (XmlNode node in _realtimeResponseXML.DocumentElement.ChildNodes) {

                if (i == _tempOccuranceCtr) {
                    RecursiveNodeProcessingForRealtimeResponse(node);
                    //At this point if successFlag is still false, that means the _tempExpectedNodeName was not found in the realtime XML
                    if (!_successFlag) {
                        _sb.AppendLine(String.Format("****** Node [{0}] NOT found in occurance # {1}!!! *****", _tempExpectedNodeName, _tempOccuranceCtr + 1));
                        return;
                    }
                    if (!_successNodeValuesMatchFlag) {
                        _sb.AppendLine(String.Format("****** Node [{0}] values DO NOT MATCH in occurance # {1}!!! *****", _tempExpectedNodeName, _tempOccuranceCtr + 1));
                        return;
                    }
                }
                i++;
            }
        }

        public static string TestForEquality3(string pathToExpectedXML, string pathToRealtimeXML) {
            try {
                _sb.Clear();
                _expectedResponsePath = pathToExpectedXML;
                _realtimeResponsePath = pathToRealtimeXML;
                _realtimeResponseXML.Load(_realtimeResponsePath);

                XmlDocument doc = new XmlDocument();
                doc.Load(pathToExpectedXML);
                int i = 0;
                foreach (XmlNode node in doc.DocumentElement.ChildNodes) {
                    _tempOccuranceCtr = i;  //This is where it shud be set

                    //Console.ReadLine();
                    RecursiveNodeProcessing(node);
                    if (!_successFlag || !_successNodeValuesMatchFlag) {
                        _sb.AppendLine("ERROR in ExpectedResponse XML" + node.Name + " occurance # " + (i + 1));
                        _sb.AppendLine("**** ERROR - HALTING PROGRAM *****");
                        _sb.Insert(0, "[ERROR#9100]");
                        _sb.AppendLine();
                        return _sb.ToString();
                    }
                    //Console.WriteLine();
                    i++;

                }
            } catch (Exception ex) {
                //throw new Exception(ex.Message);
                _sb.Insert(0, "[ERROR#9101]");
                _sb.AppendLine();
                _sb.AppendLine(ex.Message);
            }
            _sb.Insert(0, "[ERROR#0]");
            _sb.AppendLine();
            return _sb.ToString();
        }

        public static string TestForEqualityByXMLString(string oldXMLString, string newXMLString) {
            try {
                _sb.Clear();
                //_expectedResponsePath = pathToExpectedXML;
                //_realtimeResponsePath = pathToRealtimeXML;
                _realtimeResponseXML.LoadXml(newXMLString);
                

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(oldXMLString);
                

                int i = 0;
                foreach (XmlNode node in doc.DocumentElement.ChildNodes) {
                    _tempOccuranceCtr = i;  //This is where it shud be set

                    //Console.ReadLine();
                    RecursiveNodeProcessing(node);
                    if (!_successFlag || !_successNodeValuesMatchFlag) {
                        _sb.AppendLine("ERROR in ExpectedResponse XML" + node.Name + " occurance # " + (i + 1));
                        _sb.AppendLine("**** ERROR - HALTING PROGRAM *****");
                        _sb.Insert(0, "[ERROR#9100]");
                        _sb.AppendLine();
                        return _sb.ToString();
                    }
                    //Console.WriteLine();
                    i++;

                }
            } catch (Exception ex) {
                //throw new Exception(ex.Message);
                _sb.Insert(0, "[ERROR#9101]");
                _sb.AppendLine();
                _sb.AppendLine(ex.Message);
            }
            _sb.Insert(0, "[ERROR#0]");
            _sb.AppendLine();
            return _sb.ToString();
        }

        public static void TestForEquality(string expectedResponse, string realtimeResponse) {
            var expectedXML = new XmlInput(expectedResponse);
            var realtimeXML = new XmlInput(realtimeResponse);
            DiffConfiguration config = new DiffConfiguration();
            var x = new XmlDiff(expectedXML, realtimeXML, config);
            var result = x.Compare();

            Console.WriteLine("equal: " + result.Equal);
            Console.WriteLine("identical: " + result.Identical);
            Console.WriteLine("string: " + result.StringValue);

            if (result.Equal || result.Identical) {
                //No need to proceed and okay to declare test passed
                //TODO: Call routine to save test result
            } else {


                var difference = result.Difference;
                var originalNode = difference.ControlNodeType;

                DiffResult d = new DiffResult();
                d.DifferenceFound(x, difference);

                //result.DifferenceFound(x, difference);
            }

        }
    }
}
