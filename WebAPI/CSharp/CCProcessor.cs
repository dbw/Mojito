using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;

namespace LimeLM_API
{
    public class CCProcessor
    {
        public bool Errors;

        public string FirstName;
        public bool FirstNameErr;

        public string LastName;
        public bool LastNameErr;

        public string Email;
        public bool EmailErr;

        public int Quantity = 1;
        public bool QuantityErr;

        public bool IsCCCharge;


        // Credit card fields / errors
        public string CCNumber;
        public bool CCNumErr;
        public string CCNumErrMsg;

        public string CCAddress;
        public bool CCAddrErr;

        public string CCZip;
        public bool CCZipErr;

        public int CCExpiresM;
        public int CCExpiresY;
        public bool CCExpErr;


        // Bank transfer fields / errors
        public string BankErrMsg;

        public string BankAcctName;
        public bool BankAcctNameErr;

        public string BankAcctNum;
        public bool BankAcctNumErr;

        public string BankAcctType;
        public bool BankAcctTypeErr;

        public string BankName;
        public bool BankNameErr;

        public string BankRouteNum;
        public bool BankRouteNumErr;


        public bool Process()
        {
            HttpRequest req = HttpContext.Current.Request;

            FirstName = req.Form["first_name"];
            LastName = req.Form["last_name"];
            Email = req.Form["email"];

            Quantity = Convert.ToInt32(req.Form["quantity"]);

            string LongAccountType = null;

            if (FirstName == null || (FirstName = FirstName.Trim()) == string.Empty)
            {
                FirstNameErr = true;
                Errors = true;
            }

            if (LastName == null || (LastName = LastName.Trim()) == string.Empty)
            {
                LastNameErr = true;
                Errors = true;
            }

            if (Email == null || (Email = Email.Trim()) == string.Empty)
            {
                EmailErr = true;
                Errors = true;
            }

            IsCCCharge = req.Form["mf_cardtype"] == "Card";

            if (IsCCCharge)
            {
                CCNumber = Regex.Replace(req.Form["cc_num"] ?? string.Empty, @"[\D]", "");
                CCAddress = req.Form["cc_adr"];
                CCZip = req.Form["cc_zip"];
                CCExpiresM = Convert.ToInt32(req.Form["cc_expires_m"]);
                CCExpiresY = Convert.ToInt32(req.Form["cc_expires_y"]);

                if (CCZip == null || (CCZip = CCZip.Trim()) == string.Empty)
                {
                    CCZipErr = true;
                    Errors = true;
                }

                if (CCAddress == null || (CCAddress = CCAddress.Trim()) == string.Empty)
                {
                    CCAddrErr = true;
                    Errors = true;
                }

                if (CCExpiresM < 1 || CCExpiresM > 12)
                {
                    CCExpErr = true;
                    Errors = true;
                }

                DateTime date = DateTime.Now;

                // check for expired ccs
                if (CCExpiresY < date.Year || (CCExpiresY == date.Year && CCExpiresM < date.Month))
                {
                    CCExpErr = true;
                    Errors = true;
                }

                // validate credit card for typos
                // (this doesn't guarantee the CC is real - just that it passes the Luhn algorithm)

                if (CCNumber.Length == 0 || !IsLuhnValid(CCNumber))
                {
                    CCNumErrMsg = "Enter a valid credit card number";
                    CCNumErr = true;
                    Errors = true;
                }
                else if (!IsAcceptedCC(CCNumber))
                {
                    CCNumErrMsg = "We only accept " + AcceptedCCs;
                    CCNumErr = true;
                    Errors = true;
                }
            }
            else // Bank transfer
            {
                BankAcctName = req.Form["bank_acct_name"];
                BankAcctType = req.Form["bank_acct_type"];
                BankName = req.Form["bank_name"];
                BankAcctNum = Regex.Replace(req.Form["bank_acct_num"] ?? string.Empty, @"[\D]", "");
                BankRouteNum = Regex.Replace(req.Form["bank_route_num"] ?? string.Empty, @"[\D]", "");

                if (BankAcctName == null || (BankAcctName = BankAcctName.Trim()) == string.Empty)
                {
                    BankAcctNameErr = true;
                    Errors = true;
                }

                switch (BankAcctType)
                {
                    case "C":
                        LongAccountType = "CHECKING";
                        break;
                    case "S":
                        LongAccountType = "SAVINGS";
                        break;
                    default:
                        BankAcctTypeErr = true;
                        Errors = true;
                        BankAcctType = null;
                        break;
                }

                if (BankName == null || (BankName = BankName.Trim()) == string.Empty)
                {
                    BankNameErr = true;
                    Errors = true;
                }

                if (BankRouteNum.Length != 9 || !IsABARouteValid(BankRouteNum))
                {
                    BankRouteNumErr = true;
                    Errors = true;
                }

                if (BankAcctNum.Length == 0)
                {
                    BankAcctNumErr = true;
                    Errors = true;
                }
            }

            if (Quantity < 1)
            {
                Errors = true;
                QuantityErr = true;
                Quantity = 1;
            }

            if (Errors)
                return false;

            // format in the XXX.YY format
            string amount = ((PaymentSettings.AppPrice[0] * Quantity + ((PaymentSettings.AppPrice[1] * Quantity) / 100)) + "." +
                                ((PaymentSettings.AppPrice[1] * Quantity) % 100).ToString("00", CultureInfo.InvariantCulture));

            // set the Auth.Net details
            AuthorizeNet.SetLoginDetails(PaymentSettings.AuthNetLogin, PaymentSettings.AuthNetTansKey,
                                         PaymentSettings.AuthNetTest);

            // Charge or transfer
            if (IsCCCharge)
            {
                string result = AuthorizeNet.Charge(CCNumber, CCExpiresM.ToString("00", CultureInfo.InvariantCulture) + "/" + CCExpiresY,
                                    amount, FirstName, LastName, CCZip, CCAddress, PaymentSettings.AppName, Email);

                if (result != null)
                {
                    CCNumErrMsg = result;
                    CCNumErr = true;
                    Errors = true;
                    return false;
                }
            }
            else
            {
                string result = AuthorizeNet.BankTransfer(BankAcctNum, BankRouteNum, LongAccountType, BankName, BankAcctName,
                                                   amount, FirstName, LastName,
                                                   PaymentSettings.AppName, Email);

                if (result != null)
                {
                    BankErrMsg = result;
                    Errors = true;
                    return false;
                }
            }

            // generate the product key and email them to the user
            PaymentSettings.SendPKeys(Quantity, Email, FirstName, LastName);

            // return true if no errors have occurred
            return true;
        }

