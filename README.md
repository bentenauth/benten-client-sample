# Benten Client Sample

A Windows Forms application demonstrating how to integrate the [Benten SDK](https://github.com/bentenauth/benten-sdk) into a .NET application.

This sample shows all four Benten approval request types:
- Login approval
- Credit card transaction approval
- IoT device setting change approval
- Generic notification approval

---

## Prerequisites

- .NET 8 SDK
- Windows (WinForms)
- A Benten developer account and an API key (create one on the [developer dashboard](https://bententechnologies.com/developers.html))
- At least one user with a registered device in the Benten system

---

## Running the sample

```bash
git clone https://github.com/bentenauth/benten-client-sample.git
cd benten-client-sample/BentenSampleClient
dotnet run
```

Or open `BentenSampleClient.csproj` in Visual Studio and press F5.

---

## Configuration

Fill in the fields in the UI:

| Field | Description |
|-------|-------------|
| WebSocket server URL | `wss://api.bententechnologies.com/v2/` |
| API key | Your Benten API key (`bnt_sk_…`). Filled in from the `BENTEN_API_KEY` environment variable if it is set. |
| Country | Country for the target user's phone number |
| Phone number | The Benten user's registered phone number |
| Request type | Select the type of approval to request |

---

## How the sample works

1. Click **Run**.
2. The SDK exchanges your API key for a short-lived access token (`POST /v2/auth/token`), and reuses it for later runs until it is about to expire.
3. It opens a WebSocket to the Benten API and sends the selected request as JSON.
4. The Benten API sends a push notification to the user's phone (identified by country + phone number).
5. The user taps **Allow** or **Deny** in the Benten App.
6. The result is returned on the WebSocket and displayed in the output log.

---

## Key files

| File | Purpose |
|------|---------|
| `SampleWindow.cs` | Main form — UI and test logic |
| `Program.cs` | Entry point |

---

## Related

- [Benten SDK](https://github.com/bentenauth/benten-sdk) — .NET client library
- Benten App — the official iOS/Android app ([App Store](https://apps.apple.com/us/app/benten-app/id1508036928), [Google Play](https://play.google.com/store/apps/details?id=com.bententechnologies.maui.bentenapp)). Source available on request under a proprietary license: [support@bententechnologies.com](mailto:support@bententechnologies.com)

---

## License

[MIT](LICENSE)
