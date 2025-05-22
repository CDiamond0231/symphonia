//\====================================================================================
//\ Author: Christopher Diamond
//\ About : AudioManager.cpp - 
//\ Date: 11th August, 2012
//\
//\ Creates the AudioManager source file. This cpp defines the functions made in 
//\ the header file.
//\====================================================================================




//////////////////////////////////////////////////
//	Includes some header files which we will  ////
//	be using in this .cpp file				  ////
//////////////////////////////////////////////////
#include "AudioManager.h"					  ////
#include <assert.h>							  ////
#include <iostream>							  ////
//////////////////////////////////////////////////


AudioManager* AudioManager::m_Instance = NULL;

//===============================================
//	Get Class Instance ~ Singleton
//===============================================
AudioManager* AudioManager::GetInstance()
{
	return m_Instance;
}


//===============================================
//	Constructor
//===============================================
AudioManager::AudioManager()
{
	if( m_Instance == NULL )	
	{ 
		m_Instance = this; 
	}
	else						
	{ 
		std::cout << "ERROR: Only One Instance of the AudioManager can exist at any one time! \n\n\n\n\n";
		assert(0); 
	}
	
	
	
	
	InitSound();

	m_bMuteSoundEffects  = false;


	for(int i = 0; i < 3; i++)
	{
		m_ChannelInfos[i].Volume			= 0.8f;
		m_ChannelInfos[i].Paused			= false;
		m_ChannelInfos[i].Fadein			= false;
		m_ChannelInfos[i].Fadeout			= false;
		m_ChannelInfos[i].FadeTime			= 0.f;
		m_ChannelInfos[i].CurrentFadeTime	= 0.f;
		m_ChannelInfos[i].CurrentSound		= NULL;
	}

	m_ChannelInfos[0].ChannelPTR = &m_pBGMChannel;
	m_ChannelInfos[1].ChannelPTR = &m_pBGSChannel;
	m_ChannelInfos[2].ChannelPTR = &m_pSEChannel;


	m_uiTotalSoundsImported = 0;
	m_uiBGMReservedAudio  = 0;
	m_uiBGSReservedAudio  = 0;
}


//===============================================
//	Destructor
//===============================================
AudioManager::~AudioManager()
{
	m_Instance = NULL;

	if (m_pFMODSystem != NULL)
	{
		for (int i = 0; i < 3; i++)
		{
			StopSound((SoundType)i);

			for (std::list<AudioInfo*>::iterator iter = m_ChannelInfos[i].ImportedAudioList.begin(); iter != m_ChannelInfos[i].ImportedAudioList.end(); iter++)
			{
				(*iter)->Sound_ptr->release();
				delete (*iter);
			}

			m_ChannelInfos[i].ImportedAudioList.clear();
		}

		m_pFMODSystem->close();
		m_pFMODSystem->release();
		m_pFMODSystem = NULL;
	}
}

//===============================================
//	Initialise Sound
//===============================================
bool AudioManager::InitSound()
{
	FMOD_Result = System_Create( &m_pFMODSystem );
	if (FMOD_Result != FMOD_RESULT::FMOD_OK)
	{
		return false;
	}

	FMOD_Result = m_pFMODSystem->init( _NUM_CHANNELS_, FMOD_INIT_NORMAL, 0 );	
	return FMOD_Result == FMOD_RESULT::FMOD_OK;
}





//===============================================
//	Get Volume
//===============================================
unsigned int AudioManager::GetVolume( unsigned int iSoundId, SoundType eSoundType )
{
	AudioInfo* AudInfo = GetAudioInfoObject(iSoundId, eSoundType);
	if(AudInfo != NULL)
	{
		return (unsigned int)(AudInfo->Volume * 100.f); 
	}

	return 80; // 80 is default volume sound, return this if the Sound cannot be found
}
//===============================================
//	Get Tempo
//===============================================
unsigned int AudioManager::GetTempo( unsigned int iSoundId, SoundType eSoundType )
{
	AudioInfo* AudInfo = GetAudioInfoObject(iSoundId, eSoundType);
	if(AudInfo != NULL)
	{
		return (unsigned int)(AudInfo->Tempo * 100.f); 
	}

	return 100; // 100 is default Tempo of sound, return this if the Sound cannot be found
}
//===============================================
//	Get 'Is Mute'
//===============================================
bool AudioManager::IsChannelMuted(SoundType eChannelType )
{
	bool bMute = false;
	if( eChannelType == SoundType::BGM )
	{ 
		m_pBGMChannel->getMute( &bMute ); 
	}
	else if( eChannelType == SoundType::BGS)
	{ 
		m_pBGSChannel->getMute( &bMute ); 
	}
	else if( eChannelType == SoundType::SE)
	{ 
		bMute = m_bMuteSoundEffects; 
	}

	return bMute;
}





