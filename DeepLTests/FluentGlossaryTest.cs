// Copyright 2026 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeepL;
using DeepL.Model;
using NSubstitute;
using Xunit;

namespace DeepLTests {
  /// <summary>
  ///   Unit tests for the fluent glossary-management layer in <c>FluentGlossary.cs</c>.
  ///   Tests mock <see cref="IGlossaryManager" /> and verify argument forwarding.
  /// </summary>
  public sealed class FluentGlossaryTest {
    private const string GlossaryId = "glossary-abc";

    private static MultilingualGlossaryInfo MakeGlossaryInfo(string id = GlossaryId, string name = "test") =>
          new MultilingualGlossaryInfo(id, name, Array.Empty<MultilingualGlossaryDictionaryInfo>(), DateTime.UtcNow);

    private static MultilingualGlossaryDictionaryInfo MakeDictInfo(
          string src = "en", string tgt = "de", int entries = 1) =>
          new MultilingualGlossaryDictionaryInfo(src, tgt, entries);

    private static MultilingualGlossaryDictionaryEntries MakeDictEntries(string src = "en", string tgt = "de") =>
          new MultilingualGlossaryDictionaryEntries(
                src, tgt, new GlossaryEntries(new[] { ("hello", "hallo") }));

    private static GlossaryEntries MakeEntries() =>
          new GlossaryEntries(new[] { ("foo", "bar") });

    // ---------- List ----------

    [Fact]
    public async Task ListGlossariesAsync_CallsUnderlying() {
      var manager = Substitute.For<IGlossaryManager>();
      var expected = new[] { MakeGlossaryInfo() };
      manager.ListMultilingualGlossariesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

      var result = await manager.ListGlossariesAsync();

      Assert.Same(expected, result);
      await manager.Received(1).ListMultilingualGlossariesAsync(Arg.Any<CancellationToken>());
    }

    // ---------- Glossary reference: Get / Rename / Delete ----------

