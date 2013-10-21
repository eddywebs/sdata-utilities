using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.SData.Client.Core;

namespace SData_Deleter
{
    //Class for maintaining an SData connection.
    //It is used by BaseImport and then inherited by each individual import class (ie. MP Subs, MPOrderInfo).
    internal class SData
    {
        internal SDataService sdataService = null;

        internal SData()
        {
            sdataService = new SDataService();
            sdataService.UserName = SData_Deleter.Properties.Settings.Default.SDataUserName.ToString();
            sdataService.Password = SData_Deleter.Properties.Settings.Default.SDataPassword.ToString();
            sdataService.Protocol = "HTTP";
            sdataService.ServerName = SData_Deleter.Properties.Settings.Default.SDataHost.ToString();
            sdataService.Port = SData_Deleter.Properties.Settings.Default.SDataPort;
            sdataService.ApplicationName = "slx";
            sdataService.VirtualDirectory = "sdata";
            sdataService.ContractName = "dynamic";
        }

        ~SData()
        {
            sdataService = null;
        }
    }
}