//===============================================
//	Set Master Volume
//===============================================
void AudioManager::SetMasterVolume( unsigned int iVolume )
{
	float fVolume = CheckVolumeIsValid( iVolume );

	m_ChannelInfos[0].Volume = fVolume;
	m_ChannelInfos[1].Volume = fVolume;
	m_ChannelInfos[2].Volume = fVolume;

	// Correct Volume for all Channels
	CorrectChannelVolume(m_ChannelInfos[0], fVolume);
	CorrectChannelVolume(m_ChannelInfos[1], fVolume);
	CorrectChannelVolume(m_ChannelInfos[2], fVolume);
}
//===============================================
//	Set Channel Volume
//===============================================
void AudioManager::SetChannelVolume(SoundType eChannelType, unsigned int iVolume )
{
	ChannelInfo* CI = GetChannelInfo(eChannelType);
	if(CI != NULL)
	{
		CorrectChannelVolume(*CI, ((float)iVolume * 0.01f) );
	}
}
//===============================================
//	Set Sound Volume
//===============================================
void AudioManager::SetSoundVolume(unsigned int iSoundId, unsigned int iVolume, SoundType eSoundType )
{
	AudioInfo* AudInfo = GetAudioInfoObject(iSoundId, eSoundType);
	if(AudInfo != NULL)
	{
		AudInfo->Volume = CheckVolumeIsValid( iVolume );
	}
}
//===============================================
//	Set Sound Tempo
//===============================================
void AudioManager::SetSoundTempo( unsigned int iSoundId, unsigned int iTempo, SoundType eSoundType )
{
	AudioInfo* AudInfo = GetAudioInfoObject(iSoundId, eSoundType);
	if(AudInfo != NULL)
	{
		AudInfo->Volume = CheckTempoIsValid( iTempo );
	}
}
//===============================================
//	Set Channel Pause
//===============================================
bool AudioManager::SetChannelPause(SoundType eChannelType, bool bSoundPaused )
{
	ChannelInfo* CI = GetChannelInfo(eChannelType);
	if(CI != NULL)
	{
		CI->Paused = bSoundPaused;
		FMOD_Result = (*CI->ChannelPTR)->setPaused(bSoundPaused);
		return FMOD_Result == FMOD_RESULT::FMOD_OK;
	}

	return false;
}
//===============================================
//	Set Mute
//=============================================
void AudioManager::SetMute(SoundType eChannelType )
{
	bool bMute = false;
	/////////////////////////////////////////
	if( eChannelType == SoundType::BGM)
	{
		m_pBGMChannel->getMute( &bMute );

		if( !bMute )
		{ m_pBGMChannel->setMute( true ); }
		else
		{ 
			m_pBGMChannel->setMute( false ); 

			// Play Reserved Sound if you're no longer mute
			if( m_uiBGMReservedAudio != 0 )
			{
				PlaySound( m_uiBGMReservedAudio );
				m_uiBGMReservedAudio = 0;
			}
		}
	}
	/////////////////////////////////////////
	else if( eChannelType == SoundType::BGS)
	{
		m_pBGSChannel->getMute( &bMute );

		if( !bMute )
		{ m_pBGSChannel->setMute( true ); }
		else
		{ 
			m_pBGSChannel->setMute( false ); 

			if( m_uiBGSReservedAudio != 0 )
			{
				PlaySound( m_uiBGSReservedAudio );
				m_uiBGSReservedAudio = 0;
			}
		}
	}
	/////////////////////////////////////////
	else if( eChannelType == SoundType::SE)
	{
		if( !m_bMuteSoundEffects )
		{ m_bMuteSoundEffects = true; }
		else
		{ m_bMuteSoundEffects = false; }
	}
	/////////////////////////////////////////
}



//===============================================
//	Restore Defaults
//===============================================
void AudioManager::RestoreDefaults()
{
	// Correct Volume for all Channels
	CorrectChannelVolume(m_ChannelInfos[0], 0.8f);
	CorrectChannelVolume(m_ChannelInfos[1], 0.8f);
	CorrectChannelVolume(m_ChannelInfos[2], 0.8f);
}










