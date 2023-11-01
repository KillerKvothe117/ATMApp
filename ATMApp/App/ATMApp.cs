using System;
using System.Collections.Generic;
using ATMApp.Domain.Entities;
using ATMApp.Domain.Enums;
using ATMApp.Domain.Interfaces;
using ATMApp.UI;

namespace ATMApp.App
{
    public class AtmApp : IUserLogin, IUserAccountActions
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
            throw new NotImplementedException();
        }

        public void MakeWithdrawal()
        {
            throw new NotImplementedException();
        }
    }
}