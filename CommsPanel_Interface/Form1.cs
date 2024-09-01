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
            debugBox_tb.Text = packageControllers();
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
            string[] msgParts = inRqst.Split('|');

            if (msgParts[0] == "!")
            {
                //Action
            }
            else if (msgParts[0] == "#")
            {

            }
            else if (msgParts[0] == "$")
            {

            }
            else if (msgParts[0] == "%")
            {

            }

            return header;
        }
    }
}
