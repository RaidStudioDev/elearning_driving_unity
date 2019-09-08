using UnityEngine;
using System.Collections;

public class SoundEffectsLib
{
    public SoundEffectsLib()
    {

    }

    // COUNTDOWN 
    // http://leanaudioplay.dentedpixel.com/?d=a:fvb:8,,.001987448,,,8~8,,1,,-,.5,.2122908,-,,8~~~0~~1 
    // http://leanaudioplay.dentedpixel.com/?d=a:fvb:8,,.001987448,,,8~8,,.2251969,,-,.5,.03779528,-,,8~~~0~~1
    public AudioClip PlayCountdown()
    {
        AnimationCurve volumeCurve = new AnimationCurve(new Keyframe(0f, 0.2251969f, 0f, -1f), new Keyframe(0.5f, 0.03779528f, -1f, 0f));
        AnimationCurve frequencyCurve = new AnimationCurve(new Keyframe(0f, 0.001987448f, 0f, 0f));

        return LeanAudio.createAudio(volumeCurve, frequencyCurve, LeanAudio.options().setWaveSquare());
    }

    // COUNTDOWN GO!!
    // http://leanaudioplay.dentedpixel.com/?d=a:fvb:8,.02695933,.001464968,,,8~8,,1,,-,.5,.1566879,-,,8~~~0~~1
    // http://leanaudioplay.dentedpixel.com/?d=a:fvb:8,.02695933,.001464968,,,8~8,,.1952756,,-,.5,.01889764,-,,8~~~0~~1
    public AudioClip PlayCountdownGo()
    {
        AnimationCurve volumeCurve = new AnimationCurve(new Keyframe(0f, 0.1952756f, 0f, -1f), new Keyframe(0.5f, 0.01889764f, -1f, 0f));
        AnimationCurve frequencyCurve = new AnimationCurve(new Keyframe(0.02695933f, 0.001464968f, 0f, 0f));

        return LeanAudio.createAudio(volumeCurve, frequencyCurve, LeanAudio.options().setWaveSquare());
    }

    // TOO SLOW
    // http://leanaudioplay.dentedpixel.com/?d=a:fvb:8,.03811491,.003057325,,,.5,.006242038,,,8~8,.001859264,.9363058,,-,.5,.1804671,-,,8~~~0~~1
    public AudioClip PlayTooSlow()
    {
        AnimationCurve volumeCurve = new AnimationCurve(new Keyframe(0.001859264f, 0.9363058f, 0f, -1f), new Keyframe(0.5f, 0.1804671f, -1f, 0f));
        AnimationCurve frequencyCurve = new AnimationCurve(new Keyframe(0.03811491f, 0.003057325f, 0f, 0f), new Keyframe(0.5f, 0.006242038f, 0f, 0f));

        return LeanAudio.createAudio(volumeCurve, frequencyCurve, LeanAudio.options().setWaveSquare());
    }

    // High Pitch with Vibrato 0.5
    // http://leanaudioplay.dentedpixel.com/?d=a:fvb:8,.2984478,.0005949656,,,8~8,,1,,-,.5,.7377388,-,,8~.1,,,~~0~~2
    public AudioClip PlayCountdownHighPitchVibrato()
    {
        AnimationCurve volumeCurve = new AnimationCurve(new Keyframe(0f, 1f, 0f, -1f), new Keyframe(0.5f, 0.7377388f, -1f, 0f));
        AnimationCurve frequencyCurve = new AnimationCurve(new Keyframe(0.2984478f, 0.0005949656f, 0f, 0f));

        return LeanAudio.createAudio(volumeCurve, frequencyCurve, LeanAudio.options().setVibrato(new Vector3[] { new Vector3(0.1f, 0f, 0f) }).setWaveSawtooth());
    }

    // Extra High Pitch END with Vibrato 0.75
    // http://leanaudioplay.dentedpixel.com/?d=a:fvb:8,.2984478,.0005949656,,,8~8,,1,,-,.5,.7377388,-,,8~.1,,,~~0~~2
    // http://leanaudioplay.dentedpixel.com/?d=a:fvb:8,.2984478,.0005949656,,,8~8,.002012411,.5548589,,-,.5,.3777429,-,,8~.1,,,~~0~~2
    public AudioClip PlayCountdownHighPitchEndVibrato()
    {
        AnimationCurve volumeCurve = new AnimationCurve(new Keyframe(0.002012411f, 0.5548589f, 0f, -1f), new Keyframe(0.5f, 0.3777429f, -1f, 0f));
        AnimationCurve frequencyCurve = new AnimationCurve(new Keyframe(0.2984478f, 0.0005949656f, 0f, 0f));

        return LeanAudio.createAudio(volumeCurve, frequencyCurve, LeanAudio.options().setVibrato(new Vector3[] { new Vector3(0.1f, 0f, 0f) }).setWaveSawtooth());
    }