//===============================================
//	Update Sound
//===============================================
bool AudioManager::Update(float a_fDeltaTime)
{
	for (int i = 0; i < 3; i++)
	{
		UpdateChannelFade(m_ChannelInfos[i], a_fDeltaTime);
	}

	// Update SoundSystem
	FMOD_Result = m_pFMODSystem->update();
	return FMOD_Result == FMOD_RESULT::FMOD_OK;
}
//===============================================
//	Get FMOD Result
//===============================================
int AudioManager::GetFMODResult()
{
	return (int)FMOD_Result;
}
//===============================================
//	Is Playing?
//===============================================
bool AudioManager::IsPlaying(SoundType a_eSoundType)
{
	ChannelInfo* pChannel = GetChannelInfo(a_eSoundType);
	if (pChannel != NULL)
	{
		bool isPlaying;
		FMOD_Result = (*pChannel->ChannelPTR)->isPlaying(&isPlaying);
		return isPlaying;
	}
	return false;
}
//===============================================
//	Get Sound ID Of Playing Audio
//===============================================
unsigned int AudioManager::GetSoundIDOfPlayingAudio(SoundType a_eSoundType)
{
	if (IsPlaying(a_eSoundType) == false)
	{
		return 0;
	}

	ChannelInfo* CI = GetChannelInfo(a_eSoundType);
	if (CI != NULL && CI->CurrentSound != NULL)
	{
		return CI->CurrentSound->SoundID;
	}

	return 0;
}




//===============================================
//	Update Channel Fade
//===============================================
void AudioManager::UpdateChannelFade( ChannelInfo& a_Channel, float a_fDeltaTime )
{	
	if (!a_Channel.Fadein && !a_Channel.Fadeout)
	{
		return;
	}

	 // Get Current Channel Volume
	 float fVolume;
	 (*a_Channel.ChannelPTR)->getVolume(&fVolume);

	 if (a_Channel.FadeTime == 0.0f)
	 {
		 a_Channel.Fadein = false;
		 a_Channel.Fadeout = false;
		 a_Channel.CurrentFadeTime = 0.0f;

		 // Set Volume
		 (*a_Channel.ChannelPTR)->setPaused(true);
		 (*a_Channel.ChannelPTR)->setVolume(fVolume);
		 (*a_Channel.ChannelPTR)->setPaused(false);

		 return;
	 }

	 a_Channel.CurrentFadeTime += a_fDeltaTime;

	 // Get Update Volume Via DeltaTime
	 float fTempVolume = ((a_fDeltaTime * a_Channel.Volume * a_Channel.CurrentSound->Volume) / a_Channel.FadeTime);
	
	 // Add or Deduct Volume from Current Channel Volume
	 fVolume += a_Channel.Fadein ? fTempVolume : -fTempVolume;

	 // Set Volume
	 (*a_Channel.ChannelPTR)->setPaused(true);
	 (*a_Channel.ChannelPTR)->setVolume(fVolume);
	 (*a_Channel.ChannelPTR)->setPaused(false);

	 // If Fully Faded in/out, set fade to false
	 if(a_Channel.CurrentFadeTime >= a_Channel.FadeTime)
	 {
		 a_Channel.Fadein = false;
		 a_Channel.Fadeout = false;
		 a_Channel.CurrentFadeTime = 0.0f;
	 }
}




//===============================================
//	Print Out Audio Loop Info
//===============================================
void AudioManager::PrintOutAudioLoopInfo()
{
	//////////////////////////////////////
	unsigned int iSoundPosition, iLoopStart, iLoopEnd;
	//////////////////////////////////////
	m_pBGMChannel->getPosition( &iSoundPosition, FMOD_TIMEUNIT_PCM );
	m_pBGMChannel->getLoopPoints( &iLoopStart, FMOD_TIMEUNIT_PCM, &iLoopEnd, FMOD_TIMEUNIT_PCM );
	//////////////////////////////////////
	std::cout << "\n\nCurrent Sound Position: " << iSoundPosition << std::endl;
	//////////////////////////////////////
	if( iSoundPosition > iLoopStart )
		std::cout << "Sound has Entered LoopStart\n";	
	else
		std::cout << "Sound has not Entered LoopStart\n";
	//////////////////////////////////////
	if( iSoundPosition == iLoopEnd )
		std::cout << "Sound has hit LoopEnd\n";	
	else
		std::cout << "Sound has not hit LoopEnd\n";
	//////////////////////////////////////
	std::cout << "LoopStart = " << iLoopStart << std::endl;
	std::cout << "LoopEnd = " << iLoopEnd << "\n\n\n\n\n";
	//////////////////////////////////////
}




