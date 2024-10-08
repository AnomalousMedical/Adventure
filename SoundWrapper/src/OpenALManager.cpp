#include "StdAfx.h"
#include "OpenALManager.h"
#include "SourceManager.h"
#include "MemorySound.h"
#include "StreamingSound.h"
#include "Listener.h"
#include "CaptureDevice.h"
#include "Stream.h"

//Codecs
#include "OggCodec.h"

namespace SoundWrapper
{

OpenALManager::OpenALManager(void)
:ready(true),
listener(new Listener()),
sourceManager(NULL)
#ifdef ALC_SOFT_system_events
,reopenDeviceNextUpdate(false)
#endif
{
	createDevice();
}

OpenALManager::~OpenALManager(void)
{
	destroyDevice();

	delete listener;
}

CaptureDevice* OpenALManager::createCaptureDevice(BufferFormat format, int bufferSeconds, int rate)
{
	return new CaptureDevice(this, format, bufferSeconds, rate);
}

void OpenALManager::destroyCaptureDevice(CaptureDevice* captureDevice)
{
	delete captureDevice;
}

AudioCodec* OpenALManager::createAudioCodec(Stream* stream)
{
	return getCodecForStream(stream);
}

void OpenALManager::destroyAudioCodec(AudioCodec* codec)
{
	delete codec;
}

Sound* OpenALManager::createMemorySound(Stream* stream)
{
	return new MemorySound(getCodecForStream(stream));
}

Sound* OpenALManager::createMemorySound(AudioCodec* codec)
{
	return new MemorySound(codec);
}

Sound* OpenALManager::createStreamingSound(Stream* stream)
{
	return new StreamingSound(getCodecForStream(stream));
}

Sound* OpenALManager::createStreamingSound(AudioCodec* codec)
{
	return new StreamingSound(codec);
}

Sound* OpenALManager::createStreamingSound(Stream* stream, int bufferSize, int numBuffers)
{
	return new StreamingSound(getCodecForStream(stream), bufferSize, numBuffers);
}

Sound* OpenALManager::createStreamingSound(AudioCodec* codec, int bufferSize, int numBuffers)
{
	return new StreamingSound(codec, bufferSize, numBuffers);
}

void OpenALManager::destroySound(Sound* sound)
{
	delete sound;
}

AudioCodec* OpenALManager::getCodecForStream(Stream* stream)
{
	char magicBuffer[5];
	stream->read(magicBuffer, 4, 4);
	magicBuffer[4] = '\0';
	stream->seek(0, 0);
	if (strcmp(magicBuffer, "OggS") == 0)
	{
		return new OggCodec(stream);
	}

	//Need to add other types at some point, this will crash if not ogg
	return 0;
}

Source* OpenALManager::getSource()
{
	return sourceManager->getPooledSource();
}

void OpenALManager::update()
{
#ifdef ALC_SOFT_system_events
	if (reopenDeviceNextUpdate)
	{
		reopenDeviceNextUpdate = false;
		_alcReopenDeviceSOFT(device, NULL, NULL);
	}
#endif

	sourceManager->_update();
	for(std::list<CaptureDevice*>::iterator capDevice = activeDevices.begin(); capDevice != activeDevices.end(); ++capDevice)
	{
		(*capDevice)->update();
	}
}

#ifdef ALC_SOFT_system_events
#define ALEVENTS_COUNT 1
ALCenum alEvents[ALEVENTS_COUNT] = { ALC_EVENT_TYPE_DEFAULT_DEVICE_CHANGED_SOFT };

void deviceCallback(ALCenum eventType, ALCenum deviceType, ALCdevice* device, ALCsizei length, const ALCchar* message, void* userParam)
{
	switch (eventType)
	{
		case ALC_EVENT_TYPE_DEFAULT_DEVICE_CHANGED_SOFT:
			((OpenALManager*)userParam)->defaultDeviceChanged();
			break;
	}
}
#endif

void OpenALManager::createDevice()
{
	if (sourceManager == NULL)
	{
		logger << "Creating OpenAL Device" << info;

		device = alcOpenDevice(NULL);
		if (device == NULL)
		{
			logger << "Error creatig OpenAL Device." << error;
			ready = false;
			return;
		}

		const ALchar* vendor = alGetString(AL_VENDOR);
		if (vendor != NULL)
		{
			logger << " Vendor: " << vendor << info;
		}

		const ALchar* version = alGetString(AL_VERSION);
		if (version != NULL)
		{
			logger << " Version: " << version << info;
		}

		const ALchar* renderer = alGetString(AL_RENDERER);
		if (renderer != NULL)
		{
			logger << " Renderer: " << renderer << info;
		}

		//Create context(s)
		context = alcCreateContext(device, NULL);

		//Set active context
		alcMakeContextCurrent(context);

		// Clear Error Code
		alGetError();

#ifdef ALC_SOFT_system_events
		_alcEventIsSupportedSOFT = (LPALCEVENTISSUPPORTEDSOFT)alcGetProcAddress(NULL, "alcEventIsSupportedSOFT");
		if (_alcEventIsSupportedSOFT != NULL)
		{
			LPALCEVENTCALLBACKSOFT _alcEventCallbackSOFT = (LPALCEVENTCALLBACKSOFT)alcGetProcAddress(NULL, "alcEventCallbackSOFT");
			_alcEventControlSOFT = (LPALCEVENTCONTROLSOFT)alcGetProcAddress(NULL, "alcEventControlSOFT");
			_alcReopenDeviceSOFT = (LPALCREOPENDEVICESOFT)alcGetProcAddress(NULL, "alcReopenDeviceSOFT");

			bool isSupported = _alcEventIsSupportedSOFT(ALC_EVENT_TYPE_DEFAULT_DEVICE_CHANGED_SOFT, ALC_PLAYBACK_DEVICE_SOFT) == ALC_EVENT_SUPPORTED_SOFT;
			if (isSupported)
			{
				_alcEventCallbackSOFT(deviceCallback, this);

				_alcEventControlSOFT(ALEVENTS_COUNT, alEvents, true);
			}
		}
#endif

		sourceManager = new SourceManager();
	}
}

void OpenALManager::destroyDevice()
{
	if (sourceManager != NULL)
	{
		logger << "Destroying OpenAL Device" << info;

#ifdef ALC_SOFT_system_events
		if (_alcEventIsSupportedSOFT != NULL)
		{
			bool isSupported = _alcEventIsSupportedSOFT(ALC_EVENT_TYPE_DEFAULT_DEVICE_CHANGED_SOFT, ALC_PLAYBACK_DEVICE_SOFT) == ALC_EVENT_SUPPORTED_SOFT;
			if (isSupported)
			{
				_alcEventControlSOFT(ALEVENTS_COUNT, alEvents, false);
			}
		}
#endif

		delete sourceManager;
		sourceManager = NULL;

		//Disable context
		alcMakeContextCurrent(NULL);
		//Release context(s)
		alcDestroyContext(context);
		//Close device
		alcCloseDevice(device);
	}
}

void OpenALManager::defaultDeviceChanged()
{
#ifdef ALC_SOFT_system_events
	reopenDeviceNextUpdate = true;
#endif
}

}

