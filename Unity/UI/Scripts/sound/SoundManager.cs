using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager 
{
	public bool IsMute = false;
    public MusicPlayer mPlayer;
    public bool isMusicEnabled = true;
    public float Volume { get; set; } = 1.0f;

    private SoundEffectsLib soundEffectsLib;
    private Dictionary<string, SoundEffect> sounds;
    private AudioSource audioSrc;

    public SoundManager(AudioSource _audio, bool _isMusicEnabled)
    {
        isMusicEnabled = _isMusicEnabled;
        audioSrc = _audio;

        InitializeSoundEffects();
        InitializeMusicPlayer();
    }

    private void InitializeSoundEffects()
    {
        // Initialize Sound Effects
        sounds = new Dictionary<string, SoundEffect>();
        soundEffectsLib = new SoundEffectsLib();

        // cache generated sound effects
        sounds["PlayCountdown"] = new SoundEffect()
        {
            audioClip = soundEffectsLib.PlayCountdown(),
            volume = 0.5f
        };

        sounds["PlayCountdownGo"] = new SoundEffect()
        {
            audioClip = soundEffectsLib.PlayCountdownGo(),
            volume = 1.0f
        };

        sounds["PlayTooSlow"] = new SoundEffect()
        {
            audioClip = soundEffectsLib.PlayTooSlow(),
            volume = 0.5f
        };

        sounds["PlayCountdownHighPitchVibrato"] = new SoundEffect()
        {
            audioClip = soundEffectsLib.PlayCountdownHighPitchVibrato(),
            volume = 0.75f
        };

        sounds["PlayCountdownHighPitchEndVibrato"] = new SoundEffect()
        {
            audioClip = soundEffectsLib.PlayCountdownHighPitchEndVibrato(),
            volume = 0.95f
        };

        sounds["PlayLowToneButton"] = new SoundEffect()
        {
            audioClip = soundEffectsLib.PlayLowToneButton(),
            volume = 0.5f
        };

        sounds["PlayNoiseMidHighTone"] = new SoundEffect()
        {
            audioClip = soundEffectsLib.PlayNoiseMidHighTone(),
            volume = 0.65f
        };

        sounds["PlaySawLowHighTone"] = new SoundEffect()
        {
            audioClip = soundEffectsLib.PlaySawLowHighTone(),
            volume = 1.0f
        };

        sounds["PlaySawLowMidHighTone"] = new SoundEffect()
        {
            audioClip = soundEffectsLib.PlaySawLowMidHighTone(),
            volume = 0.95f
        };

        sounds["PlaySineWaveHighPitch"] = new SoundEffect()
        {
            audioClip = soundEffectsLib.PlaySineWaveHighPitch(),
            volume = 0.5f
        };

        sounds["PlayLongAmbientSineWaveVibrato"] = new SoundEffect()
        {
            audioClip = soundEffectsLib.PlayLongAmbientSineWaveVibrato(),
            volume = 1.0f
        };

        sounds["PlayLongAmbientLowMidWaveVibrato"] = new SoundEffect()
        {
            audioClip = soundEffectsLib.PlayLongAmbientLowMidWaveVibrato(),
            volume = 1.0f
        };

        //sounds["PlayCountdown"] = soundEffectsLib.PlayCountdown();
        //sounds["PlayCountdownGo"] = soundEffectsLib.PlayCountdownGo();
        //sounds["PlayTooSlow"] = soundEffectsLib.PlayTooSlow();
        //sounds["PlayCountdownHighPitchVibrato"] = soundEffectsLib.PlayCountdownHighPitchVibrato();
        //sounds["PlayCountdownHighPitchEndVibrato"] = soundEffectsLib.PlayCountdownHighPitchEndVibrato();
        //sounds["PlayLowToneButton"] = soundEffectsLib.PlayLowToneButton();
        //sounds["PlayNoiseMidHighTone"] = soundEffectsLib.PlayNoiseMidHighTone();
        //sounds["PlaySawLowHighTone"] = soundEffectsLib.PlaySawLowHighTone();
        //sounds["PlaySawLowMidHighTone"] = soundEffectsLib.PlaySawLowMidHighTone();
        //sounds["PlaySineWaveHighPitch"] = soundEffectsLib.PlaySineWaveHighPitch();
        //sounds["PlayLongAmbientSineWaveVibrato"] = soundEffectsLib.PlayLongAmbientSineWaveVibrato();
        //sounds["PlayLongAmbientLowMidWaveVibrato"] = soundEffectsLib.PlayLongAmbientLowMidWaveVibrato();
    }

    public void InitializeMusicPlayer()
    {
        mPlayer = new MusicPlayer(isMusicEnabled);
        mPlayer.AddSong("MusicTrack_1_Sun_Spots");                  // 0 Start Screen
        mPlayer.AddSong("MusicTrack_2_Start_Your_Engines");         // 1 Start Race
        mPlayer.AddSong("MusicTrack_3_EasyTopUnderscore");          // 2 End Race / Congratulations
        mPlayer.AddSong("MusicTrack_4_Hungry_For_Disaster");        // 3 Start Race
        mPlayer.AddSong("MusicTrack_5_Road_Star");                  // 4 Final Congratulations
        mPlayer.AddSong("MusicTrack_6_Run");                        // 5 Start Race
    }

    public void PlaySound(string soundKey, float volumeOverride = 0.0f, float pan = 0.0f)
    {
        if (pan > 0.0f && pan < 0.0f || volumeOverride > 0.0f)
        {
            volumeOverride = VolumeOverrideCheck(soundKey, volumeOverride);

            LeanAudio.play(sounds[soundKey].audioClip, volumeOverride).panStereo = pan;
            return;
        }

        audioSrc.PlayOneShot(sounds[soundKey].audioClip, sounds[soundKey].volume);
    }

    // LeanPlay gives us more control on volume and pan
    /*public void LeanPlaySound(string soundKey, float volumeOverride = 0.0f, float pan = 0.0f)
    {
        if (pan > 0.0f && pan < 0.0f)
        {
            LeanAudio.play(sounds[soundKey].audioClip, volumeOverride).panStereo = pan;
            return;
        }

        volumeOverride = VolumeOverrideCheck(volumeOverride);
        
        LeanAudio.play(sounds[soundKey].audioClip, volumeOverride);
    }*/

    public float VolumeOverrideCheck(string soundKey, float volumeOverride = 0.0f)
	{
		if (volumeOverride == 0.0f) volumeOverride = sounds[soundKey].volume; 

		return volumeOverride;
	}
}