//===============================================
//	Import Audio
//===============================================
unsigned int AudioManager::ImportAudio( std::string sFileName, SoundType eAudioType, bool bLoopAudio, bool bRemoveSilenceFromStart, 
										unsigned int iVolume, unsigned int iTempo, unsigned int iSampleRate_LoopStart, unsigned int iSampleRate_LoopEnd )
{
	if (sFileName.length() < 5)
	{
		std::cout << "\n\nInvalid File Extension.\n\n\n\n\n";
		return 0;
	}

	//-----------------------------------------------------------------------------------------------------------------------------------------
	if( eAudioType == SoundType::COUNT )
	{
		#if defined(__WIN32__)
			MessageBox(NULL, L"Mate... Gotta define the type of the Sound, it's either a 'BGM', 'BGS' or a 'SFX'.", L"Import_Failed", MB_ERROR);
		#endif
		
		std::cout << "\n\nImport Sound Failed: Mate... Gotta define the type of the Sound, it's either a 'BGM', 'BGS' or a 'SFX'.\n\n\n\n\n";
		return 0;
	}
	//-----------------------------------------------------------------------------------------------------------------------------------------
	
	unsigned int uiImportedSoundID = GetSoundIDOfImportedAudioFIle( sFileName );
	if (uiImportedSoundID != 0)
	{
		// Already imported
		FMOD_Result = FMOD_RESULT::FMOD_OK;
		return uiImportedSoundID;
	}

	unsigned int soundId = m_uiTotalSoundsImported + 1;

	// Create Info for the Sound
	AudioInfo* pAudInfo = new AudioInfo();
	pAudInfo->FilePath	= sFileName;
	pAudInfo->SoundID	= soundId;
	pAudInfo->SoundType	= eAudioType;
	pAudInfo->Volume	= ((float)iVolume * 0.01f);
	pAudInfo->Tempo		= ((float)iTempo * 0.01f);
	pAudInfo->LoopStart	= iSampleRate_LoopStart;
	pAudInfo->LoopEnd	= iSampleRate_LoopEnd;

	
    FMOD_MODE mode = FMOD_HARDWARE | FMOD_2D | FMOD_ACCURATETIME;
    if (bLoopAudio)
    {
        mode |= FMOD_LOOP_NORMAL;
    }
    else
    {
        mode |= FMOD_DEFAULT;
    }

	const char* sPath = pAudInfo->FilePath.c_str();
	if(pAudInfo->SoundType == SoundType::BGM)
	{
		FMOD_Result = m_pFMODSystem->createStream(sPath, mode, NULL, &pAudInfo->Sound_ptr );
		if (FMOD_Result != FMOD_RESULT::FMOD_OK)
		{
			return 0;
		}
		m_ChannelInfos[0].ImportedAudioList.push_back(pAudInfo);
	}
	else if(pAudInfo->SoundType == SoundType::BGS)
	{
		FMOD_Result = m_pFMODSystem->createSound(sPath, mode, NULL, &pAudInfo->Sound_ptr);
		if (FMOD_Result != FMOD_RESULT::FMOD_OK)
		{
			return 0;
		}
		m_ChannelInfos[1].ImportedAudioList.push_back(pAudInfo);
	}
	else
	{
		FMOD_Result = m_pFMODSystem->createSound(sPath, mode, NULL, &pAudInfo->Sound_ptr);
		if (FMOD_Result != FMOD_RESULT::FMOD_OK)
		{
			return 0;
		}
		m_ChannelInfos[2].ImportedAudioList.push_back(pAudInfo);
	}

	if (bRemoveSilenceFromStart)
	{
		unsigned int msLength;
		pAudInfo->Sound_ptr->getLength(&msLength, FMOD_TIMEUNIT_MS);

		unsigned int pcmBytesLength;
		pAudInfo->Sound_ptr->getLength(&pcmBytesLength, FMOD_TIMEUNIT_PCMBYTES);

		const unsigned int bufSize = 1000000;
		char* buffer = new char[bufSize];

		unsigned int readCount = 0;
		pAudInfo->Sound_ptr->readData(buffer, bufSize, &readCount);
		pAudInfo->Sound_ptr->seekData(0);

		for (unsigned int i = 0; i < readCount; ++i)
		{
			if (buffer[i] != '\0')
			{
				pAudInfo->AudioStartMS = (unsigned int)(((float)i / pcmBytesLength) * msLength);
				break;
			}
		}
		delete[] buffer;
	}
	
	m_uiTotalSoundsImported += 1;
	return soundId;
}






//===============================================
//	Fadein Sound
//===============================================
void AudioManager::FadeinSound(SoundType eChannelType, float fSecondsToFadeIn)
{
	ChannelInfo* CI = GetChannelInfo(eChannelType);
	if(CI != NULL)
	{
		CI->Paused		= false;
		CI->Fadein		= true;
		CI->Fadeout		= false;
		CI->FadeTime	= fSecondsToFadeIn;

		// Set Low Volume
		(*CI->ChannelPTR)->setPaused(true);
		(*CI->ChannelPTR)->setVolume(0.001f);
		(*CI->ChannelPTR)->setPaused(false);
	}
}


