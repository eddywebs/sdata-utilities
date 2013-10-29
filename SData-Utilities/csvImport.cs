using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileHelpers;

namespace SData_Utilities
{
    [DelimitedRecord(",")] 
    public class csvImport
    {
        //define the csv object by entering the format of the spreadsheet below
        public string contactID, contractEndDate, trialEnDate ;//, trialEndDate; //contactID, firstname, lastname, email, opportunityID;

            //DoAllocaitons.csv template >>MPEntityNumber, MPSubs,	CustCode, TeamName, Account, City,	State, Users, productCode;

    }
}
