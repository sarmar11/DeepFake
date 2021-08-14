using System;
using System.Windows.Forms;
using PluginsAPI;

using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace DeepFake
{
    public partial class frmDeepFake : Form
    {
        public frmDeepFake()
        {
            InitializeComponent();
        }
        private void btnScan_Click(object sender, EventArgs e)
        {
            labelSelectNone.Visible = false;
            labelInvalidURL.Visible = false;
            groupBox1.Visible = false;
            var uri = "";
            try
            {
                var url = new Uri(txtURL.Text);
                uri = url.AbsoluteUri;
            }
            catch
            {
                labelInvalidURL.Visible = true;
            }
            if (checkBox1.Checked)
            {
                if (!labelInvalidURL.Visible) {
                    btnScan.Text = "Scanning...";
                    _ = SendUriToScanAsync((object)uri);
                }
            }
            else {
                labelSelectNone.Visible = true;
            }  
        }


        /// <summary>
        /// Send value to FakeAPI and analize the results.
        /// Generic code to extract data from API
        /// var client = new HttpClient();
        /// string baseURL = "https://fake1.api";
        /// string videoURL = o.ToString();
        /// string urlReport = "/fake/request?video-url=";
        /// string accessToken = "fakeAccessToken1";
        /// client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
        /// var response = await client.GetStringAsync(baseURL + urlReport + videoURL);
        /// JObject json = JObject.Parse(response);
        /// var dfDetected = json["results"]["detected"];
        /// var dfScore = json["results"]["score"];
        /// </summary>
        /// <param name="value"></param>
        private async Task SendUriToScanAsync(object o)
        {
            int dfScore = 0;
            int nScore = 0;
            label4.Text = "Not Analyzed";

            if (checkBox1.Checked)
            {
                // DeepwareAPI Analysis
                var client = new HttpClient();
                string baseURL = "https://scanner.deepware.ai/public/";
                string videoURL = o.ToString();
                // URL SCAN
                string urlReport = "/url/scan?video-url=";
                var scanResponse = "";
                try
                {
                    scanResponse = await client.GetStringAsync(baseURL + urlReport + videoURL);
                }
                catch (Exception) {
                    labelInvalidURL.Visible = true;
                    labelInvalidURL.Text = "Videos longer than 10 minutes cannot be scanned";
                    btnScan.Text = "Scan";
                    return;
                }
                JObject json1 = JObject.Parse(scanResponse);
                var videoId = json1["video-id"];
                // URL REPORT
                string urlReport2 = "/video/report?video-id=";
                var reportResponse = "{}";
                bool queue = true;
                JObject json2 = JObject.Parse(reportResponse);

                int loop = 1;
                while (queue && loop <= 5) {
                    Thread.Sleep(5);
                    try
                    {
                        reportResponse = await client.GetStringAsync(baseURL + urlReport2 + videoId);
                        json2 = JObject.Parse(reportResponse);
                        queue = (bool)json2["queue"];
                    }
                    catch
                    {
                        queue = true;
                    }
                    loop++;
                }
                if (loop > 5) {
                    labelInvalidURL.Visible = true;
                    labelInvalidURL.Text = "Too much time processing video. Try it again later.";
                    btnScan.Text = "Scan";
                    return;
                }
                
                var dfDetected = false;
                var dfScore1 = 0;
                int ndfScore1 = 0;

                foreach (var json3 in json2["results"]) {
                    dfDetected = json3.First.Value<bool>("detected") | dfDetected;
                    dfScore1 += json3.First.Value<int>("score");
                    ndfScore1++;
                }
                dfScore1 /= ndfScore1;

                dfScore += dfScore1;
                nScore += 1;
                if (dfDetected)
                {
                    label4.Text = "The video analyzed is classified as DEEPFAKE ";
                }
                else
                {
                    label4.Text = "The video analyzed is NOT classified as DEEPFAKE";
                }
            }

            dfScore /= nScore;
            btnScan.Text = "Scan";
            groupBox1.Visible = true;
            labelScore.Text = dfScore.ToString() + "/100";
            if (dfScore < 33)
            {
                pictureBoxGreen.Visible = true;
                pictureBoxOrange.Visible = false;
                pictureBoxRed.Visible = false;
            }
            if (33 <= dfScore && 66 > dfScore)
            {
                pictureBoxGreen.Visible = false;
                pictureBoxOrange.Visible = true;
                pictureBoxRed.Visible = false;
            }
            if (dfScore >= 66)
            {
                pictureBoxGreen.Visible = false;
                pictureBoxOrange.Visible = false;
                pictureBoxRed.Visible = true;
            }
        }
    }
}
