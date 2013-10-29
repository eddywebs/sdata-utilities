using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sage.SData.Client.Core;

namespace SData_Utilities
{
    //Class for maintaining an SData connection.
    //It is used by BaseImport and then inherited by each individual import class (ie. MP Subs, MPOrderInfo).
    public class SData
    {
        internal SDataService sdataService = null;

        internal SData()
        {
            sdataService = new SDataService();
            sdataService.UserName =   Properties.Settings.Default.SDataUserName;
            sdataService.Password =Properties.Settings.Default.SDataPassword;  
            sdataService.Protocol = "HTTP";
            sdataService.ServerName = Properties.Settings.Default.SDataHost;
            sdataService.Port = Properties.Settings.Default.SDataPort;
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
