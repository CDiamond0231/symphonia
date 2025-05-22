//\====================================================================================
//\ Author: DiamondandPlatinum3
//\ About : AudioManager.h - 
//\
//\ Creates a class that will be used as a template for defining the the Audio aspects
//\ of the game, as well as maintaining audio information.
//\ 
//\ The class uses a lot of context from the FMOD Library and header files.
//\====================================================================================
#ifndef _AUDIOMANAGER_H_
#define _AUDIOMANAGER_H_

#ifndef _NUM_CHANNELS_
#define _NUM_CHANNELS_ 3
#endif

#include "FMOD/fmod.hpp"
#include <string>
#include <list>


enum SoundType
{
	BGM = 0,
	BGS = 1,
	SE = 2,

	COUNT,
};

//=======================
//	Audio Manager
//=======================
class AudioManager
{
//////////////////////////////////////////
public:
//////////////////////////////////////////	

	//===============================================
	//			Constructor & Destructor
	//===============================================
	AudioManager();
	~AudioManager();




	//===============================================
	//			Static Functions
	//===============================================

	// Get the one time instance of the AudioManager class
	static AudioManager* GetInstance();


	//===============================================
	//			Public Functions
	//===============================================

	// As a rule, update must be called every frame
	bool Update(float fDeltaTime);

	int GetFMODResult();

	bool IsPlaying(SoundType eSoundType);

	unsigned int GetSoundIDOfPlayingAudio(SoundType eSoundType);

	// Play a Sound, pass in the alias name of a sound you have imported, if you know what soundtype it is (BGM, BGS, SFX) you can pass that in as an argument to speed up the process
	bool PlaySound(unsigned int iSoundId, SoundType eSoundType = SoundType::COUNT);

	// Begins Playing a Sound and immediately pauses
	bool PreloadSound(unsigned int iSoundId, SoundType eSoundType = SoundType::COUNT);

	// Stop a Channel (including any and all sounds it is playing), pass in the alias name of a sound you have imported, Channel Types: BGM, BGS, SFX
	bool StopSound(SoundType eChannelType);

	// Gets the current position of the Sound in Milliseconds
	unsigned int GetSoundPosition(SoundType eChannelType);

	// Gets the current position of the Sound in Milliseconds
	float GetSoundProgress(SoundType eChannelType);

	// Gets the current position of the Sound in Milliseconds
	unsigned int GetSoundLength(unsigned int iSoundId, SoundType eSoundType = SoundType::COUNT);

	// Fadein Sound, pass in the amount of time you want to use until the sound is fully faded in. Use this function after using the PlaySound function
	void FadeinSound(SoundType echannelType, float fSecondsToFadeIn);

	// Fadeout Sound, pass in the amount of time you want to use until the sound is fully faded in. Use this function after using the PlaySound function
	void FadeoutSound(SoundType eChannelType, float fSecondsToFadeOut);

	// Import New Audio Files into the AudioManager
	// Filename: Filename (including extension) of the Audio File you are importing, path is included with the directory string you have set earlier
	// AudioName: You may give this sound file an alias name, this alias name will be used to identify the sound for other functions. For Example name the file "Title Theme"
	// Remove Silence From Start: This will detect silence at the start of an audio file and skip straight to the start of the audio during playback.
	// Volume: Volume of the sound file, can be between 5..100. The volume is offset by the channel volume, so 80 volume for this sound and 80 volume for the channel volume will yield a 64 total volume when played
	// Speed: Speed of the Sound File, can be between 5..200. This ONLY affects MIDI files, does not affect other sound types, they will still play at normal speeds
	// LoopStart: Position to start Looping Audio via SampleRates, if you don't know what this is, best leave it as 0
	// LoopEnd: Position to end Audio and restart from Loopstart, again if you don't know what this is, leave it as 0
	unsigned int ImportAudio( std::string sFileName, SoundType eAudioType, bool bLoopAudio, bool bRemoveSilenceFromStart, 
							  unsigned int iVolume = 80, unsigned int iSpeed = 100, unsigned int iSampleRate_LoopStart = 0, unsigned int iSampleRate_LoopEnd = 0 );
	
	// Restore Default Sound Options, sets all channel volumes back to 80
	void RestoreDefaults();

	// Prints out Audio Loop Information. Use for debugging
	void PrintOutAudioLoopInfo();

	//===============================================
	//			Getter Functions
	//===============================================
	
	// Use this function if you have lost your reference to an imported Sound Object.
	unsigned int GetSoundIDOfImportedAudioFIle(std::string sFilePath);