    // Click Low Tone 0.25
    // http://leanaudioplay.dentedpixel.com/?d=a:fvb:8,.01105769,.001647597,,,8~8,,1,,-,.2376075,.7377388,-,,.25,,,,8~~~0~~
    public AudioClip PlayLowToneButton()
    {
        AnimationCurve volumeCurve = new AnimationCurve(new Keyframe(0f, 1f, 0f, -1f), new Keyframe(0.1795745f, 0.7224256f, -1f, 0f), new Keyframe(0.25f, 0f, 0f, 0f));
        AnimationCurve frequencyCurve = new AnimationCurve(new Keyframe(0.01105769f, 0.001647597f, 0f, 0f));

        return LeanAudio.createAudio(volumeCurve, frequencyCurve, LeanAudio.options());
    }

    // Noise Mid-High Vibrato Tone 0.5
    // http://leanaudioplay.dentedpixel.com/?d=a:fvb:8,.2680972,.0008237986,,,8~8,.04856101,.0389016,,-,.3267751,.562929,,,.5,.2929062,-,,8~~~0~~3,1000,1
    public AudioClip PlayNoiseMidHighTone()
    {
        AnimationCurve volumeCurve = new AnimationCurve(new Keyframe(0.04856101f, 0.0389016f, 0f, -1f), new Keyframe(0.3267751f, 0.562929f, 0f, 0f), new Keyframe(0.5f, 0.2929062f, -1f, 0f));
        AnimationCurve frequencyCurve = new AnimationCurve(new Keyframe(0.2680972f, 0.0008237986f, 0f, 0f));

        return LeanAudio.createAudio(volumeCurve, frequencyCurve, LeanAudio.options().setWaveNoise().setWaveNoiseScale(1000));
    }

    // Sawtooth Mid-Low Tone 0.5
    // http://leanaudioplay.dentedpixel.com/?d=a:fvb:8,.1598466,.007437071,,,8~8,.04856101,.0389016,,-,.3267751,.562929,,,.5,.7185355,-,,8~.1,,,~~0~~2
    public AudioClip PlaySawLowHighTone()
    {
        AnimationCurve volumeCurve = new AnimationCurve(new Keyframe(0.06312931f, 0.0389016f, 0f, -1f), new Keyframe(0.40625f, 0.7185355f, -1f, 0f), new Keyframe(0.65f, 0.2929062f, 0f, 0f));
        AnimationCurve frequencyCurve = new AnimationCurve(new Keyframe(0.1598466f, 0.007437071f, 0f, 0f));

        return LeanAudio.createAudio(volumeCurve, frequencyCurve, LeanAudio.options().setVibrato(new Vector3[] { new Vector3(0.1f, 0f, 0f) }).setWaveSawtooth());
    }

    // Noise Low-Mid-High-Fade No Vibrato Tone 0.75
    // http://leanaudioplay.dentedpixel.com/?d=a:fvb:8,.2873193,.001556064,,,8~8,.05766619,.06864989,,-,.2691089,.3546911,,,.5,.002288329,-,,8~~~0~~3,1000,1
    public AudioClip PlaySawLowMidHighTone()
    {
        AnimationCurve volumeCurve = new AnimationCurve(new Keyframe(0.05766619f, 0.06864989f, 0f, -1f), new Keyframe(0.2691089f, 0.3546911f, 0f, 0f), new Keyframe(0.5f, 0.002288329f, -1f, 0f));
        AnimationCurve frequencyCurve = new AnimationCurve(new Keyframe(0.2873193f, 0.001556064f, 0f, 0f));

        return LeanAudio.createAudio(volumeCurve, frequencyCurve,
            LeanAudio.options().setWaveNoise().setWaveNoiseScale(1000));
    }

