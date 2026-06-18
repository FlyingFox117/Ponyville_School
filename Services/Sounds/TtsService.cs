using System;
using System.Speech.Synthesis;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonyvilleSchool.Services.Sounds;

public static class TtsService
{
    private static readonly SpeechSynthesizer Synth = new();

    static TtsService()
    {
        Synth.Rate = 0;      // скорость
        Synth.Volume = 100;  // громкость
    }

    public static void Speak(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        Synth.SpeakAsyncCancelAll();
        Synth.SpeakAsync(text);
    }

    public static void Stop()
    {
        Synth.SpeakAsyncCancelAll();
    }
}