    [Fact]
    public async Task GlossaryRef_GetAsync_CallsWithCorrectId() {
      var manager = Substitute.For<IGlossaryManager>();
      var expected = MakeGlossaryInfo();
      manager.GetMultilingualGlossaryAsync(GlossaryId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

      var result = await manager.Glossary(GlossaryId).GetAsync();

      Assert.Same(expected, result);
      await manager.Received(1).GetMultilingualGlossaryAsync(GlossaryId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GlossaryRef_RenameAsync_CallsUpdateName() {
      var manager = Substitute.For<IGlossaryManager>();
      var expected = MakeGlossaryInfo(name: "new name");
      manager.UpdateMultilingualGlossaryNameAsync(GlossaryId, "new name", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

      var result = await manager.Glossary(GlossaryId).RenameAsync("new name");

      Assert.Same(expected, result);
    }

    [Fact]
    public async Task GlossaryRef_DeleteAsync_CallsDelete() {
      var manager = Substitute.For<IGlossaryManager>();

      await manager.Glossary(GlossaryId).DeleteAsync();

      await manager.Received(1).DeleteMultilingualGlossaryAsync(GlossaryId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GlossaryRef_FromInfo_UsesItsId() {
      var manager = Substitute.For<IGlossaryManager>();
      var info = MakeGlossaryInfo("real-id");
      manager.GetMultilingualGlossaryAsync("real-id", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(info));

      await manager.Glossary(info).GetAsync();

      await manager.Received(1).GetMultilingualGlossaryAsync("real-id", Arg.Any<CancellationToken>());
    }

    // ---------- Dictionary reference ----------

    [Fact]
    public async Task DictionaryRef_GetEntriesAsync_Forwards() {
      var manager = Substitute.For<IGlossaryManager>();
      var expected = MakeDictEntries();
      manager.GetMultilingualGlossaryDictionaryEntriesAsync(GlossaryId, "en", "de", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

      var result = await manager.Glossary(GlossaryId).Dictionary("en", "de").GetEntriesAsync();

      Assert.Same(expected, result);
    }

    [Fact]
    public async Task DictionaryRef_ReplaceAsync_ForwardsEntries() {
      var manager = Substitute.For<IGlossaryManager>();
      var entries = MakeEntries();
      var expected = MakeDictInfo();
      manager.ReplaceMultilingualGlossaryDictionaryAsync(
                  GlossaryId, "en", "de", entries, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

      var result = await manager.Glossary(GlossaryId).Dictionary("en", "de").ReplaceAsync(entries);

      Assert.Same(expected, result);
    }

    [Fact]
    public async Task DictionaryRef_MergeAsync_ForwardsToUpdate() {
      var manager = Substitute.For<IGlossaryManager>();
      var entries = MakeEntries();
      var expected = MakeGlossaryInfo();
      manager.UpdateMultilingualGlossaryDictionaryAsync(
                  GlossaryId, "en", "de", entries, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

      var result = await manager.Glossary(GlossaryId).Dictionary("en", "de").MergeAsync(entries);

      Assert.Same(expected, result);
    }

    [Fact]
    public async Task DictionaryRef_ReplaceFromCsvAsync_ForwardsStream() {
      var manager = Substitute.For<IGlossaryManager>();
      using var stream = new MemoryStream(Encoding.UTF8.GetBytes("a,b"));
      var expected = MakeDictInfo();
      manager.ReplaceMultilingualGlossaryDictionaryFromCsvAsync(
                  GlossaryId, "en", "de", stream, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

      var result = await manager.Glossary(GlossaryId).Dictionary("en", "de").ReplaceFromCsvAsync(stream);

      Assert.Same(expected, result);
    }

    [Fact]
    public async Task DictionaryRef_DeleteAsync_Forwards() {
      var manager = Substitute.For<IGlossaryManager>();

      await manager.Glossary(GlossaryId).Dictionary("en", "de").DeleteAsync();

      await manager.Received(1).DeleteMultilingualGlossaryDictionaryAsync(
            GlossaryId, "en", "de", Arg.Any<CancellationToken>());
    }

    // ---------- Creation builder ----------

    [Fact]
    public async Task CreateGlossary_WithDictionaries_CallsCreateWithArray() {
      var manager = Substitute.For<IGlossaryManager>();
      var expected = MakeGlossaryInfo();
      MultilingualGlossaryDictionaryEntries[]? captured = null;
      manager.CreateMultilingualGlossaryAsync(
                  "My glossary",
                  Arg.Do<MultilingualGlossaryDictionaryEntries[]>(a => captured = a),
                  Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

      var dictA = MakeDictEntries("en", "de");
      var dictB = MakeDictEntries("de", "en");

      var result = await manager.CreateGlossary("My glossary")
            .WithDictionary(dictA)
            .WithDictionary(dictB)
            .CreateAsync();

      Assert.Same(expected, result);
      Assert.NotNull(captured);
      Assert.Equal(2, captured!.Length);
      Assert.Same(dictA, captured[0]);
      Assert.Same(dictB, captured[1]);
    }

    [Fact]
    public async Task CreateGlossary_FromCsv_CallsCsvOverload() {
      var manager = Substitute.For<IGlossaryManager>();
      var expected = MakeGlossaryInfo();
      using var stream = new MemoryStream(Encoding.UTF8.GetBytes("a,b"));
      manager.CreateMultilingualGlossaryFromCsvAsync(
                  "csv glossary", "en", "de", stream, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

      var result = await manager.CreateGlossary("csv glossary").FromCsv("en", "de", stream).CreateAsync();

      Assert.Same(expected, result);
    }

    [Fact]
    public async Task CreateGlossary_ImplicitAwait_Works() {
      var manager = Substitute.For<IGlossaryManager>();
      var expected = MakeGlossaryInfo();
      manager.CreateMultilingualGlossaryAsync(
                  Arg.Any<string>(),
                  Arg.Any<MultilingualGlossaryDictionaryEntries[]>(),
                  Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

      var result = await manager.CreateGlossary("My glossary")
            .WithDictionary("en", "de", MakeEntries());

      Assert.Same(expected, result);
    }

    // ---------- Validation ----------

    [Fact]
    public async Task CreateGlossary_WithoutDictionaryOrCsv_Throws() {
      var manager = Substitute.For<IGlossaryManager>();

      await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await manager.CreateGlossary("empty").CreateAsync());
    }

    [Fact]
    public void CreateGlossary_MixingCsvAndDictionary_Throws() {
      var manager = Substitute.For<IGlossaryManager>();
      using var stream = new MemoryStream();

      var builder = manager.CreateGlossary("x").FromCsv("en", "de", stream);
      Assert.Throws<InvalidOperationException>(() => { _ = builder.WithDictionary("en", "de", MakeEntries()); });

      var builder2 = manager.CreateGlossary("x").WithDictionary("en", "de", MakeEntries());
      Assert.Throws<InvalidOperationException>(() => { _ = builder2.FromCsv("en", "de", stream); });
    }

    [Fact]
    public void Glossary_EmptyId_Throws() {
      var manager = Substitute.For<IGlossaryManager>();
      Assert.Throws<ArgumentException>(() => { _ = manager.Glossary(""); });
      Assert.Throws<ArgumentException>(() => { _ = manager.Glossary("   "); });
    }

    [Fact]
    public void CreateGlossary_EmptyName_Throws() {
      var manager = Substitute.For<IGlossaryManager>();
      Assert.Throws<ArgumentException>(() => { _ = manager.CreateGlossary(""); });
    }

    [Fact]
    public void Dictionary_EmptyLanguage_Throws() {
      var manager = Substitute.For<IGlossaryManager>();
      var glossary = manager.Glossary(GlossaryId);
      Assert.Throws<ArgumentException>(() => { _ = glossary.Dictionary("", "de"); });
      Assert.Throws<ArgumentException>(() => { _ = glossary.Dictionary("en", ""); });
    }

    [Fact]
    public void WithDictionary_EmptySourceLanguage_Throws() {
      var manager = Substitute.For<IGlossaryManager>();
      Assert.Throws<ArgumentException>(() => { _ = manager.CreateGlossary("g").WithDictionary("", "de", MakeEntries()); });
      Assert.Throws<ArgumentException>(() => { _ = manager.CreateGlossary("g").WithDictionary("  ", "de", MakeEntries()); });
    }

    [Fact]
    public void WithDictionary_EmptyTargetLanguage_Throws() {
      var manager = Substitute.For<IGlossaryManager>();
      Assert.Throws<ArgumentException>(() => { _ = manager.CreateGlossary("g").WithDictionary("en", "", MakeEntries()); });
      Assert.Throws<ArgumentException>(() => { _ = manager.CreateGlossary("g").WithDictionary("en", "  ", MakeEntries()); });
    }

    [Fact]
    public void FromCsv_EmptySourceLanguage_Throws() {
      var manager = Substitute.For<IGlossaryManager>();
      using var stream = new MemoryStream();
      Assert.Throws<ArgumentException>(() => { _ = manager.CreateGlossary("g").FromCsv("", "de", stream); });
      Assert.Throws<ArgumentException>(() => { _ = manager.CreateGlossary("g").FromCsv("  ", "de", stream); });
    }

    [Fact]
    public void FromCsv_EmptyTargetLanguage_Throws() {
      var manager = Substitute.For<IGlossaryManager>();
      using var stream = new MemoryStream();
      Assert.Throws<ArgumentException>(() => { _ = manager.CreateGlossary("g").FromCsv("en", "", stream); });
      Assert.Throws<ArgumentException>(() => { _ = manager.CreateGlossary("g").FromCsv("en", "  ", stream); });
    }
  }
}
