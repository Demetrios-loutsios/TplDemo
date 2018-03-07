using System;
using System.Threading.Tasks.Dataflow;

namespace mega_bank_corp_tpl_demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Create a new MEGA Bank corp ");
            Console.WriteLine("");
            Console.WriteLine("Enter National ID number:");
            var userId = Console.ReadLine();

            var openAccountBlock = new BroadcastBlock<ApplicantDetails>(null);
            openAccountBlock.Post(new ApplicantDetails
            {
                NationalIdNumber = userId
            });
            var creditCheckBlock = new TransformBlock<ApplicantDetails, PerformCheck>(applicantDetails =>
            {
                Console.WriteLine($"Credit check - {applicantDetails.NationalIdNumber}");
                return new PerformCheck
                {
                    Account = applicantDetails,
                    Success = true
                };
            });
            var dbsCheck = new TransformBlock<ApplicantDetails, PerformCheck>(applicantDetails =>
            {
                Console.WriteLine($"DBS Check- {applicantDetails.NationalIdNumber}");
                return new PerformCheck
                {
                    Account = applicantDetails,
                    Success = true
                };
            });
            var sendEmailSuccessAction = new TransformBlock<ApplicantDetails, ApplicantDetails>(applicantDetails =>
            {
                Console.WriteLine($"Send Email (Success) - {applicantDetails.NationalIdNumber}");
                return applicantDetails;
            });
            var sendPostSuccessActionBlock = new TransformBlock<ApplicantDetails, ApplicantDetails>(applicantDetails =>
            {
                Console.WriteLine($"Send Post (Success) - {applicantDetails.NationalIdNumber}");
                return applicantDetails;
            });
            var notifyCardIssueDeptAction = new TransformBlock<ApplicantDetails, ApplicantDetails>(applicantDetails =>
            {
                Console.WriteLine($"Notify card issue department - {applicantDetails.NationalIdNumber}");
                return applicantDetails;
            });
            var openAccount = new TransformBlock<ApplicantDetails, ApplicantDetails>(applicantDetails =>
            {
                Console.WriteLine($"openeing account- {applicantDetails.NationalIdNumber}");

                sendEmailSuccessAction.Post(applicantDetails);
                sendPostSuccessActionBlock.Post(applicantDetails);
                notifyCardIssueDeptAction.Post(applicantDetails);
                
                return applicantDetails;
            });
            var checksCompletedBlock = new JoinBlock<PerformCheck, PerformCheck>(new GroupingDataflowBlockOptions { Greedy = false });
            var checksDesicionBlock = new ActionBlock<Tuple<PerformCheck, PerformCheck>>(tuple =>
            {
                Console.WriteLine($"Checks completed - {tuple.Item1.Account.NationalIdNumber}");

                if (tuple.Item1.Success && tuple.Item2.Success)
                {
                    openAccount.Post(tuple.Item1.Account);
                }
            });
            
            var postAccountOpenedActionsBlock = new JoinBlock<ApplicantDetails, ApplicantDetails, ApplicantDetails>(new GroupingDataflowBlockOptions { Greedy = false});
            var audidCompletedBlock = new ActionBlock<Tuple<ApplicantDetails, ApplicantDetails, ApplicantDetails>>(tuple =>
            {
                Console.WriteLine($"Audit completed successfully - {tuple.Item1.NationalIdNumber}");
            });
            
            openAccountBlock.LinkTo(creditCheckBlock, new DataflowLinkOptions());
            openAccountBlock.LinkTo(dbsCheck, new DataflowLinkOptions());
            
            creditCheckBlock.LinkTo(checksCompletedBlock.Target1); // first input of Join block
            dbsCheck.LinkTo(checksCompletedBlock.Target2); // second input of Join block
            checksCompletedBlock.LinkTo(checksDesicionBlock);

            sendEmailSuccessAction.LinkTo(postAccountOpenedActionsBlock.Target1);
            sendPostSuccessActionBlock.LinkTo(postAccountOpenedActionsBlock.Target2);
            notifyCardIssueDeptAction.LinkTo(postAccountOpenedActionsBlock.Target3);
            
            postAccountOpenedActionsBlock.LinkTo(audidCompletedBlock);
            
            Console.ReadKey();
        }
    }
}
