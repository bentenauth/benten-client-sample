# Benten Client Sample

A Windows Forms application demonstrating how to integrate the [Benten SDK](https://github.com/bentenauth/benten-sdk) into a .NET application.

This sample shows all five Benten request types:
- Login approval
- Credit card transaction approval
- IoT device setting change approval
- Generic notification approval
- Auth token request (push-to-approve token issuance)

---

## Prerequisites

- .NET 8 SDK
- Windows (WinForms)
- A Benten developer account
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
| Token URL | `https://api.bententechnologies.com/v2/auth/token` |
| Developer username | Your Benten developer account username |
| Developer password | Your Benten developer account password |
| Country | Country for the target user's phone number |
| Phone number | The Benten user's registered phone number |
| Request type | Select the type of approval to request |

---

## How the sample works

1. Click **Run**.
2. The app calls `POST /v2/auth/token` with your developer credentials to obtain a Bearer JWT.
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