//===============================================
//	Fadeout Sound
//===============================================
void AudioManager::FadeoutSound(SoundType eChannelType, float fSecondsToFadeOut)
{
	ChannelInfo* CI = GetChannelInfo(eChannelType);
	if(CI != NULL)
	{
		CI->Paused		= false;
		CI->Fadein		= false;
		CI->Fadeout		= true;
		CI->FadeTime	= fSecondsToFadeOut;
	}
}


//===============================================
//	Stop Sound
//===============================================
bool AudioManager::StopSound(SoundType eChannelType)
{
	ChannelInfo* CI = GetChannelInfo(eChannelType);
	if(CI != NULL)
	{
		FMOD_Result = (*CI->ChannelPTR)->stop();
		return FMOD_Result == FMOD_RESULT::FMOD_OK;
	}
	return false;
}

//===============================================
//	Get Sound Position
//===============================================
unsigned int AudioManager::GetSoundPosition(SoundType eChannelType)
{
	unsigned int uiPosition = 0;
	ChannelInfo* CI = GetChannelInfo(eChannelType);
	if (CI != NULL)
	{
		FMOD_Result = (*CI->ChannelPTR)->getPosition(&uiPosition, FMOD_TIMEUNIT_MS);
	}
	return uiPosition;
}

//===============================================
//	Get Sound Position
//===============================================
float AudioManager::GetSoundProgress(SoundType eChannelType)
{
	ChannelInfo* CI = GetChannelInfo(eChannelType);
	if (CI != NULL)
	{
		FMOD::Sound* pCurrentSoundPlaying = NULL;
		(*CI->ChannelPTR)->getCurrentSound(&pCurrentSoundPlaying);
		if (pCurrentSoundPlaying == NULL)
		{
			return 0.0f;
		}

		unsigned int uiCurrPosition;
		FMOD_Result = (*CI->ChannelPTR)->getPosition(&uiCurrPosition, FMOD_TIMEUNIT_MS);

		unsigned int uiTotalLength;
		FMOD_Result = pCurrentSoundPlaying->getLength(&uiTotalLength, FMOD_TIMEUNIT_MS);

		float fProgress = (float)uiCurrPosition / uiTotalLength;
		return fProgress;
	}
	return 0.0f;
}

//===============================================
//	Get Sound Length
//===============================================
unsigned int AudioManager::GetSoundLength(unsigned int iSoundId, SoundType eSoundType)
{
	AudioInfo* AudInfo = GetAudioInfoObject(iSoundId, eSoundType);
	unsigned int uiTimeInMS = 0;
	if (AudInfo != NULL)
	{
		FMOD_Result = AudInfo->Sound_ptr->getLength(&uiTimeInMS, FMOD_TIMEUNIT_MS);
	}
	return uiTimeInMS;
}


//===============================================
//	Play Sound
//===============================================
bool AudioManager::PlaySound( unsigned int iSoundId, SoundType eSoundType )
{
	if(iSoundId == 0)
	{
		return false;
	}

	if(eSoundType != SoundType::COUNT)
	{
		if(eSoundType == SoundType::BGM && BGMWasFoundAndPlaying(iSoundId)) { return true; }
		if(eSoundType == SoundType::BGS && BGSWasFoundAndPlaying(iSoundId)) { return true; }
		if(eSoundType == SoundType::SE && SEWasFoundAndPlaying(iSoundId))  { return true; }
	}

	// If We Got Up to this point, then no SoundType was Defined, we'll just have to go through each list until we find the sound
	if(BGMWasFoundAndPlaying(iSoundId) || BGSWasFoundAndPlaying(iSoundId) || SEWasFoundAndPlaying(iSoundId)) 
	{
		// Sound was Found, Exit Function
		return true;
	}

	// Sound Was Not Found, Show Error in Console
	std::cout << "Sound WIth ID: " << iSoundId << " was not found. It either does not exist or you have not imported it.\n" 
														<< "Please note that the alias name is case sensitive and that you cannot import a sound file twice, if you believe you have imported it"
														<< "correctly please check your project to be sure you have not done so twice. \n\n\n\n\n";

	return false;
}

//===============================================
//	Preload Sound
//===============================================
bool AudioManager::PreloadSound(unsigned int iSoundId, SoundType eSoundType)
{
	if (eSoundType != SoundType::BGM && eSoundType != SoundType::BGS && eSoundType != SoundType::SE)
	{
		return false;
	}

	PlaySound(iSoundId, eSoundType);
	while (!IsPlaying(eSoundType))
	{
		std::cout << "Not Loaded\n";
		// Wait
	}

	ChannelInfo* pChannel = GetChannelInfo(eSoundType);
	if (pChannel != NULL)
	{
		FMOD_Result = (*pChannel->ChannelPTR)->setPaused(true);
		return true;
	}

	return false;
}



