using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Sage.SData.Client.Extensions;
using Sage.SData.Client.Core;
using Sage.SData.Client.Atom;
using System.Diagnostics;
using System.IO;
using FileHelpers;
using Sage.Entity.Interfaces;

namespace SData_Deleter
{
    class Program : BaseImport
    {

        static void Main(string[] args)
        {
            //var baseImport = new BaseImport();
            Program bass = new Program();
            int counter = 0;
            string importFile = "qaTrials.csv"; //name of the .csv to be used in import(should be present in the same directory as of the compiled binaries)

            FileHelperEngine engine = new FileHelperEngine(typeof(csvImport));
            csvImport[] res = engine.ReadFile(importFile) as csvImport[];
            
            foreach (csvImport importRecord in res)
            {
                //sdataUpdate(importRecord.CustCode, importRecord.Users);              
                bass.sdataAdder(importRecord, "contactTrials");
                counter += 1;
                Console.WriteLine("processed .csv record : " + importRecord.contactID +" "+importRecord.contactID + " counter is at " + counter + "\n");
            }

            Console.Write("\n" +"processed:  "+counter+" records \n" + "All done !");
            Console.ReadLine();
        }

//        public void parseCsv(string importFile)
//        {
//            int counter = 0;
//            FileHelperEngine engine = new FileHelperEngine(typeof(csvImport));
//            csvImport[] res= engine.ReadFile(importFile) as csvImport[];           

//            foreach (csvImport importRecord in res)
//            {            
//                sdataUpdate(importRecord.CustCode, importRecord.Users);
//                counter += 1;
//                Console.WriteLine("parseCSV record at: " + importRecord.CustCode + "counter is at " + counter + "\n");
////////////////// Below code for Adding new users
//               // var req = new SDataResourceCollectionRequest(sdata.sdataService); // var req = new SDataResourceCollectionRequest(svc);
//               // req.ResourceKind = "Accounts";
                
//               //req.QueryValues.Add("where", "AccountKey eq \""+importRecord.accountKey+"\"");
//               //AtomFeed feed;
//               //feed = req.Read();
               
//               // if (feed.Entries.Count() == 1)
//               //{
//               //    counter++;
//               //   //send the entry to add the contact
//               //    foreach (AtomEntry entry in feed.Entries)
//               //    {
//               //        sdataAdder(entry, importRecord,counter);
//               //    }

//               //}
//               // else 
//               //      Console.WriteLine("could not find the account name: "+ importRecord.accountName+" \n");

//            }

//        }

        public AtomFeed recordSetLoader(string searchValue, string resourceKind, string searchParam)
            //this method returns the record set through sdata based on single property for any entity
        {
            SDataResourceCollectionRequest req = new SDataResourceCollectionRequest(sdata.sdataService);
            req.ResourceKind = resourceKind;
            req.QueryValues.Add("where", searchParam + " eq \"" + searchValue + "\"");
            AtomFeed feed = req.Read();

            return feed;
        }

        public SDataPayload getResource(string resourceKind, string resourceID)
            //this functions gets anyrecord entry object from the database
        {
            SDataSingleResourceRequest request = new SDataSingleResourceRequest(sdata.sdataService);
            request.ResourceKind = resourceKind;
            request.ResourceSelector = "'"+resourceID+"'";
            try
            {
                AtomEntry resourceEntry = request.Read();
                return resourceEntry.GetSDataPayload();
            }
            catch 
            {
                return null;
            }
        }

        public void recordUpdater(AtomEntry payloadEntrytoUpdate,string resourceKind)
        {
            //this is work in progress
            SDataSingleResourceRequest updateRecord = new SDataSingleResourceRequest(sdata.sdataService);
            updateRecord.ResourceKind = resourceKind;
            updateRecord.Entry = payloadEntrytoUpdate;
            try
            {

                //pull the payloads out
                //ticket = entry.GetSDataPayload();
                //account = (SDataPayload)ticket.Values["Account"];

                ////update the account payload as needed 
                //account.Values["UserField1"] = "Sam";


                ////put everything back..
                ////account back into ticket
                //ticket.Values["Account"] = account;
                ////ticket back into the entry
                //entry.SetSDataPayload(ticket);
                ////entry back into the request
                //request.Entry = entry;
                //request.Update();
                updateRecord.Update();
            }
            catch
            {
            }

        }

        public void sdataAdder(csvImport importRecord,string resourceToCreate )
            //this method adds records based on the line items of main .CSV defined at the program main
        {
            SDataTemplateResourceRequest reqNew = new SDataTemplateResourceRequest(sdata.sdataService);
            reqNew.ResourceKind = resourceToCreate;
            AtomEntry newPayload = reqNew.Read();
            SDataPayload newRecordPayload = newPayload.GetSDataPayload();



            //assign the values of the paypload properties accordingly (all record properties must be assigned a value else it fails in the business rules)
            //newRecordPayload.Values["Opportunity"] = getResource("opportunities", importRecord.opportunityID);
            newRecordPayload.Values["Contact"] = getResource("contacts", importRecord.contactID);
            newRecordPayload.Values["StartDate"] = DateTime.Today;
            newRecordPayload.Values["TrialActive"] = true;
            newRecordPayload.Values["Product"] = "REG";
            try { newRecordPayload.Values["EndDate"]= Convert.ToDateTime(importRecord.trialEnDate);} catch {newRecordPayload.Values["EndDate"]=DateTime.Today;} //importRecord.endDate;
        
            try
            {
                //create record throught sdata
                SDataSingleResourceRequest record = new SDataSingleResourceRequest(sdata.sdataService);
                record.ResourceKind = resourceToCreate;
                record.Entry = newPayload;
                record.Create();
            }
            catch
            {
                Console.WriteLine("failed creating"+importRecord.contactID+"\n");
            }

        }
        public void sdataUpdate(string recordLocater,string updateValue)
        {

            SDataResourceCollectionRequest req = new SDataResourceCollectionRequest(sdata.sdataService);

            //search for contact by passed param -- email
            req.ResourceKind = "contacts";
            req.QueryValues.Add("where", "Customercode eq \"" + recordLocater + "\"");
            //do we need to define the number of record count whenver quering from sdata ?
            req.Count = 2000;

            AtomFeed feed = req.Read();

            //start counter
            int counter = 0;
            foreach(AtomEntry entry in feed.Entries)
            {
                counter += 1;
                SDataSingleResourceRequest record = new SDataSingleResourceRequest(sdata.sdataService);
                record.ResourceKind = "contacts";
                record.Entry = entry;
                
                SDataPayload payLoad = entry.GetSDataPayload();
                //use param2 as value updated
                payLoad.Values["UserField8"] = updateValue;

               //  payLoad.Values["FaxTitle"] = "";
              //  payLoad.Values["UserField4"] = "account manager synced with ods";
                Console.WriteLine("\n sdataUpdate counter at: "+ counter + " update record id:" + record.Entry.Id.ToString() + "\n");
                record.Update();
 
               // Console.WriteLine("\n Deleting the record id: "+record.Entry.Id.ToString() +"\n");
               // record.Delete();

            }

            Console.WriteLine("process affected " + counter + " records");
           // req.ResourceSelector = "'" + TypeConversion.ToString(dr["'" + SData_Deleter.Properties.Settings.Default.EntityIDColumnName.ToString() + "'"]) + "'";
            
        }

    }
}