//CWrapper

using namespace SoundWrapper;

extern "C" _AnomalousExport OpenALManager* OpenALManager_create()
{
	return new OpenALManager();
}

extern "C" _AnomalousExport void OpenALManager_destroy(OpenALManager* openALManager)
{
	delete openALManager;
}

extern "C" _AnomalousExport CaptureDevice* OpenALManager_createCaptureDevice(OpenALManager* openALManager, BufferFormat format, int bufferSeconds, int rate)
{
	return openALManager->createCaptureDevice(format, bufferSeconds, rate);
}

extern "C" _AnomalousExport void OpenALManager_destroyCaptureDevice(OpenALManager* openALManager, CaptureDevice* captureDevice)
{
	openALManager->destroyCaptureDevice(captureDevice);
}

extern "C" _AnomalousExport AudioCodec* OpenALManager_createAudioCodec(OpenALManager* openALManager, Stream* stream)
{
	return openALManager->createAudioCodec(stream);
}

extern "C" _AnomalousExport void OpenALManager_destroyAudioCodec(OpenALManager* openALManager, AudioCodec* codec)
{
	openALManager->destroyAudioCodec(codec);
}

extern "C" _AnomalousExport Sound* OpenALManager_createMemorySound(OpenALManager* openALManager, Stream* stream)
{
	return openALManager->createMemorySound(stream);
}

extern "C" _AnomalousExport Sound* OpenALManager_createMemorySoundCodec(OpenALManager* openALManager, AudioCodec* codec)
{
	return openALManager->createMemorySound(codec);
}

extern "C" _AnomalousExport Sound* OpenALManager_createStreamingSound(OpenALManager* openALManager, Stream* stream)
{
	return openALManager->createStreamingSound(stream);
}

extern "C" _AnomalousExport Sound* OpenALManager_createStreamingSoundCodec(OpenALManager* openALManager, AudioCodec* codec)
{
	return openALManager->createStreamingSound(codec);
}

extern "C" _AnomalousExport Sound* OpenALManager_createStreamingSound2(OpenALManager* openALManager, Stream* stream, int bufferSize, int numBuffers)
{
	return openALManager->createStreamingSound(stream, bufferSize, numBuffers);
}

extern "C" _AnomalousExport Sound* OpenALManager_createStreamingSound2Codec(OpenALManager* openALManager, AudioCodec* codec, int bufferSize, int numBuffers)
{
	return openALManager->createStreamingSound(codec, bufferSize, numBuffers);
}

extern "C" _AnomalousExport void OpenALManager_destroySound(OpenALManager* openALManager, Sound* sound)
{
	openALManager->destroySound(sound);
}

extern "C" _AnomalousExport Source* OpenALManager_getSource(OpenALManager* openALManager)
{
	return openALManager->getSource();	
}

extern "C" _AnomalousExport void OpenALManager_update(OpenALManager* openALManager)
{
	openALManager->update();	
}

extern "C" _AnomalousExport Listener* OpenALManager_getListener(OpenALManager* openALManager)
{
	return openALManager->getListener();
}

extern "C" _AnomalousExport void OpenALManager_resumeAudio(OpenALManager* openALManager)
{
	openALManager->createDevice();
}

extern "C" _AnomalousExport void OpenALManager_suspendAudio(OpenALManager* openALManager)
{
	openALManager->destroyDevice();
}