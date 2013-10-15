using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProcessTests.DataAccess;
using UnitTests;
using System.Configuration;
using ProcessTests.Models;
using System.Text;


namespace ProcessTests.Controllers {
    public class TestsController : Controller {
        private static List<String> KnownErrorMessages {
            get {
                var errors = new List<string>();
                //Keep adding known issues here
                errors.Add("Cannot retrieve units or variables from database");
                //errors.Add("Variable Not Found");
                return errors;
            }
        }
        
        public ActionResult Index() {
            return View();
        }

        [HttpPost]
        public JsonResult PerformLogin(int howManyDaysHistory) {
            var result = new vmAjaxResult { ErrorCode = 3467, ErrorMessage = "You need to sign in!" };
            try {
                //If login successful or if the session object is already aware of the users identity, pull this users' history
                if (User.Identity.IsAuthenticated || Session["loggedInUsername"].ToString() != null) {
                    if (!String.IsNullOrEmpty(User.Identity.Name)) {
                        Session["loggedInUsername"] = User.Identity.Name;
                    }
                    var history = DataAccess.DataAccess.LookupTestResultByUsername(Session["loggedInUsername"].ToString(), howManyDaysHistory);
                    history.ForEach(x => {
                        x.GetValuesResult = String.Empty;
                    });
                    var js = new System.Web.Script.Serialization.JavaScriptSerializer();
                    result.Data = js.Serialize(history);
                    result.ErrorCode = 0;
                }
            } catch (Exception ex) {
                result.ErrorCode = 5256;
                result.ErrorMessage = Server.HtmlEncode(String.Format("ERROR #{0}: {1}", result.ErrorCode, ex.Message));
            }
            return Json(result);
        }

        [HttpPost]
        public JsonResult SubmitWSDLAndSeed(string url, string seed) {
            var result = new vmAjaxResult();
            try {
                int cleanSeed;
                if (Int32.TryParse(seed, out cleanSeed) && !String.IsNullOrEmpty(url)) {
                    Random random = new Random(cleanSeed);
                    var limitBoundRandom = (cleanSeed > 20000) ? (random.Next() % 20000) + 1 : cleanSeed;
                    //Next, using this range bound seed + url, figure out all we need to be able to call GetValues()... and present summary
                    var js = new System.Web.Script.Serialization.JavaScriptSerializer();
                    result.Data = js.Serialize(MultiEndPoint.GenerateTest_1_1(url, limitBoundRandom));
                    result.ErrorCode = 0;
                } else {
                    result.ErrorCode = 1000;
                    result.ErrorMessage = "Invalid seed or web service URL, please try again with a different value!";
                }
                return Json(result);
            } catch (Exception ex) {
                result.ErrorCode = 5000;
                result.ErrorMessage = Server.HtmlEncode(String.Format("ERROR #{0}: {1}", result.ErrorCode, ex.Message));
                return Json(result);
            }
        }

        [HttpPost]
        public JsonResult RunTest(string summaryString) {
            var result = new vmAjaxResult();
            try {
                var confirmation = MultiEndPoint.GetValuesWrapper(summaryString, User.Identity.Name);
                if (String.IsNullOrEmpty(confirmation)) {
                    result.ErrorCode = 5093;
                    result.ErrorMessage = "Test results may not have been written into the database archive successfully! Please try again!";
                } else {
                    result.ErrorCode = 0;
                    result.Data = confirmation;
                }
            } catch (Exception ex) {
                result.ErrorCode = 5001;
                result.ErrorMessage = Server.HtmlEncode(String.Format("ERROR #{0}: {1}", result.ErrorCode, ex.Message));
            }
            return Json(result);
        }

