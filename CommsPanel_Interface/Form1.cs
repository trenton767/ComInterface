using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FSUIPC;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace CommsPanel_Interface
{
    public partial class Form1 : Form
    {
        VATSIM VatDATA;
        List<DBS_MFS> MFS = new List<DBS_MFS>();
        public int maxFreq = 136;
        public int minFreq = 118;
        bool Connection = false;
        // KEYS = ["!","@","#","$","%","^"]
        public Form1()
        {
            openFSUIPC();
            if (Connection)
            {
                FSUIPCConnection.Process();
                InitializeComponent();
                lookForConnections();
                //scrapeVATSIM();
                CheckForConnectionsTMR.Start();
                ListenTMR.Start();
            }
            else
            {
                this.Close();
            }
            //timer1.Start();
        }

        private void openFSUIPC()
        {
            try
            {
                FSUIPCConnection.Open();
                Connection = true;
            }
            catch (Exception ex)
            {
                Connection = false;
            }
        }

        public async void scrapeVATSIM()
        {
            debugBox_tb.Text = "";
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://data.vatsim.net/v3/vatsim-data.json");
            request.Headers.Add("Accept", "application/json");
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            //Console.WriteLine(await response.Content.ReadAsStringAsync());
            string rsp = await response.Content.ReadAsStringAsync();
            VatDATA = new VATSIM(rsp);

            //debugBox_tb.Text = packageControllers();
            //debugBox_tb.Text = processRequest("$|D1|MMMX_ATIS");
        }

        private void VATSIM_ScapeTMR_Tick(object sender, EventArgs e)
        {
            scrapeVATSIM();
        }

        private string packageControllers()
        {
            string header = "@|PC|";
            int cnt = VatDATA.controllers.Count;
            string body = "";
            foreach(VATcontrollers cont in VatDATA.controllers)
            {
                if (!cont.callsign.Contains("OBS") && cont.frequency != "199.998")
                { 
                    body = body + cont.callsign + "," + cont.frequency + "|";
                }
            }

            return header + body.Remove(body.Length-1);
        }

        private string packageAircraftData()
        {
            string acData = "@|PC|";

            //grab Data with FSUIPC
            acData += "COM1_ACT," + processFreqToString(COM1ACT.Value) + "|";
            acData += "COM1_STBY," + processFreqToString(COM1STBY.Value) + "|";
            acData += "COM2_ACT," + processFreqToString(COM2ACT.Value) + "|";
            acData += "COM2_STBY," + processFreqToString(COM2STBY.Value) + "|";
            acData += "NAV1_ACT," + processFreqToString(NAV1ACT.Value) + "|";
            acData += "NAV1_STBY," + processFreqToString(NAV1STBY.Value) + "|";
            acData += "NAV2_ACT," + processFreqToString(NAV2ACT.Value) + "|";
            acData += "NAV2_STBY," + processFreqToString(NAV2STBY.Value) + "|";
            //acData += "C1TX," + "|";
            //acData = acData + "COM1_ACT,121.900|COM1_STBY,122.800|COM2_ACT,120.700|COM2_STBY,118.100|XPDR_CODE,1244|XPDR_MODE,ALT|NAV1_ACT,111.10|NAV1_STBY,111.15|NAV2_ACT,112.10|NAV2_STBY,112.20|C1TX,1|C1RX,1|";
            Console.WriteLine(acData);
            return acData;
        }

        private void sendSerialMessage(SerialPort outPort, string Message)
        {
            outPort.WriteLine(Message);
        }

        private string processRequest(string inRqst)
        {
            string header = "^|PC|";
            string body = "";
            string output = "";
            string[] msgParts = inRqst.Split('|');
            string deviceName = msgParts[1];
            DBS_MFS sender = MFS.Find(i => i.deviceName == deviceName);

            if (msgParts[0] == "!")
            {
                //Action
                if (msgParts.Length >= 4)
                {
                    foreach (string obj in msgParts)
                    {
                        if (obj.Contains("_STBY"))
                        {
                            string freq = obj.Split(',')[1];
                            if (obj.Contains("COM1"))
                            {
                                //FSUIPC Set COM1 Stby to freq
                                setCOM1Sby(freq);
                                string msg = "@|PC|COM1_STBY," + processFreqToString(COM1STBY.Value);
                                sendSerialMessage(sender.COM, msg);
                            }
                            else if (obj.Contains("COM2"))
                            {
                                //FSUIPC Set COM2 Stby to freq
                                setCOM2Sby(freq);
                                string msg = "@|PC|COM2_STBY," + processFreqToString(COM2STBY.Value);
                                sendSerialMessage(sender.COM, msg);
                            }
                            else if (obj.Contains("NAV1"))
                            {
                                //FSUIPC Set NAV1 Stby to freq
                                setNAV1Sby(freq);
                                string msg = "@|PC|NAV1_STBY," + processFreqToString(NAV1STBY.Value);
                                sendSerialMessage(sender.COM, msg);
                            }
                            else if (obj.Contains("NAV2"))
                            {
                                //FSUIPC Set NAV2 Stby to freq
                                setNAV2Sby(freq);
                                string msg = "@|PC|NAV2_STBY," + processFreqToString(NAV2STBY.Value);
                                sendSerialMessage(sender.COM, msg);
                            }
                        }
                        if (obj.Contains("_SWAP"))
                        {
                            if (obj.Contains("COM1"))
                            {
                                //FSUIPC Switch COM1 Freqs
                                swapCOM1();
                                string msg = "@|PC|COM1_STBY," + processFreqToString(COM1STBY.Value) + "|COM1_ACT," + processFreqToString(COM1ACT.Value);
                                sendSerialMessage(sender.COM, msg);
                            }
                            else if (obj.Contains("COM2"))
                            {
                                //FSUIPC Switch COM2 Freqs
                                swapCOM2();
                                string msg = "@|PC|COM2_STBY," + processFreqToString(COM2STBY.Value) + "|COM2_ACT," + processFreqToString(COM2ACT.Value);
                                sendSerialMessage(sender.COM, msg);
                            }
                            else if (obj.Contains("NAV1"))
                            {
                                //FSUIPC Switch NAV1 Freqs
                                swapNAV1();
                                string msg = "@|PC|NAV1_STBY," + processFreqToString(NAV1STBY.Value) + "|NAV1_ACT," + processFreqToString(NAV1ACT.Value);
                                sendSerialMessage(sender.COM, msg);
                            }
                            else if (obj.Contains("NAV2"))
                            {
                                //FSUIPC Switch NAV1 Freqs
                                swapNAV2();
                                string msg = "@|PC|NAV2_STBY," + processFreqToString(NAV2STBY.Value) + "|NAV2_ACT," + processFreqToString(NAV2ACT.Value);
                                sendSerialMessage(sender.COM, msg);
                            }
                        }
                        if (obj.Contains("XPDR"))
                        {
                            if (obj.Contains("_Code"))
                            {
                                string code = obj.Split(',')[1];
                                //FSUIPC Set XPDR Code
                            }
                            else if (obj.Contains("_ALT"))
                            {
                                //FSUIPC Set XPDR to ALT/Mode C
                            }
                            else if (obj.Contains("_IDENT"))
                            {
                                //FSUIPC IDENT
                            }
                        }
                    }
                }
            }
            else if (msgParts[0] == "#")
            {
                //Connection Request
                body = body + msgParts[1];
                output = header + body + "|";
                //packageSerialMessage();
                //Store Serial Connection
            }
            else if (msgParts[0] == "$")
            {
                //ATIS Request
                string atisText = VatDATA.atis.First(item => item.callsign == msgParts[2]).text_atis;
                //process ATIS Message
                string atisCode = "A";
                string atisStation = "KCVG_A";
                string atisWinds = "180@3";
                string atisAltimeter = "A2992";
                string atisTxt = "10SM BKN043 BKN050 OVC200 22/13 RMK AO2 SLP186 BINOVC T02220133. EXPECT VISUAL APPROACH TO RWY 18L. ALL AIRCRAFT SHALL READBACK RUNWAY HOLD SHORT INSTRUCTION. NOTICE TO AIR MISSIONS. RWY 18C/36C CLSD. CINCINNATI VORTAC OTS. SIGMETS/CWA IN EFCT. CTC FSS FOR INFO. BIRD ACTIVITY VCNTY ARPT. ...ADVS YOU HAVE INFO M.";
                output = header + atisCode + "|" + atisStation + "|" + atisWinds + "|" + atisAltimeter + "|" + atisTxt;
            }

            return output;  
        }

        public void lookForConnections()
        {
            string[] ports = SerialPort.GetPortNames();
            foreach(string port in ports)
            {
                Console.WriteLine(port);
                SerialPort comPort = new SerialPort(port, 9600);
                try
                {
                    comPort.Open();
                    comPort.DtrEnable = true;
                    //comPort.DataReceived += SerialPortDateReceived;
                    System.Threading.Thread.Sleep(1000);
                    string DataRecieved = comPort.ReadExisting();
                    Console.WriteLine(DataRecieved);
                    string msg = processRequest(DataRecieved);
                    if (msg != "")
                    {
                        string[] parts = msg.Split('|');
                        MFS.Add(new DBS_MFS(parts[2], comPort));
                        sendSerialMessage(comPort, msg);
                        msg = packageAircraftData();
                        sendSerialMessage(comPort, msg);
                    }
                    else
                    {
                        comPort.Dispose();
                    }
                }
                catch(Exception e)
                {
                    bool active = false;
                    Console.WriteLine("ERROR");
                    foreach(DBS_MFS device in MFS)
                    {
                        if(port == device.COM.PortName)
                        {
                            active = true;
                        }
                    }
                    if (!active)
                    {
                        comPort.Dispose();
                    }
                    //System.Windows.MessageBox.Show("");
                }
            }
        }

        private void ListenTMR_Tick(object sender, EventArgs e)
        {
            foreach (DBS_MFS device in MFS)
            {
                string inMsg = device.COM.ReadExisting();
                debugBox_tb.Text = inMsg;
                if(inMsg != "" && inMsg != null)
                {
                    string outMsg = processRequest(inMsg);
                }
            }
        }

        #region FSUIPC Variables
        private Offset<short> COM1ACT = new Offset<short>(0x034E);
        private Offset<short> COM1STBY = new Offset<short>(0x311A);
        private Offset<short> COM2ACT = new Offset<short>(0x3118);
        private Offset<short> COM2STBY = new Offset<short>(0x311C);
        private Offset<short> NAV1ACT = new Offset<short>(0x0350);
        private Offset<short> NAV1STBY = new Offset<short>(0x311E);
        private Offset<short> NAV2ACT = new Offset<short>(0x0352);
        private Offset<short> NAV2STBY = new Offset<short>(0x3120);
        private Offset<short> XPDR = new Offset<short>(0x354);
        private Offset<byte> XPDRState = new Offset<byte>(0x0B46); //TEST
        private Offset<long> Latitude = new Offset<long>(0x0560);
        private Offset<long> Longitude = new Offset<long>(0x0568);
        private PMDG_777X_Control PMDGTCASMode = PMDG_777X_Control.EVT_TCAS_MODE;
        private PMDG_777X_Control PMDGIDENT = PMDG_777X_Control.EVT_TCAS_IDENT;
        //private PMDG_777X_Offsets PMDG777Off = new PMDG_777X_Offsets();

        private Offset<byte> RadioSwitch = new Offset<byte>(0x3122);
        private Offset<byte> RadioSwap = new Offset<byte>(0x3123);
        #endregion

        #region FSUIPC Functions
        public double getLatitude()
        {
            FSUIPCConnection.Process();
            FsLatitude Lat = new FsLatitude(Latitude.Value);
            //FsLongitude Lon = new FsLongitude(Longitude.Value);
            return Lat.DecimalDegrees;
        }

        public double getLongitude()
        {
            FSUIPCConnection.Process();
            //FsLatitude Lat = new FsLatitude(Latitude.Value);
            FsLongitude Lon = new FsLongitude(Longitude.Value);
            return Lon.DecimalDegrees;
        }

        public void setCOM1Sby(string strFreq)
        {
            COM1STBY.Value = processFreqToHex(strFreq);
            FSUIPCConnection.Process();
        }

        public void setCOM2Sby(string strFreq)
        {
            COM2STBY.Value = processFreqToHex(strFreq);
            FSUIPCConnection.Process();
        }

        public void setNAV1Sby(string strFreq)
        {
            NAV1STBY.Value = processFreqToHex(strFreq);
            FSUIPCConnection.Process();
        }

        public void setNAV2Sby(string strFreq)
        {
            NAV2STBY.Value = processFreqToHex(strFreq);
            FSUIPCConnection.Process();
        }

        public void swapCOM1()
        {
            RadioSwap.Value = 8;
            FSUIPCConnection.Process();
            RadioSwap.Value = 0;
            FSUIPCConnection.Process();
        }

        public void swapCOM2()
        {
            RadioSwap.Value = 4;
            FSUIPCConnection.Process();
            RadioSwap.Value = 0;
            FSUIPCConnection.Process();
        }

        public void swapNAV1()
        {
            RadioSwap.Value = 2;
            FSUIPCConnection.Process();
            RadioSwap.Value = 0;
            FSUIPCConnection.Process();
        }

        public void swapNAV2()
        {
            RadioSwap.Value = 1;
            FSUIPCConnection.Process();
            RadioSwap.Value = 0;
            FSUIPCConnection.Process();
        }

        public void setXPDR(string strCode)
        {
            //uint[] codeArr = new uint[4];

            //codeArr[0] = Convert.ToUInt32(strCode[0].ToString());
            //codeArr[1] = Convert.ToUInt32(strCode[1].ToString());
            //codeArr[2] = Convert.ToUInt32(strCode[2].ToString());
            //codeArr[3] = Convert.ToUInt32(strCode[3].ToString());

            //byte thou = Convert.ToByte(codeArr[0]);
            //byte houn = Convert.ToByte(codeArr[1]);
            //byte ten = Convert.ToByte(codeArr[2]);
            //byte one = Convert.ToByte(codeArr[3]);

            //int temp = thou;
            //temp = (temp << 4) ^ houn;
            //temp = (temp << 4) ^ ten;
            //temp = (temp << 4) ^ one;

            XPDR.Value = processXPDRToHex(strCode);
            FSUIPCConnection.Process();

            //simConnect.TransmitClientEvent(1, EventID.EVENT_XPDRSet, (uint)temp, GroupID.DEFAULT, 0);
        }

        public void setXPDRMode(uint MODE)
        {
            //not Event but variable
            //simConnect.TransmitClientEvent(1, EventID.EVENT_XPDRMode, MODE, GroupID.DEFAULT, 0);
            XPDRState.Value = Convert.ToByte(MODE);
            //PMDG 777 not listening...
            //PMDGXPDRState.Value = Convert.ToByte(MODE);
            FSUIPCConnection.Process();
        }

        public void TX_COM1()
        {
            //simConnect.TransmitClientEvent(1, EventID.EVENT_COM1Act, 0, GroupID.DEFAULT, 0);
            RadioSwitch.Value = 128;
            FSUIPCConnection.Process();
            RadioSwitch.Value = 0;
        }

        public void TX_COM2()
        {
            //simConnect.TransmitClientEvent(1, EventID.EVENT_COM2Act, 0, GroupID.DEFAULT, 0);
            RadioSwitch.Value = 64;
            FSUIPCConnection.Process();
            RadioSwitch.Value = 0;
        }

        public void RX_COM1(uint tgl)
        {
            //simConnect.TransmitClientEvent(1, EventID.EVENT_COM1RX, tgl, GroupID.DEFAULT, 0);
            RadioSwitch.Value = 32;
            FSUIPCConnection.Process();
            //RadioSwitch.Value = 0;
        }

        public void RX_COM2(uint tgl)
        {
            //simConnect.TransmitClientEvent(1, EventID.EVENT_COM2RX, tgl, GroupID.DEFAULT, 0);
            RadioSwitch.Value = 32;
            FSUIPCConnection.Process();
            //RadioSwitch.Value = 0;
        }

        public void IDENT()
        {
            //simConnect.TransmitClientEvent(1, EventID.EVENT_IDENT, 0, GroupID.DEFAULT, 0);
        }
        #endregion

        #region Misc

        private bool verifyFreq(string Freq)
        {
            string[] temp = Freq.Split('.');
            int iFreq = Convert.ToInt32(temp[0]);
            if (minFreq <= iFreq && iFreq <= maxFreq)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string processFreqToString(short hex)
        {
            string output = "";

            string temp = hex.ToString("X");
            if (temp[3] == '0' || temp[3] == '5')
            {
                output = "1" + temp[0] + temp[1] + "." + temp[2] + temp[3] + "0";
            }
            else if (temp[3] == '2' || temp[3] == '7')
            {
                output = "1" + temp[0] + temp[1] + "." + temp[2] + temp[3] + "5";
            }

            return output;
        }

        private short processFreqToHex(string str)
        {
            short output = 0;
            string temp = str.Replace(".", "");
            string number = temp.Substring(1, 4);//str[1] + str[2] + str[4] + str[5];

            output = Convert.ToInt16(number, 16);

            return output;
        }

        private string processXPDRToString(short hex)
        {
            string output = hex.ToString("X");

            return output;
        }

        private short processXPDRToHex(string str)
        {
            short output = Convert.ToInt16(str, 16);
            return output;
        }
        #endregion

        private void CheckForConnectionsTMR_Tick(object sender, EventArgs e)
        {
            lookForConnections();
        }
    }
}
