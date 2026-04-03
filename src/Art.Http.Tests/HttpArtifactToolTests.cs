using System.Text.Json;
using System.Threading.Tasks;
using RichardSzalay.MockHttp;

namespace Art.Http.Tests;

public class HttpArtifactToolTests : HttpTestsBase
{
    [Fact]
    public async Task GetDeserializedJsonAsync_NullJson_Null()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        MockHandler.When("http://localhost/imaginaryfriend.json").Respond("application/json", "null");
        var x = await Tool.GetDeserializedJsonAsync<JsonElement?>("http://localhost/imaginaryfriend.json", cancellationToken: testCancellationToken);
        Assert.Null(x);
    }

    [Fact]
    public async Task GetDeserializedJsonAsync_NotNullJson_NotNull()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        MockHandler.When("http://localhost/watashironri.json").Respond("application/json", @"{""kaf"":""epic""}");
        var x = await Tool.GetDeserializedJsonAsync<JsonElement?>("http://localhost/watashironri.json", cancellationToken: testCancellationToken);
        Assert.NotNull(x);
    }

    [Fact]
    public async Task GetDeserializedRequiredJsonAsync_NullJsonToJsonElement_NullValue()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        MockHandler.When("http://localhost/imaginaryfriend.json").Respond("application/json", "null");
        JsonElement res = await Tool.GetDeserializedRequiredJsonAsync<JsonElement>("http://localhost/imaginaryfriend.json", cancellationToken: testCancellationToken);
        Assert.Equal(JsonValueKind.Null, res.ValueKind);
    }

    [Fact]
    public async Task GetDeserializedRequiredJsonAsync_NullJsonToNullableJsonElement_Throws()
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        MockHandler.When("http://localhost/imaginaryfriend.json").Respond("application/json", "null");
        await Assert.ThrowsAsync<NullJsonDataException>(() => Tool.GetDeserializedRequiredJsonAsync<JsonElement?>("http://localhost/imaginaryfriend.json", cancellationToken: testCancellationToken));
    }
}
