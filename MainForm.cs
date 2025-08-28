using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetScan
{
    public class MainForm : Form
    {
        private readonly TextBox txtSubnet = new() { Text = "192.168.1" };
        private readonly NumericUpDown nudStart = new() { Minimum = 1, Maximum = 254, Value = 1 };
        private readonly NumericUpDown nudEnd = new() { Minimum = 1, Maximum = 254, Value = 254 };
        private readonly TextBox txtPorts = new() { Text = "22,80,443" };
        private readonly Button btnScan = new() { Text = "Scan" };
        private readonly ListView lvResults = new() { View = View.Details, FullRowSelect = true };

        public MainForm()
        {
            Text = "NetScan";
            Width = 700;
            Height = 400;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            var lblSubnet = new Label { Text = "Subnet" };
            var lblRange = new Label { Text = "Range" };
            var lblPorts = new Label { Text = "Ports" };

            lvResults.Columns.Add("IP Address", 150);
            lvResults.Columns.Add("Hostname", 200);
            lvResults.Columns.Add("Open Ports", 200);

            btnScan.Click += async (s, e) => await ScanAsync();

            var topPanel = new TableLayoutPanel { Dock = DockStyle.Top, Height = 60, ColumnCount = 6 };
            topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            topPanel.Controls.Add(lblSubnet, 0, 0);
            topPanel.Controls.Add(txtSubnet, 1, 0);
            topPanel.Controls.Add(lblRange, 2, 0);
            topPanel.Controls.Add(nudStart, 3, 0);
            topPanel.Controls.Add(nudEnd, 5, 0);
            topPanel.Controls.Add(lblPorts, 0, 1);
            topPanel.Controls.Add(txtPorts, 1, 1);
            topPanel.Controls.Add(btnScan, 5, 1);

            lvResults.Dock = DockStyle.Fill;

            Controls.Add(lvResults);
            Controls.Add(topPanel);
        }

        private async Task ScanAsync()
        {
            btnScan.Enabled = false;
            lvResults.Items.Clear();
            string subnet = txtSubnet.Text.Trim();
            int start = (int)nudStart.Value;
            int end = (int)nudEnd.Value;
            var ports = txtPorts.Text.Split(',')
                .Select(p => int.TryParse(p.Trim(), out var port) ? port : -1)
                .Where(p => p > 0)
                .ToArray();

            for (int i = start; i <= end; i++)
            {
                string ip = $"{subnet}.{i}";
                if (await PingHost(ip))
                {
                    string hostname = await ResolveHostName(ip);
                    var openPorts = new List<int>();
                    foreach (var port in ports)
                    {
                        if (await IsPortOpen(ip, port, 200))
                        {
                            openPorts.Add(port);
                        }
                    }
                    var item = new ListViewItem(new[]
                    {
                        ip,
                        hostname,
                        string.Join(",", openPorts)
                    });
                    lvResults.Items.Add(item);
                }
            }

            btnScan.Enabled = true;
        }

        private static async Task<bool> PingHost(string ip)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(ip, 200);
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<bool> IsPortOpen(string ip, int port, int timeout)
        {
            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(ip, port);
                var result = await Task.WhenAny(connectTask, Task.Delay(timeout));
                return result == connectTask && client.Connected;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<string> ResolveHostName(string ip)
        {
            try
            {
                var entry = await Dns.GetHostEntryAsync(ip);
                return entry.HostName;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