//===============================================
//	BGM Was Found and Playing?
//===============================================
bool AudioManager::BGMWasFoundAndPlaying(unsigned int iSoundId)
{
	AudioInfo* TempAudioInfo = FindAudioInfoInBGMImportedList(iSoundId);
	if(TempAudioInfo != NULL)
	{
		if( IsChannelMuted(SoundType::BGM) )
		{ 
			m_uiBGMReservedAudio = TempAudioInfo->SoundID;
			return true; 
		}

		// Return true if already playing this sound
		FMOD::Sound* FSound;
		m_pBGMChannel->getCurrentSound(&FSound);
		if(FSound == TempAudioInfo->Sound_ptr) 
		{
			return true; 
		}

		// Setup Sound: Stop Whatever is currently playing, set loop points, play sound, then adjust volume & tempo 
		// (FMOD reconfigures volume and Tempo when it begins playing a sound, so to modify it we have to set it after we start playing)
		FMOD_Result = m_pBGMChannel->stop();
		
		// Set Loop Points (If Any)
		if( TempAudioInfo->LoopEnd > TempAudioInfo->LoopStart )
		{ 
			FMOD_Result = TempAudioInfo->Sound_ptr->setLoopPoints( TempAudioInfo->LoopStart, FMOD_TIMEUNIT_PCM, TempAudioInfo->LoopEnd, FMOD_TIMEUNIT_PCM ); 
			if (FMOD_Result != FMOD_RESULT::FMOD_OK)
			{
				return false;
			}
		}
		
		FMOD_Result = m_pFMODSystem->playSound( FMOD_CHANNEL_REUSE, TempAudioInfo->Sound_ptr, false, &m_pBGMChannel );
		if (TempAudioInfo->AudioStartMS != 0)
		{
			FMOD_Result = m_pBGMChannel->setPosition(TempAudioInfo->AudioStartMS, FMOD_TIMEUNIT_MS);
			if (FMOD_Result != FMOD_RESULT::FMOD_OK)
			{
				return false;
			}
		}

		FMOD_Result = m_pBGMChannel->setPaused(true);
		if (FMOD_Result != FMOD_RESULT::FMOD_OK)
		{
			return false;
		}
		FMOD_Result = m_pBGMChannel->setVolume(TempAudioInfo->Volume * m_ChannelInfos[0].Volume);
		if (FMOD_Result != FMOD_RESULT::FMOD_OK)
		{
			return false;
		}

		FMOD_Result = TempAudioInfo->Sound_ptr->setMusicSpeed(TempAudioInfo->Tempo);
		FMOD_Result = m_pBGMChannel->setPaused(false);
		if (FMOD_Result != FMOD_RESULT::FMOD_OK)
		{
			return false;
		}
		
		m_ChannelInfos[0].Fadein = false;
		m_ChannelInfos[0].Fadeout = false;
		m_ChannelInfos[0].Paused = false;
		m_ChannelInfos[0].CurrentSound = TempAudioInfo;

		// Found Sound in this List
		return true;
	}
	// Didn't find Sound in this List
	return false;
}
//===============================================
//	BGS Was Found and Playing?
//===============================================
bool AudioManager::BGSWasFoundAndPlaying(unsigned int iSoundId)
{
	AudioInfo* TempAudioInfo = FindAudioInfoInBGSImportedList(iSoundId);
	if(TempAudioInfo != NULL)
	{
		if( IsChannelMuted(SoundType::BGS))
		{ 
			m_uiBGSReservedAudio = TempAudioInfo->SoundID;
			return true; 
		}

		FMOD_Result = m_pBGSChannel->stop();
		if (FMOD_Result != FMOD_RESULT::FMOD_OK)
		{
			return false;
		}
		FMOD_Result = m_pFMODSystem->playSound(FMOD_CHANNEL_FREE, TempAudioInfo->Sound_ptr, false, &m_pSEChannel);
		if (FMOD_Result != FMOD_RESULT::FMOD_OK)
		{
			return false;
		}
		if (TempAudioInfo->AudioStartMS != 0)
		{
			FMOD_Result = m_pBGMChannel->setPosition(TempAudioInfo->AudioStartMS, FMOD_TIMEUNIT_MS);
			if (FMOD_Result != FMOD_RESULT::FMOD_OK)
			{
				return false;
			}
		}
		FMOD_Result = m_pSEChannel->setVolume(TempAudioInfo->Volume * m_ChannelInfos[1].Volume);
		if (FMOD_Result != FMOD_RESULT::FMOD_OK)
		{
			return false;
		}
		FMOD_Result = TempAudioInfo->Sound_ptr->setMusicSpeed(TempAudioInfo->Tempo);
		if (FMOD_Result != FMOD_RESULT::FMOD_OK)
		{
			return false;
		}
		
		m_ChannelInfos[1].Fadein = false;
		m_ChannelInfos[1].Fadeout = false;
		m_ChannelInfos[1].Paused = false;
		m_ChannelInfos[1].CurrentSound = TempAudioInfo;

		// Found Sound in this List
		return true;
	}
	// Didn't find Sound in this List
	return false;
}
//===============================================
//	SE Was Found and Playing?
//===============================================
bool AudioManager::SEWasFoundAndPlaying(unsigned int iSoundId)
{
	AudioInfo* TempAudioInfo = FindAudioInfoInSEImportedList(iSoundId);
	if(TempAudioInfo != NULL)
	{
		if( IsChannelMuted(SoundType::SE) ) 
		{ 
			return true; 
		}

		FMOD_Result = m_pFMODSystem->playSound( FMOD_CHANNEL_FREE, TempAudioInfo->Sound_ptr, false, &m_pSEChannel );
		if (FMOD_Result != FMOD_RESULT::FMOD_OK)
		{
			return false;
		}
		if (TempAudioInfo->AudioStartMS != 0)
		{
			FMOD_Result = m_pBGMChannel->setPosition(TempAudioInfo->AudioStartMS, FMOD_TIMEUNIT_MS);
			if (FMOD_Result != FMOD_RESULT::FMOD_OK)
			{
				return false;
			}
		}
		FMOD_Result = m_pSEChannel->setVolume( TempAudioInfo->Volume * m_ChannelInfos[2].Volume);
		if (FMOD_Result != FMOD_RESULT::FMOD_OK)
		{
			return false;
		}
		FMOD_Result = TempAudioInfo->Sound_ptr->setMusicSpeed(TempAudioInfo->Tempo);
		if (FMOD_Result != FMOD_RESULT::FMOD_OK)
		{
			return false;
		}
		
		m_ChannelInfos[2].Fadein = false;
		m_ChannelInfos[2].Fadeout = false;
		m_ChannelInfos[2].Paused = false;
		m_ChannelInfos[2].CurrentSound = TempAudioInfo;

		// Found Sound in this List
		return true;
	}
	// Didn't find Sound in this List
	return false;
}








