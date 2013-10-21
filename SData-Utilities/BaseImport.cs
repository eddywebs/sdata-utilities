using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Sage.SData.Client;
using Sage.SData.Client.Extensions;
using Sage.SData.Client.Core;
using Sage.SData.Client.Atom;
using System.Globalization;
using System.Threading;
using System.Diagnostics;

namespace SData_Deleter
{
    //Base class that manages the database/SData connections for the other classes.  There are also 2 lower level classes that actually hold the database/sdata connections. This level only defines them so that they don't have to be defined in every class.
    //It also contains most of the searches used by the rest of the import.
    //They all inherit this class to make them a littler cleaner and avoid replicating the same functionality across each class.

    internal class BaseImport
    {
        internal DataAccess dataAccess = null;
        internal SData sdata = null;
        internal string sql = null;
        internal DataTable dt = null;
        internal string resourceKind = null;
        internal TextInfo textInfo;

        internal BaseImport()
        {        
            textInfo = new CultureInfo("en-US",false).TextInfo;
            dataAccess = new DataAccess();
            sdata = new SData();
        }

        ~BaseImport()
        {
            try
            {
                dataAccess = null;
                sdata = null;
            }
            catch { };
        }

        //Placeholder function for other classes. This way we'll get an error if we forget to implement DoImport in a class that inherits this one.
        internal virtual void DoImport()
        {
            throw new NotImplementedException();
        }


        //Overloaded function that lets us retreive a Contact via sData.
        //This instance accepts the ContactId and therefore is a SingleResourceRequest because there can only ever be 1 record with a particular ContactId.
        internal AtomEntry GetContact(string contactId)
        {
            if (contactId == "") throw new Exception("Contact " + contactId + " not found.");

            SDataSingleResourceRequest req = new SDataSingleResourceRequest(sdata.sdataService);
            req.ResourceKind = "contacts";
            req.ResourceSelector = "'" + contactId + "'";
            return req.Read();
        }

        //Overloaded function that lets us retreive a Contact via sData.
        //This instance accepts subsNum and therefore is a ResourceCollectionRequest because although there should only be one record, it is not guaranteed.
        //If the search finds more than 1 Contact with the specified subsNum, it returns NULL because there is no way for it to know which record to pick.  As such, it picks neither.  This is a rule requested by BCA for their import and was carried into the NDR system.
        internal AtomEntry GetContact(int subsNum)
        {
            SDataResourceCollectionRequest req = new SDataResourceCollectionRequest(sdata.sdataService);
            req.ResourceKind = "contacts";
            req.QueryValues.Add("where", "Subsnum eq \"" + subsNum.ToString() + "\"");
            AtomFeed feed = req.Read();
            if (feed.Entries.Count() == 1)
            {
                foreach (AtomEntry entry in feed.Entries)
                {
                    return entry;
                }
            }

            return null;
        }

        //Overloaded function that lets us retreive a Contact via sData.
        //This instance accepts email, firstName, lastName, and accountName. It is a ResourceCollectionRequest as there may be many matches.
        //If the search finds more than 1 Contact with the specified criteria, it returns NULL because there is no way for it to know which record to pick.  As such, it picks neither.  This is a rule requested by BCA for their import and was carried into the NDR system.
        //This function is only called by the MPSubs class, which is doing Account/Contact updates.  If MPSubs previously failed to match a Contact by subsNum, we want to make sure that within this function, it searches for a Contact with blank subsNum.
        internal AtomEntry GetContact(string email, string firstName, string lastName, string accountName)
        {
            SDataResourceCollectionRequest req = new SDataResourceCollectionRequest(sdata.sdataService);
            AtomFeed feed;
            if (email != "")
            {
                req.ResourceKind = "contacts";
                req.QueryValues.Add("where", "(Email eq \"" + email + "\" or SecondaryEmail eq \"" + email + "\" or Email3 eq \"" + email + "\") and (Subsnum eq null or Subsnum eq \"\")");
                feed = req.Read();
                if (feed.Entries.Count() == 1)
                {
                    foreach (AtomEntry entry in feed.Entries)
                    {
                        return entry;
                    }
                }
            }

            //Couldn't find by email, so search by name
            req = new SDataResourceCollectionRequest(sdata.sdataService);
            req.ResourceKind = "contacts";
            req.QueryValues.Add("where", "FirstName eq \"" + firstName + "\" and LastName eq \"" + lastName + "\" and AccountName eq \"" + accountName + "\" and (Subsnum eq null or Subsnum eq \"\")");
            feed = req.Read();
            if (feed.Entries.Count() == 1)
            {
                foreach (AtomEntry entry in feed.Entries)
                {
                    return entry;
                }
            }

            return null;
        }

