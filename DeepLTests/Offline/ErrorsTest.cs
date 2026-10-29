// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DeepL;
using RichardSzalay.MockHttp;
using Xunit;

namespace DeepLTests.Offline {
  public sealed class ErrorsTest : OfflineTest {
    [Theory]
    [InlineData(400, typeof(DeepLException), "Bad request")]
    [InlineData(401, typeof(AuthorizationException), "Authorization failure, check AuthKey")]
    [InlineData(403, typeof(AuthorizationException), "Authorization failure, check AuthKey")]
    [InlineData(404, typeof(NotFoundException), "Not found")]
    [InlineData(413, typeof(DeepLException), "Unexpected status code: RequestEntityTooLarge")]
    [InlineData(429, typeof(TooManyRequestsException), "Too many requests, DeepL servers are currently experiencing high load")]
    [InlineData(456, typeof(QuotaExceededException), "Quota for this billing period has been exceeded")]
    [InlineData(500, typeof(DeepLException), "Unexpected status code: InternalServerError")]
    [InlineData(503, typeof(DeepLException), "Service unavailable")]
    public async Task ThrowsExceptionOfStatusWithMessageAndDetail(int status, Type expectedType, string expectedStart) {
      ExpectNoBody(HttpMethod.Get, "v2/usage")
            .RespondJson("{\"message\":\"the message\",\"detail\":\"the detail\"}", (HttpStatusCode)status);
      var exception = await Record.ExceptionAsync(() => CreateClient().GetUsageAsync());
      Assert.Equal(expectedType, exception.GetType());
      Assert.Equal(expectedStart + ", message: the message, detail: the detail", exception.Message);
    }

    [Theory]
    [InlineData("{\"character_count\":")]
    [InlineData("{\"character_count\":\"many\",\"character_limit\":2}")]
    public async Task ThrowsDeepLExceptionWhenResponseIsMalformed(string body) {
      ExpectNoBody(HttpMethod.Get, "v2/usage").RespondJson(body);
      var exception = await Assert.ThrowsAsync<DeepLException>(() => CreateClient().GetUsageAsync());
      Assert.IsAssignableFrom<System.Text.Json.JsonException>(exception.InnerException);
    }

    [Fact]
    public async Task ThrowsExceptionWithoutDetailWhenBodyIsNotJson() {
      ExpectNoBody(HttpMethod.Get, "v2/usage").Respond(HttpStatusCode.BadRequest, "text/html", "<html>bad</html>");
      var exception = await Assert.ThrowsAsync<DeepLException>(() => CreateClient().GetUsageAsync());
      Assert.Equal("Bad request", exception.Message);
    }
  }
}
