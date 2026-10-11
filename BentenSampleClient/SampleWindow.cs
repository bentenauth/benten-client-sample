// ============================================================================
// This file is part of the Benten Authentication and Authorization Platform.
// Copyright (c) 2024 Benten Technologies.
//
// Licensed under the MIT License. See LICENSE in the repository root for full
// license text.
// ============================================================================

using Benten.SDK;
using Benten.SDK.Models;

namespace BentenSampleClient;

/// <summary>
/// A simple Windows Forms application demonstrating the Benten SDK.
///
/// Fill in the fields at the top of the form, select a test type,
/// and click Run to send a request to the Benten API.
///
/// The API key comes from the BENTEN_API_KEY environment variable if it is
/// set, so you don't have to paste it each time. In a real application, keep
/// the key in your server's configuration or a secret store.
///
/// The user identified by the phone number will receive a push notification
/// on their Benten App and can tap Allow or Deny. The result appears in the
/// output log below.
/// </summary>
public class SampleWindow : Form
{
    // ── Controls ──────────────────────────────────────────────────────────────
    private TextBox txtServerUrl      = new() { Width = 400 };
    private TextBox txtApiKey         = new() { Width = 400, UseSystemPasswordChar = true };
    private TextBox txtCountry        = new() { Width = 200 };
    private TextBox txtPhoneNumber    = new() { Width = 150 };
    private ComboBox cmbTestType      = new() { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
    private Button btnRun             = new() { Text = "Run", Width = 80, Height = 30 };
    private Button btnClear           = new() { Text = "Clear", Width = 80, Height = 30 };
    private TextBox txtOutput         = new() { Multiline = true, ScrollBars = ScrollBars.Vertical,
                                                 Width = 700, Height = 300, ReadOnly = true, Font = new Font("Consolas", 9) };

    // Reused between runs, so the access token obtained with the API key is reused too.
    private BentenClient? _client;
    private string _clientKey = string.Empty;

    public SampleWindow()
    {
        Text = "Benten SDK Sample Client";
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(12);

        // ── Defaults ──────────────────────────────────────────────────────
        txtServerUrl.Text  = "wss://api.bententechnologies.com/v2/";
        txtApiKey.Text     = Environment.GetEnvironmentVariable("BENTEN_API_KEY") ?? "";
        txtCountry.Text    = "United States";
        txtPhoneNumber.Text = "6501234567";  // Replace with a registered user's number

        cmbTestType.Items.AddRange(new object[]
        {
            "Login Approval",
            "Credit Card Transaction",
            "IoT Setting Change",
            "Generic Notification"
        });
        cmbTestType.SelectedIndex = 0;

        // ── Layout ────────────────────────────────────────────────────────
        var layout = new TableLayoutPanel
        {
            ColumnCount = 2,
            AutoSize    = true,
            Padding     = new Padding(4)
        };

        void AddRow(string label, Control ctrl)
        {
            layout.Controls.Add(new Label { Text = label, TextAlign = ContentAlignment.MiddleRight, AutoSize = true });
            layout.Controls.Add(ctrl);
        }

        AddRow("WebSocket server URL:", txtServerUrl);
        AddRow("API key (bnt_sk_…):", txtApiKey);
        AddRow("Country:", txtCountry);
        AddRow("Phone number:", txtPhoneNumber);
        AddRow("Request type:", cmbTestType);

        var btnPanel = new FlowLayoutPanel { AutoSize = true };
        btnPanel.Controls.Add(btnRun);
        btnPanel.Controls.Add(btnClear);
        layout.Controls.Add(new Label());
        layout.Controls.Add(btnPanel);

        layout.Controls.Add(new Label { Text = "Output:", TextAlign = ContentAlignment.TopRight, AutoSize = true });
        layout.Controls.Add(txtOutput);

        Controls.Add(layout);

        btnRun.Click   += async (_, _) => await RunTestAsync();
        btnClear.Click += (_, _) => txtOutput.Clear();
    }

    // ── Test runner ───────────────────────────────────────────────────────────

    private async Task RunTestAsync()
    {
        btnRun.Enabled = false;
        Log("──────────────────────────────");
        Log($"Test: {cmbTestType.SelectedItem}");

        try
        {
            string apiKey    = txtApiKey.Text.Trim();
            string serverUrl = txtServerUrl.Text.Trim();
            if (apiKey.Length == 0)
            {
                Log("Enter your API key. Create one on the Benten developer dashboard.");
                return;
            }

            // One client per API key and server. The SDK exchanges the key for an
            // access token on the first request and renews it automatically.
            if (_client == null || _clientKey != apiKey + "|" + serverUrl)
            {
                _client    = new BentenClient(apiKey, serverUrl);
                _clientKey = apiKey + "|" + serverUrl;
            }
            var client = _client;

            string country = txtCountry.Text.Trim();
            string phone   = txtPhoneNumber.Text.Trim();

            switch (cmbTestType.SelectedIndex)
            {
                case 0: await RunLoginTest(client, country, phone);               break;
                case 1: await RunCCTransactionTest(client);                       break;
                case 2: await RunIoTTest(client);                                 break;
                case 3: await RunGenericNotificationTest(client, country, phone); break;
            }
        }
        catch (BentenException ex)
        {
            Log(ex.ErrorCode.Length > 0
                ? $"[BentenException] {ex.ErrorCode}: {BentenStatusCodes.Describe(ex.ErrorCode)}"
                : $"[BentenException] {ex.Message}");
        }
        catch (Exception ex)
        {
            Log($"[Error] {ex.GetType().Name}: {ex.Message}");
        }
        finally
        {
            btnRun.Enabled = true;
        }
    }

    private async Task RunLoginTest(BentenClient client, string country, string phone)
    {
        Log("Sending login approval request…");
        var result = await client.RequestLoginApprovalAsync(new LoginRequest
        {
            Country     = country,
            PhoneNumber = phone,
            Username    = "user@example.com"   // Replace with the login username being approved
        });

        LogResult(result);
    }

    private async Task RunCCTransactionTest(BentenClient client)
    {
        Log("Sending credit card transaction approval request…");
        // Replace CreditCardId with an ID from a registered Benten credit card
        var result = await client.RequestCCTransactionApprovalAsync(new CCTransactionRequest
        {
            CreditCardId = "REPLACE_WITH_BENTEN_CARD_ID",
            Amount       = "$99.99",
            Retailer     = "Sample Retailer"
        });

        LogResult(result);
    }

    private async Task RunIoTTest(BentenClient client)
    {
        Log("Sending IoT setting change approval request…");
        var result = await client.RequestIoTSettingApprovalAsync(new IoTSettingRequest
        {
            Name          = "Sample Thermostat",
            MacAddress    = "AA:AA:AA:AA:AA:01",
            SettingChange = "Change temperature from 72°F to 68°F"
        });

        LogResult(result);
    }

    private async Task RunGenericNotificationTest(BentenClient client, string country, string phone)
    {
        Log("Sending generic notification approval request…");
        var result = await client.RequestGenericNotificationApprovalAsync(new GenericNotificationRequest
        {
            NotificationSubType = "Document Approval",
            Country             = country,
            PhoneNumber         = phone,
            Line1Placeholder    = "Document",
            Line1               = "Q3 Financial Report",
            Line2Placeholder    = "Requested By",
            Line2               = "Finance Team",
            Line3Placeholder    = "Action",
            Line3               = "Approve for publication"
        });

        LogResult(result);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void LogResult(BentenResult result)
    {
        switch (result.Outcome)
        {
            case ApprovalOutcome.Approved:
                Log("[APPROVED] User tapped Allow.");
                break;
            case ApprovalOutcome.Denied:
                Log("[DENIED] User tapped Deny.");
                break;
            case ApprovalOutcome.TimedOut:
                Log("[TIMEOUT] User did not respond within 60 seconds.");
                break;
            case ApprovalOutcome.Error:
                Log($"[ERROR] {result.Error}");
                Log($"[RAW] {result.RawResponse}");
                break;
        }
    }

    private void Log(string message)
    {
        if (InvokeRequired)
        {
            Invoke(() => Log(message));
            return;
        }
        txtOutput.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }
}