        static bool IsLuhnValid(string number)
        {
            int[] DELTAS = new[] { 0, 1, 2, 3, 4, -4, -3, -2, -1, 0 };
            int checksum = 0;
            char[] chars = number.ToCharArray();
            for (int i = chars.Length - 1; i > -1; i--)
            {
                int j = chars[i] - 48;
                checksum += j;
                if (((i - chars.Length) % 2) == 0)
                    checksum += DELTAS[j];
            }

            return ((checksum % 10) == 0);
        }

        static bool IsAcceptedCC(string cc)
        {
            switch (cc[0])
            {
                case '1': // JCB starting with 1 or 2
                case '2':
                    return Regex.IsMatch(cc, @"^(?:2131|1800)\d{11}$");

                case '3': // American Express or JCB
                    return Regex.IsMatch(cc, @"^3[47][0-9]{13}$") || Regex.IsMatch(cc, @"^(?:35)\d{14}$");

                case '4': // Visa
                    return Regex.IsMatch(cc, @"^4[0-9]{12}(?:[0-9]{3})?$");

                case '5': // MasterCard
                    return Regex.IsMatch(cc, @"^5[1-5][0-9]{14}$");

                case '6': // Discover
                    return Regex.IsMatch(cc, @"^6(?:011|5[0-9]{2})[0-9]{12}$");
            }

            return false;
        }

        static bool IsABARouteValid(string routeNum)
        {
            char[] r = routeNum.ToCharArray();
            return (r[8] - '0') == (7 * ((r[0] - '0') + (r[3] - '0') + (r[6] - '0')) + 3 * ((r[1] - '0') + (r[4] - '0') + (r[7] - '0')) + 9 * ((r[2] - '0') + (r[5] - '0'))) % 10;
        }

        public const string AcceptedCCs = "Visa, American Express, MasterCard, Discover, and JCB";
    }
}