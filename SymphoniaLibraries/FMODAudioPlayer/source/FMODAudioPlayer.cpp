// FMODAudioPlayer.cpp : Defines the exported functions for the DLL application.
//

#include "AudioManager.h"

////////// Declarations /////////////////////////
AudioManager* g_audioManager = nullptr;

extern "C"
{
	////////// Exported Functions /////////////////////////
	__declspec(dllexport) bool InitialiseSoundSystem()
	{
		if (g_audioManager != nullptr)
		{
			return false;
		}

		g_audioManager = new AudioManager();
		return g_audioManager->GetFMODResult() == FMOD_RESULT::FMOD_OK;
	}

	__declspec(dllexport) bool UpdateSoundSystem(float deltaTime)
	{
		if (g_audioManager == nullptr)
		{
			return false;
		}

		return g_audioManager->Update(deltaTime);
	}

	__declspec(dllexport) int GetFMODResult()
	{
		if (g_audioManager == nullptr)
		{
			return -1;
		}

		return g_audioManager->GetFMODResult();
	}

	__declspec(dllexport) bool IsPlayingSound(int soundType)
	{
		if (g_audioManager == nullptr)
		{
			return false;
		}

		return g_audioManager->IsPlaying((SoundType)soundType);
	}

	__declspec(dllexport) bool IsPlayingSpecifiedSound(unsigned int soundId, int soundType)
	{
		if (g_audioManager == nullptr)
		{
			return false;
		}

		unsigned int playingSoundID = g_audioManager->GetSoundIDOfPlayingAudio((SoundType)soundType);
		return playingSoundID == soundId;
	}

	__declspec(dllexport) unsigned int GetSoundPositionMS(int soundType)
	{
		if (g_audioManager == nullptr)
		{
			return 0;
		}

		return g_audioManager->GetSoundPosition((SoundType)soundType);
	}

	__declspec(dllexport) float GetSoundPositionSeconds(int soundType)
	{
		if (g_audioManager == nullptr)
		{
			return -1;
		}

		unsigned int position = g_audioManager->GetSoundPosition((SoundType)soundType);
		float asSeconds = (float)position * 0.001f;
		return asSeconds;
	}

	__declspec(dllexport) unsigned int GetSoundLengthMS(unsigned int soundId, int soundType)
	{
		if (g_audioManager == nullptr)
		{
			return 0;
		}

		unsigned int lengthMS = g_audioManager->GetSoundLength(soundId, (SoundType)soundType);
		return lengthMS;
	}

	__declspec(dllexport) float GetSoundLengthSeconds(unsigned int soundId, int soundType)
	{
		if (g_audioManager == nullptr)
		{
			return 0.0f;
		}

		unsigned int lengthMS = g_audioManager->GetSoundLength(soundId, (SoundType)soundType);
		float asSeconds = (float)lengthMS * 0.001f;
		return asSeconds;
	}

	__declspec(dllexport) float GetSoundProgress(int soundType)
	{
		if (g_audioManager == nullptr)
		{
			return -1;
		}

		return g_audioManager->GetSoundProgress((SoundType)soundType);
	}

	__declspec(dllexport) unsigned int ImportAudio(const wchar_t* filePath, int soundType, bool loopAudio, bool removeSilenceFromAudio, unsigned int volume = 100, unsigned int speed = 100)
	{
		if (g_audioManager == nullptr)
		{
			return 0;
		}

		// Converting Unicode characters
		size_t filePathLength = wcslen(filePath);
		size_t outputSize = filePathLength + 1;
		char* convertedChars = new char[outputSize];
		size_t charsConverted = 0;
		wcstombs_s(&charsConverted, convertedChars, outputSize, filePath, filePathLength);

		unsigned int soundId = g_audioManager->ImportAudio(convertedChars, (SoundType)soundType, loopAudio, removeSilenceFromAudio, volume, speed);

		delete[] convertedChars;
		return soundId;
	}

	__declspec(dllexport) bool PlaySound(unsigned int soundId, int soundType)
	{
		if (g_audioManager == nullptr)
		{
			return false;
		}

		return g_audioManager->PlaySound(soundId, (SoundType)soundType);
	}

	__declspec(dllexport) bool IsPlaying(int soundType)
	{
		if (g_audioManager == nullptr)
		{
			return false;
		}

		return g_audioManager->IsPlaying((SoundType)soundType);
	}

	__declspec(dllexport) bool PreloadSound(unsigned int soundId, int soundType)
	{
		if (g_audioManager == nullptr)
		{
			return false;
		}

		return g_audioManager->PreloadSound(soundId, (SoundType)soundType);
	}

	__declspec(dllexport) bool PauseSound(int soundType)
	{
		if (g_audioManager == nullptr)
		{
			return false;
		}

		return g_audioManager->SetChannelPause((SoundType)soundType, true);
	}

	__declspec(dllexport) bool UnpauseSound(int soundType)
	{
		if (g_audioManager == nullptr)
		{
			return false;
		}

		return g_audioManager->SetChannelPause((SoundType)soundType, false);
	}

	__declspec(dllexport) bool StopSound(int soundType)
	{
		if (g_audioManager == nullptr)
		{
			return false;
		}

		return g_audioManager->StopSound((SoundType)soundType);
	}

	__declspec(dllexport) bool FadeinSound(int soundType, float fadeInDuration)
	{
		if (g_audioManager == nullptr)
		{
			return false;
		}

		g_audioManager->FadeinSound((SoundType)soundType, fadeInDuration);
		return true;
	}

	__declspec(dllexport) bool FadeoutSound(int soundType, float fadeOutDuration)
	{
		if (g_audioManager == nullptr)
		{
			return false;
		}

		g_audioManager->FadeoutSound((SoundType)soundType, fadeOutDuration);
		return true;
	}

	__declspec(dllexport) bool TerminateSoundSystem()
	{
		if (g_audioManager == nullptr)
		{
			return false;
		}

		delete g_audioManager;
		g_audioManager = nullptr;
		return true;
	}
}


#include <iostream>
#include <windows.h>
#undef PlaySound

int main()
{
	InitialiseSoundSystem();
	unsigned int importedId = ImportAudio(L"./CreatedFromImagination.mid", SoundType::BGM, true, false);
	PlaySound(importedId, SoundType::BGM);

	while (true)
	{
		std::cout << GetSoundPositionSeconds(SoundType::BGM) << std::endl;
	}

	TerminateSoundSystem();
	return 0;
}