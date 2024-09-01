using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CommsPanel_Interface
{
    public partial class Form1 : Form
    {
        VATSIM VatDATA;
        // KEYS = ["!","@","#","$","%","^"]
        public Form1()
        {
            InitializeComponent();
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
            debugBox_tb.Text = processRequest("$|D1|MMMX_ATIS");
        }

        private void timer1_Tick(object sender, EventArgs e)
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

        private string processRequest(string inRqst)
        {
            string header = "^|PC|";
            string body = "";
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
                //Store Serial Connection
            }
            else if (msgParts[0] == "$")
            {
                //ATIS Request
                string atisText = VatDATA.atis.First(item => item.callsign == msgParts[2]).text_atis;
            }

            return header;
            
        }
    }
}