//===============================================
//	Check if Audio is already Imported
//===============================================
unsigned int AudioManager::GetSoundIDOfImportedAudioFIle( std::string sFilePath )
{
	// If the filename matches up to one that's already been imported, return true
	for( std::list<AudioInfo*>::iterator iter = m_ChannelInfos[0].ImportedAudioList.begin(); iter != m_ChannelInfos[0].ImportedAudioList.end(); iter++ )
	{
		if((*iter)->FilePath == sFilePath )
		{
			return (*iter)->SoundID;
		}
	}

	for( std::list<AudioInfo*>::iterator iter = m_ChannelInfos[1].ImportedAudioList.begin(); iter != m_ChannelInfos[1].ImportedAudioList.end(); iter++ )
	{
		if((*iter)->FilePath == sFilePath )
		{
			return (*iter)->SoundID;
		}
	}

	for( std::list<AudioInfo*>::iterator iter = m_ChannelInfos[2].ImportedAudioList.begin(); iter != m_ChannelInfos[2].ImportedAudioList.end(); iter++ )
	{
		if((*iter)->FilePath == sFilePath )
		{
			return (*iter)->SoundID;
		}
	}

	return 0;
}

//===============================================
//	Check if Volume is Valid
//===============================================
float AudioManager::CheckVolumeIsValid( unsigned int iVolume )
{
	if (iVolume > 200)
	{
		return 2.0f;
	}
	if (iVolume < 5)
	{
		return 0.05f;
	}
	float fVolume = ((float)iVolume * 0.01f);    // iVolume / 100
	return fVolume;
}

//===============================================
//	Check if Speed is Valid
//===============================================
float AudioManager::CheckTempoIsValid( unsigned int iTempo )
{
	if (iTempo > 200)
	{
		return 2.0f;
	}
	if (iTempo < 5)
	{
		return 0.05f;
	}
	float fTempo = ((float)iTempo * 0.01f);    // iTempo / 100
	return fTempo;
}

//===============================================
//	Find Sound from PTR
//===============================================
AudioManager::AudioInfo* AudioManager::FindSoundFromPTR( FMOD::Sound* pSound )
{
	for( std::list<AudioInfo*>::iterator iter = m_ChannelInfos[0].ImportedAudioList.begin(); iter != m_ChannelInfos[0].ImportedAudioList.end(); iter++ )
	{
		if((*iter)->Sound_ptr == pSound )
		{
			return *iter;
		}
	}

	for( std::list<AudioInfo*>::iterator iter = m_ChannelInfos[1].ImportedAudioList.begin(); iter != m_ChannelInfos[1].ImportedAudioList.end(); iter++ )
	{
		if((*iter)->Sound_ptr == pSound )
		{
			return *iter;
		}
	}

	for( std::list<AudioInfo*>::iterator iter = m_ChannelInfos[2].ImportedAudioList.begin(); iter != m_ChannelInfos[2].ImportedAudioList.end(); iter++ )
	{
		if((*iter)->Sound_ptr == pSound )
		{
			return *iter;
		}
	}

	return NULL; // Won't Reach This
}

