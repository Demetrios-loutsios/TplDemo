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

            var openAccountBlock = new BroadcastBlock<CreateAccount>(null);
            openAccountBlock.Post(new CreateAccount
            {
                NationalIdNumber = userId
            });
            var creditCheckBlock = new TransformBlock<CreateAccount, PerformCheck>(createAccount =>
            {
                Console.WriteLine($"Credit check - {createAccount.NationalIdNumber}");
                return new PerformCheck
                {
                    Account = createAccount,
                    Success = true
                };
            });
            var dbsCheck = new TransformBlock<CreateAccount, PerformCheck>(createAccount =>
            {
                Console.WriteLine($"DBS Check- {createAccount.NationalIdNumber}");
                return new PerformCheck
                {
                    Account = createAccount,
                    Success = true
                };
            });
            var sendEmailSuccessAction = new TransformBlock<CreateAccount, CreateAccount>(createAccount =>
            {
                Console.WriteLine($"Send Email (Success) - {createAccount.NationalIdNumber}");
                return createAccount;
            });
            var sendPostSuccessActionBlock = new TransformBlock<CreateAccount, CreateAccount>(createAccount =>
            {
                Console.WriteLine($"Send Post (Success) - {createAccount.NationalIdNumber}");
                return createAccount;
            });
            var notifyCardIssueDeptAction = new TransformBlock<CreateAccount, CreateAccount>(createAccount =>
            {
                Console.WriteLine($"Notify card issue department - {createAccount.NationalIdNumber}");
                return createAccount;
            });
            var openAccount = new TransformBlock<CreateAccount, CreateAccount>(createAccount =>
            {
                Console.WriteLine($"openeing account- {createAccount.NationalIdNumber}");

                sendEmailSuccessAction.Post(createAccount);
                sendPostSuccessActionBlock.Post(createAccount);
                notifyCardIssueDeptAction.Post(createAccount);
                
                return createAccount;
            });
            var checksCompletedBlock = new JoinBlock<PerformCheck, PerformCheck>(new GroupingDataflowBlockOptions { Greedy = false });
            var checksDesicionBlock = new ActionBlock<Tuple<PerformCheck, PerformCheck>>(tuple =>
            {
                Console.WriteLine("Checks completed");

                if (tuple.Item1.Success && tuple.Item2.Success)
                {
                    openAccount.Post(tuple.Item1.Account);
                }
            });
            
            
            var postAccountOpenedActionsBlock = new JoinBlock<CreateAccount, CreateAccount, CreateAccount>(new GroupingDataflowBlockOptions { Greedy = false});
          
            openAccountBlock.LinkTo(creditCheckBlock, new DataflowLinkOptions());
            openAccountBlock.LinkTo(dbsCheck, new DataflowLinkOptions());
            
            creditCheckBlock.LinkTo(checksCompletedBlock.Target1); // first input of Join block
            dbsCheck.LinkTo(checksCompletedBlock.Target2); // second input of Join block
            checksCompletedBlock.LinkTo(checksDesicionBlock);

            sendEmailSuccessAction.LinkTo(postAccountOpenedActionsBlock.Target1);
            sendPostSuccessActionBlock.LinkTo(postAccountOpenedActionsBlock.Target2);
            notifyCardIssueDeptAction.LinkTo(postAccountOpenedActionsBlock.Target3);
            postAccountOpenedActionsBlock.LinkTo(new ActionBlock<Tuple<CreateAccount, CreateAccount, CreateAccount>>(tuuple =>
            {
                Console.WriteLine("Audit completed successfully");
            }));
            
            Console.ReadKey();
        }
    }
}
