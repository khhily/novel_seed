using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace NovelSeed.Controllers;

public class WorkflowsController : Controller
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public WorkflowsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Run()
    {
        var vm = new RunWorkflowVm
        {
            Parameters = new List<WorkflowParameterVm> { new() }
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Run([FromForm] RunWorkflowVm vm)
    {
        var apiKey = _configuration["DifyApiKey"] ?? "";
        var apiHost = _configuration["DifyApiHost"] ?? "";
        var userId = _configuration["DifyUserId"] ?? "";

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiHost) || string.IsNullOrWhiteSpace(userId))
        {
            vm.Error = "请在配置中设置 DifyApiKey / DifyApiHost / DifyUserId。";
            vm.Parameters ??= new List<WorkflowParameterVm> { new() };
            return View(vm);
        }

        apiHost = apiHost.Trim().TrimEnd('/');
        if (!apiHost.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !apiHost.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            apiHost = "https://" + apiHost;
        }

        var inputs = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var p in vm.Parameters ?? Enumerable.Empty<WorkflowParameterVm>())
        {
            var key = (p.Key ?? "").Trim();
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            if (key.Length > 200)
            {
                vm.Error = $"参数名过长：{key[..200]}...";
                vm.Parameters ??= new List<WorkflowParameterVm> { new() };
                return View(vm);
            }

            inputs[key] = p.Value ?? "";
        }

        var url = $"{apiHost}/v1/workflows/run";

        try
        {
            var client = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, url);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var payload = new
            {
                inputs,
                response_mode = "blocking",
                user = userId
            };

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await client.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            vm.ResponseStatus = $"{(int)response.StatusCode} {response.StatusCode}";
            vm.ResponseBody = responseBody;
        }
        catch (Exception ex)
        {
            vm.Error = ex.Message;
        }

        vm.Parameters ??= new List<WorkflowParameterVm> { new() };
        return View(vm);
    }

    public sealed class RunWorkflowVm
    {
        public List<WorkflowParameterVm>? Parameters { get; set; }

        public string? Error { get; set; }
        public string? ResponseStatus { get; set; }
        public string? ResponseBody { get; set; }
    }

    public sealed class WorkflowParameterVm
    {
        public string? Key { get; set; }
        public string? Value { get; set; }
    }
}