//===============================================
//	Correct Channel Volume
//===============================================
void AudioManager::CorrectChannelVolume( ChannelInfo& a_Channel, float fVolume )
{
	FMOD::Sound* TempSoundPTR = NULL;
	FMOD_Result = (*a_Channel.ChannelPTR)->getCurrentSound(&TempSoundPTR);
	
	// If Sound is playing, adjust volume to the sound's individual volume combined with the channel's volume
	if( TempSoundPTR != NULL ) 
	{ 
		FMOD_Result = (*a_Channel.ChannelPTR)->setPaused(true);
		FMOD_Result = (*a_Channel.ChannelPTR)->setVolume( FindSoundFromPTR(TempSoundPTR)->Volume * fVolume ); 
		FMOD_Result = (*a_Channel.ChannelPTR)->setPaused(a_Channel.Paused);
	}
	// Otherwise just set the channel straight to that specific volume
	else
	{
		FMOD_Result = (*a_Channel.ChannelPTR)->setVolume( fVolume ); 
	}
}


//===============================================
//	Convert String to ChannelInfo Index
//===============================================
AudioManager::ChannelInfo* AudioManager::GetChannelInfo(SoundType eChannelType )
{
	if(eChannelType == SoundType::BGM) { return &m_ChannelInfos[0]; }
	if(eChannelType == SoundType::BGS) { return &m_ChannelInfos[1]; }
	else							   { return &m_ChannelInfos[2]; }
}



//===============================================
//	Find AudioInfo Object in BGM Imported List
//===============================================
AudioManager::AudioInfo* AudioManager::FindAudioInfoInBGMImportedList( unsigned int iSoundId )
{
	for( std::list<AudioInfo*>::iterator iter = m_ChannelInfos[0].ImportedAudioList.begin(); iter != m_ChannelInfos[0].ImportedAudioList.end(); iter++ )
	{
		if((*iter)->SoundID == iSoundId )
		{
			return (*iter);
		}
	}
	return NULL;
}
//===============================================
//	Find AudioInfo Object in BGS Imported List
//===============================================
AudioManager::AudioInfo* AudioManager::FindAudioInfoInBGSImportedList( unsigned int iSoundId )
{
	for( std::list<AudioInfo*>::iterator iter = m_ChannelInfos[1].ImportedAudioList.begin(); iter != m_ChannelInfos[1].ImportedAudioList.end(); iter++ )
	{
		if((*iter)->SoundID == iSoundId )
		{
			return (*iter);
		}
	}
	return NULL;
}
//===============================================
//	Find AudioInfo Object in SE Imported List
//===============================================
AudioManager::AudioInfo* AudioManager::FindAudioInfoInSEImportedList( unsigned int iSoundId )
{
	for( std::list<AudioInfo*>::iterator iter = m_ChannelInfos[2].ImportedAudioList.begin(); iter != m_ChannelInfos[2].ImportedAudioList.end(); iter++ )
	{
		if( (*iter)->SoundID == iSoundId )
		{
			return (*iter);
		}
	}
	return NULL;
}

//===============================================
//	Convert String into AudioInfo Object
//===============================================
AudioManager::AudioInfo* AudioManager::GetAudioInfoObject( unsigned int iSoundId, SoundType eSoundType )
{
	AudioInfo* AudInfo = NULL;
	if(eSoundType != SoundType::COUNT)
	{
		if(eSoundType == SoundType::BGM)		{ AudInfo = FindAudioInfoInBGMImportedList(iSoundId); }
		else if(eSoundType == SoundType::BGS)	{ AudInfo = FindAudioInfoInBGSImportedList(iSoundId); }
		else									{ AudInfo = FindAudioInfoInSEImportedList(iSoundId);  }
		
		if(AudInfo != NULL) { return AudInfo; }
	}


	// Search through all three lists if it has not been found
	AudInfo = FindAudioInfoInBGMImportedList(iSoundId);
	if(AudInfo != NULL) { return AudInfo; }

	AudInfo = FindAudioInfoInBGSImportedList(iSoundId);
	if(AudInfo != NULL) { return AudInfo; }

	return FindAudioInfoInSEImportedList(iSoundId);
}