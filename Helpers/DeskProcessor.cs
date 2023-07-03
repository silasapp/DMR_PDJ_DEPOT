//using NMDPRA_Depot.Domain.Abstract;
//using NewDepot.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Transactions;
//using System.Web;
//using NewDepot.Models;

//namespace NewDepot.Helpers
//{
//    public class DeskProcessor
//    {
//        IApplication_Desk_HistoryRepository _appDHisRep;
//        IApplication_ProcessingRepository _appProcRep;
//        IUserBranchRepository _usBranchRep;

//        public DeskProcessor(IApplication_Desk_HistoryRepository appD, IApplication_ProcessingRepository appProcRepo,
//            IUserBranchRepository userBranch)
//        {
//            _appDHisRep = appD;
//            _appProcRep = appProcRepo;
//            _usBranchRep = userBranch;
//        }

//        /// <summary>
//        /// Processes the Staff Desk for Approval. Works on 
//        /// 1.  Application_Desk_History
//        /// 2.  Application Processing
//        /// 3.  User Branch
//        /// </summary>
//        /// <param name="appProc"></param>
//        /// <param name="user"></param>
//        /// <param name="ip"></param>
//        /// <returns></returns>
//        public bool Process(Application_Processing appProc, string user, string ip)
//        {
//            //using (var trans = new TransactionScope())
//            //{
//            try
//            {
//                //Application Desk History
//                var apd = new Application_Desk_History();
//                apd.Application_Id = appProc.ApplicationId;
//                apd.Comment = "Approved";
//                apd.UserName = user;
//                //apd.Current_Desk = app.Processor;
//                apd.Date =  UtilityHelper.CurrentTime;
//                apd.Status = ApplicationHistoryStatusModel.Approved;
                

//                _appDHisRep.Add(apd);
//                _appDHisRep.Save(user, ip);

//                //Application Processing
//                appProc.Dateprocessed =  UtilityHelper.CurrentTime;
//                appProc.Processed = true;
                
//                _appProcRep.Edit(appProc);
//                _appProcRep.Save(user, ip);

//                //User Branch
//                var ub = _usBranchRep.FindBy(a => a.Id == appProc.Processor).FirstOrDefault();
//                ub.DeskCount -= 1;
//                _usBranchRep.Edit(ub);
//                _usBranchRep.Save(user, ip);

//                // Getting this far, All is well
//                return true;
//            }
//            catch (Exception ex)
//            {
//                Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
//                // Something went wrong
//                return false;
//            }
//            //}

//        }
//    }
//}