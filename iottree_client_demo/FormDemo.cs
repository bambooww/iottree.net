using iottree.lib;

namespace iottree_client_demo
{
    public partial class FormDemo : Form
    {
        IOTTreeClient client = null;

        public FormDemo()
        {
            InitializeComponent();
        }

        private void FormDemo_Load(object sender, EventArgs e)
        {
            //IOTTreeClient client = new IOTTreeClient("");
        }

        private void UpdateUI()
        {
            if (client == null)
            {
                return;
            }
            List<IOTTreeTagVal> tagvals = client.GetTagValues();
            if (tagvals != null)
            {
                string ss = "";
                foreach (var tv in tagvals)
                    ss += tv.Path + "=" + tv.StrVal + "\r\n";
                tbTagVals.Text = ss;
            }
        }

        private void UpdateWaterLvl(IOTTreeTagVal tagval)
        {
            if (!tagval.Valid)
                return;
            
            labelLvl.Text = tagval.StrVal;
            float f = tagval.getValFloat(0);
            int h = (int)(f / 5 * 200);
            panLvl.Height = h;
            panLvl.Top = panelLvlC.Top + 200 - h;
        }

        private void ShowInf(bool berr, string inf)
        {
            if (berr)
            {
                labelErr.Text = inf;
                return;
            }

            labelInf.Text = inf;
        }

        private void btnStartClient_Click(object sender, EventArgs e)
        {
            if (client != null)
            {
                MessageBox.Show("client is started");
                return;
            }

            string url = tbUrl.Text;
            string clientid = tbClientId.Text;
            string tagpath_str = tbTagPaths.Text;

            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(clientid))
            {
                MessageBox.Show("no iottree server url or client id input");
                return;
            }
            string[] ss = tagpath_str.Split(' ', ',', '\r', '\n');
            List<string> tagpaths = new List<string>();
            foreach (string s in ss)
            {
                string s1 = s.Trim();
                if (string.IsNullOrEmpty(s1))
                    continue;
                tagpaths.Add(s1);
            }
            if (tagpaths.Count <= 0)
            {
                MessageBox.Show("no tag path set to synchronize to server");
                return;
            }
            client = new IOTTreeClient(url, clientid);

            client.StateChanged += (sender, e) =>
            {
                ShowInf(false, $"State changed: {e.OldState} -> {e.NewState} ({e.Message})");
            };

            client.TagValueChanged += (sender, e) =>
            {
                // Console.WriteLine($"Tag updated: {e.TagPath} = {e.Value} at {e.UpdateTime}");
                UpdateUI();
                IOTTreeTagVal tagval = e.TagVal;
                if (tagval.Path == "watertank.ch1.aio.wl_val")
                {
                    UpdateWaterLvl(tagval);
                }
                if(tagval.Path== "watertank.ch1.dio.p_running")
                {
                    if(tagval.Valid)
                    {
                        bool b = (bool)tagval.ObjVal;
                        labelRunST.Text = b ? "Pump is running" : "Pump is stopped";
                        panelRunST.BackColor = b?Color.Green:Color.Red;
                    }
                    else
                    {
                        labelRunST.Text = "Pump state is unknown";
                        panelRunST.BackColor = Color.Gray;
                    }
                }
            };

            client.ConnectionLost += (sender, e) =>
            {
                ShowInf(false, "Connection lost!");
            };

            client.ConnectionRestored += (sender, e) =>
            {
                ShowInf(false, "Connection restored!");
            };

            client.ErrorOccurred += (sender, e) =>
            {
                ShowInf(true, $"Error occurred: {e.Message}");
            };

            ShowInf(true, "---");
            ShowInf(false, "---");

            client.SetTagPaths(tagpaths);

            client.Start();
        }

        private void btnStopClient_Click(object sender, EventArgs e)
        {
            if (client == null)
                return;

            client.Dispose();
            client = null;
        }


        private void btnStartPump_Click(object sender, EventArgs e)
        {
            if (client == null) return;
            try
            {
                client.WriteTagValueAsync("watertank.ch1.dio.pstart", "true");
            }
            catch (Exception ex)
            {
                ShowInf(true, $"Write failed: {ex.Message}");
            }
        }

        private void btnStopPump_Click(object sender, EventArgs e)
        {
            if (client == null) return;
            try
            {
                client.WriteTagValueAsync("watertank.ch1.dio.pstop", "true");
            }
            catch (Exception ex)
            {
                ShowInf(true, $"Write failed: {ex.Message}");
            }
        }


        private void FormDemo_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (client != null)
                client.Dispose();
        }

    }
}
