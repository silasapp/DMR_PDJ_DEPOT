CREATE VIEW [dbo].[vJointAccountStaffs]
AS
SELECT        dbo.JointAccounts.ApplicationId, dbo.JointAccounts.Opscon, dbo.JointAccounts.DateAdded, dbo.JointAccounts.OperationsCompleted, dbo.JointAccountStaffs.Staff, dbo.JointAccountStaffs.DateAdded AS DateStaffAdded, 
                         dbo.JointAccountStaffs.Id AS JAS_Id, dbo.JointAccountStaffs.JointAccountId, dbo.Staffs.FirstName, dbo.Staffs.LastName, dbo.JointAccountStaffs.SignedOff, dbo.JointAccounts.Assigned
FROM            dbo.JointAccounts LEFT OUTER JOIN
                         dbo.JointAccountStaffs ON dbo.JointAccounts.Id = dbo.JointAccountStaffs.JointAccountId INNER JOIN
                         dbo.Staffs ON dbo.JointAccounts.Opscon = dbo.Staffs.Email