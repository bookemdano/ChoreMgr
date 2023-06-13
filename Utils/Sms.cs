using ChoreMgr.Utils;
using PrivateStash; // get from bookemdano.github.com
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Finder2020Win
{
    public class Sms
    {
        static public string SendCodeToMe(string app)
        {
            var code = new Random().Next(999999).ToString("000000");

            SendMessage("4109608923", $"Access code from {app}- {code}");
            return code;
        }
        /// <summary>
        /// Sends message with Twilio- costs $.0075 per message and spamming will get us blocked so use sparingly
        /// </summary>
        /// <param name="phone">10-digit US phone number</param>
        /// <param name="body"></param>
        /// <returns>returns false if phone number invalid</returns>
        static public bool SendMessage(string phone, string body)
        {
            DanLogger.Log($"Sms.SendMessage({phone})");
            try
            {
                phone = StripPhoneNumber(phone);
                if (phone?.Length == 10)
                    phone = "1" + phone;
                if (phone?.Length == 11)
                    phone = "+" + phone;
                if (phone?.Length != 12)
                {
                    DanLogger.Log($"SMS.SendMessage() Invalid phone number {phone}");
                    return false;
                }

                DanLogger.Log($"SMS.SendMessage() to {phone}");

                TwilioClient.Init(TwilioStash.AccountSid, TwilioStash.AuthToken);

                DanLogger.Log("Sms.SendMessage() Really send");
                var message = MessageResource.Create(
                    to: new PhoneNumber(phone),
                    from: new PhoneNumber(TwilioStash.FromNumber),
                    body: body);

                return true;
            }
            catch (Exception ex)
            {
                DanLogger.Log("SMS.SendMessage() " + ex);
                return false;
            }
        }
        public static string StripPhoneNumber(string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;

            var rv = string.Empty;
            foreach (var c in str)
                if (char.IsNumber(c))
                    rv += c;
            return rv;
        }
    }
}