        //Overloaded function that lets us retreive an Account via sData.
        //This instance accepts the AccountId and therefore is a SingleResourceRequest because there can only ever be 1 record with a particular Account.
        internal AtomEntry GetAccount(string accountId)
        {
            if (accountId == "") throw new Exception("Account " + accountId + " not found.");

            SDataSingleResourceRequest req = new SDataSingleResourceRequest(sdata.sdataService);
            req.ResourceKind = "accounts";
            req.ResourceSelector = "'" + accountId + "'";
            return req.Read();
        }

        //Overloaded function that lets us retreive an Account via sData.
        //This instance accepts entityNumber and subsNum and therefore is a ResourceCollectionRequest because although there should only be one record, it is not guaranteed.
        //If the search finds more than 1 Account, it returns NULL because there is no way for it to know which record to pick.  As such, it picks neither.  This is a rule requested by BCA for their import and was carried into the NDR system.
        internal AtomEntry GetAccount(string entityNumber, int subsNum)
        {
            if (entityNumber != "")
            {
                SDataResourceCollectionRequest req = new SDataResourceCollectionRequest(sdata.sdataService);
                req.ResourceKind = "accounts";
                req.QueryValues.Add("where", "MPEntityNumber eq \"" + entityNumber + "\"");
                AtomFeed feed = req.Read();
                if (feed.Entries.Count() == 1)
                {
                    foreach (AtomEntry entry in feed.Entries)
                    {
                        return entry;
                    }
                }
            }
            //Couldn't find by entityNumber, so search by subsNum
            return GetAccount(subsNum);
        }

        //Overloaded function that lets us retreive a Account via sData.
        //This instance accepts subsNum and therefore is a ResourceCollectionRequest because although there should only be one record, it is not guaranteed.
        //If the search finds more than 1 Account with the specified subsNum, it returns NULL because there is no way for it to know which record to pick.  As such, it picks neither.  This is a rule requested by BCA for their import and was carried into the NDR system.
        internal AtomEntry GetAccount(int subsNum)
        {
            SDataResourceCollectionRequest req = new SDataResourceCollectionRequest(sdata.sdataService);
            req.ResourceKind = "accounts";
            req.QueryValues.Add("where", "Subsnum eq \"" + subsNum.ToString() + "\"");
            AtomFeed feed = req.Read();
            if (feed.Entries.Count() == 1)
            {
                foreach (AtomEntry entry in feed.Entries)
                {
                    return entry;
                }
            }

            return null;
        }

        //Overloaded function that lets us retreive a Account via sData.  The search returns Accounts, but actually searches on Contact fields.
        //This instance accepts email, firstName, lastName, and accountName. It is a ResourceCollectionRequest as there may be many matches.
        //If the search finds more than 1 Contact with the specified criteria, it returns NULL because there is no way for it to know which record to pick.  As such, it picks neither.  This is a rule requested by BCA for their import and was carried into the NDR system.
        //This function is only called by the MPSubs class, which is doing Account/Contact updates.  If MPSubs previously failed to match a Account by entity or subsNum, we want to make sure that within this function, it searches for a Account with blank subsNum.
        internal AtomEntry GetAccount(string email, string firstName, string lastName, string accountName)
            //Gets account based on email, firstname last name of a associated contact only if account has no primary contact associated(new company from MP) or if contact searched happens to primary
        {
            SDataResourceCollectionRequest req = new SDataResourceCollectionRequest(sdata.sdataService);
            AtomFeed feed;

            if (email != "")
            {
                req.ResourceKind = "accounts"; 
                req.QueryValues.Add("where", "(Contacts.Email eq \"" + email + "\" or Contacts.SecondaryEmail eq \"" + email + "\" or Contacts.Email3 eq \"" + email + "\") and (Subsnum eq null or Subsnum eq \"\")"); // should we check for contact subsNum for the contact or Account (Dennis)
                feed = req.Read();
                if (feed.Entries.Count() == 1)
                {
                    foreach (AtomEntry entry in feed.Entries)
                    {
                        return entry;
                    }
                }
            }

            //Couldn't find by email, so search by name
            req = new SDataResourceCollectionRequest(sdata.sdataService);
            req.ResourceKind = "accounts";
            req.QueryValues.Add("where", "Contacts.FirstName eq \"" + firstName + "\" and Contacts.LastName eq \"" + lastName + "\" and AccountName eq \"" + accountName + "\" and (Subsnum eq null or Subsnum eq \"\")");
            feed = req.Read();
            if (feed.Entries.Count() == 1)
            {
                foreach (AtomEntry entry in feed.Entries)
                {
                    return entry;
                }
            }

            return null;
        }

