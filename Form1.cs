using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Media;

using System.Runtime.InteropServices;
using FSUIPC;
using System.Windows.Media.TextFormatting;

namespace DB_A25C_Handheld
{
    public partial class Handheld : Form
    {

        #region FSUIPC Variables
        private Offset<short> COM1ACT = new Offset<short>(0x034E);
        private Offset<short> COM1STBY = new Offset<short>(0x311A);
        private Offset<short> COM2ACT = new Offset<short>(0x3118);
        private Offset<short> COM2STBY = new Offset<short>(0x311C);
        private Offset<short> XPDR = new Offset<short>(0x354);
        private Offset<byte> XPDRState = new Offset<byte>(0x0B46); //TEST
        private PMDG_777X_Control PMDGTCASMode = PMDG_777X_Control.EVT_TCAS_MODE;
        private PMDG_777X_Control PMDGIDENT = PMDG_777X_Control.EVT_TCAS_IDENT;
        private PMDG_777X_Offsets PMDG777Off = new PMDG_777X_Offsets();

        private Offset<byte> RadioSwitch = new Offset<byte>(0x3122);
        private Offset<byte> RadioSwap = new Offset<byte>(0x3123);
        #endregion

        #region Globals
        //public SimConnect simConnect = null;
        enum ScreenPage 
        { 
            Menu,
            COM1,
            COM2,
            XPDR,
            FLTD
        }
        ScreenPage Page;
        enum MenuSelection
        {
            COM1,
            COM2,
            XPDR,
            FLTD
        }

        bool Connection = false;

        MenuSelection SEL;
        public bool COM1Active = true;
        public bool PWROn = false;
        public bool XPDRActive = false;
        //SimQuery SimData;
        public bool ConnStatus;
        public string Com1StbyOLD = "";
        public string Com2StbyOLD = "";
        public string OldCode = "";
        public int digits = 0;
        public bool C1Change = false;
        public bool C2Change = false;
        public bool XPDRChange = false;
        public bool InitAct = false;
        public int pulseCount = 0;
        public int maxFreq = 136;
        public int minFreq = 118;
        public bool initCOM1 = true;

        public enum DEFINITIONS
        {
            FlightData
        }

        private enum EventID
        {
            EVENT_COM1Swap,
            EVENT_COM1SbySet,
            EVENT_COM2Swap,
            EVENT_COM2SbySet,
            EVENT_COM1ActSet,
            EVENT_COM2ActSet,
            EVENT_XPDRSet,
            EVENT_IDENT,
            //EVENT_XPDRMode,
            EVENT_COM1Act,
            EVENT_COM2Act,
            EVENT_COM1RX,
            EVENT_COM2RX
        }

        private enum GroupID
        {
            DEFAULT
        }

        public enum DATA_REQUESTS
        {
            FlightData
        }

        private enum DATA_REQUEST_ID
        {
            REQUEST_1,
            REQUEST_2,
        };

