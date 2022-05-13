using System;

namespace QSDataUpdateAPI.Domain
{
    public class Constants
    {
        public static string WS_Redbox_ChannelsService
        {
            get
            {
                return "https://dev.stanbicibtc.com:8443/uat/redbox/services/channels?wsdl"; //Todo: Depend on some settings helper to readIn values from config
            }
        }

        public static string WS_Redbox_ReqManager { 
            get
            {
                return "https://dev.stanbicibtc.com:8443/uat/redbox/services/request-manager"; //Todo: Depend on some settings helper to read values from config
            }
        }
    }
}
