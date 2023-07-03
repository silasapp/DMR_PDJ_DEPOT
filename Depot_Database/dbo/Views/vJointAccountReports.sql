CREATE VIEW [dbo].[vJointAccountReports]
AS
SELECT        dbo.JointAccountReports.Id, dbo.JointAccountReports.JointAccountId, dbo.JointAccountReports.Report, dbo.JointAccountReports.Reportby, dbo.JointAccountReports.ReportDate, dbo.JointAccounts.ApplicationId, 
                         dbo.JointAccounts.Opscon
FROM            dbo.JointAccountReports INNER JOIN
                         dbo.JointAccounts ON dbo.JointAccountReports.JointAccountId = dbo.JointAccounts.Id