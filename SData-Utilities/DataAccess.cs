using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.SData.Client.Core;

namespace SData_Deleter
{
    //This class simply holds two connections to databases used by the import; SalesLogix and MPData.
    //It is used by BaseImport and then inherited by each individual import class (ie. MP Subs, MPOrderInfo).
    internal class DataAccess
    {
        internal SLXDatabase slxDB = null;
        internal OLEDBClientDatabase mpDB = null;

        internal DataAccess()
        {
            try
            {
                slxDB = new SLXDatabase(SData_Deleter.Properties.Settings.Default.SalesLogixConnection);
            }
            catch { }

        }

        ~DataAccess()
        {
            slxDB = null;
            mpDB = null;
        }
    }
}
