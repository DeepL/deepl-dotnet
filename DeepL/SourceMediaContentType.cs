// Copyright 2025 DeepL SE (https://www.deepl.com)
// Use of this source code is governed by an MIT
// license that can be found in the LICENSE file.

namespace DeepL {
  /// <summary>
  ///   String constants for audio format content types supported by the DeepL Voice API.
  ///   Use these when configuring <see cref="VoiceSessionOptions.SourceMediaContentType" />.
  /// </summary>
  public static class SourceMediaContentType {
    /// <summary>Auto-detect container and codec. Supported for all formats except PCM.</summary>
    public const string Auto = "audio/auto";

    /// <summary>FLAC container with FLAC codec.</summary>
    public const string Flac = "audio/flac";

    /// <summary>MPEG container with MP3 codec.</summary>
    public const string Mpeg = "audio/mpeg";

    /// <summary>Ogg container with auto-detected codec (FLAC or OPUS).</summary>
    public const string Ogg = "audio/ogg";

    /// <summary>WebM container with OPUS codec.</summary>
    public const string WebM = "audio/webm";

    /// <summary>Matroska container with auto-detected codec.</summary>
    public const string Matroska = "audio/x-matroska";

    /// <summary>Ogg container with FLAC codec.</summary>
    public const string OggFlac = "audio/ogg;codecs=flac";

    /// <summary>Ogg container with OPUS codec.</summary>
    public const string OggOpus = "audio/ogg;codecs=opus";

    /// <summary>PCM signed 16-bit little-endian at 8000 Hz.</summary>
    public const string PcmS16le8000 = "audio/pcm;encoding=s16le;rate=8000";

    /// <summary>PCM signed 16-bit little-endian at 16000 Hz. Recommended for general use.</summary>
    public const string PcmS16le16000 = "audio/pcm;encoding=s16le;rate=16000";

    /// <summary>PCM signed 16-bit little-endian at 44100 Hz.</summary>
    public const string PcmS16le44100 = "audio/pcm;encoding=s16le;rate=44100";

    /// <summary>PCM signed 16-bit little-endian at 48000 Hz.</summary>
    public const string PcmS16le48000 = "audio/pcm;encoding=s16le;rate=48000";

    /// <summary>PCM A-Law at 8000 Hz (G.711).</summary>
    public const string PcmAlaw8000 = "audio/pcm;encoding=alaw;rate=8000";

    /// <summary>PCM µ-Law at 8000 Hz (G.711).</summary>
    public const string PcmUlaw8000 = "audio/pcm;encoding=ulaw;rate=8000";

    /// <summary>WebM container with OPUS codec (explicit).</summary>
    public const string WebMOpus = "audio/webm;codecs=opus";

    /// <summary>Matroska container with AAC codec.</summary>
    public const string MatroskaAac = "audio/x-matroska;codecs=aac";

    /// <summary>Matroska container with FLAC codec.</summary>
    public const string MatroskaFlac = "audio/x-matroska;codecs=flac";

    /// <summary>Matroska container with MP3 codec.</summary>
    public const string MatroskaMp3 = "audio/x-matroska;codecs=mp3";

    /// <summary>Matroska container with OPUS codec.</summary>
    public const string MatroskaOpus = "audio/x-matroska;codecs=opus";
  }
}
