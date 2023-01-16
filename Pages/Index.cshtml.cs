using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace SubnetCalc.Pages {
    public class IndexModel : PageModel {
        private readonly ILogger<IndexModel> _logger;


        public IndexModel(ILogger<IndexModel> logger) {
            _logger = logger;
        }

        [BindProperty] public string Byte1 { get; set; }
        [BindProperty] public string Byte2 { get; set; }
        [BindProperty] public string Byte3 { get; set; }
        [BindProperty] public string Byte4 { get; set; }
        [BindProperty] public string SlashX { get; set; }
        [BindProperty] public List<string> SubNetList { get; set; }

        private static List<string> StaticSubNetList = new List<string>();
        private int id = 0;
        private string binaryIP = "";
        private string netID = "";

       

        public void OnGet() {

        

        }

        public void OnPost() {

            if(!variablesValidityCheck()) {
                SubNetList = null;
                return;
            }

            /// for join and list sort check this :  https://csharp.net-tutorials.com/linq/grouping-data-the-groupby-method/

            if(id == 0 && StaticSubNetList.Count == 0) {
                SubnetCalculation(SlashX);
                Debug.WriteLine("first calc");
                SubNetList = StaticSubNetList;

            }
            else {

                string[] subnetData = StaticSubNetList[id].Split(",");

                //check for if slash of network to split isnt 32 yet
                if((Int32.Parse(subnetData[5]) + 1) > 32) {
                    //StaticSubNetList.Sort();   //Sort methode selber schreiben   https://stackoverflow.com/questions/3119448/list-sort-custom-sorting  
                    // regex wild cards : https://stackoverflow.com/questions/30299671/matching-strings-with-wildcard
                    SubNetList = StaticSubNetList;
                    return;
                }

                SplitSubnet(id);
                Debug.WriteLine("splitterooo id:" + id);
                //StaticSubNetList.Sort();   //Sort methode selber schreiben
                SubNetList = StaticSubNetList;
            }


        }

        //
        // Methods Start
        //

        private void SplitSubnet(int id) {

            //Notes
            //Splitting of the subnet in the list at the position [id]
            //broadcast = broadcast base subnet
            //ip = ---
            //netid = netid first splitsubnet+1

            binaryIP = "---";  //no more ip needed its only for the first calc

            string[] idSubnetData = StaticSubNetList[id].Split(",");

            //netid
            netID = idSubnetData[0];



            // slash X
            SubnetCalculation((Int32.Parse(idSubnetData[5]) + 1).ToString());

            //last host of first subnet

            idSubnetData = StaticSubNetList[StaticSubNetList.Count - 1].Split(",");

            netID = netIdSplit(idSubnetData[2]);


            SubnetCalculation((Int32.Parse(idSubnetData[5])).ToString());

            StaticSubNetList.RemoveAt(id);


        }

        public String netIdSplit(String oldNetID) {

            String[] Bytes = oldNetID.Split('.');

            string[] idSubnetData = StaticSubNetList[id].Split(",");

            if((Int32.Parse(idSubnetData[5]) + 1) >= 24) {

                Bytes[3] = (int.Parse(Bytes[3]) + 1).ToString();

            }
            else {

                for(int i = Bytes.Length - 1; i >= 0; i--) {

                    if(Bytes[i] == "255") {

                        Bytes[i] = "0";

                    }
                    else {
                        Bytes[i] = (int.Parse(Bytes[i]) + 1).ToString();
                    }

                }

            }

            return Bytes[0] + "." + Bytes[1] + "." + Bytes[2] + "." + Bytes[3];


        }

        private void SubnetCalculation(String SlashesX) {

            //länge jedes eintrags in der list ist 7

            //Start of exceptions
            if(!Exceptions()) {
                return;
            }
            //End of exceptions

            if(binaryIP != "---") {
                binaryIP = binaryIpCalc();
            }
            long subnetClientAmount = (long)Math.Pow(2, 32 - Int32.Parse(SlashesX));

            // Start of Subnet Id calc

            string subnetID = subnetIdCalc(subnetClientAmount);

            //End of subnet id calc

            if(binaryIP != "---") {

                // Start of binairySubnetId Calc

                string binarySubnetIP = BinairySubnetIpCalc(subnetID.Split('.'));

                // End of binairySubnetId Calc
                //Start of netId calc

                netID = NetIdCalc(binarySubnetIP);

                //end of netid calc
            }

            //
            //Start of adding data to list
            //



            String subnetData = "";
            //NETID
            subnetData += netID + ",";
            //EH
            if(binaryIP == "---") {


                string[] subnetSplitData = StaticSubNetList[id].Split(",");

                //Check if slash X smaller or equal to 31 for subnet splitting purposes
                if((Int32.Parse(subnetSplitData[5]) + 1) >= 31) {

                    subnetData += netID + ",";
                }
                else {

                    subnetData += netID.Substring(0, netID.Length - 1) + (int.Parse(netID.Substring(netID.Length - 1, 1)) + 1) + ",";

                }

            }
            else {

                subnetData += netID.Substring(0, netID.Length - 1) + (int.Parse(netID.Substring(netID.Length - 1, 1)) + 1) + ",";

            }

            //Broadcast
            subnetData += BroadcastCalc(subnetClientAmount) + ",";

            //LH
            if(binaryIP == "---") {

                string[] subnetSplitData = StaticSubNetList[id].Split(",");

                //Check if slash X smaller or equal to 31 for subnet splitting purposes
                if((Int32.Parse(subnetSplitData[5]) + 1) >= 31) {

                    subnetData += BroadcastCalc((int)subnetClientAmount) + ",";
                }
                else {

                    string[] list = BroadcastCalc((long)subnetClientAmount).Split(".");

                    subnetData += list[0] + "." + list[1] + "." + list[2] + "." + (long.Parse(list[3]) + 1) + ",";

                }

            }
            else {

                string[] list = BroadcastCalc((long)subnetClientAmount).Split(".");

                subnetData += list[0] + "." + list[1] + "." + list[2] + "." + (long.Parse(list[3]) - 1) + ",";

            }
            //SubnetIP
            subnetData += subnetID + ",";
            //Subnet /x
            subnetData += SlashesX;

        
            //
            //End of adding data to list
            //

            StaticSubNetList.Add(subnetData);

        }

        //
        // Methods End
        //

        //
        //Functions Start
        //

        private bool variablesValidityCheck() {

            if(long.TryParse(Byte1 + Byte2 + Byte3 + Byte4 + SlashX, out long n)) {
                if(int.Parse(Byte1) < 256 && int.Parse(Byte2) < 256 && int.Parse(Byte3) < 256 && int.Parse(Byte4) < 256 && int.Parse(SlashX) <= 32) {
                    if(int.Parse(Byte1) >= 0 && int.Parse(Byte2) >= 0 && int.Parse(Byte3) >= 0 && int.Parse(Byte4) >= 0 && int.Parse(SlashX) >= 0) {
                        return true;
                    }
                }
            }

            return false;
        }
        private string subnetIdCalc(double subnetClientAmount) {

            string subnetID;
            int counter = 1;

            while(subnetClientAmount >= 256) {
                subnetClientAmount /= 256;
                counter++;
            }

            if(counter == 1) {
                subnetID = "255.255.255." + (255 - subnetClientAmount + 1);
            }
            else if(counter == 2) {
                subnetID = "255.255." + (255 - subnetClientAmount + 1) + ".0";
            }
            else if(counter == 3) {
                subnetID = "255." + (255 - subnetClientAmount + 1) + ".0.0";
            }
            else {
                subnetID = (255 - subnetClientAmount + 1) + ".0.0.0";
            }

            return subnetID;

        }

        private string binaryIpCalc() {

            return Convert.ToString(Int32.Parse(Byte1), 2).PadLeft(8, '0')
                 + Convert.ToString(Int32.Parse(Byte2), 2).PadLeft(8, '0')
                 + Convert.ToString(Int32.Parse(Byte3), 2).PadLeft(8, '0')
                 + Convert.ToString(Int32.Parse(Byte4), 2).PadLeft(8, '0');
        }

        private bool Exceptions() {
            String subnetData = "";
            if(SlashX == "32") {

                subnetData += Byte1 + "." + Byte2 + "." + Byte3 + "." + Byte4 + ",";
                subnetData += Byte1 + "." + Byte2 + "." + Byte3 + "." + Byte4 + ",";
                subnetData += Byte1 + "." + Byte2 + "." + Byte3 + "." + Byte4 + ",";
                subnetData += Byte1 + "." + Byte2 + "." + Byte3 + "." + Byte4 + ",";
                subnetData += 255 + "." + 255 + "." + 255 + "." + 255 + ",";
                subnetData += "32";
                StaticSubNetList.Add(subnetData);

                return false;

            }
            if(SlashX == "31" && binaryIP != "---") {

                subnetData += Byte1 + "." + Byte2 + "." + Byte3 + "." + Byte4 + ",";
                subnetData += Byte1 + "." + Byte2 + "." + Byte3 + "." + Byte4 + ",";
                subnetData += Byte1 + "." + Byte2 + "." + Byte3 + "." + (int.Parse(Byte4) + 1) + ",";
                subnetData += Byte1 + "." + Byte2 + "." + Byte3 + "." + (int.Parse(Byte4) + 1) + ",";
                subnetData += 255 + "." + 255 + "." + 255 + "." + 255 + ",";
                subnetData += "31";
                StaticSubNetList.Add(subnetData);

                return false;

            }

            return true;


        }

        public string BinairySubnetIpCalc(string[] SubnetIPParts) {

            string binarySubnetIP = "";

            foreach(string s in SubnetIPParts) {

                binarySubnetIP += Convert.ToString(Int32.Parse(s), 2).PadLeft(8, '0');

            }

            return binarySubnetIP;

        }

        private string NetIdCalc(string binarySubnetIP) {

            String binairyNetID = "";

            for(int i = 0; i < 32; i++) {

                if(binarySubnetIP[i] == binaryIP[i] && binarySubnetIP[i] == '1' && binaryIP[i] == '1') {
                    binairyNetID += '1';
                }
                else {
                    binairyNetID += '0';
                }

            }

            return Convert.ToByte(binairyNetID.Substring(0, 8), 2).ToString()
                + "." + Convert.ToByte(binairyNetID.Substring(8, 8), 2).ToString()
                + "." + Convert.ToByte(binairyNetID.Substring(16, 8), 2).ToString()
                + "." + Convert.ToByte(binairyNetID.Substring(24, 8), 2).ToString();


        }



        private string BroadcastCalc(long subnetClientAmounts) {

            String[] netIdSplit = netID.Split('.');
            int counter = 1;
            while(subnetClientAmounts >= 256) {
                subnetClientAmounts /= 256;
                counter++;
            }

            if(counter == 1) {
                return netIdSplit[0]
                + "." + netIdSplit[1]
                + "." + netIdSplit[2]
                + "." + (int.Parse(netIdSplit[3]) + subnetClientAmounts - 1).ToString();
            }
            else if(counter == 2) {
                return netIdSplit[0]
                + "." + netIdSplit[1]
                + "." + (int.Parse(netIdSplit[2]) + subnetClientAmounts - 1).ToString()
                + "." + 255;
            }
            else if(counter == 3) {
                return netIdSplit[0]
                + "." + (int.Parse(netIdSplit[2]) + subnetClientAmounts - 1).ToString()
                + "." + 255
                + "." + 255;
            }
            else {
                return (long.Parse(netIdSplit[0]) + subnetClientAmounts - 1).ToString()
                + "." + 255
                + "." + 255
                + "." + 255;
            }

        }

        //
        //Functions End
        //


        //
        // Buttons start
        //

        public void OnPostSplitbuttonX(int id) {

            this.id = id;
            OnPost();

        }

        public void OnPostClear() {

            StaticSubNetList = new List<string>();
            Response.Redirect("https://localhost:7079/", true);


        }

        //
        // Buttons End
        //



    }
}