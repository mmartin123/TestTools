using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace UnitTests {
    public class GenericTestSuiteObject {
        public string TestId { get; set; }
        public string Description { get; set; }
        public string ServiceMethodName { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public string ConstructedUrl { get; set; }
    }

    public class XMLHelpers {
        private static string ServiceUrl = ConfigurationSettings.AppSettings["ASMXURL"];

        public static string SerializeServiceResponseToXMLFile(XmlSerializer x, object result, string saveAsXMLFileName) {
            //Refer: http://support.microsoft.com/kb/815813
            var file = new StreamWriter(saveAsXMLFileName + ".xml");
            x.Serialize(file, result);
            file.Close();
            return saveAsXMLFileName + ".xml";
        }

        //Code referenced from http://stackoverflow.com/questions/751511/validating-an-xml-against-referenced-xsd-in-c-sharp/752774#752774
        public static void Validate(String filename, XmlSchemaSet schemaSet) {
            Console.WriteLine();
            Console.WriteLine("\r\nValidating XML file {0}...", filename.ToString());
            XmlSchema compiledSchema = null;
            foreach (XmlSchema schema in schemaSet.Schemas()) {
                compiledSchema = schema;
            }
            var settings = new XmlReaderSettings();
            settings.Schemas.Add(compiledSchema);
            settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);
            settings.ValidationType = ValidationType.Schema;

            //Create the schema validating reader.
            var vreader = XmlReader.Create(filename, settings);
            while (vreader.Read()) { }
            vreader.Close();
        }

        //Display any warnings or errors.
        private static void ValidationCallBack(object sender, ValidationEventArgs args) {
            if (args.Severity == XmlSeverityType.Warning) {
                Console.WriteLine("\tWarning: Matching schema not found.  No validation occurred." + args.Message);
                throw new Exception("\tWarning: Matching schema not found.  No validation occurred." + args.Message);
            } else {
                Console.WriteLine("\tValidation error: " + args.Message);
                throw new Exception("\tValidation error: " + args.Message);
            }
        }

        public static List<GenericTestSuiteObject> ParseTestXML(string pathToXML) {
            var AllTestCases = new List<GenericTestSuiteObject>();
            try {
                XmlDocument doc = new XmlDocument();
                doc.Load(pathToXML);
                foreach (XmlNode node in doc.DocumentElement.ChildNodes) {
                    var testCase = new GenericTestSuiteObject();
                    if (node.Name == "test") {
                        Console.WriteLine("ParseTextXML() ");
                        testCase.TestId = node.Attributes["testID"] != null ? node.Attributes["testID"].Value : String.Empty;
                        Console.WriteLine(testCase.TestId);
                        foreach (XmlNode childNode in node.ChildNodes) {

                            if (childNode.Name == "description") {
                                Console.WriteLine("[TestID: " + testCase.TestId + "] description: " + childNode.InnerText);
                                testCase.Description = childNode.InnerText;
                            }

                            if (childNode.Name == "function") {
                                //Console.WriteLine("function: " + childNode.InnerText);
                                testCase.ServiceMethodName = childNode.InnerText;
                            }

                            if (childNode.Name == "parameters") {
                                testCase.Parameters = new Dictionary<string, string>();
                                foreach (XmlNode grandChildNode in childNode.ChildNodes) {
                                    if (grandChildNode.Name == "param") {
                                        var paramName = grandChildNode.Attributes["name"] != null ? grandChildNode.Attributes["name"].Value : String.Empty;
                                        if (String.IsNullOrEmpty(paramName)) throw new Exception("Malformed XML - param name missing in test: " + testCase.TestId);
                                        Console.WriteLine("param [" + paramName + "]: " + grandChildNode.InnerText);
                                        testCase.Parameters.Add(paramName, grandChildNode.InnerText);
                                    }
                                }
                            }
                        }
                        AllTestCases.Add(testCase);
                        Console.WriteLine();
                    }
                }
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
            return AllTestCases;
        }

        public static List<GenericTestSuiteObject> ConstructServiceUrl(List<GenericTestSuiteObject> input) {
            foreach (var x in input) {
                var url = String.Format("{0}/{1}", ServiceUrl, x.ServiceMethodName);
                var sb = new StringBuilder();
                sb.Append(url);
                if (x.Parameters.Any()) sb.Append("?");
                foreach (var p in x.Parameters) {
                    sb.Append(String.Format("{0}={1}&", p.Key, p.Value));
                }
                x.ConstructedUrl = sb.ToString();
            }
            return input;
        }
    }
}
