// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using DeepL;
using DeepL.Model;
using Xunit;

namespace DeepLTests {
  /// <summary>Unit tests for Voice API types that do not require API access.</summary>
  public sealed class VoiceSessionUnitTest {
    [Fact]
    public void TestVoiceSessionOptionsDefaults() {
      var options = new VoiceSessionOptions();
      Assert.Equal(SourceMediaContentType.Auto, options.SourceMediaContentType);
      Assert.Null(options.MessageFormat);
      Assert.Null(options.SourceLanguage);
      Assert.Null(options.SourceLanguageMode);
      Assert.NotNull(options.TargetLanguages);
      Assert.Empty(options.TargetLanguages);
      Assert.Null(options.TargetMediaLanguages);
      Assert.Null(options.TargetMediaContentType);
      Assert.Null(options.TargetMediaVoice);
      Assert.Null(options.GlossaryId);
      Assert.Null(options.Formality);
    }

    [Fact]
    public void TestVoiceSessionOptionsConfiguration() {
      var options = new VoiceSessionOptions {
        SourceMediaContentType = SourceMediaContentType.OggOpus,
        MessageFormat = VoiceMessageFormat.Json,
        SourceLanguage = "en",
        SourceLanguageMode = DeepL.SourceLanguageMode.Fixed,
        TargetLanguages = new[] { "de", "fr", "es" },
        TargetMediaVoice = TargetMediaVoice.Female,
        GlossaryId = "test-glossary-id",
        Formality = "formal"
      };

      Assert.Equal(SourceMediaContentType.OggOpus, options.SourceMediaContentType);
      Assert.Equal(VoiceMessageFormat.Json, options.MessageFormat);
      Assert.Equal("en", options.SourceLanguage);
      Assert.Equal(DeepL.SourceLanguageMode.Fixed, options.SourceLanguageMode);
      Assert.Equal(3, options.TargetLanguages.Length);
      Assert.Equal(TargetMediaVoice.Female, options.TargetMediaVoice);
      Assert.Equal("test-glossary-id", options.GlossaryId);
      Assert.Equal("formal", options.Formality);
    }

    [Fact]
    public void TestVoiceMessageFormatApiValues() {
      Assert.Equal("json", VoiceMessageFormat.Json.ToApiValue());
      Assert.Equal("msgpack", VoiceMessageFormat.MessagePack.ToApiValue());
    }

    [Fact]
    public void TestSourceLanguageModeApiValues() {
      Assert.Equal("auto", DeepL.SourceLanguageMode.Auto.ToApiValue());
      Assert.Equal("fixed", DeepL.SourceLanguageMode.Fixed.ToApiValue());
    }

    [Fact]
    public void TestTargetMediaVoiceApiValues() {
      Assert.Equal("male", TargetMediaVoice.Male.ToApiValue());
      Assert.Equal("female", TargetMediaVoice.Female.ToApiValue());
    }

    [Fact]
    public void TestVoiceSessionInfoDeserialization() {
      var json = "{\"streaming_url\":\"wss://api.deepl.com/v3/voice/realtime/connect\"," +
                 "\"token\":\"test-token-123\"," +
                 "\"session_id\":\"test-session-456\"}";
      var info = JsonSerializer.Deserialize<VoiceSessionInfo>(json);
      Assert.NotNull(info);
      Assert.Equal("wss://api.deepl.com/v3/voice/realtime/connect", info!.StreamingUrl);
      Assert.Equal("test-token-123", info.Token);
      Assert.Equal("test-session-456", info.SessionId);
    }

    [Fact]
    public void TestTranscriptUpdateDeserialization() {
      var json = "{\"concluded\":[{\"text\":\"Hello \"}],\"tentative\":[{\"text\":\"world\"}],\"language\":\"de\"}";
      var update = JsonSerializer.Deserialize<TranscriptUpdate>(json);
      Assert.NotNull(update);
      Assert.Single(update!.Concluded);
      Assert.Equal("Hello ", update.Concluded[0].Text);
      Assert.Single(update.Tentative);
      Assert.Equal("world", update.Tentative[0].Text);
      Assert.Equal("de", update.Language);
    }