        [HttpPost]
        public JsonResult RunAll() {
            var result = new vmAjaxResult();
            try {
                //Only admins can use this feature
                if (!MultiEndPoint.AdminUsers.Contains(Session["loggedInUsername"].ToString())) throw new Exception("This user is not authorized to perform this task!");

                //Start looping over each WSDL in the Hisnetworks catalog
                var allNetworks = DataAccess.DataAccess.GetAllHisNetworksFromDatabase();
                foreach (var n in allNetworks) {
                    try {
                        if (!String.IsNullOrEmpty(n.ServiceWSDL) && n.ServiceWSDL.Contains("1_1")) {
                            //Take the WSDL we picked from the database and use a 0 seed always, to obtain the generated test
                            var generatedTest = MultiEndPoint.GenerateTest_1_1(n.ServiceWSDL, 0);
                            //Call runTest with this generatedTest instance
                            var getValues = MultiEndPoint.GetValues(n.ServiceWSDL, 0, generatedTest.randomSite, generatedTest.randomVariable, generatedTest.randomSiteInfo);
                            //Dump the result of this instance into the database
                            DataAccess.DataAccess.InsertTestResult(new ProcessTests.DataAccess.TestArchive { Id = -1, Timestamp = DateTime.Now, WSDL = n.ServiceWSDL, Seed = 0, GetValuesResult = getValues, Username = User.Identity.Name });
                        }
                    } catch (Exception e) {
                        if (KnownErrorMessages.Contains(e.Message)) {
                            //move on to next site
                            continue;
                        } else {
                            throw new Exception(e.Message);
                        }
                    }
                }   //end of loop
                result.ErrorCode = 0;
                result.Data = "Run all successful!";
            } catch (Exception ex) {
                result.ErrorCode = 5621;
                result.ErrorMessage = Server.HtmlEncode(String.Format("ERROR #{0}: {1}", result.ErrorCode, ex.Message));
            }
            return Json(result);
        }

        [HttpPost]
        public JsonResult CompareTest(int lookupType, string value1, string value2) {
            var result = new vmAjaxResult();
            try {
                DataAccess.TestArchive archive = new DataAccess.TestArchive();
                var limitBoundRandom = 0;
                switch (lookupType) {
                    default: { break; }
                    case 1: {
                            var wsdl = value1;
                            var seed = Convert.ToInt32(value2);
                            Random random = new Random(seed);
                            limitBoundRandom = (seed > 20000) ? (random.Next() % 20000) + 1 : seed;
                            archive = DataAccess.DataAccess.LookupTestResultByWSDLAndSeed(wsdl, limitBoundRandom);
                            break;
                        }
                    case 2: {
                            var confirmationNo = value1;
                            archive = DataAccess.DataAccess.LookupTestResultByConfirmationNo(Guid.Parse(confirmationNo));
                            break;
                        }
                }
                if (archive.Id > 0) {
                    //At this point, an arhived object was found. Thus, we should proceed to run the test against the WSDL and seed this archive belongs to!
                    var archivedDump = archive.GetValuesResult;
                    //Next, using this range bound seed + url, figure out all we need to be able to call GetValues()... and present summary
                    var js = new System.Web.Script.Serialization.JavaScriptSerializer();
                    var generatedTest = js.Serialize(MultiEndPoint.GenerateTest_1_1(archive.WSDL, limitBoundRandom));
                    var todaysResult = MultiEndPoint.GetValuesForComparison(generatedTest);
                    //Now we want to compare the previous outcome with the new one and let the user know if they match or not...

                    //54 characters 
                    var index1 = archivedDump.IndexOf("<creationTime>");
                    var index2 = todaysResult.IndexOf("<creationTime>");

                    //StringBuilder sbArchivedDump = new StringBuilder(archivedDump);
                    //StringBuilder sbTodaysResult = new StringBuilder(todaysResult);

                    //var archivedCreationTime = archivedDump.Substring(index1, 54);
                    //sbArchivedDump.Remove(index1, 54);

                    //sbTodaysResult.Remove(index2, 54);
                    //sbTodaysResult.Insert(index2, archivedCreationTime);

                    //archivedDump = sbArchivedDump.ToString();
                    //todaysResult = sbTodaysResult.ToString();


                    archivedDump = archivedDump.TrimStart('"');
                    archivedDump = archivedDump.TrimEnd('"');
                    todaysResult = todaysResult.TrimStart('"');
                    todaysResult = todaysResult.TrimEnd('"');
                    var compareResult = ConsoleAppForTesting.XMLComparisons.TestForEqualityByXMLString(todaysResult, archivedDump);

                    if (compareResult == "") {
                        result.ErrorCode = 0;
                    } else {
                        result.ErrorCode = 5736;
                        //result.ErrorMessage = "Today's values returned by the web service DO NOT match with the archived values!";
                        result.ErrorMessage = compareResult;
                    }
                } else {
                    result.ErrorCode = 5268;
                    result.ErrorMessage = "An archived object meeting your search constraints was not found!";
                }
            } catch (Exception ex) {
                result.ErrorCode = 5027;
                result.ErrorMessage = Server.HtmlEncode(String.Format("ERROR #{0}: {1}", result.ErrorCode, ex.Message));
            }
            return Json(result);
        }

    }
}
