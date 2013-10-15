using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProcessTests.DataAccess {
    public static class DataAccess {
        private static WaterOneFlowTestArchiveEntities db = new WaterOneFlowTestArchiveEntities();
        private static hiscentralEntities dbHiscentral = new hiscentralEntities();

        public static string InsertTestResult(TestArchive ta) {
            ta.ConfirmationNo = Guid.NewGuid();
            db.TestArchives.Add(ta);
            var result = db.SaveChanges();
            if (result > 0) {
                return ta.ConfirmationNo.ToString();
            } else {
                return String.Empty;
            }
        }

        public static TestArchive LookupTestResultByConfirmationNo(Guid confirmationNo) {
            return db.TestArchives.SingleOrDefault(x => x.ConfirmationNo == confirmationNo);
        }

        public static TestArchive LookupTestResultByWSDLAndSeed(string wsdl, int seed) {
            var result = db.TestArchives.Where(x => x.WSDL == wsdl && x.Seed == seed);
            if (result.Count() > 1) {
                return result.OrderByDescending(x => x.Timestamp).FirstOrDefault();
            } else if (result.Count() == 1) {
                return result.FirstOrDefault();
            } else {
                return new TestArchive();
            }
        }

        public static List<TestArchive> LookupTestResultByUsername(string username, int howManyDaysBack) {
            var days = -1 * howManyDaysBack;
            var allHistory = db.TestArchives.Where(x => x.Username == username).ToList();
            howManyDaysBack = -1 * howManyDaysBack;
            var filterDate = DateTime.Today.AddDays(howManyDaysBack);
            var filteredHistory = allHistory.Where(x => x.Timestamp >= filterDate).ToList();
            return filteredHistory;
        }

        public static List<HISNetwork> GetAllHisNetworksFromDatabase() {
            //WHERE IsPublic=1 and IsApproved=1 
            //                    and ServiceWSDL like '%cuahsi_1_1_%'
            //                    and ServiceWSDL not like '%sdsc%' 
            //                    and ServiceWSDL not like '%cuahsi.org%'"
            var all = dbHiscentral.HISNetworks.ToList();
            var filtered = all.Where(x => x.IsPublic == true &&
                        x.IsApproved == true &&
                        x.ServiceWSDL.Contains("cuahsi_1_1") &&
                        !x.ServiceWSDL.Contains("sdsc") &&
                        !x.ServiceWSDL.Contains("cuahsi.org"))
            .ToList();
            return filtered;
        }
    }
}