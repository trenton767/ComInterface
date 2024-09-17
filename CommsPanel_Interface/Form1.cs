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
//using FSUIPC;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace CommsPanel_Interface
{
    public partial class Form1 : Form
    {
        VATSIM VatDATA;
        List<DBS_MFS> MFS = new List<DBS_MFS>();
        // KEYS = ["!","@","#","$","%","^"]
        public Form1()
        {
            InitializeComponent();
            lookForConnections();
            scrapeVATSIM();
            //timer1.Start();
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
            acData = acData + "COM1_ACT,121.900|COM1_STBY,122.800|COM2_ACT,120.700|COM2_STBY,118.100|XPDR_CODE,1244|XPDR_MODE,ALT|NAV1_ACT,111.10|NAV1_STBY,111.15|NAV2_ACT,112.10|NAV2_STBY,112.20|C1TX,1|C1RX,1|";

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
                            }
                            else if (obj.Contains("COM2"))
                            {
                                //FSUIPC Set COM2 Stby to freq
                            }
                            else if (obj.Contains("NAV1"))
                            {
                                //FSUIPC Set NAV1 Stby to freq
                            }
                            else if (obj.Contains("NAV2"))
                            {
                                //FSUIPC Set NAV2 Stby to freq
                            }
                        }
                        if (obj.Contains("_SW"))
                        {
                            if (obj.Contains("COM1"))
                            {
                                //FSUIPC Switch COM1 Freqs
                            }
                            else if (obj.Contains("COM2"))
                            {
                                //FSUIPC Switch COM2 Freqs
                            }
                            else if (obj.Contains("NAV1"))
                            {
                                //FSUIPC Switch NAV1 Freqs
                            }
                            else if (obj.Contains("NAV2"))
                            {
                                //FSUIPC Switch NAV1 Freqs
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
                    comPort.Dispose();
                }
                catch(Exception e)
                {
                    Console.WriteLine("ERROR");
                    //System.Windows.MessageBox.Show("");
                }
            }
        }

        private void ListenTMR_Tick(object sender, EventArgs e)
        {
            foreach (DBS_MFS device in MFS)
            {
                string inMsg = device.COM.ReadExisting();
                if(inMsg != "" && inMsg != null)
                {
                    string outMsg = processRequest(inMsg);
                }
            }
        }
    }
}