    [Fact]
    public void TestTranscriptSegmentDeserialization() {
      var json = "{\"text\":\"Hello world\"}";
      var segment = JsonSerializer.Deserialize<TranscriptSegment>(json);
      Assert.NotNull(segment);
      Assert.Equal("Hello world", segment!.Text);
      Assert.Equal("Hello world", segment.ToString());
    }

    [Fact]
    public void TestTargetMediaChunkDeserialization() {
      var json = "{\"content_type\":\"audio/webm;codecs=opus\"," +
                 "\"headers\":1," +
                 "\"data\":[\"base64data1\",\"base64data2\"]," +
                 "\"text\":\"Hallo Welt\"," +
                 "\"language\":\"de\"," +
                 "\"duration\":1.5}";
      var chunk = JsonSerializer.Deserialize<TargetMediaChunk>(json);
      Assert.NotNull(chunk);
      Assert.Equal("audio/webm;codecs=opus", chunk!.ContentType);
      Assert.Equal(1, chunk.Headers);
      Assert.Equal(2, chunk.Data.Length);
      Assert.Equal("base64data1", chunk.Data[0]);
      Assert.Equal("Hallo Welt", chunk.Text);
      Assert.Equal("de", chunk.Language);
      Assert.Equal(1.5, chunk.Duration);
    }

    [Fact]
    public void TestVoiceStreamErrorDeserialization() {
      var json = "{\"code\":\"4001\",\"reason\":\"invalid_audio\",\"message\":\"Audio format not supported\"}";
      var error = JsonSerializer.Deserialize<VoiceStreamError>(json);
      Assert.NotNull(error);
      Assert.Equal("4001", error!.Code);
      Assert.Equal("invalid_audio", error.Reason);
      Assert.Equal("Audio format not supported", error.Message);
    }

    [Fact]
    public void TestSourceMediaContentTypeConstants() {
      Assert.Equal("audio/auto", SourceMediaContentType.Auto);
      Assert.Equal("audio/flac", SourceMediaContentType.Flac);
      Assert.Equal("audio/mpeg", SourceMediaContentType.Mpeg);
      Assert.Equal("audio/ogg", SourceMediaContentType.Ogg);
      Assert.Equal("audio/webm", SourceMediaContentType.WebM);
      Assert.Equal("audio/x-matroska", SourceMediaContentType.Matroska);
      Assert.Equal("audio/ogg;codecs=flac", SourceMediaContentType.OggFlac);
      Assert.Equal("audio/ogg;codecs=opus", SourceMediaContentType.OggOpus);
      Assert.Equal("audio/pcm;encoding=s16le;rate=16000", SourceMediaContentType.PcmS16le16000);
      Assert.Equal("audio/webm;codecs=opus", SourceMediaContentType.WebMOpus);
    }
  }

  /// <summary>Tests for Voice API session creation that require API access.</summary>
  public sealed class VoiceSessionClientTest : BaseDeepLTest {
    [Fact]
    public async Task TestCreateSessionRequiresTargetLanguages() {
      var client = CreateTestClient();
      var options = new VoiceSessionOptions {
        SourceMediaContentType = SourceMediaContentType.OggOpus
      };
      await Assert.ThrowsAsync<ArgumentException>(
            () => client.CreateVoiceSessionAsync(options));
    }

    [Fact]
    public async Task TestCreateSessionRejectsExcessiveTargetLanguages() {
      var client = CreateTestClient();
      var options = new VoiceSessionOptions {
        SourceMediaContentType = SourceMediaContentType.OggOpus,
        TargetLanguages = new[] { "de", "fr", "es", "it", "nl", "pt" }
      };
      await Assert.ThrowsAsync<ArgumentException>(
            () => client.CreateVoiceSessionAsync(options));
    }

    [Fact]
    public async Task TestCreateSessionRejectsNullOptions() {
      var client = CreateTestClient();
      await Assert.ThrowsAsync<ArgumentNullException>(
            () => client.CreateVoiceSessionAsync(null!));
    }
  }
}