        //Function that lets us retreive a BillTo via sData.
        //Accepts the BillToId and therefore is a SingleResourceRequest because there can only ever be 1 record with a particular BillToId.
        internal AtomEntry GetBillTo(string billToId)
        {
            if (billToId == "") throw new Exception("Bill To " + billToId + " not found.");

            SDataSingleResourceRequest req = new SDataSingleResourceRequest(sdata.sdataService);
            req.ResourceKind = "billTos";
            req.ResourceSelector = "'" + billToId + "'";
            return req.Read();
        }

        //Function that lets us retreive a Address via sData.
        //Accepts the AddressId and therefore is a SingleResourceRequest because there can only ever be 1 record with a particular AddressId.
        internal AtomEntry GetAddress(string addressId)
        {
            if (addressId == "") throw new Exception("Address not found.");

            SDataSingleResourceRequest req = new SDataSingleResourceRequest(sdata.sdataService);
            req.ResourceKind = "addresses";
            req.ResourceSelector = "'" + addressId + "'";
            return req.Read();
        }

        //Function that creates a new Address entity
        internal AtomEntry CreateAddress()
        {

            SDataTemplateResourceRequest req = new SDataTemplateResourceRequest(sdata.sdataService);
            req.ResourceKind = "addresses";
            return req.Read();
        }

        //Function that lets us retreive an AccountManager (user) via sData.
        //Accepts the AccountManagerId and therefore is a SingleResourceRequest because there can only ever be 1 record with a particular UserId.
        //If no match is found, it returns ADMIN.
        internal AtomEntry GetAccountManager(string accountManagerId)
        {
            SDataSingleResourceRequest req = new SDataSingleResourceRequest(sdata.sdataService);
            req.ResourceKind = "users";

            try
            {
                req.ResourceSelector = "'" + accountManagerId + "'";
                return req.Read();
            }
            catch
            {
                req.ResourceSelector = "'ADMIN'";
                return req.Read();
            }
        }

        //Function that lets us retreive an Owner (seccode) via sData.
        //Accepts the OwnerId and therefore is a SingleResourceRequest because there can only ever be 1 record with a particular SeccodeId.
        //If no match is found, it returns EVERYONE.
        internal AtomEntry GetOwner(string ownerId)
        {
            SDataSingleResourceRequest req = new SDataSingleResourceRequest(sdata.sdataService);
            req.ResourceKind = "owners";

            try
            {
                req.ResourceSelector = "'" + ownerId + "'";
                return req.Read();
            }
            catch
            {
                req.ResourceSelector = "'SYST00000001'";
                return req.Read();
            }
        }


        //public void doThis()
        //{

        //    //Call the view to get a listing of all records that exist in SLX, but not MP; these records should be deleted.
        //    sql = "select top 3 * from contact"; //SData_Deleter.Properties.Settings.Default.Sql;
        //    dataAccess = new DataAccess();
        //    dt = new DataTable();
        //    dt = dataAccess.mpDB.GetDatatable("select top 3 * from contact");


        //    //Loop through the view, deleting each record.
        //    foreach (DataRow dr in dt.Rows)
        //    {
        //        try
        //        {
        //            Console.WriteLine("Deleting the recordID: " + TypeConversion.ToString(dr["'" + SData_Deleter.Properties.Settings.Default.EntityIDColumnName.ToString() + "'"]));

        //            SDataSingleResourceRequest req = new SDataSingleResourceRequest(sdata.sdataService);
        //            req.ResourceKind = resourceKind;
        //            req.ResourceSelector = "'" + TypeConversion.ToString(dr["'" + SData_Deleter.Properties.Settings.Default.EntityIDColumnName.ToString() + "'"]) + "'";
        //            req.Delete();
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine("ahh oo there was an error check eventViewer for details.");
        //            Utilities.CreateEventLog("Error on Delete Loop " + TypeConversion.ToString(dr["'" + SData_Deleter.Properties.Settings.Default.EntityIDColumnName.ToString() + "'"]) + Environment.NewLine + ex.Message, EventLogEntryType.Error);
        //        }
        //    }
        //}

    }
}