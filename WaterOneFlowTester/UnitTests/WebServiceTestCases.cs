using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using System.Xml.Schema;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class GetMappedVariables2Test
    {
        [Test]
        public static void CheckIfSendingInvalidInputThrowsError()
        {
            var s = new org.cuahsi.hiscentral.hiscentral();
            try
            {
                var result = s.GetMappedVariables2("19xxx", "1xxx");
                //(remove old code)var result = s.GetMappedVariables2("19xxx", "1xx"); //19 and 1 werre the magic numbers that return valid ouput, lets damage it by adding garbage along
            }
            catch (Exception ex)
            {
                Assert.Pass();  //Notice I pass the test even though an exception occurs. Intentional, because we WANT it to result an error as we supplied invalid input
            }
            Assert.Fail("The method returned valid output even though invalied input was supplied to it!"); //But if no errors occured, something is wrong in the programming of the web service.. so the test should fail
        }

        [Test]
        public static void ValidateResponseAgainstXSDSchema()
        {
            try
            {
                var s = new org.cuahsi.hiscentral.hiscentral();
                var result = s.GetMappedVariables2("19", "1");
                var xml = new System.Xml.Serialization.XmlSerializer(result.GetType());
                var xmlFile = XMLHelpers.SerializeServiceResponseToXMLFile(xml, result, "Response_GetMappedVariables2");
                //Load the XmlSchemaSet.
                //First obtain the XSD's for each web method. You can use "XSD.EXE" to generate the XSD by passing it an XML file
                //XSD.EXE typical location (may differ for you, Google it): C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX_4.0 Tools\XSD.EXE
                //Reference: http://stackoverflow.com/questions/14022190/is-it-possible-to-generate-xsd-from-asmx
                //Example: I consumed the GetMappedVariables2 method (with magic numbers 19, 1) and saved the response as an XML file
                //Then I passed it to generate the XSD schema for this method. See "XSD" folder in this solution.

                var schemaSet = new XmlSchemaSet();
                schemaSet.Add("http://hiscentral.cuahsi.org/20100205/", "GetMappedVariables2.xsd");

                //A passing test since the live web service is currently returning valid data (toggle comment):
                XMLHelpers.Validate(xmlFile, schemaSet);
                //A demo failed test (toggle comment) where I intentionally added an "x" in the XML file below to corrupt it:
                //XMLHelpers.Validate("InvalidResponse_GetMappedVariables2.xml", schemaSet);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            Assert.Pass();
        }

        [Test]
        public static void CheckRawReadIsXML()
        {
            try
            {
                //Grab the response from the web service manually using an HTTP GET

                //string url = "http://hiscentral.cuahsi.org/webservices/hiscentral.asmx/GetMappedVariables2?conceptids=19&Networkids=1";
                string url = "http://sandbox.cuahsi.org/hiscentral/webservices/hiscentral.asmx/GetMappedVariables2?conceptids=19&Networkids=1";

                using (WebClient wc = new WebClient())
                {
                    var result = wc.DownloadString(url);

                    //Uncomment line below to force test to fail - pretending that the data received was corrupted for some reason
                    //result = result.Replace("<MappedVariable>", "<CorruptedData>");
                    //Try and convert receieved string to XML. If call to XDocument.Parse() fails, we can safely assume that the test should fail due to a transmission error

                    var xDoc = XDocument.Parse(result);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail("ERROR: Data recieved is not XML! " + ex.Message);
            }

            Assert.Pass();
        }

        [Test]
        public static void CheckIfResponseIsAsPredicted()
        {
            var s = new org.cuahsi.hiscentral.hiscentral();
            var result = s.GetMappedVariables2("19", "1");
            if (!result.Any())
            {
                Assert.Fail("This call expected one or more MappedVariables in the result, but none were returned");
            }
            if (result.Count() != 20)
            {
                Assert.Fail("This call expected specifically 20 results in the output, but that was not the case");
            }
            //Do I need to check each one of the 20 to have the predicted values, or is just checking the first one sufficient to
            //declare the test as having passed? I am only checking the first one for now.
            var passedFlag = true;
            if (result[0].variableName != "Ground-water level above NAVD 1988, feet") passedFlag = false;
            if (result[0].variableCode != "NWISDV:62611/DataType=Minimum") passedFlag = false;
            if (result[0].servCode != "NWISDV") passedFlag = false;
            if (result[0].WSDL != "http://river.sdsc.edu/wateroneflow/NWIS/DailyValues.asmx?WSDL") passedFlag = false;
            if (result[0].conceptCode != "19") passedFlag = false;
            if (passedFlag == false) Assert.Fail("The first MappedVariable did not contain the expected values in one or more of it's attributes");
            Assert.Pass();
        }

        #region EQUIVALENCE PARTITION CLASS TESTS
        //Valid ConceptIds: A = {12}, B = {9, 10, 12, 13, 16, 19}, C = {9, 10, 12, 13, 16, 19, 30, 33, 35, 36, 49, 51, 55, 56, 65, 67, 78, 90, 94}
        //Valid NetworkIds: X = {1}, Y = {1, 2, 3, 4}, Z = {1, 2, 3, 4, 5, 8, 12}

        [Test]
        //A and X
        public void OneNetworkIDAndOneConceptId()
        {
            try
            {
                var s = new org.cuahsi.hiscentral.hiscentral();
                var result = s.GetMappedVariables2("12", "1");
                if (!result.Any())
                {
                    Assert.Fail("Call expected some results but none were returned");
                }
                var passedFlag = true;
                if (result[0].variableName != "Gage height, feet") passedFlag = false;
                if (result[0].variableCode != "NWISDV:00065/statistic=00022") passedFlag = false;
                if (result[0].servCode != "NWISDV") passedFlag = false;
                if (result[0].WSDL != "http://river.sdsc.edu/wateroneflow/NWIS/DailyValues.asmx?WSDL") passedFlag = false;
                if (result[0].conceptCode != "12") passedFlag = false;
                if (passedFlag == false) Assert.Fail("The first MappedVariable did not contain the expected values in one or more of it's attributes");

                if (result[2].variableName != "Gage height, feet") passedFlag = false;
                if (result[2].variableCode != "NWISDV:00065/statistic=30700") passedFlag = false;
                if (result[2].servCode != "NWISDV") passedFlag = false;
                if (result[2].WSDL != "http://river.sdsc.edu/wateroneflow/NWIS/DailyValues.asmx?WSDL") passedFlag = false;
                if (result[2].conceptCode != "12") passedFlag = false;
                if (passedFlag == false) Assert.Fail("The third MappedVariable did not contain the expected values in one or more of it's attributes");
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            Assert.Pass();
        }

        [Test]
        //A and Y - one against few
        public void OneNetworkIDAgainstAFewConceptIds()
        {
            try
            {
                var s = new org.cuahsi.hiscentral.hiscentral();
                var result = s.GetMappedVariables2("12", "1,2,3,4");
                if (!result.Any())
                {
                    Assert.Fail("Call expected some results but none were returned");
                }
                var passedFlag = true;
                if (result[0].variableName != "Stream stage height") passedFlag = false;
                if (result[0].variableCode != "EPA:462-1") passedFlag = false;
                if (result[0].servCode != "EPA") passedFlag = false;
                if (result[0].WSDL != "http://river.sdsc.edu/wateroneflow/EPA/cuahsi_1_0.asmx?WSDL") passedFlag = false;
                if (result[0].conceptCode != "12") passedFlag = false;
                if (passedFlag == false) Assert.Fail("The first MappedVariable did not contain the expected values in one or more of it's attributes");


                if (result[10].variableName != "Gage height, feet") passedFlag = false;
                if (result[10].variableCode != "NWISDV:00065/statistic=31200") passedFlag = false;
                if (result[10].servCode != "NWISDV") passedFlag = false;
                if (result[10].WSDL != "http://river.sdsc.edu/wateroneflow/NWIS/DailyValues.asmx?WSDL") passedFlag = false;
                if (result[10].conceptCode != "12") passedFlag = false;
                if (passedFlag == false) Assert.Fail("The Eleventh MappedVariable did not contain the expected values in one or more of it's attributes");
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            Assert.Pass();
        }
        [Test]

        //A and Z - one against many
        public void OneNetworkIDAgainstManyConceptIds()
        {
            try
            {
                var s = new org.cuahsi.hiscentral.hiscentral();

                var result = s.GetMappedVariables2("16,33,49,51,56,65,67,102", "12");
                if (!result.Any())
                {
                    Assert.Fail("Call expected some results but none were returned");
                }
                var passedFlag = true;
                if (result[0].variableName != "Water level") passedFlag = false;
                if (result[0].variableCode != "ML:USU33") passedFlag = false;
                if (result[0].servCode != "MudLake") passedFlag = false;
                if (result[0].WSDL != "http://icewater.usu.edu/MudLake/cuahsi_1_0.asmx?WSDL") passedFlag = false;
                if (result[0].conceptCode != "16") passedFlag = false;
                if (passedFlag == false) Assert.Fail("The first MappedVariable did not contain the expected values in one or more of it's attributes");


                if (result[1].variableName != "Wind speed") passedFlag = false;
                if (result[1].variableCode != "ML:USU13") passedFlag = false;
                if (result[1].servCode != "MudLake") passedFlag = false;
                if (result[1].WSDL != "http://icewater.usu.edu/MudLake/cuahsi_1_0.asmx?WSDL") passedFlag = false;
                if (result[1].conceptCode != "33") passedFlag = false;
                if (passedFlag == false) Assert.Fail("The Second MappedVariable did not contain the expected values in one or more of it's attributes");
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            Assert.Pass();
        }

        [Test]
        //B and X - few against one 
        public void FewNetworkIDAgainstOneConceptIds()
        {
            try
            {
                var s = new org.cuahsi.hiscentral.hiscentral();
                var result = s.GetMappedVariables2("9, 10, 12, 13, 16, 19", "1");
                if (!result.Any())
                {
                    Assert.Fail("Call expected some results but none were returned");
                }
                var passedFlag = true;
                if (result[0].variableName != "Gage height, feet") passedFlag = false;
                if (result[0].variableCode != "NWISDV:00065/statistic=00022") passedFlag = false;
                if (result[0].servCode != "NWISDV") passedFlag = false;
                if (result[0].WSDL != "http://river.sdsc.edu/wateroneflow/NWIS/DailyValues.asmx?WSDL") passedFlag = false;
                if (result[0].conceptCode != "12") passedFlag = false;
                if (passedFlag == false) Assert.Fail("The first MappedVariable did not contain the expected values in one or more of it's attributes");

                if (result[2].variableName != "Gage height, feet") passedFlag = false;
                if (result[2].variableCode != "NWISDV:00065/statistic=30700") passedFlag = false;
                if (result[2].servCode != "NWISDV") passedFlag = false;
                if (result[2].WSDL != "http://river.sdsc.edu/wateroneflow/NWIS/DailyValues.asmx?WSDL") passedFlag = false;
                if (result[2].conceptCode != "12") passedFlag = false;
                if (passedFlag == false) Assert.Fail("The third MappedVariable did not contain the expected values in one or more of it's attributes");
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            Assert.Pass();
        }

        [Test]
        //B and Y - few against few
        public void FewNetworkIDsAgainstFewConceptIds()
        {
            try
            {
                var s = new org.cuahsi.hiscentral.hiscentral();
                var result = s.GetMappedVariables2("9, 10, 12, 13, 16, 19", "1, 2, 3, 4");
                if (!result.Any())
                {
                    Assert.Fail("Call expected some results but none were returned");
                }
                var passedFlag = true;
                if (result[0].variableName != "Stream stage height") passedFlag = false;
                if (result[0].variableCode != "EPA:462-1") passedFlag = false;
                if (result[0].servCode != "EPA") passedFlag = false;
                if (result[0].WSDL != "http://river.sdsc.edu/wateroneflow/EPA/cuahsi_1_0.asmx?WSDL") passedFlag = false;
                if (result[0].conceptCode != "12") passedFlag = false;
                if (passedFlag == false) Assert.Fail("The first MappedVariable did not contain the expected values in one or more of it's attributes");

                if (result[2].variableName != "Elevation, MSL") passedFlag = false;
                if (result[2].variableCode != "EPA:217-1") passedFlag = false;
                if (result[2].servCode != "EPA") passedFlag = false;
                if (result[2].WSDL != "http://river.sdsc.edu/wateroneflow/EPA/cuahsi_1_0.asmx?WSDL") passedFlag = false;
                if (result[2].conceptCode != "12") passedFlag = false;
                if (passedFlag == false) Assert.Fail("The third MappedVariable did not contain the expected values in one or more of it's attributes");
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            Assert.Pass();
        }

        [Test]
        //B and Z - few against many
        public void FewNetworkIDsAgainstManyConceptIds()
        {
            try
            {
                var s = new org.cuahsi.hiscentral.hiscentral();
                var result = s.GetMappedVariables2("9, 10, 12, 13, 16, 19", "1, 2, 3, 4, 5, 8, 12");
                if (!result.Any())
                {
                    Assert.Fail("Call expected some results but none were returned");
                }
                var passedFlag = true;
                if (result[0].variableName != "Depth to water level, feet below land surface") passedFlag = false;
                if (result[0].variableCode != "NWISGW:72019") passedFlag = false;
                if (result[0].servCode != "NWISGW") passedFlag = false;
                if (result[0].WSDL != "http://river.sdsc.edu/wateroneflow/NWIS/Groundwater.asmx?WSDL") passedFlag = false;
                if (result[0].conceptCode != "19") passedFlag = false;
                if (passedFlag == false) Assert.Fail("The first MappedVariable did not contain the expected values in one or more of it's attributes");

                if (result[2].variableName != "Stream stage height") passedFlag = false;
                if (result[2].variableCode != "EPA:462-1") passedFlag = false;
                if (result[2].servCode != "EPA") passedFlag = false;
                if (result[2].WSDL != "http://river.sdsc.edu/wateroneflow/EPA/cuahsi_1_0.asmx?WSDL") passedFlag = false;
                if (result[2].conceptCode != "12") passedFlag = false;
                if (passedFlag == false) Assert.Fail("The third MappedVariable did not contain the expected values in one or more of it's attributes");
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            Assert.Pass();
        }

        [Test]
        //C and X - many agasinst one 
        public void ManyNetworkIDsAgainstOneConceptIds()
        {
            try
            {
                var s = new org.cuahsi.hiscentral.hiscentral();
                var result = s.GetMappedVariables2("9, 10, 12, 13, 16, 19, 30, 33, 35, 36, 49, 51, 55, 56, 65, 67, 78, 90, 94", "1");
                if (!result.Any())
                {
                    Assert.Fail("Call expected some results but none were returned");
                }
                var passedFlag = true;
                if (result[0].variableName != "Gage height, feet") passedFlag = false;
                if (result[0].variableCode != "NWISDV:00065/statistic=00022") passedFlag = false;
                if (result[0].servCode != "NWISDV") passedFlag = false;
                if (result[0].WSDL != "http://river.sdsc.edu/wateroneflow/NWIS/DailyValues.asmx?WSDL") passedFlag = false;
                if (result[0].conceptCode != "12") passedFlag = false;
                if (passedFlag == false) Assert.Fail("The first MappedVariable did not contain the expected values in one or more of it's attributes");

                if (result[2].variableName != "Gage height, feet") passedFlag = false;
                if (result[2].variableCode != "NWISDV:00065/statistic=30700") passedFlag = false;
                if (result[2].servCode != "NWISDV") passedFlag = false;
                if (result[2].WSDL != "http://river.sdsc.edu/wateroneflow/NWIS/DailyValues.asmx?WSDL") passedFlag = false;
                if (result[2].conceptCode != "12") passedFlag = false;
                if (passedFlag == false) Assert.Fail("The third MappedVariable did not contain the expected values in one or more of it's attributes");
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            Assert.Pass();
        }

        [Test]
        //C and Y - many against few
        public void ManyNetworkIDsAgainstFewConceptIDs()
        {
            try
            {
                var s = new org.cuahsi.hiscentral.hiscentral();
                var result = s.GetMappedVariables2("9, 10, 12, 13, 16, 19, 30, 33, 35, 36, 49, 51, 55, 56, 65, 67, 78, 90, 94", "1, 2, 3, 4");
                if (!result.Any())
                {
                    Assert.Fail("Call expected some results but none were returned");
                }
                var passedFlag = true;
                if (result[0].variableName != "Stream stage height") passedFlag = false;
                if (result[0].variableCode != "EPA:462-1") passedFlag = false;
                if (result[0].servCode != "EPA") passedFlag = false;
                if (result[0].WSDL != "http://river.sdsc.edu/wateroneflow/EPA/cuahsi_1_0.asmx?WSDL") passedFlag = false;
                if (result[0].conceptCode != "12") passedFlag = false;
                if (passedFlag == false) Assert.Fail("The first MappedVariable did not contain the expected values in one or more of it's attributes");

                if (result[2].variableName != "Depth, Secchi disk depth") passedFlag = false;
                if (result[2].variableCode != "EPA:437-1") passedFlag = false;
                if (result[2].servCode != "EPA") passedFlag = false;
                if (result[2].WSDL != "http://river.sdsc.edu/wateroneflow/EPA/cuahsi_1_0.asmx?WSDL") passedFlag = false;
                if (result[2].conceptCode != "94") passedFlag = false;
                if (passedFlag == false) Assert.Fail("The third MappedVariable did not contain the expected values in one or more of it's attributes");
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            Assert.Pass();
        }

        [Test]
        //C and Z - many against many
        public void ManyNetworkIDsAgainstManyConceptIds()
        {
            try
            {
                var s = new org.cuahsi.hiscentral.hiscentral();
                var result = s.GetMappedVariables2("9, 10, 12, 13, 16, 19, 30, 33, 35, 36, 49, 51, 55, 56, 65, 67, 78, 90, 94", "1, 2, 3, 4, 5, 8, 12");
                if (!result.Any())
                {
                    Assert.Fail("Call expected some results but none were returned");
                }
                var passedFlag = true;
                if (result[0].variableName != "STREAM FLOW; INSTANTANEOUS") passedFlag = false;
                if (result[0].variableCode != "CIMS:FLOW_INS") passedFlag = false;
                if (result[0].servCode != "CIMS") passedFlag = false;
                if (result[0].WSDL != "http://eddy.ccny.cuny.edu/CIMS/cuahsi_1_1.asmx?WSDL") passedFlag = false;
                if (result[0].conceptCode != "78") passedFlag = false;
                if (passedFlag == false) Assert.Fail("The first MappedVariable did not contain the expected values in one or more of it's attributes");

                if (result[6].variableName != "Temperature") passedFlag = false;
                if (result[6].variableCode != "ML:USU9") passedFlag = false;
                if (result[6].servCode != "MudLake") passedFlag = false;
                if (result[6].WSDL != "http://icewater.usu.edu/MudLake/cuahsi_1_0.asmx?WSDL") passedFlag = false;
                if (result[6].conceptCode != "49") passedFlag = false;
                if (passedFlag == false) Assert.Fail("The Seventh MappedVariable did not contain the expected values in one or more of it's attributes");
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            Assert.Pass();
        }


        [Test]
        //Invalid NetworkId
        public void IntentionallyInvalidNetworkId()
        {
            try
            {
                var s = new org.cuahsi.hiscentral.hiscentral();
                var result = s.GetMappedVariables2("19", "0");
                if (result.Any())
                {
                    Assert.Fail("Method returned results when none were expected due to passing an intentionally invalid networkId!");
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            Assert.Pass();
        }

        [Test]
        //Invalid ConceptId
        public void IntentionallyInvalidConceptId()
        {
            try
            {
                var s = new org.cuahsi.hiscentral.hiscentral();
                var result = s.GetMappedVariables2("0", "1");
                if (result.Any())
                {
                    Assert.Fail("Method returned results when none were expected due to passing an intentionally invalid conceptId!");
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            Assert.Pass();
        }
        #endregion
    }

    [TestFixture]
    public class GetSeriesCatalogForBox2Test
    {
        [Test]
        public static void CheckIfSendingInvalidInputThrowsError()
        {
            var s = new org.cuahsi.hiscentral.hiscentral();
            try
            {
                var result = s.GetSeriesCatalogForBox2(-81, -71, 31, 51, "pHxx", "5xxx", "1958-5-21xxx", "1958-5-26xxx");
            }
            catch (Exception ex)
            {
                Assert.Pass();  //Notice I pass the test even though an exception occurs. Intentional, because we WANT it to result an error as we supplied invalid input
            }
            Assert.Fail("The method returned valid output even though invalied input was supplied to it!"); //But if no errors occured, something is wrong in the programming of the web service.. so the test should fail
        }

        [Test]

        public static void ValidateResponseAgainstXSDSchema()
        {
            try
            {
                var s = new org.cuahsi.hiscentral.hiscentral();
                var result = s.GetSeriesCatalogForBox2(-80, -70, 30, 50, "pH", "5", "1958-5-21", "1958-5-26");
                var xml = new System.Xml.Serialization.XmlSerializer(result.GetType());
                //(remove old code) var xmlFile = XMLHelpers.SerializeServiceResponseToXMLFile(xml, result, "Response_GetMappedVariables2");
                var xmlFile = XMLHelpers.SerializeServiceResponseToXMLFile(xml, result, " Response_GetSeriesCatalogForBox2");
                //Load the XmlSchemaSet.
                //First obtain the XSD's for each web method. You can use "XSD.EXE" to generate the XSD by passing it an XML file
                //XSD.EXE typical location (may differ for you, Google it): C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX_4.0 Tools\XSD.EXE
                //Reference: http://stackoverflow.com/questions/14022190/is-it-possible-to-generate-xsd-from-asmx
                //Example: I consumed the GetSeriesCatalogForBox2 method (with magic numbers -80, -70, 30, 50, pH, 5,1958-5-21, 1958-5-26 ) and saved the response as an XML file
                //Then I passed it to generate the XSD schema for this method. See "XSD" folder in this solution.

                var schemaSet = new XmlSchemaSet();
                schemaSet.Add("http://hiscentral.cuahsi.org/20100205/", "GetSeriesCatalogForBox2.xsd");

                //A passing test since the live web service is currently returning valid data (toggle comment):
                XMLHelpers.Validate(xmlFile, schemaSet);
                //A demo failed test (toggle comment) where I intentionally added an "x" in the XML file below to corrupt it:
                //XMLHelpers.Validate("InvalidResponse_GetSeriesCatalogForBox2.xml", schemaSet);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            Assert.Pass();
        }


        [Test]
        public static void CheckRawReadIsXML()
        {
            try
            {
                //Grab the response from the web service manually using an HTTP GET

                //(remove old code)//string url = "http://hiscentral.cuahsi.org/webservices/hiscentral.asmx/GetMappedVariables2??xmin=-80&xmax=-70&ymin=30&ymax=50&conceptKeyword=pH&networkIDs=5&beginDate=1958-5-21&endDate=1958-5-26";
                //(remove old code) //string url = "http://sandbox.cuahsi.org/hiscentral/webservices/hiscentral.asmx/GetMappedVariables2??xmin=-80&xmax=-70&ymin=30&ymax=50&conceptKeyword=pH&networkIDs=5&beginDate=1958-5-21&endDate=1958-5-26";
                string url = "http://hiscentral.cuahsi.org/webservices/hiscentral.asmx/GetSeriesCatalogForBox2?xmin=-80&xmax=-70&ymin=30&ymax=50&conceptKeyword=pH&networkIDs=5&beginDate=1958-5-21&endDate=1958-5-26";

                using (WebClient wc = new WebClient())
                {
                    var result = wc.DownloadString(url);
                    var xDoc = XDocument.Parse(result);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail("ERROR: Data recieved is not XML! " + ex.Message);
            }

            Assert.Pass();
        }

        [Test]
        public static void CheckIfResponseIsAsPredicted()
        {
            var s = new org.cuahsi.hiscentral.hiscentral();
            var result = s.GetSeriesCatalogForBox2(-80, -70, 30, 50, "pH", "5", "1958-5-21", "1958-5-26");
            if (!result.Any())
            {
                Assert.Fail("This call expected one or more SeriesRecord in the result, but none were returned");
            }
            if (result.Count() != 65)
            {
                Assert.Fail("This call expected specifically 65 results in the output, but that was not the case");
            }
            var passedFlag = true;
            if (result[0].ServCode != "CIMS") passedFlag = false;
            if (result[0].ServURL != "http://eddy.ccny.cuny.edu/CIMS/cuahsi_1_1.asmx?WSDL") passedFlag = false;
            if (result[0].location != "CIMS:907O") passedFlag = false;
            if (result[0].VarCode != "CIMS:PH") passedFlag = false;
            if (result[0].VarName != "PH CORRECTED FOR TEMPERATURE (25 DEG C)") passedFlag = false;
            if (result[0].beginDate != "5/20/1958 12:00:00 AM") passedFlag = false;
            if (result[0].endDate != "5/27/1958 12:00:00 AM") passedFlag = false;
            if (result[0].ValueCount != 7) passedFlag = false;
            if (result[0].Sitename != "Chesapeake Bay") passedFlag = false;
            if (result[0].latitude != 39.121799468994141) passedFlag = false;
            if (result[0].longitude != -76.303596496582031) passedFlag = false;
            if (result[0].datatype != "Sporadic") passedFlag = false;
            if (result[0].valuetype != "Sample") passedFlag = false;
            if (result[0].samplemedium != "Surface Water") passedFlag = false;
            if (result[0].timeunits != "day") passedFlag = false;
            if (result[0].conceptKeyword != "pH") passedFlag = false;
            if (result[0].TimeSupport != "0") passedFlag = false;
            if (passedFlag == false) Assert.Fail("The first GetSeriesCatalogForBox2 did not contain the expected values in one or more of it's attributes");
            Assert.Pass();
        }

        #region EQUIVALENCE PARTITIONS
        //        Simplified equivalence classes for GetSeriesCatalogForBox2

        //xmin, xmax, ymin, ymax, conceptKeyword, networkIds (1,5,12,19), startdate, enddate

        //one networkdID.. 
        //    - and fixed {xmin, xamx, ymin, ymax} and "no conceptKeyword"

        [Test]
        public void OneNetworkIDAndNoConceptKeyword()
        {
            try
            {
                var s = new org.cuahsi.hiscentral.hiscentral();
                var result = s.GetSeriesCatalogForBox2(-80, -70, 30, 50, String.Empty, "5", "1958-5-21", "1958-5-26");
                if (!result.Any())
                {
                    Assert.Fail("Call expected some results but none were returned");
                }
                var passedFlag = true;
                if (result[0].ServCode != "CIMS") passedFlag = false;
                if (result[0].ServURL != "http://eddy.ccny.cuny.edu/CIMS/cuahsi_1_1.asmx?WSDL") passedFlag = false;
                if (result[0].location != "CIMS:907N") passedFlag = false;
                if (result[0].VarCode != "CIMS:WTEMP") passedFlag = false;
                if (result[0].VarName != "WATER TEMPERATURE") passedFlag = false;
                if (result[0].beginDate != "10/12/1954 12:00:00 AM") passedFlag = false;
                if (result[0].endDate != "7/23/1968 12:00:00 AM") passedFlag = false;
                if (result[0].ValueCount != 549) passedFlag = false;
                if (result[0].Sitename != "Chesapeake Bay") passedFlag = false;
                if (result[0].latitude != 39.1234016418457) passedFlag = false;
                if (result[0].longitude != -76.318000793457031) passedFlag = false;
                if (result[0].datatype != "Sporadic") passedFlag = false;
                if (result[0].valuetype != "Sample") passedFlag = false;
                if (result[0].samplemedium != "Surface Water") passedFlag = false;
                if (result[0].timeunits != "day") passedFlag = false;
                if (result[0].conceptKeyword != "Temperature, water") passedFlag = false;
                if (result[0].TimeSupport != "0") passedFlag = false;
                if (passedFlag == false) Assert.Fail("The first SeriesRecord did not contain the expected values in one or more of it's attributes");

                if (passedFlag == false) Assert.Fail("The 14th MappedVariable did not contain the expected values in one or more of it's attributes");
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            Assert.Pass();
        }

        //    - and fixed (xmin...) and "empty conceptKeyword"

        [Test]
        //    - and fixed (xmin...) and empty startDate
        public void OneNetworkIDAndNoStartDate()
        {
            try
            {
                var s = new org.cuahsi.hiscentral.hiscentral();
                var result = s.GetSeriesCatalogForBox2(-80, -70, 30, 50, "pH", "5", String.Empty, "1958-5-26");
                if (!result.Any())
                {
                    Assert.Fail("Call expected some results but none were returned");
                }
                var passedFlag = true;
                if (result[0].ServCode != "CIMS") passedFlag = false;
                if (result[0].ServURL != "http://eddy.ccny.cuny.edu/CIMS/cuahsi_1_1.asmx?WSDL") passedFlag = false;
                if (result[0].location != "CIMS:907O") passedFlag = false;
                if (result[0].VarCode != "CIMS:PH") passedFlag = false;
                if (result[0].VarName != "PH CORRECTED FOR TEMPERATURE (25 DEG C)") passedFlag = false;
                if (result[0].beginDate != "5/20/1958 12:00:00 AM") passedFlag = false;
                if (result[0].endDate != "5/27/1958 12:00:00 AM") passedFlag = false;
                if (result[0].ValueCount != 7) passedFlag = false;
                if (result[0].Sitename != "Chesapeake Bay") passedFlag = false;
                if (result[0].latitude != 39.121799468994141) passedFlag = false;
                if (result[0].longitude != -76.303596496582031) passedFlag = false;
                if (result[0].datatype != "Sporadic") passedFlag = false;
                if (result[0].valuetype != "Sample") passedFlag = false;
                if (result[0].samplemedium != "Surface Water") passedFlag = false;
                if (result[0].timeunits != "day") passedFlag = false;
                if (result[0].conceptKeyword != "pH") passedFlag = false;
                if (result[0].TimeSupport != "0") passedFlag = false;
                if (passedFlag == false) Assert.Fail("The first SeriesRecord did not contain the expected values in one or more of it's attributes");
                if (passedFlag == false) Assert.Fail("The 14th GetSeriesCatalogForBox2 did not contain the expected values in one or more of it's attributes");
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            Assert.Pass();
        }

        [Test]
        //    - and fixed (xmin...) and empty endDate
        public void OneNetworkIDAndNoEndDate()
        {
            try
            {
                var s = new org.cuahsi.hiscentral.hiscentral();
                var result = s.GetSeriesCatalogForBox2(-80, -70, 30, 50, "pH", "5", "1958-5-26", String.Empty);
                if (!result.Any())
                {
                    Assert.Fail("Call expected some results but none were returned");
                }
                var passedFlag = true;
                if (result[0].ServCode != "CIMS") passedFlag = false;
                if (result[0].ServURL != "http://eddy.ccny.cuny.edu/CIMS/cuahsi_1_1.asmx?WSDL") passedFlag = false;
                if (result[0].location != "CIMS:907O") passedFlag = false;
                if (result[0].VarCode != "CIMS:PH") passedFlag = false;
                if (result[0].VarName != "PH CORRECTED FOR TEMPERATURE (25 DEG C)") passedFlag = false;
                if (result[0].beginDate != "5/20/1958 12:00:00 AM") passedFlag = false;
                if (result[0].endDate != "5/27/1958 12:00:00 AM") passedFlag = false;
                if (result[0].ValueCount != 7) passedFlag = false;
                if (result[0].Sitename != "Chesapeake Bay") passedFlag = false;
                if (result[0].latitude != 39.121799468994141) passedFlag = false;
                if (result[0].longitude != -76.303596496582031) passedFlag = false;
                if (result[0].datatype != "Sporadic") passedFlag = false;
                if (result[0].valuetype != "Sample") passedFlag = false;
                if (result[0].samplemedium != "Surface Water") passedFlag = false;
                if (result[0].timeunits != "day") passedFlag = false;
                if (result[0].conceptKeyword != "pH") passedFlag = false;
                if (result[0].TimeSupport != "0") passedFlag = false;
                if (passedFlag == false) Assert.Fail("The first SeriesRecord did not contain the expected values in one or more of it's attributes");
                if (passedFlag == false) Assert.Fail("The 14th GetSeriesCatalogForBox2 did not contain the expected values in one or more of it's attributes");
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            Assert.Pass();
        }



        [Test]
        //few networkIDs..
        //    - and fixed {xmin, xamx, ymin, ymax} and "no conceptKeyword"
        public void FewNetworkIDSAndNoConceptKeyword()
        {

            {
                var s = new org.cuahsi.hiscentral.hiscentral();
                var result = s.GetSeriesCatalogForBox2(-80, -70, 30, 50, String.Empty, "2,5,8", "1958-5-21", "1958-5-26");
                if (!result.Any())
                {
                    Assert.Fail("Call expected some results but none were returned");
                }

                Assert.AreEqual(result[0].ServCode, "NWISIID");
                Assert.AreEqual(result[0].ServURL, "http://river.sdsc.edu/wateroneflow/NWIS/Data.asmx?WSDL");
                Assert.AreEqual(result[0].location, "NWISIID:351557077354202");
                Assert.AreEqual(result[0].VarCode, "NWISIID:00405");
                Assert.AreEqual(result[0].VarName, "Carbon dioxide, water, unfiltered, milligrams per liter");
                Assert.AreEqual(result[0].beginDate, "5/15/1947 12:00:00 AM");
                Assert.AreEqual(result[0].endDate, "3/30/1966 12:00:00 AM");
                Assert.AreEqual(result[0].ValueCount, 5);
                Assert.AreEqual(result[0].Sitename, "LN-052 CITY OF KINSTON (WELL 2)</");
                Assert.AreEqual(result[0].latitude, 35.2662745);
                Assert.AreEqual(result[0].longitude, -77.5924692);
                Assert.AreEqual(result[0].datatype, "Sporadic");
                Assert.AreEqual(result[0].valuetype, "Sample");
                Assert.AreEqual(result[0].timeunits, "minute");
                Assert.AreEqual(result[0].conceptKeyword, "Carbon dioxide");
                Assert.AreEqual(result[0].TimeSupport, 0);
            }
        }



        [Test]
        //Fixed co-ordinates, empty conceptKeyword, FEW networkIDs
        public static void FewNetworkIDSAndEmptyEndDate()
        {
            var s = new org.cuahsi.hiscentral.hiscentral();
            var result = s.GetSeriesCatalogForBox2(-80, -70, 30, 50, "pH", "2,5,8", "1958-5-21", String.Empty);
            if (!result.Any())
            {
                Assert.Fail("Call expected some results but none were returned");
            }

            Assert.AreEqual(result[0].ServCode, "NWISIID");
            Assert.AreEqual(result[0].ServURL, "http://river.sdsc.edu/wateroneflow/NWIS/Data.asmx?WSDL");
            Assert.AreEqual(result[0].location, "NWISIID:01379530");
            Assert.AreEqual(result[0].VarCode, "NWISIID:00400");
            Assert.AreEqual(result[0].VarName, "pH, water, unfiltered, field, standard units");
            Assert.AreEqual(result[0].beginDate, "1/31/1952 12:00:00 AM");
            Assert.AreEqual(result[0].endDate, "9/16/1998 12:00:00 AM");
            Assert.AreEqual(result[0].ValueCount, 7);
            Assert.AreEqual(result[0].Sitename, "Canoe Brook near Summit NJ");
            Assert.AreEqual(result[0].latitude, 40.74444444);
            Assert.AreEqual(result[0].longitude, -74.3536111);
            Assert.AreEqual(result[0].datatype, "Sproadic");
            Assert.AreEqual(result[0].valuetype, "Sample");
            Assert.AreEqual(result[0].timeunits, "minute");
            Assert.AreEqual(result[0].conceptKeyword, "pH");
            Assert.AreEqual(result[0].TimeSupport, "0");

        }

        [Test]
        //many networkIds....
        //    - and fixed {xmin, xamx, ymin, ymax} and "no conceptKeyword"
        public void ManyNetworkIDSAndNoConceptKeyword()
        {

            var s = new org.cuahsi.hiscentral.hiscentral();
            var result = s.GetSeriesCatalogForBox2(-80, -70, 30, 50, String.Empty, "2,5,8", "1958-5-21", "1958-5-26");

            if (!result.Any())
            {
                Assert.Fail("Call expected some results but none were returned");
            }
            Assert.AreEqual(result[0].ServCode, "NWISIID");
            Assert.AreEqual(result[0].ServURL, "http://river.sdsc.edu/wateroneflow/NWIS/Data.asmx?WSDL");
            Assert.AreEqual(result[0].location, "NWISIID:351557077354202");
            Assert.AreEqual(result[0].VarCode, "NWISIID:00405");
            Assert.AreEqual(result[0].VarName, "Carbon dioxide, water, unfiltered, milligrams per liter");
            Assert.AreEqual(result[0].beginDate, "5/15/1947 12:00:00 AM");
            Assert.AreEqual(result[0].endDate, "3/30/1966 12:00:00 AM");
            Assert.AreEqual(result[0].ValueCount, 5);
            Assert.AreEqual(result[0].Sitename, "LN-052 CITY OF KINSTON (WELL 2)");
            Assert.AreEqual(result[0].latitude, 35.2662745);
            Assert.AreEqual(result[0].longitude, -77.5924692);
            Assert.AreEqual(result[0].datatype, "Sproadic");
            Assert.AreEqual(result[0].valuetype, "Sample");
            Assert.AreEqual(result[0].timeunits, "minute");
            Assert.AreEqual(result[0].conceptKeyword, "Carbon dioxide");
            Assert.AreEqual(result[0].TimeSupport, "0");





        }
        //    - and fixed (xmin...) and "empty conceptKeyword"
        //    - and fixed (xmin...) and empty startDate
        //    - and fixed (xmin...) and empty endDate



        #endregion
    }

    [TestFixture]
    public class GetServicesInBox2Test
    {
        [Test]
        //Find out why this is natively returning a value with the .99 specified (the browser shows no value being returned)
        public static void CheckIfSendingInvalidInputThrowsError()
        {
            var s = new org.cuahsi.hiscentral.hiscentral();
            try
            {
                var result = s.GetServicesInBox2(-170, 85, 150, 89.99); // Added .99 after 89 to fake it
                if (!String.IsNullOrEmpty(result[0].Title))
                {
                    Assert.Fail("The method returned valid output even though invalid input was supplied to it! " + result[0].Title); //But if no errors occured, something is wrong in the programming of the web service.. so the test should fail
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            Assert.Pass();
        }


        [Test]
        public static void CheckRawReadIsXML()
        {
            try
            {
                //Grab the response from the web service manually using an HTTP GET

                string url = "http://hiscentral.cuahsi.org/webservices/hiscentral.asmx/GetServicesInBox2?xmin=-80&xmax=-70&ymin=30&ymax=50&conceptKeyword=pH&networkIDs=5";
                //string url = "http://sandbox.cuahsi.org/hiscentral/webservices/hiscentral.asmx/GetServicesInBox2?xmin=-80&xmax=-70&ymin=30&ymax=50&conceptKeyword=pH&networkIDs=5";

                using (WebClient wc = new WebClient())
                {
                    var result = wc.DownloadString(url);
                    var xDoc = XDocument.Parse(result);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail("ERROR: Data recieved is not XML! " + ex.Message);
            }

            Assert.Pass();
        }

        [Test]
        public static void CheckIfResponseIsAsPredicted()
        {
            var s = new org.cuahsi.hiscentral.hiscentral();
            var result = s.GetServicesInBox2(-170, 85, 150, 89);
            if (!result.Any())
            {
                Assert.Fail("This call expected one or more GetServicesInBox2 in the result, but none were returned");
            }
            if (result.Count() != 1)
            {
                Assert.Fail("This call expected specifically 1 result in the output, but that was not the case");
            }
            var passedFlag = true;
            if (result[0].servURL != "http://river.sdsc.edu/wateroneflow/EPA/cuahsi_1_0.asmx?WSDL") passedFlag = false;
            if (result[0].Title != "EPA STORET") passedFlag = false;
            if (result[0].ServiceDescriptionURL != "http://hiscentral.cuahsi.org/pub_network.aspx?n=4") passedFlag = false;
            if (result[0].Email != "valentin@sdsc.edu") passedFlag = false;
            if (result[0].organization != "EPA") passedFlag = false;
            if (result[0].orgwebsite != "http://www.epa.gov") passedFlag = false;
            if (result[0].citation != "Environmental Protection Agency, STORET") passedFlag = false;
            if (result[0].valuecount != 88828637) passedFlag = false;
            if (result[0].variablecount != 19093) passedFlag = false;
            if (result[0].sitecount != 589065) passedFlag = false;
            if (result[0].ServiceID != 4) passedFlag = false;
            if (result[0].NetworkName != "EPA") passedFlag = false;
            if (result[0].minx != -170.846) passedFlag = false;
            if (result[0].miny != -85.523) passedFlag = false;
            if (result[0].maxx != 150.2273) passedFlag = false;
            if (result[0].maxy != 89.34441) passedFlag = false;

            if (passedFlag == false) Assert.Fail("The first GetServicesInBox2 did not contain the expected values in one or more of it's attributes");
            Assert.Pass();
        }

        #region EQUIVALENCE PARTITION CLASS TESTS
        //One with valid values for all 4 co-ordinates (-80, -70, 30, 50)
        //One each with ONE parameter empty: The promgram won't compile with a missing value when Vis Studio expects a double. Is it okay to skip these?
        #endregion

    }

    [TestFixture]
    public class GetSitesInBox2
    {
        [Test]
        public static void CheckIfSendingInvalidInputThrowsError()
        {
            var s = new org.cuahsi.hiscentral.hiscentral();
            try
            {
                var result = s.GetSitesInBox2(-80, -70, 30, 50, "pH", "5x");
                //magic numbers listed above are partially borrowed from GetSeriesCatalogForBox2
            }
            catch (Exception ex)
            {
                Assert.Pass();  //Notice I pass the test even though an exception occurs. Intentional, because we WANT it to result an error as we supplied invalid input
            }
            Assert.Fail("The method returned valid output even though invalied input was supplied to it!"); //But if no errors occured, something is wrong in the programming of the web service.. so the test should fail
        }

        [Test]
        public static void CheckRawReadIsXML()
        {
            try
            {
                //Grab the response from the web service manually using an HTTP GET

                string url = "http://hiscentral.cuahsi.org/webservices/hiscentral.asmx/GetSitesInBox2?xmin=-80&xmax=-70&ymin=30&ymax=50&conceptKeyword=pH&networkIDs=5";
                //string url = "http://sandbox.cuahsi.org/hiscentral/webservices/hiscentral.asmx/GetSitesInBox2?xmin=-80&xmax=-70&ymin=30&ymax=50&conceptKeyword=pH&networkIDs=5";

                using (WebClient wc = new WebClient())
                {
                    var result = wc.DownloadString(url);
                    var xDoc = XDocument.Parse(result);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail("ERROR: Data recieved is not XML! " + ex.Message);
            }

            Assert.Pass();
        }

        [Test]
        public static void CheckIfResponseIsAsPredicted()
        {
            var s = new org.cuahsi.hiscentral.hiscentral();
            var result = s.GetSitesInBox2(-80, -70, 30, 50, "pH", "5");
            if (!result.Any())
            {
                Assert.Fail("This call expected one or more Site objects in the result, but none were returned");
            }
            if (result.Count() != 608)
            {
                Assert.Fail("This call expected specifically 608 Site results in the output, but that was not the case");
            }
            var passedFlag = true;
            if (result[0].SiteName != "SUSQUEHANNA RIVER AT TOWANDA PA") passedFlag = false;
            if (result[0].SiteCode != "CIMS:1531500") passedFlag = false;
            if (result[0].Latitude != 41.765354156494141) passedFlag = false;
            if (result[0].Longitude != -76.440498352050781) passedFlag = false;
            if (result[0].HUCnumeric != 0) passedFlag = false;
            if (result[0].servCode != "CIMS") passedFlag = false;
            if (result[0].servURL != "http://eddy.ccny.cuny.edu/CIMS/cuahsi_1_1.asmx?WSDL") passedFlag = false;
            if (passedFlag == false) Assert.Fail("The first GetSitesInBox2 did not contain the expected values in one or more of it's attributes");
            Assert.Pass();
        }

        #region EQUIVALENCE CLASSES


        [Test]
        //Fixed co-ordinates, populated conceptKeyword, ONE networkID
        public static void OneNetworkIdAndConceptKeyword()
        {
            var s = new org.cuahsi.hiscentral.hiscentral();
            var result = s.GetSitesInBox2(-80, -70, 30, 50, "pH", "5");
            if (!result.Any())
            {
                Assert.Fail("This call expected one or more Site objects in the result, but none were returned");
            }
            if (result.Count() != 608)
            {
                Assert.Fail("This call expected specifically 608 Site results in the output, but that was not the case");
            }
            var passedFlag = true;
            if (result[0].SiteName != "SUSQUEHANNA RIVER AT TOWANDA PA") passedFlag = false;
            if (result[0].SiteCode != "CIMS:1531500") passedFlag = false;
            if (result[0].Latitude != 41.765354156494141) passedFlag = false;
            if (result[0].Longitude != -76.440498352050781) passedFlag = false;
            if (result[0].HUCnumeric != 0) passedFlag = false;
            if (result[0].servCode != "CIMS") passedFlag = false;
            if (result[0].servURL != "http://eddy.ccny.cuny.edu/CIMS/cuahsi_1_1.asmx?WSDL") passedFlag = false;
            if (passedFlag == false) Assert.Fail("The first GetSitesInBox2 did not contain the expected values in one or more of it's attributes");
            Assert.Pass();
        }




        [Test]
        //Fixed co-ordinates, populated conceptKeyword, FEW networkIDs
        public static void FewNetworkIdsAndConceptKeyword()
        {
            var s = new org.cuahsi.hiscentral.hiscentral();
            var result = s.GetSitesInBox2(-80, -70, 30, 50, "pH", "5,13,65");
            if (!result.Any())
            {
                Assert.Fail("This call expected one or more Site objects in the result, but none were returned");
            }
            if (result.Count() != 608)
            {
                Assert.Fail("This call expected specifically 608 Site results in the output, but that was not the case");
            }

            Assert.AreEqual(result[0].SiteName, "SUSQUEHANNA RIVER AT TOWANDA PA");
            Assert.AreEqual(result[0].SiteCode, "CIMS:1531500");
            Assert.AreEqual(result[0].Latitude, 41.765354156494141);
            Assert.AreEqual(result[0].Longitude, -76.440498352050781);
            Assert.AreEqual(result[0].HUCnumeric, 0);
            Assert.AreEqual(result[0].servCode, "CIMS");
            Assert.AreEqual(result[0].servURL, "http://eddy.ccny.cuny.edu/CIMS/cuahsi_1_1.asmx?WSDL");

        }

        [Test]
        //Fixed co-ordinates, empty conceptKeyword, ONE networkID
        public static void OneNetworkIdsAndNoConceptKeyword()
        {
            var s = new org.cuahsi.hiscentral.hiscentral();
            var result = s.GetSitesInBox2(-80, -70, 30, 50, String.Empty, "5");
            if (!result.Any())
            {
                Assert.Fail("This call expected one or more Site objects in the result, but none were returned");
            }
            if (result.Count() != 853)
            {
                Assert.Fail("This call expected specifically 608 Site results in the output, but that was not the case");
            }

            Assert.AreEqual(result[0].SiteName, "SUSQUEHANNA RIVER AT TOWANDA PA");
            Assert.AreEqual(result[0].SiteCode, "CIMS:1531500");
            Assert.AreEqual(result[0].Latitude, 41.765354156494141);
            Assert.AreEqual(result[0].Longitude, -76.440498352050781);
            Assert.AreEqual(result[0].HUCnumeric, 0);
            Assert.AreEqual(result[0].servCode, "CIMS");
            Assert.AreEqual(result[0].servURL, "http://eddy.ccny.cuny.edu/CIMS/cuahsi_1_1.asmx?WSDL");
        }

        [Test]
        //Fixed co-ordinates, empty conceptKeyword, FEW networkIDs
        public static void FewNetworkIdsAndNoConceptKeyword()
        {
            var s = new org.cuahsi.hiscentral.hiscentral();
            var result = s.GetSitesInBox2(-80, -70, 30, 50, String.Empty, "5,13");
            if (!result.Any())
            {
                Assert.Fail("This call expected one or more Site objects in the result, but none were returned");
            }
            if (result.Count() != 853)
            {
                Assert.Fail("This call expected specifically 608 Site results in the output, but that was not the case");
            }

            Assert.AreEqual(result[0].SiteName, "SUSQUEHANNA RIVER AT TOWANDA PA");
            Assert.AreEqual(result[0].SiteCode, "CIMS:1531500");
            Assert.AreEqual(result[0].Latitude, 41.765354156494141);
            Assert.AreEqual(result[0].Longitude, -76.440498352050781);
            Assert.AreEqual(result[0].HUCnumeric, 0);
            Assert.AreEqual(result[0].servCode, "CIMS");
            Assert.AreEqual(result[0].servURL, "http://eddy.ccny.cuny.edu/CIMS/cuahsi_1_1.asmx?WSDL");


        }

        [Test]
        //Fixed co-ordinates, empty conceptKeyword, FEW networkIDs
        public static void ManyNetworkIdsAndNoConceptKeyword()
        {
            var s = new org.cuahsi.hiscentral.hiscentral();
            var result = s.GetSitesInBox2(-80, -70, 30, 50, String.Empty, "5,13,16");
            if (!result.Any())
            {
                Assert.Fail("This call expected one or more Site objects in the result, but none were returned");
            }
            if (result.Count() != 853)
            {
                Assert.Fail("This call expected specifically 608 Site results in the output, but that was not the case");
            }

            Assert.AreEqual(result[2].SiteName, "WEST BRANCH SUSQUEHANNA RIVER AT LEWISBURG PA");
            Assert.AreEqual(result[2].SiteCode, "CIMS:1553500");
            Assert.AreEqual(result[2].Latitude, 40.9681396484375);
            Assert.AreEqual(result[2].Longitude, -76.873298645019531);
            Assert.AreEqual(result[2].HUCnumeric, 0);
            Assert.AreEqual(result[2].servCode, "CIMS");
            Assert.AreEqual(result[2].servURL, "http://eddy.ccny.cuny.edu/CIMS/cuahsi_1_1.asmx?WSDL");

            //Fixed co-ordinates, empty conceptKeyword, MANY networkIDs
        #endregion


        }
    }
}

