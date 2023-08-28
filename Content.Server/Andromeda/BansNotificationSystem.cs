using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Content.Shared.CCVar;
using Content.Shared.GameTicking;
using Robust.Shared.Configuration;

namespace Content.Server.Andromeda;

/// <summary>
/// Listen game events and send notifications to Discord
/// </summary>

public sealed class BansNotificationsSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _config = default!;

    private ISawmill _sawmill = default!;
    private readonly HttpClient _httpClient = new();

    private string _webhookUrl = string.Empty;

    public override void Initialize()
    {
        base.Initialize();
        _config.OnValueChanged(CCVars.DiscordBanWebhook, OnWebhookChanged, true);
        SubscribeLocalEvent<BanEvent>(OnBan);
    }

    private async void SendDiscordMessage(WebhookPayload payload)
    {
        var request = await _httpClient.PostAsync(_webhookUrl,
            new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

        var content = await request.Content.ReadAsStringAsync();
        if (!request.IsSuccessStatusCode)
        {
            _sawmill.Log(LogLevel.Error, $"Discord returned bad status code when posting message: {request.StatusCode}\nResponse: {content}");
        }
    }

    private void OnWebhookChanged(string url)
    {
        _webhookUrl = url;
    }

    public void OnBan(BanEvent e)
    {
        if (string.IsNullOrEmpty(_webhookUrl))
            return;

        var payload = new WebhookPayload();
        var text = Loc.GetString("discord-ban-msg",
			("adminnick", e.AdminNick),
            ("username", e.Username),
            ("expires", e.Expires == null ? "навсегда" : $"до {e.Expires}"),
            ("reason", e.Reason));

        payload.Content = text;

        SendDiscordMessage(payload);
    }

    public void NotifyBan(string adminNick, string username, string reason, DateTimeOffset? expires = null)
    {
        RaiseLocalEvent(new BanEvent(adminNick, username, expires, reason));
    }

    private struct WebhookPayload
    {
        [JsonPropertyName("content")]
        public string Content { get; set; } = "";

        public Dictionary<string, string[]> AllowedMentions { get; set; } =
            new()
            {
                { "parse", Array.Empty<string>() }
            };

        public WebhookPayload()
        {
        }
    }
}
