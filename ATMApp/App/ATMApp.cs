using System;
using System.Collections.Generic;
using ATMApp.Domain.Entities;
using ATMApp.Domain.Enums;
using ATMApp.Domain.Interfaces;
using ATMApp.UI;

namespace ATMApp.App
{
    public class AtmApp : IUserLogin, IUserAccountActions, ITransaction
    {
        private List<UserAccount> userAccountList;
        private UserAccount selectedAccount;

        public void Run()
        {
            AppScreen.Welcome();
            CheckUserCardNumAndPassword();
            AppScreen.WelcomeCustomer(selectedAccount.FullName);
            AppScreen.DisplayAppMenu();
            ProcessMenuOptions();
        }

        public void InitializeData()
        {
            userAccountList = new List<UserAccount>
            {
                new UserAccount{Id = 1, FullName = "Iyiegbu Kosi", AccountNumber = 123456, CardNumber = 321321, CardPin = 123123, AccountBalance = 50000.00m, IsLocked = false},
                new UserAccount{Id = 2, FullName = "Alex Nwafor", AccountNumber = 456789, CardNumber = 654654, CardPin = 456456, AccountBalance = 4000.00m, IsLocked = false},
                new UserAccount{Id = 3, FullName = "Rene Emezi", AccountNumber = 123555, CardNumber = 987987, CardPin = 789789, AccountBalance = 2000.00m, IsLocked = true},
            };
        }

        public void CheckUserCardNumAndPassword()
        {
            bool isCorrectLogin = false;

            while (isCorrectLogin == false)
            {
                UserAccount inputAccount = AppScreen.UserLoginForm();
                AppScreen.LoginProcess();
                foreach (UserAccount account in userAccountList)
                {
                    selectedAccount = account;
                    if (inputAccount.CardNumber.Equals(selectedAccount.CardNumber))
                    {
                        selectedAccount.TotalLogin++;

                        if (inputAccount.CardPin.Equals(selectedAccount.CardPin))
                        {
                            selectedAccount = account;

                            if (selectedAccount.IsLocked || selectedAccount.TotalLogin > 3)
                            {
                                AppScreen.PrintLockScreen();
                            }
                            else
                            {
                                selectedAccount.TotalLogin = 0;
                                isCorrectLogin = true;
                                break;
                            }
                        }
                    }
                    if (isCorrectLogin == false)
                    {
                        Utility.PrintMessage("\n Invalid card number or PIN", false);
                        selectedAccount.IsLocked = selectedAccount.TotalLogin == 3;
                        if (selectedAccount.IsLocked)
                        {
                            AppScreen.PrintLockScreen();
                        }
                    }
                    Console.Clear();
                }
            }
        }

        private void ProcessMenuOptions()
        {
            switch (Validator.Convert<int>("an option:"))
            {
                case (int)AppMenu.CheckBalance:
                    CheckBalance();
                    break;
                case (int)AppMenu.PlaceDesposit:
                    Console.WriteLine("Placing deposit...");
                    break;
                case (int)AppMenu.MakeWithdrawal:
                    Console.WriteLine("Making withdrawal...");
                    break;
                case (int)AppMenu.InternalTransfer:
                    Console.WriteLine("Making internal transfer...");
                    break;
                case (int)AppMenu.ViewTransaction:
                    Console.WriteLine("Viewing transactions...");
                    break;
                case (int)AppMenu.Logout:
                    AppScreen.LogoutProgress();
                    Utility.PrintMessage("You have successfully logged out, Please collect your ATM Card", true);
                    Run();
                    break;
                default:
                    Utility.PrintMessage("Invalid Option.", false);
                    break;
            }
        }

        public void CheckBalance()
        {
            Utility.PrintMessage($"Your account blance is: {Utility.FormatAmount(selectedAccount.AccountBalance)}");
        }

        public void PlaceDeposit()
        {
            Console.WriteLine("\nOnly multiples of 500 and 1000 naira allowed. \n");
            var transaction_amt = Validator.Convert<int>($"amount {AppScreen.currency}");

            //simulate counting
            Console.WriteLine("\nChecking and Counting bank notes.");
            Utility.PrintDotAnimation();
            Console.WriteLine("");

            //some guard clause
            if (transaction_amt <= 0)
            {
                Utility.PrintMessage("Amount needs to be greater than zero. Try again", false);
                return;
            }
            if (transaction_amt % 500 != 0)
            {
                Utility.PrintMessage("Enter deposit in multiples of 500 or 1000. Try again", false);
                return;
            }
            if (PreviewBankNotesCount(transaction_amt) == false)
            {
                Utility.PrintMessage("You have cancelled your action.", false);
                return;
            }


        }

        public void MakeWithdrawal()
        {
            throw new NotImplementedException();
        }

        private bool PreviewBankNotesCount(int amount)
        {
            int thousandNotesCount = amount / 1000;
            int fiveHundredNotesCount = (amount % 1000) / 500;

            Console.WriteLine("\nSummary");
            Console.WriteLine("------");
            Console.WriteLine($"{AppScreen.currency}1000 X {thousandNotesCount} = {1000 * thousandNotesCount}");
            Console.WriteLine($"{AppScreen.currency}500 X {fiveHundredNotesCount} = {500 * fiveHundredNotesCount}");
            Console.WriteLine($"Total amount: {Utility.FormatAmount(amount)}\n\n");

            int option = Validator.Convert<int>("1 to confirm");
            return option.Equals(1);
        }

        public void InsertTransaction(long _UserBankAccountId, TransactionType _tranType, decimal _tranAmount, string _description)
        {
            throw new NotImplementedException();
        }

        public void ViewTransaction()
        {
            throw new NotImplementedException();
        }
    }
}