        //[StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct FlightData
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x100)]
            public string COM1Type;
            public string COM2Type;
            public bool Com1Act;
            public double altitude;
            public double groundspeed;
            public double heading;
            public double verticalspeed;
        }

        #endregion

        public Handheld()
        {
            InitializeComponent();
            this.TopMost = true;
            //openFSUIPC();
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

        #region FuncButtons
        private void PWR_But_Click(object sender, EventArgs e)
        {
            //Power Off is broken
            if (PWROn)
            {
                if (Connection)
                {
                    //simConnect.Dispose();
                    //simConnect = null;
                    FSUIPCConnection.Close();
                }
                MainScreen.Visible = false;
                Query.Enabled = false;
                if (C1RX_Act.BackColor == Color.Lime)
                {
                    C1RX_Act.BackColor = SystemColors.ControlDarkDark;
                }
                if (C2RX_Act.BackColor == Color.Lime)
                {
                    C2RX_Act.BackColor = SystemColors.ControlDarkDark;
                }
            }
            else
            {
                if (!Connection)
                {
                    try
                    {
                        //simConnect = new SimConnect("Handheld Radio", base.Handle, 0x402, null, 0);
                        //initSimConnect();
                        openFSUIPC();
                        //Inits
                        Alt_Data.Text = "0";
                        GS_Data.Text = "0";
                        HDG_Data.Text = "0";
                        VS_Data.Text = "0";
                        C1Type.Text = "";
                        COM2FreqType.Text = "";
                        Query.Enabled = true;
                    }
                    catch (COMException)
                    {
                        //debugTB.AppendText("Sim Not Available");
                        MessageBox.Show("Unable to Connect the MSFS");
                    }
                }
                else
                {
                    //debugTB.AppendText("Sim Not Available");
                    MessageBox.Show("Unknown Error: Try Again");
                }
                if (MainScreen.Visible)
                {
                    MainScreen.Visible = false;
                }
                else if (Connection)
                {
                    MainScreen.Visible = true;
                    Page = ScreenPage.Menu;
                    C1RX_Act.BackColor = Color.Lime;
                    PWROn = true;
                    SEL = MenuSelection.COM1;
                    //SimData = new SimQuery(simConnect);
                    FSUIPCConnection.Process();
                    C1ActFreq.Text = processFreqToString(COM1ACT.Value);
                    C1StbyFreq.Text = processFreqToString(COM1STBY.Value);
                    COM2ActFreq.Text = processFreqToString(COM2ACT.Value);
                    COM2StbyFreq.Text = processFreqToString(COM2STBY.Value);
                    XPDRCode.Text = processFreqToString(XPDR.Value);
                    //setXPDR(XPDRCode.Text);
                    RX_COM1((uint)1);
                    RX_COM2((uint)0);
                    //Sync Com1 Tx??
                    //SimData.setXPDRMode((uint)4);
                }
            }
        }

        private void C1to2_But_Click(object sender, EventArgs e)
        {
            if (PWROn)
            {
                if (COM1Active)
                {
                    COM1Active = false;
                    if(C2RX_Act.BackColor != Color.Lime)
                    {
                        C2RX_Act.BackColor = Color.Lime;
                        RX_COM2((uint)1);
                    }
                    if (C1RX_Act.BackColor != Color.Gray)
                    {
                        C1RX_Act.BackColor = Color.Gray;
                        RX_COM1((uint)1);
                    }
                    TX_COM2();
                }
                else
                {
                    COM1Active = true;
                    if (C1RX_Act.BackColor != Color.Lime)
                    {
                        C1RX_Act.BackColor = Color.Lime;
                        RX_COM1((uint)1);
                    }
                    if (C2RX_Act.BackColor != Color.Gray)
                    {
                        C2RX_Act.BackColor = Color.Gray;
                        RX_COM2((uint)1);
                    }
                    TX_COM1();
                }
            }
        }

        private void C1RX_But_Click(object sender, EventArgs e)
        {
            if (PWROn)
            {
                if (C1RX_Act.BackColor == Color.Lime)
                {
                        C1RX_Act.BackColor = SystemColors.ControlDarkDark;
                        COM1Active = false;
                        RX_COM1((uint)0);
                }
                else
                {
                    C1RX_Act.BackColor = Color.Lime;
                    RX_COM1((uint)1);
                }
            }
        }

        private void C2RX_But_Click(object sender, EventArgs e)
        {
            if (PWROn)
            {
                if (C2RX_Act.BackColor == Color.Lime)
                {
                    C2RX_Act.BackColor = SystemColors.ControlDarkDark;
                    COM1Active = true;
                    RX_COM2((uint)0);
                }
                else
                {
                    C2RX_Act.BackColor = Color.Lime;
                    RX_COM2((uint)1);
                }
            }
        }

        private void Menu_But_Click(object sender, EventArgs e)
        {
            if (PWROn)
            {
                if (MainScreen.SelectedIndex != 0)
                {
                    MainScreen.SelectedIndex = 0;
                    Page = ScreenPage.Menu;
                }
            }
        }

        private void ENT_But_Click(object sender, EventArgs e)
        {
            if (PWROn)
            {
                if (Page == ScreenPage.Menu)
                {
                    if (SEL == MenuSelection.COM1)
                    {
                        MainScreen.SelectedIndex = 1;
                        Page = ScreenPage.COM1;
                        Com1StbyOLD = C1StbyFreq.Text;
                    }
                    else if (SEL == MenuSelection.COM2)
                    {
                        MainScreen.SelectedIndex = 2;
                        Page = ScreenPage.COM2;
                        Com2StbyOLD = COM2StbyFreq.Text;
                    }
                    else if (SEL == MenuSelection.XPDR)
                    {
                        MainScreen.SelectedIndex = 3;
                        Page = ScreenPage.XPDR;
                        OldCode = XPDRCode.Text;
                    }
                    else if (SEL == MenuSelection.FLTD)
                    {
                        MainScreen.SelectedIndex = 4;
                        Page = ScreenPage.FLTD;
                    }
                }
                else if(Page == ScreenPage.COM1)
                {
                    if (C1Change)
                    {
                        int pad = 7 - digits;
                        if(pad > 3)
                        {
                            int temp = pad - 4;
                            for(int i = 0; i < temp; i++)
                            {
                                C1StbyFreq.Text = C1StbyFreq.Text + "0";
                            }
                            C1StbyFreq.Text = C1StbyFreq.Text + ".000";
                        }
                        else
                        {
                            for (int i = 0; i < pad; i++)
                            {
                                C1StbyFreq.Text = C1StbyFreq.Text + "0";
                            }
                        }
                        digits = 0;
                        pulseCount = 0;
                        C1Change = false;
                        Sec_Pulse.Enabled = false;
                        C1StbyFreq.Visible = true;
                        bool goodFreq = verifyFreq(C1StbyFreq.Text);
                        if (goodFreq)
                        {
                            Com1StbyOLD = C1StbyFreq.Text;
                            setCOM1Sby(C1StbyFreq.Text);
                        }
                        else
                        {
                            C1StbyFreq.Text = Com1StbyOLD;
                            digits = 0;
                        }
                    }
                    else
                    {
                        swapCOM1();
                    }
                }
                else if(Page == ScreenPage.COM2)
                {
                    if (C2Change)
                    {
                        int pad = 7 - digits;
                        if (pad > 3)
                        {
                            int temp = pad - 4;
                            for (int i = 0; i < temp; i++)
                            {
                                COM2StbyFreq.Text = COM2StbyFreq.Text + "0";
                            }
                            COM2StbyFreq.Text = COM2StbyFreq.Text + ".000";
                        }
                        else
                        {
                            for (int i = 0; i < pad; i++)
                            {
                                COM2StbyFreq.Text = COM2StbyFreq.Text + "0";
                            }
                        }
                        digits = 0;
                        pulseCount = 0;
                        C2Change = false;
                        Sec_Pulse.Enabled = false;
                        COM2StbyFreq.Visible = true;
                        bool goodFreq = verifyFreq(COM2StbyFreq.Text);
                        if (goodFreq)
                        {
                            Com2StbyOLD = COM2StbyFreq.Text;
                            setCOM2Sby(COM2StbyFreq.Text);
                        }
                        else
                        {
                            COM2StbyFreq.Text = Com2StbyOLD;
                            digits = 0;
                        }
                    }
                    else
                    {
                        swapCOM2();
                    }
                }
            }
        }

        private void IDENT_But_Click(object sender, EventArgs e)
        {
            if (PWROn)
            {
                IDENT();
            }
        }

        private void CLR_But_Click(object sender, EventArgs e)
        {
            if (Page == ScreenPage.COM1)
            {
                if (C1Change)
                {
                    char lastCharacter = C1StbyFreq.Text[C1StbyFreq.Text.Length - 1];
                    if (lastCharacter == '.')
                    {
                        C1StbyFreq.Text = C1StbyFreq.Text.Remove(C1StbyFreq.Text.Length - 2);
                        digits = digits - 2;
                    }
                    else
                    {
                        C1StbyFreq.Text = C1StbyFreq.Text.Remove(C1StbyFreq.Text.Length - 1);
                        digits = digits - 1;
                    }
                }
                if (digits == 0)
                {
                    C1StbyFreq.Text = Com1StbyOLD;
                    Sec_Pulse.Enabled = false;
                    C1StbyFreq.Visible = true;
                    C1Change = false;
                }
            }
            else if (Page == ScreenPage.COM2)
            {
                if (C2Change)
                {
                    char lastCharacter = COM2StbyFreq.Text[COM2StbyFreq.Text.Length - 1];
                    if (lastCharacter == '.')
                    {
                        COM2StbyFreq.Text = COM2StbyFreq.Text.Remove(COM2StbyFreq.Text.Length - 2);
                        digits = digits - 2;
                    }
                    else
                    {
                        COM2StbyFreq.Text = COM2StbyFreq.Text.Remove(COM2StbyFreq.Text.Length - 1);
                        digits = digits - 1;
                    }
                    if (digits == 0)
                    {
                        COM2StbyFreq.Text = Com2StbyOLD;
                        Sec_Pulse.Enabled = false;
                        COM2StbyFreq.Visible = true;
                        C2Change = false;
                    }
                }
            }
            else if (Page == ScreenPage.XPDR)
            {
                if (XPDRChange)
                {
                    XPDRCode.Text = XPDRCode.Text.Remove(XPDRCode.Text.Length - 1);
                    digits = digits - 1;
                }
                if (digits == 0)
                {
                    XPDRCode.Text = OldCode;
                    Sec_Pulse.Enabled = false;
                    XPDRCode.Visible = true;
                    XPDRChange = false;
                }
            }
        }
        #endregion

        #region DirectionalPad
        private void Up_But_Click(object sender, EventArgs e)
        {
            if (PWROn)
            {
                if (SEL == MenuSelection.COM1 || SEL == MenuSelection.COM2)
                {
                    ;
                }
                else if (SEL == MenuSelection.XPDR)
                {
                    SEL = MenuSelection.COM1;
                    Menu_XPDR.BackColor = Color.Black;
                    Menu_XPDR.ForeColor = Color.White;
                    Menu_COM1.BackColor = Color.Gray;
                    Menu_COM1.ForeColor = Color.Black;
                }
                else if (SEL == MenuSelection.FLTD)
                {
                    SEL = MenuSelection.COM2;
                    Menu_FLTD.BackColor = Color.Black;
                    Menu_FLTD.ForeColor = Color.White;
                    Menu_COM2.BackColor = Color.Gray;
                    Menu_COM2.ForeColor = Color.Black;
                }
            }
        }

        private void Left_But_Click(object sender, EventArgs e)
        {
            if (PWROn)
            {
                if (Page == ScreenPage.Menu)
                {
                    if (SEL == MenuSelection.COM1 || SEL == MenuSelection.XPDR)
                    {
                        ;
                    }
                    else if (SEL == MenuSelection.COM2)
                    {
                        SEL = MenuSelection.COM1;
                        Menu_COM2.BackColor = Color.Black;
                        Menu_COM2.ForeColor = Color.White;
                        Menu_COM1.BackColor = Color.Gray;
                        Menu_COM1.ForeColor = Color.Black;
                    }
                    else if (SEL == MenuSelection.FLTD)
                    {
                        SEL = MenuSelection.XPDR;
                        Menu_FLTD.BackColor = Color.Black;
                        Menu_FLTD.ForeColor = Color.White;
                        Menu_XPDR.BackColor = Color.Gray;
                        Menu_XPDR.ForeColor = Color.Black;
                    }
                }
                else if(Page == ScreenPage.XPDR)
                {
                    if(XPDRAlt.FlatAppearance.BorderSize == 1)
                    {
                        XPDRAlt.FlatAppearance.BorderSize = 0;
                        XPDROff.FlatAppearance.BorderSize = 1;
                        //XPDR Mode Change Logic
                        setXPDRMode(0);
                    }
                }
            }
        }

        private void Dwn_But_Click(object sender, EventArgs e)
        {
            if (PWROn)
            {
                if (SEL == MenuSelection.XPDR || SEL == MenuSelection.FLTD)
                {
                    ;
                }
                else if (SEL == MenuSelection.COM1)
                {
                    SEL = MenuSelection.XPDR;
                    Menu_COM1.BackColor = Color.Black;
                    Menu_COM1.ForeColor = Color.White;
                    Menu_XPDR.BackColor = Color.Gray;
                    Menu_XPDR.ForeColor = Color.Black;
                }
                else if (SEL == MenuSelection.COM2)
                {
                    SEL = MenuSelection.FLTD;
                    Menu_COM2.BackColor = Color.Black;
                    Menu_COM2.ForeColor = Color.White;
                    Menu_FLTD.BackColor = Color.Gray;
                    Menu_FLTD.ForeColor = Color.Black;
                }
            }
        }

        private void Right_But_Click(object sender, EventArgs e)
        {
            if (PWROn)
            {
                if (Page == ScreenPage.Menu)
                {
                    if (SEL == MenuSelection.COM2 || SEL == MenuSelection.FLTD)
                    {
                        ;
                    }
                    else if (SEL == MenuSelection.COM1)
                    {
                        SEL = MenuSelection.COM2;
                        Menu_COM1.BackColor = Color.Black;
                        Menu_COM1.ForeColor = Color.White;
                        Menu_COM2.BackColor = Color.Gray;
                        Menu_COM2.ForeColor = Color.Black;
                    }
                    else if (SEL == MenuSelection.XPDR)
                    {
                        SEL = MenuSelection.FLTD;
                        Menu_XPDR.BackColor = Color.Black;
                        Menu_XPDR.ForeColor = Color.White;
                        Menu_FLTD.BackColor = Color.Gray;
                        Menu_FLTD.ForeColor = Color.Black;
                    }
                }
                else if (Page == ScreenPage.XPDR)
                {
                    if (XPDROff.FlatAppearance.BorderSize == 1)
                    {
                        XPDRAlt.FlatAppearance.BorderSize = 1;
                        XPDROff.FlatAppearance.BorderSize = 0;
                        //XPDR Mode Change Logic
                        setXPDRMode(4);
                    }
                }
            }
        }
        #endregion

        #region Keypad
        private void But1_Click(object sender, EventArgs e)
        {
            if (PWROn)
            {
                if(Page == ScreenPage.COM1)
                {
                    if(C1StbyFreq.Text == Com1StbyOLD)
                    {
                        C1StbyFreq.Text = "1";
                        Sec_Pulse.Enabled = true;
                        C1Change = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        C1StbyFreq.Text = C1StbyFreq.Text + "1";
                        digits = digits + 1;
                        if(digits == 3)
                        {
                            C1StbyFreq.Text = C1StbyFreq.Text + ".";
                            digits = digits + 1;
                        }
                        if(digits == 7)
                        {
                            digits = 0;
                            pulseCount = 0;
                            C1Change = false;
                            Sec_Pulse.Enabled = false;
                            C1StbyFreq.Visible = true;
                            bool goodFreq = verifyFreq(C1StbyFreq.Text);
                            if (goodFreq)
                            {
                                Com1StbyOLD = C1StbyFreq.Text;
                                setCOM1Sby(C1StbyFreq.Text);
                            }
                            else
                            {
                                C1StbyFreq.Text = Com1StbyOLD;
                                digits = 0;
                            }
                        }
                    }
                }
                else if (Page == ScreenPage.COM2)
                {
                    if (COM2StbyFreq.Text == Com2StbyOLD)
                    {
                        COM2StbyFreq.Text = "1";
                        Sec_Pulse.Enabled = true;
                        C2Change = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        COM2StbyFreq.Text = COM2StbyFreq.Text + "1";
                        digits = digits + 1;
                        if (digits == 3)
                        {
                            COM2StbyFreq.Text = COM2StbyFreq.Text + ".";
                            digits = digits + 1;
                        }
                        if (digits == 7)
                        {
                            digits = 0;
                            pulseCount = 0;
                            C2Change = false;
                            Sec_Pulse.Enabled = false;
                            COM2StbyFreq.Visible = true;
                            bool goodFreq = verifyFreq(COM2StbyFreq.Text);
                            if (goodFreq)
                            {
                                Com2StbyOLD = COM2StbyFreq.Text;
                                setCOM2Sby(COM2StbyFreq.Text);
                            }
                            else
                            {
                                COM2StbyFreq.Text = Com2StbyOLD;
                                digits = 0;
                            }
                        }
                    }
                }
                else if (Page == ScreenPage.XPDR)
                {
                    if (XPDRCode.Text == OldCode)
                    {
                        XPDRCode.Text = "1";
                        Sec_Pulse.Enabled = true;
                        XPDRChange = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        XPDRCode.Text = XPDRCode.Text + "1";
                        digits = digits + 1;
                        if (digits == 4)
                        {
                            digits = 0;
                            pulseCount = 0;
                            XPDRChange = false;
                            Sec_Pulse.Enabled = false;
                            XPDRCode.Visible = true;
                            //bool goodFreq = verifyFreq(COM2StbyFreq.Text);
                            OldCode = XPDRCode.Text;
                            setXPDR(XPDRCode.Text);
                            if (XPDRCode.Text == "7500" || XPDRCode.Text == "7600" || XPDRCode.Text == "7700")
                            {
                                XPDRCode.ForeColor = Color.Red;
                            }
                            else
                            {
                                XPDRCode.ForeColor = Color.White;
                            }
                        }
                    }
                }
            }
        }

        private void But2_Click(object sender, EventArgs e)
        {
            if (PWROn)
            {
                if (Page == ScreenPage.COM1)
                {
                    if (C1StbyFreq.Text == Com1StbyOLD)
                    {
                        C1StbyFreq.Text = "2";
                        Sec_Pulse.Enabled = true;
                        C1Change = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        C1StbyFreq.Text = C1StbyFreq.Text + "2";
                        digits = digits + 1;
                        if (digits == 3)
                        {
                            C1StbyFreq.Text = C1StbyFreq.Text + ".";
                            digits = digits + 1;
                        }
                        if (digits == 7)
                        {
                            digits = 0;
                            pulseCount = 0;
                            C1Change = false;
                            Sec_Pulse.Enabled = false;
                            C1StbyFreq.Visible = true;
                            bool goodFreq = verifyFreq(C1StbyFreq.Text);
                            if (goodFreq)
                            {
                                Com1StbyOLD = C1StbyFreq.Text;
                                setCOM1Sby(C1StbyFreq.Text);
                            }
                            else
                            {
                                C1StbyFreq.Text = Com1StbyOLD;
                                digits = 0;
                            }
                        }
                    }
                }
                else if (Page == ScreenPage.COM2)
                {
                    if (COM2StbyFreq.Text == Com2StbyOLD)
                    {
                        COM2StbyFreq.Text = "2";
                        Sec_Pulse.Enabled = true;
                        C2Change = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        COM2StbyFreq.Text = COM2StbyFreq.Text + "2";
                        digits = digits + 1;
                        if (digits == 3)
                        {
                            COM2StbyFreq.Text = COM2StbyFreq.Text + ".";
                            digits = digits + 1;
                        }
                        if (digits == 7)
                        {
                            digits = 0;
                            pulseCount = 0;
                            C2Change = false;
                            Sec_Pulse.Enabled = false;
                            COM2StbyFreq.Visible = true;
                            bool goodFreq = verifyFreq(COM2StbyFreq.Text);
                            if (goodFreq)
                            {
                                Com2StbyOLD = COM2StbyFreq.Text;
                                setCOM2Sby(COM2StbyFreq.Text);
                            }
                            else
                            {
                                COM2StbyFreq.Text = Com2StbyOLD;
                                digits = 0;
                            }
                        }
                    }
                }
                else if (Page == ScreenPage.XPDR)
                {
                    if (XPDRCode.Text == OldCode)
                    {
                        XPDRCode.Text = "2";
                        Sec_Pulse.Enabled = true;
                        XPDRChange = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        XPDRCode.Text = XPDRCode.Text + "2";
                        digits = digits + 1;
                        if (digits == 4)
                        {
                            digits = 0;
                            pulseCount = 0;
                            XPDRChange = false;
                            Sec_Pulse.Enabled = false;
                            XPDRCode.Visible = true;
                            //bool goodFreq = verifyFreq(COM2StbyFreq.Text);
                            OldCode = XPDRCode.Text;
                            setXPDR(XPDRCode.Text);
                            if (XPDRCode.Text == "7500" || XPDRCode.Text == "7600" || XPDRCode.Text == "7700")
                            {
                                XPDRCode.ForeColor = Color.Red;
                            }
                            else
                            {
                                XPDRCode.ForeColor = Color.White;
                            }
                        }
                    }
                }
            }
        }

        private void But3_Click(object sender, EventArgs e)
        {
            if (PWROn)
            {
                if (Page == ScreenPage.COM1)
                {
                    if (C1StbyFreq.Text == Com1StbyOLD)
                    {
                        C1StbyFreq.Text = "3";
                        Sec_Pulse.Enabled = true;
                        C1Change = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        C1StbyFreq.Text = C1StbyFreq.Text + "3";
                        digits = digits + 1;
                        if (digits == 3)
                        {
                            C1StbyFreq.Text = C1StbyFreq.Text + ".";
                            digits = digits + 1;
                        }
                        if (digits == 7)
                        {
                            digits = 0;
                            pulseCount = 0;
                            C1Change = false;
                            Sec_Pulse.Enabled = false;
                            C1StbyFreq.Visible = true;
                            bool goodFreq = verifyFreq(C1StbyFreq.Text);
                            if (goodFreq)
                            {
                                Com1StbyOLD = C1StbyFreq.Text;
                                setCOM1Sby(C1StbyFreq.Text);
                            }
                            else
                            {
                                C1StbyFreq.Text = Com1StbyOLD;
                                digits = 0;
                            }
                        }
                    }
                }
                else if (Page == ScreenPage.COM2)
                {
                    if (COM2StbyFreq.Text == Com2StbyOLD)
                    {
                        COM2StbyFreq.Text = "3";
                        Sec_Pulse.Enabled = true;
                        C2Change = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        COM2StbyFreq.Text = COM2StbyFreq.Text + "3";
                        digits = digits + 1;
                        if (digits == 3)
                        {
                            COM2StbyFreq.Text = COM2StbyFreq.Text + ".";
                            digits = digits + 1;
                        }
                        if (digits == 7)
                        {
                            digits = 0;
                            pulseCount = 0;
                            C2Change = false;
                            Sec_Pulse.Enabled = false;
                            COM2StbyFreq.Visible = true;
                            bool goodFreq = verifyFreq(COM2StbyFreq.Text);
                            if (goodFreq)
                            {
                                Com2StbyOLD = COM2StbyFreq.Text;
                                setCOM2Sby(COM2StbyFreq.Text);
                            }
                            else
                            {
                                COM2StbyFreq.Text = Com2StbyOLD;
                                digits = 0;
                            }
                        }
                    }
                }
                else if (Page == ScreenPage.XPDR)
                {
                    if (XPDRCode.Text == OldCode)
                    {
                        XPDRCode.Text = "3";
                        Sec_Pulse.Enabled = true;
                        XPDRChange = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        XPDRCode.Text = XPDRCode.Text + "3";
                        digits = digits + 1;
                        if (digits == 4)
                        {
                            digits = 0;
                            pulseCount = 0;
                            XPDRChange = false;
                            Sec_Pulse.Enabled = false;
                            XPDRCode.Visible = true;
                            //bool goodFreq = verifyFreq(COM2StbyFreq.Text);
                            OldCode = XPDRCode.Text;
                            setXPDR(XPDRCode.Text);
                            if (XPDRCode.Text == "7500" || XPDRCode.Text == "7600" || XPDRCode.Text == "7700")
                            {
                                XPDRCode.ForeColor = Color.Red;
                            }
                            else
                            {
                                XPDRCode.ForeColor = Color.White;
                            }
                        }
                    }
                }
            }
        }

        private void But4_Click(object sender, EventArgs e)
        {
            if (PWROn)
            {
                if (Page == ScreenPage.COM1)
                {
                    if (C1StbyFreq.Text == Com1StbyOLD)
                    {
                        C1StbyFreq.Text = "4";
                        Sec_Pulse.Enabled = true;
                        C1Change = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        C1StbyFreq.Text = C1StbyFreq.Text + "4";
                        digits = digits + 1;
                        if (digits == 3)
                        {
                            C1StbyFreq.Text = C1StbyFreq.Text + ".";
                            digits = digits + 1;
                        }
                        if (digits == 7)
                        {
                            digits = 0;
                            pulseCount = 0;
                            C1Change = false;
                            Sec_Pulse.Enabled = false;
                            C1StbyFreq.Visible = true;
                            bool goodFreq = verifyFreq(C1StbyFreq.Text);
                            if (goodFreq)
                            {
                                Com1StbyOLD = C1StbyFreq.Text;
                                setCOM1Sby(C1StbyFreq.Text);
                            }
                            else
                            {
                                C1StbyFreq.Text = Com1StbyOLD;
                                digits = 0;
                            }
                        }
                    }
                }
                else if (Page == ScreenPage.COM2)
                {
                    if (COM2StbyFreq.Text == Com2StbyOLD)
                    {
                        COM2StbyFreq.Text = "4";
                        Sec_Pulse.Enabled = true;
                        C2Change = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        COM2StbyFreq.Text = COM2StbyFreq.Text + "4";
                        digits = digits + 1;
                        if (digits == 3)
                        {
                            COM2StbyFreq.Text = COM2StbyFreq.Text + ".";
                            digits = digits + 1;
                        }
                        if (digits == 7)
                        {
                            digits = 0;
                            pulseCount = 0;
                            C2Change = false;
                            Sec_Pulse.Enabled = false;
                            COM2StbyFreq.Visible = true;
                            bool goodFreq = verifyFreq(COM2StbyFreq.Text);
                            if (goodFreq)
                            {
                                Com2StbyOLD = COM2StbyFreq.Text;
                                setCOM2Sby(COM2StbyFreq.Text);
                            }
                            else
                            {
                                COM2StbyFreq.Text = Com2StbyOLD;
                                digits = 0;
                            }
                        }
                    }
                }
                else if (Page == ScreenPage.XPDR)
                {
                    if (XPDRCode.Text == OldCode)
                    {
                        XPDRCode.Text = "4";
                        Sec_Pulse.Enabled = true;
                        XPDRChange = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        XPDRCode.Text = XPDRCode.Text + "4";
                        digits = digits + 1;
                        if (digits == 4)
                        {
                            digits = 0;
                            pulseCount = 0;
                            XPDRChange = false;
                            Sec_Pulse.Enabled = false;
                            XPDRCode.Visible = true;
                            //bool goodFreq = verifyFreq(COM2StbyFreq.Text);
                            OldCode = XPDRCode.Text;
                            setXPDR(XPDRCode.Text);
                            if (XPDRCode.Text == "7500" || XPDRCode.Text == "7600" || XPDRCode.Text == "7700")
                            {
                                XPDRCode.ForeColor = Color.Red;
                            }
                            else
                            {
                                XPDRCode.ForeColor = Color.White;
                            }
                        }
                    }
                }
            }
        }

        private void But5_Click(object sender, EventArgs e)
        {
            if (PWROn)
            {
                if (Page == ScreenPage.COM1)
                {
                    if (C1StbyFreq.Text == Com1StbyOLD)
                    {
                        C1StbyFreq.Text = "5";
                        Sec_Pulse.Enabled = true;
                        C1Change = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        C1StbyFreq.Text = C1StbyFreq.Text + "5";
                        digits = digits + 1;
                        if (digits == 3)
                        {
                            C1StbyFreq.Text = C1StbyFreq.Text + ".";
                            digits = digits + 1;
                        }
                        if (digits == 7)
                        {
                            digits = 0;
                            pulseCount = 0;
                            C1Change = false;
                            Sec_Pulse.Enabled = false;
                            C1StbyFreq.Visible = true;
                            bool goodFreq = verifyFreq(C1StbyFreq.Text);
                            if (goodFreq)
                            {
                                Com1StbyOLD = C1StbyFreq.Text;
                                setCOM1Sby(C1StbyFreq.Text);
                            }
                            else
                            {
                                C1StbyFreq.Text = Com1StbyOLD;
                                digits = 0;
                            }
                        }
                    }
                }
                else if (Page == ScreenPage.COM2)
                {
                    if (COM2StbyFreq.Text == Com2StbyOLD)
                    {
                        COM2StbyFreq.Text = "5";
                        Sec_Pulse.Enabled = true;
                        C2Change = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        COM2StbyFreq.Text = COM2StbyFreq.Text + "5";
                        digits = digits + 1;
                        if (digits == 3)
                        {
                            COM2StbyFreq.Text = COM2StbyFreq.Text + ".";
                            digits = digits + 1;
                        }
                        if (digits == 7)
                        {
                            digits = 0;
                            pulseCount = 0;
                            C2Change = false;
                            Sec_Pulse.Enabled = false;
                            COM2StbyFreq.Visible = true;
                            bool goodFreq = verifyFreq(COM2StbyFreq.Text);
                            if (goodFreq)
                            {
                                Com2StbyOLD = COM2StbyFreq.Text;
                                setCOM2Sby(COM2StbyFreq.Text);
                            }
                            else
                            {
                                COM2StbyFreq.Text = Com2StbyOLD;
                                digits = 0;
                            }
                        }
                    }
                }
                else if (Page == ScreenPage.XPDR)
                {
                    if (XPDRCode.Text == OldCode)
                    {
                        XPDRCode.Text = "5";
                        Sec_Pulse.Enabled = true;
                        XPDRChange = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        XPDRCode.Text = XPDRCode.Text + "5";
                        digits = digits + 1;
                        if (digits == 4)
                        {
                            digits = 0;
                            pulseCount = 0;
                            XPDRChange = false;
                            Sec_Pulse.Enabled = false;
                            XPDRCode.Visible = true;
                            //bool goodFreq = verifyFreq(COM2StbyFreq.Text);
                            OldCode = XPDRCode.Text;
                            setXPDR(XPDRCode.Text);
                            if (XPDRCode.Text == "7500" || XPDRCode.Text == "7600" || XPDRCode.Text == "7700")
                            {
                                XPDRCode.ForeColor = Color.Red;
                            }
                            else
                            {
                                XPDRCode.ForeColor = Color.White;
                            }
                        }
                    }
                }
            }
        }

        private void But6_Click(object sender, EventArgs e)
        {
            if (PWROn)
            {
                if (Page == ScreenPage.COM1)
                {
                    if (C1StbyFreq.Text == Com1StbyOLD)
                    {
                        C1StbyFreq.Text = "6";
                        Sec_Pulse.Enabled = true;
                        C1Change = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        C1StbyFreq.Text = C1StbyFreq.Text + "6";
                        digits = digits + 1;
                        if (digits == 3)
                        {
                            C1StbyFreq.Text = C1StbyFreq.Text + ".";
                            digits = digits + 1;
                        }
                        if (digits == 7)
                        {
                            digits = 0;
                            pulseCount = 0;
                            C1Change = false;
                            Sec_Pulse.Enabled = false;
                            C1StbyFreq.Visible = true;
                            bool goodFreq = verifyFreq(C1StbyFreq.Text);
                            if (goodFreq)
                            {
                                Com1StbyOLD = C1StbyFreq.Text;
                                setCOM1Sby(C1StbyFreq.Text);
                            }
                            else
                            {
                                C1StbyFreq.Text = Com1StbyOLD;
                                digits = 0;
                            }
                        }
                    }
                }
                else if (Page == ScreenPage.COM2)
                {
                    if (COM2StbyFreq.Text == Com2StbyOLD)
                    {
                        COM2StbyFreq.Text = "6";
                        Sec_Pulse.Enabled = true;
                        C2Change = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        COM2StbyFreq.Text = COM2StbyFreq.Text + "6";
                        digits = digits + 1;
                        if (digits == 3)
                        {
                            COM2StbyFreq.Text = COM2StbyFreq.Text + ".";
                            digits = digits + 1;
                        }
                        if (digits == 7)
                        {
                            digits = 0;
                            pulseCount = 0;
                            C2Change = false;
                            Sec_Pulse.Enabled = false;
                            COM2StbyFreq.Visible = true;
                            bool goodFreq = verifyFreq(COM2StbyFreq.Text);
                            if (goodFreq)
                            {
                                Com2StbyOLD = COM2StbyFreq.Text;
                                setCOM2Sby(COM2StbyFreq.Text);
                            }
                            else
                            {
                                COM2StbyFreq.Text = Com2StbyOLD;
                                digits = 0;
                            }
                        }
                    }
                }
                else if (Page == ScreenPage.XPDR)
                {
                    if (XPDRCode.Text == OldCode)
                    {
                        XPDRCode.Text = "6";
                        Sec_Pulse.Enabled = true;
                        XPDRChange = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        XPDRCode.Text = XPDRCode.Text + "6";
                        digits = digits + 1;
                        if (digits == 4)
                        {
                            digits = 0;
                            pulseCount = 0;
                            XPDRChange = false;
                            Sec_Pulse.Enabled = false;
                            XPDRCode.Visible = true;
                            //bool goodFreq = verifyFreq(COM2StbyFreq.Text);
                            OldCode = XPDRCode.Text;
                            setXPDR(XPDRCode.Text);
                            if (XPDRCode.Text == "7500" || XPDRCode.Text == "7600" || XPDRCode.Text == "7700")
                            {
                                XPDRCode.ForeColor = Color.Red;
                            }
                            else
                            {
                                XPDRCode.ForeColor = Color.White;
                            }
                        }
                    }
                }
            }
        }

        private void But7_Click(object sender, EventArgs e)
        {
            if (PWROn)
            {
                if (Page == ScreenPage.COM1)
                {
                    if (C1StbyFreq.Text == Com1StbyOLD)
                    {
                        C1StbyFreq.Text = "7";
                        Sec_Pulse.Enabled = true;
                        C1Change = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        C1StbyFreq.Text = C1StbyFreq.Text + "7";
                        digits = digits + 1;
                        if (digits == 3)
                        {
                            C1StbyFreq.Text = C1StbyFreq.Text + ".";
                            digits = digits + 1;
                        }
                        if (digits == 7)
                        {
                            digits = 0;
                            pulseCount = 0;
                            C1Change = false;
                            Sec_Pulse.Enabled = false;
                            C1StbyFreq.Visible = true;
                            bool goodFreq = verifyFreq(C1StbyFreq.Text);
                            if (goodFreq)
                            {
                                Com1StbyOLD = C1StbyFreq.Text;
                                setCOM1Sby(C1StbyFreq.Text);
                            }
                            else
                            {
                                C1StbyFreq.Text = Com1StbyOLD;
                                digits = 0;
                            }
                        }
                    }
                }
                else if (Page == ScreenPage.COM2)
                {
                    if (COM2StbyFreq.Text == Com2StbyOLD)
                    {
                        COM2StbyFreq.Text = "7";
                        Sec_Pulse.Enabled = true;
                        C2Change = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        COM2StbyFreq.Text = COM2StbyFreq.Text + "7";
                        digits = digits + 1;
                        if (digits == 3)
                        {
                            COM2StbyFreq.Text = COM2StbyFreq.Text + ".";
                            digits = digits + 1;
                        }
                        if (digits == 7)
                        {
                            digits = 0;
                            pulseCount = 0;
                            C2Change = false;
                            Sec_Pulse.Enabled = false;
                            COM2StbyFreq.Visible = true;
                            bool goodFreq = verifyFreq(COM2StbyFreq.Text);
                            if (goodFreq)
                            {
                                Com2StbyOLD = COM2StbyFreq.Text;
                                setCOM2Sby(COM2StbyFreq.Text);
                            }
                            else
                            {
                                COM2StbyFreq.Text = Com2StbyOLD;
                                digits = 0;
                            }
                        }
                    }
                }
                else if (Page == ScreenPage.XPDR)
                {
                    if (XPDRCode.Text == OldCode)
                    {
                        XPDRCode.Text = "7";
                        Sec_Pulse.Enabled = true;
                        XPDRChange = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        XPDRCode.Text = XPDRCode.Text + "7";
                        digits = digits + 1;
                        if (digits == 4)
                        {
                            digits = 0;
                            pulseCount = 0;
                            XPDRChange = false;
                            Sec_Pulse.Enabled = false;
                            XPDRCode.Visible = true;
                            //bool goodFreq = verifyFreq(COM2StbyFreq.Text);
                            OldCode = XPDRCode.Text;
                            setXPDR(XPDRCode.Text);
                            if (XPDRCode.Text == "7500" || XPDRCode.Text == "7600" || XPDRCode.Text == "7700")
                            {
                                XPDRCode.ForeColor = Color.Red;
                            }
                            else
                            {
                                XPDRCode.ForeColor = Color.White;
                            }
                        }
                    }
                }
            }
        }

        private void But8_Click(object sender, EventArgs e)
        {
            if (PWROn)
            {
                if (Page == ScreenPage.COM1)
                {
                    if (C1StbyFreq.Text == Com1StbyOLD)
                    {
                        C1StbyFreq.Text = "8";
                        Sec_Pulse.Enabled = true;
                        C1Change = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        C1StbyFreq.Text = C1StbyFreq.Text + "8";
                        digits = digits + 1;
                        if (digits == 3)
                        {
                            C1StbyFreq.Text = C1StbyFreq.Text + ".";
                            digits = digits + 1;
                        }
                        if (digits == 7)
                        {
                            digits = 0;
                            pulseCount = 0;
                            C1Change = false;
                            Sec_Pulse.Enabled = false;
                            C1StbyFreq.Visible = true;
                            bool goodFreq = verifyFreq(C1StbyFreq.Text);
                            if (goodFreq)
                            {
                                Com1StbyOLD = C1StbyFreq.Text;
                                setCOM1Sby(C1StbyFreq.Text);
                            }
                            else
                            {
                                C1StbyFreq.Text = Com1StbyOLD;
                                digits = 0;
                            }
                        }
                    }
                }
                else if (Page == ScreenPage.COM2)
                {
                    if (COM2StbyFreq.Text == Com2StbyOLD)
                    {
                        COM2StbyFreq.Text = "8";
                        Sec_Pulse.Enabled = true;
                        C2Change = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        COM2StbyFreq.Text = COM2StbyFreq.Text + "8";
                        digits = digits + 1;
                        if (digits == 3)
                        {
                            COM2StbyFreq.Text = COM2StbyFreq.Text + ".";
                            digits = digits + 1;
                        }
                        if (digits == 7)
                        {
                            digits = 0;
                            pulseCount = 0;
                            C2Change = false;
                            Sec_Pulse.Enabled = false;
                            COM2StbyFreq.Visible = true;
                            bool goodFreq = verifyFreq(COM2StbyFreq.Text);
                            if (goodFreq)
                            {
                                Com2StbyOLD = COM2StbyFreq.Text;
                                setCOM2Sby(COM2StbyFreq.Text);
                            }
                            else
                            {
                                COM2StbyFreq.Text = Com2StbyOLD;
                                digits = 0;
                            }
                        }
                    }
                }
                else if (Page == ScreenPage.XPDR)
                {
                    if (XPDRCode.Text == OldCode)
                    {
                        XPDRCode.Text = "8";
                        Sec_Pulse.Enabled = true;
                        XPDRChange = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        XPDRCode.Text = XPDRCode.Text + "8";
                        digits = digits + 1;
                        if (digits == 4)
                        {
                            digits = 0;
                            pulseCount = 0;
                            XPDRChange = false;
                            Sec_Pulse.Enabled = false;
                            XPDRCode.Visible = true;
                            //bool goodFreq = verifyFreq(COM2StbyFreq.Text);
                            OldCode = XPDRCode.Text;
                            setXPDR(XPDRCode.Text);
                            if (XPDRCode.Text == "7500" || XPDRCode.Text == "7600" || XPDRCode.Text == "7700")
                            {
                                XPDRCode.ForeColor = Color.Red;
                            }
                            else
                            {
                                XPDRCode.ForeColor = Color.White;
                            }
                        }
                    }
                }
            }
        }

        private void But9_Click(object sender, EventArgs e)
        {
            if (PWROn)
            {
                if (Page == ScreenPage.COM1)
                {
                    if (C1StbyFreq.Text == Com1StbyOLD)
                    {
                        C1StbyFreq.Text = "9";
                        Sec_Pulse.Enabled = true;
                        C1Change = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        C1StbyFreq.Text = C1StbyFreq.Text + "9";
                        digits = digits + 1;
                        if (digits == 3)
                        {
                            C1StbyFreq.Text = C1StbyFreq.Text + ".";
                            digits = digits + 1;
                        }
                        if (digits == 7)
                        {
                            digits = 0;
                            pulseCount = 0;
                            C1Change = false;
                            Sec_Pulse.Enabled = false;
                            C1StbyFreq.Visible = true;
                            bool goodFreq = verifyFreq(C1StbyFreq.Text);
                            if (goodFreq)
                            {
                                Com1StbyOLD = C1StbyFreq.Text;
                                setCOM1Sby(C1StbyFreq.Text);
                            }
                            else
                            {
                                C1StbyFreq.Text = Com1StbyOLD;
                                digits = 0;
                            }
                        }
                    }
                }
                else if (Page == ScreenPage.COM2)
                {
                    if (COM2StbyFreq.Text == Com2StbyOLD)
                    {
                        COM2StbyFreq.Text = "9";
                        Sec_Pulse.Enabled = true;
                        C2Change = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        COM2StbyFreq.Text = COM2StbyFreq.Text + "9";
                        digits = digits + 1;
                        if (digits == 3)
                        {
                            COM2StbyFreq.Text = COM2StbyFreq.Text + ".";
                            digits = digits + 1;
                        }
                        if (digits == 7)
                        {
                            digits = 0;
                            pulseCount = 0;
                            C2Change = false;
                            Sec_Pulse.Enabled = false;
                            COM2StbyFreq.Visible = true;
                            bool goodFreq = verifyFreq(COM2StbyFreq.Text);
                            if (goodFreq)
                            {
                                Com2StbyOLD = COM2StbyFreq.Text;
                                setCOM2Sby(COM2StbyFreq.Text);
                            }
                            else
                            {
                                COM2StbyFreq.Text = Com2StbyOLD;
                                digits = 0;
                            }
                        }
                    }
                }
                else if (Page == ScreenPage.XPDR)
                {
                    if (XPDRCode.Text == OldCode)
                    {
                        XPDRCode.Text = "9";
                        Sec_Pulse.Enabled = true;
                        XPDRChange = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        XPDRCode.Text = XPDRCode.Text + "9";
                        digits = digits + 1;
                        if (digits == 4)
                        {
                            digits = 0;
                            pulseCount = 0;
                            XPDRChange = false;
                            Sec_Pulse.Enabled = false;
                            XPDRCode.Visible = true;
                            //bool goodFreq = verifyFreq(COM2StbyFreq.Text);
                            OldCode = XPDRCode.Text;
                            setXPDR(XPDRCode.Text);
                            if (XPDRCode.Text == "7500" || XPDRCode.Text == "7600" || XPDRCode.Text == "7700")
                            {
                                XPDRCode.ForeColor = Color.Red;
                            }
                            else
                            {
                                XPDRCode.ForeColor = Color.White;
                            }
                        }
                    }
                }
            }
        }

        private void But0_Click(object sender, EventArgs e)
        {
            if (PWROn)
            {
                if (Page == ScreenPage.COM1)
                {
                    if (C1StbyFreq.Text == Com1StbyOLD)
                    {
                        C1StbyFreq.Text = "0";
                        Sec_Pulse.Enabled = true;
                        C1Change = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        C1StbyFreq.Text = C1StbyFreq.Text + "0";
                        digits = digits + 1;
                        if (digits == 3)
                        {
                            C1StbyFreq.Text = C1StbyFreq.Text + ".";
                            digits = digits + 1;
                        }
                        if (digits == 7)
                        {
                            digits = 0;
                            pulseCount = 0;
                            C1Change = false;
                            Sec_Pulse.Enabled = false;
                            C1StbyFreq.Visible = true;
                            bool goodFreq = verifyFreq(C1StbyFreq.Text);
                            if (goodFreq)
                            {
                                Com1StbyOLD = C1StbyFreq.Text;
                                setCOM1Sby(C1StbyFreq.Text);
                            }
                            else
                            {
                                C1StbyFreq.Text = Com1StbyOLD;
                                digits = 0;
                            }
                        }
                    }
                }
                else if (Page == ScreenPage.COM2)
                {
                    if (COM2StbyFreq.Text == Com2StbyOLD)
                    {
                        COM2StbyFreq.Text = "0";
                        Sec_Pulse.Enabled = true;
                        C2Change = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        COM2StbyFreq.Text = COM2StbyFreq.Text + "0";
                        digits = digits + 1;
                        if (digits == 3)
                        {
                            COM2StbyFreq.Text = COM2StbyFreq.Text + ".";
                            digits = digits + 1;
                        }
                        if (digits == 7)
                        {
                            digits = 0;
                            pulseCount = 0;
                            C2Change = false;
                            Sec_Pulse.Enabled = false;
                            COM2StbyFreq.Visible = true;
                            bool goodFreq = verifyFreq(COM2StbyFreq.Text);
                            if (goodFreq)
                            {
                                Com2StbyOLD = COM2StbyFreq.Text;
                                setCOM2Sby(COM2StbyFreq.Text);
                            }
                            else
                            {
                                COM2StbyFreq.Text = Com2StbyOLD;
                                digits = 0;
                            }
                        }
                    }
                }
                else if (Page == ScreenPage.XPDR)
                {
                    if (XPDRCode.Text == OldCode)
                    {
                        XPDRCode.Text = "0";
                        Sec_Pulse.Enabled = true;
                        XPDRChange = true;
                        digits = digits + 1;
                    }
                    else
                    {
                        XPDRCode.Text = XPDRCode.Text + "0";
                        digits = digits + 1;
                        if (digits == 4)
                        {
                            digits = 0;
                            pulseCount = 0;
                            XPDRChange = false;
                            Sec_Pulse.Enabled = false;
                            XPDRCode.Visible = true;
                            //bool goodFreq = verifyFreq(COM2StbyFreq.Text);
                            OldCode = XPDRCode.Text;
                            setXPDR(XPDRCode.Text);
                            if (XPDRCode.Text == "7500" || XPDRCode.Text == "7600" || XPDRCode.Text == "7700")
                            {
                                XPDRCode.ForeColor = Color.Red;
                            }
                            else
                            {
                                XPDRCode.ForeColor = Color.White;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Timers
        private void Query_Tick(object sender, EventArgs e)
        {
            if (Connection)
            {
                FSUIPCConnection.Process();
                //GOOD just need to toggle based on editing or not
                C1ActFreq.Text = processFreqToString(COM1ACT.Value);
                if (!C1Change)
                {
                    C1StbyFreq.Text = processFreqToString(COM1STBY.Value);
                }
                COM2ActFreq.Text = processFreqToString(COM2ACT.Value);
                if (!C2Change)
                {
                    COM2StbyFreq.Text = processFreqToString(COM2STBY.Value);
                }
                if (!XPDRChange)
                {
                    XPDRCode.Text = processXPDRToString(XPDR.Value);
                }
            }
            //simConnect.RequestDataOnSimObjectType(DATA_REQUESTS.FlightData, DEFINITIONS.FlightData, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
        }

        private void Sec_Pulse_Tick(object sender, EventArgs e)
        {
            if(pulseCount >= 120)
            {
                pulseCount = 0;
                Sec_Pulse.Enabled = false;
                if (C1Change)
                {
                    C1Change = false;
                    C1StbyFreq.Text = Com1StbyOLD;
                }
                if (C2Change)
                {
                    C2Change = false;
                    COM2StbyFreq.Text = Com2StbyOLD;
                }
                if (XPDRChange)
                {
                    XPDRChange = false;
                    XPDRCode.Text = OldCode;
                }
            }
            if (C1Change)
            {
                if (C1StbyFreq.Visible)
                {
                    C1StbyFreq.Visible = false;
                }
                else
                {
                    C1StbyFreq.Visible = true;
                }
                pulseCount = pulseCount + 1;
            }
            if (C2Change)
            {
                if (COM2StbyFreq.Visible)
                {
                    COM2StbyFreq.Visible = false;
                }
                else
                {
                    COM2StbyFreq.Visible = true;
                }
                pulseCount = pulseCount + 1;
            }
            if (XPDRChange)
            {
                if (XPDRCode.Visible)
                {
                    XPDRCode.Visible = false;
                }
                else
                {
                    XPDRCode.Visible = true;
                }
                pulseCount = pulseCount + 1;
            }
            if (InitAct) 
            {
                if (StatusPulse.Visible)
                {
                    StatusPulse.Visible = false;
                }
                else
                {
                    StatusPulse.Visible = false;
                }
            }
        }
        #endregion

        #region FSUIPC Functions
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

        public void swapCOM1()
        {
            RadioSwap.Value = 8;
            FSUIPCConnection.Process();
            RadioSwap.Value = 0;
        }

        public void swapCOM2()
        {
            RadioSwap.Value = 4;
            FSUIPCConnection.Process();
            RadioSwap.Value = 0;
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
            string[] temp =  Freq.Split('.');
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
            string temp = str.Replace(".","");
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

        private void Handheld_FormClosed(object sender, FormClosedEventArgs e)
        {
             FSUIPCConnection.Close();
        }
    }
}