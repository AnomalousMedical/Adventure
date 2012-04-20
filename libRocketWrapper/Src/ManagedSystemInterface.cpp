#include "StdAfx.h"
#include "ManagedSystemInterface.h"

ManagedSystemInterface::ManagedSystemInterface(GetElapsedTimeDelegate etDelegate, LogMessageDelegate logDelegate)
	:etDelegate(etDelegate),
	logDelegate(logDelegate)
{
}

ManagedSystemInterface::~ManagedSystemInterface()
{
}

float ManagedSystemInterface::GetElapsedTime()
{
	return etDelegate();
}

bool ManagedSystemInterface::LogMessage(Rocket::Core::Log::Type type, const Rocket::Core::String& message)
{
	logDelegate(type, message.CString());
	return false;
}

extern "C" _AnomalousExport ManagedSystemInterface* ManagedSystemInterface_Create(GetElapsedTimeDelegate etDelegate, LogMessageDelegate logDelegate)
{
	return new ManagedSystemInterface(etDelegate, logDelegate);
}

extern "C" _AnomalousExport void ManagedSystemInterface_Delete(ManagedSystemInterface* systemInterface)
{
	delete systemInterface;
}