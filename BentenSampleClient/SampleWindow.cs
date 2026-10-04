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
/// The user identified by the phone number will receive a push notification
/// on their Benten App and can tap Allow or Deny. The result appears in the
/// output log below.
/// </summary>
public class SampleWindow : Form
{
    // ── Controls ──────────────────────────────────────────────────────────────
    private TextBox txtServerUrl      = new() { Width = 400 };
    private TextBox txtTokenUrl       = new() { Width = 400 };
    private TextBox txtUsername       = new() { Width = 200 };
    private TextBox txtPassword       = new() { Width = 200, UseSystemPasswordChar = true };
    private TextBox txtCountry        = new() { Width = 200 };
    private TextBox txtPhoneNumber    = new() { Width = 150 };
    private ComboBox cmbTestType      = new() { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
    private Button btnRun             = new() { Text = "Run", Width = 80, Height = 30 };
    private Button btnClear           = new() { Text = "Clear", Width = 80, Height = 30 };
    private TextBox txtOutput         = new() { Multiline = true, ScrollBars = ScrollBars.Vertical,
                                                 Width = 700, Height = 300, ReadOnly = true, Font = new Font("Consolas", 9) };

    public SampleWindow()
    {
        Text = "Benten SDK Sample Client";
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Padding = new Padding(12);

        // ── Defaults ──────────────────────────────────────────────────────
        txtServerUrl.Text  = "wss://api.bententechnologies.com/v2/";
        txtTokenUrl.Text   = "https://api.bententechnologies.com/v2/auth/token";
        txtUsername.Text   = "your-benten-username";
        txtPassword.Text   = "";
        txtCountry.Text    = "United States";
        txtPhoneNumber.Text = "6501234567";  // Replace with a registered user's number

        cmbTestType.Items.AddRange(new object[]
        {
            "Login Approval",
            "Credit Card Transaction",
            "IoT Setting Change",
            "Generic Notification",
            "Auth Token Request"
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
        AddRow("Token URL:", txtTokenUrl);
        AddRow("Developer username:", txtUsername);
        AddRow("Developer password:", txtPassword);
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
            var client = new BentenClient(txtServerUrl.Text.Trim());

            // Step 1 — Get Bearer token
            Log("Requesting Bearer token…");
            string jwt = await client.GetBearerTokenAsync(
                txtUsername.Text.Trim(),
                txtPassword.Text.Trim());
            Log($"Token obtained (first 40 chars): {jwt[..Math.Min(40, jwt.Length)]}…");

            string country = txtCountry.Text.Trim();
            string phone   = txtPhoneNumber.Text.Trim();

            // Step 2 — Run the selected test
            switch (cmbTestType.SelectedIndex)
            {
                case 0: await RunLoginTest(client, jwt, country, phone);           break;
                case 1: await RunCCTransactionTest(client, jwt, country, phone);   break;
                case 2: await RunIoTTest(client, jwt, country, phone);             break;
                case 3: await RunGenericNotificationTest(client, jwt, country, phone); break;
                case 4: await RunAuthTokenTest(client, country, phone);            break;
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

    private async Task RunLoginTest(BentenClient client, string jwt, string country, string phone)
    {
        Log("Sending login approval request…");
        var result = await client.RequestLoginApprovalAsync(jwt, new LoginRequest
        {
            Country     = country,
            PhoneNumber = phone,
            Username    = "user@example.com"   // Replace with the login username being approved
        });

        LogResult(result);
    }

    private async Task RunCCTransactionTest(BentenClient client, string jwt, string country, string phone)
    {
        Log("Sending credit card transaction approval request…");
        // Replace CreditCardId with an ID from a registered Benten credit card
        var result = await client.RequestCCTransactionApprovalAsync(jwt, new CCTransactionRequest
        {
            CreditCardId = "REPLACE_WITH_BENTEN_CARD_ID",
            Amount       = "$99.99",
            Retailer     = "Sample Retailer"
        });

        LogResult(result);
    }

    private async Task RunIoTTest(BentenClient client, string jwt, string country, string phone)
    {
        Log("Sending IoT setting change approval request…");
        var result = await client.RequestIoTSettingApprovalAsync(jwt, new IoTSettingRequest
        {
            Name          = "Sample Thermostat",
            MacAddress    = "AA:BB:CC:DD:EE:01",
            SettingChange = "Change temperature from 72°F to 68°F"
        });

        LogResult(result);
    }

    private async Task RunGenericNotificationTest(BentenClient client, string jwt, string country, string phone)
    {
        Log("Sending generic notification approval request…");
        var result = await client.RequestGenericNotificationApprovalAsync(jwt, new GenericNotificationRequest
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

    private async Task RunAuthTokenTest(BentenClient client, string country, string phone)
    {
        Log("Sending auth token request (user must approve on their Benten App)…");
        var result = await client.RequestAuthTokenAsync(new AuthTokenRequest
        {
            Country     = country,
            PhoneNumber = phone
        });

        switch (result.Outcome)
        {
            case ApprovalOutcome.Approved when result.IsSuccess:
                Log($"[SUCCESS] Token issued (first 40 chars): {result.Token[..Math.Min(40, result.Token.Length)]}…");
                break;
            case ApprovalOutcome.Denied:
                Log("[DENIED] User tapped Deny.");
                break;
            case ApprovalOutcome.TimedOut:
                Log("[TIMEOUT] User did not respond within 60 seconds.");
                break;
            default:
                Log($"[ERROR] {result.Error}");
                Log($"[RAW] {result.RawResponse}");
                break;
        }
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
