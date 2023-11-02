using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleTables;
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
        private List<Transaction> _listOfTransactions;
        private const decimal minimumKeptAmount = 500m;
        private readonly AppScreen screen;

        public AtmApp()
        {
            screen = new AppScreen();
        }


        public void Run()
        {
            AppScreen.Welcome();
            CheckUserCardNumAndPassword();
            AppScreen.WelcomeCustomer(selectedAccount.FullName);

            while (true)
            {
                AppScreen.DisplayAppMenu();
                ProcessMenuOptions();
            }
        }

        public void InitializeData()
        {
            userAccountList = new List<UserAccount>
            {
                new UserAccount{Id = 1, FullName = "Iyiegbu Kosi", AccountNumber = 123456, CardNumber = 321321, CardPin = 123123, AccountBalance = 50000.00m, IsLocked = false},
                new UserAccount{Id = 2, FullName = "Alex Nwafor", AccountNumber = 456789, CardNumber = 654654, CardPin = 456456, AccountBalance = 4000.00m, IsLocked = false},
                new UserAccount{Id = 3, FullName = "Rene Emezi", AccountNumber = 123555, CardNumber = 987987, CardPin = 789789, AccountBalance = 2000.00m, IsLocked = true},
            };

            _listOfTransactions = new List<Transaction>();
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
                    PlaceDeposit();
                    break;
                case (int)AppMenu.MakeWithdrawal:
                    MakeWithdrawal();
                    break;
                case (int)AppMenu.InternalTransfer:
                    var internalTransfer = screen.InternalTransferForm();
                    ProcessInternalTransfer(internalTransfer);
                    break;
                case (int)AppMenu.ViewTransaction:
                    ViewTransaction();
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

            //bind transaction details to transaction object
            InsertTransaction(selectedAccount.Id, TransactionType.Deposit, transaction_amt, "");

            //update account balance
            selectedAccount.AccountBalance += transaction_amt;

            Utility.PrintMessage($"Your deposit of {Utility.FormatAmount(transaction_amt)} was successful", true);

        }

        public void MakeWithdrawal()
        {
            var transaction_amt = 0;
            int selectedAmount = AppScreen.SelectAmount();
            if (selectedAmount == -1)
            {
                MakeWithdrawal();
                return;
            }
            else if (selectedAmount != 0)
            {
                transaction_amt = selectedAmount;
            }
            else
            {
                transaction_amt = Validator.Convert<int>($"amount {AppScreen.currency}");
            }

            //input validation
            if (transaction_amt <= 0)
            {
                Utility.PrintMessage("Amount needs to be greater than zero. Try agin", false);
                return;
            }
            if (transaction_amt % 500 != 0)
            {
                Utility.PrintMessage("You can only withdraw amount in multiples of 500 or 1000 naira. Try again.", false);
                return;
            }
            //Business logic validations

            if (transaction_amt > selectedAccount.AccountBalance)
            {
                Utility.PrintMessage($"Withdrawal failed. Your balance is too low to withdraw" +
                    $"{Utility.FormatAmount(transaction_amt)}", false);
                return;
            }
            if ((selectedAccount.AccountBalance - transaction_amt) < minimumKeptAmount)
            {
                Utility.PrintMessage($"Withdrawal failed. Your account needs to have " +
                    $"minimum {Utility.FormatAmount(minimumKeptAmount)}", false);
                return;
            }

            //Bind withdrawal details to transaction object
            InsertTransaction(selectedAccount.Id, TransactionType.Withdrawal, -transaction_amt, "");
            //update account balance
            selectedAccount.AccountBalance -= transaction_amt;
            //success message
            Utility.PrintMessage($"You have successfully withdrawn " +
                $"{Utility.FormatAmount(transaction_amt)}.", true);

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
            var transaction = new Transaction()
            {
                TransactionId = Utility.GetTransactionId(),
                UserBankAccountId = _UserBankAccountId,
                TransactionDate = DateTime.Now,
                TransactionType = _tranType,
                TransactionAmount = _tranAmount,
                Description = _description,
            };

            //add transaction object to list
            _listOfTransactions.Add(transaction);
        }

        public void ViewTransaction()
        {
            var filteredTransactionList = _listOfTransactions.Where(t => t.UserBankAccountId == selectedAccount.Id).ToList();
            //check if there's a transaction
            if (filteredTransactionList.Count <= 0)
            {
                Utility.PrintMessage("You have no transaction yet.", true);
            }
            else
            {
                var table = new ConsoleTable("Id", "Transaction Date", "Type", "Descriptions", "Amount " + AppScreen.currency);
                foreach (var tran in filteredTransactionList)
                {
                    table.AddRow(tran.TransactionId, tran.TransactionDate, tran.TransactionType, tran.Description, tran.TransactionAmount);
                }
                table.Options.EnableCount = false;
                table.Write();
                Utility.PrintMessage($"You have {filteredTransactionList.Count} transaction(s)", true);
            }
        }

        private void ProcessInternalTransfer(InternalTransfer internalTransfer)
        {
            if (internalTransfer.TransferAmount <= 0)
            {
                Utility.PrintMessage("Amount needs to be more than zero. Try again.", false);
                return;
            }
            //check sender's account balance
            if (internalTransfer.TransferAmount > selectedAccount.AccountBalance)
            {
                Utility.PrintMessage($"Transfer failed. You do not hav enough balance" +
                    $" to transfer {Utility.FormatAmount(internalTransfer.TransferAmount)}", false);
                return;
            }
            //check the minimum kept amount 
            if ((selectedAccount.AccountBalance - internalTransfer.TransferAmount) < minimumKeptAmount)
            {
                Utility.PrintMessage($"Transfer failed. Your account needs to have minimum" +
                    $" {Utility.FormatAmount(minimumKeptAmount)}", false);
                return;
            }

            //check reciever's account number is valid
            var selectedBankAccountReciever = (from userAcc in userAccountList
                                               where userAcc.AccountNumber == internalTransfer.RecipientBankAccountNumber
                                               select userAcc).FirstOrDefault();
            if (selectedBankAccountReciever == null)
            {
                Utility.PrintMessage("Transfer failed. Recieber bank account number is invalid.", false);
                return;
            }
            //check receiver's name
            if (selectedBankAccountReciever.FullName != internalTransfer.RecipientBankAccountName)
            {
                Utility.PrintMessage("Transfer Failed. Recipient's bank account name does not match.", false);
                return;
            }

            //add transaction to transactions record- sender
            InsertTransaction(selectedAccount.Id, TransactionType.Transfer, -internalTransfer.TransferAmount, "Transfered " +
                $"to {selectedBankAccountReciever.AccountNumber} ({selectedBankAccountReciever.FullName})");
            //update sender's account balance
            selectedAccount.AccountBalance -= internalTransfer.TransferAmount;

            //add transaction record-reciever
            InsertTransaction(selectedBankAccountReciever.Id, TransactionType.Transfer, internalTransfer.TransferAmount, "Transfered from " +
                $"{selectedAccount.AccountNumber}({selectedAccount.FullName})");
            //update reciever's account balance
            selectedBankAccountReciever.AccountBalance += internalTransfer.TransferAmount;
            //print success message
            Utility.PrintMessage($"You have successfully transfered" +
                $" {Utility.FormatAmount(internalTransfer.TransferAmount)} to " +
                $"{internalTransfer.RecipientBankAccountName}", true);

        }
    }
}