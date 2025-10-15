using Newtonsoft.Json;
using Tennis.Models;

namespace Tennis;

internal record Payload(string source, List<League> data);

public class TriggerN8NWorkflow(HttpClient client)
{
    private const string Url = "https://n8n.nexdev.net/webhook/trigger";
    public async Task CallWorkFlowForITF(List<League> leagues)
    {

        var payload = new Payload("itf-tennis", leagues);
        await client.PostAsync(Url, new StringContent(JsonConvert.SerializeObject(payload)));
    }

    public async Task CallWorkFlowForATP(List<League> leagues)
    {
        var payload = new Payload("atp-tour", leagues);
        await client.PostAsync(Url, new StringContent(JsonConvert.SerializeObject(payload)));
    }
}