	// Get Volume for a Sound, pass in the alias name of a sound you have imported, if you know what soundtype it is (BGM, BGS, SFX) you can pass that in as an argument to speed up the process
	unsigned int GetVolume( unsigned int iSoundId, SoundType eSoundType = SoundType::COUNT);

	// Get Tempo for a Sound, pass in the alias name of a sound you have imported, if you know what soundtype it is (BGM, BGS, SFX) you can pass that in as an argument to speed up the process
	unsigned int GetTempo( unsigned int iSoundId, SoundType eSoundType = SoundType::COUNT);

	// See if a channel is mute. Channel Types: BGM, BGS, SFX
	bool IsChannelMuted(SoundType eChannelType );

	//===============================================
	//			Setter Functions
	//===============================================

	// Set Volume For Entire System, including BGM, BGS & SFX
	void SetMasterVolume( unsigned int iVolume );	

	// Set Individual Channel Volume, Channel Types: BGM, BGS, SFX
	void SetChannelVolume(SoundType eChannelType, unsigned int iVolume );

	// Set Volume for an individual sound, the channel sound still reflects off of this, so an 80 volume sound mixed in with an 80 channel volume sound will yield a 64 volume sound when played
	// If you know what soundtype it is (BGM, BGS, SFX) you can pass that in as an argument to speed up the process
	void SetSoundVolume( unsigned int iSoundId, unsigned int iVolume, SoundType eSoundType = SoundType::COUNT);

	// Set Tempo for an individual sound, only works for MIDI Files, if you know what soundtype it is (BGM, BGS, SFX) you can pass that in as an argument to speed up the process
	void SetSoundTempo( unsigned int iSoundId, unsigned int iTempo, SoundType eSoundType = SoundType::COUNT);
	
	// Set Sound Pause for a Channel, basically you can mute all sounds in that channel
	bool SetChannelPause(SoundType eChannelType, bool bSoundPaused );

	// Set Mute, you may mute a channel with this function. Channel Types: BGM, BGS, SFX
	void SetMute(SoundType eChannelType );



//////////////////////////////////////////
private:
//////////////////////////////////////////
	
	struct AudioInfo
	{
		std::string				FilePath;
		unsigned int			SoundID;
		SoundType				SoundType; // BGM, BGS, SFX
		float					Volume;
		float					Tempo;
		unsigned int			AudioStartMS; // At what point does the audio actually start
		unsigned int			LoopStart;
		unsigned int			LoopEnd;
		FMOD::Sound*			Sound_ptr;
	};

	struct ChannelInfo
	{
		FMOD::Channel**			ChannelPTR;
		AudioInfo*				CurrentSound;
		float					Volume;
		bool					Paused;
		bool					Fadein;
		bool					Fadeout;
		float					FadeTime;
		float					CurrentFadeTime;
		std::list<AudioInfo*>	ImportedAudioList;
	};

	//===============================================
	//			Private Declarations
	//===============================================
	ChannelInfo			 m_ChannelInfos[3];

	unsigned int		 m_uiBGMReservedAudio;
	unsigned int		 m_uiBGSReservedAudio;

	bool				 m_bMuteSoundEffects;
	unsigned int		 m_uiTotalSoundsImported;


	FMOD_RESULT			 FMOD_Result;
	FMOD::System*		 m_pFMODSystem;
	FMOD::Channel*		 m_pBGMChannel;
	FMOD::Channel*		 m_pBGSChannel;
	FMOD::Channel*		 m_pSEChannel;


	//===============================================
	//			Static Declarations
	//===============================================
	static AudioManager* m_Instance;


	//===============================================
	//			Private Functions
	//===============================================
	bool		 InitSound();
	void		 CorrectChannelVolume( ChannelInfo& a_Channel, float fVolume );
	void		 UpdateChannelFade( ChannelInfo& a_Channel, float fdeltaTime );
	bool		 BGMWasFoundAndPlaying(unsigned int iSoundId);
	bool		 BGSWasFoundAndPlaying(unsigned int iSoundId);
	bool		 SEWasFoundAndPlaying(unsigned int iSoundId);
	float		 CheckVolumeIsValid( unsigned int iVolume );
	float		 CheckTempoIsValid( unsigned int iTempo );
	AudioInfo*	 FindSoundFromPTR( FMOD::Sound* pSound );
	AudioInfo*	 FindAudioInfoInBGMImportedList( unsigned int iSoundId );
	AudioInfo*	 FindAudioInfoInBGSImportedList(unsigned int iSoundId );
	AudioInfo*	 FindAudioInfoInSEImportedList( unsigned int iSoundId );
	AudioInfo*	 GetAudioInfoObject(unsigned int iSoundId, SoundType eSoundType);
	ChannelInfo* GetChannelInfo(SoundType eSoundType);
};

#endif