    private AudioClip PlayBoomSound()
    {
        AnimationCurve volumeCurve = new AnimationCurve(new Keyframe(0f, 1.163155f, 0f, -1f), new Keyframe(0.3098361f, 0f, 0f, 0f), new Keyframe(0.5f, 0.003524712f, 0f, 0f));
        AnimationCurve frequencyCurve = new AnimationCurve(new Keyframe(0.000819672f, 0.007666667f, 0f, 0f), new Keyframe(0.01065573f, 0.002424242f, 0f, 0f), new Keyframe(0.02704918f, 0.007454545f, 0f, 0f), new Keyframe(0.03770492f, 0.002575758f, 0f, 0f), new Keyframe(0.052459f, 0.007090909f, 0f, 0f), new Keyframe(0.06885245f, 0.002939394f, 0f, 0f), new Keyframe(0.0819672f, 0.006727273f, 0f, 0f), new Keyframe(0.1040983f, 0.003181818f, 0f, 0f), new Keyframe(0.1188525f, 0.006212121f, 0f, 0f), new Keyframe(0.145082f, 0.004151515f, 0f, 0f), new Keyframe(0.1893443f, 0.005636364f, 0f, 0f));

        return LeanAudio.createAudio(
            volumeCurve,
            frequencyCurve,
            LeanAudio.options()
            .setVibrato(new Vector3[] { new Vector3(0.1f, 0f, 0f) })
            .setFrequency(11025)
        );
    }

    // Ambient Low-Mid Vibrato Tone 1.95
    // http://leanaudioplay.dentedpixel.com/?d=a:fvb:8,.2873193,.001556064,,,8~8,.05766619,.06864989,,-,.2691089,.3546911,,,.5,.002288329,-,,8~~~0~~3,1000,1
    public AudioClip PlayLongAmbientSineWaveVibrato()
    {
        AnimationCurve volumeCurve = new AnimationCurve(new Keyframe(-0.001455604f, 0.004420519f, 1.83897f, 1.83897f), new Keyframe(1.007153f, 0.9045518f, 0.02452584f, 0.008126878f), new Keyframe(1.996322f, 0.003967403f, -2.285804f, -2.285804f));
        AnimationCurve frequencyCurve = new AnimationCurve(new Keyframe(0f, 0.002f, 0f, 0f), new Keyframe(2f, 0.002f, 0f, 0f));

        return LeanAudio.createAudio(volumeCurve, frequencyCurve, LeanAudio.options().setVibrato(new Vector3[] { new Vector3(0.003f, 0f, 0f), new Vector3(0.0012f, 0f, 0f), new Vector3(0.0016f, 0f, 0f) }));
    }

    // Ambient Low-Mid Vibrato Tone 2.5
    // http://leanaudioplay.dentedpixel.com/?d=a:fvb:8,,.002,,,2.5,0.002,,,0.001822857f,0.004420519,,1.83897,1.83897,,,.5,.002288329,-,,8~~~0~~3,1000,1
    public AudioClip PlayLongAmbientLowMidWaveVibrato()
    {
        AnimationCurve volumeCurve = new AnimationCurve(new Keyframe(-0.001822857f, 0.004420519f, 1.83897f, 1.83897f), new Keyframe(1.261261f, 0.9045518f, 0.02452584f, 0.008126878f), new Keyframe(2.5f, 0.003967403f, -2.285804f, -2.285804f));
        AnimationCurve frequencyCurve = new AnimationCurve(new Keyframe(0f, 0.002f, 0f, 0f), new Keyframe(2.5f, 0.002f, 0f, 0f));

        return LeanAudio.createAudio(volumeCurve, frequencyCurve, LeanAudio.options().setVibrato(new Vector3[] { new Vector3(0.003f, 0f, 0f), new Vector3(0.0012f, 0f, 0f), new Vector3(0.0016f, 0f, 0f) }));
    }

    // http://leanaudioplay.dentedpixel.com/?d=a:fvb:4,.01002278,.001229508,,,.03234624,.0005327869,,,.05831435,.0003688525,,,.1047836,.008155738,,,4~8,.0001858941,1.033342,,-,.0405467,.8681767,-3,-1.7,.2,.02177083,-,,2~.1,,,~~0~~
    public AudioClip PlaySineWaveHighPitch()
    {
        AnimationCurve volumeCurve = new AnimationCurve(new Keyframe(0.0001858941f, 1.033342f, 0f, -1f), new Keyframe(0.0405467f, 0.8681767f, -3f, -1.7f), new Keyframe(0.2f, 0.02177083f, -1f, 0f));
        AnimationCurve frequencyCurve = new AnimationCurve(new Keyframe(0.01002278f, 0.001229508f, 0f, 0f), new Keyframe(0.03234624f, 0.0005327869f, 0f, 0f), new Keyframe(0.05831435f, 0.0003688525f, 0f, 0f), new Keyframe(0.1047836f, 0.008155738f, 0f, 0f));

        return LeanAudio.createAudio(volumeCurve, frequencyCurve, LeanAudio.options().setVibrato(new Vector3[] { new Vector3(0.1f, 0f, 0f) }));
    }

}
